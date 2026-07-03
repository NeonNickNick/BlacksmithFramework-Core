using BlacksmithCore.Driver;
using BlacksmithCore.Infrastructure.Models.Player;

namespace BlacksmithCore.Infrastructure.Models.Profession
{
    public interface ISkillCheckContext
    {
        public ISudoOperations SudoOperations { get; }
        public Community Self { get; }
        public SkillDeclareData SkillDeclareData { get; }
    }
    public interface ISkillExecuteContext
    {
        public ISudoOperations SudoOperations { get; }
        public Community Self { get; }
        public SkillDeclareData SkillDeclareData { get; }
    }
    public interface ISudoOperations
    {
        public GameInstance DeepCopy();
        public GameInstance DeepCopy(int roundCount);
    }
}
