using System;
using BusinessLogic;
using CoinAppService;

namespace ConsoleApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var bl = new Logic();

            Console.WriteLine(bl.GetCambiaValuteValue());
            Console.WriteLine(bl.GetGoogleChangeValue());
        }
    }
}
