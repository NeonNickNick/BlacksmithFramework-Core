using BlacksmithCore.Infrastructure.Models.Player;

namespace BlacksmithCore.Infrastructure.Judgement.Core
{
    public class Intent
    {
        public required Action<Community> Execute { get; set; }
    }
}
