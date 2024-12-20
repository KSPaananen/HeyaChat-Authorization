﻿using HeyaChat_Authorization.Models;

namespace HeyaChat_Authorization.Repositories.Interfaces
{
    public interface IDevicesRepository
    {
        Device GetDeviceWithUUID(Guid UUID);
        // Returns ID of a created row
        long InsertDevice(Device device);

        (long deviceId, bool alreadyExisted) InsertDeviceIfDoesntExist(Device device);

        // Returns ID of a created row
        long UpdateDevice(Device device);

        void DeleteDevice(long deviceId);
    }
}
