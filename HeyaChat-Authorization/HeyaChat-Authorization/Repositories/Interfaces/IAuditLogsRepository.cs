namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IAuditLogsRepository
    {
        // Types
        // 0: Logged in from a new device
        // 1: Changed password

        long InsertAuditLog(long deviceId, int type);
    }
}
