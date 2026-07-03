using BlacksmithCore.Infrastructure.Models.Profession;

namespace BlacksmithCore.Driver
{
    public partial class GameInstance : ISudoOperations
    {
        private void Copy(GameInstance origin)
        {
            Player.Copy(origin.Player);
            Enemy.Copy(origin.Enemy);
            Judger.Copy(origin.Judger);
            History.Copy(origin.History);
            Metadata.Copy(origin.Metadata);
        }
    }
}
