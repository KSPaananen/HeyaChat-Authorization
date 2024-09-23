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

        private IUsersRepository _usersRepository;
        private IDevicesRepository _devicesRepository;

        public RecoveryController(IUsersRepository usersRepository, IDevicesRepository devicesRepository, IMessageService messageService, 
            IJwtService jwtService, IAuditLogsRepository auditLogsRepository)
        {
            _messageService = messageService ?? throw new NullReferenceException(nameof(messageService));
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
        }

        // Returns
        // 201: Email sent to user      // 404: Requested email not tied to an account
        [HttpPost]
        [Route("RecoverPassword")]
        public IActionResult RecoverPassword(RecoveryDRO dro)
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


    }
}
