using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace esco.report.server
{
    class Messages
    {
        //Messages
        public static string _Error = "Error: ";
        public static string NotInitOnPremise = "On Premise service was not started correctly.";
        public static string NotInitOnCloud = "On Cloud service was not started correctly.";
        public static string FailedEmbedToken = "Failed to generate embed token.";
        public static string ErrorExportId = "Export Id could not be generated.";
        public static string ErrorExport = "Operation incomplete. ";
        public static string ExportTimeout = "Export waiting time has passed.";
        public static string ExportNotPaginated = "Only Paginated Reports can be exported.";
        public static string ReportNotFound = "Report identifier does not exist.";
        public static string AnyReports = "There are no reports on the server.";
        public static string ApiError = "REST API connection failed.";
        public static string ErrorEmbed = "Report link could not be generated.";
        public static string NoParameters = "Parameter is required to generate Report.";
        public static string NoGroups = "Report Groups is required in Power BI On Cloud";
    }
}
