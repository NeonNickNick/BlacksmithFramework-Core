using BlacksmithClient.Frontend;
using BlacksmithCore.AI;
using BlacksmithCore.AI.Strategies;
using BlacksmithCore.Infrastructure.Loader;
namespace BlacksmithClient
{
    public static class Program
    {
        public static void Main()
        {
            ModLoader.Initialize(AppContext.BaseDirectory);

            List<IAIStrategy> strategies = new()
            {
                new GeneralStrategy(),
                new AdversarialStrategy(),
            };
            Console.WriteLine("Welcome!\n");
            LocalHost.Start(strategies);
        }

    }
}
