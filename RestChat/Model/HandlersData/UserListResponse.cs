using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.HandlersData
{
    public class UserListResponse
    {
        [JsonProperty("users")]
        public List<UserDetailsResponse> Users;
    }
}
