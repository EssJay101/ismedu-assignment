using System.ComponentModel.DataAnnotations;

namespace Ismedu.Api.DTOs;

public class CountryRequest
{
    [Required(ErrorMessage = "Country name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Country name must be 2 to 100 characters.")]
    public string CountryName { get; set; } = string.Empty;

    [RegularExpression("^[A-Za-z]{2,3}$", ErrorMessage = "Country code must be 2 or 3 letters.")]
    public string? CountryCode { get; set; }

    public bool IsActive { get; set; } = true;
}