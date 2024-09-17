using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Devices.Interfaces
{
    public interface IDevicesRepository
    {
        long InsertDeviceToTable(Models.Devices device);

        bool DeleteDevice(long deviceId);
    }
}
