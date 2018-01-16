using System;
using System.Net.Http;

namespace BusinessLogic
{

    public class PageDownloader
    {
        private static readonly string CambiaValuteUrl = "https://cambiavalute.ch/?accepted=true";
        private static readonly string XeChangeUrl = "http://www.xe.com/it/currencyconverter/convert/?From=CHF&To=EUR";


        public CambiaValutePage DownloadCambiaValutePage()
        {
            return GetPage<CambiaValutePage>(CambiaValuteUrl);
        }

        public XeChangePage DownloadXeChangePage()
        {
            return GetPage<XeChangePage>(XeChangeUrl);
        }

        private static T GetPage<T>(string url)
            where T : IValutePage, new()
        {
            using (var httpClient = new HttpClient())
            {
                //httpClient.BaseAddress = new Uri(CambiaValuteUrl);

                var requestResult = httpClient.GetAsync(new Uri(url)).Result.Content.ReadAsStringAsync().Result;
                var result = new T();
                result.SetPage(requestResult);
                return result;
            }
        }
    }
}