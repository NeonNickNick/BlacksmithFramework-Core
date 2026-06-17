using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Core;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Infra.Models.Components.AnalyzableDatas
{
    public class EffectEntity : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public bool IsMark { get; set; }
        public required EffectType.CEValue Type { get; init; }
        public required ClapRoundClock Clock { get; init; }
    }
}