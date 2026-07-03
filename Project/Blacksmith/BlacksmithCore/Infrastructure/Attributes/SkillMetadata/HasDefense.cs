using BlacksmithCore.Infrastructure.Attributes.SkillMetadata.Core;

namespace BlacksmithCore.Infrastructure.Attributes.SkillMetadata
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = true, Inherited = false)]
    public class HasDefense : Attribute, ISkillMetadata
    {
    }
}
