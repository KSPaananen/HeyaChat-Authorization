using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HeyaChat_Authorization.Middleware
{
    /// <summary>
    ///     <para>This middleware handles all unhandled exceptions.</para>
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private RequestDelegate _requestDel;

        private ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate requestDel, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _requestDel = requestDel ?? throw new NullReferenceException(nameof(requestDel));

            _logger = logger ?? throw new NullReferenceException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _requestDel(context);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Set content type to application/json
            context.Response.ContentType = "application/json";

            switch (ex)
            {
                case AccessViolationException: // Occurs when token is expired or not active
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    break;
                case FormatException: // Occurs when values in body are formatted wrong. For example GUI is incorrect length
                    context.Response.StatusCode = StatusCodes.Status406NotAcceptable;

                    break;
                default: // Default to 500 if reason is unclear
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    break;
            }

            return context.Response.CompleteAsync();

            // If you wish to provide further details, write this as json to response
            //var errorResponse = new
            //{
            //    message = "An internal server error has occured.",
            //    details = ex.Message
            //};
            //return context.Response.WriteAsJsonAsync(errorResponse);
        }


    }
}
