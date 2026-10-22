namespace Ismedu.Api.Models;

public class City
{
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int StateId { get; set; }
    public bool IsActive { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
}