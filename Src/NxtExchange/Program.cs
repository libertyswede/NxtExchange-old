using NxtExchange.DAL;
using NxtLib;

namespace NxtExchange
{
    class Program
    {
        private const string NxtUri = "http://localhost:6876/nxt";

        static void Main()
        {
            var controller = new NxtController(new NxtRepository(), new NxtConnector(new ServiceFactory(NxtUri)));
        }
    }
}
