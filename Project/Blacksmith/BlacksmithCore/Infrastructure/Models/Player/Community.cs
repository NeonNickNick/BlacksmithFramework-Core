using BlacksmithCore.Infrastructure.Models.LifeCycle;

namespace BlacksmithCore.Infrastructure.Models.Player
{
    public class Community
    {
        private class Unit
        {
            public Action Action;
            public ClapRoundClock Clock;
            public Unit(Action action, ClapRoundClock clock)
            {
                Action = action;
                Clock = clock;
            }
        }
        public Body Focus { get; set; }
        public bool IsPlayer { get; private set; }
        public string CurrentSkillName { get; set; } = null!;
        private readonly Dictionary<string, List<Body>> _summonLists = new();
        public List<Body> SummonList
        {
            get
            {
                var res = new List<Body>();
                foreach(var list in _summonLists.Values)
                {
                    res.AddRange(list);
                }
                return res;
            }
        }
        public Community(bool isPlayer)
        {
            Focus = new(this, "Main");
            IsPlayer = isPlayer;
        }
        public void Copy(Community origin)
        {
            Focus.Copy(origin.Focus);
            IsPlayer = origin.IsPlayer;
            CurrentSkillName = origin.CurrentSkillName;

            _summonLists.Clear();
            foreach (var key in origin._summonLists.Keys)
            {
                _summonLists[key] = new();
                foreach (var body in origin._summonLists[key])
                {
                    var copy = new Body(this, body.Name);
                    copy.Copy(body);
                    _summonLists[key].Add(copy);
                }
            }
        }
        public List<Body> CreateSummonList(string listName)
        {
            if (_summonLists.TryGetValue(listName, out var _))
            {
                throw new ArgumentException($"不允许有重名召唤物列表<{listName}>!");
            }
            _summonLists[listName] = new();
            return _summonLists[listName];
        }
        public void Update()
        {
            Focus.Update();
            foreach (var summonList in _summonLists.Values)
            {
                foreach (var s in summonList)
                {
                    s.Update();
                }
            }
        }
    }
}