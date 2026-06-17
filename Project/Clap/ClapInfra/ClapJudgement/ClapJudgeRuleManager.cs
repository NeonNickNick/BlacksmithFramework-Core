using ClapInfra.ClapModels.Components;

namespace ClapInfra.ClapJudgement
{
    public abstract class ClapJudgeRuleManager<TCommunity, TIAnalyzableData>
        where TIAnalyzableData : IClapAnalyzableData<TCommunity>
    {
        public abstract void Judge(TCommunity player, TCommunity enemy);
    }
}
