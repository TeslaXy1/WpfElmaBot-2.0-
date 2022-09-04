using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot.Service
{
    public class TelegramCore
    {
        public static ITelegramBotClient bot = new TelegramBotClient("5440355360:AAEHIY2L0IaRF-VWPSkyhMvNrOqSjsEwm1s");      
        private static CancellationTokenSource _cancelTokenSource;
        public void Start()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );          
            _cancelTokenSource = new CancellationTokenSource();
        }
               
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
           new CommandRoute().ExecuteCommand(update.Message.Text, botClient, update, cancellationToken);
           
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            //await botClient.SendTextMessageAsync(chatId: 804483111, text: Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            //SettingApp.Log.Error(JsonConvert.SerializeObject(exception) + " Функция Бота");

        }
    }
}
