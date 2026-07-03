using BlacksmithCore.Infrastructure.Attributes.SkillMetadata;
using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;
using BlacksmithCore.Infrastructure.Models.Components;
using BlacksmithCore.Infrastructure.Models.Profession;
using BlacksmithCore.Infrastructure.SkillSystem.SkillDSL;

namespace BlacksmithCore.BuiltInProfessions
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    public partial class Alchemy : SkillPackageDefinition
    {
        private static bool MidasTouchCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 1, true);
        }
        [HasResource]
        [Labels(Impression.Robust, Strength.Strong)]
        private static IDSLSourceFile MidasTouch(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1, ResourceType.Instance.Iron(), true)
                .WriteResource(5, ResourceType.Instance.Gold_Iron());
            return DSL.CreateBy(pen);
        }
    }
}