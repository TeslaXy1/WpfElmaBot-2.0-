using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfElmaBot.Models;
using WpfElmaBot.Service;

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

        //public ElmaMessages(CommandRoute route)
        //{

        //    this.route = route;


        //}

        private static CancellationTokenSource _cancelTokenSource;
        private const int TimerIntervalInSeconds = 10;

        public static void Start()
        {
            Stop();
            _cancelTokenSource = new CancellationTokenSource();
            _ = MainCycle(_cancelTokenSource.Token);         
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
                    var authEntity = await new ELMA().PostRequest<Auth>($"Authorization/LoginWith?username={ELMA.login}", ELMA.password);
                    authSprav = authEntity.AuthToken;
                    sessionSprav = authEntity.SessionToken;
                }
                catch(Exception exeption)
                {
                    Stop();
                    MessageBox.Show("Неверный логин или пароль");
                    //TODO обработка ошибок
                }
                var entity = await new ELMA().GetEntity<Entity>("c5c12f67-3f57-45c2-aa70-02dfded87f77",authSprav,sessionSprav);
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
                        var chekToken = await new ELMA().UpdateToken<Auth>(authToken);
                        try
                        {
                            var message = await new ELMA().GetUnreadMessage<MessegesOtvet>(chekToken.AuthToken,chekToken.SessionToken);
                            if (message != null)
                            {
                                for (int j = message.Data.Count - 1; j > -1; j--)
                                {
                                    if (idMessage != message.Data[j].Id && idMessage < message.Data[j].Id)
                                    {
                                        if (message.Data[j].ObjectGroupText == "Задача")
                                        {
                                            if (message.Data[j].Text == null)
                                            {
                                                
                                                await route.MessageCommand.Send(TelegramCore.bot ,chatId: idTelegram, msg: "Новая задача📋" + "\n" + "👨‍💻 " + message.Data[j].CreationAuthor.Name + "\n" + "📃 " + message.Data[j].Subject, TelegramCore.cancellation);
                                            }
                                            else
                                            {
                                                await route.MessageCommand.Send(TelegramCore.bot, chatId: idTelegram, msg: "Новая задача📋" + "\n" + "👨‍💻 " + message.Data[j].CreationAuthor.Name + "\n" + "📃 " + message.Data[j].Subject+ "\n" + message.Data[j].Text, TelegramCore.cancellation);
                                            }

                                        }
                                        if (message.Data[j].ObjectGroupText == "Сообщение")
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
                                    }

                                }
                            }
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
                        UpdateStatus(userElma, idTelegram, authToken, sessionToken, loginUser , idMessage, entityId);
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
            var body = new Entity()
            {
                IdUserElma = userelma,
                IdTelegram = Convert.ToString(idtelegram),
                AuthToken = authtoken,
                SessionToken = sessiontoken,
                AuthorizationUser = "false",
                Login = login,
                IdLastSms = Convert.ToString(idmessage)
                //TypeUid = "c5c12f67-3f57-45c2-aa70-02dfded87f77",
                //Id = Convert.ToString(70),
                //Uid = "9584321b-dbdf-469b-bde2-a022c868fecb"


            };
            string ss = Convert.ToString(body);
            //TODO разораться с телом запроса
            string boody = "[{IdUserElma:" + userelma + ",IdTelegram:" +idtelegram+",AuthToken:" +authtoken+",SessionToken:" +sessiontoken +",IdLastSms:" +idmessage+",AuthorizationUser:false,Login:" + login+ "}]";
            var entity = await new ELMA().PostRequest1<Entity>($"{ELMA.FullURL}Entity/Update/{ELMA.TypeUid}/{identity}", body, authSprav, sessionSprav);
            //TODO пост запрос обновление статуса
        }
         
       //TODO авторизация справочника
       //TODO получение записей 
       //TODO прогон всех записей
    }
}
