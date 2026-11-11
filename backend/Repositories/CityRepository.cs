using System.Data;
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
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        using var connection = _db.CreateConnection();
        using var results = await connection.QueryMultipleAsync(
            "sp_City_GetPaged",
            new
            {
                p_Search = search,
                p_CountryId = query.CountryId,
                p_StateId = query.StateId,
                p_IsActive = query.IsActive,
                p_Page = query.Page,
                p_PageSize = query.PageSize
            },
            commandType: CommandType.StoredProcedure);

        var totalCount = await results.ReadSingleAsync<int>();
        var items = (await results.ReadAsync<City>()).ToList();

        return (items, totalCount);
    }

    public async Task<City?> GetByIdAsync(int id)
    {
        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<City>(
            "sp_City_GetById",
            new { p_CityId = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<City>> GetActiveByStateAsync(int stateId)
    {
        using var connection = _db.CreateConnection();
        return await connection.QueryAsync<City>(
            "sp_City_GetActiveByState",
            new { p_StateId = stateId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> NameExistsAsync(string name, int stateId, int? excludeId = null)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            "sp_City_NameExists",
            new { p_CityName = name, p_StateId = stateId, p_ExcludeId = excludeId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(City city)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "sp_City_Insert",
            new
            {
                p_CityName = city.CityName,
                p_StateId = city.StateId,
                p_IsActive = city.IsActive
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(City city)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_City_Update",
            new
            {
                p_CityId = city.CityId,
                p_CityName = city.CityName,
                p_StateId = city.StateId,
                p_IsActive = city.IsActive
            },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_City_Delete",
            new { p_CityId = id },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<int> GetStudentCountAsync(int id)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "sp_City_GetStudentCount",
            new { p_CityId = id },
            commandType: CommandType.StoredProcedure);
    }
}