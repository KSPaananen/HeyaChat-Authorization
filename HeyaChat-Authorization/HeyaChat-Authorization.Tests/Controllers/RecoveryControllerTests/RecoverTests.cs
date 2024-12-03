using FakeItEasy;
using FluentAssertions;
using HeyaChat_Authorization.Controllers;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Tests.Controllers.RecoveryControllerTests
{
    public class RecoverTests
    {
        private IMessageService _messageService;
        private IJwtService _jwtService;
        private IToolsService _toolsService;

        private IUsersRepository _usersRepository;
        private IUserDetailsRepository _userDetailsRepository;
        private IDevicesRepository _devicesRepository;

        private readonly RecoveryController _controller;
        public RecoverTests()
        {
            _messageService = A.Fake<IMessageService>();
            _jwtService = A.Fake<IJwtService>();
            _toolsService = A.Fake<IToolsService>();

            _usersRepository = A.Fake<IUsersRepository>();
            _userDetailsRepository = A.Fake<IUserDetailsRepository>();
            _devicesRepository = A.Fake<IDevicesRepository>();
            _controller = new RecoveryController(_usersRepository, _userDetailsRepository, _devicesRepository, _messageService,
                _jwtService, _toolsService);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public void RecoveryController_Recover_Returns404_UserNotFound()
        {
            // Arrange
            var dro = new RecoveryDRO() { Contact = "test.email@example.com", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByUsernameOrEmail("test.email@example.com")).Returns(new User() { UserId = 0 });

            // Act
            var result = _controller.Recover(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            result.Value.Should().BeEquivalentTo(new ContactDTO { Contact = "", Details = new DetailsDTO { Code = 1030, Details = "User matching requested login couldn't be found." } });
        }

        [Fact]
        public void RecoveryController_Recover_Returns200_SendsTextMessageVerification()
        {
            // Arrange
            var dro = new RecoveryDRO() { Contact = "test.email@example.com", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByUsernameOrEmail("test.email@example.com")).Returns(new User() { UserId = 1000, Phone = "+1234567890" });
            A.CallTo(() => _devicesRepository.InsertDeviceIfDoesntExist(A<Device>.Ignored)).Returns((1000, false));
            A.CallTo(() => _userDetailsRepository.GetUserDetailsByUserId(A<long>.Ignored)).Returns(new UserDetail() { EmailVerified = true, PhoneVerified = true });
            A.CallTo(() => _toolsService.MaskPhoneNumber(A<string>.Ignored)).Returns("+***");
            A.CallTo(() => _messageService.SendVerificationTextMessage(A<long>.Ignored, A<string>.Ignored));

            // Act
            var result = _controller.Recover(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _messageService.SendVerificationTextMessage(A<long>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new ContactDTO { Contact = "+***", Details = new DetailsDTO { Code = 1071, Details = "Verification code sent." } });
        }

        [Fact]
        public void RecoveryController_Recover_Returns200_SendsEmailVerification()
        {
            // Arrange
            var dro = new RecoveryDRO() { Contact = "test.email@example.com", Device = new UserDevice() { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByUsernameOrEmail("test.email@example.com")).Returns(new User() { UserId = 1000, Email = "test.email@example.com" });
            A.CallTo(() => _devicesRepository.InsertDeviceIfDoesntExist(A<Device>.Ignored)).Returns((1000, false));
            A.CallTo(() => _userDetailsRepository.GetUserDetailsByUserId(A<long>.Ignored)).Returns(new UserDetail() { EmailVerified = true });
            A.CallTo(() => _messageService.SendRecoveryEmail(A<long>.Ignored, A<string>.Ignored));

            // Act
            var result = _controller.Recover(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _messageService.SendRecoveryEmail(A<long>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new ContactDTO { Contact = "test.email@example.com", Details = new DetailsDTO { Code = 1070, Details = "Verification code sent." } });
        }


    }
}
