using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Devices.Interfaces;
using HeyaChat_Authorization.Repositories.UserDetails.Interfaces;
using HeyaChat_Authorization.Repositories.Users.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

namespace HeyaChat_Authorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private IConfiguration _config;
        private AuthorizationDBContext _context;

        private IHasherService _hasherService;
        private IEmailService _emailService;

        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;
        

        public AuthorizationController(IConfiguration config, AuthorizationDBContext context, IUsersRepository usersRepository, 
            IUserDetailsRepository userDetailsRepository, IDevicesRepository devicesRepository,IHasherService hasherService,
            IEmailService emailService)
        {
            _config = config ?? throw new NullReferenceException(nameof(config));
            _context = context ?? throw new NullReferenceException(nameof(context));

            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));
            _emailService = emailService ?? throw new NullReferenceException(nameof(emailService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new NullReferenceException(nameof(userDetailsRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
        }

        private Regex usernameRgx = new Regex(@"^[a-zA-Z0-9_-]{3,20}$");
        private Regex emailRgx = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        private Regex passwordRgx = new Regex(@"^.{8,}$");

        [HttpPost]              // Returns
        [Route("Register")]     // 201: New user created    304: New user not created   500: Problems with the database
        public IActionResult Register(RegisterDRO reDRO)
        {
            // Stop execution if username or email are found in database or RegisterDRO fails regex check
            if (_usersRepository.DoesUserExist(reDRO.Username, reDRO.Email) || !usernameRgx.IsMatch(reDRO.Username) && !emailRgx.IsMatch(reDRO.Email) && !passwordRgx.IsMatch(reDRO.Password))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // Generate password salt and hash the password
            byte[] salt = _hasherService.GenerateSalt();
            string hashedPassword = _hasherService.Hash(reDRO.Password, salt);

            // Create new user object and insert it to the DB
            User newUser = new User
            {
                Username = reDRO.Username,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                BiometricsKey = null,
                Email = reDRO.Email,
            };

            long userId = _usersRepository.InsertUser(newUser);

            // Create a new userDetails object and insert it to the DB
            UserDetail details = new UserDetail
            {
                UserId = userId,
                EmailVerified = false,
                PhoneVerified = false,
                MfaEnabled = false,
                // CreatedAt is handled by the database
                // UpdatedAt is handled by the database
            };

            long userDetailsId = _userDetailsRepository.InsertUserDetailsToTable(details);

            // Create new device object and insert it to the DB
            Device device = new Device
            {
                DeviceName = reDRO.DeviceName,
                DeviceIdentifier = reDRO.DeviceIdentifier,
                CountryTag = reDRO.CountryTag,
                // UsedAt is handled by the database
                UserId = userId
            };

            long deviceId = _devicesRepository.InsertDeviceToTable(device);

            // Stop execution and clear any previously created rows if any of the Id's are 0. This indicates a problem with inserting rows and/or database.
            if (userId <= 0 && userDetailsId <= 0 && deviceId <= 0)
            {
                // Delete all previously inserted rows since there were problems with insertion.
                _usersRepository.DeleteUser(userId);
                _userDetailsRepository.DeleteUserDetails(userDetailsId);
                _devicesRepository.DeleteDevice(deviceId);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // Send verification email to verify users email address
            _emailService.SendVerificationEmail(userId, reDRO.Email);

            // Don't generate a JWT here yet. We'll give it to the user after they've verified their email.

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login()
        {
            // If user logs in succesfully, but their email isnt verified, prompt them to verify email and only then give them a jwt

            _emailService.SendVerificationEmail("awdad");


            return StatusCode(StatusCodes.Status401Unauthorized);
        }



    }
}
