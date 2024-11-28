using FluentAssertions;
using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.IntegrationTests.Repositories
{
    public class TokensRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private TokensRepository _repository;

        public TokensRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new TokensRepository(_context);
        }

        [Fact]
        public void GetTokenByGuid_ExistingToken()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId });
                _context.SaveChanges();

                var token = _context.Tokens.Add(new Token() { Identifier = default(Guid), ExpiresAt = DateTime.UtcNow.AddDays(1), Active = true, DeviceId = device.Entity.DeviceId });
                _context.SaveChanges();

                // Act
                var result = _repository.GetTokenByGuid(token.Entity.Identifier);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Token>();
                result.TokenId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetTokenByGuid_NonExistingToken()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                Guid nonExistingTokenGuid = new Guid();

                // Act
                var result = _repository.GetTokenByGuid(nonExistingTokenGuid);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Token>();
                result.TokenId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InsertToken_InsertNewObject()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId });
                _context.SaveChanges();

                var token = new Token() { Identifier = default(Guid), ExpiresAt = DateTime.UtcNow.AddDays(1), Active = true, DeviceId = device.Entity.DeviceId };

                // Act
                var result = _repository.InsertToken(token);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void UpdateToken_UpdatingExistingToken()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId });
                _context.SaveChanges();

                var token = _context.Tokens.Add(new Token() { Identifier = default(Guid), ExpiresAt = DateTime.UtcNow.AddDays(1), Active = true, DeviceId = device.Entity.DeviceId });
                _context.SaveChanges();

                token.Entity.Active = false;

                // Act
                var result = _repository.UpdateToken(token.Entity);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InvalidateAllTokens_ExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                // Act
                Action act = () => _repository.InvalidateAllTokens(user.Entity.UserId);

                // Assert
                act.Should().NotThrow();

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InvalidateAllTokens_NonExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                long nonExistingUserId = 2000;

                // Act
                Action act = () => _repository.InvalidateAllTokens(nonExistingUserId);

                // Assert
                act.Should().Throw<Exception>();

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void IsTokenValid_TokenIsValid()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId });
                _context.SaveChanges();

                var token = _context.Tokens.Add(new Token() { Identifier = default(Guid), ExpiresAt = DateTime.UtcNow.AddDays(1), Active = true, DeviceId = device.Entity.DeviceId });
                _context.SaveChanges();

                var userDevice = new UserDevice() { DeviceName = "userDevice", DeviceIdentifier = default(Guid), CountryCode = "tes" };

                // Act
                var result = _repository.IsTokenValid(default(Guid), userDevice);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Token>();
                result.TokenId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void IsTokenValid_TokenIsNotValid()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId });
                _context.SaveChanges();

                var token = _context.Tokens.Add(new Token() { Identifier = default(Guid), ExpiresAt = DateTime.UtcNow.AddDays(1), Active = false, DeviceId = device.Entity.DeviceId });
                _context.SaveChanges();

                var userDevice = new UserDevice() { DeviceName = "userDevice", DeviceIdentifier = default(Guid), CountryCode = "tes" };

                // Act
                var result = _repository.IsTokenValid(default(Guid), userDevice);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Token>();
                result.TokenId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });
        }


    }
}
