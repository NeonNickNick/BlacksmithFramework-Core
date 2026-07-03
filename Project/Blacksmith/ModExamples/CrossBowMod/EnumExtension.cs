using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;

namespace ModExamples.CrossBowMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Bolt(this ResourceType resourceType) => ResourceType.GetCEValue();
    }

}
