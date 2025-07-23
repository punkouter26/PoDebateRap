using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace PoDebateRap.ServerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/Error")]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (context != null)
            {
                var exception = context.Error;
                _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

                // Return a generic 500 Internal Server Error response
                return Problem(
                    detail: "An unexpected error occurred. Please try again later.",
                    title: "Internal Server Error",
                    statusCode: 500);
            }

            _logger.LogWarning("Error endpoint hit without exception context.");
            return Problem(
                detail: "An unknown error occurred.",
                title: "Unknown Error",
                statusCode: 500);
        }
    }
}
