using BlacksmithCore.Infrastructure.Attributes.SkillMetadata.Core;
using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;

namespace BlacksmithCore.Infrastructure.Attributes.SkillMetadata
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = true, Inherited = false)]
    public class HasAttack : Attribute, ISkillMetadata
    {
        public readonly int Power;
        private readonly string _name;
        public AttackType.CEValue Type => AttackType.EnumDict[_name];
        public HasAttack(int power, string name = nameof(AttackType.Instance.Physical))
        {
            Power = power;
            _name = name;
        }
    }
}
