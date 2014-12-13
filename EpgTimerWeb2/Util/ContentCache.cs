using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpgTimer
{
    public class ContentCache
    {
        private static ContentCache _instance;
        public static ContentCache Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ContentCache();
                return _instance;
            }
            set { _instance = value; }
        }
        public class CacheItem
        {
            public string Value { set; get; }
            public DateTime ExpireDate { set; get; }
        }
        public ContentCache()
        {
            Cache = new Dictionary<string, CacheItem>();
        }
        public Dictionary<string, CacheItem> Cache { set; get; }
        public string Get(string Url)
        {
            try
            {
                if (!Cache.ContainsKey(Url)) return null;
                if (Cache[Url].ExpireDate <= DateTime.Now)
                {
                    Cache.Remove(Url);
                    return null;
                }
                return Cache[Url].Value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void Set(string Url, string Value, TimeSpan Expire)
        {
            try
            {
                DateTime ExpireDate = DateTime.Now.Add(Expire);
                Cache[Url] = new CacheItem()
                {
                    ExpireDate = ExpireDate,
                    Value = Value
                };
            }
            catch (Exception ex)
            {

            }
        }
        public void ClearAll()
        {
            try
            {
                Cache.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        public bool Contains(string Url)
        {
            if (!Cache.ContainsKey(Url)) return false;
            if (Cache[Url].ExpireDate <= DateTime.Now)
            {
                //Cache.Remove(Url);
                return false;
            }
            return true;
        }
    }
}
