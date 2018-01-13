using System;
using System.Globalization;

namespace CoinAppService
{
    public class CambiaValutePage
    {
        private readonly string _requestResult;

        public CambiaValutePage(string requestResult)
        {
            _requestResult = requestResult;
        }

        public decimal GetChangeValue()
        {
            var findPattern = "cambio CHF/EUR ";
            var substring = _requestResult.Substring(_requestResult.IndexOf(findPattern, StringComparison.Ordinal) + findPattern.Length + 1);
            substring = substring.Substring(0, substring.IndexOf(" ", StringComparison.Ordinal) + 1);

            return decimal.Parse(substring, CultureInfo.InvariantCulture);
        }
    }
}