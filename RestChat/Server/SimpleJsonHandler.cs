using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Server.Model;

namespace Server
{
    public abstract class SimpleJsonHandler<TRequestData, TResponseData> : RestHandler
    {
        public AuthorizedUser CurrentUser;

        private readonly AuthorizationManager manager;

        protected SimpleJsonHandler(AuthorizationManager manager)
        {
            this.manager = manager;
        }

        public override async Task Handle()
        {
            if (Request.Headers["Authorization"] != null)
            {
                Guid token = Guid.Parse(Request.Headers["Authorization"]);
                CurrentUser = manager.AuthorizeUser(token);
            }

            string requestJson;
            using (var textReader = new StreamReader(Request.InputStream, Request.ContentEncoding))
            {
                requestJson = await textReader.ReadToEndAsync().ConfigureAwait(false);
            }
            TRequestData requestData = JsonConvert.DeserializeObject<TRequestData>(requestJson);

            TResponseData responseData = await Handle(requestData).ConfigureAwait(false);

            var responseJson = JsonConvert.SerializeObject(responseData);

            Response.StatusCode = 200;
            Response.Headers.Add(HttpResponseHeader.ContentType, "application/json");

            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
            Response.ContentLength64 = buffer.Length;
            Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public abstract Task<TResponseData> Handle(TRequestData requestData);
    }
}
