using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Devices.Interfaces;

namespace HeyaChat_Authorization.Repositories.Devices
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

        /// <summary>
        ///     Insert device to devices table and return id of the created row.
        /// </summary>
        /// <returns>deviceId of the created row. 0 if row could not be inserted.</returns>
        public long InsertDeviceToTable(Device device)
        {
            try
            {
                _context.Devices.Add(device);
                _context.SaveChanges();

                return device.DeviceId;
            }
            catch
            {
                return 0;
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
            catch
            {
                return false;
            }
        }

    }
}
