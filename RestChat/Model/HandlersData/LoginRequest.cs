using Newtonsoft.Json;

namespace Model.HandlersData
{
    public class LoginRequest
    {
        [JsonRequired]
        [JsonProperty("username")]
        public string Username;
    }
}
