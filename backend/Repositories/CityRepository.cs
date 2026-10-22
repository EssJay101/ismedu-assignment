using Dapper;
using Ismedu.Api.Data;
using Ismedu.Api.DTOs;
using Ismedu.Api.Models;

namespace Ismedu.Api.Repositories;

public interface ICityRepository
{
    Task<(IEnumerable<City> Items, int TotalCount)> GetPagedAsync(CityQuery query);
    Task<City?> GetByIdAsync(int id);
    Task<IEnumerable<City>> GetActiveByStateAsync(int stateId);
    Task<bool> NameExistsAsync(string name, int stateId, int? excludeId = null);
    Task<int> CreateAsync(City city);
    Task<bool> UpdateAsync(City city);
    Task<bool> DeleteAsync(int id);
    Task<int> GetStudentCountAsync(int id);
}

public class CityRepository : ICityRepository
{
    private readonly IDbConnectionFactory _db;

    public CityRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<City> Items, int TotalCount)> GetPagedAsync(CityQuery query)
    {
        const string fromAndWhere = @"
            FROM Cities ci
            INNER JOIN States s    ON s.StateId   = ci.StateId
            INNER JOIN Countries c ON c.CountryId = s.CountryId
            WHERE (@Search IS NULL
                   OR ci.CityName   LIKE @SearchPattern
                   OR s.StateName   LIKE @SearchPattern
                   OR c.CountryName LIKE @SearchPattern)
              AND (@StateId   IS NULL OR ci.StateId  = @StateId)
              AND (@CountryId IS NULL OR s.CountryId = @CountryId)
              AND (@IsActive  IS NULL OR ci.IsActive = @IsActive)";

        var countSql = "SELECT COUNT(*) " + fromAndWhere + ";";

        var itemsSql = @"
            SELECT ci.CityId, ci.CityName, ci.StateId, ci.IsActive,
                   s.StateName, s.CountryId, c.CountryName
            " + fromAndWhere + @"
            ORDER BY ci.CityId
            LIMIT @PageSize OFFSET @Offset;";

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var parameters = new
        {
            Search = search,
            SearchPattern = search is null ? null : $"%{search}%",
            query.StateId,
            query.CountryId,
            query.IsActive,
            query.PageSize,
            Offset = (query.Page - 1) * query.PageSize
        };

        using var connection = _db.CreateConnection();
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<City>(itemsSql, parameters);

        return (items, totalCount);
    }

    public async Task<City?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT ci.CityId, ci.CityName, ci.StateId, ci.IsActive,
                   s.StateName, s.CountryId, c.CountryName
            FROM Cities ci
            INNER JOIN States s    ON s.StateId   = ci.StateId
            INNER JOIN Countries c ON c.CountryId = s.CountryId
            WHERE ci.CityId = @Id;";

        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<City>(sql, new { Id = id });
    }

    public async Task<IEnumerable<City>> GetActiveByStateAsync(int stateId)
    {
        const string sql = @"
            SELECT CityId, CityName
            FROM Cities
            WHERE StateId = @StateId AND IsActive = TRUE
            ORDER BY CityName;";

        using var connection = _db.CreateConnection();
        return await connection.QueryAsync<City>(sql, new { StateId = stateId });
    }

    public async Task<bool> NameExistsAsync(string name, int stateId, int? excludeId = null)
    {
        const string sql = @"
            SELECT EXISTS(
                SELECT 1 FROM Cities
                WHERE CityName = @Name
                  AND StateId = @StateId
                  AND (@ExcludeId IS NULL OR CityId <> @ExcludeId)
            );";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            sql, new { Name = name, StateId = stateId, ExcludeId = excludeId });
    }

    public async Task<int> CreateAsync(City city)
    {
        const string sql = @"
            INSERT INTO Cities (CityName, StateId, IsActive)
            VALUES (@CityName, @StateId, @IsActive);
            SELECT LAST_INSERT_ID();";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, city);
    }

    public async Task<bool> UpdateAsync(City city)
    {
        const string sql = @"
            UPDATE Cities
            SET CityName = @CityName,
                StateId  = @StateId,
                IsActive = @IsActive
            WHERE CityId = @CityId;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, city);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Cities WHERE CityId = @Id;";

        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<int> GetStudentCountAsync(int id)
    {
        const string sql = "SELECT COUNT(*) FROM Students WHERE CityId = @Id;";

        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
    }
}