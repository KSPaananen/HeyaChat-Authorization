using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Devices.Interfaces
{
    public interface IDevicesRepository
    {
        long InsertDeviceToTable(Device device);

        bool DeleteDevice(long deviceId);
    }
}
