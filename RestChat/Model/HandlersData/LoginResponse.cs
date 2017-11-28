using System;
using Newtonsoft.Json;

namespace Model.HandlersData
{
    public class LoginResponse
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("username")]
        public string Username;

        [JsonProperty("online")]
        public bool Online;

        [JsonProperty("token")]
        public Guid Token;
    }
}
