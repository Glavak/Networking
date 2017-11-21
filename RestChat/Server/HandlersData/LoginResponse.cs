using System;
using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class LoginResponse
    {
        [JsonProperty("username")]
        public string Username;

        [JsonProperty("online")]
        public bool Online;

        [JsonProperty("token")]
        public Guid Token;
    }
}
