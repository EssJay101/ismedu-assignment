using System.Data;
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
        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "sp_User_GetByEmail",
            new { p_Email = email },
            commandType: CommandType.StoredProcedure);
    }
}