namespace ClapInfra.ClapProfession
{
    public interface IClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        public HashSet<string> AvailableSkillNames { get; set; }
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker { get; }
        public Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> SkillSourceFileGenerator { get; }
        public abstract TIDSLSourceFile PassiveSkill(TISkillContext sc);
    }

    public abstract class ClapSkillPackage<TISkillContext, TIDSLSourceFile>
        : IClapSkillPackage<TISkillContext, TIDSLSourceFile>
    {
        protected  HashSet<string> _availableSkillNames = new();
        protected readonly Dictionary<string, Func<TISkillContext, bool>> _skillChecker = new();
        protected readonly Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> _skillSourceFileGenerator = new();

        public HashSet<string> AvailableSkillNames { get => _availableSkillNames; set => _availableSkillNames = value; }
        public Dictionary<string, Func<TISkillContext, bool>> SkillChecker => _skillChecker;
        public Dictionary<string, Func<TISkillContext, TIDSLSourceFile>> SkillSourceFileGenerator => _skillSourceFileGenerator;

        protected ClapSkillPackage()
        {
            RegistSkills();
        }

        protected void RegistSkill(
            string skillName,
            Func<TISkillContext, bool> checker,
            Func<TISkillContext, TIDSLSourceFile> generator)
        {
            _availableSkillNames.Add(skillName);
            _skillChecker.Add(skillName, checker);
            _skillSourceFileGenerator.Add(skillName, generator);
        }
        protected virtual void RegistSkills() { }
        public virtual void RegistAnalyzers()
        {

        }

        public abstract TIDSLSourceFile PassiveSkill(TISkillContext sc);
    }
}
