using System;

namespace Server
{
    public class AssociatedClient
    {
        public Guid guid { get; set; }

        public DateTime LastActivity { get; set; }

        public AssociatedClient(Guid guid, DateTime lastActivity)
        {
            this.guid = guid;
            LastActivity = lastActivity;
        }
    }
}
