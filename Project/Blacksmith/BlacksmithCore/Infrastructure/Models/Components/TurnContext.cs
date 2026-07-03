using System.Collections;
using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;
using BlacksmithCore.Infrastructure.Models.LifeCycle;
using BlacksmithCore.Infrastructure.Models.Player;
using BlacksmithCore.Infrastructure.SkillSystem.Analyzer;

namespace BlacksmithCore.Infrastructure.Models.Components
{
    public partial class PreprocessUnit : IAnalyzableData
    {
        public required string AnalyzerKey { get; init; }
        public required ClapRoundClock Clock { get; init; }
    }
    public class TurnContext : IComponent<Body>, IUpdatePerRound
    {

        private Dictionary<Type, IList> _analyzableDataLists = new();
        private Dictionary<Type, List<PreprocessUnit>> _preprocesses = new();

        public TurnContext()
        {
            var types = new HashSet<Type>
            {
                typeof(AttackAnalyzableData),
                typeof(DefenseAnalyzableData),
                typeof(ResourceAnalyzableData),
                typeof(EffectAnalyzableData)
            };
            foreach (var type in types)
            {
                Type listType = typeof(List<>).MakeGenericType(type);
                _analyzableDataLists[type] = (IList)Activator.CreateInstance(listType)!;
                _preprocesses[type] = new();
            }
        }

        public List<TAnalyzableData> Get<TAnalyzableData>()
            where TAnalyzableData : IAnalyzableData
        {
            if (_analyzableDataLists.TryGetValue(typeof(TAnalyzableData), out var list))
            {
                return (List<TAnalyzableData>)list;
            }
            else
            {
                throw new InvalidOperationException(
                    $"AnalyzableData type {typeof(TAnalyzableData).Name} is not registered in the context.");
            }
        }
        public virtual void WriteAnalyzableData(IAnalyzableData analyzableData)
        {
            if (analyzableData == null)
            {
                throw new ArgumentNullException(nameof(analyzableData));
            }

            var pp = _preprocesses[analyzableData.GetType()];
            pp.ForEach(p =>
            {
                if (p.Clock.IsRinging)
                {
                    AnalyzerRegistry.Preprocess.Get(p.AnalyzerKey)(analyzableData);
                }
            });

            if (_analyzableDataLists.TryGetValue(analyzableData.GetType(), out var list))
            {
                list.Add(analyzableData);
            }
            else
            {
                throw new InvalidOperationException(
                    $"AnalyzableData type {analyzableData.GetType().Name} is not registered in the context.");
            }
        }
        public void Copy(TurnContext origin)
        {
            foreach (var key in origin._preprocesses.Keys)
            {
                _preprocesses[key].Clear();
                foreach (var item in origin._preprocesses[key])
                {
                    _preprocesses[key].Add(item.Copy());
                }
            }
            var attack = Get<AttackAnalyzableData>();
            attack.Clear();
            foreach (var a in origin.Get<AttackAnalyzableData>())
            {
                attack.Add(a.Copy());
            }

            var defense = Get<DefenseAnalyzableData>();
            defense.Clear();
            foreach (var a in origin.Get<DefenseAnalyzableData>())
            {
                defense.Add(a.Copy());
            }

            var resource = Get<ResourceAnalyzableData>();
            resource.Clear();
            foreach (var a in origin.Get<ResourceAnalyzableData>())
            {
                resource.Add(a.Copy());
            }

            var effect = Get<EffectAnalyzableData>();
            effect.Clear();
            foreach (var a in origin.Get<EffectAnalyzableData>())
            {
                effect.Add(a.Copy());
            }
        }
        public void AddPreprocess<TAnalyzableData>(
            string analyzerKey,
            ClapRoundClock clock)
            where TAnalyzableData : IAnalyzableData
        {
            _preprocesses[typeof(TAnalyzableData)].Add(new()
            {
                AnalyzerKey = analyzerKey,
                Clock = clock
            });
        }
        public void Update()
        {
            foreach (var list in _preprocesses.Values)
            {
                int n = list.Count;
                for (int i = n - 1; i >= 0; i--)
                {
                    if (list[i].Clock.IsDead)
                    {
                        list.RemoveAt(i);
                        continue;
                    }
                    list[i].Clock.RoundPass();
                }
            }
        }

        public List<(string name, int delayRounds, int power)> GetFutureDefenseView()
        {
            return Get<DefenseAnalyzableData>()
                .Select(d => (d.Defense.GetType().Name, d.Clock.DelayRounds, (int)d.Defense.Power))
                .ToList();
        }
        public List<(string name, int delayRounds, int power)> GetFutureAttackView()
        {
            return Get<AttackAnalyzableData>()
                .Select(a => (a.Type.ToString(), a.Clock.DelayRounds, (int)a.Power))
                .ToList();
        }
    }
}
