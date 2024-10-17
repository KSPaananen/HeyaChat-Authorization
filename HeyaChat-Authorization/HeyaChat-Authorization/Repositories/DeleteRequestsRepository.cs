using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;

namespace HeyaChat_Authorization.Repositories
{
    public class DeleteRequestsRepository : IDeleteRequestsRepository
    {
        private AuthorizationDBContext _context;

        public DeleteRequestsRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public DeleteRequest GetDeleteRequestByUserId(long userId)
        {
            try
            {
                var result = (from req in _context.DeleteRequests
                              where req.UserId == userId && req.Fulfilled == false
                              select req).SingleOrDefault() ?? new DeleteRequest();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long InsertDeleteRequest(DeleteRequest request)
        {
            try
            {
                _context.DeleteRequests.Add(request);
                _context.SaveChanges();

                return request.DeleteId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void DeleteDeleteRequest(long userId)
        {
            try
            {
                var result = (from req in _context.DeleteRequests
                              where req.UserId == userId && req.Fulfilled == false
                              select req).SingleOrDefault() ?? null;

                if (result != null)
                {
                    _context.DeleteRequests.Remove(result);
                    _context.SaveChanges();
                }

                throw new Exception("Could not delete DeleteRequest found with provided userId.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

       
    }
}
