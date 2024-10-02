using HtmlAgilityPack;

namespace ExchangeRateTalk
{
    class CurrencyRate
    {
        readonly string baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "null";

        private static double GetCurrencyRate(string url) // html 크롤링 함수 
        {
            try
            {
                // URL에서 HTML 문서 가져오기
                HtmlWeb web = new();

                HtmlDocument doc = web.Load(url);

                HtmlNode currentRateNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"content\"]/div[2]/div[2]/div[2]/strong/text()");
                float currentRate = float.Parse(currentRateNode.InnerText);
                
                return currentRate;
            }
            catch
            {
                Thread.Sleep(1000 * 5);
                return GetCurrencyRate(url);
            }
        }

        public double USD() // 현재 USD 환율 
        {
            string USDUrl = $"{baseUrl}USDKRW";
            return Math.Round(GetCurrencyRate(USDUrl), 2);
        }

        public double JPY() // 현재 JPY 환율 
        {
            string JPYurl = $"{baseUrl}JPYKRW";
            return Math.Round(GetCurrencyRate(JPYurl), 2);
        }
    }
}