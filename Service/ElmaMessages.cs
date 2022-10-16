using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfElmaBot.Models;
using WpfElmaBot.Service;
using WpfElmaBot.Service.Commands;
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
        public static int entityId;

        

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
        public  async Task ProcessingMessages()
        {
            try
            {
                bool Auth;
                var wait = 5;
                await Task.Delay(TimeSpan.FromSeconds(wait));
                try
                {
                    //await ELMA.getInstance().PostRequest<Auth>($"Authorization/LoginWith?username={ELMA.login}", ELMA.password);
                    if(Common.IsPass=="true")
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
                    var entity = await ELMA.getInstance().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}", authSprav, sessionSprav);
                    for (int i = 0; i < entity.Count; i++)
                    {

                        userElma = entity[i].IdUserElma;//id user elma
                        idTelegram = Convert.ToInt64(entity[i].IdTelegram);//id user telegram
                        authToken = entity[i].AuthToken;//authToken
                        sessionToken = entity[i].SessionToken;//sessiaToken                   
                        idMessage = Convert.ToInt32(entity[i].IdLastSms); //id last sms
                        status = Convert.ToString(entity[i].AuthorizationUser); //Статус авторизации
                        loginUser = entity[i].Login;
                        entityId = Convert.ToInt32(entity[i].Id);// уникальный идентификатор записи в справочнике
                        try
                        {
                            var chekToken = await ELMA.getInstance().UpdateToken<Auth>(authToken);
                            try
                            {
                                TelegramCore.getInstance().bot.GetCacheData(idTelegram).Value.AuthToken = chekToken.AuthToken;
                                TelegramCore.getInstance().bot.GetCacheData(idTelegram).Value.SessionToken = chekToken.SessionToken;
                                TelegramCore.getInstance().bot.GetCacheData(idTelegram).Value.StatusAuth = true;
                                var message = await ELMA.getInstance().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken, chekToken.SessionToken);
                                GenerateMsg(message, loginUser);
                            }
                            catch (Exception exception)
                            {

                                MainWindowViewModel.Log.Error($"Неудалось получить сообщения пользователя {loginUser} | " + exception);

                            }


                        }
                        catch (Exception exception)
                        {

                            KeyValuePair<long, UserCache> Check = BotExtension.GetCacheData(TelegramCore.getInstance().bot, idTelegram);
                            TelegramCore.getInstance().bot.GetCacheData(idTelegram).Value.StatusAuth = false;
                            TelegramCore.getInstance().bot.GetCacheData(idTelegram).Value.AuthToken = null;
                            TelegramCore.getInstance().bot.GetCacheData(idTelegram).Value.SessionToken = null;
                            MainWindowViewModel.Log.Error("Ошибка обновления токена | " + exception);
                            await UpdateStatus(userElma, idTelegram, authToken, sessionToken, loginUser, idMessage, entityId);
                            if (status == "true")
                            {
                                
                                
                                    List<string> ids = new List<string>() { "🔑Авторизация" };
                                    message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(2, ids, "");
                                    TelegramCore.getInstance().bot.ClearStepUser(idTelegram);                               
                                    await route.MessageCommand.Send(TelegramCore.getInstance().bot, chatId: idTelegram, msg: "Вам нужно авторизоваться", TelegramCore.cancellation, message);




                            }

                        }
                    }
                }
                
                
            }
            catch(Exception exception)
            {

                if(exception.StackTrace.Contains("ElmaMessages.cs:строка 120") || exception.StackTrace.Contains("ElmaMessages.cs:line 120"))
                {
                    mwvm.AttachedPropertyAppendError += "Неверный TypeUid справочника";
                    Stop();
                }
                
                MainWindowViewModel.Log.Error("Ошибка обработки сообщений | " + exception);
            }

        }
        
        public static async Task UpdateStatus(string userelma, long idtelegram, string authtoken, string sessiontoken,string login , int idmessage, int identity)
        {
            try
            {
                var body = new EntityMargin()
                {
                    IdUserElma = userelma,
                    IdTelegram = Convert.ToString(idtelegram),
                    AuthToken = authtoken,
                    SessionToken = sessiontoken,
                    AuthorizationUser = "false",
                    Login = login,
                    IdLastSms = Convert.ToString(idmessage)
                };
                string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                var entity = await ELMA.getInstance().PostRequest<Entity>($"Entity/Update/{ELMA.TypeUid}/{identity}", jsonBody, authSprav, sessionSprav);
            }
            catch(Exception ex)
            {
                if(ex.Message.Contains("Error converting value to type 'WpfElmaBot.Models.Entity'. Path '', line 1, position 4."))
                {

                }
                else
                {
                    MainWindowViewModel.Log.Error("Ошибка обновления статуса в справочнике | " + ex);
                }
                
                
               
            }

        }

        public static async Task UpdateMessage(string userelma, long idtelegram, string authtoken, string sessiontoken, string login, int maxIdMes, int identity)
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
                    IdLastSms = Convert.ToString(maxIdMes)
                };
                string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                var entity = await ELMA.getInstance().PostRequest<Entity>($"Entity/Update/{ELMA.TypeUid}/{identity}", jsonBody, authSprav, sessionSprav);
            }
            catch (Exception ex)
            {
                if(ex.Message == "Error converting value \"76\" to type 'WpfElmaBot.Models.Entity'. Path '', line 1, position 4.")
                { }
                else
                {
                    MainWindowViewModel.Log.Error("Ошибка обновения последнего сообщения в справочнике | " + ex);

                }
                
            }
            

        }
        public async Task GenerateMsg(MessegesOtvet message,string user)
        {
            try
            { 
                int maxIdMes = message.Data.Select(x => x.Id).Max(); //последнее сообщение
                if (message != null)
                {
                    for (int j = message.Data.Count - 1; j > -1; j--)
                    {
                        if (idMessage != message.Data[j].Id && idMessage < message.Data[j].Id)
                        {

                            bool isTask = message.Data[j].ObjectGroupText == "Задача";
                            bool hasText = message.Data[j].Text == null;
                            string msg = (isTask ? "Новая задача📋" : "Новое сообщение📋");
                            msg += "\n";
                            msg += "👨‍💻 " + message.Data[j].CreationAuthor.Name;
                            msg += "\n";
                            msg += "📃 " + message.Data[j].Subject;
                            msg += (hasText ? "" : "\n📝" + message.Data[j].Text);
                            await route.MessageCommand.Send(TelegramCore.getInstance().bot, chatId: idTelegram, msg: msg, TelegramCore.cancellation);
                            MainWindowViewModel.Log.Info($"{DateTime.Now.ToString("g")} - Сообщение {message.Data[j].Id} отправлено пользователю {user}");
                        }

                    }
                    await UpdateMessage(userElma, idTelegram, authToken, sessionToken, loginUser, maxIdMes, entityId);
                }
            }
            catch(Exception ex)
            {
                mwvm.AttachedPropertyAppendError = $"{DateTime.Now.ToString("g")} неверный логин или пароль для справочника";
                MainWindowViewModel.Log.Error("Ошибка авторизации спарвочника | " + ex);
            }
        }

        public async Task<Auth> AuthEntity(string login,string password)
        {
            var authEntity = await ELMA.getInstance().PostRequest<Auth>($"Authorization/LoginWith?username={login}", password);
            return authEntity;
        }


    }
}
