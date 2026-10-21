using System.Diagnostics;
using System.Text.Json;
using Ismedu.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Ismedu.Api.Middleware;

public static class ValidationErrorFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
            .ToDictionary(
                entry => JsonNamingPolicy.CamelCase.ConvertName(entry.Key),
                entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var response = new ErrorResponse
        {
            Status = StatusCodes.Status400BadRequest,
            Message = "One or more fields are invalid.",
            Errors = errors,
            TraceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    }
}