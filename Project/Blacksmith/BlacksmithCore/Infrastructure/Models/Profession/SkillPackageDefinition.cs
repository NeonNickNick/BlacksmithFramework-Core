namespace BlacksmithCore.Infrastructure.Models.Profession
{
    public abstract class SkillPackageDefinition : SkillPackageBase
    {
        public SkillPackageDefinition()
        {
            ProfessionRegistry.AddModOnInit(this);
        }
    }
}
