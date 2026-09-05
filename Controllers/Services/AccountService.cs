using VotingSystem.Controllers.Services;

public sealed class AccountService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LoginResult> LoginUserAsync(LoginViewModel model)
    {
        // // TODO: Replace this demo check with MongoDB lookup.
        // if (model.Username != "admin" ||
        //     model.Password != "admin123")
        // {
        //     return LoginResult.Failed(
        //         "Invalid username or password.");
        // }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Administrator")
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

        return LoginResult.Success();
    }
}
