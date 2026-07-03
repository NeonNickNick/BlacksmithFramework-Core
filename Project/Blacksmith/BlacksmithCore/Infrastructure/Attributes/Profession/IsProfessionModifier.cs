namespace BlacksmithCore.Infrastructure.Attributes.Profession
{
    [AttributeUsage(AttributeTargets.Class,
        AllowMultiple = false, Inherited = false)]
    public class IsSkillPackageModifier : Attribute
    {
        public readonly string TargetName;
        public IsSkillPackageModifier(string targetName)
        {
            TargetName = targetName;
        }
    }
}

