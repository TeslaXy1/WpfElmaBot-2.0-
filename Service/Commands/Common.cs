using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WpfElmaBot.Models;
using WpfElmaBot_2._0_.Models.EntityPack;
using WpfElmaBot_2._0_.Service;
using WpfElmaBot_2._0_.Service.Commands;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot.Service.Commands
{
    public class Common
    {
        public static string IsPass;
        private CommandRoute route;
        public static Common instance;
        private ELMA elma = new ELMA();
        public OptionTelegramMessage message = new OptionTelegramMessage();

        

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
                List<string> ids = new List<string>() { CommandRoute.AUTHMENU};
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2,ids ,"");
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вам нужно авторизоваться", cancellationToken,message);
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге /start | " + ex);
            }
        }

        public async Task Menu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) //шаг меню
        {
            try
            {
                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
                List<string> ids = new List<string>() { CommandRoute.Status}; //"✉️Кол-во непрочитанных сообщений" ,
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, "Вы вышли в меню",  cancellationToken, message);
            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка на шаге menu | " + ex);
            }
        }

        public async Task Auth(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)//запрос на авторизацию
        {
            try
            {
                    string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                    TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
                    List<string> ids = new List<string>() { CommandRoute.AUTHMENU };
                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
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
                botClient.GetCacheData(update.GetChatId()).Value.Login =  update.Message.Text;
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
                KeyValuePair<long,UserCache> loginpas= BotExtension.GetCacheData(botClient, update.Message.Chat.Id);
                if(IsPass=="false") //условие на предачу пароля в кавычках или без
                {
                     pass = $@"""{loginpas.Value.Password}""";

                }
                else
                {
                    pass = loginpas.Value.Password;
                }
                string path = $"Authorization/LoginWith?username={loginpas.Value.Login}";
                var authorization =  await elma.PostRequest<Auth>(path, pass);
                elma.AuthorizationUser(authorization, Convert.ToInt64(update.Message.Chat.Id),loginpas.Value.Login);

               
                botClient.GetCacheData(update.GetChatId()).Value.AuthToken = authorization.AuthToken;
                botClient.GetCacheData(update.GetChatId()).Value.SessionToken = authorization.SessionToken;
                botClient.GetCacheData(update.GetChatId()).Value.StatusAuth = true;
                botClient.RegisterNextStep(update.Message.Chat.Id, Menu);

                List<string> ids = new List<string>() { CommandRoute.MENU };
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");

                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы успешно авторизованы", cancellationToken, message);
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Request failed"))
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
        public async Task CountUnread(ITelegramBotClient botClient,Update update,CancellationToken cancellationToken)//Получить количество непрочитанных сообщений
        {
            string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
            TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);
            botClient.ClearStepUser(update.Message.Chat.Id);
            KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(botClient, update.Message.Chat.Id);
            if(info.Value.StatusAuth!=false)
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
       
        public async Task Status(ITelegramBotClient botClient,Update update ,CancellationToken cancellationToken)//шаг статус
        {
            try
            {
                string msg = $"Получено '{update.Message.Text}' от чата {update.GetChatId()} ( " + update.Message.Chat.FirstName + "  " + update.Message.Chat.LastName + ")";
                TelegramCore.getTelegramCore().InvokeCommonLog(msg, TelegramCore.TelegramEvents.Password);

                botClient.ClearStepUser(update.Message.Chat.Id);
                var updateToken = await elma.UpdateToken<Auth>(botClient.GetCacheData(update.GetChatId()).Value.AuthToken);


                botClient.GetCacheData(update.GetChatId()).Value.AuthToken = updateToken.AuthToken;
                botClient.GetCacheData(update.GetChatId()).Value.SessionToken = updateToken.SessionToken;
                botClient.GetCacheData(update.GetChatId()).Value.StatusAuth = true;

                var entity = await ELMA.getElma().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}&IdUserElma={updateToken.CurrentUserId}", updateToken.AuthToken, updateToken.SessionToken);
                if(entity.Count > 0)
                {
                    UpdateToken(updateToken.CurrentUserId, update.Message.Chat.Id, updateToken.AuthToken, updateToken.SessionToken, entity[0].Login, Convert.ToInt32(entity[0].Id));
                }


                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вы авторизованы", cancellationToken);

            }
            catch (Exception ex)
            {
                List<string> ids = new List<string>() { CommandRoute.AUTHMENU };
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                await route.MessageCommand.Send(botClient, update.Message.Chat.Id, $"Вам нужно авторизоваться", cancellationToken, message);
                //MessageBox.Show(ex+"");
            }


        }
        public static async Task UpdateToken(string userelma, long idtelegram, string authtoken, string sessiontoken, string login,  int identity) //функция обновления статуса авторизации пользователя в справочнике
        {
            try
            {
                var body = new EntityMargin()
                {
                    IdUserElma = userelma,
                    IdTelegram = Convert.ToString(idtelegram),
                    AuthToken = authtoken,
                    SessionToken = sessiontoken,
                    AuthorizationUser = "true",
                    Login = login,
                    //IdLastSms           = Convert.ToString(idmessage),
                    TimeMessage = DateTime.Now
                };
                string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                var entity = await ELMA.getElma().PostRequest<Entity>($"Entity/Update/{ELMA.TypeUid}/{identity}", jsonBody, authtoken, sessiontoken);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("line 1, position 4."))
                {
                    MainWindowViewModel.Log.Error("Успешное обновление статуса в справочнике | " + ex);
                }

                MainWindowViewModel.Log.Error("Ошибка обновления статуса в справочнике | " + ex);
                //TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка обновления статуса {userElma}", TelegramCore.TelegramEvents.Password);

            }

        }


    }
}
