using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Configuration.Interfaces;
using HeyaChat_Authorization.Repositories.MfaCodes.Interfaces;

namespace HeyaChat_Authorization.Repositories.MfaCodes
{
    /// <summary>
    ///     <para>This table is indexed with UserId.</para>
    /// </summary>
    public class CodesRepository : ICodesRepository
    {
        private AuthorizationDBContext _context;
        private IConfigurationRepository _repository;

        public CodesRepository(AuthorizationDBContext context, IConfigurationRepository repository)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
            _repository = repository ?? throw new NullReferenceException(nameof(repository));
        }

        /// <summary>
        ///     <para></para>
        /// </summary>
        /// <returns>ID of the created row. 0 if there was an issue.</returns>
        public long InsertCode(long userId, string code)
        {
            try
            {
                TimeSpan lifetime = _repository.GetCodeLifeTime();

                MfaCode newCode = new MfaCode
                {
                    Code = code,
                    ExpiresAt = DateTime.UtcNow + lifetime,
                    Used = false,
                    UserId = userId,
                };

                _context.MfaCodes.Add(newCode);
                _context.SaveChanges();

                return newCode.CodeId;
            }
            catch
            {
                return 0;
            }
        }
    }
}
