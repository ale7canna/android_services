using System;
using System.Net.Http;
using Org.Apache.Http.Impl.Client;

namespace CoinAppService
{

    public class PageDownloader
    {
        private static readonly string CambiaValuteUrl = "https://cambiavalute.ch/?accepted=true";

        public CambiaValutePage DownloadPage()
        {
            using (var httpClient = new HttpClient())
            {
                //httpClient.BaseAddress = new Uri(CambiaValuteUrl);

                var requestResult = httpClient.GetAsync(new Uri(CambiaValuteUrl)).Result.Content.ReadAsStringAsync().Result;
                return new CambiaValutePage(requestResult);
            }
        }
    }
}