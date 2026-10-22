using Ismedu.Api.DTOs;
using Ismedu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ismedu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _service;

    public CountriesController(ICountryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CountryResponse>>> GetAll([FromQuery] CountryQuery query)
    {
        var result = await _service.GetPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CountryResponse>> GetById(int id)
    {
        var country = await _service.GetByIdAsync(id);
        return Ok(country);
    }

    [HttpPost]
    public async Task<ActionResult<CountryResponse>> Create([FromBody] CountryRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.CountryId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CountryResponse>> Update(int id, [FromBody] CountryRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("options")]
    public async Task<ActionResult<IEnumerable<CountryOption>>> GetOptions()
    {
        var options = await _service.GetOptionsAsync();
        return Ok(options);
    }
}