using System;
using System.Collections.Generic;
using System.Linq;
using Server.Exceptions;

namespace Server.Model
{
    public class AuthorizationManager
    {
        private readonly List<AuthorizedUser> authorizedUsers;
        private int nextUserId = 1;

        public AuthorizationManager()
        {
            this.authorizedUsers = new List<AuthorizedUser>();
        }

        public AuthorizedUser AuthorizeUser(string username)
        {
            if (authorizedUsers.Any(u => u.Username == username))
            {
                throw new UsernameTakenException();
            }

            var user = new AuthorizedUser
            {
                Id = nextUserId,
                Username = username,
                Token = Guid.NewGuid()
            };
            nextUserId++;

            authorizedUsers.Add(user);
            return user;
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

        public AuthorizedUser GetAuthorizedUser(int id)
        {
            var user = authorizedUsers.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                throw new HttpException(404);
            }

            return user;
        }

        public IEnumerable<AuthorizedUser> GetAuthorizedUsers()
        {
            return authorizedUsers;
        }
    }
}
