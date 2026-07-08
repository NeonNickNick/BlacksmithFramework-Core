using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;
using BlacksmithCore.Infrastructure.SkillSystem;
namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public partial class DefenseType : BlacksmithEnum<DefenseType>
    {
        [IsBlacksmithEnumMember(-16)]
        public DefenseType.CEValue PhysicalImmunity() => _PhysicalImmunity_GetOrCreate();
        [IsBlacksmithEnumMember(-16)]
        public DefenseType.CEValue MagicalImmunity() => _MagicalImmunity_GetOrCreate();
        [IsBlacksmithEnumMember(-8)]
        public DefenseType.CEValue PercentageReduction() => _PercentageReduction_GetOrCreate();
        [IsBlacksmithEnumMember(0)]
        public CEValue RealReduction() => _RealReduction_GetOrCreate();
        [IsBlacksmithEnumMember(8)]
        public CEValue ThornReduction() => _ThornReduction_GetOrCreate();
        [IsBlacksmithEnumMember(16)]
        public CEValue CommonReduction() => _CommonReduction_GetOrCreate();
        [IsBlacksmithEnumMember(32)]
        public CEValue StoneShell() => _StoneShell_GetOrCreate();
        [IsBlacksmithEnumMember(64)]
        public CEValue RealArmor() => _RealArmor_GetOrCreate();
        [IsBlacksmithEnumMember(128)]
        public CEValue CommonArmor() => _CommonArmor_GetOrCreate();
    }
    public partial class DefenseEntity : IAnalyzableData
    {
        public required string Name { get; init; }
        public required string AnalyzerKey { get; init; }
        public string MergeKey { get; init; } = nameof(StandardAnalyzers.DefaultMerge);
        public required DefenseType.CEValue Type { get; init; }
        public required int Power { get; set; }
        public required ClapRoundClock Clock { get; init; }
        public bool CanMerge { get; init; } = false;
        public void Update()
        {
            Clock.RoundPass();
            if (Power <= 0)
            {
                Clock.Kill();
            }
        }

    }
}