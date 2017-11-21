using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class PostMessageResponse
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("message")]
        public string Message;
    }
}
