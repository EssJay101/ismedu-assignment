namespace Ismedu.Api.Models;

public class State
{
    public int StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public bool IsActive { get; set; }
    public string CountryName { get; set; } = string.Empty;
}