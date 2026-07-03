namespace BlacksmithCore.Infrastructure.Attributes.SkillMarkOnly
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsExperimental : Attribute
    {
        public string Description { get; }
        public IsExperimental(string description = "")
        {
            Description = description;
        }
    }
}
