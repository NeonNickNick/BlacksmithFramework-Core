using BlacksmithCore.Infrastructure.Judgement;
using BlacksmithCore.Infrastructure.Judgement.Core;

namespace BlacksmithCore.Infrastructure.SkillSystem.SkillDSL
{
    public interface IDSLSourceFile
    {
        public bool IsPassive { get; set; }
        public Intent Compile(JudgeRuleManager? judgeRuleManager = null);
    }
}
