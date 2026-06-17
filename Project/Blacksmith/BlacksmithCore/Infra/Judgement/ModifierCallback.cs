using BlacksmithCore.Infra.Judgement.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Judgement
{
    public class ModifierCallback : ICallbackOnJudge
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required JudgeStage.CEValue Stage { get; init; }
        public required bool IsPlayer { get; init; }
        public required ModifierOrder ModifierOrder { get; init; }
    }
}
