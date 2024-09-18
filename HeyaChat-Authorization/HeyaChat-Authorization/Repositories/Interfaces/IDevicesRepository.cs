using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IDevicesRepository
    {
        // Returns ID of a created row
        long InsertDeviceToTable(Device device);

        // Returns boolean based on if deletion was succesful
        bool DeleteDevice(long deviceId);
    }
}
