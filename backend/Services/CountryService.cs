using Ismedu.Api.DTOs;
using Ismedu.Api.Exceptions;
using Ismedu.Api.Models;
using Ismedu.Api.Repositories;

namespace Ismedu.Api.Services;

public interface ICountryService
{
    Task<PagedResult<CountryResponse>> GetPagedAsync(CountryQuery query);
    Task<CountryResponse> GetByIdAsync(int id);
    Task<CountryResponse> CreateAsync(CountryRequest request);
    Task<CountryResponse> UpdateAsync(int id, CountryRequest request);
    Task DeleteAsync(int id);
    Task<IEnumerable<CountryOption>> GetOptionsAsync();
}

public class CountryService : ICountryService
{
    private readonly ICountryRepository _countries;

    public CountryService(ICountryRepository countries)
    {
        _countries = countries;
    }

    public async Task<PagedResult<CountryResponse>> GetPagedAsync(CountryQuery query)
    {
        var (items, totalCount) = await _countries.GetPagedAsync(query);

        return new PagedResult<CountryResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CountryResponse> GetByIdAsync(int id)
    {
        var country = await _countries.GetByIdAsync(id)
            ?? throw new NotFoundException($"Country with ID {id} was not found.");

        return ToResponse(country);
    }

    public async Task<CountryResponse> CreateAsync(CountryRequest request)
    {
        var country = Clean(request);
        await EnsureUniqueAsync(country, excludeId: null);

        country.CountryId = await _countries.CreateAsync(country);
        return ToResponse(country);
    }

    public async Task<CountryResponse> UpdateAsync(int id, CountryRequest request)
    {
        var existing = await _countries.GetByIdAsync(id)
            ?? throw new NotFoundException($"Country with ID {id} was not found.");

        var country = Clean(request);
        country.CountryId = existing.CountryId;
        await EnsureUniqueAsync(country, excludeId: id);

        await _countries.UpdateAsync(country);
        return ToResponse(country);
    }

    public async Task DeleteAsync(int id)
    {
        var country = await _countries.GetByIdAsync(id)
            ?? throw new NotFoundException($"Country with ID {id} was not found.");

        var (stateCount, studentCount) = await _countries.GetUsageAsync(id);
        if (stateCount > 0 || studentCount > 0)
        {
            throw new ConflictException(
                $"Cannot delete '{country.CountryName}' because it has {stateCount} state(s) " +
                $"and {studentCount} student(s). You can mark it as inactive instead.");
        }

        await _countries.DeleteAsync(id);
    }
    
    public async Task<IEnumerable<CountryOption>> GetOptionsAsync()
    {
        var countries = await _countries.GetActiveAsync();

        return countries
            .Select(c => new CountryOption { CountryId = c.CountryId, CountryName = c.CountryName })
            .ToList();
    }


    // Helpers

    private static Country Clean(CountryRequest request)
    {
        var name = request.CountryName.Trim();
        if (name.Length < 2)
        {
            throw new BadRequestException("Country name must be 2 to 100 characters.",
                new Dictionary<string, string[]>
                {
                    ["countryName"] = new[] { "Country name must be 2 to 100 characters." }
                });
        }

        var code = string.IsNullOrWhiteSpace(request.CountryCode)
            ? null
            : request.CountryCode.Trim().ToUpperInvariant();

        return new Country
        {
            CountryName = name,
            CountryCode = code,
            IsActive = request.IsActive
        };
    }

    private async Task EnsureUniqueAsync(Country country, int? excludeId)
    {
        var errors = new Dictionary<string, string[]>();

        if (await _countries.NameExistsAsync(country.CountryName, excludeId))
        {
            errors["countryName"] = new[] { "A country with this name already exists." };
        }

        if (country.CountryCode is not null &&
            await _countries.CodeExistsAsync(country.CountryCode, excludeId))
        {
            errors["countryCode"] = new[] { "A country with this code already exists." };
        }

        if (errors.Count > 0)
        {
            throw new ConflictException("A country with the same details already exists.", errors);
        }
    }

    private static CountryResponse ToResponse(Country country)
    {
        return new CountryResponse
        {
            CountryId = country.CountryId,
            CountryName = country.CountryName,
            CountryCode = country.CountryCode,
            IsActive = country.IsActive
        };
    }
}