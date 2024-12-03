using FluentAssertions;
using HeyaChat_Authorization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyaChat_Authorization.Tests.Services
{
    public class HasherServiceTests
    {

        private HasherService _service;
        public HasherServiceTests()
        {
            _service = new HasherService();
        }

        [Fact]
        public void HasherService_GenerateSalt_ShouldSucceed()
        {
            // Arrange

            // Act
            var result = _service.GenerateSalt();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void HasherService_Hash_ShouldSucceed()
        {
            // Arrange
            string password = "testpassword";
            byte[] salt = new byte[32];

            // Act
            var result = _service.Hash(password, salt);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public void HasherService_Verify_ShouldSucceed()
        {
            // Arrange
            string password = "testpassword";
            string hash = "b2eFnPbA/guK6fHdJT+w0ahwDya/XBIQ1iTJ2/jJg4Y=";
            byte[] salt = new byte[32];

            // Act
            var result = _service.Verify(salt, hash, password);

            // Assert
            result.Should().Be(true);  
        }

        [Fact]
        public void HasherService_Verify_ShouldFail()
        {
            // Arrange
            string password = "testpassword";
            string hash = "b2eFnPbA/guK6fHdJT+w0ahfgthtffghXBIQ1iTJ2/sfg4Y=";
            byte[] salt = new byte[32];

            // Act
            var result = _service.Verify(salt, hash, password);

            // Assert
            result.Should().Be(false);
        }


    }
}
