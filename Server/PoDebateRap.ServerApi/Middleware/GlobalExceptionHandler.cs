using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace PoDebateRap.ServerApi.Middleware
{
    /// <summary>
    /// Global exception handling middleware that transforms all unhandled exceptions
    /// into RFC 7807 Problem Details responses.
    /// </summary>
    public class GlobalExceptionHandler : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null argument in request to {Path}", context.Request.Path);
                await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, "Invalid Request", ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in request to {Path}", context.Request.Path);
                await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, "Invalid Request", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation in {Path}", context.Request.Path);
                await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, "Invalid Operation", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in {Path}", context.Request.Path);
                await WriteErrorResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred. Please try again later.");
            }
        }

        private static async Task WriteErrorResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string title,
            string detail)
        {
            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{(int)statusCode}"
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
