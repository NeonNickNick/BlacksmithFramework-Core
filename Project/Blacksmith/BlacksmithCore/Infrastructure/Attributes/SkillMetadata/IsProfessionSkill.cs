using BlacksmithCore.Infrastructure.Attributes.SkillMetadata.Core;

namespace BlacksmithCore.Infrastructure.Attributes.SkillMetadata
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsProfessionSkill : Attribute, ISkillMetadata
    {
    }
}
