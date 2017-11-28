using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.Exceptions;
using Model.HandlersData;
using Server.Model;

namespace Server.Handlers
{
    public class UserListHandler : SimpleJsonHandler<EmptyData, UserListResponse>
    {
        private readonly AuthorizationManager manager;

        public UserListHandler(AuthorizationManager manager) : base(manager)
        {
            this.manager = manager;
        }

        public override Regex Endpoint => new Regex("^/users$");
        public override string HttpMethod => "GET";

        public override Task<UserListResponse> Handle(EmptyData _)
        {
            if (CurrentUser == null)
            {
                throw new HttpException(401);
            }

            return Task.FromResult(new UserListResponse
            {
                Users = manager
                    .GetAuthorizedUsers()
                    .Select(aU => new UserDetailsResponse
                    {
                        Id = aU.Id,
                        Username = aU.Username,
                        Online = aU.Online ? (bool?) true : null
                    })
                    .ToList()
            });
        }
    }
}
