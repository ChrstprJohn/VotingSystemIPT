namespace VotingSystem.Controllers.Services
{
    public sealed class LoginResult
    {
        private LoginResult(
            bool succeeded,
            string? errorMessage)
        {
            Succeeded = succeeded;
            ErrorMessage = errorMessage;
        }

        public bool Succeeded { get; }

        public string? ErrorMessage { get; }

        public static LoginResult Success()
        {
            return new LoginResult(
                succeeded: true,
                errorMessage: null);
        }

        public static LoginResult Failed(string errorMessage)
        {
            return new LoginResult(
                succeeded: false,
                errorMessage: errorMessage);
        }
    }
}