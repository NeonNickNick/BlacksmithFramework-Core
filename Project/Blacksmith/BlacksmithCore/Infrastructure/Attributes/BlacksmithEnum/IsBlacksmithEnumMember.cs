using BlacksmithCore.Infrastructure.Enum;

namespace BlacksmithCore.Infrastructure.Attributes.BlacksmithEnum
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsBlacksmithEnumMember : Attribute, IBlacksmithEnumMember
    {
        public int Priority { get; }
        public IsBlacksmithEnumMember(int priority)
        {
            Priority = priority;
        }
    }
}
