using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;

namespace HeyaChat_Authorization.Repositories
{
    /// <summary>
    ///     <para>This table is indexed with UserId.</para>
    /// </summary>
    public class CodesRepository : ICodesRepository
    {
        private AuthorizationDBContext _context;
        private IConfigurationRepository _configurationRepository;

        public CodesRepository(AuthorizationDBContext context, IConfigurationRepository configurationRepository)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
            _configurationRepository = configurationRepository ?? throw new NullReferenceException(nameof(configurationRepository));
        }

        public long InsertCode(long userId, string code)
        {
            try
            {
                TimeSpan lifetime = _configurationRepository.GetCodeLifeTime();

                Codes newCode = new Codes
                {
                    Code = code,
                    ExpiresAt = DateTime.UtcNow + lifetime,
                    Used = false,
                    UserId = userId,
                };

                _context.Codess.Add(newCode);
                _context.SaveChanges();

                return newCode.CodeId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Codes IsCodeValid(long userId, string code)
        {
            try
            {
                Codes result = (from codes in _context.Codess
                             where codes.UserId == userId && codes.Code == code && codes.Used == false && codes.ExpiresAt > DateTime.UtcNow
                             select codes).FirstOrDefault() ?? new Codes();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long MarkCodeAsUsed(long codeId)
        {
            try
            {
                var result = (from code in _context.Codess
                               where code.CodeId == codeId
                               select code).FirstOrDefault() ?? null;

                if (result != null)
                {
                    result.Used = true;
                    _context.SaveChanges();

                    return result.CodeId;
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
