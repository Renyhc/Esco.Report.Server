using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.PowerBI.Api.Models;

namespace esco.report.server.Models
{
    #region Public

    /// <summary>
    /// ExportedFile Model
    /// </summary>
    public class ExportedFile
    {
        public Stream FileStream;
        public string ReportName;
        public string FileExtension;
    }

    /// <summary>
    /// ReportEmbed Model
    /// </summary>
    public class ReportEmbed
    {
        public string Id;
        public string EmbedToken;
        public string EmbedUrl;
        public string Name;
        public string Type;
        public string Group;
        public string WebUrl;
        public string Dataset;
    }
    #endregion

    #region Private

    enum FileFormats
    {
        PPTX = 0,
        PDF = 1,
        PNG = 2,
        IMAGE = 3,
        XLSX = 4,
        DOCX = 5,
        CSV = 6,
        XML = 7,
        MHTML = 8,
        ACCESSIBLEPDF = 9
    }
    class ExportFormat
    {
        public static readonly Dictionary<string, FileFormats> format = new Dictionary<string, FileFormats>
        {
            { "PPTX", FileFormats.PPTX },
            { "PDF", FileFormats.PDF },
            { "IMAGE", FileFormats.IMAGE },
            { "EXCELOPENXML", FileFormats.XLSX },
            { "WORDOPENXML", FileFormats.DOCX },
            { "MHTML", FileFormats.MHTML },
            { "CSV", FileFormats.CSV },
            { "XML", FileFormats.XML },
        };

        public static readonly Dictionary<string, string> extension = new Dictionary<string, string>
        {
            { "PPTX", ".pptx" },
            { "PDF", ".pdf" },
            { "IMAGE", ".png" },
            { "EXCELOPENXML", ".xlsx" },
            { "WORDOPENXML", ".docx" },
            { "MHTML", ".mhtml" },
            { "CSV", ".csv" },
            { "XML", ".xml" },
        };
    }

    class AccessToken
    {
        public string token_type;
        public string access_token;
    }

    class LocalReportsList
    {
        public List<LocalReport> value { get; set; }
    }

    class LocalReport
    {
        public string Id;
        public string Path;
        public string EmbedUrl;
        public string Name;
        public string Type;
        public string Description;
        public string Dataset;
        public bool Hidden;
        public string ParentFolderId;
        public string Content;
        public bool IsFavorite;
        public bool HasDataSources;
        public bool HasParameters;
    }

    class CloudReport
    {
        public string id;
        public string reportType;
        public string embedUrl;
        public string name;
        public string webUrl;
        public string datasetId;
    }

    class PBISystem
    {
        public string ReportServerAbsoluteUrl;
        public string ReportServerRelativeUrl;
        public string WebPortalRelativeUrl;
        public string ProductName;
        public string ProductVersion;
        public string ProductType;
    }

    class JsonFormat
    {
        public string format;        
    }
    class ReportExported
    {
        public string id;
        public string status;
        public string percentComplete;
    }
    #endregion
}
