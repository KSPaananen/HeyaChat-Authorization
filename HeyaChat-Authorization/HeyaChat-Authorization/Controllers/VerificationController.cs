using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO;
using HeyaChat_Authorization.Models;
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

        // Returns
        // 200: Verified succesfully    // 404: Incorrect code      500: Problems with the database
        [HttpPost, Authorize]
        [TokenTypeAuthorize("login")]
        [Route("VerifyEmail")]
        public IActionResult VerifyEmail(VerifyDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Query database if code is valid and associated with the user
            Codes validCode = _codesRepository.GetValidCodeWithUserIdAndCode(userId, dro.Code);

            if (validCode.CodeId <= 0)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseDetails { Code = 1330, Details = "Code expired or doesn't belong to user." });
            }

            // Set code as used and update to database
            validCode.Used = true;

            long affectedRow = _codesRepository.UpdateCode(validCode);

            // Get user details and update email verified column to true
            UserDetail details = _userDetailsRepository.GetUserDetailsByUserId(userId);

            details.EmailVerified = true;

            _userDetailsRepository.UpdateUserDetails(details);

            return StatusCode(StatusCodes.Status200OK, new ResponseDetails { Code = 1370, Details = "Code is valid and email has been updated as verified." });
        }

        // Returns
        // 200: Verified succesfully    404: Incorrect code     500: Problems with the database
        [HttpPost, Authorize]
        [Route("VerifyCode")]
        public IActionResult VerifyCode(VerifyDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Check if code is valid
            Codes validCode = _codesRepository.GetValidCodeWithUserIdAndCode(userId, dro.Code);

            // CodeId being 0 indicates code wasn't valid
            if (validCode.CodeId <= 0)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseDetails { Code = 1130, Details = "Code expired or doesn't belong to user." });
            }

            // Set code as used and update to database
            validCode.Used = true;

            // Get full device details with dro details
            Device foundDevice = _devicesRepository.GetDeviceWithUUID(dro.Device.DeviceIdentifier);

            // Add password type token to user. Type is important for restricting access to only certain methods
            Request.Headers.Authorization = _jwtService.GenerateToken(userId, foundDevice.DeviceId, "temporary");

            long affectedRow = _codesRepository.UpdateCode(validCode);

            // If Code was correct, return 200 to proceed to the next step
            return StatusCode(StatusCodes.Status200OK, new ResponseDetails { Code = 1170, Details = "Code is valid." });
        }

        // Returns
        // 200: Verified succesfully    404: Incorrect code
        [HttpPost]                             
        [Route("VerifyMFA")]              
        public IActionResult VerifyMFA(VerifyDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Check if code is valid
            Codes validCode = _codesRepository.GetValidCodeWithUserIdAndCode(userId, dro.Code);

            if (validCode.CodeId <= 0)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseDetails { Code = 1430, Details = "Code expired or doesn't belong to user." });
            }

            // Set code as used and update to database
            validCode.Used = true;

            long affectedRow = _codesRepository.UpdateCode(validCode);

            // Invalidate tokens for other devices to enforce only one device online policy
            _jwtService.InvalidateAllTokens(userId);

            // Get device from database
            // Users current device was already added to the database in the login method
            var device = _devicesRepository.GetDeviceWithUUID(dro.Device.DeviceIdentifier);

            // Generate token and add it to the authorization header
            Response.Headers.Authorization = _jwtService.GenerateToken(userId, device.DeviceId, "login");

            return StatusCode(StatusCodes.Status200OK, new ResponseDetails { Code = 1470, Details = "Code is valid." });
        }


    }
}
