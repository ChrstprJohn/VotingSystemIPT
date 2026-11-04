using System.Security.Cryptography;

namespace VotingSystem.Controllers.Services
{
    /// <summary>
    /// Produces unguessable, URL-safe tokens for secure links (leader forms, ballot links).
    /// </summary>
    public static class TokenGenerator
    {
        public static string Create(int byteLength = 24)
        {
            var bytes = RandomNumberGenerator.GetBytes(byteLength);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
