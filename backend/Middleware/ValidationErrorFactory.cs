using System.Diagnostics;
using System.Text.Json;
using Ismedu.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Ismedu.Api.Middleware;

public static class ValidationErrorFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var parameterNames = context.ActionDescriptor.Parameters
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var errors = context.ModelState
            .Where(entry => entry.Value != null && entry.Value.Errors.Count > 0)
            .Where(entry => !parameterNames.Contains(entry.Key))
            .ToDictionary(
                entry => CleanKey(entry.Key),
                entry => entry.Key.StartsWith("$")
                    ? new[] { "This value is not in a valid format." }
                    : entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var response = new ErrorResponse
        {
            Status = StatusCodes.Status400BadRequest,
            Message = "One or more fields are invalid.",
            Errors = errors,
            TraceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    }

    private static string CleanKey(string key)
    {
        if (key.StartsWith("$."))
        {
            key = key.Substring(2);
        }
        else if (key == "$")
        {
            key = string.Empty;
        }
        return JsonNamingPolicy.CamelCase.ConvertName(key);
    }
}