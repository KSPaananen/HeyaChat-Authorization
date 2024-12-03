using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class AddBiometricsKey
    {
        public byte[] BiometricsKey { get; set; } = null!;

        public UserDevice Device { get; set; } = null!;
    }
}
