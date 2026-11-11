using System.Data;
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
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        using var connection = _db.CreateConnection();
        using var results = await connection.QueryMultipleAsync(
            "sp_Country_GetPaged",
            new
            {
                p_Search = search,
                p_IsActive = query.IsActive,
                p_Page = query.Page,
                p_PageSize = query.PageSize
            },
            commandType: CommandType.StoredProcedure);

        var totalCount = await results.ReadSingleAsync<int>();
        var items = (await results.ReadAsync<Country>()).ToList();

        return (items, totalCount);
    }

    public async Task<Country?> GetByIdAsync(int id)
    {
        using var connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Country>(
            "sp_Country_GetById",
            new { p_CountryId = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            "sp_Country_NameExists",
            new { p_CountryName = name, p_ExcludeId = excludeId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(
            "sp_Country_CodeExists",
            new { p_CountryCode = code, p_ExcludeId = excludeId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(Country country)
    {
        using var connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "sp_Country_Insert",
            new
            {
                p_CountryName = country.CountryName,
                p_CountryCode = country.CountryCode,
                p_IsActive = country.IsActive
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(Country country)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_Country_Update",
            new
            {
                p_CountryId = country.CountryId,
                p_CountryName = country.CountryName,
                p_CountryCode = country.CountryCode,
                p_IsActive = country.IsActive
            },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _db.CreateConnection();
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "sp_Country_Delete",
            new { p_CountryId = id },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<(int StateCount, int StudentCount)> GetUsageAsync(int id)
    {
        using var connection = _db.CreateConnection();
        var usage = await connection.QuerySingleAsync<CountryUsage>(
            "sp_Country_GetUsage",
            new { p_CountryId = id },
            commandType: CommandType.StoredProcedure);

        return (usage.StateCount, usage.StudentCount);
    }

    public async Task<IEnumerable<Country>> GetActiveAsync()
    {
        using var connection = _db.CreateConnection();
        return await connection.QueryAsync<Country>(
            "sp_Country_GetActive",
            commandType: CommandType.StoredProcedure);
    }

    private sealed class CountryUsage
    {
        public int StateCount { get; set; }
        public int StudentCount { get; set; }
    }
}