using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using System.IO.Abstractions;
using System.Net;
using System.Net.Mail;

namespace HeyaChat_Authorization.Services
{
    public class MessageService : IMessageService
    {
        private IFileSystem _fileSystem;

        private IConfigurationRepository _configurationRepository;
        private ICodesRepository _codesRepository;

        private int port;
        private string host;
        private string sender;
        private string password;

        private string _folderPath;

        private SmtpClient _client;

        public MessageService(IConfigurationRepository configurationRepository, ICodesRepository codesRepository, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new NullReferenceException(nameof(fileSystem));

            _configurationRepository = configurationRepository ?? throw new NullReferenceException(nameof(configurationRepository));
            _codesRepository = codesRepository ?? throw new NullReferenceException(nameof(codesRepository));

            port = _configurationRepository.GetEmailPort();
            host = _configurationRepository.GetEmailHost();
            sender = configurationRepository.GetEmailSender();
            password = _configurationRepository.GetEmailPassword();

            _folderPath = $"/app/EmailTemplates";

            _client = new SmtpClient
            {
                Port = port,
                Host = host,
                EnableSsl = true,
                Credentials = new NetworkCredential(sender, password)
            };
        }

        public Task SendRecoveryEmail(long userId, string email)
        {
            // Generate code
            string code = GenerateCode();

            // Store code to DB
            Codes newCode = new Codes
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow + _configurationRepository.GetCodeLifeTime(),
                Used = false,
                UserId = userId,
            };

            long codeId = _codesRepository.InsertCode(newCode);

            // Set subject
            string subject = "Here's your account recovery code!";

            // Read html file to string
            string htmlBody = _fileSystem.File.ReadAllText($"{_folderPath}/RecoveryEmail.html");

            // Insert dynamic values to body string
            string body = htmlBody.Replace("{{code}}", code);

            try
            {
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(sender);
                    message.To.Add(email);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.Priority = MailPriority.Normal;

                    _client.SendMailAsync(message);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task SendVerificationEmail(long userId, string email)
        {
            // Generate code
            string code = GenerateCode();

            // Store code to DB
            Codes newCode = new Codes
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow + _configurationRepository.GetCodeLifeTime(),
                Used = false,
                UserId = userId,
            };

            long codeId = _codesRepository.InsertCode(newCode);

            // Set subject
            string subject = "Here's your verification code!";

            // Read html file to string
            string htmlBody = _fileSystem.File.ReadAllText($"{_folderPath}/VerificationEmail.html");

            // Insert dynamic values to body string
            string body = htmlBody.Replace("{{code}}", code);

            try
            {
                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(sender);
                    message.To.Add(email);
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.Priority = MailPriority.Normal;

                    _client.SendMailAsync(message);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Task SendVerificationTextMessage(long userId, string phone)
        {
            return Task.CompletedTask;
        }

        private string GenerateCode()
        {
            string charString = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] charArray = new char[8];
            var random = new Random();

            for (int i = 0; i < charArray.Length; i++)
            {
                charArray[i] = charString[random.Next(charString.Length)];
            }

            return new string(charArray);
        }

    }
}
