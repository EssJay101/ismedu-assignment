using Ismedu.Api.DTOs;
using Ismedu.Api.Exceptions;
using Ismedu.Api.Models;
using Ismedu.Api.Repositories;

namespace Ismedu.Api.Services;

public interface ICityService
{
    Task<PagedResult<CityResponse>> GetPagedAsync(CityQuery query);
    Task<CityResponse> GetByIdAsync(int id);
    Task<IEnumerable<CityOption>> GetOptionsByStateAsync(int stateId);
    Task<CityResponse> CreateAsync(CityRequest request);
    Task<CityResponse> UpdateAsync(int id, CityRequest request);
    Task DeleteAsync(int id);
}

public class CityService : ICityService
{
    private readonly ICityRepository _cities;
    private readonly IStateRepository _states;

    public CityService(ICityRepository cities, IStateRepository states)
    {
        _cities = cities;
        _states = states;
    }

    public async Task<PagedResult<CityResponse>> GetPagedAsync(CityQuery query)
    {
        var (items, totalCount) = await _cities.GetPagedAsync(query);

        return new PagedResult<CityResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CityResponse> GetByIdAsync(int id)
    {
        var city = await _cities.GetByIdAsync(id)
            ?? throw new NotFoundException($"City with ID {id} was not found.");

        return ToResponse(city);
    }

    public async Task<IEnumerable<CityOption>> GetOptionsByStateAsync(int stateId)
    {
        var state = await _states.GetByIdAsync(stateId);
        if (state is null)
        {
            throw new NotFoundException($"State with ID {stateId} was not found.");
        }

        var cities = await _cities.GetActiveByStateAsync(stateId);

        return cities
            .Select(c => new CityOption { CityId = c.CityId, CityName = c.CityName })
            .ToList();
    }

    public async Task<CityResponse> CreateAsync(CityRequest request)
    {
        var city = Clean(request);
        var state = await GetStateOrThrowAsync(city.StateId);
        await EnsureUniqueAsync(city, state, excludeId: null);

        city.CityId = await _cities.CreateAsync(city);
        FillNames(city, state);
        return ToResponse(city);
    }

    public async Task<CityResponse> UpdateAsync(int id, CityRequest request)
    {
        var existing = await _cities.GetByIdAsync(id)
            ?? throw new NotFoundException($"City with ID {id} was not found.");

        var city = Clean(request);
        city.CityId = existing.CityId;

        var state = await GetStateOrThrowAsync(city.StateId);

        if (city.StateId != existing.StateId)
        {
            var studentCount = await _cities.GetStudentCountAsync(id);
            if (studentCount > 0)
            {
                throw new ConflictException(
                    $"Cannot move '{existing.CityName}' to another state because " +
                    $"{studentCount} student(s) are registered in it.",
                    new Dictionary<string, string[]>
                    {
                        ["stateId"] = new[] { "This city is in use, so its state cannot be changed." }
                    });
            }
        }

        await EnsureUniqueAsync(city, state, excludeId: id);

        await _cities.UpdateAsync(city);
        FillNames(city, state);
        return ToResponse(city);
    }

    public async Task DeleteAsync(int id)
    {
        var city = await _cities.GetByIdAsync(id)
            ?? throw new NotFoundException($"City with ID {id} was not found.");

        var studentCount = await _cities.GetStudentCountAsync(id);
        if (studentCount > 0)
        {
            throw new ConflictException(
                $"Cannot delete '{city.CityName}' because {studentCount} student(s) are " +
                $"registered in it. You can mark it as inactive instead.");
        }

        await _cities.DeleteAsync(id);
    }

    // Helpers

    private static City Clean(CityRequest request)
    {
        var name = request.CityName.Trim();
        if (name.Length < 2)
        {
            throw new BadRequestException("City name must be 2 to 100 characters.",
                new Dictionary<string, string[]>
                {
                    ["cityName"] = new[] { "City name must be 2 to 100 characters." }
                });
        }

        return new City
        {
            CityName = name,
            StateId = request.StateId,
            IsActive = request.IsActive
        };
    }

    private async Task<State> GetStateOrThrowAsync(int stateId)
    {
        var state = await _states.GetByIdAsync(stateId);
        if (state is null)
        {
            throw new BadRequestException("Selected state does not exist.",
                new Dictionary<string, string[]>
                {
                    ["stateId"] = new[] { "Selected state does not exist." }
                });
        }

        return state;
    }

    private async Task EnsureUniqueAsync(City city, State state, int? excludeId)
    {
        if (await _cities.NameExistsAsync(city.CityName, city.StateId, excludeId))
        {
            var message = $"A city named '{city.CityName}' already exists in {state.StateName}.";
            throw new ConflictException(message,
                new Dictionary<string, string[]>
                {
                    ["cityName"] = new[] { message }
                });
        }
    }

    private static void FillNames(City city, State state)
    {
        city.StateName = state.StateName;
        city.CountryId = state.CountryId;
        city.CountryName = state.CountryName;
    }

    private static CityResponse ToResponse(City city)
    {
        return new CityResponse
        {
            CityId = city.CityId,
            CityName = city.CityName,
            StateId = city.StateId,
            StateName = city.StateName,
            CountryId = city.CountryId,
            CountryName = city.CountryName,
            IsActive = city.IsActive
        };
    }
}