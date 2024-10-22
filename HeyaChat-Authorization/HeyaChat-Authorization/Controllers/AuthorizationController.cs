using HeyaChat_Authorization.AuthorizeAttributes;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
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
        private IToolsService _toolsService;

        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;
        private ISuspensionsRepository _suspensionsRepository;
        private IAuditLogsRepository _auditLogsRepository;
        private IBlockedCredentialsRepository _blockedCredentialsRepository;
        private IDeleteRequestsRepository _deleteRequestsRepository;
        
        public AuthorizationController(IUsersRepository usersRepository, IUserDetailsRepository userDetailsRepository, IDevicesRepository devicesRepository, 
            IHasherService hasherService, IMessageService messageService, IJwtService jwtService, ISuspensionsRepository suspensionsRepository, IAuditLogsRepository auditLogsRepository,
            IBlockedCredentialsRepository blockedCredentialsRepository, IToolsService toolsService, IDeleteRequestsRepository deleteRequestsRepository)
        {
            _hasherService = hasherService ?? throw new NullReferenceException(nameof(hasherService));
            _messageService = messageService ?? throw new NullReferenceException(nameof(messageService));
            _jwtService = jwtService ?? throw new NullReferenceException(nameof(jwtService));
            _toolsService = toolsService ?? throw new NullReferenceException(nameof(toolsService));

            _usersRepository = usersRepository ?? throw new NullReferenceException(nameof(usersRepository));
            _userDetailsRepository = userDetailsRepository ?? throw new NullReferenceException(nameof(userDetailsRepository));
            _devicesRepository = devicesRepository ?? throw new NullReferenceException(nameof(devicesRepository));
            _suspensionsRepository = suspensionsRepository ?? throw new NullReferenceException(nameof(suspensionsRepository));
            _auditLogsRepository = auditLogsRepository ?? throw new NullReferenceException(nameof(auditLogsRepository));
            _blockedCredentialsRepository = blockedCredentialsRepository ?? throw new NullReferenceException(nameof(blockedCredentialsRepository));
            _deleteRequestsRepository = deleteRequestsRepository ?? throw new NullReferenceException(nameof(deleteRequestsRepository));
        }

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
                return StatusCode(StatusCodes.Status302Found, new DetailsDTO { Code = 1530, Details = "Username and email address already in use by another account." });
            }
            else if (existResults.usernameInUse)
            {
                return StatusCode(StatusCodes.Status302Found, new DetailsDTO { Code = 1531, Details = "Username already in use by another account." });
            }
            else if (existResults.emailInUse)
            {
                return StatusCode(StatusCodes.Status302Found, new DetailsDTO { Code = 1532, Details = "Email address already in use by another account." });
            }

            // Make sure dro passes regex check
            if (!usernameRgx.IsMatch(dro.Username) && !emailRgx.IsMatch(dro.Email) && !passwordRgx.IsMatch(dro.Password))
            {
                return StatusCode(StatusCodes.Status406NotAcceptable, new DetailsDTO { Code = 1534, Details = "Request didn't pass regex check." });
            }

            bool isBlocked = _blockedCredentialsRepository.IsCredentialBlocked(dro.Email);

            // Also make sure requested email isn't blocked from creating new accounts
            if (isBlocked)
            {
                return StatusCode(StatusCodes.Status302Found, new DetailsDTO { Code = 1533, Details = "Email address blocked from creating new accounts." });
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

            return StatusCode(StatusCodes.Status201Created, new DetailsDTO { Code = 1570, Details = "New user succesfully registered." });
        }

        // Returns
        // 200: Login succesful     202: MFA verification required      401: Login unsuccesful    403: User suspended   406: User has an active delete request
        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginDRO dro)
        {
            User user = _usersRepository.GetUserByLoginDetails(dro.Login, dro.BiometricsKey);

            // UserId will be 0 if user with details cannot be found
            if (user.UserId <= 0)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new LoginDTO { Contact = "", Suspension = new SuspensionDTO { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1230, Details = "User couldn't be found." } });
            }

            // Hash the password from dro with salt from the user object and see if they match
            var droPasswordHash = _hasherService.Hash(dro.Password, user.PasswordSalt);

            // Check that login details match
            if (droPasswordHash != user.PasswordHash || (dro.BiometricsKey != user.BiometricsKey && dro.Login == "" && dro.Password == ""))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new LoginDTO { Contact = "", Suspension = new SuspensionDTO { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1231, Details = "User couldn't login." } });
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
            var foundSusp = _suspensionsRepository.IsCurrentlySuspended(user.UserId);

            if (foundSusp.SuspensionId != 0)
            {
                if (foundSusp.ExpiresAt == null)
                {
                    Response.Headers.Authorization = _jwtService.GenerateToken(user.UserId, deviceResults.deviceId, "suspended");

                    return StatusCode(StatusCodes.Status403Forbidden, new LoginDTO { Contact = "", Suspension = { Reason = foundSusp.Reason ?? "", Expires = foundSusp.ExpiresAt.ToString() }, Details = new DetailsDTO { Code = 1232, Details = "User is permanently suspended." } });
                }

                return StatusCode(StatusCodes.Status403Forbidden, new LoginDTO { Contact = "", Suspension = { Reason = foundSusp.Reason ?? "", Expires = foundSusp.ExpiresAt.ToString() }, Details = new DetailsDTO { Code = 1233, Details = "User is temporarily suspended." } });
            }

            // Check if user has an active delete request. User has to undo delete request to login
            var foundDelRequest = _deleteRequestsRepository.GetDeleteRequestByUserId(user.UserId);

            if (foundDelRequest.DeleteId != 0)
            {
                var daysLeft = Math.Abs((foundDelRequest.DateRequested - DateTime.Now).TotalDays);

                return StatusCode(StatusCodes.Status406NotAcceptable, new LoginDTO { Contact = "", Suspension = { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1234, Details = $"User has an active delete request. Account will be deleted in {daysLeft}." } });
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

                string contact = "";
                int code = 0;

                // Check which type of mfa to use
                if (user.Phone != null && userDetails.PhoneVerified && textMessageWorks != false)
                {
                    contact = _toolsService.MaskPhoneNumber(user.Phone);
                    code = 1270;

                    // Send code as a text message
                    _messageService.SendVerificationTextMessage(user.UserId, user.Phone);
                }
                else
                {
                    contact = _toolsService.MaskEmail(user.Email);
                    code = 1271;

                    // Send code as an email
                    _messageService.SendVerificationEmail(user.UserId, user.Email);
                }

                // Generate token after mfa code verification

                return StatusCode(StatusCodes.Status202Accepted, new LoginDTO { Contact = contact, Suspension = { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = code, Details = "Login succesful. Additional confirmation required." } });
            }

            // Invalidate tokens for other devices to enforce only one device online policy
            _jwtService.InvalidateAllTokens(user.UserId);

            // Generate a new token and add it to the authorization header. GenerateToken() method automatically adds token to database
            Response.Headers.Authorization = _jwtService.GenerateToken(user.UserId, deviceResults.deviceId, "login");

            if (userDetails.EmailVerified)
            {
                return StatusCode(StatusCodes.Status200OK, new LoginDTO { Contact = "", Suspension = { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1272, Details = "Login succesful." } });
            }
            else
            {
                // Send email with verification code
                _messageService.SendVerificationEmail(user.UserId, user.Email);

                string contact = _toolsService.MaskEmail(user.Email);

                // Return 206 to notify frontend of the required extra steps
                return StatusCode(StatusCodes.Status200OK, new LoginDTO { Contact = contact, Suspension = new SuspensionDTO { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1273, Details = "Login succesful. Email confirmation required." } });
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
                return StatusCode(StatusCodes.Status200OK, new DetailsDTO { Code = 1670, Details = "Token invalidated. User logged out." });
            }

            return StatusCode(StatusCodes.Status404NotFound, new DetailsDTO { Code = 1630, Details = "Token not found or didn't belong to user." });
        }

        // Returns
        // 200: User still logged in
        [HttpPost]
        [TokenTypeAuthorize("login")]
        [Route("PingBackend")]
        public IActionResult PingBackend(UserDevice dro)
        {
            // All token related verifying and renewing is handled at middleware so just return 200

            return StatusCode(StatusCodes.Status200OK, new DetailsDTO { Code = 1770, Details = "Token is valid. User still logged in." });
        }


    }
}
