namespace Ismedu.Api.DTOs;

public class StateQuery : PagedQuery
{
    public int? CountryId { get; set; }
    public bool? IsActive { get; set; }
}