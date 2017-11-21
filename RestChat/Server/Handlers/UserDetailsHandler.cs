using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.Exceptions;
using Server.HandlersData;
using Server.Model;

namespace Server.Handlers
{
    public class UserDetailsHandler : SimpleJsonHandler<EmptyData, UserListResponseUser>
    {
        private readonly AuthorizationManager manager;

        public UserDetailsHandler(AuthorizationManager manager) : base(manager)
        {
            this.manager = manager;
        }

        public override Regex Endpoint => new Regex("^/users/(\\w+)$");
        public override string HttpMethod => "GET";

        public override Task<UserListResponseUser> Handle(EmptyData _)
        {
            if (CurrentUser == null)
            {
                throw new HttpException(401);
            }

            var username = EndpointRegexMatch.Groups[1].Value;
            var user = manager.GetAuthorizedUser(username);

            return Task.FromResult(new UserListResponseUser
            {
                Username = user.Username,
                Online = true
            });
        }
    }
}
