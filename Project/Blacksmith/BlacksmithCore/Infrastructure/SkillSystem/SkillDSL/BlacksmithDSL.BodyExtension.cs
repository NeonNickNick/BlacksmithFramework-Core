using BlacksmithCore.Infrastructure.Models.AnalyzableDatas;
using BlacksmithCore.Infrastructure.Models.Components;
using BlacksmithCore.Infrastructure.Models.Player;

namespace BlacksmithCore.Infrastructure.SkillSystem.SkillDSL
{
    public static partial class BlacksmithDSL
    {
        public static void AddMark(this Body body, string markName)
        {
            if (body.Get<Effect>().Marks.TryGetValue(markName, out var mark))
            {
                mark.LayerNum++;
                return;
            }
            var entity = new MarkEntity()
            {
                MarkName = markName,
                Clock = new(isInfinite: true)
            };
            body.Get<Effect>().AddMark(entity);
        }
        public static int CountMark(this Body body, string markName)
        {
            if (body.Get<Effect>().Marks.TryGetValue(markName, out var mark))
            {
                return mark.LayerNum;
            }
            return 0;
        }
        public static int TakeMark(this Body body, string markName)
        {
            if (body.Get<Effect>().Marks.TryGetValue(markName, out var mark))
            {
                var res = mark.LayerNum;
                body.Get<Effect>().Marks.Remove(markName);
                return res;
            }
            return 0;
        }
    }
}
