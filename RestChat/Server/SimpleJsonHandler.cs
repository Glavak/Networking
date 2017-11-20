using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Server
{
    public abstract class SimpleJsonHandler<TRequestData, TResponseData> : RestHandler
    {
        public override async Task Handle()
        {
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
