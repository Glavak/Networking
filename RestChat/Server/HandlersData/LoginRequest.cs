using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class LoginRequest
    {
        [JsonRequired]
        [JsonProperty("username")]
        public string Username;
    }
}
