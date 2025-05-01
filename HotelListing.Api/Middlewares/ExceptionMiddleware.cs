using HotelListing.Api.Exceptions;
using Newtonsoft.Json;
using System.Net;

namespace HotelListing.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddleware> logger;

        public ExceptionMiddleware(RequestDelegate next,ILogger<ExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Something went wrong in the {httpContext.Request.Path}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.ContentType = "application/json";
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            var errorDetails = new ErrorDetails
            {
                ErrorType="Failure",
                ErrorMessage=ex.Message
            };

            switch (ex)
            {
                case NotFoundException notFoundException:
                     statusCode = HttpStatusCode.NotFound;
                    errorDetails.ErrorType = "Not Found";
                    break;
                 
                default:
                    break;
            }

            string response = JsonConvert.SerializeObject(errorDetails);
            httpContext.Response.StatusCode = (int)statusCode;
            return httpContext.Response.WriteAsync(response);
        }

        public class ErrorDetails
        {
            public string ErrorType;
            public string ErrorMessage;
        }
    }
}
