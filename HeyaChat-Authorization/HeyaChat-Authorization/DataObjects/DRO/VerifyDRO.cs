using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class VerifyDRO
    {
        public string Code { get; set; } = "";

        public string Email { get; set; } = "";

        public UserDevice Device { get; set; } = null!;
    }
}
