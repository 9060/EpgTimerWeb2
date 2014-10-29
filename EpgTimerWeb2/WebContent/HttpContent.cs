using EpgTimerWeb2.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace EpgTimer
{
    public class HttpContent
    {
        private void SendResponse(HttpContext Context, byte[] Bytes)
        {
            if (!Context.Response.Headers.ContainsKey("Content-Length"))
            {
                Context.Response.Headers.Add("Content-Length", Bytes.LongLength.ToString());
            }
            Context.Response.OutputStream.Write(Bytes, 0, Bytes.Length);
            Context.Response.Send();
        }
        private void SendResponse(HttpContext Context, string Str)
        {
            SendResponse(Context, Encoding.UTF8.GetBytes(Str));
        }
        public static string HTMLAutoIndent(string HTML)
        {
            //return HTML;
            var doc = new XmlDocument();
            doc.LoadXml(HTML);
            var sb = new StringBuilder();
            var xw = XmlTextWriter.Create(sb, new XmlWriterSettings() { Indent = true , OmitXmlDeclaration = true, NewLineHandling = NewLineHandling.Replace});
            doc.WriteTo(xw);
            xw.Flush();
            xw.Close();
            return sb.ToString();
        }
        public static string HTMLAutoIndent(XmlDocument HTML)
        {
            //return HTML;
            var sb = new StringBuilder();
            var xw = XmlTextWriter.Create(sb, new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true, NewLineHandling = NewLineHandling.Replace });
            HTML.WriteTo(xw);
            xw.Flush();
            xw.Close();
            return sb.ToString();
        }
        private void Redirect(HttpContext Context, string Url)
        {
            var Domain = "localhost:8080";
            if (Context.Request.Headers.ContainsKey("host"))
            {
                Domain = Context.Request.Headers["host"];
            }
            Context.Response.StatusCode = 301;
            Context.Response.StatusText = "Moved Permanently";
            Context.Response.Headers["Location"] = "http://" + Domain + Url;
            Context.Response.Send();
        }
        public bool RequestUrl(HttpContext Context)
        {
            var Ret = false;
            switch (Context.Request.Url.ToLower())
            {
                case "/":
                case "/index.html":
                case "/index.htm":
                    Redirect(Context, "/dashboard");
                    Ret = true;
                    break;
                case "/dashboard":
                    Context.Response.Headers.Add("Content-Type", "text/html; charset=utf8");
                    SendResponse(Context,  File.ReadAllText(".\\www\\index.html", Encoding.UTF8));
                    Ret = true;
                    break;
                case "/css/bootstrap.css":
                    Context.Response.Headers.Add("Content-Type", "text/css");
                    SendResponse(Context, Resources.BootStrapStyle);
                    Ret = true;
                    break;
                case "/js/bootstrap.js":
                    Context.Response.Headers.Add("Content-Type", "application/javascript");
                    SendResponse(Context, Resources.BootStrap);
                    Ret = true;
                    break;
                case "/js/jquery.js":
                    Context.Response.Headers.Add("Content-Type", "application/javascript");
                    SendResponse(Context, Resources.JQuery);
                    Ret = true;
                    break;
                case "/js/respond.js":
                    Context.Response.Headers.Add("Content-Type", "application/javascript");
                    SendResponse(Context, Resources.Respond);
                    Ret = true;
                    break;
                case "/js/main.js":
                    Context.Response.Headers.Add("Content-Type", "application/javascript");
                    SendResponse(Context, File.ReadAllText(".\\www\\js\\main.js", Encoding.UTF8));
                    Ret = true;
                    break;
                case "/js/api.js":
                    Context.Response.Headers.Add("Content-Type", "application/javascript");
                    SendResponse(Context, File.ReadAllText(".\\www\\js\\api.js", Encoding.UTF8));
                    Ret = true;
                    break;
                case "/img/not_thumb.png":
                    Context.Response.Headers.Add("Content-Type", "image/png");
                    var Stream = new MemoryStream();
                    Resources.NotThumbnail.Save(Stream, ImageFormat.Png);
                    SendResponse(Context, Stream.GetBuffer());
                    Stream.Close();
                    Ret = true;
                    break;
                case "/img/loader.gif":
                    Context.Response.Headers.Add("Content-Type", "image/gif");
                    var Stream1 = new MemoryStream();
                    Resources.loader.Save(Stream1, ImageFormat.Gif);
                    SendResponse(Context, Stream1.GetBuffer());
                    Stream1.Close();
                    Ret = true;
                    break;
                case "/fonts/glyphicons-halflings-regular.eot":
                    Ret = true;
                    SendResponse(Context, Resources.glyphicons_halflings_regular_eot);
                    break;
                case "/fonts/glyphicons-halflings-regular.svg":
                    Ret = true;
                    SendResponse(Context, Resources.glyphicons_halflings_regular_svg);
                    break;
                case "/fonts/glyphicons-halflings-regular.ttf":
                    Ret = true;
                    SendResponse(Context, Resources.glyphicons_halflings_regular_ttf);
                    break;
                case "/fonts/glyphicons-halflings-regular.woff":
                    Ret = true;
                    SendResponse(Context, Resources.glyphicons_halflings_regular_woff);
                    break;
            }
            return Ret;
        }
    }
}
