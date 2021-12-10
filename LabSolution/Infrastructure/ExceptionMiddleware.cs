using LabSolution.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace LabSolution.Infrastructure
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IConfiguration _configurationSettings;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IConfiguration configurationSettings)
        {
            _next = next;
            _logger = logger;
            _configurationSettings = configurationSettings;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (AccessViolationException avEx)
            {
                _logger.LogCritical($"A new violation exception has been thrown: {avEx}");
                await HandleExceptionAsync(httpContext, avEx);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            try
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string response;

                switch (exception)
                {
                    case ResourceNotFoundException:
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            const string exceptionMessage = "Resource not found";
                            response = JsonSerializer.Serialize(exceptionMessage, options);
                            break;
                        }
                    case CustomException:
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response = JsonSerializer.Serialize(exception.Message, options);
                            break;
                        }
                    case Exception:
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            var exceptionMessage = $"An exception has been occured. {GetMessageDetails(exception)}";
                            response = JsonSerializer.Serialize(exceptionMessage, options);
                            break;
                        }
                    default:
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var exceptionMessage = $"Oops. There is a problem on the server. {GetMessageDetails(exception)}";
                        response = JsonSerializer.Serialize(exceptionMessage, options);
                        break;
                    }
                }

                await context.Response.WriteAsync(response);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, ex.Message);
            }
        }

        private object GetMessageDetails(Exception exception)
        {
            var showDetailedErrorInProduction = _configurationSettings["ShowDetailedErrorsInProd"].Equals("true", StringComparison.InvariantCultureIgnoreCase);
            return showDetailedErrorInProduction ? $"Details: {exception.Message}" : "Please contact system administrator";
        }
    }
}
