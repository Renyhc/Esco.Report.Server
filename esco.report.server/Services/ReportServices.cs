using esco.report.server.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace esco.report.server
{
    /// <summary>
    /// Conector ESCO Report Services
    /// </summary>  
    public class ReportServices
    {
        private readonly bool _oncloud = false;

        private readonly PowerBI _pbi;
        private readonly ReportServer _rs;

        private readonly string _user;
        private readonly string _pass;

        #region Constuctors
        /// <summary>
        /// Inicialización del Conector ESCO Report para PowerBI Service (on cloud)
        /// </summary>
        /// <param name="client">(Required) Id de la Aplicación AD del Inquilino del Azure AD.</param> 
        /// <param name="secret">(Required) Contraseña o Secreto de del Inquilino del Azure AD.</param>                       
        /// <returns></returns>
        public ReportServices(string client, string secret)
        {
            _pbi = new PowerBI(client, secret);
            _user = client;
            _pass = secret;
            _oncloud = true;
        }

        /// <summary>
        /// Inicialización del Conector ESCO Report para Report Server (on premise)
        /// </summary>
        /// <param name="user">(Required) Nombre o Id del Usuario o Cliente.</param> 
        /// <param name="pass">(Required) Contraseña del usuario de Report Server.</param> 
        /// <param name="url">(Required) Url del Web Services On Premise de Reportes.</param>      
        /// <param name="version">(Optional) Número de versión de la Api Rest de Report Server.</param> 
        /// <param name="enviroment">(Optional) Ambiente de trabajo. "Production (PROD) por defecto.</param> 
        /// <returns></returns>
        public ReportServices(string user, string pass, string url, string version = "2.0", string enviroment="PROD")
        {
            _rs = new ReportServer(user, pass, url, version, enviroment);
            _user = user;
            _pass = pass;
            _oncloud = false;
        }
        #endregion

        #region ViewReport
        /// <summary>
        /// Retorna el Reporte con sus campos asociados
        /// </summary>
        /// <param name="report">(Required) Id o nombre del Reporte</param>                          
        /// <param name="group">(Optional) Id del grupo o carpeta del Reporte</param> 
        /// <param name="param">(Optional) Parametros a filtrar en el Reporte</param> 
        /// <param name="accessToken">(Optional) Token de acceso Azure AD (on cloud)</param>
        /// <returns>ReportEmbed object</returns>
        public async Task<ReportEmbed> GetReport(string report, string group = null, string param = null, string accessToken = null)
        {
            return (_oncloud) ? 
                await ExecReportPBI(report, group, accessToken) : 
                await ExecReportRS(report, group, param);
        }

        /// <summary>
        /// Visualizar Reporte
        /// </summary>
        /// <param name="report">(Required) Id o nombre del Reporte a visualizar.</param>                          
        /// <param name="group">(Optional) Id del grupo o carpeta del Reporte a visualizar.</param> 
        /// <param name="param">(Optional) Parametros a filtrar en el Reporte</param> 
        /// <param name="maximizated">(Optional) Mostrar el reporte de forma maximizada (default: false).</param>
        /// <param name="accessToken">(Optional) Token de acceso Azure AD (on cloud)</param>
        /// <returns>Task void</returns>
        public async Task<ReportEmbed> ExecuteReport(string report, string group = null, string param = null, bool maximizated = false, string accessToken = null)
        {
            ReportEmbed embed = (_oncloud) ? 
                await ExecReportPBI(report, group, accessToken) : 
                await ExecReportRS(report, group, param, maximizated);            

            if (embed == null) { throw new Exception(Messages.ErrorEmbed); }

            Browser browser = new Browser(_oncloud, embed.EmbedUrl, _user, _pass)
            {
                Text = embed.Name
            };
            browser.ShowDialog();

            return embed;
        }

        /// <summary>
        /// Visualizar Reporte en Browser por defecto
        /// </summary>
        /// <param name="report">(Required) Id o nombre del Reporte a visualizar.</param>                          
        /// <param name="group">(Optional) Id del grupo o carpeta del Reporte a visualizar.</param> 
        /// <param name="param">(Optional) Parametros a filtrar en el Reporte</param> 
        /// <param name="maximizated">(Optional) Mostrar el reporte de forma maximizada (default: false).</param>
        /// <param name="accessToken">(Optional) Token de acceso Azure AD (on cloud)</param>
        /// <returns>Task void</returns>
        public async Task<ReportEmbed> ExecuteBrowser(string report, string group = null, string param = null, bool maximizated = false, string accessToken = null)
        {
            ReportEmbed embed = (_oncloud) ?
                await ExecReportPBI(report, group, accessToken) :
                await ExecReportRS(report, group, param, maximizated);

            if (embed == null) { throw new Exception(Messages.ErrorEmbed); }

            string user = (_user.Contains("\\")) ? _user.Split('\\')[1] : _user;
            string http = (embed.EmbedUrl.Contains(Config.https)) ? Config.https : Config.http;
            string url = http + user + ":" + _pass + "@" + embed.EmbedUrl.Replace(http, String.Empty);

            BrowserHelper.OpenBrowserUrl(url);

            return embed;
        }

        private async Task<ReportEmbed> ExecReportPBI(string report, string group, string accessToken)
        {
            if (_pbi == null)
            {
                throw new Exception(Messages.NotInitOnCloud);
            }
            try
            {
                await _pbi.Authenticate(accessToken);
                return await _pbi.GenerateEmbed(report, group);
            }
            catch { throw; }
        }

        private async Task<ReportEmbed> ExecReportRS(string report, string group = null, string param = null, bool max = false)
        {
            if (_rs == null)
            {
                throw new Exception(Messages.NotInitOnPremise);
            }
            try
            {
                return await _rs.GenerateEmbed(report, group, param, max);
            }
            catch { throw; }
        }
        #endregion

        #region Export
        /// <summary>
        /// Exportar Reporte
        /// </summary>
        /// <param name="report">(Required) Nombre o Id del Reporte a Exportar.</param> 
        /// <param name="format">(Optional) Formato del archivo de exportación (default: PDF) Valores aceptados: PPTX, MHTML, IMAGE, EXCELOPENXML, WORDOPENXML, CSV, PDF, XML</param>
        /// <param name="group">(Optional) Grupo al que pertenece el Reporte</param>         
        /// <param name="param">(Optional) Parametros a filtrar eb el Reporte</param> 
        /// <param name="timeOutInMinutes">(Optional) Tiempo de timeout del reporte</param>  
        /// <param name="accessToken">(Optional) Token de acceso Azure AD (on cloud)</param> 
        /// <param name="token">(Optional) Token de cancelación del proceso</param>  
        /// <returns>ExportedFile object</returns>
        public async Task<ExportedFile> ExportReport(
            string report,
            string format = "PDF",
            string group = null,
            string param = null, 
            int timeOutInMinutes = 1,
            string accessToken = null,
            CancellationToken token = default)
        {
            return (_oncloud) ? 
                await ExportReportPBI(report, format, group, timeOutInMinutes, accessToken, token) : 
                await ExportReportRS(report, format, group, param);
        }        

        private async Task<ExportedFile> ExportReportPBI(
            string report, 
            string format, 
            string group, 
            int timeOutInMinutes, 
            string accessToken, 
            CancellationToken token)
        {
            if (_pbi == null)
            {
                throw new Exception(Messages.NotInitOnCloud);
            }
            if (group == null)
            {
                throw new Exception(Messages.NoGroups);
            }
            try
            {
                await _pbi.Authenticate(accessToken);
                return await _pbi.ExportReport(Guid.Parse(report), Guid.Parse(group), format, timeOutInMinutes, token);
            }
            catch { throw; }
        }

        private async Task<ExportedFile> ExportReportRS(string report, string format, string group, string param)
        {
            if (_rs == null)
            {
                throw new Exception(Messages.NotInitOnPremise);
            }
            try
            {
                return await _rs.ExportReport(report, group, format, param);
            }
            catch { throw; }
        }
        #endregion 
    }
}
