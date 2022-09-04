using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfElmaBot_2._0_.Service
{
    internal class ElmaMessages
    {
        public static string authSprav;
        public static string sessionSprav;
        public static string entityAnswer;
        private static CancellationTokenSource _cancelTokenSource;
        private const int TimerIntervalInSeconds = 10;

        public static void Start()
        {
            Stop();
            _cancelTokenSource = new CancellationTokenSource();
            _ = MainCycle(_cancelTokenSource.Token);         
        }
        public bool IsWorking => _cancelTokenSource != null;

        async static Task MainCycle(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var nextTime = Task.Delay(TimeSpan.FromSeconds(TimerIntervalInSeconds));
                try
                {
                    await Task.WhenAll(nextTime, ProcessingMessages());
                }
                catch (Exception exception)
                {
                    

                }

            }
        } //цикл для получения смс
        public static void Stop()
        {
            _cancelTokenSource?.Cancel();
            _cancelTokenSource = null;
        }
        public static async Task ProcessingMessages()
        {
            var wait = 25;
            await Task.Delay(TimeSpan.FromSeconds(wait));
        }
        
       //TODO авторизация справочника
       //TODO получение записей 
       //TODO прогон всех записей
    }
}
