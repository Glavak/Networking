using System;
using System.Collections.Generic;
using System.Linq;
using Server.Exceptions;

namespace Server.Model
{
    public class AuthorizationManager
    {
        private static readonly TimeSpan OfflineTimeout = TimeSpan.FromSeconds(30);

        private readonly List<AuthorizedUser> authorizedUsers;
        private int nextUserId = 1;

        public AuthorizationManager()
        {
            this.authorizedUsers = new List<AuthorizedUser>();
        }

        public AuthorizedUser AuthorizeUser(string username)
        {
            UpdateUsersOnline();

            var user = authorizedUsers.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                user = new AuthorizedUser
                {
                    Id = nextUserId,
                    Username = username,
                    Token = Guid.NewGuid(),
                    LastActivity = DateTime.Now,
                    Online = true
                };
                nextUserId++;
                authorizedUsers.Add(user);
                return user;
            }
            else if (user.Online != true)
            {
                user.LastActivity = DateTime.Now;
                user.Online = true;
                return user;
            }
            else
            {
                throw new UsernameTakenException();
            }
        }

        public void DeauthorizeUser(AuthorizedUser user)
        {
            user.Online = false;
        }

        public AuthorizedUser AuthorizeUser(Guid token)
        {
            UpdateUsersOnline();

            var user = authorizedUsers.FirstOrDefault(u => u.Token == token);

            if (user == null)
            {
                throw new HttpException(403);
            }

            user.LastActivity = DateTime.Now;

            return user;
        }

        public AuthorizedUser GetAuthorizedUser(int id)
        {
            UpdateUsersOnline();

            var user = authorizedUsers.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                throw new HttpException(404);
            }

            return user;
        }

        public IEnumerable<AuthorizedUser> GetAuthorizedUsers()
        {
            UpdateUsersOnline();

            return authorizedUsers;
        }

        private void UpdateUsersOnline()
        {
            foreach (var user in authorizedUsers)
            {
                if (user.Online == true && DateTime.Now - user.LastActivity > OfflineTimeout)
                {
                    user.Online = null;
                }
            }
        }
    }
}
