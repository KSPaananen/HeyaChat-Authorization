﻿using HeyaChat_Authorization.AuthorizeAttributes;
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
        [TokenTypeAuthorize("password")]    
        [Route("ChangePassword")]           
        public IActionResult ChangePassword(PasswordChangeDRO dro)
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

        // Returns
        //
        [HttpDelete, Authorize]
        [TokenTypeAuthorize("login, suspended")]
        [Route("DeleteAccount")]
        public IActionResult DeleteAccount()
        {
            long userId = _jwtService.GetClaims(Request).userId;

            _usersRepository.DeleteUser(userId);

            return StatusCode(StatusCodes.Status201Created);
        }

    }
}
