using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using WpfElmaBot.Models;
using WpfElmaBot.Service.Commands;
using WpfElmaBot_2._0_;
using WpfElmaBot_2._0_.Models;
using WpfElmaBot_2._0_.Models.EntityPack;
using WpfElmaBot_2._0_.Service.Commands;
using WpfElmaBot_2._0_.ViewModels;
using static System.Net.WebRequestMethods;

namespace WpfElmaBot.Service
{
    public class ELMA
    {
        public static RestClient RestClient { get; set; }

        private static ELMA elma;
        public static string FullURL { get; set; }
        public static string FullURLpublic { get; set; } 
        public static string appToken { get; set; } 
        public static string TypeUid { get; set; } 
        public static string login { get; set; } 
        public static string password { get; set; }

        public static string TypeUidTaskBase;

        private ELMA()
        {           
            RestClient = new RestClient();
        }  
        
        public static ELMA getElma()
        {
            if (elma == null)
                elma = new ELMA();
            return elma;
        }

        public async Task<Auth> UpdateToken<Auth>(string authtoken)
        {
            var data = await GetRequest<Auth>($"{FullURL}Authorization/CheckToken?token={authtoken}");
            return data;
            
        }
        private async Task<T> GetRequest<T>(string path, string authToken = null, string sessionToken =null)
        {                          
            var request = new RestRequest(path);               
            AddHeadersELMA(request, authToken, sessionToken);
            var test = RestClient.BuildUri(request);           
            var response = await RestClient.ExecuteGetAsync(request);
            if (response.ErrorException != null || response.Content.Contains("Запуск сервера"))
            {
                if (response.Content != null)
                {
                    throw new WebException(response.Content);
                }
                else
                {
                    throw new WebException(response.ErrorMessage);
                }
            }
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));         
        }     
        public async Task<T> PostRequest<T>(string path, string body, string authToken = null , string sessionToken = null )
        {
            var request = new RestRequest($"{FullURL}" + path);
            AddHeadersELMA(request, authToken, sessionToken);
            request.AddStringBody(body, DataFormat.Json);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.ExecutePostAsync(request);
            if (response.ErrorException != null || response.Content.Contains("Запуск сервера"))
            {
                if (response.Content != null)
                {
                    throw new WebException(response.Content);
                }
                else
                {
                    throw new WebException(response.ErrorMessage);
                }
            }
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
            
        }

        public async Task<string> PostRequestNotDeserialze(string path, string body, string authToken = null, string sessionToken = null)
        {
            var request = new RestRequest($"{FullURL}" + path);
            AddHeadersELMA(request, authToken, sessionToken);
            request.AddStringBody(body, DataFormat.Json);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.ExecutePostAsync(request);
            if (response.ErrorException != null || response.Content.Contains("Запуск сервера"))
            {
                if(response.Content!=null)
                {
                    throw new WebException(response.Content);
                }
                else
                {
                    throw new WebException(response.ErrorMessage);
                }
               
            }
            return response.Content;
        }

        public async Task<List<T>> GetEntity<T>(string path, string authToken , string sessionToken ) where T : Entity
        {  
            var request = new RestRequest($"{FullURL}" + path);
            AddHeadersELMA(request, authToken, sessionToken);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.ExecuteGetAsync(request);
            if (response.ErrorException != null)
            {
                if (response.Content != null)
                {
                    throw new WebException(response.Content);
                }
                else
                {
                    throw new WebException(response.ErrorMessage);
                }
            }
            return JsonConvert.DeserializeObject<List<T>>(response.Content.Trim(new char[] { '\uFEFF' }));                      
        }
        public async Task<T> GetEntityById<T>(string typeUId, long entityId, string authToken, string sessionToken ) where T : Entity
        {
            
            var obj = await GetRequest<T>($"{FullURL}Entity/Load?type={typeUId}&id={entityId}",authToken,sessionToken);
            return obj;
        }

        public async Task<T> GetUnreadMessage<T>(string authtoken, string sessionToken) where T : MessegesOtvet
        {
            string AFTER = "";
            string BEFORE = "";
            var LIMIT = "200";
            var ONLYUNREAD = "true";
            //var updateToken = await UpdateToken<Auth>(authtoken);
            var obj = await GetRequest<T>($"{FullURLpublic}EleWise.ELMA.Messages/MessageFeed/Posts/Feed?after={AFTER}&before={BEFORE}&limit={LIMIT}&onlyUnread={ONLYUNREAD}", authtoken, sessionToken);
            return obj;


        }
        public  async Task<T> GetAllMessage<T>(string authtoken, string sessionToken) where T : MessegesOtvet
        {

            string AFTER = "";
            string BEFORE = "";
            var LIMIT = "100000";
            var obj = await GetRequest<T>($"{FullURLpublic}EleWise.ELMA.Messages/MessageFeed/Posts/Feed?after={AFTER}&before={BEFORE}&limit={LIMIT}", authtoken, sessionToken);
            return obj;


        }

        private static void AddHeadersELMA(RestRequest request, string authToken = null, string sessionToken = null)
        {
            request.AddHeader("ApplicationToken", appToken);
            request.AddHeader("Accept", "application/json; charset=utf-8");
            if (!string.IsNullOrEmpty(authToken))
            {
                request.AddHeader("WebData-Version", "2.0");
                request.AddHeader("AuthToken", authToken);
            }
            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                request.AddHeader("SessionToken", sessionToken);
            }


        }

        public async Task AuthorizationUser(Auth authorization, long chatid, string login)
        {
                var eqlQuery = $"IdUserElma={authorization.CurrentUserId}";  
                var limit = "1";
                var offset = "0";
                var sort = "";
                var filterProviderUid = "";
                var filterProviderData = "";
                var filter = "";
            try
            {
                    var entity = await GetEntity<EntityMargin>($"Entity/Query?type={TypeUid}&q={eqlQuery}&limit={limit}&offset={offset}&sort={sort}&filterProviderUid={filterProviderUid}&filterProviderData={filterProviderData}&filter={filter}", authorization.AuthToken, authorization.SessionToken);
                    string jsonBody="";                  
                    if (entity.Count==0)
                    {
                        var body = new EntityMargin()
                        {
                            IdUserElma = authorization.CurrentUserId,
                            IdTelegram = Convert.ToString(chatid),
                            AuthToken = authorization.AuthToken,
                            SessionToken = authorization.SessionToken,
                            AuthorizationUser = "true",
                            Login = login,
                            TimeMessage = DateTime.UtcNow
                        };
                        jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                    }
                    else
                    {

                        if (entity[0].IdTelegram != Convert.ToString(chatid))
                        {
                            OptionTelegramMessage message = new OptionTelegramMessage();
                            List<string> ids = new List<string>() { CommandRoute.AUTHMENU };
                            message.MenuReplyKeyboardMarkup = MenuGenerator.ReplyKeyboard(1, ids, "");

                            await new CommandRoute().MessageCommand.Send(TelegramCore.getTelegramCore().bot, chatId: Convert.ToInt64(entity[0].IdTelegram), msg: "Выполенен вход с другого аккаунта телеграм", TelegramCore.cancellation, message);

                            TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[0].IdTelegram)).Value.AuthToken = null;
                            TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[0].IdTelegram)).Value.SessionToken = null;
                            TelegramCore.getTelegramCore().bot.GetCacheData(Convert.ToInt64(entity[0].IdTelegram)).Value.StatusAuth = false;
                        }


                        entity[0].AuthToken = authorization.AuthToken;
                        entity[0].SessionToken = authorization.SessionToken;
                        entity[0].IdTelegram = Convert.ToString(chatid);
                        entity[0].AuthorizationUser = "true";
                        jsonBody = System.Text.Json.JsonSerializer.Serialize(entity[0]);

                    }                                                          
                    var entityPost = await PostRequestNotDeserialze(entity.Count > 0 ? $"Entity/Update/{ELMA.TypeUid}/{entity[0].Id}"  : $"Entity/Insert/{TypeUid}", jsonBody, authorization.AuthToken, authorization.SessionToken);

                    

                }
                catch (Exception ex)
                {

                    if (ex.Message.Contains("Error converting value"))
                    {
                        MainWindowViewModel.Log.Error("Успешное добавление/обновление записи в справочник | " + ex);

                    }
                    else
                    {
                        MainWindowViewModel.Log.Error("Ошибка добавления/обновления записи в справочник | " + ex);
                        TelegramCore.getTelegramCore().InvokeCommonError("Ошибка добавления/обвноления записи в справочник", TelegramCore.TelegramEvents.Password);
                    }
                    
                   

                }
           
        }
        
        public async Task<T> GetCountunread<T>(string authToken,string sessionToken)
        {
            var obj = await GetRequest<T>($"{FullURLpublic}EleWise.ELMA.Messages/MessageFeed/Posts/Feed/UnreadCount", authToken, sessionToken);
            return obj;
        }

        public async Task <Auth> UpdateTokenAndEntity<T>(long chatId,string login, string authToken)
        {
           
                var update = await getElma().UpdateToken<Auth>(authToken);
                getElma().AuthorizationUser(update, chatId, login);
                return update;
                       
        }

        
    }
}
