using ExchangeRateTalk;
using Telegram.Bot;
using Telegram.Bot.Types;
using DotNetEnv;

internal class Program
{
    static readonly CurrencyRate cur = new();
    public static void Main(string[] args)
    {
        Env.Load();

        string testId = Environment.GetEnvironmentVariable("TEST_ID") ?? "DEFAULT_VALUE";
        string id = Environment.GetEnvironmentVariable("ID") ?? "DEFAULT_VALUE";
        string token = Environment.GetEnvironmentVariable("TOKEN") ?? "DEFAULT_VALUE";

        double ef = 3.0; // 단위가
        double diffRateJPY = 0; // 달러 기준가
        double diffRateUSD = 0; // 엔화 기준가

        long chatID = long.Parse(id);
        long adminId = long.Parse(testId);

        // 목표 시간 배열
        DateTime[] timeArray =
        [
            DateTime.Today.Add(new(9, 0, 0)), // 환율시장 시작
            DateTime.Today.Add(new(15, 30, 0)), // 환율시장 마감
            DateTime.Today.Add(new(18, 0, 0)), // 시간 외 거래시간 마감
        ];

        // 알림 시간 체크 후 지난 시간 다음 날로 변경 
        for (int i = 0; i < timeArray.Length; i++)
        {
            if (DateTime.Now > timeArray[i])
                timeArray[i] = timeArray[i].AddDays(1);
        }
        SendMessage($"서비스 시작 시간 : {DateTime.Now}", adminId);

        // 현재 시각이 알림 설정 시간 범위 내에 있는지 체크
        bool IsInTimeRange(DateTime time, DateTime[] targetTime)
        {
            for (int i = 0; i < targetTime.Length; i++)
            {
                TimeSpan range = TimeSpan.FromSeconds(30);
                DateTime over = targetTime[i] + range;
                DateTime under = targetTime[i] - range;

                if (time <= over && time >= under)
                {
                    // 일치 시 배열 내 시간 값 변경 
                    targetTime[i] = targetTime[i].AddDays(1);
                    return true;
                }
            }
            return false;
        }

        while (true)
        {
            DateTime now = DateTime.Now;
            double usdC = cur.USD();
            double jpyC = cur.JPY();
            string JPYMessage = $"100¥ : {jpyC}원\n{MsgType(jpyC, diffRateJPY)}";
            string USDMessage = $"1$: {usdC}원\n{MsgType(usdC, diffRateUSD)}";

            if (IsOverDiff(jpyC, diffRateJPY))
            {
                SendMessage(JPYMessage, chatID);
                // 기준가 이상 차이 날 때 값 == 기준가
                diffRateJPY = jpyC;
            }

            if (IsOverDiff(usdC, diffRateUSD))
            {
                SendMessage(USDMessage, chatID);
                diffRateUSD = usdC;
            }

            if (IsInTimeRange(now, timeArray))
            {
                SendMessage($"{now}\n\n100¥ : {jpyC}\n1$ : {usdC}\n\n제공 : 하나은행", chatID);
            }

            Thread.Sleep(1000 * 60); // 1회 실행 후 대기 (1000 -> 1초)
        }

        string MsgType(double basePrice, double diff)
        {
            if (basePrice > diff)
                return $"직전 알림 가격 차 : {Math.Round(basePrice - diff, 2)} ▲";
            return $"직전 알림 가격 차 : {Math.Round(diff - basePrice, 2)} ▼";

        }

        // 텔레그램 봇 전송
        void SendMessage(string message, long id)
        {
            var bot = new TelegramBotClient(token);

            try
            {
                bot.SendTextMessageAsync(new ChatId(id), message);
            }
            catch (Exception)
            {
                bot.SendTextMessageAsync(new ChatId(adminId), "Error");
            }
        }

        // 단위 가격차 만족 여부
        bool IsOverDiff(double currentRate, double parsedRate)
        {
            if (currentRate >= parsedRate + ef || currentRate <= parsedRate - ef)
                return true;
            return false;
        }
    }
}