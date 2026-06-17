using System.Collections;
using ClapInfra.ClapUnit;

namespace ClapInfra.ClapModels.Components
{
    public interface IClapAnalyzableData<TCommunity>
    {
        public ClapRoundClock Clock { get; init; }
        public string AnalyzerKey { get; init; }
    }
    public abstract class ClapTurnContext<TIAnalyzableData, TCommunity>
        where TIAnalyzableData : IClapAnalyzableData<TCommunity>
	{
        protected Dictionary<Type, IList> _analyzableDataLists = new();
        protected ClapTurnContext(HashSet<Type> analyzableDataTypes)
        {
            if (analyzableDataTypes == null)
            {
                throw new ArgumentNullException(nameof(analyzableDataTypes));
            }
            foreach (var type in analyzableDataTypes)
            {
                if (type == null)
                {
                    throw new ArgumentException("AnalyzableData type cannot be null!");
                }
                if (!(type.IsAssignableTo(typeof(TIAnalyzableData))))
                {
                    throw new ArgumentException($"AnalyzableData must derive from {nameof(TIAnalyzableData)}");
                }
                Type listType = typeof(List<>).MakeGenericType(type);
                _analyzableDataLists[type] = (IList)Activator.CreateInstance(listType)!;
            }
        }
        public List<TAnalyzableData> Get<TAnalyzableData>()
            where TAnalyzableData : TIAnalyzableData
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
        public virtual void WriteAnalyzableData(TIAnalyzableData analyzableData)
        {
            if (analyzableData == null)
            {
                throw new ArgumentNullException(nameof(analyzableData));
            }

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
    }
}
