using HeyaChat_Authorization.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace HeyaChat_Authorization.Services
{
    public class ProtectorService : IProtectorService
    {
        private IDataProtector _protector;

        public ProtectorService(IDataProtectionProvider protector)
        {
            _protector = protector.CreateProtector("HeyaChat.Auth");
        }

        public string ProtectData(string data)
        {
            var protectedData = _protector.Protect(data);

            return protectedData;
        }

        public string UnProtectData(string data)
        {
            try
            {
                var unprotectedData = _protector.Unprotect(data);

                return unprotectedData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
