namespace Ismedu.Api.DTOs;

public class CityQuery : PagedQuery
{
    public int? CountryId { get; set; }
    public int? StateId { get; set; }
    public bool? IsActive { get; set; }
}