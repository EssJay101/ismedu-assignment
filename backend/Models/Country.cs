namespace Ismedu.Api.Models;

public class Country
{
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public bool IsActive { get; set; }
}