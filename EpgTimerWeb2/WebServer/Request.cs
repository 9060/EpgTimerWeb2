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
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace EpgTimer
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string RawUrl { get; set; }
        public string Url { get; set; }
        public string GetParam { get; set; }
        public byte[] PostData { get; set; }
        public string HttpVersion { get; set; }
        public HttpHeaderArray Headers { get; set; }
        public Cookie Cookie { get; set; }
        public string PostString
        {
            get { return PostData.Length == 0 ? "" : Encoding.UTF8.GetString(PostData); }
        }
        public static HttpRequest Parse(Stream Input)
        {
            
            var Start = HttpCommon.StreamReadLine(Input);
            var Request = Start.Split(' ');
            if (Request.Length != 3)
            {
                throw new Exception("Invalid Http Request " + Start);
            }
            var Headers = HttpHeaderArray.Parse(Input);
            var Res = new HttpRequest()
            {
                Method = Request[0].ToUpper(),
                RawUrl = Request[1],
                Headers = Headers,
                HttpVersion = Request[2],
                GetParam = "",
                PostData = new byte[0],
                Url = "",
                Cookie = new Cookie()
            };
            if (Headers.ContainsKey("content-length")) //POSTかも
            {
                Input.ReadTimeout = 1000;
                var ContentLength = 0;
                if (int.TryParse(Headers["content-length"], out ContentLength))
                {
                    if(ContentLength < Setting.Instance.MaxUploadSize && ContentLength > 0)
                        Res.PostData = Util.ReadStream(Input, ContentLength);
                }
                Input.ReadTimeout = Timeout.Infinite;
            }
            if (Res.RawUrl.IndexOf("?") > 0) //GETかも
            {
                Res.GetParam = Res.RawUrl.Substring(Request[1].IndexOf("?") + 1);
                Res.Url = Res.RawUrl.Substring(0, Res.RawUrl.IndexOf("?"));
            }
            else
            {
                Res.Url = Res.RawUrl;
            }
            Res.Url = Uri.UnescapeDataString(Res.Url);
            Res.GetParam = Uri.UnescapeDataString(Res.GetParam);
            if (Res.Headers.ContainsKey("Cookie"))
            {
                Res.Cookie = Cookie.Parse(Res.Headers["Cookie"]);
            }
            return Res;
        }
    }
}
