using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using WpfElmaBot.Models;
using WpfElmaBot_2._0_.Models;
using WpfElmaBot_2._0_.Service.Commands;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot.Service.Commands
{
    public class Common
    {
        public static string IsPass;
        private CommandRoute route;
        public static Common instance;
        private readonly ELMA elma = ELMA.getElma();

        public Common(CommandRoute route)
        {

            this.route = route;

        }
        public static Common GetCommon()
        {
            if (instance == null)
                instance = new Common(new CommandRoute());
            return instance;
        }

        public async Task Start(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//шаг старт
        {
            try
            {
                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);

                botClient.ClearStepUser(update.Message.Chat.Id);

                var updateToken = await elma.UpdateTokenAndEntity<Auth>(Convert.ToInt64(update.Message.Chat.Id), botClient.GetCacheData(update.GetChatId()).Value.Login, botClient.GetCacheData(update.GetChatId()).Value.AuthToken);

                botClient.GetCacheData(update.GetChatId()).Value.AuthToken = updateToken.AuthToken;
                botClient.GetCacheData(update.GetChatId()).Value.SessionToken = updateToken.SessionToken;
                botClient.GetCacheData(update.GetChatId()).Value.StatusAuth = true;

                OptionTelegramMessage message = new();
                List<string> ids = new() { CommandRoute.Status, CommandRoute.MENU };
                message.ClearMenu = false;
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");


                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы авторизованы", cancellationToken, message);

            }
            catch (WebException exception)
            {
                if(exception.Message.Contains("Запуск сервера") || exception.Message.Contains("Подключение не установлено") || exception.Message.Contains("Fault xmlns"))
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Сервер недоступен. Пожалуйста подождите", cancellationToken);

                }
                else
                {
                    OptionTelegramMessage message = new();
                    List<string> ids = new() { CommandRoute.AUTHMENU };
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");

                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вам нужно авторизоваться", cancellationToken, message);
                }

                MainWindowViewModel.Log.Error("Ошибка на шаге /start | " + exception);

            }
        }

        public async Task Menu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //шаг меню
        {
            try
            {
                botClient.ClearStepUser(update.Message.Chat.Id);

                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);

                KeyValuePair<long, UserCache> loginpas = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);

                OptionTelegramMessage message = new();
                List<string> ids = new();
                if (loginpas.Value.StatusAuth == true)
                {
                    ids = new List<string> { CommandRoute.Status, CommandRoute.MENU };
                }
                else
                {
                    ids = new List<string> { CommandRoute.AUTHMENU, CommandRoute.MENU };
                }
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Вы вышли в меню", cancellationToken, message);
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Запуск сервера") || exception.Message.Contains("Подключение не установлено") || exception.Message.Contains("Fault xmlns"))
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Сервер недоступен. Пожалуйста подождите", cancellationToken);

                }
                else
                {

                }
                MainWindowViewModel.Log.Error("Ошибка на шаге menu | " + exception);
            }
        }

        public async Task Auth(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//запрос на авторизацию
        {
            try
            {
                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);

                OptionTelegramMessage message = new();

                List<string> ids = new() { CommandRoute.MENU };
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");


                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите логин", cancellationToken, message);
                botClient.RegisterNextStep(update.Message.Chat.Id, Login);

            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге ввода логина | " + ex);
            }
        }

        public async Task Login(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//ввод логина
        {
            try
            {
                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите пароль", cancellationToken);

                botClient.GetCacheData(update.GetChatId()).Value.Login = update.Message.Text;
                botClient.RegisterNextStep(update.Message.Chat.Id, LoginPasswordHandler);
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге ввода пароля | " + ex);
            }
        }

        public async Task LoginPasswordHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//ввод пароля и авторизация
        {
            try
            {

                string msg = $"Получено 'пароль' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);

                string pass = "";
                botClient.ClearStepUser(update.Message.Chat.Id);
                botClient.GetCacheData(update.GetChatId()).Value.Password = update.Message.Text;

                KeyValuePair<long, UserCache> loginpas = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);

                pass = IsPass == "false" ? $@"""{loginpas.Value.Password}""" : loginpas.Value.Password;

                string path = $"Authorization/LoginWith?username={loginpas.Value.Login}";
                var authorization = await elma.PostRequest<Auth>(path, pass);

                await elma.AuthorizationUser(authorization, Convert.ToInt64(update.Message.Chat.Id), loginpas.Value.Login);


                botClient.GetCacheData(update.GetChatId()).Value.AuthToken = authorization.AuthToken;
                botClient.GetCacheData(update.GetChatId()).Value.SessionToken = authorization.SessionToken;
                botClient.GetCacheData(update.GetChatId()).Value.StatusAuth = true;
                botClient.GetCacheData(update.GetChatId()).Value.CountAttempt = 0;
                //botClient.RegisterNextStep(update.Message.Chat.Id, Menu);

                OptionTelegramMessage message = new();
                List<string> ids = new() { CommandRoute.Status, CommandRoute.MENU };
                message.ClearMenu = false;
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");


                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы успешно авторизованы", cancellationToken, message);
            }
            catch (WebException exception)
            {
                if (exception.Message.Contains("Запуск сервера") || exception.Message.Contains("Подключение не установлено") || exception.Message.Contains("Fault xmlns"))
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Сервер недоступен. Пожалуйста подождите", cancellationToken);

                }
                else
                {
                    var ex = JsonConvert.DeserializeObject<ErrorResponse>(exception.Message);
                    if (ex.Message.Contains("Ошибка авторизации"))
                    {
                        OptionTelegramMessage message = new();
                        List<string> ids = new() { CommandRoute.AUTHMENU, CommandRoute.MENU };
                        message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Неверный логин или пароль", cancellationToken, message);
                    }
                    else
                    {
                        MainWindowViewModel.Log.Error("Ошибка на шаге авторизации | " + ex);

                    }
                }

            }

        }
        public async Task CountUnread(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//Получить количество непрочитанных сообщений
        {
            try
            {
                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
                botClient.ClearStepUser(update.Message.Chat.Id);
                KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);
                if (info.Value.StatusAuth != false)
                {
                    var Count = await ELMA.getElma().GetCountunread<int>(info.Value.AuthToken, info.Value.SessionToken);
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Непрочитанных сообщений: {Count}", cancellationToken);
                }
                else
                {
                    botClient.ClearStepUser(update.Message.Chat.Id);
                    await Start(botClient, update, cancellationToken);

                }
            }
            catch (WebException exception)
            {
                if (exception.Message.Contains("Запуск сервера") || exception.Message.Contains("Подключение не установлено") || exception.Message.Contains("Fault xmlns"))
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Сервер недоступен. Пожалуйста подождите", cancellationToken);

                }
                MainWindowViewModel.Log.Error("Ошибка на шаге количество непрочитанных смс | " + exception);
            }


        }

        public async Task Status(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//шаг статус
        {
            try
            {
                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);

                botClient.ClearStepUser(update.Message.Chat.Id);

                var updateToken = await elma.UpdateTokenAndEntity<Auth>(Convert.ToInt64(update.Message.Chat.Id), botClient.GetCacheData(update.GetChatId()).Value.Login, botClient.GetCacheData(update.GetChatId()).Value.AuthToken);

                botClient.GetCacheData(update.GetChatId()).Value.AuthToken = updateToken.AuthToken;
                botClient.GetCacheData(update.GetChatId()).Value.SessionToken = updateToken.SessionToken;
                botClient.GetCacheData(update.GetChatId()).Value.StatusAuth = true;

                OptionTelegramMessage message = new();
                List<string> ids = new() { CommandRoute.Status, CommandRoute.MENU };
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Авторизация не нужна", cancellationToken, message);

            }
            catch (WebException exception)
            {
                if (exception.Message.Contains("Запуск сервера") || exception.Message.Contains("Подключение не установлено") || exception.Message.Contains("Fault xmlns"))
                {
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Сервер недоступен. Пожалуйста подождите", cancellationToken);

                }
                else
                {
                    var ex = JsonConvert.DeserializeObject<ErrorResponse>(exception.Message);
                    if (ex.Message.Contains("Токен недействителен"))
                    {
                        OptionTelegramMessage message = new();
                        List<string> ids = new() { CommandRoute.AUTHMENU };
                        message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");

                        await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вам нужно авторизоваться", cancellationToken, message);

                        MainWindowViewModel.Log.Error("Ошибка на шаге статуса | " + ex.Message);
                    }
                }
            }
        }
    }
}
