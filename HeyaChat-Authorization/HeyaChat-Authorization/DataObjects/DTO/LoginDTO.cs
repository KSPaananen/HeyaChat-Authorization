using HeyaChat_Authorization.DataObjects.DTO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DTO
{
    public class LoginDTO
    {
        public string? Contact { get; set; }

        public SuspensionDTO Suspension { get; set; } = null!;

        public DetailsDTO Details { get; set; } = null!;
    }
}
