using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.Repositories
{
    /// <summary>
    ///     <para>This table is indexed with UserId.</para>
    /// </summary>
    public class CodesRepository : ICodesRepository
    {
        private AuthorizationDBContext _context;

        public CodesRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public Codes GetCodeByUserId(long userId)
        {
            try
            {
                var result = (from code in _context.Codess
                             where code.UserId == userId && code.ExpiresAt > DateTime.UtcNow
                             select code).FirstOrDefault() ?? new Codes();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Codes GetValidCodeWithUserIdAndCode(long userId, string code)
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

        public long InsertCode(Codes code)
        {
            try
            {
                _context.Codess.Add(code);
                _context.SaveChanges();

                return code.CodeId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long UpdateCode(Codes code)
        {
            try
            {
                _context.Attach(code);
                _context.Entry(code).State = EntityState.Modified;
                int affectedRows = _context.SaveChanges();

                if (affectedRows <= 0)
                {
                    throw new Exception($"Code with the ID {code.CodeId} could not be updated.");
                }

                return code.CodeId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
