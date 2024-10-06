using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DTO;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
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
        private IToolsService _toolsService;

        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;

        public RecoveryController(IUsersRepository usersRepository, IUserDetailsRepository userDetailsRepository, IDevicesRepository devicesRepository, IMessageService messageService, 
            IJwtService jwtService, IAuditLogsRepository auditLogsRepository, IToolsService toolsService)
        {
            _messageService = messageService ?? throw new NullReferenceException(nameof(messageService));
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new NullReferenceException(nameof(userDetailsRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
            _toolsService = toolsService ?? throw new NullReferenceException(nameof(toolsService));
        }

        // Returns
        // 200: Email sent to user      // 404: Requested email not tied to an account
        [HttpPost]
        [Route("Recover")]
        public IActionResult Recover(RecoveryDRO dro)
        {
            // Try finding requested email
            User user = _usersRepository.GetUserByUsernameOrEmail(dro.Email);

            if (user.UserId <= 0)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ContactDTO { Contact = "", Details = new ResponseDetails { Code = 1030, Details = "User matching requested login couldn't be found." } });
            }

            // In the future we need logic for handling password changing requests from new devices & countries to prevent account theft

            // Add users current device to database if it doesn't exist there or get DeviceId of already saved device
            Device device = new Device
            {
                DeviceName = dro.Device.DeviceName,
                DeviceIdentifier = dro.Device.DeviceIdentifier,
                CountryTag = dro.Device.CountryCode,
                // UsedAt is handled by the database
                UserId = user.UserId
            };

            var deviceResults= _devicesRepository.InsertDeviceIfDoesntExist(device);

            // Read userdetails and send recovery code based on verification status of email and phone number
            var userDetails = _userDetailsRepository.GetUserDetailsByUserId(user.UserId);

            string contact = "";
            int code = 0;

            if (user.Phone != null && userDetails.EmailVerified && userDetails.PhoneVerified) // Prioritize phone verifying over email
            {
                // Mask users phone number to not entirely leak it
                contact = _toolsService.MaskPhoneNumber(user.Phone);
                code = 1071;

                _messageService.SendVerificationTextMessage(user.UserId, user.Phone);
            }
            else if (userDetails.EmailVerified) // Send code to email if phone isn't verified
            {
                // No need to mask users email since thats what they used to request
                contact = user.Email;
                code = 1070;

                _messageService.SendRecoveryEmail(user.UserId, user.Email);
            }

            return StatusCode(StatusCodes.Status200OK, new ContactDTO { Contact = contact, Details = new ResponseDetails { Code = code, Details = "Verification code sent." } });
        }


    }
}
