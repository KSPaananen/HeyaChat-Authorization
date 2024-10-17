using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IJwtService _jwtService;
        private IHasherService _hasherService;

        private IUsersRepository _usersRepository;
        private IDevicesRepository _devicesRepository;
        private IAuditLogsRepository _auditLogsRepository;
        private IDeleteRequestsRepository _deleteRequestsRepository;

        public AccountController(IUsersRepository usersRepository, IDevicesRepository devicesRepository, IAuditLogsRepository auditLogsRepository,
            IJwtService jwtService, IHasherService hasherService, IDeleteRequestsRepository deleteRequestsRepository)
        {
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));
            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
            _auditLogsRepository = auditLogsRepository ?? throw new NullReferenceException(nameof(auditLogsRepository));
            _deleteRequestsRepository = deleteRequestsRepository ?? throw new NullReferenceException(nameof(deleteRequestsRepository));
        }

        //Returns
        //
        [HttpGet, Authorize]
        [TokenTypeAuthorize("login")]
        [Route("GetAccountData")]
        public IActionResult GetAccountData()
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // Returns
        //
        [HttpPost, Authorize]
        [TokenTypeAuthorize("login")]
        [Route("ChangeUsername")]
        public IActionResult ChangeUsername()
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // Returns
        //
        [HttpPost, Authorize]
        [TokenTypeAuthorize("login")]
        [Route("ChangeEmail")]
        public IActionResult ChangeEmail()
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // Returns
        //
        [HttpPost, Authorize]
        [TokenTypeAuthorize("login")]
        [Route("ChangeBiometricsKey")]
        public IActionResult ChangeBiometricsKey(AddBiometricsKey dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Read user from db with userid
            User user = _usersRepository.GetUserByUserID(userId);

            // Assign new biometricskey to user and save to db
            user.BiometricsKey = dro.BiometricsKey;

            _usersRepository.UpdateUser(user);

            // We probably don't have to audit log this?

            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        // Returns
        // 201: Password changed    304: Passwords didn't match   500: Problems saving changes to database
        [HttpPost, Authorize]
        [TokenTypeAuthorize("temporary", "login")]
        [Route("ChangePassword")]
        public IActionResult ChangePassword(PasswordChangeDRO dro)
        {
            // Check if passwords match
            if (dro.Password != dro.PasswordRepeat)
            {
                return StatusCode(StatusCodes.Status304NotModified, new DetailsDTO { Code = 1830, Details = "Passwords didn't match." });
            }

            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Generate new salt and passwordhash
            byte[] salt = _hasherService.GenerateSalt();
            string passwordHash = _hasherService.Hash(dro.Password, salt);

            // Get user from database and update passwordHash and salt
            User foundUser = _usersRepository.GetUserByUserID(userId);

            foundUser.PasswordHash = passwordHash;
            foundUser.PasswordSalt = salt;

            _usersRepository.UpdateUser(foundUser);

            // Get user device for audit logging
            Device device = _devicesRepository.GetDeviceWithUUID(dro.Device.DeviceIdentifier);

            // Audit log event
            long auditLogId = _auditLogsRepository.InsertAuditLog(device.DeviceId, 1);

            // User has to log in after password changing, so don't generate token here

            return StatusCode(StatusCodes.Status201Created, new DetailsDTO { Code = 1870, Details = "Password changed." });
        }

        // Returns
        // 201: DeleteRequest created and added to database     403: User already has an active delete request     500: Problems saving changes to database
        [HttpDelete, Authorize]
        [TokenTypeAuthorize("login, suspended")]
        [Route("RequestDelete")]
        public IActionResult RequestDelete(UserDevice device)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Try finding pre-existing delete requests. We'd like to avoid multiple delete requests for one account
            var result = _deleteRequestsRepository.GetDeleteRequestByUserId(userId);

            if (result.DeleteId != 0)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new DetailsDTO { Code = 1930, Details = "User already has an active delete request." });
            }

            // Create deleterequest object and add it to database
            DeleteRequest request = new DeleteRequest
            {
                UserId = userId,
                // DateRequested is automatically added by the database
                Fulfilled = false
            };

            _deleteRequestsRepository.InsertDeleteRequest(request);

            return StatusCode(StatusCodes.Status201Created, new DetailsDTO { Code = 1970, Details = "Deletion requested. Account will be deleted in 60 days." });
        }

        // Returns
        // 201: DeleteRequest deleted    500: Problems saving changes to database
        [HttpDelete, Authorize]
        [TokenTypeAuthorize("login, suspended")]
        [Route("UndoRequestDelete")]
        public IActionResult UndoRequestDelete(UserDevice device)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Remove deleterequest from db
            _deleteRequestsRepository.DeleteDeleteRequest(userId);

            return StatusCode(StatusCodes.Status201Created, new DetailsDTO { Code = 2070, Details = "Delete request removed." });
        }


    }
}
