using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;

namespace ModExamples.PhantomBookMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Dream(this ResourceType resourceType) => ResourceType.GetCEValue("Dream");
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Spirit(this ResourceType resourceType) => ResourceType.GetCEValue("Spirit");
    }
}
