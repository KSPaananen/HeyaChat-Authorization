using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]                      // Returns
        [Route("Register")]             // 201: New user created    304: New user not created   500: Problems with the database
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
            _emailService.SendVerificationEmail(userId, dro.Email);

            // Generate JWT with type "login". This method automatically adds token to DB
            var token = _jwtService.GenerateToken(userId, deviceId, "login");

            // Add token to Response header under Authorization
            Response.Headers.Authorization = token;

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]                      // Returns
        [Route("Login")]                // 200: Login succesful     202: Login succesful, but email isn't verified      401: Login unsuccesful
        public IActionResult Login(LoginDRO dro)
        {
            User userObj = _usersRepository.GetUserByUsernameOrEmail(dro.Login);

            if (userObj.UserId <= 0)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            // Hash the password from dro with salt from user object and see if they match
            byte[] salt = userObj.PasswordSalt;

            var droHashedPassword = _hasherService.Hash(dro.Password, salt);

            // Check login mathces either username or email and passwordhash
            if (droHashedPassword == userObj.PasswordHash && dro.Login == userObj.Username || dro.Login == userObj.Email)
            {
                // After succesful login invalid tokens for other devices to enforce only one device online policy
                _jwtService.InvalidateAllTokens(userObj.UserId);

                // Add users current device to database if it doesn't exist there or get DeviceId of already saved device
                Device device = new Device
                {
                    DeviceName = dro.Device.DeviceName,
                    DeviceIdentifier = dro.Device.DeviceIdentifier,
                    CountryTag = dro.Device.CountryTag,
                    // UsedAt is handled by the database
                    UserId = userObj.UserId
                };

                // Insert new device to db or update already existing
                long deviceId = _devicesRepository.InsertOrUpdateDevice(device);

                // Generate a token for current device and add it to authorization header
                string token = _jwtService.GenerateToken(userObj.UserId, deviceId, "login");

                Response.Headers.Authorization = token;

                // Send statuscode according to email verification status
                UserDetail userDetails = _userDetailsRepository.GetUserDetailsByUserId(userObj.UserId);

                if (userDetails.EmailVerified)
                {
                    return StatusCode(StatusCodes.Status200OK);
                }
                else
                {
                    return StatusCode(StatusCodes.Status202Accepted);
                }
            }

            return StatusCode(StatusCodes.Status401Unauthorized);
        }

        [HttpPost]
        [TokenTypeAuthorize("login")]   // Returns
        [Route("PingBackend")]          // 200: User still logged in
        public IActionResult PingBackend()
        {
            // All token related verifying is handled at middleware so just return 200

            return StatusCode(StatusCodes.Status200OK);
        }


    }
}
