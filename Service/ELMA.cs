using Newtonsoft.Json;
using RestSharp;
using System;
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
        private static string authToken { get; set; } 
        private static string sessionToken { get; set; }
        private static string FullURL { get; set; } = "http://127.0.0.1:8000/API/REST/";
        private static string appToken { get; set; } = "65605731B74E8CE81B9F8F2B799B0C00E0D2FFD8ACFDA1E01FFC4A8C4409A8FF853679B82D47E0FCD7E3E8480FA41917A94982C095819F291D788F041D880CC2";
        private static string login { get; set; } 
        private static string password { get; set; } 
        

        public ELMA()
        {
           RestClient = new RestClient();
        }

       

        public async Task UpdateToken(string authtoken=null)
        {
            if (!string.IsNullOrWhiteSpace(authtoken))
            {
                var data = await GetRequest<Auth>($"{FullURL}Authorization/CheckToken?token={authtoken}");
                authToken = data.AuthToken;
                sessionToken = data.SessionToken;
            }
            else
            {
                
                PostRequest<Auth>($"{FullURL}Authorization/LoginWith?username={login}", password);
               //TODO вызвать команду авторизации
            }

        }
        private static async Task SignIn(string login, string password) //доделать
        {
           
            var request = new RestRequest($"{FullURL}Authorization/LoginWith?username={login}");
            AddHeadersELMA(request);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.GetAsync(request);
            return  ;

        }

        private static async Task<T> GetRequest<T>(string path)
        {
            var request = new RestRequest(path);
            AddHeadersELMA(request);
            var test = RestClient.BuildUri(request);
            var response = await RestClient.GetAsync(request);
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
        }

        public async Task<T> PostRequest<T>(string path, string body)
        {
            var request = new RestRequest($"{FullURL}" + path);
            AddHeadersELMA(request);
            request.AddStringBody(body, DataFormat.Json);         
            var test = RestClient.BuildUri(request);
            var response = await RestClient.PostAsync(request);
            return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
        }

        public async Task<T> GetEntity<T>(string typeUId) where T : Entity
        {
            await UpdateToken();
            var obj = await GetRequest<T>($"{FullURL}Entity/Query?type={typeUId}");
            return obj;
        }

        private static void AddHeadersELMA(RestRequest request)
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
