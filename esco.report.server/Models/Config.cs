using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace esco.report.server
{
    class Config
    {
        public static string http                   = "http://";
        public static string https                  = "https://";
        public static string loginUriBase           = "https://login.microsoftonline.com/";        
        public static string authorityUriBase       = "https://login.microsoftonline.com/common/";
        public static string authorityUri           = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
        public static string tokenUri               = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
        public static string tokenUriBase           = "https://login.microsoftonline.com/common/oauth2/token";
        public static string tokenTenantUri         = "https://login.microsoftonline.com/{0}/oauth2/token";

        public static string apiPbi                 = "https://api.powerbi.com/v1.0/myorg/";
        public static string resource               = "https://analysis.windows.net/powerbi/api";
        public static string apiBase                = "https://api.powerbi.com/";
        public static string appBase                = "https://app.powerbi.com/";
        public static string scope                  = "https://analysis.windows.net/powerbi/api/.default";

        public static string scopeid                = "openid";        
        public static string nonce                  = "abced";
        public static string state                  = "12345";
        public static string response               = "code";
        public static string mode                   = "query";
        public static string grant_type             = "client_credentials";
        public static string grant_type_pass        = "password";
        public static string redirect               = "http://localhost/myapp";

        public static string maxEmbed               = "?rs:Command=Render&rc:Toolbar=false";
        public static string embed                  = "?rs:embed=true";
        public static string autoAuth               = "&autoAuth=true";

        public static string render                 = "&rs:Command=Render";
        public static string export                 = "&rs:Format=";

        public static string paginated              = "PaginatedReport";
        public static string typeReport             = "Report";
        public static string typePowerBI            = "PowerBIReport";
        public static string pathReport             = "report";
        public static string pathPowerBI            = "powerbi";


        //Endpoints        
        public class Local
        {
            public static string api_reports = "reports/{0}";
            public static string api_reportspbi = "powerbireports/{0}";
            public static string api_catalogitems = "CatalogItems/";
            public static string api_catalogitemsId = "CatalogItems/{0}";
            public static string api_system = "System";
        }

        public class Cloud
        {
            public static string api_reportsId = "reports/{0}";
            public static string api_reports = "reports";
            public static string api_export = "reports/{0}/ExportTo";
            public static string api_exportstatus = "reports/{0}/exports/{1}/status";
            public static string api_exportfile = "reports/{0}/exports/{1}/file";
        }
        

    }
   
}
