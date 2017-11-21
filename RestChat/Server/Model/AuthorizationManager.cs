using System;
using System.Collections.Generic;
using System.Linq;
using Server.Exceptions;

namespace Server.Model
{
    public class AuthorizationManager
    {
        private readonly List<AuthorizedUser> authorizedUsers;

        public AuthorizationManager()
        {
            this.authorizedUsers = new List<AuthorizedUser>();
        }

        public Guid AuthorizeUser(string username)
        {
            if (authorizedUsers.Any(u => u.Username == username))
            {
                throw new UsernameTakenException();
            }

            var token = Guid.NewGuid();

            authorizedUsers.Add(new AuthorizedUser
            {
                Username = username,
                Token = token
            });

            return token;
        }

        public void DeauthorizeUser(AuthorizedUser user)
        {
            authorizedUsers.Remove(user);
        }

        public AuthorizedUser GetAuthorizedUser(Guid token)
        {
            var user = authorizedUsers.FirstOrDefault(u => u.Token == token);

            if (user == null)
            {
                throw new HttpException(403);
            }

            return user;
        }
    }
}
