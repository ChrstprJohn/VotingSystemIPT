using MongoDB.Driver;
using VotingSystem.Models.Domain;
using VotingSystem.Models.ViewModels;

namespace VotingSystem.Controllers.Services
{
    public sealed class VoterService
    {
        private readonly IMongoCollection<Voter> _voters;
        private readonly IMongoCollection<VoterFile> _files;

        public VoterService(IMongoDatabase database)
        {
            _voters = database.GetCollection<Voter>(CollectionNames.Voters);
            _files = database.GetCollection<VoterFile>(CollectionNames.VoterFiles);
        }

        public async Task<List<VoterFile>> GetFilesAsync(string electionId)
        {
            return await _files
                .Find(f => f.ElectionId == electionId)
                .SortBy(f => f.DepartmentName)
                .ToListAsync();
        }

        public async Task<long> CountEligibleAsync(string electionId)
        {
            return await _voters.CountDocumentsAsync(v => v.ElectionId == electionId);
        }

        public async Task<long> CountVotedAsync(string electionId)
        {
            return await _voters.CountDocumentsAsync(v => v.ElectionId == electionId && v.HasVoted);
        }

        public async Task<List<Voter>> GetVotersAsync(string electionId, int skip = 0, int take = 50)
        {
            return await _voters
                .Find(v => v.ElectionId == electionId)
                .SortBy(v => v.Course).ThenBy(v => v.FullName)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();
        }

        public async Task<List<Voter>> GetAllVotersAsync(string electionId)
        {
            return await _voters.Find(v => v.ElectionId == electionId).ToListAsync();
        }

        /// <summary>
        /// Imports (or replaces) one department's CSV. The new file completely overrides
        /// any existing file for the same department in this election.
        /// </summary>
        public async Task<VoterImportResult> ImportDepartmentAsync(
            string electionId, string departmentName, string fileName, string csvContent)
        {
            var result = new VoterImportResult { DepartmentName = departmentName };
            departmentName = departmentName.Trim();

            var rows = CsvReader.Parse(csvContent);
            if (rows.Count < 2)
            {
                result.Errors.Add("The file has no data rows.");
                return result;
            }

            var header = rows[0].Select(NormalizeHeader).ToArray();
            int Col(params string[] names) => Array.FindIndex(header, h => names.Contains(h));

            var idxStudent = Col("studentnumber", "studentno", "studentid", "srcode", "idnumber", "id", "studentid#");
            var idxName = Col("fullname", "name", "studentname");
            var idxFirst = Col("firstname", "fname", "givenname");
            var idxLast = Col("lastname", "lname", "surname", "familyname");
            var idxMiddle = Col("middlename", "middle", "mname");
            var idxEmail = Col("email", "emailaddress", "schoolemail", "schoolemailaddress", "semail");
            var idxCourse = Col("course", "department", "coursedepartment", "program", "dept");
            var idxSection = Col("section", "block", "sec");

            if (idxStudent < 0)
            {
                result.Errors.Add("Missing a student number column (e.g. 'student_number').");
            }

            if (idxEmail < 0)
            {
                result.Errors.Add("Missing a school email column (e.g. 'email'). The school email is required.");
            }

            if (idxName < 0 && (idxFirst < 0 || idxLast < 0))
            {
                result.Errors.Add("Missing a name column (either 'full_name' or 'first_name' + 'last_name').");
            }

            if (result.Errors.Count > 0)
            {
                return result;
            }

            var docs = new List<Voter>();
            var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var r = 1; r < rows.Count; r++)
            {
                var row = rows[r];
                string Get(int idx) => idx >= 0 && idx < row.Length ? row[idx].Trim() : string.Empty;

                var studentNumber = Get(idxStudent);
                var email = Get(idxEmail);

                var fullName = idxName >= 0
                    ? Get(idxName)
                    : string.Join(' ', new[] { Get(idxFirst), Get(idxMiddle), Get(idxLast) }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                if (string.IsNullOrWhiteSpace(studentNumber) || string.IsNullOrWhiteSpace(email))
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Row {r + 1}: missing student number or email — skipped.");
                    continue;
                }

                if (!seenInFile.Add(studentNumber))
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Row {r + 1}: duplicate student number {studentNumber} in file — skipped.");
                    continue;
                }

