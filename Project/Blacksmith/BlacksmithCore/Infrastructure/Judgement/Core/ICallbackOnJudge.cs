using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;

namespace BlacksmithCore.Infrastructure.Judgement.Core
{
    public enum ModifierOrder
    {
        Before,
        After
    }
    public interface ICallbackOnJudge : IAnalyzableData
    {
        public JudgeStage.CEValue Stage { get; init; }
        public bool IsPlayer { get; set; }
    }
}
