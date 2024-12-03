using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.IntegrationTests.Repositories
{
    public class UserDetailsRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private UserDetailsRepository _repository;

        public UserDetailsRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new UserDetailsRepository(_context);
        }

        [Fact]
        public void GetUserDetailsByUserId_WithExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var userDetails = _context.UserDetails.Add(new UserDetail() { EmailVerified = true, PhoneVerified = true, MfaEnabled = true, CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow.AddDays(-2) });
                _context.SaveChanges();

                // Act
                var result = _repository.GetUserDetailsByUserId(user.Entity.UserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<UserDetail>();
                result.DetailId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetUserDetailsByUserId_WithNonExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                long nonExistingUserId = 2000;

                // Act
                var result = _repository.GetUserDetailsByUserId(nonExistingUserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<UserDetail>();
                result.DetailId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InsertUserDetails_InsertNewObject()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var userDetails = new UserDetail() { EmailVerified = true, PhoneVerified = true, MfaEnabled = true, CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow.AddDays(-2) };

                // Act
                var result = _repository.InsertUserDetails(userDetails);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void UpdateUserDetails_UpdatingExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var userDetails = _context.UserDetails.Add(new UserDetail() { EmailVerified = false, PhoneVerified = true, MfaEnabled = true, CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow.AddDays(-2) });
                _context.SaveChanges();

                userDetails.Entity.EmailVerified = true;

                // Act
                var result = _repository.UpdateUserDetails(userDetails.Entity);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }


    }
}
