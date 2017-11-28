using Newtonsoft.Json;

namespace Model.HandlersData
{
    public class LogoutResponse
    {
        [JsonProperty("message")]
        public string Message;
    }
}
