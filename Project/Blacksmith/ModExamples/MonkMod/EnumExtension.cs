using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;

namespace ModExamples.MonkMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Jade(this ResourceType resourceType) => ResourceType.GetCEValue("Jade");
    }
}
