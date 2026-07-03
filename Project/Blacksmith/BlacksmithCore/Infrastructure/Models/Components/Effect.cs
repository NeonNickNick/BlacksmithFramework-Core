using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;
using BlacksmithCore.Infrastructure.Models.Player;
namespace BlacksmithCore.Infrastructure.Models.Components
{
    public class Effect : IUpdatePerRound, IComponent<Body>
    {
        private readonly List<EffectEntity> _effects = new();
        private readonly Dictionary<string, MarkEntity> _marks = new();
        public List<EffectEntity> Effects => _effects;
        public Dictionary<string, MarkEntity> Marks => _marks;
        public void Copy(Effect origin)
        {
            _effects.Clear();
            _marks.Clear();
            foreach (var effect in origin._effects)
            {
                _effects.Add(effect.Copy());
            }
            foreach (var key in origin._marks.Keys)
            {
                _marks[key] = origin._marks[key].Copy();
            }
        }
        public void Add(EffectEntity effectEntity)
        {
            _effects.Add(effectEntity);
        }
        public void AddMark(MarkEntity markEntity)
        {
            if (_marks.TryGetValue(markEntity.MarkName, out var _))
            {
                throw new ArgumentException($"不允许有重名标记<{markEntity.MarkName}>!");
            }
            _marks[markEntity.MarkName] = markEntity;
        }
        public IEnumerable<EffectEntity> Where(EffectType.CEValue type)
        {
            return _effects.Where(e => e.Type == type);
        }
        public void Update()
        {
            _effects.ForEach(e => e.Clock.RoundPass());
            _effects.RemoveAll(e => e.Clock.IsDead);
            foreach (var key in _marks.Keys)
            {
                _marks[key].Clock.RoundPass();
                if (_marks[key].Clock.IsDead)
                {
                    _marks.Remove(key);
                }
            }
        }
    }
}
