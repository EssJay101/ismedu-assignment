using Ismedu.Api.DTOs;
using Ismedu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ismedu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitiesController : ControllerBase
{
    private readonly ICityService _service;

    public CitiesController(ICityService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CityResponse>>> GetAll([FromQuery] CityQuery query)
    {
        var result = await _service.GetPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CityResponse>> GetById(int id)
    {
        var city = await _service.GetByIdAsync(id);
        return Ok(city);
    }

    [HttpGet("/api/states/{stateId:int}/cities")]
    public async Task<ActionResult<IEnumerable<CityOption>>> GetByState(int stateId)
    {
        var options = await _service.GetOptionsByStateAsync(stateId);
        return Ok(options);
    }

    [HttpPost]
    public async Task<ActionResult<CityResponse>> Create([FromBody] CityRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.CityId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CityResponse>> Update(int id, [FromBody] CityRequest request)
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
}