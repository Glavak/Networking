using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class UserListResponse
    {
        [JsonProperty("users")]
        public List<UserListResponseUser> Users;
    }

    public class UserListResponseUser
    {
        [JsonProperty("username")]
        public string Username;

        [JsonProperty("online")]
        public bool Online;
    }
}
