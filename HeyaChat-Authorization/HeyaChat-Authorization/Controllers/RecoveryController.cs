using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecoveryController : ControllerBase
    {
        private IMessageService _messageService;
        private IJwtService _jwtService;
        private IHasherService _hasherService;

        private IUsersRepository _usersRepository;
        private IDevicesRepository _devicesRepository;
        private IAuditLogsRepository _auditLogsRepository;

        public RecoveryController(IUsersRepository usersRepository, IDevicesRepository devicesRepository, IMessageService messageService, 
            IJwtService jwtService, IHasherService hasherService, IAuditLogsRepository auditLogsRepository)
        {
            _messageService = messageService ?? throw new NullReferenceException(nameof(messageService));
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));
            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
            _auditLogsRepository = auditLogsRepository ?? throw new NullReferenceException(nameof(auditLogsRepository));
        }

        [HttpPost]                          // Returns
        [Route("PasswordRecovery")]         // 201: Email sent   404: Requested email not found
        public IActionResult PasswordRecovery(RecoveryDRO dro)
        {
            // Try finding requested email
            User user = _usersRepository.GetUserByUsernameOrEmail(dro.email);

            if (user.UserId <= 0)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            // In the future we need logic for handling password changing requests from new devices & countries to prevent account theft

            // Add users current device to database if it doesn't exist there or get DeviceId of already saved device
            Device device = new Device
            {
                DeviceName = dro.Device.DeviceName,
                DeviceIdentifier = dro.Device.DeviceIdentifier,
                CountryTag = dro.Device.CountryTag,
                // UsedAt is handled by the database
                UserId = user.UserId
            };

            var deviceResults= _devicesRepository.InsertDeviceIfDoesntExist(device);

            // Add password type token to user. Type is important for restricting access to only password change related methods
            Request.Headers.Authorization = _jwtService.GenerateToken(user.UserId, deviceResults.deviceId, "password");

            // Send a code to requested email
            _messageService.SendRecoveryEmail(user.UserId, user.Email);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost, Authorize]
        [TokenTypeAuthorize("password")]    // Returns
        [Route("PasswordChanging")]         // 201: Password changed
        public IActionResult PasswordChanging(PasswordChangeDRO dro)
        {
            // Get userId from token
            long userId = _jwtService.GetClaims(Request).userId;

            // Generate new salt and passwordhash
            byte[] salt = _hasherService.GenerateSalt();
            string passwordHash = _hasherService.Hash(dro.Password, salt);

            // Update users passwordhash and salt
            long updatedUserId = _usersRepository.UpdateUsersPasswordAndSalt(userId, passwordHash, salt);

            // Get user device for audit logging
            Device device = _devicesRepository.GetDeviceWithUUID(dro.Device.DeviceIdentifier);

            // Audit log event
            long auditLogId = _auditLogsRepository.InsertAuditLog(device.DeviceId, 1);

            // User has to log in after password changing, so don't generate token here

            return StatusCode(StatusCodes.Status201Created);
        }


    }
}
