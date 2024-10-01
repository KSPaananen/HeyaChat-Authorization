using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace HeyaChat_Authorization.Middleware
{
    /// <summary>
    ///     <para>This middleware will automatically check validity of the authorization header.</para>
    /// </summary>
    public class AuthorizeHeaderMiddleware
    {
        private RequestDelegate _requestDel;

        public AuthorizeHeaderMiddleware(RequestDelegate requestDel)
        {
            _requestDel = requestDel ?? throw new ArgumentNullException(nameof(requestDel));
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            var _jwtService = serviceProvider.GetRequiredService<IJwtService>();
            var _deviceRepository = serviceProvider.GetRequiredService<IDevicesRepository>();

            // Check if Authorization header is present in the request and it's not empty
            if (context.Request.Headers.ContainsKey("Authorization") && context.Request.Headers.Authorization != "")
            {
                // We also need device information from requests body. This SHOULD BE in every request made by frontend
                UserDevice userDevice = await ReadDeviceFromBody(context);

                // Now that we have device object from body, lets use it verify if it has an active jti associated with it
                if (userDevice.DeviceName != "")
                {
                    // Read claims from Request
                    var claims = _jwtService.GetClaims(context.Request);

                    Guid jti = claims.jti;
                    long userId = claims.userId;
                    string type = claims.type;

                    // Check if jti in token is valid. VerifyToken will automatically renew token if its below set renew time.
                    var tokenResults = _jwtService.VerifyToken(jti, userDevice);

                    // Proceed to next middleware if token was valid. Throw AccessViolation if no active jti was found with users device, thus they're logged out
                    if (tokenResults.isValid)
                    {
                        // Check if token is about to expire, renew if true
                        if (tokenResults.expiresSoon)
                        {
                            // Get DeviceId to renew the token
                            var result = _deviceRepository.GetDeviceWithUUID(userDevice.DeviceIdentifier);

                            context.Response.Headers.Authorization = _jwtService.RenewToken(userId, result.DeviceId, type, jti);
                        }

                        await _requestDel(context);
                    }
                    else
                    {
                        throw new AccessViolationException();
                    }
                }
            }
            else
            {
                // Since no Authorization header was present, proceed to the next middleware
                await _requestDel(context);
            }
        }

        private async Task<UserDevice> ReadDeviceFromBody(HttpContext context)
        {
            // Enable buffering so controller methods can access request body
            context.Request.EnableBuffering();

            // We also need device information from requests body. This SHOULD BE in every request made by frontend
            UserDevice userDevice = new UserDevice();

            StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);

            string body = await reader.ReadToEndAsync();

            if (body != null)
            {
                try
                {
                    JsonDocument jsonDocument = JsonDocument.Parse(body);

                    if (jsonDocument.RootElement.TryGetProperty("device", out var deviceObject))
                    {
                        string guidString = deviceObject.GetProperty("deviceIdentifier").ToString();

                        userDevice.DeviceIdentifier = Guid.Parse(guidString);
                        userDevice.DeviceName = deviceObject.GetProperty("deviceName").ToString();
                        userDevice.CountryCode = deviceObject.GetProperty("countryTag").ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw new FormatException(ex.Message);
                }

                // Remember to rewing position back to 0 or controller methods will have trouble reading json from body
                context.Request.Body.Position = 0;
            }

            return userDevice;
        }

    }
}
