using FakeItEasy;
using FluentAssertions;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HeyaChat_Authorization.Tests.Services
{
    public class MessageServiceTests
    {
        private IFileSystem _fileSystem;

        private IConfigurationRepository _configurationRepository;
        private ICodesRepository _codesRepository;

        private SmtpClient _client;

        private MessageService _service;

        public MessageServiceTests()
        {
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { "/app/EmailTemplates/RecoveryEmail.html", new MockFileData("<html><body>Recovery Code: {{code}}</body></html>") },
                { "/app/EmailTemplates/VerificationEmail.html", new MockFileData("<html><body>Verification Code: {{code}}</body></html>") }
            });

            _configurationRepository = A.Fake<IConfigurationRepository>();
            _codesRepository = A.Fake<ICodesRepository>();

            _client = A.Fake<SmtpClient>();

            A.CallTo(() => _configurationRepository.GetEmailPort()).Returns(587);
            A.CallTo(() => _configurationRepository.GetEmailHost()).Returns("smtp.example.com");
            A.CallTo(() => _configurationRepository.GetEmailSender()).Returns("test.email@example.com");
            A.CallTo(() => _configurationRepository.GetEmailPassword()).Returns("testpassword");
            A.CallTo(() => _configurationRepository.GetCodeLifeTime()).Returns(TimeSpan.FromMinutes(10));

            _service = new MessageService(_configurationRepository, _codesRepository, _fileSystem);
        }

        [Fact]
        public void MessageService_SendRecoveryEmail_ShouldSucceed()
        {
            // Arrange
            long userId = 123;
            string email = "user@example.com";

            A.CallTo(() => _codesRepository.InsertCode(A<Codes>._)).Returns(1000);

            // Act
            var result = _service.SendRecoveryEmail(userId, email);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _codesRepository.InsertCode(A<Codes>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void MessageService_SendVerificationEmail_ShouldSucceed()
        {
            // Arrange
            long userId = 123;
            string email = "user@example.com";

            A.CallTo(() => _codesRepository.InsertCode(A<Codes>._)).Returns(1000);

            // Act
            var result = _service.SendVerificationEmail(userId, email);

            // Assert
            result.Should().NotBeNull();
            A.CallTo(() => _codesRepository.InsertCode(A<Codes>.Ignored)).MustHaveHappenedOnceExactly();
        
        }
    }
}
