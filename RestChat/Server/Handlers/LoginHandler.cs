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

        public override Regex GetEndpoint => new Regex("^/login$");
        public override string HttpMethod => "POST";

        public override Task<LoginResponse> Handle(LoginRequest requestData)
        {
            var token = manager.AuthorizeUser(requestData.Username);

            return Task.FromResult(new LoginResponse
            {
                Username = requestData.Username,
                Online = true,
                Token = token
            });
        }
    }
}
