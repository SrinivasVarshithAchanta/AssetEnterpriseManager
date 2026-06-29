using System.Security.Claims;
using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Services;

public interface IAuthService
{
    Task<User> ValidateCredentialsAsync(string email, string password);
    ClaimsPrincipal BuildPrincipal(User user);
}

// Handles credential checking and turns a valid user into a claims principal.
// The actual cookie sign in/out happens in AccountController via HttpContext,
// keeping framework calls in the controller and logic in the service.
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditService _audit;

    public AuthService(ApplicationDbContext db, IPasswordHasher hasher, IAuditService audit)
    {
        _db = db;
        _hasher = hasher;
        _audit = audit;
    }

    public async Task<User> ValidateCredentialsAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        email = email.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !user.IsActive)
        {
            await _audit.LogAsync(user?.Id, "LoginFailed", "User", user?.Id, $"Failed login attempt for {email}.");
            return null;
        }

        if (!_hasher.Verify(password, user.PasswordHash))
        {
            await _audit.LogAsync(user.Id, "LoginFailed", "User", user.Id, $"Incorrect password for {email}.");
            return null;
        }

        await _audit.LogAsync(user.Id, "LoginSuccess", "User", user.Id, $"User {email} signed in.");
        return user;
    }

    public ClaimsPrincipal BuildPrincipal(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("Department", user.Department ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
