using Microsoft.AspNetCore.Http;

namespace APIBestPractices.Shared.Common;

public class Error
{
    public static IHttpContextAccessor? HttpContextAccessor { get; set; } // Optional, can be injected for context

    // Problem Details for API responses
    public string Type { get; set; } = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public string Title { get; set; } = "An error occurred while processing your request.";
    public int Status { get; set; } = (int)StatusCodes.Status500InternalServerError;
    public string[] Detail { get; set; } = ["An unexpected error occurred. Please try again later."];
    public string? Instance { get; set; } = null; // Will be set by the framework

    public Error(string type, string title, int status, string[] detail, string? instace = null) 
    {
        Type = type;
        Title = title;
        Status = status;
        Detail = detail;
        Instance = HttpContextAccessor?.HttpContext.Request.Path;
    }

    public static Error ValidationError(string title, string[] detail, string? instance = null) 
        => new("https://tools.ietf.org/html/rfc7231#section-6.5.1", title, (int) StatusCodes.Status400BadRequest, detail, instance);


    public static Error NotFoundError(string title, string[] detail, string? instance = null) 
        => new("https://tools.ietf.org/html/rfc7231#section-6.5.4", title, (int) StatusCodes.Status404NotFound, detail, instance);

}

public enum StatusCodes
{
    Status400BadRequest = 400,
    Status401Unauthorized = 401,
    Status403Forbidden = 403,
    Status404NotFound = 404,
    Status409Conflict = 409,
    Status500InternalServerError = 500
}
