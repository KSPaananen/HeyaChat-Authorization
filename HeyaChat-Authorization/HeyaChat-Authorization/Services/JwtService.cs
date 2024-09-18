using HeyaChat_Authorization.DataObjects.DRO.SubClasses;
using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Repositories.Interfaces;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HeyaChat_Authorization.Services
{
    public class JwtService : IJwtService
    {
        private IConfigurationRepository _configurationRepository;
        private ITokensRepository _tokensRepository;

        public JwtService(IConfigurationRepository configurationRepository, ITokensRepository tokensRepository)
        {
            _configurationRepository = configurationRepository ?? throw new NullReferenceException(nameof(configurationRepository));
            _tokensRepository = tokensRepository ?? throw new NullReferenceException(nameof(tokensRepository));
        }

        // userID self explanatory
        // type defines what token can be used for. Types: "login" "password" "suspended"
        public string GenerateToken(long userId, long deviceId, string type)
        {
            // Get required values from repository for creating jwt
            TimeSpan lifetime = _configurationRepository.GetTokenLifeTime();
            byte[] signingKey = _configurationRepository.GetSigningKey();
            string issuer = _configurationRepository.GetIssuer();
            string audience = _configurationRepository.GetAudience();

            // Encrypt userID & token type before appending to claims
            string encryptedUserID = EncryptClaim(userId.ToString());
            string encryptedType = EncryptClaim(type);

            // Generate JTI (Json token identifier)
            Guid jti = Guid.NewGuid();

            // Set claims
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()), // Tokens identifier
                new Claim(JwtRegisteredClaimNames.Sub, encryptedUserID), // userID 
                new Claim(JwtRegisteredClaimNames.Typ, encryptedType) // Token type
            };

            // Configure token
            SecurityTokenDescriptor tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(lifetime),
                IssuedAt = DateTime.UtcNow,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(tokenDesc);

            // Create new Token object with generated jti and insert to DB
            try
            {
                Token rowToken = new Token
                {
                    Identifier = jti,
                    ExpiresAt = DateTime.UtcNow + lifetime,
                    Active = true,
                    DeviceId = deviceId,
                };

                _tokensRepository.InsertToken(rowToken);
            }
            catch (Exception ex)
            {
                // Log error here

            }
           
            return tokenHandler.WriteToken(token);
        }

        public long InvalidateToken()
        {
            throw new NotImplementedException();
        }

        public List<string> GetClaims(HttpRequest request)
        {
            // Get token from authorization header
            string token = "";

            if (request.Headers.Authorization.ToString() != "")
            {
                token = request.Headers.Authorization.ToString();
            }

            // Sanitize token string
            if (token.ToLower().Contains("bearer"))
            {
                token = token.Substring(token.IndexOf(" ") + 1);
            }

            JwtSecurityToken securityToken = new JwtSecurityToken();

            try
            {
                securityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            }
            catch
            {
                // Log error here

                // Return empty list if token is empty
                return new List<string>();
            }

            // Get claims fron securityToken
            // https://datatracker.ietf.org/doc/html/rfc7519#section-4.1
            string jti = securityToken.Claims.Single(c => c.Type == "jti").ToString();
            string userID = securityToken.Claims.Single(c => c.Type == "sub").ToString();
            string type = securityToken.Claims.Single(c => c.Type == "typ").ToString();

            // Sanitize strings
            jti = jti.Substring(jti.IndexOf(" ") + 1);
            userID = userID.Substring(userID.IndexOf(" ") + 1);
            type = type.Substring(type.IndexOf(" ") + 1);

            return new List<string> { jti, userID, type };
        }

        public bool VerifyToken(Guid jti, UserDevice device)
        {
            try
            {
                // Check if valid token can be found with tokensrepository
                bool isValid = _tokensRepository.IsTokenValid(jti, device);

                return isValid;
            }
            catch (Exception ex)
            {
                // Log error here


                return false;
            }
        }

        private string EncryptClaim(string value)
        {
            byte[] key = _configurationRepository.GetEncryptionKey();

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(value);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

    }
}
