using System;
using System.Globalization;

namespace BusinessLogic
{
    public interface IValutePage
    {
        decimal GetChangeValue();
        void SetPage(string pageBody);
    }

    public class CambiaValutePage : IValutePage
    {
        private string _pageBody;

        public decimal GetChangeValue()
        {
            const string findPattern = "cambio CHF/EUR ";
            var substring = _pageBody.Substring(_pageBody.IndexOf(findPattern, StringComparison.Ordinal) + findPattern.Length + 1);
            substring = substring.Substring(0, substring.IndexOf(" ", StringComparison.Ordinal) + 1);

            return decimal.Parse(substring, CultureInfo.InvariantCulture);
        }

        public void SetPage(string pageBody)
        {
            _pageBody = pageBody;
        }
    }

    public class XeChangePage : IValutePage
    {
        private string _pageBody;

        public decimal GetChangeValue()
        {
            const string findPattern = "<span class=\'uccResultAmount\'>";
            var substring = _pageBody.Substring(_pageBody.IndexOf(findPattern, StringComparison.Ordinal) + findPattern.Length);
            substring = substring.Substring(0, substring.IndexOf("</span>", StringComparison.Ordinal));

            return decimal.Parse(substring, CultureInfo.CurrentCulture);
        }

        public void SetPage(string pageBody)
        {
            _pageBody = pageBody;
        }
    }
}