using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class HttpResponse
    {
        public HttpResponse(HttpContext c)
        {
            StatusCode = 200;
            StatusText = "OK";
            OutputStream = new MemoryStream();
            Headers = new Dictionary<string, string>()
            {
            };
            context = c;
        }
        public void Send()
        {
            OutputStream.Flush();
            HttpResponseGenerater.SendResponse(context);
        }
        public MemoryStream OutputStream { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        private HttpContext context;
    }
    public class HttpResponseGenerater
    {
        public static bool SendResponseHeader(HttpContext Context, IDictionary<string,string> Input)
        {
            var HeaderText = Encoding.UTF8.GetBytes(HttpHeader.Generate(Input) + "\r\n");
            return SendResponseBody(Context, HeaderText);
        }
        public static bool SendResponseBody(HttpContext Context, Stream Input)
        {
            try
            {
                if (!Context.Client.Connected) return false;
                Input.CopyTo(Context.HttpStream);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool SendResponseBody(HttpContext Context, byte[] Input)
        {
            try
            {
                if (!Context.Client.Connected) return false;
                Context.HttpStream.Write(Input, 0, Input.Length);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool SendResponseBodyWS(HttpContext Context, byte[] Input)
        {
            try
            {
                if (!Context.Client.Connected) return false;
                Context.IsWsSend = true;
                Context.HttpStream.Write(Input, 0, Input.Length);
                Context.IsWsSend = false;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static bool SendResponseCode(HttpContext Context)
        {
            var ResHeader = Encoding.UTF8.GetBytes(
                String.Format("HTTP/1.1 {0} {1}\r\n",
                Context.Response.StatusCode, Context.Response.StatusText));
            return SendResponseBody(Context, ResHeader);
        }
        public static bool SendResponse(HttpContext Context)
        {
            Context.Response.OutputStream.Seek(0, SeekOrigin.Begin);
            if (!Context.Response.Headers.ContainsKey("Content-Length"))
            {
                Context.Response.Headers["Content-Length"] = Context.Response.OutputStream.Length.ToString();
            }
            SendResponseCode(Context);
            SendResponseHeader(Context, Context.Response.Headers);
            return SendResponseBody(Context, Context.Response.OutputStream);
        }
        public static bool SendFileResponse(HttpContext Context, string Path)
        {
            using (FileStream Stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (!Context.Response.Headers.ContainsKey("Content-Length"))
                {
                    Context.Response.Headers["Content-Length"] = (new FileInfo(Path).Length).ToString();
                }
                Context.Response.Headers["Content-Type"] = Mime.Get(Path, "application/x-javascript");
                SendResponseCode(Context);
                SendResponseHeader(Context, Context.Response.Headers);
                Stream.Position = 0;
                if (!SendResponseBody(Context, Stream))
                {
                    Debug.Print(Path);
                    Stream.Close();
                    return false;
                }
                Stream.Close();
            }
            return true;
        }
        
    }
}
