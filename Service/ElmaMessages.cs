using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfElmaBot.Models;
using WpfElmaBot.Service;
using WpfElmaBot_2._0_.Models.EntityPack;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot_2._0_.Service
{
    internal class ElmaMessages
    {
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

       

        private static CommandRoute route = new CommandRoute();

        private static CancellationTokenSource _cancelTokenSource;
        private const int TimerIntervalInSeconds = 10;

        public static void Start()
        {
            Stop();
            _cancelTokenSource = new CancellationTokenSource();
            _ = MainCycle(_cancelTokenSource.Token);

            //new MainWindowViewModel("Бот запущен");

        }
        public bool IsWorking => _cancelTokenSource != null;

        async static Task MainCycle(CancellationToken token)
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
                    

                }

            }
        } //цикл для получения смс
        public static void Stop()
        {
            _cancelTokenSource?.Cancel();
            _cancelTokenSource = null;
        }
        public static async Task ProcessingMessages()
        {
            try
            { 
                var wait = 5;
                await Task.Delay(TimeSpan.FromSeconds(wait));
                try
                {
                    var authEntity = await ELMA.getInstance().PostRequest<Auth>($"Authorization/LoginWith?username={ELMA.login}", ELMA.password);
                    authSprav = authEntity.AuthToken;
                    sessionSprav = authEntity.SessionToken;
                }
                catch(Exception exeption)
                {
                    Stop();
                    MessageBox.Show("Неверный логин или пароль");
                    //TODO обработка ошибок
                }
                var entity = await ELMA.getInstance().GetEntity<EntityMargin>($"Entity/Query?type={ELMA.TypeUid}", authSprav,sessionSprav);
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
                            var message = await ELMA.getInstance().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken,chekToken.SessionToken);
                            GenerateMsg(message);
                        }
                        catch (Exception exception)
                        {
                            //TODO обработчик ошибок
                            i++;
                        }


                    }
                    catch (Exception exception)
                    {
                        //TODO обновление данных в справочнике (AuthorizationUser = false)
                        await UpdateStatus(userElma, idTelegram, authToken, sessionToken, loginUser , idMessage, entityId);
                        await route.MessageCommand.Send(TelegramCore.bot, chatId: idTelegram, msg: "Вам нужно авторизоваться", TelegramCore.cancellation);

                        i++;
                    }
                }
                
                
            }
            catch(Exception exception)
            {
                //TODO обработчик ошибок
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
                //TODO обработать исключение
            }
                    
            //TODO пост запрос обновление статуса
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
                //TODO обработать исключение
            }
            //TODO записать последнее сообщение

        }
        public static async Task GenerateMsg(MessegesOtvet message)
        {
            int maxIdMes = message.Data.Select(x => x.Id).Max(); //последнее сообщение
            if (message != null)
            {
                for (int j = message.Data.Count - 1; j > -1; j--)
                {
                    if (idMessage != message.Data[j].Id && idMessage < message.Data[j].Id)
                    {
                        //var msg = GenerateMsg.GetMsg();

                        //bool isTask = message.Data[j].ObjectGroupText == "Задача";
                        //bool hasText = message.Data[j].Text != null;

                        //string msg = (isTask ? "Новая задача📋" : "Новое сообщени");
                        //msg += "\n ";
                        //if (hasText)
                        //{

                        //}

                        if (message.Data[j].ObjectGroupText == "Задача")
                        {
                            try
                            {
                                if (message.Data[j].Text == null)
                                {

                                    await route.MessageCommand.Send(TelegramCore.bot, chatId: idTelegram, msg: "Новая задача📋" + "\n" + "👨‍💻 " + message.Data[j].CreationAuthor.Name + "\n" + "📃 " + message.Data[j].Subject, TelegramCore.cancellation);
                                }
                                else
                                {
                                    await route.MessageCommand.Send(TelegramCore.bot, chatId: idTelegram, msg: "Новая задача📋" + "\n" + "👨‍💻 " + message.Data[j].CreationAuthor.Name + "\n" + "📃 " + message.Data[j].Subject + "\n" + message.Data[j].Text, TelegramCore.cancellation);
                                }
                            }
                            catch (Exception ex)
                            {
                                //TODO обработка ошибок
                            }

                        }
                        if (message.Data[j].ObjectGroupText == "Сообщение")
                        {
                            try
                            {


                                if (message.Data[j].Text == null)
                                {

                                    await route.MessageCommand.Send(TelegramCore.bot, chatId: idTelegram, msg: "Новаое сообщение📋" + "\n" + "👨‍💻 " + message.Data[j].CreationAuthor.Name + "\n" + "📃 " + message.Data[j].Subject, TelegramCore.cancellation);
                                }
                                else
                                {
                                    await route.MessageCommand.Send(TelegramCore.bot, chatId: idTelegram, msg: "Новое сообщение📋" + "\n" + "👨‍💻 " + message.Data[j].CreationAuthor.Name + "\n" + "📃 " + message.Data[j].Subject + "\n" + message.Data[j].Text, TelegramCore.cancellation);
                                }
                            }
                            catch (Exception ex)
                            {
                                //TODO обработка ошибок
                            }
                        }
                    }

                }
                await UpdateMessage(userElma, idTelegram, authToken, sessionToken, loginUser, maxIdMes, entityId);
            }
        }
         
       //TODO авторизация справочника
       //TODO получение записей 
       //TODO прогон всех записей
    }
}
