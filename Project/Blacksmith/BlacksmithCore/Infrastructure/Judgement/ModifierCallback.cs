using BlacksmithCore.Infrastructure.Judgement.Core;
using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Judgement
{
    public partial class ModifierCallback : ICallbackOnJudge
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public required JudgeStage.CEValue Stage { get; init; }
        public bool IsPlayer { get; set; }
        public required ModifierOrder ModifierOrder { get; init; }
    }
}
