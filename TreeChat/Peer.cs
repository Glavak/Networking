using System;
using System.Collections.Concurrent;

namespace TreeChat
{
    public class Peer
    {
        public ConcurrentDictionary<Message, DateTime> PendingMessagesLastSendAttempt =
            new ConcurrentDictionary<Message, DateTime>();
    }
}
