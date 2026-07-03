namespace BlacksmithCore.Infrastructure.Attributes.Analyzer
{
    public enum AnalyzerType
    {
        DSL,
        Defense,
        JudgeCallback,
        Preprocess,
        Universal
    }
    [AttributeUsage(AttributeTargets.Method,
        AllowMultiple = false, Inherited = false)]
    public class IsAnalyzer : Attribute
    {
        public readonly AnalyzerType Type;
        public IsAnalyzer(AnalyzerType type)
        {
            Type = type;
        }
    }
}
