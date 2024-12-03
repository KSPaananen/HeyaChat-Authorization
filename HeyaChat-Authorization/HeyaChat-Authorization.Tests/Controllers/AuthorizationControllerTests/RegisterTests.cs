using FakeItEasy;
using FluentAssertions;
using HeyaChat_Authorization.Controllers;
using HeyaChat_Authorization.DataObjects.DRO;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.DataObjects.DTO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HeyaChat_Authorization.Tests.Controllers.AuthorizationControllerTests
{
    public class RegisterTests
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

        public RegisterTests()
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
        public void AuthorizationController_Register_Returns201_RegisteringSuccesful()
        {
            // Arrange
            var dro = new RegisterDRO { Username = "testuser", Password = "testpassword", Email = "test.user@example.com", Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.UsernameOrEmailInUse(dro.Username, dro.Email)).Returns((false, false));
            A.CallTo(() => _blockedCredentialsRepository.IsCredentialBlocked(dro.Email)).Returns(false);

            A.CallTo(() => _hasherService.GenerateSalt()).Returns(new byte[2048]);
            A.CallTo(() => _hasherService.Hash(dro.Password, A<byte[]>.Ignored)).Returns("hashedpassword");
            A.CallTo(() => _usersRepository.InsertUser(A<User>.Ignored)).Returns(1000);
            A.CallTo(() => _userDetailsRepository.InsertUserDetails(A<UserDetail>.Ignored)).Returns(1000);
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _jwtService.GenerateToken(A<long>.Ignored, A<long>.Ignored, "login")).Returns("token");

            // Act
            var result = _controller.Register(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _messageService.SendVerificationEmail(1000, dro.Email)).MustHaveHappenedOnceExactly();
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1570, Details = "New user succesfully registered." });
        }

        [Fact]
        public void AuthorizationController_Register_Returns302_UsernameAndEmailAlreadyInUse()
        {
            // Arrange
            var dro = new RegisterDRO { Username = "testuser", Password = "testpassword", Email = "test.user@example.com", Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.UsernameOrEmailInUse(dro.Username, dro.Email)).Returns((true, true));

            // Act
            var result = _controller.Register(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status302Found);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1530, Details = "Username and email address already in use by another account." });
        }

        [Fact]
        public void AuthorizationController_Register_Returns406_UsernameRegexFails()
        {
            // Arrange
            var dro = new RegisterDRO { Username = "!!", Password = "testpassword", Email = "test.user@example.com", Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.UsernameOrEmailInUse(dro.Username, dro.Email)).Returns((false, false));

            // Act
            var result = _controller.Register(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status406NotAcceptable);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1534, Details = "Request didn't pass regex check." });
        }

        [Fact]
        public void AuthorizationController_Register_Returns406_EmailRegexFails()
        {
            // Arrange
            var dro = new RegisterDRO { Username = "testuser", Password = "testpassword", Email = "incorrecttestemail", Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.UsernameOrEmailInUse(dro.Username, dro.Email)).Returns((false, false));

            // Act
            var result = _controller.Register(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status406NotAcceptable);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1534, Details = "Request didn't pass regex check." });
        }

        [Fact]
        public void AuthorizationController_Register_Returns302_EmailBlocked()
        {
            // Arrange
            var dro = new RegisterDRO { Username = "testuser", Password = "testpassword", Email = "test.user@example.com", Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.UsernameOrEmailInUse(dro.Username, dro.Email)).Returns((false, false));
            A.CallTo(() => _blockedCredentialsRepository.IsCredentialBlocked(dro.Email)).Returns(true);

            // Act
            var result = _controller.Register(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status302Found);
            result.Value.Should().BeEquivalentTo(new DetailsDTO { Code = 1533, Details = "Email address blocked from creating new accounts." });
        }


    }
}
