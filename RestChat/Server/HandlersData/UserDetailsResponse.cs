using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class UserDetailsResponse
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("online")]
        public bool? Online;
    }
}
