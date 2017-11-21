using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class PostMessageRequest
    {
        [JsonRequired]
        [JsonProperty("message")]
        public string Message;
    }
}
