using BlacksmithCore.Infrastructure.Attributes.Analyzer;
using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public partial class AttackType : BlacksmithEnum<AttackType>
    {
        [IsBlacksmithEnumMember(256)]
        public CEValue Physical() => _Physical_GetOrCreate();
        [IsBlacksmithEnumMember(128)]
        public CEValue Magical() => _Magical_GetOrCreate();
        [IsBlacksmithEnumMember(0)]
        public CEValue Real() => _Real_GetOrCreate();
    }
    public partial class AttackStage : BlacksmithEnum<AttackStage>
    {
        [IsBlacksmithEnumMember(0)]
        public CEValue OnHitArmorFirstTime() => _OnHitArmorFirstTime_GetOrCreate();
        [IsBlacksmithEnumMember(1)]
        public CEValue OnHitBody() => _OnHitBody_GetOrCreate();
        [IsBlacksmithEnumMember(2)]
        public CEValue OnEnd() => _OnEnd_GetOrCreate();
    }
    [IsManual]
    public partial class AttackAnalyzableData : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required AttackType.CEValue Type { get; init; }
        public required int Power { get; set; }
        public required float APFactor { get; init; }
        public int TotalDamage { get; set; } = 0;
        public Dictionary<AttackStage.CEValue, List<string>> StageKeys { get; init; } = new();
        public Dictionary<string, float> ExtraParams { get; init; } = new();
        public AttackAnalyzableData Copy()
        {
            return new()
            {
                AnalyzerKey = AnalyzerKey,
                Clock = Clock.Copy(),
                Type = Type,
                Power = Power,
                APFactor = APFactor,
                TotalDamage = TotalDamage,
                StageKeys = StageKeys.ToDictionary(
                    s => s.Key,
                    s => new List<string>(s.Value)),
                ExtraParams = ExtraParams.ToDictionary()
            };
        }
    }
}