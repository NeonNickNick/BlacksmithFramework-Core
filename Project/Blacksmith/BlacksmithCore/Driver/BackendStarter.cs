
namespace BlacksmithCore.Driver
{
    public class BackendStarter
    {
        public GameInstance StartBackend()
        {
            GameInstance instance = new(ifRecordInstanceHistory: true);
            return instance;
        }
    }
}
