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

            var cambiaValuteValue = bl.GetCambiaValuteValue();
            var xeChangeValue = bl.GetXeChangeValue();
            Console.WriteLine(cambiaValuteValue);
            Console.WriteLine(xeChangeValue);
            var value = xeChangeValue - cambiaValuteValue;
            Console.WriteLine(value);
            Console.WriteLine((decimal)100 * value / xeChangeValue);

            Console.ReadLine();
        }
    }
}
