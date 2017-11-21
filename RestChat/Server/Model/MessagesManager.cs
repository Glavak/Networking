using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Model
{
    public class MessagesManager
    {
        private readonly List<Message> messages = new List<Message>();

        public Message PostMessage(string text, AuthorizedUser author)
        {
            var message = new Message
            {
                Id = messages.Count,
                Text = text,
                AuthorId = author.Id
            };

            Console.WriteLine(text);
            messages.Add(message);
            return message;
        }

        public IEnumerable<Message> GetMessages(int offset, int count)
        {
            return messages.Skip(offset).Take(count);
        }
    }
}
