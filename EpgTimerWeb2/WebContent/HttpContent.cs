using EpgTimerWeb2.Properties;
using System.Drawing.Imaging;
using System.IO;

namespace EpgTimer
{
    public class HttpContent
    {
        
        public bool RequestUrl(HttpContext Context)
        {
            var Ret = false;
            if (Context.Request.Url.ToLower().StartsWith("/modules/"))
            {
                var PartName = Context.Request.Url.ToLower().Replace("/modules/", "").Replace("/", "\\");
                if (PartName.IndexOf("..\\") < 0 && File.Exists(".\\web\\modules\\" + PartName))
                {
                    string MimeType = Mime.Get(PartName, "application/javascript");
                    if (!Mime.IsImage(PartName))
                        MimeType += "; charset=utf-8";
                    Context.Response.Headers.Add("Content-Type", MimeType);
                    HttpContext.SendResponse(Context, File.ReadAllBytes(".\\web\\modules\\" + PartName));
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
                        Context.Response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                        HttpContext.SendResponse(Context, File.ReadAllBytes(".\\web\\index.html"));
                        Ret = true;
                        break;
                    case "/js/jquery.js":
                        Context.Response.Headers.Add("Content-Type", "application/javascript");
                        HttpContext.SendResponse(Context, Resources.JQuery);
                        Ret = true;
                        break;
                    case "/js/bootstrap.js":
                        Context.Response.Headers.Add("Content-Type", "application/javascript");
                        HttpContext.SendResponse(Context, Resources.BootStrap);
                        Ret = true;
                        break;
                    case "/js/respond.js":
                        Context.Response.Headers.Add("Content-Type", "application/javascript");
                        HttpContext.SendResponse(Context, Resources.Respond);
                        Ret = true;
                        break;
                    case "/css/bootstrap.css":
                        Context.Response.Headers.Add("Content-Type", "text/css");
                        HttpContext.SendResponse(Context, Resources.BootStrapStyle);
                        Ret = true;
                        break;
                    case "/css/bootstrap.css.map":
                        Context.Response.Headers.Add("Content-Type", "text/plain");
                        HttpContext.SendResponse(Context, Resources.BootStrapCssMap);
                        Ret = true;
                        break;
                    case "/css/main.css":
                        Context.Response.Headers.Add("Content-Type", "text/css");
                        HttpContext.SendResponse(Context, File.ReadAllBytes(".\\web\\css\\main.css"));
                        Ret = true;
                        break;
                    case "/img/not_thumb.png":
                        Context.Response.Headers.Add("Content-Type", "image/png");
                        var Stream = new MemoryStream();
                        Resources.NotThumbnail.Save(Stream, ImageFormat.Png);
                        HttpContext.SendResponse(Context, Stream.GetBuffer());
                        Stream.Close();
                        Ret = true;
                        break;
                    case "/img/loader.gif":
                        Context.Response.Headers.Add("Content-Type", "image/gif");
                        var Stream1 = new MemoryStream();
                        Resources.loader.Save(Stream1, ImageFormat.Gif);
                        HttpContext.SendResponse(Context, Stream1.GetBuffer());
                        Stream1.Close();
                        Ret = true;
                        break;
                    case "/fonts/glyphicons-halflings-regular.eot":
                        Ret = true;
                        HttpContext.SendResponse(Context, Resources.glyphicons_halflings_regular_eot);
                        break;
                    case "/fonts/glyphicons-halflings-regular.svg":
                        Ret = true;
                        HttpContext.SendResponse(Context, Resources.glyphicons_halflings_regular_svg);
                        break;
                    case "/fonts/glyphicons-halflings-regular.ttf":
                        Ret = true;
                        HttpContext.SendResponse(Context, Resources.glyphicons_halflings_regular_ttf);
                        break;
                    case "/fonts/glyphicons-halflings-regular.woff":
                        Ret = true;
                        HttpContext.SendResponse(Context, Resources.glyphicons_halflings_regular_woff);
                        break;
                }
            }
            return Ret;
        }
    }
}
