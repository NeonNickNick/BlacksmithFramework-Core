using BlacksmithCore.Infra.Profession;

namespace BlacksmithCore.Driver
{
    public class GameHistory
    {
        public List<(ISkillContext, ISkillContext)> SkillHistory { get; set; } = new();
        public void Copy(GameHistory origin)
        {
            //暂时可不写
        }
    }
}
