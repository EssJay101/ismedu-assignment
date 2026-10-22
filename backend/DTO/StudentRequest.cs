using System.ComponentModel.DataAnnotations;

namespace Ismedu.Api.DTOs;

public class StudentRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2 to 50 characters.")]
    [RegularExpression("^[A-Za-z' -]+$",
        ErrorMessage = "First name can only contain letters, spaces, apostrophes and hyphens.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2 to 50 characters.")]
    [RegularExpression("^[A-Za-z' -]+$",
        ErrorMessage = "Last name can only contain letters, spaces, apostrophes and hyphens.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required.")]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(254, ErrorMessage = "Email cannot be longer than 254 characters.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression("^[6-9][0-9]{9}$",
        ErrorMessage = "Mobile must be a 10-digit Indian number starting with 6, 7, 8 or 9.")]
    public string Mobile { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a country.")]
    public int CountryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a state.")]
    public int StateId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a city.")]
    public int CityId { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(250, MinimumLength = 10, ErrorMessage = "Address must be 10 to 250 characters.")]
    public string Address { get; set; } = string.Empty;
}