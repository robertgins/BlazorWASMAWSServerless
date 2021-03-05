using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BalsamicSolutions.ApiCommon;
using BalsamicSolutions.ApiCommon.Configuration;

using System.Net.Http.Json;

namespace BalsamicSolutions.BlazorClient.Services
{
    /// <summary>
    /// local implementation of stuff and nonsense 
    /// </summary>
    public class StuffAndNonsenseService : IStuffAndNonsense
    {
        private HttpClient Http { get; set; }
        private Settings Settings { get; set; }
        public StuffAndNonsenseService(IHttpClientFactory httpFactory, Settings setttings)
        {
            Http = httpFactory.CreateClient("APIClient");
            Settings = setttings;
        }

        /// <summary>
        /// option one, build the url and call it
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public async Task<string> RandomSentance(int minLength, int maxLength)
        {
            string serverUrl = $"{Settings.AuthorizedApiUrl}/StuffAndNonsense/RandomSentance?minLength={minLength}&maxLength={maxLength}";
            return await Http.GetStringAsync(serverUrl);
        }

        /// <summary>
        /// or POST a JSON object
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<string[]> RandomSentances(StuffAndNonsenseOptions options)
        {
            string serverUrl = $"{Settings.AuthorizedApiUrl}/StuffAndNonsense/RandomSentances";
            HttpResponseMessage response = await Http.PostAsJsonAsync(serverUrl, options);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<string[]>();
            }
            throw new Exception($"The service returned with status {response.StatusCode}");
        }
    }
}
