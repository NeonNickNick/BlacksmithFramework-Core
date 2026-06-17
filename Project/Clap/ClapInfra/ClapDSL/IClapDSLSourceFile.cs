using ClapInfra.ClapJudgement;
using ClapInfra.ClapJudgement.Core;
using ClapInfra.ClapModels.Components;

namespace ClapInfra.ClapDSL
{
    public interface IClapDSLSourceFile<TCommunity, TJudgeRuleManager, TIntent, TIDSLSourceFile, TIAnalyzableData>
        where TIDSLSourceFile : IClapDSLSourceFile<TCommunity, TJudgeRuleManager, TIntent, TIDSLSourceFile, TIAnalyzableData>
        where TIntent : ClapIntent<TCommunity>
        where TJudgeRuleManager : ClapJudgeRuleManager<TCommunity, TIAnalyzableData>, new()
        where TIAnalyzableData : IClapAnalyzableData<TCommunity>
    {
        public TIntent Compile(TJudgeRuleManager? judgeRuleManager = null);
    }
}
