using System;

namespace Server.Exceptions
{
    public class HttpException : Exception
    {
        public int ErrorCode;

        public HttpException(int errorCode)
        {
            ErrorCode = errorCode;
        }
    }
}
