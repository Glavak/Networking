using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.Exceptions;
using Server.HandlersData;
using Server.Model;

namespace Server.Handlers
{
    public class UserDetailsHandler : SimpleJsonHandler<EmptyData, UserDetailsResponse>
    {
        private readonly AuthorizationManager manager;

        public UserDetailsHandler(AuthorizationManager manager) : base(manager)
        {
            this.manager = manager;
        }

        public override Regex Endpoint => new Regex("^/users/(\\w+)$");
        public override string HttpMethod => "GET";

        public override Task<UserDetailsResponse> Handle(EmptyData _)
        {
            if (CurrentUser == null)
            {
                throw new HttpException(401);
            }

            var username = EndpointRegexMatch.Groups[1].Value;
            var user = manager.GetAuthorizedUser(username);

            return Task.FromResult(new UserDetailsResponse
            {
                Username = user.Username,
                Online = true
            });
        }
    }
}
