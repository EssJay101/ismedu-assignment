using Dapper;
using Ismedu.Api.Data;
using Ismedu.Api.Models;

namespace Ismedu.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;

    public UserRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT UserId, FullName, Email, PasswordHash, IsActive, CreatedAt
            FROM Users
            WHERE Email = @Email
            LIMIT 1;";

        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }
}