using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class ApiException : Exception
    {
        public HttpStatusCode httpStatusCode;

        public ApiException(HttpStatusCode httpStatusCode)
        {
            this.httpStatusCode = httpStatusCode;
        }
    }
}
