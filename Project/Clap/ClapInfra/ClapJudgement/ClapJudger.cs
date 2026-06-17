using ClapInfra.ClapDSL;
using ClapInfra.ClapJudgement.Core;
using ClapInfra.ClapModels.Components;

namespace ClapInfra.ClapJudgement
{
    public class ClapJudger<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile, TIAnalyzableData>
        where TJudger : ClapJudger<TCommunity, TJudger, TJudgeRuleManager, TIntent, TIDSLSourceFile, TIAnalyzableData>
        where TIDSLSourceFile : IClapDSLSourceFile<TCommunity, TJudgeRuleManager, TIntent, TIDSLSourceFile, TIAnalyzableData>
        where TIntent : ClapIntent<TCommunity>
        where TJudgeRuleManager : ClapJudgeRuleManager<TCommunity, TIAnalyzableData>, new()
        where TIAnalyzableData : IClapAnalyzableData<TCommunity>
    {
        public TCommunity Player { get; protected set; }
        public TCommunity Enemy { get; protected set; }
        public TJudgeRuleManager JudgeRuleManager { get; }
        protected ClapJudger(TCommunity player, TCommunity enemy)
        {
            Player = player;
            Enemy = enemy;
            JudgeRuleManager = new();
        }
        protected virtual IEnumerable<TIntent> Compile(IEnumerable<TIDSLSourceFile> sourceFiles)
        {
            return sourceFiles.Select(s => s.Compile(JudgeRuleManager));
        }
        public virtual void Judge(IEnumerable<TIDSLSourceFile> playerSfs, IEnumerable<TIDSLSourceFile> enemySfs)
        {
            foreach (var temp in Compile(playerSfs))
            {
                temp.Execute(Player);
            }
            foreach (var temp in Compile(enemySfs))
            {
                temp.Execute(Enemy);
            }
            JudgeRuleManager.Judge(Player, Enemy);
        }
    }
}
