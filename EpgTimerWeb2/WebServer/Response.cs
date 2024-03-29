﻿/*
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
using System.Diagnostics;
using System.IO;
using System.Text;

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
                {"Server", "EpgTimerWeb2/1.0"},
                {"Date", DateTime.Now.ToString("R")},
                {"Content-Language", "ja"},
                {"Connection", "close"}
            };
            context = c;
        }
        public void Send()
        {
            OutputStream.Flush();
            HttpResponse.SendResponse(context);
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
        public static void StatusPage(HttpContext Context, string Reason)
        {
            byte[] Page = Encoding.UTF8.GetBytes(string.Format(@"<html>
<body>
<h1>{0} - {1}</h1>
{2}
<hr />
EpgTimerWeb(v2) by YUKI
</body>
</html>", Context.Response.StatusCode, Context.Response.StatusText, Reason));
            Context.Response.OutputStream.Write(Page, 0, Page.Length);
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
                lock (Context.LockObject)
                {
                    Input.CopyTo(Context.HttpStream);
                }
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
                lock (Context.LockObject)
                {
                    Context.HttpStream.Write(Input, 0, Input.Length);
                }
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
                lock (Context.LockObject)
                {
                    Context.HttpStream.Write(Input, 0, Input.Length);
                }
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
