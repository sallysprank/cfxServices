using QBODataCollect.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Extensions.Logging;

namespace QBODataCollect.CustomExceptionMiddleware
{

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerManager _logger;

    public ExceptionMiddleware(RequestDelegate next, ILoggerManager logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong: {ex}");
            var statusCode = HttpStatusCode.InternalServerError;
                if (ex is ArgumentException) statusCode = HttpStatusCode.BadRequest;
                else if (ex is UnauthorizedAccessException) statusCode = HttpStatusCode.Forbidden;
            await HandleExceptionAsync(httpContext, ex, statusCode);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
            //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                //Message = "Internal Server Error from the custom middleware."
                Message = exception.Message
        }.ToString());
    }
}
}
