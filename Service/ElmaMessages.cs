using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Telegram.Bot.Types.ReplyMarkups;
using WpfElmaBot.Models;
using WpfElmaBot.Service;
using WpfElmaBot.Service.Commands;
using WpfElmaBot_2._0_.Models;
using WpfElmaBot_2._0_.Models.EntityPack;
using WpfElmaBot_2._0_.Service.Commands;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot_2._0_.Service
{
    internal class ElmaMessages
    {
        private static string authSprav;
        private static string sessionSprav;
        private static string priority = "";
        private static bool firstLaunch = true;

        private static CommandRoute route = new CommandRoute();
        private static CancellationTokenSource _cancelTokenSource;
        private const int timerIntervalInSeconds = 10;

        public ElmaMessages()
        {

        }


        public  void Start()
        {
            Stop();
            _cancelTokenSource = new CancellationTokenSource();
            _ = MainCycle(_cancelTokenSource.Token);

        }
        public bool IsWorking => _cancelTokenSource != null;

        async  Task MainCycle(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var nextTime = Task.Delay(TimeSpan.FromSeconds(timerIntervalInSeconds));
                try
                {
                    await Task.WhenAll(nextTime, ProcessingMessages());
                }
                catch (Exception exception)
                {
                    MainWindowViewModel.Log.Error("Ошибка цикла получения смс | " + exception);


                }

            }
        } //цикл для получения смс
        public static void Stop()
        {
            _cancelTokenSource?.Cancel();
            _cancelTokenSource = null;
            TelegramCore.getTelegramCore().InvokeCommonStatus($"Бот остановлен", TelegramCore.TelegramEvents.Status);
        }
        public  async Task ProcessingMessages() //функция обработки сообщений
        {
            try
            {
                Random random = new Random();
                bool Auth;              
                int wait = 10 + random.Next(10);
                if(firstLaunch==true)
                {
                    firstLaunch = false;
                }
                else { await Task.Delay(TimeSpan.FromSeconds(wait)); }              
                try
                {

                        var authEntity = await AuthEntity(ELMA.login, Common.IsPass=="false"? $@"""{ELMA.password}""" : ELMA.password);
                        authSprav = authEntity.AuthToken;
                        sessionSprav = authEntity.SessionToken;
                        Auth = true;
                    
                }
                catch(Exception exeption)
                {
                    Stop();
                    
                    if(exeption.Message.Contains("запрещенным правами"))
                    {
                        TelegramCore.getTelegramCore().InvokeCommonError(exeption.Message, TelegramCore.TelegramEvents.Password);
                    }
                    else
                    {
                        if(exeption.Message.Contains("Unexpected character"))
                        {
                            MessageBox.Show("Убедитесь, что сервер запущен");
                            TelegramCore.getTelegramCore().InvokeCommonError("Убедитесь, что сервер запущен", TelegramCore.TelegramEvents.Password);
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль");
                            TelegramCore.getTelegramCore().InvokeCommonError("Неверный логин или пароль", TelegramCore.TelegramEvents.Password);
                        }
                        
                    }
                    MainWindowViewModel.Log.Error("Ошибка авторизации спарвочника | "+exeption);                 
                    Auth = false;
                    

                }
                if (Auth == true)
                {
                    var entity = await ELMA.getElma().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}", authSprav, sessionSprav); //получение записей из справочника
                    for (int i = 0; i < entity.Count; i++)
                    {

                  
                        try
                        {
                            // var chekToken = await ELMA.getElma().UpdateToken<Auth>(entity[i].AuthToken); //обновления токена пользователя
                            var chekToken = await ELMA.getElma().UpdateTokenAndEntity<Auth>(Convert.ToInt64(entity[i].IdTelegram), entity[i].Login, entity[i].AuthToken);
                            try
                            {
                                 
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.AuthToken     = chekToken.AuthToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.SessionToken  = chekToken.SessionToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth    = true;


                                var allMessages = await ELMA.getElma().GetAllMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);

                                //var unreadMessages = await ELMA.getElma().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);
                                
                               
                                await GenerateDictionary(entity[i], allMessages);
                                await GenerateMsg(entity[i], allMessages,  chekToken.AuthToken, chekToken.SessionToken, entity[i].TimeMessage);
                                await GenerateComment(allMessages, entity[i].Login, Convert.ToInt64(entity[i].IdTelegram));
                            }
                           
                            catch (Exception exception)
                            {

                                MainWindowViewModel.Log.Error($"Неудалось получить сообщения пользователя {entity[i].Login} | " + exception);
                                MessageBox.Show("" + exception);
                                
                            }


                        }
                        catch (Exception exception)
                        {
                            
                                KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, Convert.ToInt64(entity[i].IdTelegram));

                                MainWindowViewModel.Log.Error("Ошибка обновления токена | " + exception);
                                
                                if (entity[i].AuthorizationUser == "true" || info.Value.StatusAuth == true)
                                {
                                    OptionTelegramMessage message = new OptionTelegramMessage();
                                    List<string> ids = new List<string>() { CommandRoute.AUTHMENU };
                                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");

                                    TelegramCore.getTelegramCore().bot.ClearStepUser(Convert.ToInt64(entity[i].IdTelegram));
                                    await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity[i].IdTelegram), msg: "Вам нужно авторизоваться", TelegramCore.cancellation, message);

                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth = false;

                                    await UpdateStatus(entity[i]);
                                }
                           

                        }
                    }
                }
                
                
            }
            catch(Exception exception)
            {

                if(exception.StackTrace.Contains("ElmaMessages.cs:строка 106") || exception.StackTrace.Contains("ElmaMessages.cs:line 106"))
                {
                    TelegramCore.getTelegramCore().InvokeCommonError("Неверный TypeUid справочника", TelegramCore.TelegramEvents.Password);
                    
                    Stop();
                }
                
                MainWindowViewModel.Log.Error("Ошибка обработки сообщений | " + exception);
            }

        }
        
        public static async Task UpdateStatus(EntityMargin entity) //функция обновления статуса авторизации пользователя в справочнике
        {
            try
            {
                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.StatusAuth    = false;
                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.AuthToken     = null;
                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.SessionToken  = null;

                entity.AuthorizationUser = "false";
              
                string jsonBody     = System.Text.Json.JsonSerializer.Serialize(entity);
                var updateEntity    = await ELMA.getElma().PostRequestNotDeserialze($"Entity/Update/{ELMA.TypeUid}/{entity.Id}", jsonBody, authSprav, sessionSprav);
            }
            catch(Exception ex)
            {
                if(ex.Message.Contains("line 1, position 4."))
                {
                    MainWindowViewModel.Log.Error("Успешное обновление статуса в справочнике | " + ex);
                }
                else
                {
                    MainWindowViewModel.Log.Error("Ошибка обновления статуса в справочнике | " + ex);
                    TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка обновления статуса", TelegramCore.TelegramEvents.Password);
                }

                
                

            }

        }

        public static async Task UpdateMessage(EntityMargin entity,DateTime time) //функция обновления последнего сообщения в справочнике
        {

            try
            {

                entity.TimeMessage = time;

                string jsonBody               = System.Text.Json.JsonSerializer.Serialize(entity);
                var updateEntity              = await ELMA.getElma().PostRequestNotDeserialze($"Entity/Update/{ELMA.TypeUid}/{entity.Id}", jsonBody, authSprav, sessionSprav);

            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("Error converting value"))
                {
                    MainWindowViewModel.Log.Error("Успешное обновление записи| " + ex);
                }
                else
                {
                    MainWindowViewModel.Log.Error("Ошибка обновения последнего сообщения в справочнике | " + ex);
                }
               
                
                //TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка обновления последнего смс {userElma}", TelegramCore.TelegramEvents.Password);


            }
            

        }
        public async Task GenerateDictionary(EntityMargin entity, MessegesOtvet message)
        {
            if (message != null)
            {
                
                for (int IdMes = message.Data.Count - 1; IdMes > -1; IdMes--)
                {
                    KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, Convert.ToInt64(entity.IdTelegram));
                    if (message.Data[IdMes].LastComments.Count != 0)
                    {
                        int max = message.Data[IdMes].LastComments.Data.Select(x => x.Id).Max();
                        try
                        {

                            if (info.Value.LastCommentId != null)
                            {
                                if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                                {

                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(message.Data[IdMes].Id, max);

                                }
                            }
                            else
                            {
                                if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                                {

                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(message.Data[IdMes].Id, 0);

                                }

                            }
                        }
                        catch (Exception ex) { MainWindowViewModel.Log.Error($"Ошибка добавления последнего комментария для {entity.Login}| " + ex); }
                    }
                    else
                    {
                        if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                        {

                            TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(message.Data[IdMes].Id, 0);

                        }
                    }
                }
            }
        }
        public async Task GenerateMsg(EntityMargin entity,MessegesOtvet messages,string authtoken,string sessiontoken, DateTime dateMes) //функцию генерации сообщения
        {
            try
            {
                if (messages != null)
                {
                    for (int IdMes = messages.Data.Count - 1; IdMes > -1; IdMes--)
                    {                    
                        if (messages.Data[IdMes].CreationDate > dateMes && messages.Data[IdMes].IsRead == false)
                        {

                            bool isTask     = messages.Data[IdMes].ObjectGroupClass.ToLower().Contains("task");
                            bool hasText    = string.IsNullOrWhiteSpace(messages.Data[IdMes].Text);
                            bool isEvent    = messages.Data[IdMes].ObjectGroupText == "Событие в календаре";
                            bool isAsk      = messages.Data[IdMes].ObjectGroupText.ToLower().Contains("вопрос");
                            bool isPlanWork = false;

                            List<TaskBase> taskbase = new List<TaskBase>();
                            if (isTask)
                            {
                                var eqlQuery = $"Id={messages.Data[IdMes].ActionObjectId}";
                                taskbase = await ELMA.getElma().GetEntity<TaskBase>($"Entity/Query?type={ELMA.TypeUidTaskBase}&limit=1&q={eqlQuery}", authtoken, sessiontoken);
                                if (taskbase[0].Priority == "1") { priority = "🔴"; }
                                if (taskbase[0].Priority == "2") { priority = "🟡"; }
                                if (taskbase[0].Priority == "3") { priority = "🟢"; }
                                isPlanWork = taskbase[0].PlanWorkLog != null;

                            }

                            string msg = ( isAsk? $"Новый вопрос к теме:\n{messages.Data[IdMes].Subject}" : isEvent ? "Новое событие" : (isTask ? $"{priority}Новая задача" : "Новое сообщение📋"));
                            msg += "\n";
                            msg += "👨‍💻 " + messages.Data[IdMes].CreationAuthor.Name;
                            msg +=  isAsk ? "" : (isTask ? $"\n⏰ Сроки \n{taskbase[0].StartDate.Replace("/",".")}-\n{taskbase[0].EndDate.Replace("/", ".")}": "");
                            msg +=  isTask ? ( isPlanWork ? $"\n⏳ {taskbase[0].PlanWorkLog} минут" : "" ): "";
                            msg +=  isAsk ? "" :  "\n📃 " + messages.Data[IdMes].Subject;
                            msg += (hasText ? "" : "\n📝 " + messages.Data[IdMes].Text);

                            string isType = (isAsk ? $"Перейти к вопросу" : isEvent ? "Перейти к событию" : (isTask ? $"Перейти к задаче" : "Перейти к сообщению"));
                            
                            OptionTelegramMessage message = new OptionTelegramMessage();
                            var ikm = new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithUrl(isType, $"http://{MainWindowViewModel.Adress}:{MainWindowViewModel.Port}{messages.Data[IdMes].Url}")
                                    }
                                });
                            message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);

                            await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity.IdTelegram), msg: msg, TelegramCore.cancellation, messages.Data[IdMes].Url!= null ? message: null);
                            MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Сообщение {messages.Data[IdMes].Id} отправлено пользователю {entity.Login}");

                            await UpdateMessage(entity, messages.Data[IdMes].CreationDate);
                        }

                    }
                    
                }
            }
            catch(Exception ex)
            {
                TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка генерации сообщения для {entity.Login}", TelegramCore.TelegramEvents.Password);
                MainWindowViewModel.Log.Error($"Ошибка генерации  сообщения для {entity.Login}| " + ex) ;
            }
        }
        public async Task GenerateComment(MessegesOtvet messages,string user,long idTelegram)
        {
            KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, idTelegram);
            if (messages != null)
            {
                for (int IdMes = messages.Data.Count - 1; IdMes > -1; IdMes--)
                { 
                        for(int IdComment = 0; IdComment < messages.Data[IdMes].LastComments.Count;IdComment++)
                        {                      
                            if (messages.Data[IdMes].LastComments.Count != 0)
                            {
                                try
                                {
                                    if (info.Value.LastCommentId[messages.Data[IdMes].Id] != messages.Data[IdMes].LastComments.Data[IdComment].Id && info.Value.LastCommentId[messages.Data[IdMes].Id] < messages.Data[IdMes].LastComments.Data[IdComment].Id)
                                    {
                                        
                                        TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.LastCommentId[messages.Data[IdMes].Id] = messages.Data[IdMes].LastComments.Data[IdComment].Id;

                                        string msg = "Новый комментарий к теме:\n📃" + messages.Data[IdMes].Subject;
                                        msg += "\n👨‍💻 " + messages.Data[IdMes].CreationAuthor.Name;
                                        msg += messages.Data[IdMes].LastComments.Data[IdComment].ActionText !="" ? $"\n📍 {messages.Data[IdMes].LastComments.Data[IdComment].ActionText}" : "";
                                       
                                        msg += "\n";
                                        if(messages.Data[IdMes].LastComments.Data[IdComment].Text != "\r\n")
                                        {
                                            msg += "📝 " + messages.Data[IdMes].LastComments.Data[IdComment].Text;
                                        }

                                        bool isTask = messages.Data[IdMes].ObjectGroupClass.ToLower().Contains("task");
                                        bool isEvent = messages.Data[IdMes].ObjectGroupText == "Событие в календаре";
                                        bool isAsk = messages.Data[IdMes].ObjectGroupText.ToLower().Contains("вопрос");

                                        string isType = (isAsk ? $"Перейти к вопросу" : isEvent ? "Перейти к событию" : (isTask ? $"Перейти к задаче" : "Перейти к сообщению"));

                                        OptionTelegramMessage message = new OptionTelegramMessage();
                                        var ikm = new InlineKeyboardMarkup(new[]
                                        {
                                            new[]
                                            {
                                                InlineKeyboardButton.WithUrl(isType, $"http://{MainWindowViewModel.Adress}:{MainWindowViewModel.Port}{messages.Data[IdMes].Url}")
                                            }
                                        });
                                        message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);

                                        await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: idTelegram, msg: msg, TelegramCore.cancellation, messages.Data[IdMes].Url != null ? message : null);
                                        MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Комментарий {messages.Data[IdMes].Id} отправлен пользователю {user}");

                                    }
                                }catch(Exception ex)
                                {

                                    TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка генерации комментария для {user}", TelegramCore.TelegramEvents.Password);
                                    MainWindowViewModel.Log.Error($"Ошибка генерации  комментария для {user}| " + ex) ;
                                }
                            }
                            
                        }
                    
                }
            }
        }

        public async Task<Auth> AuthEntity(string login,string password) //функция авторизации справочника
        {
            var authEntity = await ELMA.getElma().PostRequest<Auth>($"Authorization/LoginWith?username={login}", password);
            return authEntity;
        }

    }
}
