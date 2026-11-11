using System.Data;
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
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        using var connection = _db.CreateConnection();
        using var results = await connection.QueryMultipleAsync(
            "sp_State_GetPaged",
            new
            {
                p_Search = search,
                p_CountryId = query.CountryId,
                p_IsActive = query.IsActive,
                p_Page = query.Page,
                p_PageSize = query.PageSize
            },
            commandType: CommandType.StoredProcedure);

        var totalCount = await results.ReadSingleAsync<int>();
        var items = (await results.ReadAsync<State>()).ToList();

        return (items, totalCount);
    }

    public async Task<State?> GetByIdAsync(int id)
    {
        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<State>(
            "sp_State_GetById",
            new { p_StateId = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<State>> GetActiveByCountryAsync(int countryId)
    {
        using var connection = _db.CreateConnection();
        return await connection.QueryAsync<State>(
            "sp_State_GetActiveByCountry",
            new { p_CountryId = countryId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> NameExistsAsync(string name, int countryId, int? excludeId = null)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            "sp_State_NameExists",
            new { p_StateName = name, p_CountryId = countryId, p_ExcludeId = excludeId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(State state)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "sp_State_Insert",
            new
            {
                p_StateName = state.StateName,
                p_CountryId = state.CountryId,
                p_IsActive = state.IsActive
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(State state)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_State_Update",
            new
            {
                p_StateId = state.StateId,
                p_StateName = state.StateName,
                p_CountryId = state.CountryId,
                p_IsActive = state.IsActive
            },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_State_Delete",
            new { p_StateId = id },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<(int CityCount, int StudentCount)> GetUsageAsync(int id)
    {
        using var connection = _db.CreateConnection();
        var usage = await connection.QuerySingleAsync<StateUsage>(
            "sp_State_GetUsage",
            new { p_StateId = id },
            commandType: CommandType.StoredProcedure);

        return (usage.CityCount, usage.StudentCount);
    }

    private sealed class StateUsage
    {
        public int CityCount { get; set; }
        public int StudentCount { get; set; }
    }
}