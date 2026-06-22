using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClapSourceGenerators.SkillRegistration
{
    [Generator]
    public class AnalyzersRegistrationGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor WrongReturnTypeRule = new(
            id: "CLAP003",
            title: "Analyzer method must return void",
            messageFormat: "Analyzer method '{0}' has AnalyzerType '{1}' but returns '{2}' instead of 'void'. Change the return type to 'void'.",
            category: "ClapAnalyzerRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor WrongParameterSignatureRule = new(
            id: "CLAP004",
            title: "Analyzer method has wrong parameter signature",
            messageFormat: "Analyzer method '{0}' has AnalyzerType '{1}' which expects parameters ({2}), but the method has parameters ({3}). Fix the parameter list to match.",
            category: "ClapAnalyzerRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor DuplicateAnalyzerNameRule = new(
            id: "CLAP005",
            title: "Duplicate analyzer or alias name within same package kind",
            messageFormat: "'{0}' in package kind '{1}' conflicts with '{2}' declared earlier. Analyzer and alias names must be unique within the same package kind.",
            category: "ClapAnalyzerRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor AliasTargetNotFoundRule = new(
            id: "CLAP006",
            title: "IsAnalyzerAlias target method not found",
            messageFormat: "Alias method '{0}' targets '{1}', but no [IsAnalyzer] method named '{1}' exists in this class. The target must be a method with [IsAnalyzer] declared in the same partial class.",
            category: "ClapAnalyzerRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MustBeStaticRule = new(
            id: "CLAP007",
            title: "Analyzer method must be static",
            messageFormat: "Method '{0}' is marked with '{1}' but is not static. Methods with [IsAnalyzer] or [IsAnalyzerAlias] must be static.",
            category: "ClapAnalyzerRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor MustBePublicRule = new(
            id: "CLAP008",
            title: "Analyzer method must be public",
            messageFormat: "Analyzer method '{0}' is not public. Methods with [IsAnalyzer] must be public.",
            category: "ClapAnalyzerRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor BothAnalyzerAndAliasRule = new(
            id: "CLAP009",
            title: "Method cannot have both [IsAnalyzer] and [IsAnalyzerAlias]",
            messageFormat: "Method '{0}' has both [IsAnalyzer] and [IsAnalyzerAlias] attributes. A method cannot serve as both an analyzer and an alias.",
            category: "ClapAnalyzerRegistration",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is ClassDeclarationSyntax,
                    transform: (ctx, _) => GetPackageInfo(ctx))
                .Where(info => info is not null)
                .Collect();

            context.RegisterSourceOutput(provider, GenerateSource);
        }

        private static AnalyzerPackageInfo? GetPackageInfo(GeneratorSyntaxContext ctx)
        {
            var classDecl = (ClassDeclarationSyntax)ctx.Node;
            var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (symbol is not { IsAbstract: false })
                return null;

            var baseType = FindSkillPackageBase(symbol);
            if (baseType == null)
                return null;

            var packageKind = DeterminePackageKind(symbol);

            // Use MethodKind.Ordinary to include both static and non-static ordinary methods.
            // Non-static methods with analyzer attributes must be detected and reported.
            var methods = symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary)
                .ToList();

            var analyzerMethods = new List<AnalyzerMethodInfo>();
            var aliasMethods = new List<AnalyzerAliasInfo>();
            var diagnostics = new List<AnalyzerDiagnosticInfo>();

            var currentFilePath = classDecl.SyntaxTree.FilePath;

            foreach (var method in methods)
            {
                // Only process methods declared in the current partial file.
                // GetMembers() returns all members across all partial declarations,
                // so we must filter to avoid processing the same method multiple times.
                var isDeclaredInThisFile = method.Locations.Any(
                    loc => loc.SourceTree?.FilePath == currentFilePath);
                if (!isDeclaredInThisFile)
                    continue;

                var isAnalyzerAttr = method.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString() == "BlacksmithCore.Infra.Attributes.Analyzer.IsAnalyzer");

                var isAnalyzerAliasAttr = method.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.ToDisplayString() == "BlacksmithCore.Infra.Attributes.Analyzer.IsAnalyzerAlias");

                // Skip methods with neither attribute
                if (isAnalyzerAttr == null && isAnalyzerAliasAttr == null)
                    continue;

                // Check static requirement — both attributes require static methods
                if (!method.IsStatic)
                {
                    var attrName = isAnalyzerAttr != null ? "IsAnalyzer" : "IsAnalyzerAlias";
                    diagnostics.Add(new AnalyzerDiagnosticInfo(
                        AnalyzerDiagnosticKind.MustBeStatic,
                        method.Locations.FirstOrDefault(),
                        method.Name,
                        attrName));

                    // For [IsAnalyzer] on non-static, also run existing signature validation
                    // so the developer sees all issues in one compilation cycle.
                    if (isAnalyzerAttr != null)
                    {
                        var nsAnalyzerType = ExtractAnalyzerType(isAnalyzerAttr);
                        var nsValidationResult = ValidateAnalyzerMethod(method, nsAnalyzerType, ctx.SemanticModel.Compilation);
                        if (nsValidationResult != null)
                            diagnostics.Add(nsValidationResult);
                    }
                    continue;
                }

                // Both [IsAnalyzer] and [IsAnalyzerAlias] on the same method —
                // a method cannot serve as both, and the name would be registered twice.
                if (isAnalyzerAttr != null && isAnalyzerAliasAttr != null)
                {
                    diagnostics.Add(new AnalyzerDiagnosticInfo(
                        AnalyzerDiagnosticKind.BothAnalyzerAndAlias,
                        method.Locations.FirstOrDefault(),
                        method.Name));
                    diagnostics.Add(new AnalyzerDiagnosticInfo(
                        AnalyzerDiagnosticKind.DuplicateAnalyzerName,
                        method.Locations.FirstOrDefault(),
                        method.Name,
                        packageKind.ToString(),
                        method.Name));
                    continue;
                }

                // [IsAnalyzerAlias] on static method: collect without signature validation
                if (isAnalyzerAliasAttr != null)
                {
                    var targetName = isAnalyzerAliasAttr.ConstructorArguments.Length > 0
                        ? isAnalyzerAliasAttr.ConstructorArguments[0].Value as string ?? string.Empty
                        : string.Empty;

                    aliasMethods.Add(new AnalyzerAliasInfo(
                        method.Name,
                        targetName,
                        method.Locations.FirstOrDefault()));
                    continue;
                }

                // [IsAnalyzer] on static method: existing validation logic
                // isAnalyzerAttr is guaranteed non-null here — we already skipped null+null and alias-only cases
                var analyzerType = ExtractAnalyzerType(isAnalyzerAttr!);

                var validationResult = ValidateAnalyzerMethod(method, analyzerType, ctx.SemanticModel.Compilation);
                if (validationResult != null)
                {
                    diagnostics.Add(validationResult);
                    continue; // Skip this method — don't register broken analyzers
                }

                // [IsAnalyzer] methods must be public
                if (method.DeclaredAccessibility != Accessibility.Public)
                {
                    diagnostics.Add(new AnalyzerDiagnosticInfo(
                        AnalyzerDiagnosticKind.MustBePublic,
                        method.Locations.FirstOrDefault(),
                        method.Name));
                    continue; // Skip — don't register non-public analyzers
                }

                analyzerMethods.Add(new AnalyzerMethodInfo(
                    method.Name,
                    analyzerType,
                    method.Locations.FirstOrDefault()));
            }

            if (analyzerMethods.Count == 0 && aliasMethods.Count == 0 && diagnostics.Count == 0)
                return null;

            var filePath = classDecl.SyntaxTree.FilePath;

            return new AnalyzerPackageInfo(
                symbol.Name,
                symbol.ContainingNamespace.ToDisplayString(),
                filePath,
                packageKind,
                analyzerMethods,
                diagnostics,
                aliasMethods,
                new List<ResolvedAliasInfo>());
        }

        private static INamedTypeSymbol? FindSkillPackageBase(INamedTypeSymbol type)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.Name == "SkillPackageBase" &&
                    current.TypeArguments.Length == 0 &&
                    current.ContainingNamespace.ToDisplayString() == "BlacksmithCore.Infra.Profession")
                    return current;
                current = current.BaseType;
            }
            return null;
        }

        private static PackageKind DeterminePackageKind(INamedTypeSymbol type)
        {
            var current = type.BaseType;
            while (current != null)
            {
                var fullName = current.ToDisplayString();
                if (fullName == "BlacksmithCore.Infra.Profession.MainProfession")
                    return PackageKind.MainProfession;
                if (fullName == "BlacksmithCore.Infra.Profession.ProfessionModifier")
                    return PackageKind.Modifier;
                current = current.BaseType;
            }
            return PackageKind.Other;
        }

        /// <summary>
        /// Extracts the AnalyzerTypeKind from an [IsAnalyzer] attribute's constructor argument.
        /// Defaults to DSL if no argument is provided.
        /// </summary>
        private static AnalyzerTypeKind ExtractAnalyzerType(AttributeData attr)
        {
            if (attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is int typeValue)
                return (AnalyzerTypeKind)typeValue;
            return AnalyzerTypeKind.DSL;
        }

        /// <summary>
        /// Validates that the method signature matches the expected delegate for the given AnalyzerType.
        /// Returns an AnalyzerDiagnosticInfo if validation fails, null if it passes.
        /// Universal type always passes validation.
        /// </summary>
        private static AnalyzerDiagnosticInfo? ValidateAnalyzerMethod(
            IMethodSymbol method,
            AnalyzerTypeKind analyzerType,
            Compilation compilation)
        {
            if (analyzerType == AnalyzerTypeKind.Universal)
                return null;

            // Check return type is void
            if (method.ReturnType.SpecialType != SpecialType.System_Void)
            {
                return new AnalyzerDiagnosticInfo(
                    AnalyzerDiagnosticKind.WrongReturnType,
                    method.Locations.FirstOrDefault(),
                    method.Name,
                    analyzerType.ToString(),
                    method.ReturnType.ToDisplayString());
            }

            // Build the expected parameter type names
            var expectedParamTypeNames = GetExpectedParameterTypeNames(analyzerType);

            // Check parameter count
            if (method.Parameters.Length != expectedParamTypeNames.Length)
            {
                return new AnalyzerDiagnosticInfo(
                    AnalyzerDiagnosticKind.WrongParameterSignature,
                    method.Locations.FirstOrDefault(),
                    method.Name,
                    analyzerType.ToString(),
                    string.Join(", ", expectedParamTypeNames),
                    string.Join(", ", method.Parameters.Select(p => p.Type.ToDisplayString())));
            }

            // Check each parameter type
            for (int i = 0; i < expectedParamTypeNames.Length; i++)
            {
                var actualType = method.Parameters[i].Type;
                if (!IsTypeMatch(actualType, expectedParamTypeNames[i]))
                {
                    return new AnalyzerDiagnosticInfo(
                        AnalyzerDiagnosticKind.WrongParameterSignature,
                        method.Locations.FirstOrDefault(),
                        method.Name,
                        analyzerType.ToString(),
                        string.Join(", ", expectedParamTypeNames),
                        string.Join(", ", method.Parameters.Select(p => p.Type.ToDisplayString())));
                }
            }

            return null;
        }

        private static string[] GetExpectedParameterTypeNames(AnalyzerTypeKind analyzerType)
        {
            return analyzerType switch
            {
                AnalyzerTypeKind.DSL => new[]
                {
                    "BlacksmithCore.Infra.Models.Entites.Community",
                    "BlacksmithCore.Infra.Models.Entites.Community",
                    "BlacksmithCore.Infra.Models.Components.IAnalyzableData"
                },
                AnalyzerTypeKind.Defense => new[]
                {
                    "BlacksmithCore.Infra.Models.Entites.Community",
                    "BlacksmithCore.Infra.Models.Entites.Community",
                    "BlacksmithCore.Infra.Models.Components.AnalyzedObjects.DefenseEntity",
                    "BlacksmithCore.Infra.Models.Components.AnalyzableDatas.AttackAnalyzableData"
                },
                AnalyzerTypeKind.JudgeCallback => new[]
                {
                    "BlacksmithCore.Infra.Models.Entites.Community",
                    "BlacksmithCore.Infra.Models.Entites.Community"
                },
                _ => Array.Empty<string>()
            };
        }

        private static bool IsTypeMatch(ITypeSymbol type, string expectedFullName)
        {
            return type.ToDisplayString() == expectedFullName;
        }

        private static void GenerateSource(SourceProductionContext ctx, ImmutableArray<AnalyzerPackageInfo?> infos)
        {
            // --- Step 1: Merge partial class declarations ---
            // Multiple .cs files can declare the same partial class. Merge them by (Namespace, ClassName).
            var mergedByClass = new Dictionary<string, AnalyzerPackageInfo>();
            foreach (var info in infos)
            {
                if (info == null) continue;
                var classKey = $"{info.Namespace}.{info.ClassName}";
                if (mergedByClass.TryGetValue(classKey, out var existing))
                {
                    // Merge: combine analyzer methods, aliases, and diagnostics
                    existing.AnalyzerMethods.AddRange(info.AnalyzerMethods);
                    existing.AliasMethods.AddRange(info.AliasMethods);
                    existing.Diagnostics.AddRange(info.Diagnostics);
                    // Keep the first FilePath as primary
                }
                else
                {
                    mergedByClass[classKey] = info;
                }
            }

            // --- Step 1.5: Resolve alias targets across the entire PackageKind ---
            // Build a global map: (PackageKind, methodName) -> (AnalyzerType, QualifiedClassName)
            // from ALL [IsAnalyzer] methods across ALL classes. This allows an alias in one
            // class to target an [IsAnalyzer] method in another class of the same PackageKind.
            var globalAnalyzerMap = new Dictionary<(PackageKind, string), (AnalyzerTypeKind Type, string QualifiedClass)>();
            foreach (var info in mergedByClass.Values)
            {
                var qualifiedClass = $"{info.Namespace}.{info.ClassName}";
                foreach (var am in info.AnalyzerMethods)
                {
                    var key = (info.PackageKind, am.MethodName);
                    if (!globalAnalyzerMap.ContainsKey(key))
                        globalAnalyzerMap[key] = (am.AnalyzerType, qualifiedClass);
                }
            }

            // Resolve each alias against the global map using the alias's own PackageKind
            foreach (var info in mergedByClass.Values)
            {
                foreach (var alias in info.AliasMethods)
                {
                    var lookupKey = (info.PackageKind, alias.TargetName);
                    if (globalAnalyzerMap.TryGetValue(lookupKey, out var targetInfo))
                    {
                        info.ResolvedAliases.Add(new ResolvedAliasInfo(
                            alias.AliasName,
                            alias.TargetName,
                            targetInfo.QualifiedClass,
                            targetInfo.Type,
                            alias.Location));
                    }
                    else
                    {
                        info.Diagnostics.Add(new AnalyzerDiagnosticInfo(
                            AnalyzerDiagnosticKind.AliasTargetNotFound,
                            alias.Location,
                            alias.AliasName,
                            alias.TargetName));
                    }
                }
            }

            // --- Step 2: Cross-class deduplication pass ---
            // Within each PackageKind, analyzer method names must be unique.
            var firstSeenByKind = new Dictionary<(PackageKind, string), AnalyzerMethodInfo>();
            var duplicateMethods = new HashSet<(PackageKind Kind, string MethodName, AnalyzerPackageInfo Package)>();

            foreach (var info in mergedByClass.Values)
            {
                foreach (var method in info.AnalyzerMethods)
                {
                    var key = (info.PackageKind, method.MethodName);
                    if (firstSeenByKind.TryGetValue(key, out var firstSeen))
                    {
                        duplicateMethods.Add((info.PackageKind, method.MethodName, info));
                    }
                    else
                    {
                        firstSeenByKind[key] = method;
                    }
                }
            }

            // Alias name dedup within PackageKind — ALL aliases (resolved + unresolved)
            // must not conflict with analyzer names or other alias names.
            var firstAliasByKind = new Dictionary<(PackageKind, string), (string ClassName, Location? Location)>();

            foreach (var info in mergedByClass.Values)
            {
                foreach (var alias in info.AliasMethods)
                {
                    var key = (info.PackageKind, alias.AliasName);

                    // Conflict: alias name matches an [IsAnalyzer] name in the same PackageKind
                    if (globalAnalyzerMap.ContainsKey(key))
                    {
                        info.Diagnostics.Add(new AnalyzerDiagnosticInfo(
                            AnalyzerDiagnosticKind.DuplicateAnalyzerName,
                            alias.Location,
                            alias.AliasName,
                            info.PackageKind.ToString(),
                            alias.AliasName));
                    }

                    // Conflict: alias name duplicates another alias in the same PackageKind
                    if (firstAliasByKind.TryGetValue(key, out var firstSeen))
                    {
                        info.Diagnostics.Add(new AnalyzerDiagnosticInfo(
                            AnalyzerDiagnosticKind.DuplicateAnalyzerName,
                            alias.Location,
                            alias.AliasName,
                            info.PackageKind.ToString(),
                            $"{firstSeen.ClassName}.{alias.AliasName}"));
                    }
                    else
                    {
                        firstAliasByKind[key] = (info.ClassName, alias.Location);
                    }
                }
            }

            // --- Step 3: Emit per-class sources and diagnostics ---
            foreach (var info in mergedByClass.Values)
            {
                // Report per-method validation diagnostics
                foreach (var diag in info.Diagnostics)
                {
                    var descriptor = diag.Kind switch
                    {
                        AnalyzerDiagnosticKind.WrongReturnType => WrongReturnTypeRule,
                        AnalyzerDiagnosticKind.WrongParameterSignature => WrongParameterSignatureRule,
                        AnalyzerDiagnosticKind.AliasTargetNotFound => AliasTargetNotFoundRule,
                        AnalyzerDiagnosticKind.MustBeStatic => MustBeStaticRule,
                        AnalyzerDiagnosticKind.MustBePublic => MustBePublicRule,
                        AnalyzerDiagnosticKind.BothAnalyzerAndAlias => BothAnalyzerAndAliasRule,
                        AnalyzerDiagnosticKind.DuplicateAnalyzerName => DuplicateAnalyzerNameRule,
                        _ => WrongReturnTypeRule // fallback
                    };

                    var location = diag.Location
                        ?? Location.Create(info.FilePath, new Microsoft.CodeAnalysis.Text.TextSpan(0, 0),
                            new Microsoft.CodeAnalysis.Text.LinePositionSpan(
                                new Microsoft.CodeAnalysis.Text.LinePosition(0, 0),
                                new Microsoft.CodeAnalysis.Text.LinePosition(0, 0)));

                    ctx.ReportDiagnostic(Diagnostic.Create(descriptor, location, diag.Args));
                }

                // Report cross-class dedup diagnostics for this package's duplicate analyzer methods
                foreach (var method in info.AnalyzerMethods)
                {
                    if (duplicateMethods.Contains((info.PackageKind, method.MethodName, info)))
                    {
                        var firstSeen = firstSeenByKind[(info.PackageKind, method.MethodName)];
                        var loc = method.Location
                            ?? Location.Create(info.FilePath, new Microsoft.CodeAnalysis.Text.TextSpan(0, 0),
                                new Microsoft.CodeAnalysis.Text.LinePositionSpan(
                                    new Microsoft.CodeAnalysis.Text.LinePosition(0, 0),
                                    new Microsoft.CodeAnalysis.Text.LinePosition(0, 0)));

                        ctx.ReportDiagnostic(Diagnostic.Create(
                            DuplicateAnalyzerNameRule, loc,
                            method.MethodName, info.PackageKind.ToString(), firstSeen.MethodName));
                    }
                }

                if (info.AnalyzerMethods.Count > 0 || info.ResolvedAliases.Count > 0)
                {
                    ctx.AddSource(
                        $"{info.Namespace}.{info.ClassName}.AnalyzersRegistration.g.cs",
                        GeneratePartialClass(info));
                }
            }
        }

        private static string GetRegistryMemberName(AnalyzerTypeKind analyzerType)
        {
            return analyzerType switch
            {
                AnalyzerTypeKind.DSL => "DSL",
                AnalyzerTypeKind.Defense => "Defense",
                AnalyzerTypeKind.JudgeCallback => "JudgeCallback",
                AnalyzerTypeKind.Universal => "Universal",
                _ => "DSL"
            };
        }

        private static string GeneratePartialClass(AnalyzerPackageInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            sb.AppendLine($"namespace {info.Namespace};");
            sb.AppendLine();
            sb.AppendLine($"partial class {info.ClassName}");
            sb.AppendLine("{");
            sb.AppendLine("    public override void RegistAnalyzers()");
            sb.AppendLine("    {");
            foreach (var method in info.AnalyzerMethods)
            {
                var memberName = GetRegistryMemberName(method.AnalyzerType);
                sb.AppendLine($"        global::BlacksmithCore.Infra.DSL.AnalyzerRegistry.{memberName}.Regist(\"{method.MethodName}\", {method.MethodName});");
            }
            foreach (var alias in info.ResolvedAliases)
            {
                var memberName = GetRegistryMemberName(alias.AnalyzerType);
                sb.AppendLine($"        global::BlacksmithCore.Infra.DSL.AnalyzerRegistry.{memberName}.Regist(\"{alias.AliasName}\", {alias.TargetQualifiedReference}.{alias.TargetMethodName});");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }
        internal enum AnalyzerTypeKind
        {
            DSL = 0,
            Defense = 1,
            JudgeCallback = 2,
            Universal = 3
        }

        internal enum PackageKind
        {
            MainProfession,
            Modifier,
            Other
        }

        internal enum AnalyzerDiagnosticKind
        {
            WrongReturnType,
            WrongParameterSignature,
            AliasTargetNotFound,
            MustBeStatic,
            MustBePublic,
            DuplicateAnalyzerName,
            BothAnalyzerAndAlias
        }

        internal sealed class AnalyzerDiagnosticInfo
        {
            public AnalyzerDiagnosticKind Kind { get; }
            public Location? Location { get; }
            public string[] Args { get; }

            public AnalyzerDiagnosticInfo(AnalyzerDiagnosticKind kind, Location? location, params string[] args)
            {
                Kind = kind;
                Location = location;
                Args = args;
            }
        }

        internal sealed class AnalyzerMethodInfo
        {
            public string MethodName { get; }
            public AnalyzerTypeKind AnalyzerType { get; }
            public Location? Location { get; }

            public AnalyzerMethodInfo(string methodName, AnalyzerTypeKind analyzerType, Location? location)
            {
                MethodName = methodName;
                AnalyzerType = analyzerType;
                Location = location;
            }
        }

        internal sealed class AnalyzerAliasInfo
        {
            public string AliasName { get; }
            public string TargetName { get; }
            public Location? Location { get; }

            public AnalyzerAliasInfo(string aliasName, string targetName, Location? location)
            {
                AliasName = aliasName;
                TargetName = targetName;
                Location = location;
            }
        }

        internal sealed class ResolvedAliasInfo
        {
            public string AliasName { get; }
            public string TargetMethodName { get; }
            public string TargetQualifiedReference { get; }
            public AnalyzerTypeKind AnalyzerType { get; }
            public Location? Location { get; }

            public ResolvedAliasInfo(string aliasName, string targetMethodName, string targetQualifiedReference, AnalyzerTypeKind analyzerType, Location? location)
            {
                AliasName = aliasName;
                TargetMethodName = targetMethodName;
                TargetQualifiedReference = targetQualifiedReference;
                AnalyzerType = analyzerType;
                Location = location;
            }
        }

        internal sealed class AnalyzerPackageInfo
        {
            public string ClassName { get; }
            public string Namespace { get; }
            public string FilePath { get; }
            public PackageKind PackageKind { get; }
            public List<AnalyzerMethodInfo> AnalyzerMethods { get; }
            public List<AnalyzerDiagnosticInfo> Diagnostics { get; }
            public List<AnalyzerAliasInfo> AliasMethods { get; }
            public List<ResolvedAliasInfo> ResolvedAliases { get; }

            public AnalyzerPackageInfo(
                string className,
                string @namespace,
                string filePath,
                PackageKind packageKind,
                List<AnalyzerMethodInfo> analyzerMethods,
                List<AnalyzerDiagnosticInfo> diagnostics,
                List<AnalyzerAliasInfo> aliasMethods,
                List<ResolvedAliasInfo> resolvedAliases)
            {
                ClassName = className;
                Namespace = @namespace;
                FilePath = filePath;
                PackageKind = packageKind;
                AnalyzerMethods = analyzerMethods;
                Diagnostics = diagnostics;
                AliasMethods = aliasMethods;
                ResolvedAliases = resolvedAliases;
            }
        }
    }
}
