using EpgTimerWeb2;
using EpgTimerWeb2.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        public static void Redirect(HttpContext Context, string Url)
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
            if (Context.Request.Url.ToLower().StartsWith("/modules/"))
            {
                var PartName = Context.Request.Url.ToLower().Replace("/modules/", "").Replace("/", "\\");
                if (File.Exists(".\\modules\\" + PartName))
                {
                    Context.Response.Headers.Add("Content-Type", Mime.Get(PartName, "application/javascript") +( PartName.EndsWith(".png") ? "" : " ;charset=utf8"));
                    if (PartName.EndsWith(".png"))
                        SendResponse(Context, File.ReadAllBytes(".\\modules\\" + PartName));
                    else
                        SendResponse(Context, File.ReadAllText(".\\modules\\" + PartName, Encoding.UTF8));
                    Ret = true;
                }
            }
            else
            {
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
                        SendResponse(Context, File.ReadAllText(".\\www\\index.html", Encoding.UTF8));
                        Ret = true;
                        break;
                    case "/css/bootstrap.css":
                        Context.Response.Headers.Add("Content-Type", "text/css");
                        SendResponse(Context, Resources.BootStrapStyle);
                        Ret = true;
                        break;
                    case "/css/site.css":
                        Context.Response.Headers.Add("Content-Type", "text/css");
                        SendResponse(Context, File.ReadAllText(".\\www\\main.css", Encoding.UTF8));
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
            }
            return Ret;
        }
    }
}
