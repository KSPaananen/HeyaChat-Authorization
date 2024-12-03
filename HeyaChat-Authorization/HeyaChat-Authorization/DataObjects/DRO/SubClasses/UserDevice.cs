namespace HeyaChat_Authorization.DataObjects.DRO.SubClasses
{
    public class UserDevice
    {
        public string DeviceName { get; set; } = "";

        public Guid DeviceIdentifier { get; set; }

        public string CountryCode { get; set; } = "";
    }
}
