/*
 *  EpgTimerWeb2
 *  Copyright (C) 2015  YukiBoard 0X7hT.k8kU <yuki@yukiboard.tk>
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using EpgTimerWeb2.Properties;
using System.Drawing.Imaging;
using System.IO;

namespace EpgTimer
{
    public class HttpContent
    {

        public bool RequestUrl(HttpContext Context, bool IsAuth)
        {
            var Ret = false;
            var PartName = Context.Request.Url.ToLower().Replace("/", "\\");
            if (PartName.IndexOf("..\\") < 0 && File.Exists(".\\web\\" + PartName) && IsAuth)
            {
                string MimeType = Mime.Get(PartName, "application/javascript");
                if (!Mime.IsImage(PartName))
                    MimeType += "; charset=utf-8";
                Context.Response.Headers.Add("Content-Type", MimeType);
                HttpContext.SendResponse(Context, File.ReadAllBytes(".\\web\\" + PartName));
                Ret = true;
            }
            else
            {
                switch (Context.Request.Url.ToLower())
                {
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
                    case "/js/jquery.datatables.min.js":
                        Context.Response.Headers.Add("Content-Type", "application/javascript");
                        HttpContext.SendResponse(Context, Resources.jqury_dataTables_js);
                        Ret = true;
                        break;
                    case "/js/datatables.bootstrap.js":
                        Context.Response.Headers.Add("Content-Type", "application/javascript");
                        HttpContext.SendResponse(Context, Resources.dataTables_bootstrap);
                        Ret = true;
                        break;
                    case "/js/jquery.datatables.min.css":
                        Context.Response.Headers.Add("Content-Type", "text/css");
                        HttpContext.SendResponse(Context, Resources.jquery_dataTables_css);
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
