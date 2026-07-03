using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public class ResourceType : BlacksmithEnum<ResourceType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Iron() => GetCEValue();
        [IsBlacksmithEnumMember(1)]
        public CEValue Gold_Iron() => GetCEValue();
        [IsBlacksmithEnumMember(2)]
        public CEValue Space() => GetCEValue();
        [IsBlacksmithEnumMember(3)]
        public CEValue Time() => GetCEValue();
        [IsBlacksmithEnumMember(4)]
        public CEValue Magic() => GetCEValue();
    }
    public partial class ResourceAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required ResourceType.CEValue Type { get; init; }
        public required float Power { get; set; }
    }
}
