using BlacksmithCore.Infrastructure.Attributes.Analyzer;
using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public class EffectTargetType : BlacksmithEnum<EffectTargetType>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue Self() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue Enemy() => GetCEValue();
    }
    [IsManual]
    public partial class EffectAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required EffectEntity EffectEntity { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public EffectAnalyzableData Copy()
        {
            return new()
            {
                AnalyzerKey = AnalyzerKey,
                EffectEntity = EffectEntity.Copy(),
                Clock = Clock.Copy()
            };
        }
    }
}