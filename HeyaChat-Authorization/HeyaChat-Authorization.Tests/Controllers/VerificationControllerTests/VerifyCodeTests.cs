using FakeItEasy;
using HeyaChat_Authorization.Controllers;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;

namespace HeyaChat_Authorization.Tests.Controllers.VerificationControllerTests
{
    public class VerifyCodeTests
    {
        private IJwtService _jwtService;

        private ICodesRepository _codesRepository;
        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;

        private VerificationController _controller;
        public VerifyCodeTests()
        {
            _jwtService = A.Fake<IJwtService>();

            _codesRepository = A.Fake<ICodesRepository>();
            _usersRepository = A.Fake<IUsersRepository>();
            _userDetailsRepository = A.Fake<IUserDetailsRepository>();
            _devicesRepository = A.Fake<IDevicesRepository>();

            _controller = new VerificationController(_jwtService, _usersRepository, _codesRepository, _userDetailsRepository, _devicesRepository);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public void VerifyController_VerifyCode_Returns404()
        {
            // Arrange
            var dro = new VerifyDRO() { Email = "test.email@example.com", Code = "testcode", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByUsernameOrEmail(A<string>.Ignored)).Returns(new User());
            A.CallTo(() => _codesRepository.GetValidCodeWithUserIdAndCode(A<long>.Ignored, A<string>.Ignored)).Returns(new Codes() { CodeId = 0 });

            // Act
            var result = _controller.VerifyCode(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1130, Details = "Code expired or doesn't belong to user." });
        }

        [Fact]
        public void VerifyController_VerifyCode_Returns200()
        {
            // Arrange
            var dro = new VerifyDRO() { Email = "test.email@example.com", Code = "testcode", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByUsernameOrEmail(A<string>.Ignored)).Returns(new User());
            A.CallTo(() => _codesRepository.GetValidCodeWithUserIdAndCode(A<long>.Ignored, A<string>.Ignored)).Returns(new Codes() { CodeId = 1000 });
            A.CallTo(() => _devicesRepository.GetDeviceWithUUID(A<Guid>.Ignored)).Returns(new Device());
            A.CallTo(() => _jwtService.GenerateToken(A<long>.Ignored, A<long>.Ignored, A<string>.Ignored)).Returns("token");
            A.CallTo(() => _codesRepository.UpdateCode(A<Codes>.Ignored)).Returns(1000);

            // Act
            var result = _controller.VerifyCode(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1170, Details = "Code is valid." });
        }


    }
}
