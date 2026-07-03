using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public interface IAnalyzableData
    {
        public ClapRoundClock Clock { get; init; }
        public string AnalyzerKey { get; init; }
    }
}
