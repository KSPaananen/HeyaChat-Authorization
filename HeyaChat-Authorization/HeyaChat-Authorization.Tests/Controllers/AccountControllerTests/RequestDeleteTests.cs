using FakeItEasy;
using FluentAssertions;
using HeyaChat_Authorization.Controllers;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyaChat_Authorization.Tests.Controllers.AccountControllerTests
{
    public class RequestDeleteTests
    {
        private IJwtService _jwtService;
        private IHasherService _hasherService;

        private IUsersRepository _usersRepository;
        private IDevicesRepository _devicesRepository;
        private IAuditLogsRepository _auditLogsRepository;
        private IDeleteRequestsRepository _deleteRequestsRepository;

        private readonly AccountController _controller;

        public RequestDeleteTests()
        {
            _jwtService = A.Fake<IJwtService>();
            _hasherService = A.Fake<IHasherService>();

            _usersRepository = A.Fake<IUsersRepository>();
            _devicesRepository = A.Fake<IDevicesRepository>();
            _auditLogsRepository = A.Fake<IAuditLogsRepository>();
            _deleteRequestsRepository = A.Fake<IDeleteRequestsRepository>();
            _controller = new AccountController(_usersRepository, _devicesRepository, _auditLogsRepository, _jwtService, _hasherService, _deleteRequestsRepository);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public void AccountController_RequestDelete_Returns403()
        {
            // Arrange
            var dro = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" };

            A.CallTo(() => _jwtService.GetClaims(A<HttpRequest>.Ignored)).Returns((new Guid(), 1000, "test"));
            A.CallTo(() => _deleteRequestsRepository.GetDeleteRequestByUserId(A<long>.Ignored)).Returns(new DeleteRequest() { DeleteId = 1000 });

            // Act
            var result = _controller.RequestDelete(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1930, Details = "User already has an active delete request." });
        }

        [Fact]
        public void AccountController_RequestDelete_Returns201()
        {
            // Arrange
            var dro = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" };

            A.CallTo(() => _jwtService.GetClaims(A<HttpRequest>.Ignored)).Returns((new Guid(), 1000, "test"));
            A.CallTo(() => _deleteRequestsRepository.GetDeleteRequestByUserId(A<long>.Ignored)).Returns(new DeleteRequest() { DeleteId = 0 });
            A.CallTo(() => _deleteRequestsRepository.InsertDeleteRequest(new DeleteRequest())).Returns(1000);

            // Act
            var result = _controller.RequestDelete(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1970, Details = "Deletion requested. Account will be deleted in 60 days." });
        }


    }
}
