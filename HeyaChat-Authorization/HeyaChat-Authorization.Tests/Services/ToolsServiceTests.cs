using FluentAssertions;
using HeyaChat_Authorization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeyaChat_Authorization.Tests.Services
{
    public class ToolsServiceTests
    {
        private ToolsService _service;

        public ToolsServiceTests()
        {
            _service = new ToolsService();
        }

        [Theory]
        [InlineData("1234567890", "1234*****0")]
        [InlineData("+1234567890", "+123******0")]
        public void ToolsService_MaskPhoneNumber_MasksCorrectly(string input, string expected)
        {
            // Arrange

            // Act
            var result = _service.MaskPhoneNumber(input);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("test.email@example.com", "t***.e***@*******.***")]
        [InlineData("testemail@example.com", "t********@*******.***")]
        public void ToolsService_MaskEmail_MasksCorrectly(string input, string expected)
        {
            // Arrange

            // Act
            var result = _service.MaskEmail(input);

            // Assert
            result.Should().Be(expected);
        }


    }
}
