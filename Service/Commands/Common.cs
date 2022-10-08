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

        
        public async Task GetMyId(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {



                message.MenuReplyKeyboardMarkup =
                     new string[][]
                     {
                        new string[] {"Авторизация"}
                     };

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Ваш id {update.Message.Chat.Id}", cancellationToken, message);
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
                message.ClearMenu = true;
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
                message.ClearMenu = true;
              
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

            
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы успешно авторизованы", cancellationToken, message);
            }
            catch (Exception ex)
            {
                if(ex.Message== "Request failed with status code BadRequest")
                {
                    OptionTelegramMessage auth = new OptionTelegramMessage();
                    auth.MenuReplyKeyboardMarkup =
                         new string[][]
                         {
                        new string[] {"Авторизация"}
                         };
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Неверный логин или пароль", cancellationToken, auth);
                }
                if(ex.Message ==  "Request failed with status code InternalServerError")
                {
                    OptionTelegramMessage auth = new OptionTelegramMessage();
                    auth.MenuReplyKeyboardMarkup =
                         new string[][]
                         {
                        new string[] {"Авторизация"}
                         };
                    await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Неверный логин или пароль", cancellationToken, auth);
                }
                else
                {
                    MainWindowViewModel.Log.Error("Ошибка на шаге авторизации | " + ex);

                }

            }
           
        }
        public async Task Menu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы вышли в меню", cancellationToken);

            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге меню | " + ex);

            }
        }
        

    }
}
