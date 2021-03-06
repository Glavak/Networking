﻿using System;

namespace TreeChat
{
    [Serializable]
    public class Message
    {
        public readonly Guid Id;

        public Message(Guid id)
        {
            Id = id;
        }

        public string SenderName;

        public string Text;

        public DateTime Created;

        public bool Equals(Message other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Message && Equals((Message) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Message left, Message right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Message left, Message right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"[{this.SenderName}]: {this.Text}";
        }
    }
}
