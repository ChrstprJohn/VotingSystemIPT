namespace VotingSystem.Controllers.Services
{
    public sealed class LoginResult
    {
        private LoginResult(
            bool succeeded,
            string? errorMessage,
            string? role = null)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
            Role = role;
        }

        public bool Succeeded { get; }

        public string? ErrorMessage { get; }

        public string? Role { get; }

        public static LoginResult Success(string? role = null)
        {
            return new LoginResult(
                succeeded: true,
                errorMessage: null,
                role: role);
        }

        public static LoginResult Failed(string errorMessage)
        {
            return new LoginResult(
                succeeded: false,
                errorMessage: errorMessage,
                role: null);
        }
    }
}