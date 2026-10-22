using Dapper;
using Ismedu.Api.Data;
using Ismedu.Api.DTOs;
using Ismedu.Api.Models;

namespace Ismedu.Api.Repositories;

public interface IStateRepository
{
    Task<(IEnumerable<State> Items, int TotalCount)> GetPagedAsync(StateQuery query);
    Task<State?> GetByIdAsync(int id);
    Task<IEnumerable<State>> GetActiveByCountryAsync(int countryId);
    Task<bool> NameExistsAsync(string name, int countryId, int? excludeId = null);
    Task<int> CreateAsync(State state);
    Task<bool> UpdateAsync(State state);
    Task<bool> DeleteAsync(int id);
    Task<(int CityCount, int StudentCount)> GetUsageAsync(int id);
}

public class StateRepository : IStateRepository
{
    private readonly IDbConnectionFactory _db;

    public StateRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<State> Items, int TotalCount)> GetPagedAsync(StateQuery query)
    {
        const string fromAndWhere = @"
            FROM States s
            INNER JOIN Countries c ON c.CountryId = s.CountryId
            WHERE (@Search IS NULL
                   OR s.StateName LIKE @SearchPattern
                   OR c.CountryName LIKE @SearchPattern)
              AND (@CountryId IS NULL OR s.CountryId = @CountryId)
              AND (@IsActive IS NULL OR s.IsActive = @IsActive)";

        var countSql = "SELECT COUNT(*) " + fromAndWhere + ";";

        var itemsSql = @"
            SELECT s.StateId, s.StateName, s.CountryId, s.IsActive, c.CountryName
            " + fromAndWhere + @"
            ORDER BY c.CountryName, s.StateName
            LIMIT @PageSize OFFSET @Offset;";

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var parameters = new
        {
            Search = search,
            SearchPattern = search is null ? null : $"%{search}%",
            query.CountryId,
            query.IsActive,
            query.PageSize,
            Offset = (query.Page - 1) * query.PageSize
        };

        using var connection = _db.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<State>(itemsSql, parameters);

        return (items, totalCount);
    }

    public async Task<State?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT s.StateId, s.StateName, s.CountryId, s.IsActive, c.CountryName
            FROM States s
            INNER JOIN Countries c ON c.CountryId = s.CountryId
            WHERE s.StateId = @Id;";

        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<State>(sql, new { Id = id });
    }

    public async Task<IEnumerable<State>> GetActiveByCountryAsync(int countryId)
    {
        const string sql = @"
            SELECT StateId, StateName
            FROM States
            WHERE CountryId = @CountryId AND IsActive = TRUE
            ORDER BY StateName;";

        using var connection = _db.CreateConnection();
        return await connection.QueryAsync<State>(sql, new { CountryId = countryId });
    }

    public async Task<bool> NameExistsAsync(string name, int countryId, int? excludeId = null)
    {
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM States
                WHERE StateName = @Name
                  AND CountryId = @CountryId
                  AND (@ExcludeId IS NULL OR StateId <> @ExcludeId)
            );";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            sql, new { Name = name, CountryId = countryId, ExcludeId = excludeId });
    }

    public async Task<int> CreateAsync(State state)
    {
        const string sql = @"
            INSERT INTO States (StateName, CountryId, IsActive)
            VALUES (@StateName, @CountryId, @IsActive);
            SELECT LAST_INSERT_ID();";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, state);
    }

    public async Task<bool> UpdateAsync(State state)
    {
        const string sql = @"
            UPDATE States
            SET StateName = @StateName,
                CountryId = @CountryId,
                IsActive  = @IsActive
            WHERE StateId = @StateId;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, state);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM States WHERE StateId = @Id;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<(int CityCount, int StudentCount)> GetUsageAsync(int id)
    {
        const string citySql = "SELECT COUNT(*) FROM Cities WHERE StateId = @Id;";
        const string studentSql = "SELECT COUNT(*) FROM Students WHERE StateId = @Id;";

        using var connection = _db.CreateConnection();
        var cityCount = await connection.ExecuteScalarAsync<int>(citySql, new { Id = id });
        var studentCount = await connection.ExecuteScalarAsync<int>(studentSql, new { Id = id });

        return (cityCount, studentCount);
    }
}