using Dapper;
using Ismedu.Api.Data;
using Ismedu.Api.DTOs;
using Ismedu.Api.Models;

namespace Ismedu.Api.Repositories;

public interface IStudentRepository
{
    Task<(IEnumerable<Student> Items, int TotalCount)> GetPagedAsync(StudentQuery query);
    Task<Student?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<bool> MobileExistsAsync(string mobile, int? excludeId = null);
    Task<int> CreateAsync(Student student);
    Task<bool> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
}

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnectionFactory _db;

    private const string SelectWithNames = @"
        SELECT st.StudentId, st.FirstName, st.LastName, st.DateOfBirth,
               st.Email, st.Mobile, st.CountryId, st.StateId, st.CityId,
               st.Address, st.CreatedAt, st.UpdatedAt,
               c.CountryName, s.StateName, ci.CityName
        FROM Students st
        INNER JOIN Countries c ON c.CountryId = st.CountryId
        INNER JOIN States s    ON s.StateId   = st.StateId
        INNER JOIN Cities ci   ON ci.CityId   = st.CityId";

    public StudentRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<Student> Items, int TotalCount)> GetPagedAsync(StudentQuery query)
    {
        const string where = @"
            WHERE (@Search IS NULL
                   OR st.FirstName LIKE @SearchPattern
                   OR st.LastName  LIKE @SearchPattern
                   OR CONCAT(st.FirstName, ' ', st.LastName) LIKE @SearchPattern
                   OR st.Email     LIKE @SearchPattern
                   OR st.Mobile    LIKE @SearchPattern)
              AND (@CountryId IS NULL OR st.CountryId = @CountryId)
              AND (@StateId   IS NULL OR st.StateId   = @StateId)
              AND (@CityId    IS NULL OR st.CityId    = @CityId)";

        var countSql = "SELECT COUNT(*) FROM Students st " + where + ";";

        var itemsSql = SelectWithNames + " " + where + @"
            ORDER BY st.StudentId
            LIMIT @PageSize OFFSET @Offset;";

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var parameters = new
        {
            Search = search,
            SearchPattern = search is null ? null : $"%{search}%",
            query.CountryId,
            query.StateId,
            query.CityId,
            query.PageSize,
            Offset = (query.Page - 1) * query.PageSize
        };

        using var connection = _db.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<Student>(itemsSql, parameters);

        return (items, totalCount);
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        var sql = SelectWithNames + " WHERE st.StudentId = @Id;";

        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Student>(sql, new { Id = id });
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM Students
                WHERE Email = @Email
                  AND (@ExcludeId IS NULL OR StudentId <> @ExcludeId)
            );";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email, ExcludeId = excludeId });
    }

    public async Task<bool> MobileExistsAsync(string mobile, int? excludeId = null)
    {
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM Students
                WHERE Mobile = @Mobile
                  AND (@ExcludeId IS NULL OR StudentId <> @ExcludeId)
            );";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { Mobile = mobile, ExcludeId = excludeId });
    }

    public async Task<int> CreateAsync(Student student)
    {
        const string sql = @"
            INSERT INTO Students
                (FirstName, LastName, DateOfBirth, Email, Mobile,
                 CountryId, StateId, CityId, Address)
            VALUES
                (@FirstName, @LastName, @DateOfBirth, @Email, @Mobile,
                 @CountryId, @StateId, @CityId, @Address);
            SELECT LAST_INSERT_ID();";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, student);
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        const string sql = @"
            UPDATE Students
            SET FirstName   = @FirstName,
                LastName    = @LastName,
                DateOfBirth = @DateOfBirth,
                Email       = @Email,
                Mobile      = @Mobile,
                CountryId   = @CountryId,
                StateId     = @StateId,
                CityId      = @CityId,
                Address     = @Address
            WHERE StudentId = @StudentId;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, student);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Students WHERE StudentId = @Id;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }
}