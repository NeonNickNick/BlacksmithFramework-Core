using BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum;
using BlacksmithCore.Infrastructure.Enum;
using BlacksmithCore.Infrastructure.Models.LifeCycle;
using BlacksmithCore.Infrastructure.SkillSystem;
namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public class DefenseType : BlacksmithEnum<DefenseType>
    {
        [IsBlacksmithEnumMember(-16)]
        public DefenseType.CEValue PhysicalImmunity() => GetCEValue();
        [IsBlacksmithEnumMember(-16)]
        public DefenseType.CEValue MagicalImmunity() => GetCEValue();
        [IsBlacksmithEnumMember(-8)]
        public DefenseType.CEValue PercentageReduction() => GetCEValue();
        [IsBlacksmithEnumMember(0)]
        public CEValue RealReduction() => GetCEValue();
        [IsBlacksmithEnumMember(8)]
        public CEValue ThornReduction() => GetCEValue();
        [IsBlacksmithEnumMember(16)]
        public CEValue CommonReduction() => GetCEValue();
        [IsBlacksmithEnumMember(32)]
        public CEValue StoneShell() => GetCEValue();
        [IsBlacksmithEnumMember(64)]
        public CEValue RealArmor() => GetCEValue();
        [IsBlacksmithEnumMember(128)]
        public CEValue CommonArmor() => GetCEValue();
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