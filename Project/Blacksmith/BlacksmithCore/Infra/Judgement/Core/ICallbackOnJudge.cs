using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Judgement.Core
{
    public enum ModifierOrder
    {
        Before,
        After
    }
    public interface ICallbackOnJudge : IAnalyzableData
    {
        public JudgeStage.CEValue Stage { get; init; }
        public bool IsPlayer { get; init; }
    }
}
