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
    public class BlockedCredentialsRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private AuthorizationDBContext _context;
        private BlockedCredentialsRepository _repository;

        private IDbContextTransaction? _transaction;
        public BlockedCredentialsRepositoryTests(DatabaseFixture fixture)
        {
            _context = fixture._context;
            _repository = new BlockedCredentialsRepository(_context);
        }

        [Fact]
        public void IsCredentialBlocked_IsBlocked()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                _context.BlockedCredentials.Add(new BlockedCredential() { Email = "blocked.test@email.com", Phone = "+0987654321" });
                _context.SaveChanges();

                string credential = "blocked.test@email.com";

                // Act
                var result = _repository.IsCredentialBlocked(credential);

                // Assert
                result.Should().BeTrue();

                // Clean
                _transaction.Rollback();
            });
            
        }

        [Fact]
        public void IsCredentialBlocked_IsNotBlocked()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var _transaction = _context.Database.BeginTransaction();

                // Arrange
                _context.BlockedCredentials.Add(new BlockedCredential() { Email = "blocked.test@email.com", Phone = "+0987654321" });
                _context.SaveChanges();

                // Arrange
                string credential = "notblocked.test@email.com";

                // Act
                var result = _repository.IsCredentialBlocked(credential);

                // Assert
                result.Should().BeFalse();

                // Clean
                _transaction.Rollback();
            });

        }


    }
}
