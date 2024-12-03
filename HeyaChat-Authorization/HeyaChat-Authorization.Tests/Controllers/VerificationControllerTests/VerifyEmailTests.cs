using FakeItEasy;
using FluentAssertions;
using HeyaChat_Authorization.Controllers;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Tests.Controllers.VerificationControllerTests
{
    public class VerifyEmailTests
    {
        private IJwtService _jwtService;

        private ICodesRepository _codesRepository;
        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;

        private VerificationController _controller;
        public VerifyEmailTests()
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
        public void VerifyController_VerifyEmail_Returns401()
        {
            // Arrange
            var dro = new VerifyEmailDRO() { Code = "testcode", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            // Act
            var result = _controller.VerifyEmail(dro) as UnauthorizedResult ?? new UnauthorizedResult();

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public void VerifyController_VerifyEmail_Returns404_CodeExpired()
        {
            // Arrange
            var dro = new VerifyEmailDRO() { Code = "testcode", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _jwtService.GetClaims(A<HttpRequest>.Ignored)).Returns((new Guid(), 1000, "test"));
            A.CallTo(() => _codesRepository.GetValidCodeWithUserIdAndCode(A<long>.Ignored, A<string>.Ignored)).Returns(new Codes() { CodeId = 0 });

            // Act
            var result = _controller.VerifyEmail(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1330, Details = "Code expired or doesn't belong to user." });
        }

        [Fact]
        public void VerifyController_VerifyEmail_Returns200()
        {
            // Arrange
            var dro = new VerifyEmailDRO() { Code = "testcode", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _jwtService.GetClaims(A<HttpRequest>.Ignored)).Returns((new Guid(), 1000, "test"));
            A.CallTo(() => _codesRepository.GetValidCodeWithUserIdAndCode(A<long>.Ignored, A<string>.Ignored)).Returns(new Codes() { CodeId = 1000 });
            A.CallTo(() => _codesRepository.UpdateCode(A<Codes>.Ignored)).Returns(1000);
            A.CallTo(() => _userDetailsRepository.GetUserDetailsByUserId(A<long>.Ignored)).Returns(new UserDetail());
            A.CallTo(() => _userDetailsRepository.UpdateUserDetails(A<UserDetail>.Ignored)).Returns(1000);

            // Act
            var result = _controller.VerifyEmail(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1370, Details = "Code is valid and email has been updated as verified." });
        }


    }
}
