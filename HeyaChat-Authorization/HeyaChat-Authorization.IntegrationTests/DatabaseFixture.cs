using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace HeyaChat_Authorization.IntegrationTests
{
    public class DatabaseFixture : IDisposable
    {
        public AuthorizationDBContext _context { get; set; }

        private IConfiguration _configuration;
        private ConfigurationRepository _repository;

        public DatabaseFixture()
        {
            // Load configuration from appsettings.test
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            // Remember to set "Copy to output directory" from appsettings.test.json properties to "copy always"
            .AddJsonFile("appsettings.test.json", false, true)
            .Build();

            _repository = new ConfigurationRepository(_configuration);

            // Configure a new authorizationDbContext that uses test db
            var options = new DbContextOptionsBuilder<AuthorizationDBContext>()
                .UseNpgsql(_repository.GetAzurePostGreSqlServerConnectionString())
                .Options;

            _context = new AuthorizationDBContext(_repository, options);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
