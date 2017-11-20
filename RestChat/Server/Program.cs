using Server.Handlers;
using Server.Model;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            RestServer server = new RestServer();

            var authorizationManager = new AuthorizationManager();
            server.Handlers.Add(new LoginHandler(authorizationManager));

            server.Start();

            while (true)
            {

            }
        }
    }
}
