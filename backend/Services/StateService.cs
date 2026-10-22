using Ismedu.Api.DTOs;
using Ismedu.Api.Exceptions;
using Ismedu.Api.Models;
using Ismedu.Api.Repositories;

namespace Ismedu.Api.Services;

public interface IStateService
{
    Task<PagedResult<StateResponse>> GetPagedAsync(StateQuery query);
    Task<StateResponse> GetByIdAsync(int id);
    Task<IEnumerable<StateOption>> GetOptionsByCountryAsync(int countryId);
    Task<StateResponse> CreateAsync(StateRequest request);
    Task<StateResponse> UpdateAsync(int id, StateRequest request);
    Task DeleteAsync(int id);
}

public class StateService : IStateService
{
    private readonly IStateRepository _states;
    private readonly ICountryRepository _countries;

    public StateService(IStateRepository states, ICountryRepository countries)
    {
        _states = states;
        _countries = countries;
    }

    public async Task<PagedResult<StateResponse>> GetPagedAsync(StateQuery query)
    {
        var (items, totalCount) = await _states.GetPagedAsync(query);

        return new PagedResult<StateResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StateResponse> GetByIdAsync(int id)
    {
        var state = await _states.GetByIdAsync(id)
            ?? throw new NotFoundException($"State with ID {id} was not found.");

        return ToResponse(state);
    }

    public async Task<IEnumerable<StateOption>> GetOptionsByCountryAsync(int countryId)
    {
        var country = await _countries.GetByIdAsync(countryId);
        if (country is null)
        {
            throw new NotFoundException($"Country with ID {countryId} was not found.");
        }

        var states = await _states.GetActiveByCountryAsync(countryId);

        return states
            .Select(s => new StateOption { StateId = s.StateId, StateName = s.StateName })
            .ToList();
    }

    public async Task<StateResponse> CreateAsync(StateRequest request)
    {
        var state = Clean(request);
        var country = await GetCountryOrThrowAsync(state.CountryId);
        await EnsureUniqueAsync(state, country, excludeId: null);

        state.StateId = await _states.CreateAsync(state);
        state.CountryName = country.CountryName;
        return ToResponse(state);
    }

    public async Task<StateResponse> UpdateAsync(int id, StateRequest request)
    {
        var existing = await _states.GetByIdAsync(id)
            ?? throw new NotFoundException($"State with ID {id} was not found.");

        var state = Clean(request);
        state.StateId = existing.StateId;

        var country = await GetCountryOrThrowAsync(state.CountryId);

        if (state.CountryId != existing.CountryId)
        {
            var (cityCount, studentCount) = await _states.GetUsageAsync(id);
            if (cityCount > 0 || studentCount > 0)
            {
                throw new ConflictException(
                    $"Cannot move '{existing.StateName}' to another country because it has " +
                    $"{cityCount} city(ies) and {studentCount} student(s).",
                    new Dictionary<string, string[]>
                    {
                        ["countryId"] = new[] { "This state is in use, so its country cannot be changed." }
                    });
            }
        }

        await EnsureUniqueAsync(state, country, excludeId: id);

        await _states.UpdateAsync(state);
        state.CountryName = country.CountryName;
        return ToResponse(state);
    }

    public async Task DeleteAsync(int id)
    {
        var state = await _states.GetByIdAsync(id)
            ?? throw new NotFoundException($"State with ID {id} was not found.");

        var (cityCount, studentCount) = await _states.GetUsageAsync(id);
        if (cityCount > 0 || studentCount > 0)
        {
            throw new ConflictException(
                $"Cannot delete '{state.StateName}' because it has {cityCount} city(ies) " +
                $"and {studentCount} student(s). You can mark it as inactive instead.");
        }

        await _states.DeleteAsync(id);
    }

    // Helpers

    private static State Clean(StateRequest request)
    {
        var name = request.StateName.Trim();
        if (name.Length < 2)
        {
            throw new BadRequestException("State name must be 2 to 100 characters.",
                new Dictionary<string, string[]>
                {
                    ["stateName"] = new[] { "State name must be 2 to 100 characters." }
                });
        }

        return new State
        {
            StateName = name,
            CountryId = request.CountryId,
            IsActive = request.IsActive
        };
    }

    private async Task<Country> GetCountryOrThrowAsync(int countryId)
    {
        var country = await _countries.GetByIdAsync(countryId);
        if (country is null)
        {
            throw new BadRequestException("Selected country does not exist.",
                new Dictionary<string, string[]>
                {
                    ["countryId"] = new[] { "Selected country does not exist." }
                });
        }

        return country;
    }

    private async Task EnsureUniqueAsync(State state, Country country, int? excludeId)
    {
        if (await _states.NameExistsAsync(state.StateName, state.CountryId, excludeId))
        {
            var message = $"A state named '{state.StateName}' already exists in {country.CountryName}.";
            throw new ConflictException(message,
                new Dictionary<string, string[]>
                {
                    ["stateName"] = new[] { message }
                });
        }
    }

    private static StateResponse ToResponse(State state)
    {
        return new StateResponse
        {
            StateId = state.StateId,
            StateName = state.StateName,
            CountryId = state.CountryId,
            CountryName = state.CountryName,
            IsActive = state.IsActive
        };
    }
}