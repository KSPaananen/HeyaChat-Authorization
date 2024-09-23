namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IAuditLogsRepository
    {
        // Types
        // 0: Logged in from a new device
        // 1: Changed password

        // Returns the id of a created row
        long InsertAuditLog(long deviceId, int type);
    }
}
