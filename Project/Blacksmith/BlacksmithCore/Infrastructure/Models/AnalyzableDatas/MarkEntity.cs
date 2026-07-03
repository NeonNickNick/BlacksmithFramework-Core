using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.AnalyzableDatas
{
    public class MarkEntity
    {
        public required string MarkName { get; init; }
        public required ClapRoundClock Clock { get; init; }
        public int LayerNum { get; set; } = 1;
        public bool IsHidden { get; init; } = false;
        public MarkEntity Copy()
        {
            return new()
            {
                MarkName = MarkName,
                Clock = Clock.Copy(),
                LayerNum = LayerNum,
                IsHidden = IsHidden
            };
        }
    }
}