                docs.Add(new Voter
                {
                    ElectionId = electionId,
                    StudentNumber = studentNumber,
                    FullName = fullName,
                    Email = email.ToLowerInvariant(),
                    Course = idxCourse >= 0 ? Get(idxCourse) : departmentName,
                    Section = Get(idxSection),
                    HasVoted = false
                });
            }

            if (docs.Count == 0)
            {
                result.Errors.Add("No valid rows to import.");
                return result;
            }

            // Replace this department's data entirely.
            var existingFile = await _files
                .Find(f => f.ElectionId == electionId && f.DepartmentName == departmentName)
                .FirstOrDefaultAsync();

            if (existingFile is not null)
            {
                await _voters.DeleteManyAsync(v => v.VoterFileId == existingFile.Id);
                await _files.DeleteOneAsync(f => f.Id == existingFile.Id);
            }

            // Enforce uniqueness across the whole election: a student number or email
            // already registered by another department wins and the new row is skipped.
            var others = await _voters
                .Find(v => v.ElectionId == electionId)
                .Project(v => new { v.StudentNumber, v.Email })
                .ToListAsync();
            var takenStudents = others.Select(o => o.StudentNumber).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var takenEmails = others.Select(o => o.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var accepted = new List<Voter>();
            foreach (var d in docs)
            {
                if (takenStudents.Contains(d.StudentNumber))
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Student number {d.StudentNumber} already exists in another department — skipped.");
                    continue;
                }

                if (takenEmails.Contains(d.Email))
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Email {d.Email} already exists in another department — skipped.");
                    continue;
                }

                accepted.Add(d);
            }

            if (accepted.Count == 0)
            {
                result.Errors.Add("Every row collided with existing voters — nothing imported.");
                return result;
            }

            var file = new VoterFile
            {
                ElectionId = electionId,
                DepartmentName = departmentName,
                FileName = fileName,
                RowCount = accepted.Count,
                UploadedAt = DateTime.UtcNow
            };
            await _files.InsertOneAsync(file);

            foreach (var d in accepted)
            {
                d.VoterFileId = file.Id;
            }

            await _voters.InsertManyAsync(accepted);

            result.Succeeded = true;
            result.ImportedCount = accepted.Count;
            return result;
        }

        public async Task RemoveDepartmentAsync(string electionId, string voterFileId)
        {
            var file = await _files.Find(f => f.Id == voterFileId && f.ElectionId == electionId).FirstOrDefaultAsync();
            if (file is null)
            {
                return;
            }

            await _voters.DeleteManyAsync(v => v.VoterFileId == file.Id);
            await _files.DeleteOneAsync(f => f.Id == file.Id);
        }

        public async Task DeleteByElectionAsync(string electionId)
        {
            await _voters.DeleteManyAsync(v => v.ElectionId == electionId);
            await _files.DeleteManyAsync(f => f.ElectionId == electionId);
        }

        // ---- Voter verification / ballot access ---------------------------------

        public async Task<Voter?> VerifyAsync(string electionId, string studentNumber, string email)
        {
            studentNumber = studentNumber.Trim();
            email = email.Trim().ToLowerInvariant();

            return await _voters
                .Find(v => v.ElectionId == electionId
                           && v.StudentNumber == studentNumber
                           && v.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<string> IssueAccessTokenAsync(string voterId)
        {
            var token = TokenGenerator.Create();
            var update = Builders<Voter>.Update
                .Set(v => v.AccessToken, token)
                .Set(v => v.TokenIssuedAt, DateTime.UtcNow);
            await _voters.UpdateOneAsync(v => v.Id == voterId, update);
            return token;
        }

        public async Task<Voter?> GetByAccessTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            return await _voters.Find(v => v.AccessToken == token).FirstOrDefaultAsync();
        }

        public async Task<Voter?> GetByIdAsync(string id)
        {
            return await _voters.Find(v => v.Id == id).FirstOrDefaultAsync();
        }

        public async Task MarkVotedAsync(string voterId)
        {
            var update = Builders<Voter>.Update
                .Set(v => v.HasVoted, true)
                .Set(v => v.VotedAt, DateTime.UtcNow)
                .Set(v => v.AccessToken, null);
            await _voters.UpdateOneAsync(v => v.Id == voterId, update);
        }

        private static string NormalizeHeader(string h)
        {
            return new string(h.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }
    }
}
