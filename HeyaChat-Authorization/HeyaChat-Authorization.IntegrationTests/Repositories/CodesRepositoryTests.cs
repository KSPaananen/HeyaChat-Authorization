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
    public class CodesRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private CodesRepository _repository;

        private IDbContextTransaction? _transaction;

        public CodesRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new CodesRepository(_context);
        }

        [Fact]
        public void GetCodeByUserId_CodeExists()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testname12356",  PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                // Act
                var result = _repository.GetCodeByUserId(user.Entity.UserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Codes>();
                result.CodeId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });
            
        }

        [Fact]
        public void GetCodeByUserId_CodeDoesNotExist()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                long nonExistingUserId = 9999;

                // Act
                var result = _repository.GetCodeByUserId(nonExistingUserId);

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Codes>();
                result.CodeId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });

        }

        [Fact]
        public void GetValidCodeWithUserIdAndCode_ValidCodeCanBeFound()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testname", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var code = _context.Codess.Add(new Codes() { Code = "testcode", ExpiresAt = DateTime.UtcNow.AddDays(1) });
                _context.SaveChanges();

                // Act
                var result = _repository.GetValidCodeWithUserIdAndCode(user.Entity.UserId, "testcode");

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Codes>();
                result.CodeId.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });

        }

        [Fact]
        public void GetValidCodeWithUserIdAndCode_ValidCodeCanNotBeFound()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                long nonExistingUserId = 9999;

                // Act
                var result = _repository.GetValidCodeWithUserIdAndCode(nonExistingUserId, "nonexistingtestcode");

                // Assert
                result.Should().NotBeNull();
                result.Should().BeOfType<Codes>();
                result.CodeId.Should().Be(0);

                // Clean
                _transaction.Rollback();
            });

        }

        [Fact]
        public void InsertCode_InsertNewObject()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testname", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                Codes code = new Codes() { Code = "new code", ExpiresAt = DateTime.UtcNow.AddDays(1), Used = false, UserId = user.Entity.UserId };

                // Act
                var result = _repository.InsertCode(code);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });

        }

        [Fact]
        public void UpdateCode_UpdateFoundCode()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                var user = _context.Users.Add(new User() { Username = "testname", PasswordHash = "testhash", PasswordSalt = new byte[16], BiometricsKey = new byte[16], Email = "test.email@example.com", Phone = "+1234567890" });
                _context.SaveChanges();

                var code = _context.Codess.Add(new Codes() { Code = "testcode", ExpiresAt = DateTime.UtcNow.AddDays(1) });
                _context.SaveChanges();

                // Act
                code.Entity.Used = true;

                var result = _repository.UpdateCode(code.Entity);

                // Assert
                result.Should().NotBe(0);

                // Clean
                _transaction.Rollback();
            });

        }


    }
}
