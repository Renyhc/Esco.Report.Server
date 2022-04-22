using esco.report.server.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace esco.report.server
{
    class ReportServer
    {
        private readonly string _api;
        private readonly string _env;
        private readonly HttpServices _httpClient;

        public ReportServer(string user, string password, string api, string version, string enviroment)
        {
            _api = api;
            _env = enviroment;
            _httpClient = new HttpServices(user, password, api, version);
        }

        private async Task<String> GetEmbedUrl(LocalReport report, string action, string param, bool max = false, string format = "")
        {
            string url = _api;
            param = (param == null) ? String.Empty :
                (param.Contains("=")) ? "&" + param : "&parameter=" + param;
            if (report.Type == Config.typePowerBI)
            {
                var rs = max ? Config.maxEmbed : Config.embed;
                url = _api + Config.pathPowerBI + report.Path + param + rs;
            }
            if (report.Type == Config.typeReport)
            {
                var abs = await _httpClient.GetReportServerAbsoluteUrl();
                url = abs + "?" + report.Path + action + format + param;
            }            
            return url;
        }
        

        private async Task<HttpResponseMessage> FindReportId(string name, string group = null)
        {
            //Find by Name Report
            string id = String.Empty;
            try
            { 
                HttpResponseMessage response = _httpClient.GetAsync(Config.Local.api_catalogitems);
                string responseData = await _httpClient.GetResponseData(response);

                LocalReportsList list = JsonConvert.DeserializeObject<LocalReportsList>(responseData);
                if (list != null)
                {
                    LocalReport report = (group != null)? 
                        list.value.Find(x => x.Name == name && x.Path == "/" + group + "/" + x.Name) : 
                        list.value.Find(x => x.Name == name);
                    if (report != null)
                    {
                        var url = string.Format(Config.Local.api_catalogitemsId, report.Id);
                        return _httpClient.GetAsync(url);
                    }
                    else
                    {
                        throw new Exception(Messages.ReportNotFound);
                    }
                }
                else
                {
                    throw new Exception(Messages.AnyReports);
                }
            }
            catch { throw; }            
        }

        private async Task<LocalReport> GetLocalReport(string reportId, string group)
        {
            try
            {
                reportId = (_env != null && _env != "PROD") ? 
                    reportId + _env : reportId;
                string url = string.Format(Config.Local.api_catalogitemsId, reportId);
                HttpResponseMessage response = _httpClient.GetAsync(url);
                LocalReport report = new LocalReport();

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    response = await FindReportId(reportId, group);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception(Messages.ReportNotFound);
                    }
                }
                if (response.IsSuccessStatusCode)
                {
                    string responseData = await _httpClient.GetResponseData(response);
                    report = JsonConvert.DeserializeObject<LocalReport>(responseData);                    
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
                return report;
            }
            catch (Exception e) { throw e; }
        }

        public async Task<ReportEmbed> GenerateEmbed(string reportId, string group, string param, bool max)
        {
            try
            {
                LocalReport report = await GetLocalReport(reportId, group);                
                if (!report.HasParameters)
                {
                    param = null;
                }
                
                return new ReportEmbed()
                {
                    Id = report.Id,
                    Name = report.Name,
                    Type = report.Type,
                    EmbedUrl = await GetEmbedUrl(report, Config.render, param, max),
                    Group = report.ParentFolderId
                };
            }
            catch (Exception e) { throw e; }
        }

        public async Task<ExportedFile> ExportReport(string reportId, string group, string format, string param)
        {
            try
            {
                LocalReport report = await GetLocalReport(reportId, group);
                if (report.Type == Config.typePowerBI)
                {
                    throw new Exception(Messages.ExportNotPaginated);
                }
                if (report.HasParameters && param == null)
                {
                    throw new Exception(Messages.NoParameters);
                }
                if (!report.HasParameters)
                {
                    param = null;
                }
                var embedUrl = await GetEmbedUrl(report, Config.export, param, false, format ?? "PDF");
                return new ExportedFile
                {
                    FileStream = await GetFileExport(embedUrl),
                    ReportName = report.Name,
                    FileExtension = ExportFormat.extension[format]
                };
            }
            catch { throw; }
        }

        private async Task<Stream> GetFileExport(string url)
        {
            try
            {
                Stream stream = Stream.Null;
                HttpResponseMessage response = _httpClient.GetAsync(url);              
                if (response.IsSuccessStatusCode)
                {
                    stream = await response.Content.ReadAsStreamAsync();
                }
                else 
                {                    
                    var error = (response.Content != null)? 
                        await _httpClient.GetExportError(response): response.ReasonPhrase;
                    throw new Exception(Messages.ErrorExport + error);
                }
                return stream;
            }
            catch { throw; }
        }       

    }
}
