﻿namespace HeyaChat_Authorization.DataObjects.DRO.SubClasses
{
    public class UserDevice
    {
        public string DeviceName { get; set; } = null!;

        public Guid DeviceIdentifier { get; set; } = new Guid();

        public string CountryTag { get; set; } = null!;
    }
}
