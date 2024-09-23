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
        private IMessageService _messageService;
        private IJwtService _jwtService;

        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;
        private ISuspensionsRepository _suspensionsRepository;
        private IAuditLogsRepository _auditLogsRepository;
        

        public AuthorizationController(IUsersRepository usersRepository, IUserDetailsRepository userDetailsRepository, IDevicesRepository devicesRepository, 
            IHasherService hasherService, IMessageService messageService, IJwtService jwtService, ISuspensionsRepository suspensionsRepository, IAuditLogsRepository auditLogsRepository)
        {
            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));
            _messageService = messageService ?? throw new NullReferenceException(nameof(messageService));
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new NullReferenceException(nameof(userDetailsRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
            _suspensionsRepository = suspensionsRepository ?? throw new NullReferenceException(nameof(suspensionsRepository));
            _auditLogsRepository = auditLogsRepository ?? throw new NullReferenceException(nameof(auditLogsRepository));
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

            // Generate JWT with type "login". This method automatically adds token to DB
            var token = _jwtService.GenerateToken(userId, deviceId, "login");

            // Add token to Response header under Authorization
            Response.Headers.Authorization = token;

            // Send verification email to verify user. This method automatically saves code to database
            // Send email after everything else incase theres problems with the email sending
            _messageService.SendVerificationEmail(userId, dro.Email);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost]                      // 200: Login succesful        202: MFA verification required     206: Login succesful, but email isn't verified
        [Route("Login")]                // 401: Login unsuccesful      403: User suspended
        public IActionResult Login(LoginDRO dro)
        {
            User user;

            // Perform login based on the type of login. Credentials or biometrics
            if (dro.Login == "" && dro.Password == "")
            {
                bool isValid = _usersRepository.IsBiometricsLoginValid(dro.BiometricsKey);

                if (!isValid)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
            }
            else
            {
                user = _usersRepository.GetUserByUsernameOrEmail(dro.Login);

                // Hash the password from dro with salt from the user object and see if they match
                var requestHashedPassword = _hasherService.Hash(dro.Password, user.PasswordSalt);

                if (requestHashedPassword != user.PasswordHash)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
            }

            // Check if user is currently suspended
            bool isSuspended = _suspensionsRepository.IsCurrentlySuspended(user.UserId);

            if (isSuspended)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            // Read userdetails to define login flow
            var userDetails = _userDetailsRepository.GetUserDetailsByUserId(user.UserId);

            // See if current device already exists in the database
            Device device = new Device
            {
                DeviceName = dro.Device.DeviceName,
                DeviceIdentifier = dro.Device.DeviceIdentifier,
                CountryTag = dro.Device.CountryTag,
                // UsedAt is handled by the database
                UserId = user.UserId
            };

            var deviceResults = _devicesRepository.InsertDeviceIfDoesntExist(device);

            // Add to audit logs if user is logging in from a new device
            if (deviceResults.alreadyExisted == false)
            {
                long auditLogId = _auditLogsRepository.InsertAuditLog(device.DeviceId, 0);
            }

            // - Act on multifactorauth if user logs in for the first time on a device
            // - Users with unverified emails should be sent a verification email until they verify it

            // MFA enabled and logging in from a new device
            if (deviceResults.alreadyExisted == false && userDetails.MfaEnabled)
            {
                // Check which type of mfa to use
                if (userDetails.PhoneVerified)
                {
                    // Send code as a text message
                    _messageService.SendVerificationTextMessage(user.UserId, user.Email);
                }
                else
                {
                    // Send code as an email
                    _messageService.SendVerificationEmail(user.UserId, user.Email);
                }

                // Generate token after mfa code verification

                return StatusCode(StatusCodes.Status202Accepted);
            }

            // Invalidate tokens for other devices to enforce only one device online policy
            _jwtService.InvalidateAllTokens(user.UserId);

            // Generate a new token and add it to the authorization header. GenerateToken() method automatically adds token to database
            Response.Headers.Authorization = _jwtService.GenerateToken(user.UserId, deviceResults.deviceId, "login");

            if (userDetails.EmailVerified)
            {
                return StatusCode(StatusCodes.Status200OK);
            }
            else
            {
                // Send email with verification code
                _messageService.SendVerificationEmail(user.UserId, user.Email);

                // Return 206 to notify frontend of the required extra steps
                return StatusCode(StatusCodes.Status206PartialContent);
            }
        }

        [HttpPost]
        [TokenTypeAuthorize("login")]   // Returns
        [Route("LogOut")]               // 200: Logged out
        public IActionResult LogOut()
        {
            // Get token indetifier from authorization header
            Guid jti = _jwtService.GetClaims(Request).jti;

            // Set token identifiers "active" property to false
            var awda = _jwtService.InvalidateToken(jti);

            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpPost]
        [TokenTypeAuthorize("login")]   // Returns
        [Route("PingBackend")]          // 200: User still logged in
        public IActionResult PingBackend()
        {
            // All token related verifying and renewing is handled at middleware so just return 200

            return StatusCode(StatusCodes.Status200OK);
        }


    }
}
