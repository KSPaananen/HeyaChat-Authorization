using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HeyaChat_Authorization.AuthorizeAttributes
{
    /// <summary>
    ///     <para>This AuthorizeAttribute allows controller method filtering based on token type claim.</para>
    /// </summary>
    public class TokenTypeAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private string[] _typeArray;

        public TokenTypeAuthorizeAttribute(params string[] typeArray)
        {
            _typeArray = typeArray;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User ?? null;

            if (user != null && user.Identity != null)
            {
                // Make sure user is authenticated
                if (!user.Identity.IsAuthenticated)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Get the 'type' claim from the token
                var type = user.Claims.FirstOrDefault(c => c.Type == "typ")?.Value ?? "";

                if (type != "")
                {
                    // type claim is currently encrypted so we need to decrypt it with jwtservice
                    var jwtService = context.HttpContext.RequestServices.GetService<IJwtService>() ?? throw new NullReferenceException();

                    string decryptedType = jwtService.DecryptClaim(type);

                    // Check if the type claim is one of the allowed types
                    if (type == null || !_typeArray.Contains(decryptedType))
                    {
                        context.Result = new ForbidResult();
                    }
                }
            }
        }


    }
}
