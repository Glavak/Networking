using System;

namespace TreeChat
{
    public struct MessageSentData
    {
        public MessageSentData(DateTime lastSendTime, int sendAttempts = 1)
        {
            LastSendTime = lastSendTime;
            SendAttempts = sendAttempts;
        }

        public DateTime LastSendTime { get; set; }

        public int SendAttempts { get; set; }
    }
}
