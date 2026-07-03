using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;

namespace ModExamples.HolyBookMod
{
    [IsBlacksmithEnumModifier]
    public static class ResourceExtension
    {
        [IsBlacksmithEnumMember(0)]
        public static ResourceType.CEValue Cross(this ResourceType resourceType) => ResourceType.GetCEValue();
    }
    [IsBlacksmithEnumModifier]
    public static class DefenseExtension
    {

        [IsBlacksmithEnumMember(32768)]
        public static DefenseType.CEValue GreyHP(this DefenseType defenseType) => DefenseType.GetCEValue();
    }
}
