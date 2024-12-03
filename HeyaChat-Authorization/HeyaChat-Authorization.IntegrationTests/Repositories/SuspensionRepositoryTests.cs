using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.IntegrationTests.Repositories
{
    public class SuspensionRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private SuspensionsRepository _repository;

        public SuspensionRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new SuspensionsRepository(_context);
        }

        [Fact]
        public void IsCurrentlySuspended_IsSuspended()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var suspension = _context.Suspensions.Add(new Suspension() { Reason = "test reason", SuspendedAt = DateTime.UtcNow.AddDays(-1), ExpiresAt = DateTime.UtcNow.AddDays(2), LiftedAt = null, UserId = user.Entity.UserId });
                _context.SaveChanges();

                // Act
                var result = _repository.IsCurrentlySuspended(user.Entity.UserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Suspension>();
                result.SuspensionId.Should().NotBe(0);
                result.LiftedAt.Should().BeNull();

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void IsCurrentlySuspended_IsNotSuspended()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var suspension = _context.Suspensions.Add(new Suspension() { Reason = "test reason", SuspendedAt = DateTime.UtcNow.AddDays(-2), ExpiresAt = DateTime.UtcNow.AddDays(-1), LiftedAt = DateTime.UtcNow.AddDays(-0.5), UserId = user.Entity.UserId });
                _context.SaveChanges();

                // Act
                var result = _repository.IsCurrentlySuspended(user.Entity.UserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Suspension>();
                result.SuspensionId.Should().NotBe(0);
                result.LiftedAt.Should().NotBeNull();

                // Clean
                _transaction.Rollback();
            });
        }


    }
}
