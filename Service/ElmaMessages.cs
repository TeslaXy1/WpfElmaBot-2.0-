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

        public static string authSprav;
        public static string sessionSprav;
        public static string entityAnswer;


        public static string userElma;
        public static long idTelegram;
        public static string authToken;
        public static string sessionToken;
        public static int idMessage;
        public static string status;
        public static string loginUser;
        public static DateTime timeMessage;
        public static int entityId;


        public static string priority = "";



        public MainWindowViewModel mwvm;
        public ElmaMessages(MainWindowViewModel mwvm)
        {
            this.mwvm = mwvm;
            this.mwvm.AttachedPropertyAppend = "Здесь будут отображаться сообщения из телеграма\n" + Environment.NewLine;
            this.mwvm.AttachedPropertyAppendError = "Здесь будут отображаться неполадки в работе программы\n" + Environment.NewLine;

        }

        private static CommandRoute route = new CommandRoute();
        private static CancellationTokenSource _cancelTokenSource;
        private const int TimerIntervalInSeconds = 10;

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
                    if(Common.IsPass=="false")
                    {
                        
                        var authEntity = await AuthEntity(ELMA.login, $@"""{ELMA.password}""");
                        authSprav = authEntity.AuthToken;
                        sessionSprav = authEntity.SessionToken;
                        

                    }
                    else
                    {
                        var authEntity = await AuthEntity(ELMA.login, ELMA.password);
                        authSprav = authEntity.AuthToken;
                        sessionSprav = authEntity.SessionToken;
                    }
                    Auth = true;
                    
                }
                catch(Exception exeption)
                {
                    Stop();
                    MessageBox.Show("Неверный логин или пароль");
                    MainWindowViewModel.Log.Error("Ошибка авторизации спарвочника | "+exeption);
                    mwvm.AttachedPropertyAppendError += "Неверный логин или пароль" + "\n";
                    Auth = false;
                    

                }
                if (Auth == true)
                {
                    var entity = await ELMA.getElma().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}", authSprav, sessionSprav); //получение записей из справочника
                    for (int i = 0; i < entity.Count; i++)
                    {

                        userElma        = entity[i].IdUserElma;//id user elma
                        idTelegram      = Convert.ToInt64(entity[i].IdTelegram);//id usesr telegram
                        authToken       = entity[i].AuthToken;//authToken
                        sessionToken    = entity[i].SessionToken;//sessiaToken                   
                        idMessage       = Convert.ToInt32(entity[i].IdLastSms); //id last sms
                        status          = Convert.ToString(entity[i].AuthorizationUser); //Статус авторизации
                        loginUser       = entity[i].Login;
                        entityId        = Convert.ToInt32(entity[i].Id);// уникальный идентификатор записи в справочнике
                        timeMessage     = entity[i].TimeMessage;
                        try
                        {
                            var chekToken = await ELMA.getElma().UpdateToken<Auth>(authToken); //обновления токена пользователя
                            try
                            {
                                 
                                TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.AuthToken     = chekToken.AuthToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.SessionToken  = chekToken.SessionToken;
                                TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.StatusAuth    = true;
                                
                                var message = await ELMA.getElma().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);
                                await GenerateMsg(message, loginUser, chekToken.AuthToken, chekToken.SessionToken,timeMessage);
                                await GenerateComment(message, loginUser, chekToken.AuthToken, chekToken.SessionToken,idTelegram);
                            }
                            catch (Exception exception)
                            {

                                MainWindowViewModel.Log.Error($"Неудалось получить сообщения пользователя {loginUser} | " + exception);

                            }


                        }
                        catch (Exception exception)
                        {
                            KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, idTelegram);
                            MainWindowViewModel.Log.Error("Ошибка обновления токена | " + exception);
                            await UpdateStatus(userElma, idTelegram, authToken, sessionToken, loginUser, idMessage, entityId);
                            if (status == "true" || info.Value.StatusAuth==true)
                            {                                            
                                    List<string> ids = new List<string>() { "🔑Авторизация" };
                                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                                    TelegramCore.getTelegramCore().bot.ClearStepUser(idTelegram);                               
                                    await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: idTelegram, msg: "Вам нужно авторизоваться", TelegramCore.cancellation, message);
                                    TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.StatusAuth = false;
                            }

                        }
                    }
                }
                
                
            }
            catch(Exception exception)
            {

                if(exception.StackTrace.Contains("ElmaMessages.cs:строка 125") || exception.StackTrace.Contains("ElmaMessages.cs:line 125"))
                {
                    mwvm.AttachedPropertyAppendError += "Неверный TypeUid справочника";
                    Stop();
                }
                
                MainWindowViewModel.Log.Error("Ошибка обработки сообщений | " + exception);
            }

        }
        
        public static async Task UpdateStatus(string userelma, long idtelegram, string authtoken, string sessiontoken,string login , int idmessage, int identity) //функция обновления статуса авторизации пользователя в справочнике
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
                    IdLastSms           = Convert.ToString(idmessage),
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

                MainWindowViewModel.Log.Error("Ошибка обновления статуса в справочнике | " + ex);


            }

        }

        public static async Task UpdateMessage(string userelma, long idtelegram, string authtoken, string sessiontoken, string login, int maxIdMes, int identity) //функция обновления последнего сообщения в справочнике
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
                    IdLastSms           = Convert.ToString(maxIdMes),
                    TimeMessage         = DateTime.Now
                };
                string jsonBody         = System.Text.Json.JsonSerializer.Serialize(body);
                var entity              = await ELMA.getElma().PostRequest<Entity>($"Entity/Update/{ELMA.TypeUid}/{identity}", jsonBody, authSprav, sessionSprav);
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains( "line 1, position 4."))
                {
                    MainWindowViewModel.Log.Error("Успешное обновление записи| " + ex);
                }
               
                MainWindowViewModel.Log.Error("Ошибка обновения последнего сообщения в справочнике | " + ex);

                
                
            }
            

        }
        public async Task GenerateMsg(MessegesOtvet message,string user,string authtoken,string sessiontoken, DateTime dateMes) //функцию генерации сообщения
        {
            try
            { 
                int maxIdMes = message.Data.Select(x => x.Id).Max(); //последнее сообщение
                if (message != null)
                {
                    for (int IdMes = message.Data.Count - 1; IdMes > -1; IdMes--)
                    {
                        KeyValuePair<long, UserCache> info = BotExtension.GetCacheData(TelegramCore.getTelegramCore().bot, idTelegram);
                        if (message.Data[IdMes].LastComments.Count!=0)
                        {
                            int max = message.Data[IdMes].LastComments.Data.Select(x => x.Id).Max();    
                            try
                            {

                                if (info.Value.LastCommentId != null)
                                {
                                    if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                                    {

                                        TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.LastCommentId.Add(message.Data[IdMes].Id, max);

                                    }
                                }
                                else
                                {
                                    if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                                    {

                                        TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.LastCommentId.Add(message.Data[IdMes].Id, 0);

                                    }

                                }
                            }
                            catch (Exception ex) { MainWindowViewModel.Log.Error($"Ошибка добавления последнего комментария для {user}| " + ex); }
                        }
                        else
                        {
                            if (!info.Value.LastCommentId.ContainsKey(message.Data[IdMes].Id))
                            {

                                TelegramCore.getTelegramCore().bot.GetCacheData(idTelegram).Value.LastCommentId.Add(message.Data[IdMes].Id, 0);

                            }
                        }
                        

                        if (idMessage != message.Data[IdMes].Id && idMessage < message.Data[IdMes].Id || message.Data[IdMes].CreationDate > dateMes )
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

                            await route.MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: idTelegram, msg: msg, TelegramCore.cancellation);
                            MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Сообщение {message.Data[IdMes].Id} отправлено пользователю {user}");
                        }

                    }
                    await UpdateMessage(userElma, idTelegram, authToken, sessionToken, loginUser, maxIdMes, entityId);
                }
            }
            catch(Exception ex)
            {
                mwvm.AttachedPropertyAppendError = $"{DateTime.Now.ToString("g")} Ошибка генерации сообщения для {user}\n";
                MainWindowViewModel.Log.Error($"Ошибка генерации  сообщения для {user}| " + ex) ;
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
                                    mwvm.AttachedPropertyAppendError = $"{DateTime.Now.ToString("g")} Ошибка генерации комментария для {user}\n";
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
