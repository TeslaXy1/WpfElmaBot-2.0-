using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public OptionTelegramMessage message = new OptionTelegramMessage();

        private static string authSprav;
        private static string sessionSprav;

        //private static string userElma;
        //private static long idTelegram;
        //private static string authToken;
        //private static string sessionToken;
        //private static int idMessage;
        //private static string status;
        //private static string loginUser;
        //private static DateTime timeMessage;
        //private static int entityId;

        public static string priority = "";


        private static CommandRoute route = new CommandRoute();
        private static CancellationTokenSource _cancelTokenSource;
        private const int TimerIntervalInSeconds = 10;

        public MainWindowViewModel mwvm;
        public ElmaMessages(MainWindowViewModel mwvm)
        {
            this.mwvm = mwvm;

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
                var nextTime = Task.Delay(TimeSpan.FromSeconds(TimerIntervalInSeconds));
                try
                {
                    await Task.WhenAll(nextTime, ProcessingMessages());
                }
                catch (Exception exception)
                {
                    MainWindowViewModel.Log.Error("Ошибка цикла получения смс | " + exception);
                    //TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка цикла получения смс", TelegramCore.TelegramEvents.Password);

                }

            }
        } //цикл для получения смс
        public static void Stop()
        {
            _cancelTokenSource?.Cancel();
            _cancelTokenSource = null;
        }
        public  async Task ProcessingMessages() //функция обработки сообщений
        {
            try
            {
                bool Auth;
                var wait = 5;
                await Task.Delay(TimeSpan.FromSeconds(wait));
                try
                {
                    //if(Common.IsPass=="false")
                    //{
                        
                        var authEntity = await AuthEntity(ELMA.login, Common.IsPass=="false"? $@"""{ELMA.password}""" : ELMA.password);
                        authSprav = authEntity.AuthToken;
                        sessionSprav = authEntity.SessionToken;
                        

                    //}
                    //else
                    //{
                    //    var authEntity = await AuthEntity(ELMA.login, ELMA.password);
                    //    authSprav = authEntity.AuthToken;
                    //    sessionSprav = authEntity.SessionToken;
                    //}
                    Auth = true;
                    
                }
                catch(Exception exeption)
                {
                    Stop();
                    MessageBox.Show("Неверный логин или пароль");
                    MainWindowViewModel.Log.Error("Ошибка авторизации спарвочника | "+exeption);
                    //mwvm.AttachedPropertyAppendError = "Неверный логин или пароль" + "\n";
                    TelegramCore.getTelegramCore().InvokeCommonError("Неверный логин или пароль", TelegramCore.TelegramEvents.Password);
                    Auth = false;
                    

                }
                if (Auth == true)
                {
                    var entity = await ELMA.getElma().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}", authSprav, sessionSprav); //получение записей из справочника
                    for (int i = 0; i < entity.Count; i++)
                    {

                        //userElma        = entity[i].IdUserElma;//id user elma
                        //idTelegram      = Convert.ToInt64(entity[i].IdTelegram);//id usesr telegram
                        //authToken       = entity[i].AuthToken;//authToken
                        //sessionToken    = entity[i].SessionToken;//sessiaToken                   
                        ////idMessage       =  Convert.ToInt32(entity[i].IdLastSms); //id last sms
                        //status          = Convert.ToString(entity[i].AuthorizationUser); //Статус авторизации
                        //loginUser       = entity[i].Login;
                        //entityId        = Convert.ToInt32(entity[i].Id);// уникальный идентификатор записи в справочнике
                        //timeMessage     = entity[i].TimeMessage;
                        try
                        {
                            var chekToken = await ELMA.getElma().UpdateToken<Auth>(entity[i].AuthToken); //обновления токена пользователя
                            try
                            {
                                 
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.AuthToken     = chekToken.AuthToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.SessionToken  = chekToken.SessionToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth    = true;


                                var allMessages = await ELMA.getElma().GetAllMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);

                                //var unreadMessages = await ELMA.getElma().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);
                                
                               
                                await GenerateDictionary(entity[i], allMessages);
                                await GenerateMsg(entity[i], allMessages, entity[i].Login, chekToken.AuthToken, chekToken.SessionToken, entity[i].TimeMessage);
                                await GenerateComment(allMessages, entity[i].Login, chekToken.AuthToken, chekToken.SessionToken, Convert.ToInt64(entity[i].IdTelegram));
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
                            //try
                            //{
                            //    var chekToken = await ELMA.getElma().UpdateToken<Auth>(info.Value.AuthToken);

                            //    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.AuthToken = chekToken.AuthToken;
                            //    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.SessionToken = chekToken.SessionToken;
                            //    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth = true;

                            //    //var unreadMessages = await ELMA.getElma().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);
                            //    var allMessages = await ELMA.getElma().GetAllMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);

                            //    await GenerateDictionary(entity[i], allMessages);
                            //    await GenerateMsg(entity[i], allMessages, entity[i].Login, chekToken.AuthToken, chekToken.SessionToken, entity[i].TimeMessage);
                            //    await GenerateComment(allMessages, entity[i].Login, chekToken.AuthToken, chekToken.SessionToken, Convert.ToInt64(entity[i].IdTelegram));
                            //}
                            //catch
                            //{
                                MainWindowViewModel.Log.Error("Ошибка обновления токена | " + exception);
                                await UpdateStatus(entity[i].IdUserElma, Convert.ToInt64(entity[i].IdTelegram), entity[i].AuthToken, entity[i].SessionToken, entity[i].Login, Convert.ToInt32(entity[i].Id), entity[i].TimeMessage);
                                if (entity[i].AuthorizationUser == "true" || info.Value.StatusAuth == true)
                                {
                                    List<string> ids = new List<string>() { CommandRoute.AUTHMENU };
                                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                                    TelegramCore.getTelegramCore().bot.ClearStepUser(Convert.ToInt64(entity[i].IdTelegram));
                                    await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity[i].IdTelegram), msg: "Вам нужно авторизоваться", TelegramCore.cancellation, message);
                                    TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[i].IdTelegram)).Value.StatusAuth = false;
                                }
                            //}

                        }
                    }
                }
                
                
            }
            catch(Exception exception)
            {

                if(exception.StackTrace.Contains("ElmaMessages.cs:строка 122") || exception.StackTrace.Contains("ElmaMessages.cs:line 122"))
                {
                    TelegramCore.getTelegramCore().InvokeCommonError("Неверный TypeUid справочника", TelegramCore.TelegramEvents.Password);
                    
                    Stop();
                }
                
                MainWindowViewModel.Log.Error("Ошибка обработки сообщений | " + exception);
            }

        }
        
        public static async Task UpdateStatus(string userelma, long idtelegram, string authtoken, string sessiontoken,string login , int identity,DateTime time) //функция обновления статуса авторизации пользователя в справочнике
        {
            try
            {
                TelegramCore.getTelegramCore().bot.GetCacheData(idtelegram).Value.StatusAuth    = false;
                TelegramCore.getTelegramCore().bot.GetCacheData(idtelegram).Value.AuthToken     = null;
                TelegramCore.getTelegramCore().bot.GetCacheData(idtelegram).Value.SessionToken  = null;

                var body = new EntityMargin()
                {
                    IdUserElma          = userelma,
                    IdTelegram          = Convert.ToString(idtelegram),
                    AuthToken           = authtoken,
                    SessionToken        = sessiontoken,
                    AuthorizationUser   = "false",
                    Login               = login,
                    //IdLastSms           = Convert.ToString(idmessage),
                    TimeMessage         = DateTime.Now
                };
                string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                var entity = await ELMA.getElma().PostRequest<Entity>($"Entity/Update/{ELMA.TypeUid}/{identity}", jsonBody, authSprav, sessionSprav);
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
                }

                
                //TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка обновления статуса {userElma}", TelegramCore.TelegramEvents.Password);

            }

        }

        public static async Task UpdateMessage(string userelma, long idtelegram, string authtoken, string sessiontoken, string login, int maxIdMes, int identity,DateTime time) //функция обновления последнего сообщения в справочнике
        {

            try
            {
                var body = new EntityMargin()
                {
                    IdUserElma          = userelma,
                    IdTelegram          = Convert.ToString(idtelegram),
                    AuthToken           = authtoken,
                    SessionToken        = sessiontoken,
                    AuthorizationUser   = "true",
                    Login               = login,
                    //IdLastSms           = Convert.ToString(maxIdMes),
                    TimeMessage         = time
                };
                string jsonBody         = System.Text.Json.JsonSerializer.Serialize(body);
                var entity              = await ELMA.getElma().PostRequest<Entity>($"Entity/Update/{ELMA.TypeUid}/{identity}", jsonBody, authSprav, sessionSprav);
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
        public async Task GenerateMsg(EntityMargin entity,MessegesOtvet message,string user,string authtoken,string sessiontoken, DateTime dateMes) //функцию генерации сообщения
        {
            try
            {
                int maxIdMes = message.Data.Count >0 ? message.Data.Select(x => x.Id).Max(): 0; //последнее сообщение
                if (message != null)
                {
                    for (int IdMes = message.Data.Count - 1; IdMes > -1; IdMes--)
                    {                    
                        //idMessage != message.Data[IdMes].Id && idMessage < message.Data[IdMes].Id ||
                        if (message.Data[IdMes].CreationDate > dateMes && message.Data[IdMes].IsRead == false)
                        {

                            bool isTask     = message.Data[IdMes].ObjectGroupText == "Задача";
                            bool hasText    = message.Data[IdMes].Text == null;

                            List<TaskBase> taskbase = new List<TaskBase>();
                            if (isTask)
                            {
                                var eqlQuery = $"Id={message.Data[IdMes].ActionObjectId}";
                                taskbase = await ELMA.getElma().GetEntity<TaskBase>($"Entity/Query?type={ELMA.TypeUidTaskBase}&limit=1&q={eqlQuery}", authtoken, sessiontoken);
                                if (taskbase[0].Priority == "1") { priority = "🔴"; }
                                if (taskbase[0].Priority == "2") { priority = "🟡"; }
                                if (taskbase[0].Priority == "3") { priority = "🟢"; }

                            }

                            string msg = (isTask ? $"{priority}Новая задача" : "Новое сообщение📋");
                            msg += "\n";
                            msg += "👨‍💻 " + message.Data[IdMes].CreationAuthor.Name;
                            msg += "\n";
                            msg += (isTask ? $"⏰Сроки \n{taskbase[0].StartDate.Replace("/",".")}-\n{taskbase[0].EndDate.Replace("/", ".")}\n": "");
                            msg += "📃 " + message.Data[IdMes].Subject;
                            msg += (hasText ? "" : "\n📝" + message.Data[IdMes].Text);

                            await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity.IdTelegram), msg: msg, TelegramCore.cancellation);
                            MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Сообщение {message.Data[IdMes].Id} отправлено пользователю {entity.Login}");
                            await UpdateMessage(entity.IdUserElma, Convert.ToInt64(entity.IdTelegram), authtoken, sessiontoken, entity.Login, maxIdMes, Convert.ToInt32(entity.Id), message.Data[IdMes].CreationDate);
                        }

                    }
                    
                }
            }
            catch(Exception ex)
            {
                TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка генерации сообщения для {entity.Login}", TelegramCore.TelegramEvents.Password);
                //mwvm.AttachedPropertyAppendError = $"{DateTime.Now.ToString("g")} Ошибка генерации сообщения для {user}\n";
                MainWindowViewModel.Log.Error($"Ошибка генерации  сообщения для {entity.Login}| " + ex) ;
            }
        }
        public async Task GenerateComment(MessegesOtvet message,string user,string authToken,string sessionToken,long idTelegram)
        {
            KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, idTelegram);
            if (message != null)
            {
                for (int IdMes = message.Data.Count - 1; IdMes > -1; IdMes--)
                { 
                        for(int IdComment = 0; IdComment < message.Data[IdMes].LastComments.Count;IdComment++)
                        {                      
                            if (message.Data[IdMes].LastComments.Count != 0)
                            {
                                try
                                {
                                    if (info.Value.LastCommentId[message.Data[IdMes].Id] != message.Data[IdMes].LastComments.Data[IdComment].Id && info.Value.LastCommentId[message.Data[IdMes].Id] < message.Data[IdMes].LastComments.Data[IdComment].Id)
                                    {
                                        
                                        TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.LastCommentId[message.Data[IdMes].Id] = message.Data[IdMes].LastComments.Data[IdComment].Id;

                                        string msg = "Новый комментарий к теме:\n     " + message.Data[IdMes].Subject;
                                        msg += "\n";
                                        msg += "👨‍💻" + message.Data[IdMes].CreationAuthor.Name;
                                        msg += "\n";
                                        msg += "📝" + message.Data[IdMes].LastComments.Data[IdComment].Text;

                                        await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: idTelegram, msg: msg, TelegramCore.cancellation);
                                        MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Комментарий {message.Data[IdMes].Id} отправлено пользователю {user}");

                                    }
                                }catch(Exception ex)
                                {

                                    TelegramCore.getTelegramCore().InvokeCommonError($"Ошибка генерации комментария для {user}", TelegramCore.TelegramEvents.Password);
                                    //mwvm.AttachedPropertyAppendError = $"{DateTime.Now.ToString("g")} Ошибка генерации комментария для {user}\n";
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
