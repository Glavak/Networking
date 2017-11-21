using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.HandlersData
{
    public class UserListResponse
    {
        [JsonProperty("users")]
        public List<UserDetailsResponse> Users;
    }
}
