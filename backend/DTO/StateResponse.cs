namespace Ismedu.Api.DTOs;

public class StateResponse
{
    public int StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}