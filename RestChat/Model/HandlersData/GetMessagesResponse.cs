using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.HandlersData
{
    public class GetMessagesResponse
    {
        [JsonProperty("messages")]
        public List<PostMessageResponse> Messages;
    }
}
