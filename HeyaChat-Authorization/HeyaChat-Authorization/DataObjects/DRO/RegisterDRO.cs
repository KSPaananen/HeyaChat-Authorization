namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class RegisterDRO
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DeviceData Device { get; set; } = null!;
    }

    public class DeviceData
    { 
        public string DeviceName { get; set; } = null!;

        public Guid? DeviceIdentifier { get; set; }

        public string CountryTag { get; set; } = null!;
    }
}
