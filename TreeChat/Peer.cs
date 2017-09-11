using System.Collections.Concurrent;

namespace TreeChat
{
    public class Peer
    {
        public ConcurrentDictionary<Message, MessageSentData> PendingMessagesLastSendAttempt =
            new ConcurrentDictionary<Message, MessageSentData>();
    }
}
