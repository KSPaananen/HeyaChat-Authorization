using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private IJwtService _jwtService;

        public VerificationController(IJwtService jwtService)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        [HttpPost, Authorize]   // Returns
        [Route("VerifyEmail")]  // 200: Verification succesful    400: Bad token
        public IActionResult VerifyEmail(VerifyEmailDRO dro)
        {
            // Get claims from Authorization header
            List<string> claims = _jwtService.GetClaims(Request);

            Guid jti = Guid.Parse(claims[0]);
            long userId = long.Parse(claims[1]);
            string type = claims[2];

            // Check if Authorization headers token is valid
            bool isValid = _jwtService.VerifyToken(jti, dro.Device);
            
            if (isValid == false)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            // Try finding 



            



            return StatusCode(StatusCodes.Status200OK);
        }


    }
}
