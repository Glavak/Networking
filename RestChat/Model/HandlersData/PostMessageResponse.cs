using Newtonsoft.Json;

namespace Model.HandlersData
{
    public class PostMessageResponse
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("author")]
        public int AuthorId;
    }
}
