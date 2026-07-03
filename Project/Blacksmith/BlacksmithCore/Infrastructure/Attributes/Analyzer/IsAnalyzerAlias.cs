namespace BlacksmithCore.Infrastructure.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsAnalyzerAlias : Attribute
    {
        public readonly string Target;
        public IsAnalyzerAlias(string target)
        {
            Target = target;
        }
    }
}
