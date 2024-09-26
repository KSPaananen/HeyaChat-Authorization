using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services;
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

        public AccountController(IUsersRepository usersRepository, IDevicesRepository devicesRepository, IAuditLogsRepository auditLogsRepository,
            IJwtService jwtService, IHasherService hasherService)
        {
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));
            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
            _auditLogsRepository = auditLogsRepository ?? throw new NullReferenceException(nameof(auditLogsRepository));
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
        [TokenTypeAuthorize("password")]
        [Route("ChangePassword")]
        public IActionResult ChangePassword(PasswordChangeDRO dro)
        {
            // Check if passwords match
            if (dro.Password != dro.PasswordRepeat)
            {
                return StatusCode(StatusCodes.Status304NotModified);
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

            return StatusCode(StatusCodes.Status201Created);
        }

        // Returns
        //
        [HttpDelete, Authorize]
        [TokenTypeAuthorize("login, suspended")]
        [Route("DeleteAccount")]
        public IActionResult DeleteAccount()
        {
            // Create delete request and wait 60 days

            return StatusCode(StatusCodes.Status501NotImplemented);
        }


    }
}
