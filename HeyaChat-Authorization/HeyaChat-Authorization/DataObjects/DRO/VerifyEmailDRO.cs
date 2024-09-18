using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class VerifyEmailDRO
    {
        public string Code { get; set; } = null!;

        public UserDevice Device {  get; set; } = null!;
    }
}
