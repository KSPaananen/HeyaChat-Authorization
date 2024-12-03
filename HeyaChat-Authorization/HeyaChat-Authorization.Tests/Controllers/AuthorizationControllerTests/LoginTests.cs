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

namespace HeyaChat_Authorization.Tests.Controllers.AuthorizationControllerTests
{
    public class LoginTests
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

        public LoginTests()
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
        public void AuthorizationController_Login_Returns401_UserNotFound()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testlogin", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(dro.Login, dro.BiometricsKey)).Returns((new User { UserId = 0, Username = "", PasswordHash = "", PasswordSalt = [0], BiometricsKey = [0], Email = "", Phone = "" }));

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "", Suspension = new SuspensionDTO { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1230, Details = "User couldn't be found." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns401_WrongPassword()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User()));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns(("differenthash"));

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "", Suspension = new SuspensionDTO { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1230, Details = "User couldn't be found." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns403_UserTemporarilySuspended()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User() { UserId = 1000, Username = "testusername", PasswordHash = "testhash", BiometricsKey = [0] }));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns(("testhash"));
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _suspensionsRepository.IsCurrentlySuspended(A<long>.Ignored)).Returns((new Suspension() { SuspensionId = 1000, ExpiresAt = new DateTime(), Reason = "testreason" }));

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "", Suspension = { Reason = "testreason", Expires = new DateTime().ToString() }, Details = new DetailsDTO { Code = 1233, Details = "User is temporarily suspended." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns403_UserPermanentlySuspended()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User() { UserId = 1000, Username = "testusername", PasswordHash = "testhash", BiometricsKey = [0] }));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns(("testhash"));
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _suspensionsRepository.IsCurrentlySuspended(A<long>.Ignored)).Returns((new Suspension() { SuspensionId = 1000, ExpiresAt = null, Reason = "testreason" }));

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "", Suspension = { Reason = "testreason", Expires = "" }, Details = new DetailsDTO { Code = 1232, Details = "User is permanently suspended." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns406_UserHasActiveDeleteRequest()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User() { UserId = 1000, Username = "testusername", PasswordHash = "testhash", BiometricsKey = [0] }));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns(("testhash"));
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _suspensionsRepository.IsCurrentlySuspended(A<long>.Ignored)).Returns((new Suspension()));
            A.CallTo(() => _deleteRequestsRepository.GetDeleteRequestByUserId(A<long>.Ignored)).Returns((new DeleteRequest() { DeleteId = 1000, DateRequested = DateTime.Now.Date.AddDays(60) }));
            int daysLeft = (int)Math.Abs((DateTime.Now.Date.AddDays(60) - DateTime.Now).TotalDays);

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status406NotAcceptable);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "", Suspension = { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1234, Details = $"User has an active delete request. Account will be deleted in {daysLeft}." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns202_LoggedInWithPhoneMFA()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User() { UserId = 1000, Username = "testusername", PasswordHash = "testhash", BiometricsKey = [0], Phone = "+1234567890" }));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns("testhash");
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _auditLogsRepository.InsertAuditLog(A<long>.Ignored, A<int>.Ignored)).Returns(1000);
            A.CallTo(() => _devicesRepository.InsertDeviceIfDoesntExist(A<Device>.Ignored)).Returns((1000, false));
            A.CallTo(() => _suspensionsRepository.IsCurrentlySuspended(A<long>.Ignored)).Returns(new Suspension());
            A.CallTo(() => _deleteRequestsRepository.GetDeleteRequestByUserId(A<long>.Ignored)).Returns(new DeleteRequest());
            A.CallTo(() => _userDetailsRepository.GetUserDetailsByUserId(A<long>.Ignored)).Returns(new UserDetail() { DetailId = 1000, MfaEnabled = true, PhoneVerified = true });
            A.CallTo(() => _toolsService.MaskPhoneNumber(A<string>.Ignored)).Returns("+***");
            A.CallTo(() => _messageService.SendVerificationTextMessage(A<long>.Ignored, A<string>.Ignored));

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            A.CallTo(() => _messageService.SendVerificationTextMessage(A<long>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "+***", Suspension = { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1270, Details = "Login succesful. Additional confirmation required." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns202_LoggedInWithEmailMFA()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User() { UserId = 1000, Username = "testusername", PasswordHash = "testhash", BiometricsKey = [0], Email = "test.email@example.com" }));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns("testhash");
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _auditLogsRepository.InsertAuditLog(A<long>.Ignored, A<int>.Ignored)).Returns(1000);
            A.CallTo(() => _devicesRepository.InsertDeviceIfDoesntExist(A<Device>.Ignored)).Returns((1000, false));
            A.CallTo(() => _suspensionsRepository.IsCurrentlySuspended(A<long>.Ignored)).Returns(new Suspension());
            A.CallTo(() => _deleteRequestsRepository.GetDeleteRequestByUserId(A<long>.Ignored)).Returns(new DeleteRequest());
            A.CallTo(() => _userDetailsRepository.GetUserDetailsByUserId(A<long>.Ignored)).Returns(new UserDetail() { DetailId = 1000, MfaEnabled = true, EmailVerified = true });
            A.CallTo(() => _toolsService.MaskEmail(A<string>.Ignored)).Returns("t***.e***@***.***");
            A.CallTo(() => _messageService.SendVerificationEmail(A<long>.Ignored, A<string>.Ignored));

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _messageService.SendVerificationEmail(A<long>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            result.StatusCode.Should().Be(StatusCodes.Status202Accepted);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "t***.e***@***.***", Suspension = { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1271, Details = "Login succesful. Additional confirmation required." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns200_LoginSuccesfulWithUnVerifiedEmail()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User() { UserId = 1000, Username = "testusername", PasswordHash = "testhash", BiometricsKey = [0], Email = "test.email@example.com" }));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns("testhash");
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _auditLogsRepository.InsertAuditLog(A<long>.Ignored, A<int>.Ignored)).Returns(1000);
            A.CallTo(() => _devicesRepository.InsertDeviceIfDoesntExist(A<Device>.Ignored)).Returns((1000, false));
            A.CallTo(() => _suspensionsRepository.IsCurrentlySuspended(A<long>.Ignored)).Returns(new Suspension());
            A.CallTo(() => _deleteRequestsRepository.GetDeleteRequestByUserId(A<long>.Ignored)).Returns(new DeleteRequest());
            A.CallTo(() => _userDetailsRepository.GetUserDetailsByUserId(A<long>.Ignored)).Returns(new UserDetail() { DetailId = 1000, EmailVerified = false });
            A.CallTo(() => _jwtService.InvalidateAllTokens(A<long>.Ignored));
            A.CallTo(() => _jwtService.GenerateToken(A<long>.Ignored, A<long>.Ignored, A<string>.Ignored)).Returns("token");
            A.CallTo(() => _messageService.SendVerificationEmail(A<long>.Ignored, A<string>.Ignored));
            A.CallTo(() => _toolsService.MaskEmail(A<string>.Ignored)).Returns("t***.e***@***.***");

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _jwtService.InvalidateAllTokens(A<long>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _jwtService.GenerateToken(A<long>.Ignored, A<long>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _messageService.SendVerificationEmail(A<long>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "t***.e***@***.***", Suspension = new SuspensionDTO { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1273, Details = "Login succesful. Email confirmation required." } });
        }

        [Fact]
        public void AuthorizationController_Login_Returns200_LoginSuccesful()
        {
            // Arrange
            var dro = new LoginDRO { Login = "testusername", Password = "testpassword", BiometricsKey = [1, 2, 4], Device = new UserDevice { DeviceName = "testname", DeviceIdentifier = new Guid(), CountryCode = "tes" } };

            A.CallTo(() => _usersRepository.GetUserByLoginDetails(A<string>.Ignored, A<byte[]>.Ignored)).Returns((new User() { UserId = 1000, Username = "testusername", PasswordHash = "testhash", BiometricsKey = [0], Email = "test.email@example.com" }));
            A.CallTo(() => _hasherService.Hash(A<string>.Ignored, A<byte[]>.Ignored)).Returns("testhash");
            A.CallTo(() => _devicesRepository.InsertDevice(A<Device>.Ignored)).Returns(1000);
            A.CallTo(() => _auditLogsRepository.InsertAuditLog(A<long>.Ignored, A<int>.Ignored)).Returns(1000);
            A.CallTo(() => _devicesRepository.InsertDeviceIfDoesntExist(A<Device>.Ignored)).Returns((1000, false));
            A.CallTo(() => _suspensionsRepository.IsCurrentlySuspended(A<long>.Ignored)).Returns(new Suspension());
            A.CallTo(() => _deleteRequestsRepository.GetDeleteRequestByUserId(A<long>.Ignored)).Returns(new DeleteRequest());
            A.CallTo(() => _userDetailsRepository.GetUserDetailsByUserId(A<long>.Ignored)).Returns(new UserDetail() { DetailId = 1000, EmailVerified = true });
            A.CallTo(() => _jwtService.InvalidateAllTokens(A<long>.Ignored));
            A.CallTo(() => _jwtService.GenerateToken(A<long>.Ignored, A<long>.Ignored, A<string>.Ignored)).Returns("token");

            // Act
            var result = _controller.Login(dro) as ObjectResult ?? new ObjectResult(dro);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _jwtService.InvalidateAllTokens(A<long>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _jwtService.GenerateToken(A<long>.Ignored, A<long>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new LoginDTO { Contact = "", Suspension = { Reason = "", Expires = "" }, Details = new DetailsDTO { Code = 1272, Details = "Login succesful." } });
        }


    }
}
