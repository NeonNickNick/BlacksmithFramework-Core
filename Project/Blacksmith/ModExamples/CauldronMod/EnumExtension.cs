using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;

namespace ModExamples.CauldronMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Fire(this ResourceType resourceType) => ResourceType.GetCEValue("Fire");
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Water(this ResourceType resourceType) => ResourceType.GetCEValue("Water");
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Wood(this ResourceType resourceType) => ResourceType.GetCEValue("Wood");
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Earth(this ResourceType resourceType) => ResourceType.GetCEValue("Earth");
    }
}
