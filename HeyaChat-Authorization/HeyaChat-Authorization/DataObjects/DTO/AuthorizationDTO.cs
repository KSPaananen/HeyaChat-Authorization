namespace HeyaChat_Authorization.DataObjects.DTO
{
    public class AuthorizationDTO
    {
        public string Status { get; set; } = null!;

        public int Code { get; set; }

        public string Details { get; set; } = null!;

    }
}
