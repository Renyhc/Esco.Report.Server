using esco.report.server.Models;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace esco.report.server
{
    class PowerBI
    {

        #region Authenticate & Initalization

        private PowerBIClient pbiClient;
        private readonly HttpServices _httpClient;

        public PowerBI(string client, string secret)
        {
            _httpClient = new HttpServices(client, secret);
        }

        public async Task Authenticate(string accessToken = null)
        {
            try
            {
                AccessToken _token = await _httpClient.GetAccessToken();
                accessToken = (accessToken != null) ? accessToken : _token.access_token;
                TokenCredentials _tokenCredentials = new TokenCredentials(accessToken, "Bearer");

                pbiClient = new PowerBIClient(new Uri(Config.apiBase), _tokenCredentials);
                _httpClient.SetToken(accessToken);
            }
            catch { throw; }
        }

        public async Task<ReportEmbed> GenerateEmbed(string reportId, string groupId)
        {
            try
            {
                Report _report = await pbiClient.Reports.GetReportAsync(Guid.Parse(reportId));
                return new ReportEmbed()
                {
                    Id = _report.Id.ToString(),
                    Name = _report.Name,
                    Type = _report.ReportType,
                    Group = groupId,
                    EmbedUrl = _report.EmbedUrl + Config.autoAuth,
                    WebUrl = _report.WebUrl,
                    Dataset = _report.DatasetId
                };
            }
            catch { throw; }
        }

        public async Task<ExportedFile> ExportReport(Guid reportId, Guid groupId, string format, int timeOutInMinutes, CancellationToken token)
        {
            try
            {
                Report _report = await pbiClient.Reports.GetReportAsync(reportId);
                return (_report.ReportType == Config.paginated) ?
                    await ExportReportPowerBI(reportId, groupId, format, timeOutInMinutes, token) :
                    throw new Exception(Messages.ExportNotPaginated);                  
            }
            catch { throw; }
        }

        #endregion

        #region ReportPaginated
        public async Task<ExportedFile> ExportReportPaginated(Guid reportId, Guid groupId, string format, int timeOutInMinutes, CancellationToken token)
        {
            try
            {
                var exportId = await _httpClient.ExportToFileAsync(reportId, format);

                var export = await PollExportRequest(reportId, groupId, exportId, timeOutInMinutes, token);
                if (export == null || export.Status != ExportState.Succeeded)
                {
                    throw new Exception(Messages.ErrorExport);
                }
                return new ExportedFile
                {
                    FileStream = await _httpClient.GetExportedFile(reportId, exportId),
                    ReportName = export.ReportName,
                    FileExtension = export.ResourceFileExtension,
                };
            }
            catch { throw; }
        }       

        public async Task<Export> PollExportRequest(Guid reportId, Guid groupId, string exportId, int timeOutInMinutes, CancellationToken token)
        {
            Progress progressWin = new Progress();
            progressWin.Show();

            DateTime startTime = DateTime.UtcNow;
            const int secToMillisec = 100;
            int counter = 0;
            Export exportStatus;
            do
            {
                if (DateTime.UtcNow.Subtract(startTime).TotalMinutes > timeOutInMinutes || token.IsCancellationRequested)
                {
                    throw new Exception(Messages.ExportTimeout);
                }

                exportStatus = pbiClient.Reports.GetExportToFileStatusInGroup(groupId, reportId, exportId);

                string percent = (exportStatus.PercentComplete == 0) ? counter.ToString() : exportStatus.PercentComplete.ToString();
                progressWin.SetProgress(percent);
                counter = (counter < 100) ? (counter + 10) : (counter);

                if (exportStatus.Status == ExportState.Running || exportStatus.Status == ExportState.NotStarted)
                {
                    Task.Delay(secToMillisec).Wait();
                }
            }
            // While not in a terminal state, keep polling
            while (exportStatus.Status != ExportState.Succeeded && exportStatus.Status != ExportState.Failed);
            progressWin.Close();
            return exportStatus;
        }
        #endregion        

        #region ReportPowerBI
        private async Task<ExportedFile> ExportReportPowerBI(
            Guid reportId,
            Guid groupId,
            string format,
            int timeOutInMinutes,
            CancellationToken token,
            IList<string> pageNames = null,
            string urlFilter = null)
        {
            const int c_secToMillisec = 10;
            try
            {
                Export export = null;
                var exportId = await PostExportRequestPBI(reportId, groupId, format, pageNames, urlFilter);
                do
                {                  
                    var httpMessage = await pbiClient.Reports.GetExportToFileStatusInGroupWithHttpMessagesAsync(groupId, reportId, exportId);

                    export = httpMessage.Body;
                    if (export == null)
                    {
                        throw new Exception("Report null");
                    }
                    if (export.Status == ExportState.Failed)
                    {
                        var retryAfter = httpMessage.Response.Headers.RetryAfter;
                        if (retryAfter == null)
                        {
                            throw new Exception("Report null");
                        }

                        var retryAfterInSec = retryAfter.Delta.Value.Seconds;
                        Task.Delay(retryAfterInSec * c_secToMillisec).Wait();
                    }
                }
                while (export.Status != ExportState.Succeeded && export.Status != ExportState.Failed);

                if (export.Status != ExportState.Succeeded)
                {
                    throw new Exception("Report null");
                }

                return await GetExportedFilePBI(reportId, groupId, export);
            }
            catch { throw; }
        }

        private async Task<string> PostExportRequestPBI(Guid reportId, Guid groupId, string format, IList<string> pageNames = null, string urlFilter = null)
        {
            var powerBIReportExportConfiguration = new PowerBIReportExportConfiguration
            {
                Settings = new ExportReportSettings
                {
                    Locale = "en-us",
                },
                //Pages = pageNames?.Select(pn => new ExportReportPage(Name = pn)).ToList(),
                ReportLevelFilters = !string.IsNullOrEmpty(urlFilter) ? new List<ExportFilter>() { new ExportFilter(urlFilter) } : null,
            };

            var exportRequest = new ExportReportRequest
            {
                Format = FileFormat.PDF,
                PowerBIReportConfiguration = powerBIReportExportConfiguration,
            };
            try
            {
                var export = await pbiClient.Reports.ExportToFileAsync(reportId, exportRequest);
                return export.Id;
            }
            catch { throw; }            
        }

        public async Task<HttpOperationResponse<Export>> PollExportRequestPBI(Guid reportId, Guid groupId, string exportId, int timeOutInMinutes, CancellationToken token)
        {
            HttpOperationResponse<Export> httpMessage;
            Progress progressWin = new Progress();
            progressWin.Show();

            DateTime startTime = DateTime.UtcNow;
            const int secToMillisec = 10;
            int counter = 0;
            Export exportStatus;
            do
            {
                if (DateTime.UtcNow.Subtract(startTime).TotalMinutes > timeOutInMinutes || token.IsCancellationRequested)
                {
                    throw new Exception(Messages.ExportTimeout);
                }

                httpMessage =
                    await pbiClient.Reports.GetExportToFileStatusInGroupWithHttpMessagesAsync(groupId, reportId, exportId);

                exportStatus = httpMessage.Body;
                string percent = (exportStatus.PercentComplete == 0) ? counter.ToString() : exportStatus.PercentComplete.ToString();
                progressWin.SetProgress(percent);
                counter = (counter < 100) ? (counter + 10) : (counter);

                if (exportStatus.Status == ExportState.Running || exportStatus.Status == ExportState.NotStarted)
                {
                    var retryAfter = httpMessage.Response.Headers.RetryAfter;
                    var retryAfterInSec = retryAfter.Delta.Value.Seconds;

                    Task.Delay(retryAfterInSec * secToMillisec).Wait();
                }
            }
            // While not in a terminal state, keep polling
            while (exportStatus.Status != ExportState.Succeeded && exportStatus.Status != ExportState.Failed);
            progressWin.Close();
            return httpMessage;
        }

        private async Task<ExportedFile> GetExportedFilePBI(Guid reportId, Guid groupId, Export export)
        {
            if (export.Status == ExportState.Succeeded)
            {
                // The 'Client' object is an instance of the Power BI .NET SDK
                var fileStream = await pbiClient.Reports.GetFileOfExportToFileAsync(groupId, reportId, export.Id);
                return new ExportedFile
                {
                    FileStream = fileStream,
                    ReportName = export.ReportName,
                    FileExtension = export.ResourceFileExtension
                };
            }
            return null;
        }

        #endregion

        #region Obsolete
        //public async Task<string> PostExportRequest(Guid reportId, Guid groupId, string format, IList<string> pageNames = null, string urlFilter = null)
        //{
        //    var exportRequest = new ExportReportRequest
        //    {
        //        Format = ExportFormat.format[format]
        //    };

        //    try
        //    {                
        //        //ReportExported exportId = await _http.ExportToFileAsync(reportId, format);
        //        var export = await pbiClient.Reports.ExportToFileAsync(reportId, exportRequest);
        //        return export.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //public async Task<ExportedFile> GetExportedFile(Guid reportId, Guid groupId, Export export)
        //{
        //    if (export.Status == ExportState.Succeeded)
        //    {
        //        var httpMessage =
        //            await pbiClient.Reports.GetFileOfExportToFileWithHttpMessagesAsync(reportId, export.Id);

        //        return new ExportedFile
        //        {
        //            FileStream = httpMessage.Body,
        //            ReportName = export.ReportName,
        //            FileExtension = export.ResourceFileExtension,
        //        };
        //    }
        //    else
        //    {
        //        throw new Exception(Messages.ErrorExport);
        //    }
        //}
        #endregion
    }
}
