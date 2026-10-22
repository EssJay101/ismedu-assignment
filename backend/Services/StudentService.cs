using Ismedu.Api.DTOs;
using Ismedu.Api.Exceptions;
using Ismedu.Api.Models;
using Ismedu.Api.Repositories;

namespace Ismedu.Api.Services;

public interface IStudentService
{
    Task<PagedResult<StudentResponse>> GetPagedAsync(StudentQuery query);
    Task<StudentResponse> GetByIdAsync(int id);
    Task<StudentResponse> CreateAsync(StudentRequest request);
    Task<StudentResponse> UpdateAsync(int id, StudentRequest request);
    Task DeleteAsync(int id);
}

public class StudentService : IStudentService
{
    private static readonly DateOnly OldestAllowedDateOfBirth = new(1900, 1, 1);

    private readonly IStudentRepository _students;
    private readonly ICountryRepository _countries;
    private readonly IStateRepository _states;
    private readonly ICityRepository _cities;

    public StudentService(
        IStudentRepository students,
        ICountryRepository countries,
        IStateRepository states,
        ICityRepository cities)
    {
        _students = students;
        _countries = countries;
        _states = states;
        _cities = cities;
    }

    public async Task<PagedResult<StudentResponse>> GetPagedAsync(StudentQuery query)
    {
        var (items, totalCount) = await _students.GetPagedAsync(query);

        return new PagedResult<StudentResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<StudentResponse> GetByIdAsync(int id)
    {
        var student = await _students.GetByIdAsync(id)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        return ToResponse(student);
    }

    public async Task<StudentResponse> CreateAsync(StudentRequest request)
    {
        var student = Clean(request);
        await ValidateLocationAsync(student, requireActive: true);
        await EnsureUniqueAsync(student, excludeId: null);

        var newId = await _students.CreateAsync(student);

        var saved = await _students.GetByIdAsync(newId)
            ?? throw new InvalidOperationException("Student was saved but could not be loaded.");

        return ToResponse(saved);
    }

    public async Task<StudentResponse> UpdateAsync(int id, StudentRequest request)
    {
        var existing = await _students.GetByIdAsync(id)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        var student = Clean(request);
        student.StudentId = existing.StudentId;

        var locationChanged =
            student.CountryId != existing.CountryId ||
            student.StateId != existing.StateId ||
            student.CityId != existing.CityId;

        await ValidateLocationAsync(student, requireActive: locationChanged);
        await EnsureUniqueAsync(student, excludeId: id);

        await _students.UpdateAsync(student);

        var saved = await _students.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Student was updated but could not be loaded.");

        return ToResponse(saved);
    }

    public async Task DeleteAsync(int id)
    {
        _ = await _students.GetByIdAsync(id)
            ?? throw new NotFoundException($"Student with ID {id} was not found.");

        await _students.DeleteAsync(id);
    }

    // Helpers

    private static Student Clean(StudentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        var firstName = request.FirstName.Trim();
        if (firstName.Length < 2 || firstName.Length > 50)
        {
            errors["firstName"] = new[] { "First name must be 2 to 50 characters." };
        }

        var lastName = request.LastName.Trim();
        if (lastName.Length < 2 || lastName.Length > 50)
        {
            errors["lastName"] = new[] { "Last name must be 2 to 50 characters." };
        }

        var dateOfBirth = request.DateOfBirth!.Value;
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (dateOfBirth >= today)
        {
            errors["dateOfBirth"] = new[] { "Date of birth must be earlier than today." };
        }
        else if (dateOfBirth < OldestAllowedDateOfBirth)
        {
            errors["dateOfBirth"] = new[] { "Date of birth cannot be before 01-01-1900." };
        }

        var address = request.Address.Trim();
        if (address.Length < 10 || address.Length > 250)
        {
            errors["address"] = new[] { "Address must be 10 to 250 characters." };
        }

        if (errors.Count > 0)
        {
            throw new BadRequestException("One or more fields are invalid.", errors);
        }

        return new Student
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth.ToDateTime(TimeOnly.MinValue),
            Email = request.Email.Trim(),
            Mobile = request.Mobile.Trim(),
            CountryId = request.CountryId,
            StateId = request.StateId,
            CityId = request.CityId,
            Address = address
        };
    }

    private async Task ValidateLocationAsync(Student student, bool requireActive)
    {
        var errors = new Dictionary<string, string[]>();

        var country = await _countries.GetByIdAsync(student.CountryId);
        var state = await _states.GetByIdAsync(student.StateId);
        var city = await _cities.GetByIdAsync(student.CityId);

        if (country is null)
        {
            errors["countryId"] = new[] { "Selected country does not exist." };
        }
        else if (requireActive && !country.IsActive)
        {
            errors["countryId"] = new[] { "Selected country is inactive." };
        }

        if (state is null)
        {
            errors["stateId"] = new[] { "Selected state does not exist." };
        }
        else if (state.CountryId != student.CountryId)
        {
            errors["stateId"] = new[] { "Selected state does not belong to the selected country." };
        }
        else if (requireActive && !state.IsActive)
        {
            errors["stateId"] = new[] { "Selected state is inactive." };
        }

        if (city is null)
        {
            errors["cityId"] = new[] { "Selected city does not exist." };
        }
        else if (city.StateId != student.StateId)
        {
            errors["cityId"] = new[] { "Selected city does not belong to the selected state." };
        }
        else if (requireActive && !city.IsActive)
        {
            errors["cityId"] = new[] { "Selected city is inactive." };
        }

        if (errors.Count > 0)
        {
            throw new BadRequestException("The selected location is not valid.", errors);
        }
    }

    private async Task EnsureUniqueAsync(Student student, int? excludeId)
    {
        var errors = new Dictionary<string, string[]>();

        if (await _students.EmailExistsAsync(student.Email, excludeId))
        {
            errors["email"] = new[] { "This email is already registered." };
        }

        if (await _students.MobileExistsAsync(student.Mobile, excludeId))
        {
            errors["mobile"] = new[] { "This mobile number is already registered." };
        }

        if (errors.Count > 0)
        {
            throw new ConflictException("Some details are already registered to another student.", errors);
        }
    }

    private static StudentResponse ToResponse(Student student)
    {
        return new StudentResponse
        {
            StudentId = student.StudentId,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = DateOnly.FromDateTime(student.DateOfBirth),
            Email = student.Email,
            Mobile = student.Mobile,
            CountryId = student.CountryId,
            CountryName = student.CountryName,
            StateId = student.StateId,
            StateName = student.StateName,
            CityId = student.CityId,
            CityName = student.CityName,
            Address = student.Address,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt
        };
    }
}