using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class GetMessagesResponse
    {
        [JsonProperty("messages")]
        public List<PostMessageResponse> Messages;
    }
}
