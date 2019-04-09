using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VoteSpammer
{
    public static class ProxyProvider
    {

        public static void LoadProxyList()
        {
            var proxyLines = File.ReadAllLines("proxyList.txt");

            _badProxies = JsonConvert.DeserializeObject<List<WebProxy>>(File.ReadAllText("badProxies.txt"));

            if(_badProxies == null)
            {
                _badProxies = new List<WebProxy>();
            }

            foreach (var proxy in proxyLines)
            {
                var webProxy = new WebProxy(proxy);

                if (_badProxies.Select(p => p.Address).Contains(webProxy.Address))
                    continue;

                _proxyList.Add(webProxy);
            }
        }

        public static List<WebProxy> AllProxies
        {
            get
            {
                return _proxyList;
            }
        }

        public static List<WebProxy> AllWorkingProxies
        {
            get
            {
                return _proxyList.Where(p => !_badProxies.Contains(p)).ToList();
            }
        }

        public static void AddBadProxy(WebProxy proxy)
        {
            _badProxies.Add(proxy);
            SaveBadProxies();
        }

        public static void SaveBadProxies()
        {
            var json = JsonConvert.SerializeObject(_badProxies);
            File.WriteAllText("badProxies.txt", json);
        }

        public static bool IsBadProxy(WebProxy proxy)
        {
            return _badProxies.Contains(proxy);
        }

        private static List<WebProxy> _badProxies = new List<WebProxy>();
        private static List<WebProxy> _proxyList = new List<WebProxy>();

    }

}
