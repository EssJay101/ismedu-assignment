using System.ComponentModel.DataAnnotations;

namespace Ismedu.Api.DTOs;

public class CityRequest
{
    [Required(ErrorMessage = "City name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City name must be 2 to 100 characters.")]
    public string CityName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Please select a state.")]
    public int StateId { get; set; }

    public bool IsActive { get; set; } = true;
}