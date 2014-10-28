using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string RawUrl { get; set; }
        public string Url { get; set; }
        public string GetParam { get; set; }
        public string PostParam { get; set; }
        public string HttpVersion { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }
    public class HttpRequestParser
    {
        public static HttpRequest Parse(Stream Input)
        {
            var Start = HttpCommon.StreamReadLine(Input);
            var Request = Start.Split(' ');
            if (Request.Length != 3)
            {
                throw new Exception("Invalid Http Request " + Start);
            }
            var Headers = HttpHeader.Parse(Input);
            var Pos = Request[1].IndexOf("?");
            var Res = new HttpRequest()
            {
                Method = Request[0].ToUpper(),
                RawUrl = Request[1],
                Headers = Headers,
                HttpVersion = Request[2],
                GetParam = "",
                PostParam = "",
                Url = ""
            };
            if (Headers.ContainsKey("content-length")) //POSTかも
            {
                var ContentLength = 0;
                if (int.TryParse(Headers["content-length"], out ContentLength))
                {
                    var Buffer = new byte[1024];
                    var BufferList = new List<byte>();
                    int Size = 0, AllSize = 0;
                    int oldTo = Input.ReadTimeout;
                    Input.ReadTimeout = Math.Max(Math.Min(ContentLength, 10000), 2000);
                    while ((Size = Input.Read(Buffer, 0, Buffer.Length)) != 0)
                    {
                        var Temp = new byte[Size];
                        Array.Copy(Buffer, Temp, Size);
                        BufferList.AddRange(Temp);
                        AllSize += Size;
                        if (AllSize >= ContentLength) break;
                    }
                    Input.ReadTimeout = oldTo;
                }
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
            Res.PostParam = Uri.UnescapeDataString(Res.PostParam);
            return Res;
        }
    }
}
