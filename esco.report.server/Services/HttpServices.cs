using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using esco.report.server.Models;
using Microsoft.Rest;

namespace esco.report.server
{
    class HttpServices
    {
        private readonly string _client;
        private readonly string _secret;

        private readonly HttpClient _httpClient;

        public HttpServices(string client, string secret)
        {
            _client = client;
            _secret = secret;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(Config.apiPbi)
            };
        }

        public HttpServices(string user, string password, string api, string version)
        {
            string domain = (user.Contains("\\")) ? user.Split('\\')[0] : null;
            user = (user.Contains("\\")) ? user.Split('\\')[1] : user;

            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password, domain),
                PreAuthenticate = true,
            };
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(api + "api/v" + version + "/")
            };
        }

        public void SetToken(string token)
        {            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HttpResponseMessage GetAsync(string url)
        {
            try
            {
                return _httpClient.GetAsync(url).Result;
            }
            catch 
            { 
                throw; 
            }
        }

        public async Task<string> GetResponseData(HttpResponseMessage response)
        {

            if (response.IsSuccessStatusCode)
            {
                Stream stream = await response.Content.ReadAsStreamAsync();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            else
            {
                var error = (response.Content != null) ?
                        await GetExportError(response) : response.ReasonPhrase;
                throw new Exception(Messages.ErrorExport + error);
            }
        }

        public async Task<string> GetExportError(HttpResponseMessage response)
        {
            try
            {
                string error = response.ReasonPhrase;

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return error;
                }
                Stream _stream = await response.Content.ReadAsStreamAsync();
                StreamReader reader = new StreamReader(_stream, Encoding.UTF8);
                error = reader.ReadToEnd();

                String[] _li = { "<li>", "</li>" };
                String[] _array = error.Split(_li, 2, StringSplitOptions.RemoveEmptyEntries);

                if (_array.Length > 1)
                {
                    String[] _a = { "", "<a" };
                    String[] _error = _array[1].Split(_a, 2, StringSplitOptions.RemoveEmptyEntries);
                    error = (_error.Length > 0) ? _error[0] : error;
                }
                return error;
            }
            catch { throw; }
        }

        public async Task<String> GetReportServerAbsoluteUrl()
        {
            try
            {
                HttpResponseMessage response = _httpClient.GetAsync(Config.Local.api_system).Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await GetResponseData(response);
                    PBISystem sys = JsonConvert.DeserializeObject<PBISystem>(responseData);
                    return sys.ReportServerAbsoluteUrl;
                }
                else
                {
                    throw new Exception(Messages.ApiError);
                }
            }
            catch { throw; }
        }

        public async Task<string> ExportToFileAsync(Guid reportId, string format)
        {

            string url = string.Format(Config.Cloud.api_export, reportId.ToString());
            try
            {
                var json = JsonConvert.SerializeObject(new JsonFormat() { format = ExportFormat.format[format].ToString() });
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(url, data);
                string responseData = await GetResponseData(response);
                ReportExported exported = JsonConvert.DeserializeObject<ReportExported>(responseData);
                return exported.id;
            }
            catch { throw; }
        }

        public async Task<Stream> GetExportedFile(Guid reportId, string exportId)
        {
            string url = string.Format(Config.Cloud.api_exportfile, reportId.ToString(), exportId);
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                return await response.Content.ReadAsStreamAsync();
            }
            catch (HttpOperationException ex)
            {
                throw new Exception(Messages.ErrorExport + ": " + ex.Message);
            }
        }

        public async Task<CloudReport> GetReport(Guid reportId)
        {
            string url = string.Format(Config.Cloud.api_reportsId, reportId.ToString());
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                string responseData = await GetResponseData(response);
                return JsonConvert.DeserializeObject<CloudReport>(responseData); 
            }
            catch { throw; }
        }

        public async Task<AccessToken> GetAccessToken()
        {
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", Config.grant_type),
                new KeyValuePair<string, string>("resource", Config.resource),
                new KeyValuePair<string, string>("client_id", _client),
                new KeyValuePair<string, string>("client_secret", _secret)
            };
            try
            {                
                HttpContent content = new FormUrlEncodedContent(vals);
                HttpResponseMessage response = _httpClient.PostAsync(Config.tokenUriBase, content).Result;

                string responseData = await GetResponseData(response);
                return JsonConvert.DeserializeObject<AccessToken>(responseData);
            }
            catch { throw; }
        }
        public async Task<AccessToken> Authorization(string username, string password)
        {
            List<KeyValuePair<string, string>> vals = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", Config.grant_type_pass),
                new KeyValuePair<string, string>("resource", Config.resource),
                new KeyValuePair<string, string>("client_id", _client),
                new KeyValuePair<string, string>("client_secret", _secret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("paswword",password),
            };
            try
            {
                HttpContent content = new FormUrlEncodedContent(vals);
                HttpResponseMessage response = _httpClient.PostAsync(Config.tokenUriBase, content).Result;

                string responseData = await GetResponseData(response);
                return JsonConvert.DeserializeObject<AccessToken>(responseData);
            }
            catch { throw; }
        }
    }
}
