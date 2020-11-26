namespace Hoganvest.Core.Common
{
    public class AppSettings
    {
        public UrjanetDetails UrjanetDetails { get; set; } = new UrjanetDetails();
        public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    }
    public class UrjanetDetails
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Search { get; set; }
        public string StatementPath { get; set; }
        public string BaseAddress { get; set; }
        public string TableName { get; set; }
        public int ResponseFailureRetriveCount { get; set; }
        public bool OnOffPDFDownloads { get; set; }
        public string AccountNumbers { get; set; }
        public string TempPath { get; set; }
    }
    public class GoogleDriveDetails
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string StructureFolderId { get; set; }
        public string HoganvestFolderId { get; set; }
    }
    public class OneDriveDetails
    {
        public string Instance { get; set; } = "https://login.microsoftonline.com/{0}";
        public string ApiUrl { get; set; } = "https://graph.microsoft.com/";
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
    public class ConnectionStrings
    {
        public string HoganvestDBString { get; set; }
    }
    public class MailSettings
    {
        public string FromMail { get; set; }
        public string FromPsd { get; set; }
        public string ToMails { get; set; }
        public string CCMails { get; set; }
        public string BCCmails { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Subject { get; set; }
        public bool EnableSSL { get; set; }
    }
}

