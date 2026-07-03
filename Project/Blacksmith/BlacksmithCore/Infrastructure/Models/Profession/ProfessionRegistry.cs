using System.Reflection;
using BlacksmithCore.Infrastructure.Attributes.SkillMetadata;
using BlacksmithCore.Infrastructure.Attributes.SkillMetadata.Core;
using BlacksmithCore.Infrastructure.SkillSystem.SkillDSL;

namespace BlacksmithCore.Infrastructure.Models.Profession
{
    public static class ProfessionRegistry
    {
        public static readonly HashSet<string> SkillPackages = new();
        private static readonly Lazy<IReadOnlySet<string>> _professionSkillNames = new(() =>
            SkillMetadataDict!
            .Where(kvp => kvp.Value.Any(isc => isc.GetType() == typeof(IsProfessionSkill)))
            .Select(kvp => kvp.Key)
            .ToHashSet()
        );
        public static IReadOnlySet<string> ProfessionSkillNames => _professionSkillNames.Value;
        private static readonly Lazy<IReadOnlySet<string>> _equipmentSkillNames = new(() =>
            SkillMetadataDict!
            .Where(kvp => kvp.Value.Any(isc => isc.GetType() == typeof(IsEquipmentSkill)))
            .Select(kvp => kvp.Key)
            .ToHashSet()
        );
        public static IReadOnlySet<string> EquipmentSkillNames => _equipmentSkillNames.Value;
        public static readonly Dictionary<string, HashSet<ISkillMetadata>> SkillMetadataDict = new();
        private static readonly Dictionary<string, List<SkillPackageModifier>> _modifierInstances = new();

        public static void RegistSkillPackageName(string professionName)
        {
            if (SkillPackages.Contains(professionName))
            {
                throw new ArgumentException($"Profession \"{professionName}\" already exists! Expansion addition failed!");
            }
            SkillPackages.Add(professionName);
            Console.WriteLine($"Successfully added the extended profession \"{professionName}\"!");
        }

        public static void CollectSkillMetadata(SkillPackageBase package)
        {
            static bool IsValidSkillMethod(MethodInfo method)
            {
                return method.IsPrivate
                    && method.ReturnType == typeof(IDSLSourceFile)
                    && method.GetParameters() is { Length: 1 } parameters
                    && parameters[0].ParameterType == typeof(ISkillExecuteContext);
            }
            var minfos = package.GetType().GetMethods(
                BindingFlags.NonPublic | BindingFlags.Static
            );

            foreach (var info in minfos)
            {
                if (!IsValidSkillMethod(info))
                {
                    continue;
                }

                // 直接覆盖
                SkillMetadataDict[info.Name.ToLower()] = new();
                var metadatas = info.GetCustomAttributes();
                if (metadatas == null)
                {
                    continue;
                }
                foreach (var m in metadatas)
                {
                    if (m is ISkillMetadata metadata)
                    {
                        SkillMetadataDict[info.Name.ToLower()].Add(metadata);
                    }
                }
            }
        }

        public static void RegistSkillPackageModifier(string targetName, SkillPackageModifier modifier)
        {
            if (!_modifierInstances.TryGetValue(targetName, out var list))
            {
                _modifierInstances[targetName] = list = new();
            }
            list.Add(modifier);
        }

        public static void AddModOnInit(SkillPackageDefinition package)
        {
            if (_modifierInstances.TryGetValue(package.GetType().Name, out var instances))
            {
                foreach (var instance in instances)
                {
                    var modifier = (SkillPackageModifier)instance.Copy();
                    package.AvailableSkillNames.UnionWith(modifier.AvailableSkillNames);
                    foreach (var kv in modifier.SkillChecker)
                    {
                        package.SkillChecker[kv.Key] = kv.Value;
                    }
                    foreach (var kv in modifier.SkillSourceFileGenerator)
                    {
                        package.SkillSourceFileGenerator[kv.Key] = kv.Value;
                    }
                }
            }
        }
    }
}
