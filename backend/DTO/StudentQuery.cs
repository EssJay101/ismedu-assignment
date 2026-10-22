namespace Ismedu.Api.DTOs;

public class StudentQuery : PagedQuery
{
    public int? CountryId { get; set; }
    public int? StateId { get; set; }
    public int? CityId { get; set; }
}