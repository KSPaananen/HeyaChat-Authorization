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

            Token rowToken = new Token
            {
                Identifier = jti,
                ExpiresAt = DateTime.UtcNow + lifetime,
                Active = true,
                DeviceId = deviceId,
            };

            _tokensRepository.InsertToken(rowToken);

            return tokenHandler.WriteToken(token);
        }

        public string RenewToken(long userId, long deviceId, string type, Guid oldJti)
        {
            // Invalidate old token with identifier
            long result = InvalidateToken(oldJti);

            // Generate a new token to user if Invalidation was succesful
            if (result > 0)
            {
                string newToken = GenerateToken(userId, deviceId, type);

                return newToken;
            }

            return "";
        }

        public long InvalidateToken(Guid identifier)
        {
            long result = _tokensRepository.InvalidateToken(identifier);

            return result;
        }

        public void InvalidateAllTokens(long userId)
        {
            _tokensRepository.InvalidateAllTokens(userId);
        }

        public (bool isValid, bool expiresSoon) VerifyToken(Guid jti, UserDevice device)
        {
            // IsTokenValid will either return row of the valid token or new object
            Token token = _tokensRepository.IsTokenValid(jti, device);

            if (token.TokenId != 0)
            {
                TimeSpan renewtime = _configurationRepository.GetTokenRenewTime();

                // If token is about to expire, let next layer know they need to renew their token
                if (token.ExpiresAt < DateTime.UtcNow + renewtime && token.ExpiresAt > DateTime.UtcNow)
                {
                    return (isValid: true, expiresSoon: true);
                }

                return (isValid: true, expiresSoon: false);
            }

            return (isValid: false, expiresSoon: false);
        }

        public (Guid jti, long userId, string type) GetClaims(HttpRequest request)
        {
            // Get token from authorization header
            string token = request.Headers.Authorization.ToString();

            // Sanitize token string
            if (token.ToLower().Contains("bearer"))
            {
                token = token.Substring(token.IndexOf(" ") + 1);
            }

            JwtSecurityToken securityToken = new JwtSecurityToken();

            securityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Get claims fron securityToken. Check https://datatracker.ietf.org/doc/html/rfc7519#section-4.1
            string jti = securityToken.Claims.Single(c => c.Type == "jti").ToString();
            string userId = securityToken.Claims.Single(c => c.Type == "sub").ToString();
            string type = securityToken.Claims.Single(c => c.Type == "typ").ToString();

            // Clean up strings because now they are like "jti: ?oalbdw=fssd134"
            jti = jti.Substring(jti.IndexOf(" ") + 1);
            userId = userId.Substring(userId.IndexOf(" ") + 1);
            type = type.Substring(type.IndexOf(" ") + 1);

            // Decrypt values
            string decryptedUserId = DecryptClaim(userId);
            string decryptedType = DecryptClaim(type);

            // Convert claims to their right types
            Guid convJti = Guid.Parse(jti);
            long convUserId = long.Parse(decryptedUserId);

            return (jti: convJti, userId: convUserId, type: decryptedType);
        }

        public string EncryptClaim(string value)
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

        public string DecryptClaim(string value)
        {
            byte[] key = _configurationRepository.GetEncryptionKey();
            byte[] cipherTextCombined = Convert.FromBase64String(value);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;

                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(cipherTextCombined, iv, iv.Length);
                aesAlg.IV = iv;

                byte[] cipherText = new byte[cipherTextCombined.Length - iv.Length];
                Array.Copy(cipherTextCombined, iv.Length, cipherText, 0, cipherText.Length);

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}
