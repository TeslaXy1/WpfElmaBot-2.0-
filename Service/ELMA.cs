using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using WpfElmaBot.Models;
using WpfElmaBot.Service.Commands;
using WpfElmaBot_2._0_;
using WpfElmaBot_2._0_.Models.EntityPack;
using WpfElmaBot_2._0_.ViewModels;

namespace WpfElmaBot.Service
{
    internal class ELMA
    {
        private static RestClient RestClient { get; set; }
        private static ELMA elma;
        public static string FullURL { get; set; }
        public static string FullURLpublic { get; set; } 
        public static string appToken { get; set; } 
        public static string TypeUid { get; set; } 
        public static string login { get; set; } 
        public static string password { get; set; } 


        public ELMA()
        {
            RestClient = new RestClient();
        }

        
        public static ELMA getInstance()
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
            var response = await RestClient.GetAsync(request);
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
        }

        public async Task<T> PostRequest<T>(string path, string body, string authToken = null , string sessionToken = null )
        {

            var request = new RestRequest($"{FullURL}" + path);
            AddHeadersELMA(request, authToken, sessionToken);
            request.AddStringBody(body, DataFormat.Json);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.PostAsync(request);
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));


        }      

        public async Task<List<T>> GetEntity<T>(string path, string authToken , string sessionToken ) where T : Entity
        {
            //await UpdateToken<Auth>(authToken
            var request = new RestRequest($"{FullURL}" + path);
            AddHeadersELMA(request, authToken, sessionToken);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.GetAsync(request);
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
        public async void AuthorizationUser(Auth authorization, long chatid,string login)
        {
            var eqlQuery = $"IdTelegram={chatid}";
            var limit = "1";
            var offset = "0";
            var sort = ""; 
            var filterProviderUid = "";
            var filterProviderData = "";
            var filter = "";

            var message = await GetUnreadMessage<MessegesOtvet>(authorization.AuthToken, authorization.SessionToken);
            var entity = await GetEntity<Entity>($"Entity/Query?type={TypeUid}&q={eqlQuery}&limit={limit}&offset={offset}&sort={sort}&filterProviderUid={filterProviderUid}&filterProviderData={filterProviderData}&filter={filter}", authorization.AuthToken, authorization.SessionToken);

            if (entity.Count==0)
            {
                try
                {
                    var body = new EntityMargin()
                    {
                        IdUserElma = authorization.CurrentUserId,
                        IdTelegram = Convert.ToString(chatid),
                        AuthToken = authorization.AuthToken,
                        SessionToken = authorization.SessionToken,
                        AuthorizationUser = "true",
                        Login = login,
                        IdLastSms = Convert.ToString(message.Data[0].Id)
                    };
                    string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                    var entityPost = await PostRequest<Entity>($"Entity/Insert/{TypeUid}", jsonBody, authorization.AuthToken, authorization.SessionToken);
                }
                catch (Exception ex)
                {
                    MainWindowViewModel.Log.Error("Ошибка добавления записи в справочник | " + ex);
               
                }
                
            }
            else
            {
                try
                {
                    var body = new EntityMargin()
                    {
                        IdUserElma = authorization.CurrentUserId,
                        IdTelegram = Convert.ToString(chatid),
                        AuthToken = authorization.AuthToken,
                        SessionToken = authorization.SessionToken,
                        AuthorizationUser = "true",
                        Login = login,
                        IdLastSms = Convert.ToString(message.Data[0].Id)
                    };
                    string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                    var entityPost = await PostRequest<Entity>($"Entity/Update/{ELMA.TypeUid}/{entity[0].Id}", jsonBody, authorization.AuthToken, authorization.SessionToken);
                }
                catch (Exception ex)
                {
                    MainWindowViewModel.Log.Error("Ошибка обновления записи в справочник | " + ex);

                    
                }

            }
        }
        public async Task<T> GetCountunread<T>(string authToken,string sessionToken)
        {
            var obj = await GetRequest<T>($"{FullURLpublic}EleWise.ELMA.Messages/MessageFeed/Posts/Feed/UnreadCount", authToken, sessionToken);
            return obj;
        }
    }
}
