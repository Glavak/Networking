using System;

namespace Server.Model
{
    public class AuthorizedUser
    {
        public int Id;

        public Guid Token;

        public string Username;
    }
}
