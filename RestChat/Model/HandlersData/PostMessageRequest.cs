using Newtonsoft.Json;

namespace Model.HandlersData
{
    public class PostMessageRequest
    {
        [JsonRequired]
        [JsonProperty("message")]
        public string Message;
    }
}
