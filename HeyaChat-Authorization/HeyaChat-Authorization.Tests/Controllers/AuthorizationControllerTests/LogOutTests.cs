﻿using FakeItEasy;
using FluentAssertions;
using HeyaChat_Authorization.Controllers;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyaChat_Authorization.Tests.Controllers.AuthorizationControllerTests
{
    public class LogOutTests
    {
        private readonly IHasherService _hasherService;
        private readonly IMessageService _messageService;
        private readonly IJwtService _jwtService;
        private readonly IToolsService _toolsService;

        private readonly IUsersRepository _usersRepository;
        private readonly IUserDetailsRepository _userDetailsRepository;
        private readonly IDevicesRepository _devicesRepository;
        private readonly ISuspensionsRepository _suspensionsRepository;
        private readonly IAuditLogsRepository _auditLogsRepository;
        private readonly IBlockedCredentialsRepository _blockedCredentialsRepository;
        private readonly IDeleteRequestsRepository _deleteRequestsRepository;

        private readonly AuthorizationController _controller;


        public LogOutTests()
        {
            _hasherService = A.Fake<IHasherService>();
            _messageService = A.Fake<IMessageService>();
            _jwtService = A.Fake<IJwtService>();
            _toolsService = A.Fake<IToolsService>();

            _usersRepository = A.Fake<IUsersRepository>();
            _userDetailsRepository = A.Fake<IUserDetailsRepository>();
            _devicesRepository = A.Fake<IDevicesRepository>();
            _suspensionsRepository = A.Fake<ISuspensionsRepository>();
            _auditLogsRepository = A.Fake<IAuditLogsRepository>();
            _blockedCredentialsRepository = A.Fake<IBlockedCredentialsRepository>();
            _deleteRequestsRepository = A.Fake<IDeleteRequestsRepository>();

            _controller = new AuthorizationController(_usersRepository, _userDetailsRepository, _devicesRepository, _hasherService, _messageService, _jwtService, _suspensionsRepository,
            _auditLogsRepository, _blockedCredentialsRepository, _toolsService, _deleteRequestsRepository);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public void AuthorizationController_LogOut_Returns401()
        {
            // Arrange
            var dro = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" };

            // Act
            var result = _controller.LogOut(dro) as UnauthorizedResult ?? new UnauthorizedResult();

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public void AuthorizationController_LogOut_Returns404()
        {
            // Arrange
            var dro = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" };

            A.CallTo(() => _jwtService.GetClaims(A<HttpRequest>.Ignored)).Returns((new Guid(), 1000, "type"));
            A.CallTo(() => _jwtService.InvalidateToken(new Guid())).Returns(0);

            // Act
            var result = _controller.LogOut(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1630, Details = "Token not found or didn't belong to user." });
        }

        [Fact]
        public void AuthorizationController_LogOut_Returns200()
        {
            // Arrange
            var dro = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" };

            A.CallTo(() => _jwtService.GetClaims(A<HttpRequest>.Ignored)).Returns((new Guid(), 1000, "type"));
            A.CallTo(() => _jwtService.InvalidateToken(new Guid())).Returns(1000);

            // Act
            var result = _controller.LogOut(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1670, Details = "Token invalidated. User logged out." });
        }


    }
}
