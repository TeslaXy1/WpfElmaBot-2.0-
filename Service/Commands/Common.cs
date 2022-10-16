using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WpfElmaBot.Models;
using WpfElmaBot_2._0_.Service.Commands;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot.Service.Commands
{
    public class Common
    {
        private CommandRoute route;
        public static string IsPass;
        
        private ELMA elma = new ELMA();
        public OptionTelegramMessage message = new OptionTelegramMessage();

        public Common(CommandRoute route)
        {
            
            this.route = route;
            
        }

        
        public async Task Start(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                List<string> ids = new List<string>() { "🔑Авторизация"};
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2,ids ,"");
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вам нужно авторизоваться", cancellationToken,message);
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге /start | " + ex);
            }
        }

        public async Task Menu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                List<string> ids = new List<string>() { "✉️Кол-во непрочитанных сообщений" };
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Вы вышли в меню",  cancellationToken, message);
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге /start | " + ex);
            }
        }

        public async Task Auth(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                    List<string> ids = new List<string>() { "🔑Авторизация" };
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите логин", cancellationToken, message);        
                    botClient.RegisterNextStep(update.Message.Chat.Id, Login);
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге /логина | " + ex);
            }
        }

        public async Task Login(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Введите пароль", cancellationToken);
                botClient.GetCacheData(update.GetChatId()).Value.Login =  update.Message.Text;
                botClient.RegisterNextStep(update.Message.Chat.Id, LoginPasswordHandler);
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге пароля | " + ex);
            }
        }
      
        public async Task LoginPasswordHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                string pass = "";             
                botClient.ClearStepUser(update.Message.Chat.Id);
                botClient.GetCacheData(update.GetChatId()).Value.Password = update.Message.Text;
                KeyValuePair<long,UserCache> loginpas= BotExtension.GetCacheData(botClient, update.Message.Chat.Id);
                if(IsPass=="true")
                {
                     pass = $@"""{loginpas.Value.Login}""";

                }
                else
                {
                    pass = loginpas.Value.Login;
                }
                string path = $"Authorization/LoginWith?username={pass}";
                var authorization =  await elma.PostRequest<Auth>(path, loginpas.Value.Password);
                elma.AuthorizationUser(authorization, Convert.ToInt64(update.Message.Chat.Id),loginpas.Value.Login);
                

                botClient.GetCacheData(update.GetChatId()).Value.AuthToken = authorization.AuthToken;
                botClient.GetCacheData(update.GetChatId()).Value.SessionToken = authorization.SessionToken;
                botClient.GetCacheData(update.GetChatId()).Value.StatusAuth = true;
                botClient.RegisterNextStep(update.Message.Chat.Id, Menu);

                List<string> ids = new List<string>() { "Меню" };
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы успешно авторизованы", cancellationToken, message);
            }
            catch (Exception ex)
            {
                if(ex.Message== "Request failed with status code BadRequest")
                {
                    OptionTelegramMessage message = new OptionTelegramMessage();
                    List<string> ids = new List<string>() { "🔑Авторизация" };
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Неверный логин или пароль", cancellationToken, message);
                }
                if(ex.Message ==  "Request failed with status code InternalServerError")
                {
                    OptionTelegramMessage message = new OptionTelegramMessage();
                    List<string> ids = new List<string>() { "🔑Авторизация" };
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Неверный логин или пароль", cancellationToken, message);
                }
                else
                {
                    MainWindowViewModel.Log.Error("Ошибка на шаге авторизации | " + ex);

                }

            }
           
        }
        public async Task CountUnread(ITelegramBotClient botClient,Update update,CancellationToken cancellationToken)
        {
            botClient.ClearStepUser(update.Message.Chat.Id);
            KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);
            if(info.Value.StatusAuth!=false)
            {
                var Count = await ELMA.getInstance().GetCountunread<int>(info.Value.AuthToken, info.Value.SessionToken);
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Непрочитанных сообщений: {Count}", cancellationToken);
            }
            else
            {
                botClient.ClearStepUser(update.Message.Chat.Id);
                await Start(botClient, update, cancellationToken);
               
            }
            

        }
       
        

    }
}
