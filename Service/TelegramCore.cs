using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using WpfElmaBot_2._0_;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot.Service
{
    public class TelegramCore
    {
        public static string TelegramToken;
        public  ITelegramBotClient bot = new TelegramBotClient(TelegramToken);
        public static CancellationToken cancellation;


        private static TelegramCore instance;
        public static TelegramCore getInstance()
        {

            if (instance == null)
                instance = new TelegramCore();
            return instance;
        }

        private MainWindowViewModel vm;
        public TelegramCore(MainWindowViewModel vm = null)
        {
            this.vm = vm;
        }



        //public static CancellationTokenSource _cancelTokenSource;
        public async void Start()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            cancellation = cts.Token;
            await ClearUpdates();

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
            vm.Status = $"{DateTime.Now.ToString("g")}-Бот запущен";
            //_cancelTokenSource = new CancellationTokenSource();
        }
               
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {

                    new CommandRoute().ExecuteCommand(update.Message.Text, botClient, update, cancellationToken);
                    vm.AttachedPropertyAppend = $"{DateTime.UtcNow.ToString("G")}: Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ") \n" + Environment.NewLine;
           
                  
                }
                if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    var query = update.CallbackQuery.Data;
                    new CommandRoute().ExecuteCommand(query, botClient, update, cancellationToken);
                }

            }
            catch (Exception ex) 
            {
                MainWindowViewModel.Log.Error("Ошибка обновления телеграма | " + ex);
            }

        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            MainWindowViewModel.Log.Error("Ошибка телеграм | " + ErrorMessage);


        }
        public async Task ClearUpdates()
        {
            var update = await bot.GetUpdatesAsync();
            foreach (var item in update)
            {
                var offset = item.Id + 1;
                await bot.GetUpdatesAsync(offset);
            }
        }
    }
}
