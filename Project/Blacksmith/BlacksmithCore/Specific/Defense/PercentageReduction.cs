using BlacksmithCore.Infra.DSL;
using BlacksmithCore.Infra.Models.Components.AnalyzedObjects;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;
using ClapInfra.ClapUnit;

namespace BlacksmithCore.Specific.Defense
{
    public class PercentageReduction : DefenseEntity
    {
        public override string AnalyzerKey { get; init; } = nameof(StandardAnalyzers.PercentageReduction);
        public override DefenseType.CEValue Type { get; init; } = DefenseType.Instance.PercentageReduction();
        public override int Power { get; set; } = 0;
        public override ClapRoundClock Clock { get; init; } = new();
    }
}
