using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO;
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
        private IBlockedCredentialsRepository _blockedCredentialsRepository;
        
        public AuthorizationController(IUsersRepository usersRepository, IUserDetailsRepository userDetailsRepository, IDevicesRepository devicesRepository, 
            IHasherService hasherService, IMessageService messageService, IJwtService jwtService, ISuspensionsRepository suspensionsRepository, IAuditLogsRepository auditLogsRepository,
            IBlockedCredentialsRepository blockedCredentialsRepository)
        {
            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));
            _messageService = messageService ?? throw new NullReferenceException(nameof(messageService));
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new NullReferenceException(nameof(userDetailsRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
            _suspensionsRepository = suspensionsRepository ?? throw new NullReferenceException(nameof(suspensionsRepository));
            _auditLogsRepository = auditLogsRepository ?? throw new NullReferenceException(nameof(auditLogsRepository));
            _blockedCredentialsRepository = blockedCredentialsRepository ?? throw new NullReferenceException(nameof(blockedCredentialsRepository));
        }

        // AuthorizationDTO codes
        // 310: Username already in use
        // 311: Email address already in use
        // 312: Both username and email address already in use
        // 313: Request didn't pass regex check
        // 314: Email blocked from creating new accounts
        // 315: New user registered
        // 410: User couldn't be found
        // 411: User temporarily suspended
        // 412: User permanently suspended
        // 420: User logged in, but additional confirmation required (mfa)
        // 421: User logged in.
        // 422: User logged in. Email confirmation required.
        // 510: Token not found or didn't belong to user
        // 511: Token invalidated and user logged out.
        // 610: Token valid. User still logged in

        private Regex usernameRgx = new Regex(@"^[a-zA-Z0-9_-]{3,20}$");
        private Regex emailRgx = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        private Regex passwordRgx = new Regex(@"^.{8,}$");

        // Returns
        // 201: New user registered     302: Username or email already in use or blocked       // 406: RegisterDRO did not pass regex check
        // 500: Internal server error
        [HttpPost]
        [Route("Register")]
        public IActionResult Register(RegisterDRO dro)
        {
            // Make sure username and email aren't already in use
            var existResults = _usersRepository.UsernameOrEmailInUse(dro.Username, dro.Email);

            if (existResults.usernameInUse && existResults.emailInUse)
            {
                return StatusCode(StatusCodes.Status302Found, new AuthorizationDTO { Status = "Failure", Code = 312, Details = "Username and email address already in use by another account." });
            }
            else if (existResults.usernameInUse)
            {
                return StatusCode(StatusCodes.Status302Found, new AuthorizationDTO { Status = "Failure", Code = 310, Details = "Username already in use by another account." });
            }
            else if (existResults.emailInUse)
            {
                return StatusCode(StatusCodes.Status302Found, new AuthorizationDTO { Status = "Failure", Code = 311, Details = "Email address already in use by another account." });
            }

            // Make sure dro passes regex check
            if (!usernameRgx.IsMatch(dro.Username) && !emailRgx.IsMatch(dro.Email) && !passwordRgx.IsMatch(dro.Password))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, new AuthorizationDTO { Status = "Failure", Code = 313, Details = "Request didn't pass regex check." });
            }

            bool isBlocked = _blockedCredentialsRepository.IsCredentialBlocked(dro.Email);

            // Also make sure requested email isn't blocked from creating new accounts
            if (isBlocked)
            {
                return StatusCode(StatusCodes.Status302Found, new AuthorizationDTO { Status = "Failure", Code = 314, Details = "Email address blocked from creating new accounts." });
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
                CountryTag = dro.Device.CountryCode,
                // UsedAt is handled by the database
                UserId = userId
            };

            long deviceId = _devicesRepository.InsertDevice(device);

            // Generate JWT with type "login". This method automatically adds token to DB
            var token = _jwtService.GenerateToken(userId, deviceId, "login");

            // Add token to Response header under Authorization
            Response.Headers.Authorization = token;

            // Send verification email to verify user. This method automatically saves code to database
            // Send email after everything else incase theres problems with the email sending
            _messageService.SendVerificationEmail(userId, dro.Email);

            return StatusCode(StatusCodes.Status201Created, new AuthorizationDTO { Status = "Success", Code = 315, Details = "New user succesfully registered." });
        }

        // Returns
        // 200: Login succesful     202: MFA verification required      401: Login unsuccesful    403: User suspended
        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginDRO dro)
        {
            User user = _usersRepository.GetUserByLoginDetails(dro.Login, dro.BiometricsKey);

            // UserId will be 0 if user with details cannot be found
            if (user.UserId <= 0)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new AuthorizationDTO { Status = "Failure", Code = 410, Details = "User couldn't be found." });
            }

            // Hash the password from dro with salt from the user object and see if they match
            var droPasswordHash = _hasherService.Hash(dro.Password, user.PasswordSalt);

            // Check that login details match
            if (droPasswordHash != user.PasswordHash || (dro.BiometricsKey != null && dro.BiometricsKey != user.BiometricsKey))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            // See if current device already exists in the database
            Device device = new Device
            {
                DeviceName = dro.Device.DeviceName,
                DeviceIdentifier = dro.Device.DeviceIdentifier,
                CountryTag = dro.Device.CountryCode,
                // UsedAt is handled by the database
                UserId = user.UserId
            };

            var deviceResults = _devicesRepository.InsertDeviceIfDoesntExist(device);

            // Add to audit logs if user is logging in from a new device
            if (deviceResults.alreadyExisted == false)
            {
                long auditLogId = _auditLogsRepository.InsertAuditLog(device.DeviceId, 0);
            }

            // Check if user is currently suspended
            var susResults = _suspensionsRepository.IsCurrentlySuspended(user.UserId);

            if (susResults.suspended)
            {
                if (susResults.permanent)
                {
                    Response.Headers.Authorization = _jwtService.GenerateToken(user.UserId, deviceResults.deviceId, "suspended");

                    return StatusCode(StatusCodes.Status403Forbidden, new AuthorizationDTO { Status = "Failure", Code = 412, Details = "User is permanently suspended." });
                }

                return StatusCode(StatusCodes.Status403Forbidden, new AuthorizationDTO { Status = "Failure", Code = 411, Details = "User is temporarily suspended." });
            }

            // Read userdetails to define login flow
            var userDetails = _userDetailsRepository.GetUserDetailsByUserId(user.UserId);

            // --- Login plan ---
            // - Act on multifactorauth if user logs in for the first time on a device
            // - Users with unverified emails should be sent a verification email until they verify it

            // MFA enabled and logging in from a new device
            if (deviceResults.alreadyExisted == false && userDetails.MfaEnabled)
            {
                // temporary boolean till we actually implement text message sending
                bool textMessageWorks = false;

                // Check which type of mfa to use
                if (user.Phone != null && userDetails.PhoneVerified && textMessageWorks != false)
                {
                    // Send code as a text message
                    _messageService.SendVerificationTextMessage(user.UserId, user.Phone);
                }
                else
                {
                    // Send code as an email
                    _messageService.SendVerificationEmail(user.UserId, user.Email);
                }

                // Generate token after mfa code verification

                return StatusCode(StatusCodes.Status202Accepted, new AuthorizationDTO { Status = "Success", Code = 420, Details = "Login succesful. Additional confirmation required." });
            }

            // Invalidate tokens for other devices to enforce only one device online policy
            _jwtService.InvalidateAllTokens(user.UserId);

            // Generate a new token and add it to the authorization header. GenerateToken() method automatically adds token to database
            Response.Headers.Authorization = _jwtService.GenerateToken(user.UserId, deviceResults.deviceId, "login");

            if (userDetails.EmailVerified)
            {
                return StatusCode(StatusCodes.Status200OK, new AuthorizationDTO { Status = "Success", Code = 421, Details = "Login succesful." });
            }
            else
            {
                // Send email with verification code
                _messageService.SendVerificationEmail(user.UserId, user.Email);

                // Return 206 to notify frontend of the required extra steps
                return StatusCode(StatusCodes.Status200OK, new AuthorizationDTO { Status = "Success", Code = 422, Details = "Login succesful. Email confirmation required." });
            }
        }

        // Returns
        // 200: User logged out     404: Token couldn't be found
        [HttpPost]
        [TokenTypeAuthorize("login")]
        [Route("LogOut")]
        public IActionResult LogOut(UserDevice dro)
        {
            // Get token indetifier from authorization header
            Guid jti = _jwtService.GetClaims(Request).jti;

            // Set token identifiers "active" property to false
            long tokenId = _jwtService.InvalidateToken(jti);

            if (tokenId > 0)
            {
                return StatusCode(StatusCodes.Status200OK, new AuthorizationDTO { Status = "Success", Code = 511, Details = "Token invalidated. User logged out." });
            }

            return StatusCode(StatusCodes.Status404NotFound, new AuthorizationDTO { Status = "Failure", Code = 510, Details = "Token not found or didn't belong to user." });
        }

        // Returns
        // 200: User still logged in
        [HttpGet]
        [TokenTypeAuthorize("login")]
        [Route("PingBackend")]
        public IActionResult PingBackend(UserDevice dro)
        {
            // All token related verifying and renewing is handled at middleware so just return 200

            return StatusCode(StatusCodes.Status200OK, new AuthorizationDTO { Status = "Success", Code = 610, Details = "Token is valid. User still logged in." });
        }


    }
}
