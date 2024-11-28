using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.IntegrationTests.Repositories
{
    public class UsersRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private UsersRepository _repository;

        public UsersRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new UsersRepository(_context);
        }

        [Fact]
        public void GetUserByUserID_WithExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                // Act
                var result = _repository.GetUserByUserID(user.Entity.UserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<User>();
                result.UserId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetUserByUserID_WithNonExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                long nonExistingUserId = 2000;

                // Act
                var result = _repository.GetUserByUserID(nonExistingUserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<User>();
                result.UserId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetUserByUsernameOrEmail_WithExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                // Act
                var result = _repository.GetUserByUsernameOrEmail("testuser");

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<User>();
                result.UserId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetUserByUsernameOrEmail_WithNonExistingUser()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                // Act
                var result = _repository.GetUserByUsernameOrEmail("wrong.email@test.com");

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<User>();
                result.UserId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetUserByLoginDetails_WithExistingUser_BiometricsKey()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                // Act
                var result = _repository.GetUserByLoginDetails("", new byte[16]);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<User>();
                result.UserId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void GetUserByLoginDetails_WithExistingUser_Username()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                // Act
                var result = _repository.GetUserByLoginDetails("testuser", null!);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<User>();
                result.UserId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void InsertUser_InsertingNewObject()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" };
                _context.SaveChanges();

                // Act
                var result = _repository.InsertUser(user);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void UpdateUser_UpdatingUsername()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                user.Entity.Username = "newUsername";

                // Act
                var result = _repository.UpdateUser(user.Entity);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
        }

        [Fact]
        public void UsernameOrEmailInUse_WithUsernameAndEmailInUse()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testuser", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                string username = "testuser";
                string email = "test.email@example.com";

                // Act
                var result = _repository.UsernameOrEmailInUse(username, email);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeEquivalentTo((true, true));

                // Clean
                _transaction.Rollback();
            });
        }

    }
}
