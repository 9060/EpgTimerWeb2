using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class Mime
    {
        public static bool IsImage(string path)
        {
            if (path.IndexOf(".") < 0) return false;
            var split = path.Split(new char[] { '.' });
            var ext = split[split.Length - 1];
            switch (ext.ToLower())
            {
                case "png":
                case "jpg":
                case "bmp":
                case "ico":
                    return true;
                default:
                    return false;
            }
        }
        public static string Get(string path, string mimeProposed)
        {
            if (path.IndexOf(".") < 0) return mimeProposed;
            var split = path.Split(new char[] { '.' });
            var ext = split[split.Length - 1];
            var key = Registry.ClassesRoot.OpenSubKey("." + ext);
            if (key == null)
                return mimeProposed;
            var mime = key.GetValue("Content Type");
            if (ext == ".css") mime = "text/css";
            if (mime == null)
                return mimeProposed;
            else
                return (string)mime;
        }
    }
}
