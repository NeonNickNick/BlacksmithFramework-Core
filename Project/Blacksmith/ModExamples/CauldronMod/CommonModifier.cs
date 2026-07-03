using BlacksmithCore.BuiltInProfessions;
using BlacksmithCore.Infrastructure.Attributes.Profession;
using BlacksmithCore.Infrastructure.Attributes.SkillMetadata;
using BlacksmithCore.Infrastructure.Models.Components;
using BlacksmithCore.Infrastructure.Models.Profession;
using BlacksmithCore.Infrastructure.SkillSystem.SkillDSL;

namespace ModExamples.CauldronMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsSkillPackageModifier(nameof(Common))]
    public partial class CommonModifier : SkillPackageModifier
    {
        private bool CauldronCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3f);
        }
        [IsEquipmentSkill]
        private IDSLSourceFile Cauldron(ISkillExecuteContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Cauldron()));
            Pen pen = sf => sf
                .UseResource(3f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    source.Focus.Get<Skill>().RemoveSkill(nameof(Common), nameof(Cauldron));
                }, false);
            return DSL.CreateBy(pen);
        }
    }
}
