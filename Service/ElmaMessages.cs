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
            TelegramCore.getTelegramCore().InvokeCommonStatus($"Бот запущен", TelegramCore.TelegramEvents.Status);

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
                if(firstLaunch!=true)
                {
                     await Task.Delay(TimeSpan.FromSeconds(wait)); 
                }              
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
                            //MessageBox.Show("Убедитесь, что сервер запущен");
                            TelegramCore.getTelegramCore().InvokeCommonError("Убедитесь, что сервер запущен", TelegramCore.TelegramEvents.Password);
                            
                        }
                        else
                        {
                            //MessageBox.Show("Неверный логин или пароль");
                            TelegramCore.getTelegramCore().InvokeCommonError("Неверный логин или пароль", TelegramCore.TelegramEvents.Password);
                      
                        }
                        
                    }
                    MainWindowViewModel.Log.Error($"Ошибка авторизации спарвочника ||| {exeption.GetBaseException().ToString} ||| {exeption}");                 
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
                                 
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.AuthToken     = chekToken.AuthToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.SessionToken  = chekToken.SessionToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth    = true;


                                var allMessages = await ELMA.getElma().GetAllMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);

                                //var unreadMessages = await ELMA.getElma().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);
                                
                                if(firstLaunch==true)
                                {
                                    await GenerateDictionary(entity[i], allMessages);
                                    
                                }
                                
                                await GenerateMsg(entity[i], allMessages,  chekToken.AuthToken, chekToken.SessionToken, entity[i].TimeMessage);
                                await GenerateComment(allMessages, entity[i].Login, Convert.ToInt64(entity[i].IdTelegram));
                                await GenerateDictionary(entity[i], allMessages);
                            }
                           
                            catch (Exception exception)
                            {

                                MainWindowViewModel.Log.Error($"Неудалось получить сообщения пользователя {entity[i].Login} | " + exception);
                               // MessageBox.Show("" + exception);
                            

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
                    firstLaunch = false;
                }
                
                
            }
            catch (Exception exception)
            {

                if(exception.StackTrace.Contains("ElmaMessages.cs:строка 124") || exception.StackTrace.Contains("ElmaMessages.cs:line 124"))
                {
                    TelegramCore.getTelegramCore().InvokeCommonError("Неверный TypeUid справочника", TelegramCore.TelegramEvents.Password);
                
                    Stop();
                }

                MainWindowViewModel.Log.Error("Ошибка обработки сообщений | " + exception);
            }

        }
        
        public  async Task UpdateStatus(EntityMargin entity) //функция обновления статуса авторизации пользователя в справочнике
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

        public  async Task UpdateMessage(EntityMargin entity,DateTime time) //функция обновления последнего сообщения в справочнике
        {

            try
            {

                entity.TimeMessage = time;

                string jsonBody               = System.Text.Json.JsonSerializer.Serialize(entity);
                var updateEntity              = await ELMA.getElma().PostRequestNotDeserialze($"Entity/Update/{ELMA.TypeUid}/{entity.Id}", jsonBody, authSprav, sessionSprav);

            }
            catch (Exception ex)
            {
                    MainWindowViewModel.Log.Error("Ошибка обновения последнего сообщения в справочнике | " + ex);

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
                        int max = message.Data[IdMes].LastComments.Data.Select(x => x.Id).Max(); //TODO не последний комментарий
                        try
                        {

                            //if (info.Value.LastCommentId != null)
                            //{
                                if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                                {
                                    

                                   TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(message.Data[IdMes].Id, max);        
                                                                       
                                }
                                else
                                {
                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(message.Data[IdMes].Id, 0);
                                }
                            //}
                            //else
                            //{
                            //    if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                            //    {

                            //        TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity.IdTelegram)).Value.LastCommentId.Add(message.Data[IdMes].Id, 0);

                            //    }

                            //}
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

                            await SendMsg(messages.Data[IdMes],entity,authtoken,sessiontoken);
                            await UpdateMessage(entity, messages.Data[IdMes].CreationDate);
                            if(messages.Data[IdMes].LastComments.Count>0)
                            {
                                //await SendComment(messages.Data[IdMes], 0, entity.Login, Convert.ToInt64(entity.IdTelegram));
                            }
                            
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

                string msg = (isAsk ? $"Новый вопрос к теме:\n{messages.Subject}" : isEvent ? "Новое событие" : (isTask ? $"{priority}Новая задача" : "Новое сообщение📋"));
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

                bool msgLength = msg.Length > 4090;
                //if(msg.Length>4090)
                //{
                    for (var i = 0; i < msg.Length; i += 4090)
                    {

                        await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity.IdTelegram), msg: msgLength? msg.Substring(i, Math.Min(4090, msg.Length - i)):msg, TelegramCore.cancellation);

                    }
                    MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Сообщение {messages.Id} отправлено пользователю {entity.Login}");

                //}
                //else
                //{
                //    await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity.IdTelegram), msg: msg, TelegramCore.cancellation, messages.Url != null ? message : null);
                //    MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Сообщение {messages.Id} отправлено пользователю {entity.Login}");
                //}

               
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex + "");
                MainWindowViewModel.Log.Error($"Ошибка отправки сообщения для {entity.Login}| " + ex);
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

                                        await SendComment(messages.Data[IdMes],IdComment,user,idTelegram);

                                    }
                                }
                                catch(Exception ex)
                                {

                                    //TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка генерации комментария для {user}", TelegramCore.TelegramEvents.Password);
                                    MainWindowViewModel.Log.Error($"Ошибка генерации  комментария для {user}| " + ex) ;
                                   
                                }
                            }
                            
                        }
                    
                }
            }
        }
        public async Task SendComment(Datum messages,int IdComment,string user,long idTelegram)
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
                MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Комментарий {messages.Id} отправлен пользователю {user}");


                messages.LastComments.Data[IdComment].IsRead = true; //не записывать 
            }
            catch(Exception ex)
            {
                MainWindowViewModel.Log.Error($"Ошибка отправки комментария для {user}| " + ex);

            }
        }

        

        public async Task<Auth> AuthEntity(string login,string password) //функция авторизации справочника
        {
            var authEntity = await ELMA.getElma().PostRequest<Auth>($"Authorization/LoginWith?username={login}", password);
            return authEntity;
        }

    }
}
