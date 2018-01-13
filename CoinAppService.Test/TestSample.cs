using System;
using FluentAssertions;
using NUnit.Framework;
using Xunit;


namespace CoinAppService.Test
{
    public class TestsSample
    {
        private PageDownloader _sut;

        public TestsSample(PageDownloader sut)
        {
            _sut = sut;
        }

        [Fact]
        public void should_download_cambiavalute_page()
        {
            var page = _sut.DownloadPage();
            var result = page.GetChangeValue();

            result.Should().BeGreaterThan(0);
        }
    }
}