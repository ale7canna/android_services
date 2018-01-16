using System;

namespace BusinessLogic
{
    public class Logic
    {
        private readonly PageDownloader _pageDownloader;

        public Logic()
        {
            _pageDownloader = new PageDownloader();
        }

        public decimal GetXeChangeValue()
        {
            return GetPageValue(_pageDownloader.DownloadXeChangePage);
        }

        public decimal GetCambiaValuteValue()
        {
            return GetPageValue(_pageDownloader.DownloadCambiaValutePage);
        }

        private static decimal GetPageValue(Func<IValutePage> getPageMethod)
        {
            IValutePage page = getPageMethod();
            return page.GetChangeValue();
        }
    }
}