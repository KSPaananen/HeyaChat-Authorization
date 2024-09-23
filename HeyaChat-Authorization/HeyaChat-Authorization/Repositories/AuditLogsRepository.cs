using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeyaChat_Authorization.Repositories
{
    public class AuditLogsRepository : IAuditLogsRepository
    {
        private AuthorizationDBContext _context;

        public AuditLogsRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public long InsertAuditLog(long deviceId, int type)
        {
            try
            {
                string action = "";

                switch (type)
                {
                    case 0:
                        action = "Logged in from a new device.";
                        break;
                    case 1:
                        action = "Changed account password";
                        break;
                }

                AuditLog log = new AuditLog
                {
                    PerformedAction = action,
                    // PerformedAt is handled by the database
                    DeviceId = deviceId,
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();

                return log.LogId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
