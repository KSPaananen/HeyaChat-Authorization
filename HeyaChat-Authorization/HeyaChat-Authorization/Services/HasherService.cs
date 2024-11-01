using HeyaChat_Authorization.Services.Interfaces;
using System.Security.Cryptography;

namespace HeyaChat_Authorization.Services
{
    public class HasherService : IHasherService
    {
        private static readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;

        private const int keySize = 256 / 8;
        private const int iterations = 10000;

        public byte[] GenerateSalt()
        {
            int size = 128 / 8;

            byte[] salt = RandomNumberGenerator.GetBytes(size);

            return salt;
        }

        public string Hash(string password, byte[] salt)
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);

            string base64String = Convert.ToBase64String(hash);

            return base64String;
        }

        // Verify hashed password
        public bool Verify(byte[] salt, string hash, string passwordString)
        {
            // Transform hash to bytes
            byte[] hashBytes = Convert.FromBase64String(hash);

            byte[] hashInput = Rfc2898DeriveBytes.Pbkdf2(passwordString, salt, iterations, hashAlgorithm, keySize);

            return CryptographicOperations.FixedTimeEquals(hashBytes, hashInput);
        }

    }
}
