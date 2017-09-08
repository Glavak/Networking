using System;

namespace TreeChat
{
    [Serializable]
    public class Message
    {
        public Guid id;

        public string SenderName;

        public string Text;

        public DateTime Created;

        [NonSerialized] public DateTime LastTransferred;
    }
}
