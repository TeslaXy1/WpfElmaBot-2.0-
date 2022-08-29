using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfElmaBot.Service
{
    internal class ELMA
    {

        //public async Task UpdateToken()
        //{
        //    if (!string.IsNullOrWhiteSpace(userToken))
        //    {
        //        var data = await GetRequest<Auth>($"{FullURL}Authorization/CheckToken?token={userToken}");
        //        userToken = data.AuthToken;
        //        sessionToken = data.SessionToken;
        //    }
        //    else
        //    {
        //        await SignIn();
        //    }

        //}

        //private static async Task<T> GetRequest<T>(string path)
        //{
        //    var request = new RestRequest(path);
        //    AddHeadersELMA(request);
        //    var test = RestClient.BuildUri(request);
        //    var response = await RestClient.GetAsync(request);
        //    return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
        //}

        //private static async Task<T> PostRequest<T>(string path, string body)
        //{
        //    var request = new RestRequest(path);
        //    AddHeadersELMA(request);
        //    request.AddStringBody(body, DataFormat.Json);
        //    var test = RestClient.BuildUri(request);
        //    var response = await RestClient.PostAsync(request);
        //    return JsonConvert.DeserializeObject<T>(response.Content.Trim(new char[] { '\uFEFF' }));
        //}

        //public async Task<T> GetEntityById<T>(string typeUId, long entityId) where T : EntityELMA
        //{
        //    await UpdateToken();
        //    var obj = await GetRequest<T>($"{FullURL}Entity/Load?type={typeUId}&id={entityId}");
        //    return obj;
        //}

        //private static void AddHeadersELMA(RestRequest request)
        //{
        //    request.AddHeader("ApplicationToken", appToken);
        //    request.AddHeader("Accept", "application/json; charset=utf-8");
        //    if (!string.IsNullOrEmpty(userToken))
        //    {
        //        request.AddHeader("WebData-Version", "2.0");
        //        request.AddHeader("AuthToken", userToken);
        //    }
        //    if (!string.IsNullOrWhiteSpace(sessionToken))
        //    {
        //        request.AddHeader("SessionToken", sessionToken);
        //    }
        //}
    }
}
