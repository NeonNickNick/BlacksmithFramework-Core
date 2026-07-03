using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;

namespace ModExamples.CauldronMod.Defense
{
    public class StoneShell : DefenseEntity
    {
        public override DefenseType.CEValue Type { get; set; } = DefenseType.Instance.StoneShell();
        public override int Power { get; set; } = 0;
        public override bool CanMerge { get; set; } = true;
        public override bool IsDead { get; set; } = false;
        public override void Merge(DefenseEntity addition)
        {
            //什么都不做即可
        }
        public override void Update()
        {
            //什么都不做即可
        }
        public override (int, int) Work(Body source, Body owner, int attack, AttackType.CEValue type)
        {
            var res = (0, attack);
            IsDead = true;
            return res;
        }
    }
}
