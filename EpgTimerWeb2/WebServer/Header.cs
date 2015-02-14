using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class HttpHeaderArray : IEnumerable<KeyValuePair<string, string>>
    {
        private List<KeyValuePair<string, string>> _items = null;
        public HttpHeaderArray()
        {
            _items = new List<KeyValuePair<string, string>>();
        }
        public string this[string key]
        {
            get
            {
                if (_items.Count(s => s.Key.ToLower() == key.ToLower()) > 0)
                    return _items.First(s => s.Key.ToLower() == key.ToLower()).Value;
                throw new KeyNotFoundException();
            }
            set
            {
                _items.Add(new KeyValuePair<string, string>(key, value));
            }
        }
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
        public bool ContainsKey(string key)
        {
            return (_items.Count(s => s.Key.ToLower() == key.ToLower()) > 0);
        }
        public void Add(string Key, string Value)
        {
            _items.Add(new KeyValuePair<string, string>(Key, Value));
        }

        public static string Generate(HttpHeaderArray Input)
        {
            StringBuilder Ret = new StringBuilder();
            foreach (KeyValuePair<string, string> Item in Input)
            {
                Ret.AppendFormat("{0}: {1}\r\n", Item.Key, Item.Value);
            }
            return Ret.ToString();
        }
        public static HttpHeaderArray Parse(Stream Input)
        {
            string Line = "";
            HttpHeaderArray Dict = new HttpHeaderArray();
            while ((Line = HttpCommon.StreamReadLine(Input)) != null)
            {
                if (Line == "") return Dict;
                int Separator = Line.IndexOf(":");
                if (Separator == -1) throw new Exception("Invalid Http Header " + Line);
                var Name = Line.Substring(0, Separator);
                Dict[Name] = Util.RemoveStartSpace(Line.Substring(Separator + 1));
            }
            return Dict;
        }
    }

}
