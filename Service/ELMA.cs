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

namespace WpfElmaBot.Service
{
    internal class ELMA
    {
        private static RestClient RestClient { get; set; }
        //public static ELMA elma { get; set; }
        //private static string authToken { get; set; } 
        //private static string sessionToken { get; set; } 
        public static string FullURL { get; set; } = "http://127.0.0.1:8000/API/REST/";
        private static string FullURLpublic { get; set; } = "http://127.0.0.1:8000/PublicAPI/REST/";
        private static string appToken { get; set; } = "65605731B74E8CE81B9F8F2B799B0C00E0D2FFD8ACFDA1E01FFC4A8C4409A8FF853679B82D47E0FCD7E3E8480FA41917A94982C095819F291D788F041D880CC2";
        public static string TypeUid { get; set; } = "c5c12f67-3f57-45c2-aa70-02dfded87f77";
        public static string login { get; set; } = "Sprav";
        public static string password { get; set; } = "0000";
        

        public ELMA()
        {
           RestClient = new RestClient();
        }
      

       

        public async Task<Auth> UpdateToken<Auth>(string authtoken=null)
        {          
                var data = await GetRequest<Auth>($"{FullURL}Authorization/CheckToken?token={authtoken}");              
                return data;
            
        }
        private async Task<T> GetRequest<T>(string path, string authToken=null, string sessionToken=null)
        {
            var request = new RestRequest(path);
            AddHeadersELMA(request,authToken,sessionToken);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.GetAsync(request);                 
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
        }

        public async Task<T> PostRequest<T>(string path, string body, string authToken = null, string sessionToken = null)
        {
            var request = new RestRequest($"{FullURL}" + path);         
            AddHeadersELMA(request,authToken,sessionToken);
            request.AddStringBody(body, DataFormat.Json);         
            var test = RestClient.BuildUri(request);
            var response = await RestClient.PostAsync(request);
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
        }

        public async Task<T> PostRequest1<T>(string path, Entity body, string authToken = null, string sessionToken = null)
        {
                var request = new RestRequest($"{FullURL}" + path);
                AddHeadersELMA(request, authToken, sessionToken);
                request.AddBody(body, "application/json");
                var test = RestClient.BuildUri(request);
                var response = await RestClient.PostAsync(request);
                return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));        
        }

        public async Task<List<T>> GetEntity<T>(string typeUId, string authToken = null, string sessionToken = null) where T : Entity
        {
            //await UpdateToken<Auth>(authToken);
            var request = new RestRequest($"{FullURL}Entity/Query?type={typeUId}");
            AddHeadersELMA(request,authToken,sessionToken);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.GetAsync(request);
            return JsonConvert.DeserializeObject<List<T>>(response.Content.Trim(new char[] { '\uFEFF' }));

        }
       
        public async Task<T> GetUnreadMessage<T>(string authtoken, string sessionToken) where T : MessegesOtvet
        {
            string AFTER = "";
            string BEFORE = "";
            var LIMIT = "20";
            var ONLYUNREAD = "true";
            //var updateToken = await UpdateToken<Auth>(authtoken);
            var obj = await GetRequest<T>($"{FullURLpublic}EleWise.ELMA.Messages/MessageFeed/Posts/Feed?after={AFTER}&before={BEFORE}&limit={LIMIT}&onlyUnread={ONLYUNREAD}", authtoken,sessionToken);
            return obj;


        }

        private static void AddHeadersELMA(RestRequest request,string authToken=null,string sessionToken = null)
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
    }
}
