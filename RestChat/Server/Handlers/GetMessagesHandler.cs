using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.Exceptions;
using Server.HandlersData;
using Server.Model;

namespace Server.Handlers
{
    public class GetMessagesHandler : SimpleJsonHandler<EmptyData, GetMessagesResponse>
    {
        private readonly MessagesManager messagesManager;

        public GetMessagesHandler(AuthorizationManager authorizationManager, MessagesManager messagesManager)
            : base(authorizationManager)
        {
            this.messagesManager = messagesManager;
        }

        public override Regex Endpoint => new Regex("^/messages$");
        public override string HttpMethod => "GET";

        public override Task<GetMessagesResponse> Handle(EmptyData _)
        {
            if (CurrentUser == null)
            {
                throw new HttpException(401);
            }

            var offsetString = Request.QueryString["offset"];
            int offset;
            if (offsetString == null || !int.TryParse(offsetString, out offset))
            {
                offset = 0;
            }
            else if (offset <= 0)
            {
                throw new HttpException(400);
            }

            var countString = Request.QueryString["count"];
            int count;
            if (countString == null || !int.TryParse(countString, out count))
            {
                count = 10;
            }
            else if (count <= 0 || count > 100)
            {
                throw new HttpException(400);
            }

            var messages = messagesManager.GetMessages(offset, count);

            return Task.FromResult(new GetMessagesResponse
            {
                Messages = messages
                    .Select(m => new PostMessageResponse
                    {
                        Id = m.Id,
                        Message = m.Text,
                        AuthorId = m.AuthorId
                    })
                    .ToList()
            });
        }
    }
}
