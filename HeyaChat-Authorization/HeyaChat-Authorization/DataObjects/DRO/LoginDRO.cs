namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class LoginDRO
    {
        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string DeviceName { get; set; } = null!;

        public Guid? DeviceIdentifier { get; set; }

        public string CountryTag { get; set; } = null!;
    }


}
