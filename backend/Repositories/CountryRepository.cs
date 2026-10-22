using Dapper;
using Ismedu.Api.Data;
using Ismedu.Api.DTOs;
using Ismedu.Api.Models;

namespace Ismedu.Api.Repositories;

public interface ICountryRepository
{
    Task<(IEnumerable<Country> Items, int TotalCount)> GetPagedAsync(CountryQuery query);
    Task<Country?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    Task<int> CreateAsync(Country country);
    Task<bool> UpdateAsync(Country country);
    Task<bool> DeleteAsync(int id);
    Task<(int StateCount, int StudentCount)> GetUsageAsync(int id);
    Task<IEnumerable<Country>> GetActiveAsync();
}

public class CountryRepository : ICountryRepository
{
    private readonly IDbConnectionFactory _db;

    public CountryRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<Country> Items, int TotalCount)> GetPagedAsync(CountryQuery query)
    {
        const string where = @"
            WHERE (@Search IS NULL
                   OR CountryName LIKE @SearchPattern
                   OR CountryCode LIKE @SearchPattern)
              AND (@IsActive IS NULL OR IsActive = @IsActive)";

        var countSql = "SELECT COUNT(*) FROM Countries " + where + ";";

        var itemsSql = @"
            SELECT CountryId, CountryName, CountryCode, IsActive
            FROM Countries " + where + @"
            ORDER BY CountryId
            LIMIT @PageSize OFFSET @Offset;";

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var parameters = new
        {
            Search = search,
            SearchPattern = search is null ? null : $"%{search}%",
            query.IsActive,
            query.PageSize,
            Offset = (query.Page - 1) * query.PageSize
        };

        using var connection = _db.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<Country>(itemsSql, parameters);

        return (items, totalCount);
    }

    public async Task<Country?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT CountryId, CountryName, CountryCode, IsActive
            FROM Countries
            WHERE CountryId = @Id;";

        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Country>(sql, new { Id = id });
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM Countries
                WHERE CountryName = @Name
                  AND (@ExcludeId IS NULL OR CountryId <> @ExcludeId)
            );";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { Name = name, ExcludeId = excludeId });
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM Countries
                WHERE CountryCode = @Code
                  AND (@ExcludeId IS NULL OR CountryId <> @ExcludeId)
            );";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(sql, new { Code = code, ExcludeId = excludeId });
    }

    public async Task<int> CreateAsync(Country country)
    {
        const string sql = @"
            INSERT INTO Countries (CountryName, CountryCode, IsActive)
            VALUES (@CountryName, @CountryCode, @IsActive);
            SELECT LAST_INSERT_ID();";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, country);
    }

    public async Task<bool> UpdateAsync(Country country)
    {
        const string sql = @"
            UPDATE Countries
            SET CountryName = @CountryName,
                CountryCode = @CountryCode,
                IsActive    = @IsActive
            WHERE CountryId = @CountryId;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, country);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Countries WHERE CountryId = @Id;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<(int StateCount, int StudentCount)> GetUsageAsync(int id)
    {
        const string stateSql = "SELECT COUNT(*) FROM States WHERE CountryId = @Id;";
        const string studentSql = "SELECT COUNT(*) FROM Students WHERE CountryId = @Id;";

        using var connection = _db.CreateConnection();
        var stateCount = await connection.ExecuteScalarAsync<int>(stateSql, new { Id = id });
        var studentCount = await connection.ExecuteScalarAsync<int>(studentSql, new { Id = id });

        return (stateCount, studentCount);
    }

     public async Task<IEnumerable<Country>> GetActiveAsync()
    {
        const string sql = @"
            SELECT CountryId, CountryName
            FROM Countries
            WHERE IsActive = TRUE
            ORDER BY CountryName;";

        using var connection = _db.CreateConnection();
        return await connection.QueryAsync<Country>(sql);
    }
}