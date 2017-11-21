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
            var messagesManager = new MessagesManager();

            server.Handlers.Add(new LoginHandler(authorizationManager));
            server.Handlers.Add(new LogoutHandler(authorizationManager));

            server.Handlers.Add(new UserListHandler(authorizationManager));
            server.Handlers.Add(new UserDetailsHandler(authorizationManager));

            server.Handlers.Add(new PostMessageHandler(authorizationManager, messagesManager));

            server.Start();

            while (true)
            {

            }
        }
    }
}
