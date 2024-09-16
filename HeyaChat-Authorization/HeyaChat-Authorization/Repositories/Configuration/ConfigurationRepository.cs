using HeyaChat_Authorization.Repositories.Configuration.Interfaces;
using System.Text;

namespace HeyaChat_Authorization.Repositories.Configuration
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private static IConfiguration? _config;

        public ConfigurationRepository(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public TimeSpan GetTokenLifeTimeFromConfiguration()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            if (TimeSpan.TryParse(_config.GetSection("jwt:lifetime").Value, out TimeSpan result))
            {
                return result;
            }

            throw new FormatException($"Unable to read lifetime in jwt from configuration.");
        }

        public byte[] GetSigningKeyFromConfiguration()
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

            throw new FormatException($"Unable to read signingkey in jwt from configuration.");
        }

        public string GetIssuerFromConfiguration()
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

            throw new FormatException($"Unable to read issuer in jwt from configuration.");
        }

        public string GetAudienceFromConfiguration()
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

            throw new FormatException($"Unable to read issuer in jwt from configuration.");
        }

        public byte[] GetEncryptionKeyFromConfiguration()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            byte[] result = Encoding.UTF8.GetBytes(_config.GetSection($"jwt:encryptionkey").Value!);

            if (result.Length > 0)
            {
                return result;
            }

            throw new FormatException($"Unable to read encryption key in jwt from configuration.");
        }

        public string GetConnectionStringFromConfiguration()
        {
            if (_config == null)
            {
                throw new NullReferenceException("Configuration is null.");
            }

            // Use GetSection since connectionstrins is lowercase in appsettings
            string result = _config.GetSection("connectionstrings:postgresqlserver").Value ?? "";

            if (result != "")
            {
                return result;
            }

            throw new FormatException($"Unable to read connection string in connectionstrings from configuration.");
        }

    }
}
