namespace TreeChat
{
    public enum CommandCode : byte
    {
        ConnectToParent = 10,
        ConnectToParentAck = 12,
        Message = 20,
        MessageAck = 22,
        Ping = 30,
        Pong = 31
    }
}
