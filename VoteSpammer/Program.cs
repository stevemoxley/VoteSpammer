using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VoteSpammer
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("10Best Vote Spammer");
            Console.WriteLine("---------------------");
            Console.WriteLine("How many votes would you like to send?");
            var numberOfVotes = int.Parse(Console.ReadLine());

            for (int i = 0; i < numberOfVotes; i++)
            {
                var webInfo = GetWebInfo();

                var result = Vote(webInfo);

                Console.WriteLine($"{i+1}: {result}");

            }
        }


        private static WebInfo GetWebInfo()
        {
            var url = "http://www.10best.com/awards/travel/best-zoo/saint-louis-zoo-st-louis/";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = new CookieContainer();

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var document = new HtmlDocument();
                    document.LoadHtml(reader.ReadToEnd());

                    var voteKey = document.GetElementbyId("votekey").Attributes["value"].Value;
                    var voteKeyEncoded = WebUtility.UrlEncode(voteKey);
                    var cookieValue = response.Cookies[1].Value;
                    var uniqueId = response.Cookies[0].Value;

                    var webInfo = new WebInfo()
                    {
                        CookieNumber = cookieValue,
                        VoteKey = voteKeyEncoded,
                        UniqueID = uniqueId
                    };

                    return webInfo;
                }
            }

        }

        private static string Vote(WebInfo webInfo)
        {
            var url = $"http://www.10best.com/common/ajax/vote.php?voteKey={ webInfo.VoteKey }&c={webInfo.CookieNumber }";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = new CookieContainer();

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

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    return result;
                }
            }
        }


        class WebInfo
        {
            public string CookieNumber { get; set; }

            public string VoteKey { get; set; }

            public string UniqueID { get; set; }

        }
    }
}






