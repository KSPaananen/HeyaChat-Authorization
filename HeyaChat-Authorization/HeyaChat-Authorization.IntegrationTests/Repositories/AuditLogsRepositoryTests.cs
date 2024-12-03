using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyaChat_Authorization.IntegrationTests.Repositories
{
    public class AuditLogsRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private AuditLogsRepository _repository;

        private IDbContextTransaction? _transaction;

        public AuditLogsRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new AuditLogsRepository(_context);

        }

        [Fact]
        public void AuditLog_CanInsertNewAuditLog()
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

                int type = 0;

                // Act
                var result = _repository.InsertAuditLog(device.Entity.DeviceId, type);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });

        }


    }
}
