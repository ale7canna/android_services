using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoinAppService;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var filename = Path.Combine(path, "change.csv");

            if (!File.Exists(filename))
            {
                IEnumerable<string> lines = new List<string>()
                {
                    "CAMBIO CHF/EUR",
                    "\"Giorno e ora\";\"cambio\""
                };
                File.AppendAllLines(filename, lines);
            }


            var _sut = new PageDownloader();

            var page = _sut.DownloadPage();
            var result = page.GetChangeValue();


            File.AppendAllLines(filename, new [] {$"\"{DateTime.Now.ToString("s", CultureInfo.InvariantCulture)}\";\"{result.ToString(CultureInfo.InvariantCulture)}\""});
        }
    }
}
