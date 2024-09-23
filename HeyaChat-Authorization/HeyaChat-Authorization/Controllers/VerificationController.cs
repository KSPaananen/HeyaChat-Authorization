using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories;
using HeyaChat_Authorization.Repositories.Interfaces;
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

        private ICodesRepository _codesRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;

        public VerificationController(IJwtService jwtService, ICodesRepository codesRepository, IUserDetailsRepository userDetailsRepository, IDevicesRepository devicesRepository)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));

            _codesRepository = codesRepository ?? throw new ArgumentNullException(nameof(codesRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new ArgumentNullException(nameof(userDetailsRepository));
            _devicesRepository = devicesRepository ?? throw new ArgumentNullException(nameof(devicesRepository));
        }

        [HttpPost, Authorize]
        [TokenTypeAuthorize("login")]       // Returns
        [Route("EmailVerification")]        // 200: Verification succesful      304: Incorrect code     500: Problems with the database
        public IActionResult EmailVerification(VerifyDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Query database if code is valid and associated with the user
            Codes result = _codesRepository.IsCodeValid(userId, dro.Code);

            if (result.CodeId == 0)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // Update email verified column in userDetails
            long rowId = _userDetailsRepository.UpdateEmailVerified(userId);

            // rowId being 0 indicates a problem with updating the row
            if (rowId > 0)
            {
                return StatusCode(StatusCodes.Status201Created);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost, Authorize]
        [TokenTypeAuthorize("password, login")] // Returns
        [Route("CodeVerification")]             // 200: Verification succesful   304: Incorrect code
        public IActionResult CodeVerification(VerifyDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Check if code is valid
            Codes result = _codesRepository.IsCodeValid(userId, dro.Code);

            // CodeId being 0 indicates code wasn't valid
            if (result.CodeId <= 0)
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // If Code was correct, return 200 to proceed to the next step
            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPost]                              // Returns
        [Route("MFAVerification")]              // 401: Incorrect code
        public IActionResult MFAVerification(VerifyDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Check if code is valid
            Codes result = _codesRepository.IsCodeValid(userId, dro.Code);

            if (result.CodeId <= 0)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            // Invalidate tokens for other devices to enforce only one device online policy
            _jwtService.InvalidateAllTokens(userId);

            // Get device from database
            // Users current device was already added to the database in the login method
            var device = _devicesRepository.GetDeviceWithUUID(dro.Device.DeviceIdentifier);

            // Generate token and add it to the authorization header
            Response.Headers.Authorization = _jwtService.GenerateToken(userId, device.DeviceId, "login");

            return StatusCode(StatusCodes.Status200OK);
        }


    }
}
