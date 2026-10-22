using Ismedu.Api.DTOs;
using Ismedu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ismedu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatesController : ControllerBase
{
    private readonly IStateService _service;

    public StatesController(IStateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<StateResponse>>> GetAll([FromQuery] StateQuery query)
    {
        var result = await _service.GetPagedAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StateResponse>> GetById(int id)
    {
        var state = await _service.GetByIdAsync(id);
        return Ok(state);
    }

    [HttpGet("/api/countries/{countryId:int}/states")]
    public async Task<ActionResult<IEnumerable<StateOption>>> GetByCountry(int countryId)
    {
        var options = await _service.GetOptionsByCountryAsync(countryId);
        return Ok(options);
    }

    [HttpPost]
    public async Task<ActionResult<StateResponse>> Create([FromBody] StateRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.StateId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StateResponse>> Update(int id, [FromBody] StateRequest request)
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