using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;

namespace ModExamples.WineGlassMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Wine(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
}
