using System.Net;

namespace HeyaChat_Authorization.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private RequestDelegate _requestDel;
        // Add logger later

        public ExceptionHandlerMiddleware(RequestDelegate requestDel)
        {
            _requestDel = requestDel ?? throw new NullReferenceException(nameof(requestDel));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _requestDel(context);
            }
            catch (Exception ex)
            {
                // *Error logging here*

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Set error code and content type
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            // Set error message
            var errorResponse = new
            {
                message = "An internal server error occurred.",
                details = ex.Message
            };

            return context.Response.WriteAsJsonAsync(errorResponse);
        }


    }
}
