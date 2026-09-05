using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using VotingSystem.Models;

namespace VotingSystem.Controllers.Services
{
    public sealed class AccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMongoCollection<UserAccount> _accounts;

        public AccountService(IHttpContextAccessor httpContextAccessor, IMongoDatabase database)
        {
            _httpContextAccessor = httpContextAccessor;
            _accounts = database.GetCollection<UserAccount>("users");
        }

        public async Task<LoginResult> LoginUserAsync(LoginViewModel model)
        {
            var account = await _accounts
                .Find(a => a.Username == model.Username)
                .FirstOrDefaultAsync();

            if (account is null || account.PasswordHash != HashPassword(model.Password))
            {
                return LoginResult.Failed("Invalid username or password.");
            }

            var role = account.Role;

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, account.Username),
                new(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return LoginResult.Failed(
                    "Unable to access the current HTTP context.");
            }

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });

            return LoginResult.Success(role);
        }

        internal static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}