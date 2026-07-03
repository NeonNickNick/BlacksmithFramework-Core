using BlacksmithCore.Infrastructure.Attributes.SkillMetadata;
using BlacksmithCore.Infrastructure.Models.Profession;
using BlacksmithCore.Infrastructure.SkillSystem.SkillDSL;

namespace ModExamples.CauldronMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class ElementalArmor : SkillPackageDefinition
    {
        private bool HammerCheck(ISkillCheckContext sc) => true;
        [HasAttack]
        private IDSLSourceFile Hammer(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .WriteAttack(6f, AttackType.Instance.Physical());
            return DSL.CreateBy(pen);
        }
        private bool GuardCheck(ISkillCheckContext sc) => true;
        private IDSLSourceFile Guard(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .WriteDefense(8f, new CommonReduction());
            return DSL.CreateBy(pen);
        }
    }
}
