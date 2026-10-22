using System.ComponentModel.DataAnnotations;

namespace Ismedu.Api.DTOs;

public class StateRequest
{
    [Required(ErrorMessage = "State name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "State name must be 2 to 100 characters.")]
    public string StateName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a country.")]
    public int CountryId { get; set; }

    public bool IsActive { get; set; } = true;
}