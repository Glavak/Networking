using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RestServer
    {
        private readonly HttpListener httpListener;

        public List<RestHandler> Handlers;

        public RestServer()
        {
            httpListener = new HttpListener();
            Handlers = new List<RestHandler>();
            httpListener.Prefixes.Add("http://127.0.0.1:4242/");
        }

        public void Start()
        {
            httpListener.Start();
            Task.Run(Loop);
        }

        public void Stop()
        {
            httpListener.Stop();
        }

        private async Task Loop()
        {
            while (true)
            {
                var context = await httpListener.GetContextAsync().ConfigureAwait(false);
                var url = context.Request.Url.AbsolutePath;

                var handler = FindHandler(url, context.Request.HttpMethod);
                if (handler == null)
                {
                    context.Response.StatusCode = 404;
                    context.Response.Headers.Add(HttpResponseHeader.ContentType, "text");

                    byte[] buffer = Encoding.UTF8.GetBytes("No API handlers to process your request");
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    continue;
                }

                handler.Request = context.Request;
                handler.Response = context.Response;
                handler.StartHandling();
            }
        }

        private RestHandler FindHandler(string url, string httpMethod)
        {
            return Handlers.FirstOrDefault(handler =>
                handler.GetEndpoint.IsMatch(url) && handler.HttpMethod == httpMethod);
        }
    }
}
