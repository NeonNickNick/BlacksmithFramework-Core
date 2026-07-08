using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BlacksmithSourceGenerators.Enum
{
    [Generator]
    public class BlacksmithEnumCacheGenerator : IIncrementalGenerator
    {
        /// <summary>CLAP015: BlacksmithEnum subclass is not partial.</summary>
        private static readonly DiagnosticDescriptor NotPartialRule = new(
            id: "CLAP015",
            title: "BlacksmithEnum 子类未声明 partial",
            messageFormat: "类 '{0}' 继承自 BlacksmithEnum 但未声明为 partial。请添加 partial 关键字以启用枚举成员缓存。",
            category: "ClapEnumCache",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetEnumInfo(ctx))
                .Where(info => info is not null)
                .Collect();

            context.RegisterSourceOutput(provider, GenerateSource);
        }

        private static EnumInfo? GetEnumInfo(GeneratorSyntaxContext ctx)
        {
            var classDecl = (ClassDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (symbol is not { IsAbstract: false })
                return null;

            var compilation = ctx.SemanticModel.Compilation;

            // Find the closed-constructed BlacksmithEnum<,> base type.
            var openGenericBase = compilation.GetTypeByMetadataName(
                "BlacksmithCore.Infrastructure.Enum.BlacksmithEnum`2");
            if (openGenericBase == null)
                return null;

            var constructedBase = FindConstructedBase(symbol, openGenericBase);
            if (constructedBase == null)
                return null;

            // Get the CEValue nested type from the constructed base.
            var ceValueType = constructedBase.GetTypeMembers("CEValue").FirstOrDefault();
            if (ceValueType == null)
                return null;

            // The second type argument is the member attribute type.
            var memberAttributeType = constructedBase.TypeArguments.Length >= 2
                ? constructedBase.TypeArguments[1]
                : null;
            if (memberAttributeType == null)
                return null;

            var currentFilePath = classDecl.SyntaxTree.FilePath;

            var methods = symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m is
                {
                    MethodKind: MethodKind.Ordinary,
                    IsStatic: false,
                    Parameters.Length: 0
                });

            var methodNames = new List<string>();
            foreach (var method in methods)
            {
                // Only process methods declared in the current partial file.
                var isDeclaredInThisFile = method.Locations.Any(
                    loc => loc.SourceTree?.FilePath == currentFilePath);
                if (!isDeclaredInThisFile)
                    continue;

                // Must return CEValue.
                if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, ceValueType))
                    continue;

                // Must have the member attribute.
                var hasMemberAttr = method.GetAttributes().Any(a =>
                    SymbolEqualityComparer.Default.Equals(
                        a.AttributeClass, memberAttributeType));
                if (!hasMemberAttr)
                    continue;

                methodNames.Add(method.Name);
            }

            if (methodNames.Count == 0)
                return null;

            bool isPartial = classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

            return new EnumInfo(
                symbol.Name,
                symbol.ContainingNamespace.ToDisplayString(),
                classDecl.SyntaxTree.FilePath,
                isPartial,
                methodNames,
                classDecl.GetLocation());
        }

        /// <summary>
        /// Walks the base-type chain of <paramref name="type"/> to find
        /// the closed-constructed version of <paramref name="openGenericBase"/>.
        /// </summary>
        private static INamedTypeSymbol? FindConstructedBase(
            INamedTypeSymbol type, INamedTypeSymbol openGenericBase)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(
                        current.OriginalDefinition, openGenericBase))
                {
                    return current;
                }
                current = current.BaseType;
            }
            return null;
        }

        private static void GenerateSource(
            SourceProductionContext ctx, ImmutableArray<EnumInfo?> infos)
        {
            // Merge partial declarations by (Namespace, ClassName).
            var mergedByClass = new Dictionary<string, EnumInfo>();
            foreach (var info in infos)
            {
                if (info == null)
                    continue;

                var classKey = $"{info.Namespace}.{info.ClassName}";
                if (mergedByClass.TryGetValue(classKey, out var existing))
                {
                    existing.MethodNames.AddRange(info.MethodNames);
                    // Keep the first file's info for location reporting.
                }
                else
                {
                    mergedByClass[classKey] = info;
                }
            }

            foreach (var info in mergedByClass.Values)
            {
                if (!info.IsPartial)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        NotPartialRule,
                        info.Location,
                        info.ClassName));
                    continue;
                }

                ctx.AddSource(
                    $"{info.Namespace}.{info.ClassName}.EnumCache.g.cs",
                    GeneratePartialClass(info));
            }
        }

        private static string GeneratePartialClass(EnumInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine($"namespace {info.Namespace};");
            sb.AppendLine();
            sb.AppendLine($"partial class {info.ClassName}");
            sb.AppendLine("{");
            foreach (var name in info.MethodNames)
            {
                sb.AppendLine($"    private static CEValue? _cev_{name};");
                sb.AppendLine($"    private static CEValue _{name}_GetOrCreate()");
                sb.AppendLine($"        => _cev_{name} ??= GetCEValue(\"{name}\");");
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

        internal sealed class EnumInfo
        {
            public string ClassName { get; }
            public string Namespace { get; }
            public string FilePath { get; }
            public bool IsPartial { get; }
            public List<string> MethodNames { get; }
            public Location Location { get; }

            public EnumInfo(
                string className,
                string @namespace,
                string filePath,
                bool isPartial,
                List<string> methodNames,
                Location location)
            {
                ClassName = className;
                Namespace = @namespace;
                FilePath = filePath;
                IsPartial = isPartial;
                MethodNames = methodNames;
                Location = location;
            }
        }
    }
}
