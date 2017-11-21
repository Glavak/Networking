using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.Exceptions;
using Server.HandlersData;
using Server.Model;

namespace Server.Handlers
{
    public class PostMessageHandler: SimpleJsonHandler<PostMessageRequest, PostMessageResponse>
    {
        private readonly MessagesManager messagesManager;

        public PostMessageHandler(AuthorizationManager authorizationManager, MessagesManager messagesManager)
            : base(authorizationManager)
        {
            this.messagesManager = messagesManager;
        }

        public override Regex Endpoint => new Regex("^/messages$");
        public override string HttpMethod => "POST";

        public override Task<PostMessageResponse> Handle(PostMessageRequest requestData)
        {
            if (CurrentUser == null)
            {
                throw new HttpException(401);
            }

            var message = messagesManager.PostMessage(requestData.Message, CurrentUser);

            return Task.FromResult(new PostMessageResponse
            {
                Id = message.Id,
                Message = message.Text
            });
        }
    }
}
