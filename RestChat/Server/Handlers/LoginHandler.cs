using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.HandlersData;
using Server.Model;

namespace Server.Handlers
{
    public class LoginHandler : SimpleJsonHandler<LoginRequest, LoginResponse>
    {
        private readonly AuthorizationManager manager;

        public LoginHandler(AuthorizationManager manager) : base(manager)
        {
            this.manager = manager;
        }

        public override Regex Endpoint => new Regex("^/login$");
        public override string HttpMethod => "POST";

        public override Task<LoginResponse> Handle(LoginRequest requestData)
        {
            var user = manager.AuthorizeUser(requestData.Username);

            return Task.FromResult(new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Online = true,
                Token = user.Token
            });
        }
    }
}
