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
            Headers = new HttpHeaderArray()
            {
                {"Server", "EpgTimerWeb2/1.0"}
            };
            context = c;
        }
        public void Send()
        {
            OutputStream.Flush();
            HttpResponseGenerater.SendResponse(context);
        }
        public MemoryStream OutputStream { get; set; }
        public HttpHeaderArray Headers { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        private HttpContext context;
        public void SetStatus(int Code, string Text)
        {
            StatusCode = Code;
            StatusText = Text;
        }
    }
    public class HttpResponseGenerater
    {
        public static void NotFound(HttpContext Context)
        {
            byte[] NotFound = Encoding.UTF8.GetBytes(@"<html>
<body>
<h1>404 - Not found</h1>
<hr />
EpgTimerWeb(v2) by YUKI
</body>
</html>");
            Context.Response.SetStatus(404, "Not Found");
            Context.Response.OutputStream.Write(NotFound, 0, NotFound.Length);
            Context.Response.Send();
        }
        public static bool SendResponseHeader(HttpContext Context, HttpHeaderArray Input)
        {
            var HeaderText = Encoding.UTF8.GetBytes(HttpHeaderArray.Generate(Input) + "\r\n");
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
                Debug.Print("Response Error: {0}", e.Message);
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
                Debug.Print("Response Error: {0}", e.Message);
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
                Debug.Print("Response Error: {0}", e.Message);
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
    }
}
