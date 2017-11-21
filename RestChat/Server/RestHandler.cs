using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Server.Exceptions;

namespace Server
{
    public abstract class RestHandler
    {
        public HttpListenerRequest Request;
        public HttpListenerResponse Response;

        public Match EndpointRegexMatch;

        public void StartHandling()
        {
            Task.Run(async () =>
            {
                try
                {
                    await this.Handle().ConfigureAwait(false);
                }
                catch (HttpException e)
                {
                    Response.StatusCode = e.ErrorCode;
                }
                catch (UsernameTakenException)
                {
                    Response.StatusCode = 401;
                    Response.Headers.Add(HttpResponseHeader.WwwAuthenticate, "Token realm='Username is already in use'");
                }
                catch (Exception e)
                {
                    Response.StatusCode = 500;
                    Response.Headers.Add(HttpResponseHeader.ContentType, "text");

                    byte[] buffer = Encoding.UTF8.GetBytes(e.Message);
                    Response.ContentLength64 = buffer.Length;
                    Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                finally
                {
                    Response.OutputStream.Close();
                }
            });
        }

        public abstract Task Handle();

        public abstract Regex Endpoint { get; }

        public abstract string HttpMethod { get; }
    }
}
