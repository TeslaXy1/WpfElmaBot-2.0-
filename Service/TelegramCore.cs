﻿using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using WpfElmaBot.Service.Commands;
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
        public static TelegramCore getTelegramCore(MainWindowViewModel vm = null)
        {

            if (instance == null)
                instance = new TelegramCore(vm);
            return instance;
        }

        public enum TelegramEvents
        {
            Auth,
            Login,
            Password,
            Status
        }

        public delegate void CommonLog(string message,TelegramEvents events);

        public event CommonLog OnCommonLog;
        public event CommonLog OnCommonError;
        public event CommonLog OnCommonStatus;

        private MainWindowViewModel vm;
        private TelegramCore(MainWindowViewModel vm = null)
        {
            this.vm = vm;
        }
        public void InvokeCommonStatus(string msg, TelegramEvents events)
        {
            OnCommonStatus?.Invoke(msg, events);
        }

        public void InvokeCommonLog(string msg, TelegramEvents events)
        {
            OnCommonLog?.Invoke(msg, events);
        }
        public void InvokeCommonError(string msg, TelegramEvents events)
        {
            OnCommonError?.Invoke(msg, events);
        }


        //public static CancellationTokenSource _cancelTokenSource;
        public async void Start()
        {
            try
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
                getTelegramCore().InvokeCommonStatus($"Бот запущен", TelegramCore.TelegramEvents.Status);
                //vm.Status = $"{DateTime.Now.ToString("g")}-Бот запущен";
                //_cancelTokenSource = new CancellationTokenSource();
            }
            catch(Telegram.Bot.Exceptions.RequestException ex)
            {
                //vm.AttachedPropertyAppendError = "Нет подключения к интернету";
                getTelegramCore().InvokeCommonError("Нет подключения к интернету", TelegramCore.TelegramEvents.Status);
                MessageBox.Show("Убедитесь, что есть подключение к интернету");
            }
        }
               
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {

                    new CommandRoute().ExecuteCommand(update.Message.Text, botClient, update, cancellationToken);           
                  
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

            getTelegramCore().InvokeCommonStatus("Бот остановлен", TelegramCore.TelegramEvents.Status);
            
            if (ErrorMessage.Contains("An error occurred while sending the request"))
            {
                MessageBox.Show("Проверьте подключение к интернету");
                getTelegramCore().InvokeCommonError("Бот остановлен - проверьте подключение к интернету", TelegramCore.TelegramEvents.Status);

            }
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
