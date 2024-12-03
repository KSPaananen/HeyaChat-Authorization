using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.IntegrationTests.Repositories
{
    public class DevicesRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private DevicesRepository _repository;

        public DevicesRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new DevicesRepository(_context);
        }

        [Fact]
        public void GetDeviceWithUUID_ExistingDevice()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId }).Entity;
                _context.SaveChanges();

                // Act
                var result = _repository.GetDeviceWithUUID(device.DeviceIdentifier);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Device>();
                result.DeviceId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetDeviceWithUUID_NonExistingDevice()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                Guid guid = Guid.NewGuid();

                // Act
                var result = _repository.GetDeviceWithUUID(guid);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Device>();
                result.DeviceId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InsertDevice_InsertNewObject()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId };
                _context.SaveChanges();

                // Act
                var result = _repository.InsertDevice(device);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InsertDeviceIfDoesntExist_DeviceExists()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId }).Entity;
                _context.SaveChanges();

                // Act
                var result = _repository.InsertDeviceIfDoesntExist(device);

                // Assert
                result.Should().NotBeNull();
                result.Should().Be((device.DeviceId, true));

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InsertDeviceIfDoesntExist_DeviceDoesNotExists()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId }).Entity;
                _context.SaveChanges();

                var newDevice = new Device() { DeviceName = "newDevice", DeviceIdentifier = new Guid(), CountryTag = "set", UserId = user.Entity.UserId };

                // Act
                var result = _repository.InsertDeviceIfDoesntExist(newDevice);

                // Assert
                result.Should().NotBeNull();
                result.Should().NotBe((0, true));

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void UpdateDevice_UpdateExistingDevice()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId }).Entity;
                _context.SaveChanges();

                device.DeviceName = "newTestName";

                // Act
                var result = _repository.UpdateDevice(device);

                // Assert
                result.Should().NotBe(0);
                result.Should().Be(device.DeviceId);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void DeleteDevice_DeviceExists()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var device = _context.Devices.Add(new Device() { DeviceName = "testname", DeviceIdentifier = default(Guid), CountryTag = "tes", UserId = user.Entity.UserId }).Entity;
                _context.SaveChanges();

                // Act
                Action act = () => _repository.DeleteDevice(device.DeviceId);

                // Assert
                act.Should().NotThrow();
                var result = _context.Devices.First(x => x.DeviceId == device.DeviceId);
                result.Should().BeNull();

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void DeleteDevice_DeviceDoesNotExist()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                long nonExistingDeviceId = 2000;

                // Act
                Action act = () => _repository.DeleteDevice(nonExistingDeviceId);

                // Assert
                act.Should().Throw<Exception>();

                // Clean
                _transaction.Rollback();
            });
        }


    }
}
