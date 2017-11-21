using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class LogoutResponse
    {
        [JsonProperty("message")]
        public string Message;
    }
}
