using HeyaChat_Authorization.Repositories.Configuration;
using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace HeyaChat_Authorization.Services
{
    public class JwtService : IJwtService
    {
        private IConfiguration _config;
        private ConfigurationRepository _repository;

        public JwtService(IConfiguration config)
        {
            _config = config;
            _repository = new ConfigurationRepository(_config);
        }

        // userID self explanatory
        // type defines what token can be used for. Types: "login" "password" "suspended"
        public string GenerateToken(int userID, string type)
        {
            // Get required values from repository for creating jwt
            TimeSpan lifetime = _repository.GetTokenLifeTimeFromConfiguration();
            byte[] signingKey = _repository.GetSigningKeyFromConfiguration();
            string issuer = _repository.GetIssuerFromConfiguration();
            string audience = _repository.GetAudienceFromConfiguration();

            // Encrypt userID & token type before appending to claims
            string encryptedUserID = EncryptClaim(userID.ToString());
            string encryptedType = EncryptClaim(type);

            // Set claims
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Tokens identifier
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

            return tokenHandler.WriteToken(token);
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

            JwtSecurityToken securityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Get claims fron securityToken
            // https://datatracker.ietf.org/doc/html/rfc7519#section-4.1
            string jti = securityToken.Claims.Single(c => c.Type == "jti").ToString();
            string userID = securityToken.Claims.Single(c => c.Type == "sub").ToString();
            string type = securityToken.Claims.Single(c => c.Type == "syp").ToString();

            // Sanitize strings
            jti = jti.Substring(jti.IndexOf(" ") + 1);
            userID = userID.Substring(userID.IndexOf(" ") + 1);
            type = type.Substring(type.IndexOf(" ") + 1);

            return new List<string> { jti, userID, type };
        }

        private string EncryptClaim(string value)
        {
            byte[] key = _repository.GetEncryptionKeyFromConfiguration();

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV();

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length); // Prepend IV
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
