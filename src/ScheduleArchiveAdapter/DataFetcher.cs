using AdapterUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace ScheduleArchiveAdapter
{
    public class DataFetcher
    {
        public ConfigurationManager Config_ { get; set; } = new ConfigurationManager();
        public async Task<string> GetIdentityServerToken()
        {
            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = Config_.IdentityServerHost,
                Policy =
                {
                    RequireHttps = false
                }
            });
            if (disco.IsError)
            {
                // Console.WriteLine(disco.Error);
                return null;
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = Config_.ClientId,
                ClientSecret = Config_.ClientPassword,

                Scope = "scada_archive"
            });

            if (tokenResponse.IsError)
            {
                // Console.WriteLine(tokenResponse.Error);
                return null;
            }
            // Console.WriteLine(tokenResponse.Json);
            return tokenResponse.AccessToken;
        }

        public async Task FetchAndFlushData(AdapterParams prms)
        {
            // get measurement id from command line arguments
            string[] measIdFrags = prms.MeasId.Split('|');
            // check if we have a valid measurement Id
            if (measIdFrags.Length < 2)
            {
                ConsoleUtils.FlushChunks("");
                return;
            }
            // get the start and end times
            DateTime startTime = prms.FromTime;
            DateTime endTime = prms.ToTime;

            List<double> measData;

            // get token
            string token = await GetIdentityServerToken();

            // call api            
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(token);

            string outStr = "";
            // http://portal.wrldc.in/dashboard/api/wbesArchive/MOUDA/Total/2020-01-16/2020-01-17
            var response = await apiClient.GetAsync($"{Config_.DataHost}/api/wbesArchive/{measIdFrags[0]}/{measIdFrags[1]}/{startTime.ToString("yyyy-MM-dd")}/{endTime.ToString("yyyy-MM-dd")}");
            if (!response.IsSuccessStatusCode)
            {
                //Console.WriteLine(response.StatusCode);
            }
            else
            {
                // we get the result in the form of ts1,val1,ts2,val2,...
                string content = await response.Content.ReadAsStringAsync();
                if (prms.IncludeQuality)
                {
                    measData = JsonConvert.DeserializeObject<List<double>>(content);
                    outStr = "";
                    // add good quality in the middle of each point
                    for (int pntIter = 0; pntIter < measData.Count; pntIter += 2)
                    {
                        outStr += $"{((pntIter > 0) ? "," : "")}{measData[pntIter]},{measData[pntIter + 1]}";
                    }
                }
                else
                {
                    if (content.Length > 4)
                    {
                        outStr = content.Remove(content.Length - 1, 1).Substring(1);
                    }
                }
            }
            ConsoleUtils.FlushChunks(outStr);
        }

        public async Task<List<string>> FetchUtilsList()
        {
            List<string> measList = new List<string>();
            // get token
            string token = await GetIdentityServerToken();

            // call api            
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(token);

            var response = await apiClient.GetAsync($"{Config_.DataHost}/api/wbesArchive/getUtilities");
            if (!response.IsSuccessStatusCode)
            {
                //Console.WriteLine(response.StatusCode);
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                measList = JsonConvert.DeserializeObject<List<string>>(content);
            }

            return measList;
        }

        public async Task<List<SchType>> FetchSchTypes()
        {
            List<SchType> measList = new List<SchType>();
            // get token
            string token = await GetIdentityServerToken();

            // call api            
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(token);

            var response = await apiClient.GetAsync($"{Config_.DataHost}/api/wbesArchive/getSchTypes");
            if (!response.IsSuccessStatusCode)
            {
                //Console.WriteLine(response.StatusCode);
            }
            else
            {
                string content = await response.Content.ReadAsStringAsync();
                measList = JsonConvert.DeserializeObject<List<SchType>>(content);
            }

            return measList;
        }
    }
}