namespace BlacksmithCore.Infrastructure.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Class,
        AllowMultiple = false, Inherited = false)]
    public class IsManual : Attribute
    {
    }
}