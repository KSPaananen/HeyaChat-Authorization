using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
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

        public VerificationController(IJwtService jwtService, ICodesRepository codesRepository, IUserDetailsRepository userDetailsRepository)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));

            _codesRepository = codesRepository ?? throw new ArgumentNullException(nameof(codesRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new ArgumentNullException(nameof(userDetailsRepository));
        }

        [HttpPost, Authorize]   
        [TokenTypeAuthorize("login")]   // Returns
        [Route("VerifyEmail")]          // 200: Verification succesful      304: Incorrect code     500: Problems with the database
        public IActionResult VerifyEmail(VerifyDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).Item2;

            // Query database if code is valid and associated with the user
            Codes result = _codesRepository.IsCodeValid(userId, dro.Code);

            // End execution if code isn't valid
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


    }
}
