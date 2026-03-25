
using Microsoft.AspNetCore.Mvc;
using MoveLens.Api.Models;
using System.Net;
using System.Text.Json;

namespace MoveLens.API.Middlewares
{
    public sealed class GlobalExceptionMiddleware(
     ILogger<GlobalExceptionMiddleware> logger) : IMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };


        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Unhandled exception on {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);


                var problemDetails = new ProblemDetails()
                {
                    Title = "Internal Server Error",
                    Detail = ex.Message,
                    Status = (int)HttpStatusCode.InternalServerError,
                    Instance = context.Request.Path
                };


                context.Response.ContentType = "application/json";
                context.Response.StatusCode = problemDetails.Status.Value;

                await context.Response.WriteAsJsonAsync(problemDetails);


            }
        }
    }
}