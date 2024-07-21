using BookStoreApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookStoreApi.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }

        if (!context.Response.HasStarted &&
            context.Response.StatusCode > 400 &&
            context.Response.StatusCode < 600 &&
            context.Response.StatusCode != 404)
        {
            await HandleStatusCodeAsync(context);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unexpected error occurred.");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred."
        };

        await context.Response.WriteAsync(response.ToString());
    }

    private Task HandleStatusCodeAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = GetStatusCodeMessage(context)
        };

        return context.Response.WriteAsync(response.ToString());
    }

    public static string GetStatusCodeMessage(HttpContext context)
    {
        var statusCodeField = typeof(StatusCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(f => (int?)f.GetValue(null) == context.Response.StatusCode);

        if (statusCodeField == null)
        {
            return "An error occurred";
        }

        var name = statusCodeField.Name.Replace("Status", "");
        var spacedName = string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        return spacedName;
    }
}
