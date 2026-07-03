using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public class EffectType : BlacksmithEnum<EffectType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue AfterAnalyzableDataWritten() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue AfterTransport() => GetCEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue AfterResult() => GetCEValue();
    }
    public partial class EffectEntity : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required EffectTargetType.CEValue TargetType { get; init; }
        public required EffectType.CEValue Type { get; init; }
        public required ClapRoundClock Clock { get; init; }
    }
}