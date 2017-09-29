using System;
using System.Collections.Concurrent;

namespace TreeChat
{
    public class Peer
    {
        public ConcurrentDictionary<Message, MessageSentData> PendingMessagesLastSendAttempt;
        public MessageSentData LastPinged;
        public bool HasBackupParent;
        public bool AwareOfOurDeath;

        public Peer()
        {
            PendingMessagesLastSendAttempt = new ConcurrentDictionary<Message, MessageSentData>();
            MarkAlive();
        }

        public void MarkAlive()
        {
            LastPinged = new MessageSentData(DateTime.Now, 0);
        }
    }
}
