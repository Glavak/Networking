using System;

namespace Server.Model
{
    public class AuthorizedUser
    {
        private static readonly TimeSpan OfflineTimeout = TimeSpan.FromSeconds(30);

        public int Id;
        public Guid Token;
        public string Username;
        public DateTime LastActivity;

        public bool Online => DateTime.Now - LastActivity < OfflineTimeout;
    }
}
