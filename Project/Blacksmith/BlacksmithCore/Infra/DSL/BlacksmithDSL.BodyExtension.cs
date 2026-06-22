using BlacksmithCore.Infra.Models.Components;
using BlacksmithCore.Infra.Models.Components.AnalyzableDatas;
using BlacksmithCore.Infra.Models.Core;
using BlacksmithCore.Infra.Models.Entites;

namespace BlacksmithCore.Infra.DSL
{
    public static partial class BlacksmithDSL
    {
        public static void AddMark(this Body body, string markName)
        {
            var entity = new EffectEntity()
            {
                AnalyzerKey = markName,
                IsMark = true,
                Type = EffectType.Instance.Default(),
                Clock = new(isInfinite: true)
            };
            body.Get<Effect>().Add(entity);
        }
        public static int CountMark(this Body body, string markName)
        {
            var effects = body.Get<Effect>().Effects;
            var marks = effects.FindAll(m => m.AnalyzerKey == markName);
            return marks.Count;
        }
        public static int TakeMark(this Body body, string markName)
        {
            var effects = body.Get<Effect>().Effects;
            var marks = effects.FindAll(m => m.AnalyzerKey == markName);
            var res =  marks.Count;
            foreach (var mark in marks)
            {
                effects.Remove(mark);
            }
            return res;
        }
    }
}
