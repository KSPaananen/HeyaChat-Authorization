using HeyaChat_Authorization.Models;
using HeyaChat_Authorization.Models.Context;
using HeyaChat_Authorization.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public Device GetDeviceWithUUID(Guid UUID)
        {
            try
            {
                Device result = (from device in _context.Devices
                              where device.DeviceIdentifier == UUID
                              select device).FirstOrDefault() ?? new Device();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long InsertDevice(Device device)
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

        public (long deviceId, bool alreadyExisted) InsertDeviceIfDoesntExist(Device device)
        {
            try
            {
                // See if device exists
                var resultDevice = GetDeviceWithUUID(device.DeviceIdentifier);

                if (resultDevice.DeviceId > 0)
                {
                    return (deviceId: resultDevice.DeviceId, alreadyExisted: true);
                }
                else
                {
                    long insertedId = InsertDevice(device);
                    return (deviceId: insertedId, alreadyExisted: false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public long UpdateDevice(Device device)
        {
            try
            {
                _context.Attach(device);
                _context.Entry(device).State = EntityState.Modified;
                int affectedRows = _context.SaveChanges();

                if (affectedRows <= 0)
                {
                    throw new Exception($"User with the ID {device.DeviceId} could not be updated.");
                }

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
