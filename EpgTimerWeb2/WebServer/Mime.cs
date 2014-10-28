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
        public static string Get(string path, string mimeProposed)
        {
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
