using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
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
        private IHasherService _hasherService;
        private IEmailService _emailService;
        private IJwtService _jwtService;

        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;
        

        public AuthorizationController(IUsersRepository usersRepository, IUserDetailsRepository userDetailsRepository, IDevicesRepository devicesRepository, 
            IHasherService hasherService, IEmailService emailService, IJwtService jwtService)
        {
            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));
            _emailService = emailService ?? throw new NullReferenceException(nameof(emailService));
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new NullReferenceException(nameof(userDetailsRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
        }

        private Regex usernameRgx = new Regex(@"^[a-zA-Z0-9_-]{3,20}$");
        private Regex emailRgx = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        private Regex passwordRgx = new Regex(@"^.{8,}$");

        [HttpPost]              // Returns
        [Route("Register")]     // 201: New user created    304: New user not created   500: Problems with the database
        public IActionResult Register(RegisterDRO dro)
        {
            // Stop execution if username or email are found in database or RegisterDRO fails regex check
            if (_usersRepository.DoesUserExist(dro.Username, dro.Email) || !usernameRgx.IsMatch(dro.Username) && !emailRgx.IsMatch(dro.Email) && !passwordRgx.IsMatch(dro.Password))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // Generate password salt and hash the password
            byte[] salt = _hasherService.GenerateSalt();
            string hashedPassword = _hasherService.Hash(dro.Password, salt);

            // Create new user object and insert it to the DB
            User newUser = new User
            {
                Username = dro.Username,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                BiometricsKey = null,
                Email = dro.Email,
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

            long userDetailsId = _userDetailsRepository.InsertUserDetails(details);

            // Create new device object and insert it to the DB
            Device device = new Device
            {
                DeviceName = dro.Device.DeviceName,
                DeviceIdentifier = dro.Device.DeviceIdentifier,
                CountryTag = dro.Device.CountryTag,
                // UsedAt is handled by the database
                UserId = userId
            };

            long deviceId = _devicesRepository.InsertDevice(device);

            // Any of the Id's being 0 indicates a problem with the database, so to prevent "stuck" emails & usernames delete just created rows.
            if (userId <= 0 && userDetailsId <= 0 && deviceId <= 0)
            {
                // Delete recently added user. Delete cascades to other tables.
                _usersRepository.DeleteUser(userId);

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // Send verification email to verify users email. This method automatically saves code to database
            //_emailService.SendVerificationEmail(userId, dro.Email);

            // Generate JWT with type "login". This method automatically adds token to DB
            var token = _jwtService.GenerateToken(userId, deviceId, "login");

            // Add token to Response header under Authorization
            Response.Headers.Authorization = token;

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login()
        {

            // If user logs in succesfully, but their email isnt verified, prompt them to verify email and only then give them a jwt

            // When user logs in, check if any other devices have active tokens and disable them if more than 1 are found
            

            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        [HttpPost]
        [Route("PingBackend")]
        public IActionResult PingBackend()
        {
            // Jwt and jti verification is handled at middleware, so we can just return 200 OK if request reaches this method.

            return StatusCode(StatusCodes.Status200OK);
        }



    }
}
