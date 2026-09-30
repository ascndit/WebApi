using Microsoft.AspNetCore.Mvc;
using WebApi.Exceptions;

namespace WebApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status404NotFound,
                    ex.Message);
            }
            catch (ConflictException ex)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Непредвиденная ошибка.");

                await WriteProblemAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Произошла внутренняя ошибка сервера.");
            }
        }

        private static async Task WriteProblemAsync(
            HttpContext context,
            int statusCode,
            string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = message
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
