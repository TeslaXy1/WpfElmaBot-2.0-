using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public class ElmaMessages
    {
        private static string authSprav;
        private static string sessionSprav;
        private static string priority = "";
        private static bool firstLaunch = true;
        private const int waitDelay = 20;
        private const int timerIntervalInSeconds = 10;

        private static readonly CommandRoute route = new CommandRoute();
        private static CancellationTokenSource _cancelTokenSource;
            
        public ElmaMessages()
        {
            
        }
        public  void Start()
        {
            Stop();
            _cancelTokenSource = new CancellationTokenSource();
            _ = MainCycle(_cancelTokenSource.Token);
            TelegramCore.getTelegramCore().InvokeCommonStatus($"Бот запущен", TelegramCore.TelegramEvents.Status);

        }
        public bool IsWorking => _cancelTokenSource != null;

        private async  Task MainCycle(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var nextTime = Task.Delay(TimeSpan.FromSeconds(timerIntervalInSeconds));
                try
                {
                    await Task.WhenAll(nextTime, ProcessingMessages1());
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
            TelegramCore.getTelegramCore().InvokeCommonStatus($"Обработка сообщений остановлена", TelegramCore.TelegramEvents.Status);
        }
        private  async Task ProcessingMessages() //функция обработки сообщений
        {
            //var allMessasges = await ELMA.getElma().GetAllMessage<MessegesOtvet>("4a78c532-5236-499b-93091334e15acef0", "454941A1F7A2E31451163C110D76195FC5E617DB6D6717D12BC55C95B11028081D32F0BB0D53D5C5D1288C0E16B8BF6BC187172C9BF7A84C23F06D07D44F34E3");//TODO удалить или переделать
            try
            {
                bool Auth=true;              
                
                if(firstLaunch!=true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(waitDelay)); 
                }              
                try
                {
                    var authEntity = await AuthEntity(ELMA.login, Common.IsPass=="false"? $@"""{ELMA.password}""" : ELMA.password);
                    authSprav = authEntity.AuthToken;
                    sessionSprav = authEntity.SessionToken;
                    Auth = true;                    
                }     
                catch(WebException exception)
                {
                    Stop();
                    if (exception.Message.Contains("Запуск сервера"))
                    {
                        TelegramCore.getTelegramCore().InvokeCommonError("Убедитесь, что сервер запущен", TelegramCore.TelegramEvents.Password);
                        MainWindowViewModel.Log.Error($"Ошибка авторизации спарвочника | {exception}");
                        ReInitializationELMA();
                    }
                    else
                    {
                        var ex = JsonConvert.DeserializeObject<ErrorResponse>(exception.Message);
                        if (ex.Message.Contains("Ошибка авторизации"))
                        {
                            TelegramCore.getTelegramCore().InvokeCommonError("Неверный логин или пароль", TelegramCore.TelegramEvents.Password);
                        }
                        if (ex.Message.Contains("запрещенными правами"))
                        {
                            TelegramCore.getTelegramCore().InvokeCommonError(ex.Message, TelegramCore.TelegramEvents.Password);
                        }
                        MainWindowViewModel.Log.Error($"Ошибка авторизации спарвочника | {ex.Message} - {ex.StatusCode}");
                    }
                    Auth = false;
                }
                catch(Exception exception)
                {

                    Stop();                    
                    MainWindowViewModel.Log.Error($"Ошибка авторизации спарвочника | {exception}");                 
                    Auth = false;
                }               
                if (Auth == true)
                {
                    var entity = await ELMA.getElma().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}", authSprav, sessionSprav); //получение записей из справочника
                    for (int i = 0; i < entity.Count; i++)
                    {
                        try
                        {
                            var chekToken = await ELMA.getElma().UpdateTokenAndEntity<Auth>(Convert.ToInt64(entity[i].IdTelegram), entity[i].Login, entity[i].AuthToken);
                            try
                            {

                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.AuthToken = chekToken.AuthToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.SessionToken = chekToken.SessionToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth = true;

                                var allMessages = await ELMA.getElma().GetAllMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);

                                if (firstLaunch == true)
                                {
                                    await GenerateDictionary(entity[i], allMessages);

                                }

                                await FindLastMessage(entity[i], allMessages, chekToken.AuthToken, chekToken.SessionToken, entity[i].TimeMessage);
                                await FindLastComment(allMessages, entity[i]);
                                await GenerateDictionary(entity[i], allMessages);
                            }
                            catch (Exception exception)
                            {
                                MainWindowViewModel.Log.Error($"Неудалось получить сообщения пользователя {entity[i].Login} | " + exception);

                            }


                        }
                        catch (WebException exception)
                        {
                            exception.TargetSite.DeclaringType.AssemblyQualifiedName.Contains("GetRequest");
                            if(!exception.Message.Contains("Запуск сервера"))
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
                        catch (Exception exception)
                        {
                            
                        }
                    }
                    firstLaunch = false;
                }
                               
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Unexpected character"))
                {
                    Stop();
                    TelegramCore.getTelegramCore().InvokeCommonError("Убедитесь, что сервер запущен", TelegramCore.TelegramEvents.Password);
                    ReInitializationELMA();
                }
                else if (exception.StackTrace.Contains("ElmaMessages.cs:строка 123") || exception.StackTrace.Contains("ElmaMessages.cs:line 123"))
                {
                    TelegramCore.getTelegramCore().InvokeCommonError("Неверный TypeUid справочника", TelegramCore.TelegramEvents.Password);
                
                    Stop();
                }

                MainWindowViewModel.Log.Error("Ошибка обработки сообщений | " + exception);
            }

        }

        private async Task ProcessingMessages1()
        {
            try
            {
                if (!firstLaunch)
                {
                    await Task.Delay(TimeSpan.FromSeconds(waitDelay));
                }
                var authEntity = await AuthEntity(ELMA.login, Common.IsPass == "false" ? $@"""{ELMA.password}""" : ELMA.password); //Авторизация справочника
                authSprav = authEntity.AuthToken;
                sessionSprav = authEntity.SessionToken;
                var entity = await ELMA.getElma().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}", authEntity.AuthToken, authEntity.SessionToken); //получение записей из справочника
                for (int i = 0; i < entity.Count; i++) //цикл пользователей
                {
                    try
                    {
                        var chekToken = await ELMA.getElma().UpdateTokenAndEntity<Auth>(Convert.ToInt64(entity[i].IdTelegram), entity[i].Login, entity[i].AuthToken); //проверка токена

                        TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.AuthToken = chekToken.AuthToken;
                        TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.SessionToken = chekToken.SessionToken;
                        TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth = true;

                        var allMessages = await ELMA.getElma().GetAllMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);

                        if (firstLaunch)
                        {
                            await GenerateDictionary(entity[i], allMessages);
                        }
                        await FindLastMessage(entity[i], allMessages, chekToken.AuthToken, chekToken.SessionToken, entity[i].TimeMessage);
                        await FindLastComment(allMessages, entity[i]);
                        await GenerateDictionary(entity[i], allMessages);
                    }
                    catch(WebException exception)
                    {
                        if (!exception.Message.Contains("Запуск сервера") || !exception.Message.Contains("Fault xmlns") || exception.Message.Contains("Подключение не установлено"))
                        {                     
                            var ex = JsonConvert.DeserializeObject<ErrorResponse>(exception.Message);
                            if (ex.Message.Contains("Ошибка авторизации"))
                            {
                               await NotificationUser(entity[i]);
                            }
                        }
                    }                                            
                }
                firstLaunch = false;                
            }
            catch(WebException exception)
            {
                Stop();
                if (exception.Message.Contains("Запуск сервера") || exception.Message.Contains("Подключение не установлено"))
                {
                    TelegramCore.getTelegramCore().InvokeCommonError("Убедитесь, что сервер запущен", TelegramCore.TelegramEvents.Password);
                    MainWindowViewModel.Log.Error($"Ошибка авторизации спарвочника | {exception}");
                    await ReInitializationELMA();
                }
                else
                {
                    var ex = JsonConvert.DeserializeObject<ErrorResponse>(exception.Message);

                    if (ex.Message.Contains("Ошибка авторизации"))
                    {                     
                        TelegramCore.getTelegramCore().InvokeCommonError("Неверный логин или пароль", TelegramCore.TelegramEvents.Password);                        
                    }

                    if (ex.Message.Contains("запрещенными правами"))
                    {
                        TelegramCore.getTelegramCore().InvokeCommonError(ex.Message, TelegramCore.TelegramEvents.Password);
                    }

                    if(ex.Message.Contains("Тип с уникальным идентификатором не найден"))
                    {
                        TelegramCore.getTelegramCore().InvokeCommonError("Неверный TypeUid справочника", TelegramCore.TelegramEvents.Password);
                    }

                    

                    MainWindowViewModel.Log.Error($"Ошибка авторизации спарвочника | {ex.Message} - {ex.StatusCode}");
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Unexpected character"))
                {
                    Stop();
                    TelegramCore.getTelegramCore().InvokeCommonError("Убедитесь, что сервер запущен", TelegramCore.TelegramEvents.Password);
                    await ReInitializationELMA();
                }

                MainWindowViewModel.Log.Error("Ошибка обработки сообщений | " + exception);
            }

        }
        private async Task NotificationUser(EntityMargin entity)
        {
            KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, Convert.ToInt64(entity.IdTelegram));

            if (entity.AuthorizationUser == "true" || info.Value.StatusAuth == true)
            {
                OptionTelegramMessage message = new OptionTelegramMessage();
                List<string> ids = new List<string>() { CommandRoute.AUTHMENU };
                message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");

                TelegramCore.getTelegramCore().bot.ClearStepUser(Convert.ToInt64(entity.IdTelegram));
                await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity.IdTelegram), msg: "Вам нужно авторизоваться", TelegramCore.cancellation, message);

                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.StatusAuth = false;

                await UpdateStatus(entity);
            }
        }

        private  async Task UpdateStatus(EntityMargin entity) //функция обновления статуса авторизации пользователя в справочнике
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
                MainWindowViewModel.Log.Error("Ошибка обновления статуса в справочнике | " + ex);
                TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка обновления статуса", TelegramCore.TelegramEvents.Password);
               
            }

        }

        private  async Task UpdateMessage(EntityMargin entity,DateTime time) //функция обновления последнего сообщения в справочнике
        {
            try
            {
                entity.TimeMessage            = time;

                string jsonBody               = System.Text.Json.JsonSerializer.Serialize(entity);
                var updateEntity              = await ELMA.getElma().PostRequestNotDeserialze($"Entity/Update/{ELMA.TypeUid}/{entity.Id}", jsonBody, authSprav, sessionSprav);
                MainWindowViewModel.Log.Info($"Сообщение от {time} записано для {entity.Login}");

            }
            catch (Exception ex)
            {
                MainWindowViewModel.Log.Error("Ошибка обновения последнего сообщения в справочнике | " + ex);
            }           
        }
        private Task GenerateDictionary(EntityMargin entity, MessegesOtvet message)
        {
            if (message != null)
            {               
                for (int IdMes = message.Data.Count - 1; IdMes > -1; IdMes--)
                {
                    KeyValuePair<long, UserCache> info = TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram));
                    if (message.Data[IdMes].LastComments.Count != 0)
                    {                      
                        try
                        {
                                int max = message.Data[IdMes].LastComments.Data.Select(x => x.Id).Max();
                                if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                                {                                   
                                   TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(message.Data[IdMes].Id, max);        
                                                                       
                                }
                                else
                                {
                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId[message.Data[IdMes].Id]  = max;
                                }
                        }
                        catch (Exception ex) 
                        {
                            if (!ex.Message.Contains("key has already been added"))
                            {
                                MainWindowViewModel.Log.Error($"Ошибка добавления последнего комментария для {entity.Login}| " + ex);
                            }
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
            }

            return Task.CompletedTask;
        }

        private async Task FindLastMessage(EntityMargin entity,MessegesOtvet messages,string authtoken,string sessiontoken, DateTime dateMes) //функцию генерации сообщения
        {
            try
            {
                if (messages != null)
                {
                    for (int IdMes = messages.Data.Count - 1; IdMes > -1; IdMes--)
                    {                    
                        if (messages.Data[IdMes].CreationDate > dateMes && messages.Data[IdMes].IsRead == false)
                        {
                            if(messages.Data[IdMes].LastComments.Count>0)
                            {
                                int max = messages.Data[IdMes].LastComments.Data.Select(x => x.Id).Max();
                                if (TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.ContainsKey(messages.Data[IdMes].Id))
                                {
                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId[messages.Data[IdMes].Id] = max;
                                }
                                else
                                {
                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(messages.Data[IdMes].Id, max);
                                }
                            }
                            await SendMsg(messages.Data[IdMes],entity,authtoken,sessiontoken);
                            await UpdateMessage(entity, messages.Data[IdMes].CreationDate);
                            
                        }

                    }
                    
                }
            }
            catch (Exception ex)
            {
                TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка генерации сообщения для {entity.Login}", TelegramCore.TelegramEvents.Password);
                MainWindowViewModel.Log.Error($"Ошибка генерации  сообщения для {entity.Login}| " + ex) ;
               
            }
            
        }

        private async Task SendMsg(Datum messages,EntityMargin entity, string authtoken,string sessiontoken)
        {
            try
            {
                List<TaskBase> taskbase = new List<TaskBase>();

                bool isTask         = messages.ObjectGroupClass.ToLower().Contains("task");
                bool hasText        = string.IsNullOrWhiteSpace(messages.Text);
                bool isEvent        = messages.ObjectGroupText == "Событие в календаре";
                bool isAsk          = messages.ObjectGroupText.ToLower().Contains("вопрос");
                bool isPlanWork     = false;
                bool isNullDate     = true;

                if (isTask)
                {
                    var eqlQuery = $"Id={messages.ActionObjectId}";
                    taskbase = await ELMA.getElma().GetEntity<TaskBase>($"Entity/Query?type={ELMA.TypeUidTaskBase}&limit=1&q={eqlQuery}", authtoken, sessiontoken);
                    if (taskbase[0].Priority == "1") { priority = "🔴"; }
                    if (taskbase[0].Priority == "2") { priority = "🟡"; }
                    if (taskbase[0].Priority == "3") { priority = "🟢"; }
                    isPlanWork = taskbase[0].PlanWorkLog != null;
                    isNullDate = taskbase[0].StartDate == null && taskbase[0].EndDate == null;

                }

                string msg = (isAsk ? $"Новый вопрос к теме:\n{messages.Subject}" : isEvent ? "Новое событие" : (isTask ? $"{priority} Новая задача" : "Новое сообщение📋"));
                msg += "\n";
                msg += "👨‍💻 " + messages.CreationAuthor.Name;
                msg += isAsk ? "" : (isTask ? (isNullDate ? "" : $"\n⏰ Сроки \n{taskbase[0].StartDate.Replace("/", ".")}-\n{taskbase[0].EndDate.Replace("/", ".")}") : "");
                msg += isTask ? (isPlanWork ? $"\n⏳ {taskbase[0].PlanWorkLog} минут" : "") : "";
                msg += isAsk ? "" : "\n📃 " + messages.Subject;
                msg += (hasText ? "" : "\n📝 " + messages.Text);

                if (messages.LastComments.Count > 0)
                {
                    msg += "\n\nКомментарии:\n "; 
                    foreach (var comment in messages.LastComments.Data)
                    {
                        msg += "\n👨‍💻 " + messages.CreationAuthor.Name;
                        msg += comment.ActionText != "" ? $"\n📍 {comment.ActionText}" : "\n";
                        if (comment.Text != "\r\n")
                        {
                            msg += "📝 " + comment.Text;
                        }
                    }

                }
                bool msgLength = msg.Length > 4090;

                string isType = (isAsk ? $"Перейти к вопросу" : isEvent ? "Перейти к событию" : (isTask ? $"Перейти к задаче" : "Перейти к сообщению"));

                OptionTelegramMessage message = new OptionTelegramMessage();
                var ikm = new InlineKeyboardMarkup(new[]
                {
                     new[]
                     {
                       InlineKeyboardButton.WithUrl(isType, $"http://{MainWindowViewModel.Adress}:{MainWindowViewModel.Port}{messages.Url}")
                     }
                });
                message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);

                for (var i = 0; i < msg.Length; i += 4090)
                {
                   await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity.IdTelegram), msg: msgLength? msg.Substring(i, Math.Min(4090, msg.Length - i)) : msg, TelegramCore.cancellation, messages.Url != null ? message : null);
                }

                MainWindowViewModel.Log.Info($"Сообщение {messages.Id} отправлено пользователю {entity.Login}, предыдущее значение - {entity.TimeMessage}");
               
            }
            catch(Exception ex)
            {
                if(ex.Message == "Forbidden: bot was blocked by the user")
                {
                    TelegramCore.getTelegramCore().InvokeCommonError($"Сообщение {messages.Id} не доставлено {entity.Login}, т.к. он заблокировал бота", TelegramCore.TelegramEvents.Password);

                }
                MainWindowViewModel.Log.Error($"Ошибка отправки сообщения для {entity.Login}| " + ex);
            }
        }
        
        private async Task FindLastComment(MessegesOtvet messages, EntityMargin entity)
        {
            KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, Convert.ToInt64(entity.IdTelegram));
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
                                        TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId[messages.Data[IdMes].Id] = messages.Data[IdMes].LastComments.Data[IdComment].Id;
                                        await SendComment(messages.Data[IdMes],IdComment, entity.Login, Convert.ToInt64(entity.IdTelegram));
                                    }
                                }
                                catch(Exception ex)
                                {
                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(messages.Data[IdMes].Id, 0);
                                    //IdComment = IdComment - 1;
                                    MainWindowViewModel.Log.Error($"Ошибка генерации  комментария для {entity.Login}| " + ex) ;
                                   
                                }
                            }
                            
                        }
                    
                }
            }
        }
        private static async Task SendComment(Datum messages,int IdComment,string user,long idTelegram)
        {
            try
            {
                string msg = "Новый комментарий к теме:\n📃 " + messages.Subject; // messages.Data[IdMes].Subject
                msg += "\n👨‍💻 " + messages.CreationAuthor.Name;
                msg += messages.LastComments.Data[IdComment].ActionText != "" ? $"\n📍 {messages.LastComments.Data[IdComment].ActionText}" : "";

                msg += "\n";
                if (messages.LastComments.Data[IdComment].Text != "\r\n")
                {
                    msg += "📝 " + messages.LastComments.Data[IdComment].Text;
                }

                bool isTask     = messages.ObjectGroupClass.ToLower().Contains("task");
                bool isEvent    = messages.ObjectGroupText == "Событие в календаре";
                bool isAsk      = messages.ObjectGroupText.ToLower().Contains("вопрос");

                string isType = (isAsk ? $"Перейти к вопросу" : isEvent ? "Перейти к событию" : (isTask ? $"Перейти к задаче" : "Перейти к сообщению"));

                OptionTelegramMessage message = new OptionTelegramMessage();
                var ikm = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                       InlineKeyboardButton.WithUrl(isType, $"http://{MainWindowViewModel.Adress}:{MainWindowViewModel.Port}{messages.Url}")
                    }
                });
                message.MenuInlineKeyboardMarkup = MenuGenerator.UnitInlineKeyboard(ikm);

                await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: idTelegram, msg: msg, TelegramCore.cancellation, messages.Url != null ? message : null);
                MainWindowViewModel.Log.Info($"Комментарий {messages.LastComments.Data[IdComment].Id} к соообщению {messages.Id} отправлен пользователю {user}");

            }
            catch(Exception ex)
            {
                MainWindowViewModel.Log.Error($"Ошибка отправки комментария для {user}| " + ex);

            }
        }

        

        public async Task<Auth> AuthEntity(string login,string password) //функция авторизации справочника
        {
            
            var authEntity = await ELMA.getElma().PostRequestNotDeserialze($"Authorization/LoginWith?username={login}", password);
            var tokens = JsonConvert.DeserializeObject<Auth>(authEntity.Trim(new char[] { '\uFEFF' }));
            return tokens;
        }

        public async Task ReInitializationELMA()
        {
            try
            {
                var result = await AuthEntity(ELMA.login, Common.IsPass == "false" ? $@"""{ELMA.password}""" : ELMA.password);
                TelegramCore.getTelegramCore().InvokeCommonError($"Подключение установлено", TelegramCore.TelegramEvents.Password);
                Start();
            }            
            catch
            {
                int delay = 60 * 5000;
                TelegramCore.getTelegramCore().InvokeCommonError($"Попытка переподключения к серверу через {delay / 60000} минут", TelegramCore.TelegramEvents.Password);
                await Task.Delay(60*5000);
                await ReInitializationELMA();
                return;
            }
            

        }

    }
}
