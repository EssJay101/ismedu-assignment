using Ismedu.Api.DTOs;
using Ismedu.Api.Repositories;

namespace Ismedu.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, ITokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim();
        var user = await _users.GetByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordIsCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordIsCorrect)
        {
            return null;
        }

        var (token, expiresAt) = _tokens.CreateToken(user);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            FullName = user.FullName,
            Email = user.Email
        };
    }
}