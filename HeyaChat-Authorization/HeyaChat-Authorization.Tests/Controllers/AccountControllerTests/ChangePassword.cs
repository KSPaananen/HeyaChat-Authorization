using FakeItEasy;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Controllers;
using HeyaChat_Authorization.Repositories;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HeyaChat_Authorization.DataObjects.DRO;
using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;

namespace HeyaChat_Authorization.Tests.Controllers.AccountControllerTests
{
    public class ChangePassword
    {
        private IJwtService _jwtService;
        private IHasherService _hasherService;

        private IUsersRepository _usersRepository;
        private IDevicesRepository _devicesRepository;
        private IAuditLogsRepository _auditLogsRepository;
        private IDeleteRequestsRepository _deleteRequestsRepository;

        private readonly AccountController _controller;

        public ChangePassword()
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
        public void AccountController_ChangePassword_Returns304()
        {
            // Arrange
            var dro = new PasswordChangeDRO() { Password = "testpassword", PasswordRepeat = "nonmatchingpassword", Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            // Act
            var result = _controller.ChangePassword(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status304NotModified);
        }

        [Fact]
        public void AccountController_ChangePassword_Returns201()
        {
            // Arrange
            var dro = new PasswordChangeDRO() { Password = "testpassword", PasswordRepeat = "testpassword", Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _jwtService.GetClaims(A<HttpRequest>.Ignored)).Returns((new Guid(), 1000, "test"));
            A.CallTo(() => _hasherService.GenerateSalt()).Returns(new byte[2048]);
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns("hashedpassword");
            A.CallTo(() => _usersRepository.GetUserByUserID(A<long>.Ignored)).Returns(new User() { });
            A.CallTo(() => _usersRepository.UpdateUser(new User()));
            A.CallTo(() => _devicesRepository.GetDeviceWithUUID(A<Guid>.Ignored)).Returns(new Device());
            A.CallTo(() => _auditLogsRepository.InsertAuditLog(A<long>.Ignored, A<int>.Ignored)).Returns(1000);

            // Act
            var result = _controller.ChangePassword(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1870, Details = "Password changed." });
        }


    }
}
