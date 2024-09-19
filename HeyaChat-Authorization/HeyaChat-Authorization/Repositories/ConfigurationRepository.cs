using HeyaChat_Authorization.Repositories.Interfaces;
using System;
using System.Text;

namespace HeyaChat_Authorization.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private IConfiguration? _config;

        public ConfigurationRepository(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public TimeSpan GetTokenLifeTime()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            if (TimeSpan.TryParse(_config.GetSection("jwt:lifetime").Value, out TimeSpan result))
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read lifetime in jwt from configuration.");
        }

        public TimeSpan GetTokenRenewTime()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            if (TimeSpan.TryParse(_config.GetSection("jwt:renewtime").Value, out TimeSpan result))
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read renewtime in jwt from configuration.");
        }

        public byte[] GetSigningKey()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            byte[] result = Encoding.UTF8.GetBytes(_config.GetSection($"jwt:signingkey").Value!);

            if (result.Length > 0)
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read signingkey in jwt from configuration.");
        }

        public string GetIssuer()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            string result = _config.GetSection("jwt:issuer").Value ?? "";

            if (result != "")
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read issuer in jwt from configuration.");
        }

        public string GetAudience()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            string result = _config.GetSection("jwt:audience").Value ?? "";

            if (result != "")
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read issuer in jwt from configuration.");
        }

        public byte[] GetEncryptionKey()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            byte[] result = Encoding.UTF8.GetBytes(_config.GetSection($"encryption:key").Value!);

            if (result.Length > 0)
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read key in encryption from configuration.");
        }

        public string GetConnectionString()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            // Use GetSection since connectionstrins is lowercase in appsettings
            string result = _config.GetConnectionString("postgresqlserver") ?? "";

            if (result != "")
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read connection string in connectionstrings from configuration.");
        }

        public string GetEmailSender()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            string result = _config.GetSection("emailService:sender").Value ?? "";

            if (result != "")
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read sender in emailService from configuration.");
        }

        public string GetEmailPassword()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            string result = _config.GetSection("emailService:password").Value ?? "";

            if (result != "")
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read password in emailService from configuration.");
        }

        public string GetEmailHost()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            string result = _config.GetSection("emailService:host").Value ?? "";

            if (result != "")
            {
                return result;
            }

            throw new NullReferenceException($"Unable to read host in emailService from configuration.");
        }

        public int GetEmailPort()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            string result = _config.GetSection("emailService:port").Value ?? "";

            if (result != "")
            {
                return int.Parse(result);
            }

            throw new NullReferenceException($"Unable to read port in emailService from configuration.");
        }

        public TimeSpan GetCodeLifeTime()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            string result = _config.GetSection("codes:lifetime").Value ?? "";

            if (result != "")
            {
                return TimeSpan.Parse(result);
            }

            throw new NullReferenceException($"Unable to read lifetime in codesfrom configuration.");
        }
    }
}
