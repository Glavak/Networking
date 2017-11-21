using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class UserDetailsResponse
    {
        [JsonProperty("username")]
        public string Username;

        [JsonProperty("online")]
        public bool Online;
    }
}
