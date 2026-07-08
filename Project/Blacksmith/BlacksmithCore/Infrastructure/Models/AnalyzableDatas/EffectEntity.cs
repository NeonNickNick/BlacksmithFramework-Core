using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public partial class EffectType : BlacksmithEnum<EffectType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue AfterAnalyzableDataWritten() => _AfterAnalyzableDataWritten_GetOrCreate();
        [IsBlacksmithEnumMember(8)]
        public CEValue AfterTransport() => _AfterTransport_GetOrCreate();
        [IsBlacksmithEnumMember(16)]
        public CEValue AfterResult() => _AfterResult_GetOrCreate();
    }
    public partial class EffectEntity : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required EffectTargetType.CEValue TargetType { get; init; }
        public required EffectType.CEValue Type { get; init; }
        public required ClapRoundClock Clock { get; init; }
    }
}