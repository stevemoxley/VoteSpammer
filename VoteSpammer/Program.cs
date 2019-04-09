using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoteSpammer
{
    class Program
    {
        static void Main(string[] args)
        {

            System.Net.ServicePointManager.DefaultConnectionLimit = 20;

            Console.WriteLine("10Best Vote Spammer");
            Console.WriteLine("---------------------");
            Console.WriteLine("Loading Proxy List");
            ProxyProvider.LoadProxyList();
            Console.WriteLine("---------------------");
            Console.WriteLine("How many votes would you like to send?");
            var numberOfVotes = int.Parse(Console.ReadLine());

            Console.WriteLine("Starting voting");
            StartVoting(numberOfVotes);

            Console.ReadLine();

        }

        static async void StartVoting(int numberOfVotes)
        {
            var allProxies = ProxyProvider.AllWorkingProxies;

            foreach (var webProxy in allProxies)
            {
                Console.WriteLine($"Trying vote on proxy { webProxy.Address }");
                var result = await Vote(webProxy);
                if(result)
                {
                    Console.WriteLine($"Proxy worked. Beginning vote spam");
                    VoteUsingWorkingProxy(numberOfVotes, webProxy);
                }
            }

            ProxyProvider.SaveBadProxies();
        }

        static void VoteUsingWorkingProxy(int numberOfVotes, WebProxy webProxy)
        {
            List<Task<bool>> allTasks = new List<Task<bool>>();

            for (int i = 0; i < numberOfVotes; i++)
            {
                allTasks.Add(Vote(webProxy));
            }

            Task.WaitAll(allTasks.ToArray());

            Console.WriteLine($"Finished voting with proxy { webProxy.Address }");
        }


        private static async Task<WebInfo> GetWebInfo(WebProxy proxy)
        {
            var url = "https://www.10best.com/awards/travel/best-zoo-2019/saint-louis-zoo-st-louis/";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = new CookieContainer();
            request.Proxy = proxy;
            request.Timeout = _timeout;
            request.ReadWriteTimeout = _timeout;

            using (var response = await request.GetResponseAsync())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var httpResponse = (HttpWebResponse)response;
                var document = new HtmlDocument();
                document.LoadHtml(reader.ReadToEnd());

                var voteKey = document.GetElementbyId("votekey").Attributes["value"].Value;
                var voteKeyEncoded = WebUtility.UrlEncode(voteKey);
                var cookieValue = httpResponse.Cookies[1].Value;
                var uniqueId = httpResponse.Cookies[0].Value;

                var webInfo = new WebInfo()
                {
                    CookieNumber = cookieValue,
                    VoteKey = voteKeyEncoded,
                    UniqueID = uniqueId
                };

                return webInfo;
            }
        }

        private static async Task<bool> Vote(WebProxy proxy)
        {
            try
            {
                var webInfo = await GetWebInfo(proxy);

                var url = $"http://www.10best.com/common/ajax/vote.php?voteKey={ webInfo.VoteKey }&c={webInfo.CookieNumber }";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = new CookieContainer();
                request.Proxy = proxy;
                request.Timeout = _timeout;
                request.ReadWriteTimeout = _timeout;

                request.CookieContainer.Add(new Cookie
                {
                    Value = webInfo.CookieNumber,
                    Domain = ".www.10best.com",
                    Path = "/",
                    Expires = DateTime.Now + new TimeSpan(365, 0, 0, 0, 0),
                    Name = "rnd",
                });

                request.CookieContainer.Add(new Cookie
                {
                    Value = webInfo.UniqueID,
                    Domain = ".www.10best.com",
                    Path = "/",
                    Expires = DateTime.Now + new TimeSpan(365, 0, 0, 0, 0),
                    Name = "uniqID"
                });

                using (var response = await request.GetResponseAsync())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    if (result.Contains("Voting is not permitted in your location"))
                    {
                        ProxyProvider.AddBadProxy(proxy);
                        return false;
                    }
                    Console.WriteLine(result);
                    return true;
                }
            }
            catch (WebException webEx)
            {
                Console.WriteLine($"WebException: {webEx.Message}.");
                ProxyProvider.AddBadProxy(proxy);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ProxyProvider.AddBadProxy(proxy);
                return false;
            }

        }

        private static int _timeout = 2000;

        class WebInfo
        {
            public string CookieNumber { get; set; }

            public string VoteKey { get; set; }

            public string UniqueID { get; set; }

        }
    }
}






