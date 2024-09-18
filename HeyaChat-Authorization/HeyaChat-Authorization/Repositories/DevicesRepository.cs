using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;

namespace HeyaChat_Authorization.Repositories
{
    /// <summary>
    ///     <para>This table is indexed with UserId.</para>
    /// </summary>
    public class DevicesRepository : IDevicesRepository
    {
        private AuthorizationDBContext _context;

        public DevicesRepository(AuthorizationDBContext context)
        {
            _context = context ?? throw new NullReferenceException(nameof(context));
        }

        public long InsertDeviceToTable(Device device)
        {
            try
            {
                _context.Devices.Add(device);
                _context.SaveChanges();

                return device.DeviceId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool DeleteDevice(long deviceId)
        {
            try
            {
                var result = (from device in _context.Devices
                              where device.DeviceId == deviceId
                              select device).SingleOrDefault() ?? null;

                if (result != null)
                {
                    _context.Devices.Remove(result);
                    _context.SaveChanges();
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
