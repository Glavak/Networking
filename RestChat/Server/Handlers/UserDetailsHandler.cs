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

        public override Regex Endpoint => new Regex("^/users/(\\d+)$");
        public override string HttpMethod => "GET";

        public override Task<UserDetailsResponse> Handle(EmptyData _)
        {
            if (CurrentUser == null)
            {
                throw new HttpException(401);
            }

            var endpointRegexMatch = Endpoint.Match(Request.Url.AbsolutePath);
            var userId = int.Parse(endpointRegexMatch.Groups[1].Value);
            var user = manager.GetAuthorizedUser(userId);

            return Task.FromResult(new UserDetailsResponse
            {
                Id = user.Id,
                Username = user.Username,
                Online = true
            });
        }
    }
}
