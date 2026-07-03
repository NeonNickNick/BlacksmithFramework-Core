using BlacksmithCore.Driver;
using BlacksmithCore.Infrastructure.Attributes.Analyzer;
using BlacksmithCore.Infrastructure.Attributes.SkillMarkOnly;
using BlacksmithCore.Infrastructure.Attributes.SkillMetadata;
using BlacksmithCore.Infrastructure.Judgement;
using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;
using BlacksmithCore.Infrastructure.Models.Components;
using BlacksmithCore.Infrastructure.Models.Core;
using BlacksmithCore.Infrastructure.Models.Entites;
using BlacksmithCore.Infrastructure.Models.Profession;
using BlacksmithCore.Infrastructure.SkillSystem.SkillDSL;
using ModExamples.PhantomBookMod.Defense;

namespace ModExamples.PhantomBookMod
{
    using DSL = BlacksmithDSL;
    using Pen = Func<BlacksmithDSL.SourceFile, BlacksmithDSL.SourceFile>;
    [IsExperimental]
    public partial class PhantomBook : SkillPackageDefinition
    {
        private static bool FantasiaCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Iron(), 0.5f);
        }
        private static IDSLSourceFile Fantasia(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(0.5f, ResourceType.Instance.Iron())
                .WriteResource(1, ResourceType.Instance.Dream());
            return DSL.CreateBy(pen);
        }

        private static bool AssociationCheck(ISkillCheckContext sc)
        {
            string expectedSkill = sc.SkillDeclareData.StringParam;
            var copyInstance = sc.SudoOperations.DeepCopy();
            var fakeSelf = sc.Self.IsPlayer ? copyInstance.Enemy : copyInstance.Player;
            var fakeEnemy = sc.Self.IsPlayer ? copyInstance.Player : copyInstance.Enemy;
            var fakeSelfSkill = fakeSelf.Focus.Get<Skill>();
            var fakeEnemySkill = fakeEnemy.Focus.Get<Skill>();
            if (sc.SkillDeclareData.Next == null)
            {
                return false;
            }
            var fsc = new DefaultSkillContext
            {
                SudoOperations = copyInstance,
                Self = fakeSelf,
                SkillDeclareData = sc.SkillDeclareData.Next
            };
            var fec = new DefaultSkillContext
            {
                SudoOperations = copyInstance,
                Self = fakeEnemy,
                SkillDeclareData = sc.SkillDeclareData.Next
            };
            if (ProfessionRegistry.EquipmentSkillNames.Contains(expectedSkill) ||
                ProfessionRegistry.ProfessionSkillNames.Contains(expectedSkill) ||
                expectedSkill == $"{nameof(Association).ToLower()}" ||
                fakeSelfSkill.TryDeclare(fsc) != SkillDeclareResult.Success ||
                fakeEnemySkill.TryDeclare(fec) == SkillDeclareResult.Success)
            {
                return false;
            }
            copyInstance.ReturnToPool();
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        private static IDSLSourceFile Association(ISkillExecuteContext sc)
        {
            string expectedSkill = sc.SkillDeclareData.StringParam;
            var copyInstance = sc.SudoOperations.DeepCopy();
            var fakeSelf = sc.Self.IsPlayer ? copyInstance.Enemy : copyInstance.Player;
            var fakeSkill = fakeSelf.Focus.Get<Skill>();
            var fsc = new DefaultSkillContext
            {
                SudoOperations = copyInstance,
                Self = fakeSelf,
                SkillDeclareData = SkillDeclareData.Parse($"{expectedSkill}(p:{sc.SkillDeclareData.Param},s:{sc.SkillDeclareData.StringParam})")!
            };
            var stolenSF = (DSL.SourceFile)fakeSkill.Declare(fsc.SkillDeclareData.SkillName, fsc);
            copyInstance.ReturnToPool();
            return stolenSF
                .Exclude(new HashSet<DSL.SentenceType>
                {
                    DSL.SentenceType.ResourceUse,
                    DSL.SentenceType.HPLose
                })
                .UseResource(2f, ResourceType.Instance.Dream());
        }
        private static bool HallucinateCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        private static IDSLSourceFile Hallucinate(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
               .UseResource(2f, ResourceType.Instance.Dream())
               .WriteEffect(
                EffectType.Instance.AfterAnalyzableDataWritten(),
                EffectTargetType.Instance.Enemy(),
                new(),
                nameof(HallucinateEffectAnalyzer));
            return DSL.CreateBy(pen);
        }
        [IsAnalyzer(AnalyzerType.DSL)]
        public static void HallucinateEffectAnalyzer(Community player, Community enemy, IAnalyzableData analyzable)
        {
            var tc = enemy.Focus.Get<TurnContext>();
            tc.Get<AttackAnalyzableData>().ForEach(a => a.Clock.DelayRounds++);
            tc.AddPreprocess<AttackAnalyzableData>(
                nameof(HallucinatePreprocessAnalyzer),
                new(isInfinite: true));
        }
        [IsAnalyzer(AnalyzerType.Preprocess)]
        public static void HallucinatePreprocessAnalyzer(IAnalyzableData analyzableData)
        {
            analyzableData.Clock.DelayRounds++;
        }
        private static bool AwakeningCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 2f);
        }
        [IsExperimental]
        [IsHighCost]
        private static IDSLSourceFile Awakening(ISkillExecuteContext sc)
        {
            Body cb = null!;
            Pen pen = sf => sf
                .WriteFree(source =>
                {
                    var sandBoxInstance = sc.SudoOperations.DeepCopy(roundCount: 3);
                    Body copiedBody = source.IsPlayer ? sandBoxInstance.Player.Focus : sandBoxInstance.Enemy.Focus;
                    var resource = copiedBody.Get<Resource>();
                    float m = MathF.Min(2f, resource.QueryAll(ResourceType.Instance.Dream()));
                    resource.Use(ResourceType.Instance.Dream(), m);
                    cb = copiedBody;
                })
                .RegistCallbackOnJudge(new()
                {
                    new ModifierCallback()
                    {

                    }
                })
            return DSL.CreateBy(pen);
        }
        private static bool IllusionCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 1f);
        }
        private static IDSLSourceFile Illusion(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .GainHP(5);
            return DSL.CreateBy(pen);
        }
        private static bool NightmareCheck(ISkillCheckContext sc)
        {
            return sc.Self.Focus.Get<Resource>().Check(ResourceType.Instance.Dream(), 3f)
                && sc.Self.Focus.Get<Health>().HP > 1;
        }
        [IsExperimental]
        [IsEquipmentSkill]
        private static IDSLSourceFile Nightmare(ISkillExecuteContext sc)
        {
            Pen pen = sf => sf
                .UseResource(1f, ResourceType.Instance.Dream())
                .LoseHP(5)
                .WriteDefense(0f, new PhysicalImmunity())
                .WriteDefense(6f, new NightmareArmor(owner =>
                {
                    owner.Focus.Get<Skill>().AddSkill(nameof(PhantomBook), nameof(Nightmare));
                    owner.Focus.Get<Skill>().RemovePackage(nameof(Nightmare));
                }))
                .WriteFree(source =>
                {
                    source.Focus.Get<Skill>().RemoveSkill(nameof(PhantomBook), nameof(Nightmare));
                    source.Focus.Get<Skill>().AddPackage(new(new Nightmare()));
                }, false);
            return DSL.CreateBy(pen);
        }
    }
}
