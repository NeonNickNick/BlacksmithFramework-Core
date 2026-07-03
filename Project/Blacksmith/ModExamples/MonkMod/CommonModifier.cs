using BlacksmithCore.BuiltInProfessions;
using BlacksmithCore.Infrastructure.Attributes.Profession;
using BlacksmithCore.Infrastructure.Attributes.SkillMetadata;
using BlacksmithCore.Infrastructure.Models.Components;
using BlacksmithCore.Infrastructure.Models.Profession;
using BlacksmithCore.Infrastructure.SkillSystem.SkillDSL;

namespace ModExamples.MonkMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsSkillPackageModifier(nameof(Common))]
    public partial class CommonModifier : SkillPackageModifier
    {
        private bool MonkCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 3f);
        }
        [IsProfessionSkill]
        private IDSLSourceFile Monk(ISkillExecuteContext sc)
        {
            sc.Self.Focus.Get<Skill>().AddPackage(new(new Monk()));
            Pen pen = sf => sf
                .UseResource(3f, ResourceType.Instance.Iron())
                .WriteFree(source =>
                {
                    Common.ExcludeAllProfessions(source);
                }, false);
            return DSL.CreateBy(pen);
        }
    }
}
