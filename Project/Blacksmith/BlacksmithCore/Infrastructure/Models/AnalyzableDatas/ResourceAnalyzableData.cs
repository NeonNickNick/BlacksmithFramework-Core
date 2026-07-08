using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public partial class ResourceType : BlacksmithEnum<ResourceType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Iron() => _Iron_GetOrCreate();
        [IsBlacksmithEnumMember(1)]
        public CEValue Gold_Iron() => _Gold_Iron_GetOrCreate();
        [IsBlacksmithEnumMember(2)]
        public CEValue Space() => _Space_GetOrCreate();
        [IsBlacksmithEnumMember(3)]
        public CEValue Time() => _Time_GetOrCreate();
        [IsBlacksmithEnumMember(4)]
        public CEValue Magic() => _Magic_GetOrCreate();
    }
    public partial class ResourceAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required ResourceType.CEValue Type { get; init; }
        public required float Power { get; set; }
    }
}
