using FakeItEasy;
using FluentAssertions;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HeyaChat_Authorization.Tests.Services
{
    public class JwtServiceTests
    {
        private IConfigurationRepository _configurationRepository;
        private ITokensRepository _tokensRepository;

        private JwtService _service;

        public JwtServiceTests()
        {
            _configurationRepository = A.Fake<IConfigurationRepository>();
            _tokensRepository = A.Fake<ITokensRepository>();

            _service = new JwtService(_configurationRepository, _tokensRepository);
        }

        [Fact]
        public void JwtService_GenerateToken_ShouldSucceed()
        {
            // Arrange
            long userId = 1000;
            long deviceId = 1000;
            string type = "test";

            A.CallTo(() => _configurationRepository.GetTokenLifeTime()).Returns(TimeSpan.Parse("00:05:00"));
            A.CallTo(() => _configurationRepository.GetSigningKey()).Returns(new byte[256]);
            A.CallTo(() => _configurationRepository.GetIssuer()).Returns("testissuer");
            A.CallTo(() => _configurationRepository.GetAudience()).Returns("testaudience");
            A.CallTo(() => _configurationRepository.GetEncryptionKey()).Returns(new byte[16]);
            A.CallTo(() => _tokensRepository.InsertToken(A<Token>.Ignored));

            // Act
            var result = _service.GenerateToken(userId, deviceId, type);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void JwtService_RenewToken_ShouldSucceed()
        {
            // Arrange
            long userId = 1000;
            long deviceId = 1000;
            string type = "test";
            Guid jti = new Guid();

            var _fakeService =  A.Fake<IJwtService>();

            A.CallTo(() => _tokensRepository.GetTokenByGuid(jti)).Returns(new Token() { Active = false });
            A.CallTo(() => _tokensRepository.UpdateToken(A<Token>.Ignored)).Returns(1000);
            A.CallTo(() => _configurationRepository.GetTokenLifeTime()).Returns(TimeSpan.Parse("00:05:00"));
            A.CallTo(() => _configurationRepository.GetSigningKey()).Returns(new byte[256]);
            A.CallTo(() => _configurationRepository.GetIssuer()).Returns("testissuer");
            A.CallTo(() => _configurationRepository.GetAudience()).Returns("testaudience");
            A.CallTo(() => _configurationRepository.GetEncryptionKey()).Returns(new byte[16]);
            A.CallTo(() => _tokensRepository.InsertToken(A<Token>.Ignored));

            // Act
            var result = _service.RenewToken(userId, deviceId, type, jti);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void JwtService_RenewToken_ShouldFail()
        {
            // Arrange
            long userId = 1000;
            long deviceId = 1000;
            string type = "test";
            Guid jti = new Guid();

            // Act
            var result = _service.RenewToken(userId, deviceId, type, jti);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void JwtService_InvalidateToken_ShouldSucceed()
        {
            // Arrange
            Guid jti = new Guid();

            A.CallTo(() => _tokensRepository.GetTokenByGuid(jti)).Returns(new Token() { Active = false });
            A.CallTo(() => _tokensRepository.UpdateToken(A<Token>.Ignored)).Returns(1000);

            // Act
            var result = _service.InvalidateToken(jti);

            // Assert
            result.Should().NotBe(0);
            A.CallTo(() => _tokensRepository.GetTokenByGuid(A<Guid>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _tokensRepository.UpdateToken(A<Token>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void JwtService_InvalidateAllTokenS_ShouldSucceed()
        {
            // Arrange
            long userId = 1000;

            // Act
            _service.InvalidateAllTokens(userId);

            // Assert
            A.CallTo(() => _tokensRepository.InvalidateAllTokens(userId)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void JwtService_VerifyToken_ShouldSucceed_TokenValidAndDoesntExpireSoon()
        {
            // Arrange
            Guid jti = new Guid();
            UserDevice device = new UserDevice();

            A.CallTo(() => _tokensRepository.IsTokenValid(A<Guid>.Ignored, A<UserDevice>.Ignored)).Returns(new Token() { TokenId = 1000, ExpiresAt = DateTime.UtcNow.AddDays(-2) });
            A.CallTo(() => _configurationRepository.GetTokenRenewTime()).Returns(TimeSpan.Parse("48:00:00"));

            // Act
            var result = _service.VerifyToken(jti, device);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be((true, false));
        }

        [Fact]
        public void JwtService_VerifyToken_ShouldSucceed_TokenValidAndExpiresSoon()
        {
            // Arrange
            Guid jti = new Guid();
            UserDevice device = new UserDevice();

            A.CallTo(() => _tokensRepository.IsTokenValid(A<Guid>.Ignored, A<UserDevice>.Ignored)).Returns(new Token() { TokenId = 1000, ExpiresAt = DateTime.UtcNow.AddDays(1.99) });
            A.CallTo(() => _configurationRepository.GetTokenRenewTime()).Returns(TimeSpan.Parse("48:00:00"));

            // Act
            var result = _service.VerifyToken(jti, device);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be((true, true));
        }

        [Fact]
        public void JwtService_VerifyToken_ShouldFail()
        {
            // Arrange
            Guid jti = new Guid();
            UserDevice device = new UserDevice();

            A.CallTo(() => _tokensRepository.IsTokenValid(A<Guid>.Ignored, A<UserDevice>.Ignored)).Returns(new Token() { TokenId = 0 });

            // Act
            var result = _service.VerifyToken(jti, device);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be((false, false));
        }

        [Fact]
        public void JwtService_GetClaims_ShouldSucceed_HeaderContainsBearer()
        {
            // Arrange
            var request = A.Fake<HttpRequest>();
 
            Guid jti = new Guid();
            long userId = 1000;
            string type = "test";
            // Pre-encrypt values to see if method is able to decrypt them
            string encryptedUserId = "XuOtXonvXveOogupjLUI76t4gL0n3+gmSXkWm5Bb6As=";
            string encryptedType = "pyWmtMZ9OVWAbkERFlo3Sh0dLtrL/zyCBfD7qTYw3MI=";

            // Create new token for this test
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, encryptedUserId),
                new Claim(JwtRegisteredClaimNames.Typ, encryptedType)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key-thats-around-256-bits"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "testIssuer",
                audience: "testAudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var tokenHandler = new JwtSecurityTokenHandler();
            string jwt = tokenHandler.WriteToken(token);

            request.Headers.Authorization = $"bearer {jwt}";

            A.CallTo(() => _configurationRepository.GetEncryptionKey()).Returns(new byte[16]);

            // Act
            var result = _service.GetClaims(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be((jti, userId, type));
        }

        [Fact]
        public void JwtService_GetClaims_ShouldSucceed_HeaderDoesntContainBearer()
        {
            // Arrange
            var request = A.Fake<HttpRequest>();

            Guid jti = new Guid();
            long userId = 1000;
            string type = "test";
            // Pre-encrypt values to see if method is able to decrypt them
            string encryptedUserId = "XuOtXonvXveOogupjLUI76t4gL0n3+gmSXkWm5Bb6As=";
            string encryptedType = "pyWmtMZ9OVWAbkERFlo3Sh0dLtrL/zyCBfD7qTYw3MI=";

            // Create new token for this test
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, encryptedUserId),
                new Claim(JwtRegisteredClaimNames.Typ, encryptedType)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-signing-key-thats-around-256-bits"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "testIssuer",
                audience: "testAudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            var tokenHandler = new JwtSecurityTokenHandler();
            string jwt = tokenHandler.WriteToken(token);

            request.Headers.Authorization = $"bearer {jwt}";

            A.CallTo(() => _configurationRepository.GetEncryptionKey()).Returns(new byte[16]);

            // Act
            var result = _service.GetClaims(request);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be((jti, userId, type));
        }



    }
}
