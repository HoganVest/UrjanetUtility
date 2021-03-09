using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Hoganvest.Business.Interfaces;
using Hoganvest.Core.Common;
using Hoganvest.Core.Helpers;
using Hoganvest.Model.Responses;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Hoganvest.Business
{
    public class UrjanetStatementBusiness : IUrjanetStatementBusiness
    {
        private readonly UrjanetDetails _urjanetDetails;
        private readonly ConnectionStrings _connectionStrings;
        private readonly GoogleDriveDetails _googleDriveDetails;
        private readonly OneDriveDetails _oneDriveDetails;

        public UrjanetStatementBusiness(UrjanetDetails urjanetDetails, ConnectionStrings connectionStrings, GoogleDriveDetails googleDriveDetails, OneDriveDetails oneDriveDetails)
        {
            _urjanetDetails = urjanetDetails;
            _connectionStrings = connectionStrings;
            _googleDriveDetails = googleDriveDetails;
            _oneDriveDetails = oneDriveDetails;
        }
        private Response AddUrjanetStatments(DataTable urjanetStatementsDto)
        {
            Response response = new Response();
            try
            {
                string csDestination = _connectionStrings.HoganvestDBString;
                using (SqlConnection destinationConnection = new SqlConnection(csDestination))
                {
                    destinationConnection.Open();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
                    {
                        bulkCopy.DestinationTableName = _urjanetDetails.TableName;
                        bulkCopy.BulkCopyTimeout = 0;
                        bulkCopy.WriteToServer(urjanetStatementsDto);
                    }
                    destinationConnection.Close();
                    response.IsSuccess = true;
                    Console.WriteLine("Inserted " + urjanetStatementsDto.Rows.Count + " statement details in database");
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Messages.Add(ex.Message.ToString());
                Console.WriteLine("Error :" + ex.Message.ToString());
            }
            return response;
        }
        private async ValueTask<Response> DownEachStatement(DataTable dt, DateTime dateTime, string token)
        {
            Response response = new Response();
            List<string> accountNumbers = _urjanetDetails.AccountNumbers.Split(',').ToList();
            try
            {
                int i = dt.Rows.Count;
                int hoganvestStatementsUploadCount = 0, structureStatementsUploadCount = 0, localFilesDownloadCount = 0;
                List<PropertyDirectory> propertyDirectories = GetPropertyDirectories();
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (row[dt.Columns["\"Correlation_Id\""]].ToString().ToLower() == "structure")
                        {
                            if (col.ColumnName.Replace('"', ' ').Trim() == "Raw_Account_Number")
                            {
                                var v = accountNumbers.FirstOrDefault(x => x.Contains(row[col].ToString()));
                                if (v == null)
                                {
                                    break;
                                }
                            }
                            if (col.ColumnName.Replace('"', ' ').Trim() == "Statement_Id")
                            {
                                if (!string.IsNullOrEmpty(row[col].ToString()))
                                {
                                    string fileName = string.Empty;
                                    if (row["PropertyID"] is DBNull)
                                    {
                                        fileName = row[col].ToString().Replace('"', ' ').Trim();
                                    }
                                    else
                                    {
                                        fileName = GetFileName(propertyDirectories, row);
                                    }

                                    UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                                    bool isDownloaded = await urjanetHelper.DownloadStatemet((row[col].ToString()), fileName, "Structure");
                                    Console.WriteLine("File Downloaded successfully");
                                    structureStatementsUploadCount++;

                                }
                            }
                        }
                        else if (row[dt.Columns["\"Correlation_Id\""]].ToString().ToLower() == "hoganvest")
                        {
                            if (col.ColumnName.Replace('"', ' ').Trim() == "Statement_Id")
                            {
                                if (!string.IsNullOrEmpty(row[col].ToString()))
                                {
                                    string fileName = string.Empty;
                                    if (row["PropertyID"] is DBNull)
                                    {
                                        fileName = row[col].ToString().Replace('"', ' ').Trim();
                                    }
                                    else
                                    {
                                        fileName = GetFileName(propertyDirectories, row);
                                    }
                                    UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                                    bool isDownloaded = await urjanetHelper.DownloadStatemet((row[col].ToString()), fileName, "Hoganvest");
                                    Console.WriteLine("File Downloaded successfully");
                                    hoganvestStatementsUploadCount++;

                                }
                            }
                        }
                        else
                        {
                            if (col.ColumnName.Replace('"', ' ').Trim() == "Statement_Id")
                            {
                                if (!string.IsNullOrEmpty(row[col].ToString()))
                                {
                                    string fileName = string.Empty;
                                    if (row["PropertyID"] is DBNull)
                                    {
                                        fileName = row[col].ToString().Replace('"', ' ').Trim();
                                    }
                                    else
                                    {
                                        fileName = GetFileName(propertyDirectories, row);
                                    }
                                    UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                                    bool isDownloaded = await urjanetHelper.DownloadStatemet((row[col].ToString()), fileName, "Default");
                                    Console.WriteLine("File Downloaded successfully");
                                    localFilesDownloadCount++;

                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Uploaded " + hoganvestStatementsUploadCount + " hoganvest statements to google drive folder");
                Console.WriteLine("Uploaded " + structureStatementsUploadCount + " structure statements to google drive folder");
                Console.WriteLine("Uploaded " + localFilesDownloadCount + " statements to local file system");
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                throw ex;
            }
            return response;
        }
        public async ValueTask<Response> AddStatement(string token, string[] args)
        {
            Console.WriteLine("Accessing statement started.....");
            DataTable dt = new DataTable();
            Response response = new Response();
            try
            {
                UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                var result = await urjanetHelper.StatementResponse(_urjanetDetails.Search, args);
                if (result.Item1.Rows.Count > 0)
                {
                    Console.WriteLine("Successfully accessed statement");
                    Console.WriteLine("Adding statement to database started.......");
                    //Loop through each row. check if statementid present, remove from dt
                    dt = CheckEachStatementExistsInDB(result.Item1);
                    if (dt.Rows.Count > 0)
                    {
                        response = AddUrjanetStatments(dt);
                        if (response.IsSuccess && _urjanetDetails.OnOffPDFDownloads)
                        {
                            Console.WriteLine("Successfully added statement to the database");
                            await DownEachStatement(dt, result.Item2, token);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No new statements found");
                    }
                }
                else
                {
                    Console.WriteLine("No statements found with search criteria");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                throw ex;
            }
            return response;
        }

        public async ValueTask<string> getToken()
        {
            Console.WriteLine("Accessing urjanet token...");
            string token = string.Empty;
            try
            {
                UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, null);
                TokenResponse tokenResponse = await urjanetHelper.connectUrjanet();
                if (tokenResponse != null && tokenResponse.Status == 200 && tokenResponse.Token != null)
                {
                    token = tokenResponse.Token;
                    Console.WriteLine("Successfully accessed token...");
                }
                else
                    Console.WriteLine("Error :" + tokenResponse.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                throw ex;
            }
            return token;
        }

        #region Private Methods
        private DataTable CheckEachStatementExistsInDB(DataTable dt)
        {
            DataTable res = new DataTable();
            string csDestination = _connectionStrings.HoganvestDBString;
            try
            {
                res = dt.Clone();
                DataTable dbTable = new DataTable();
                using (var sqlConnection = new SqlConnection(csDestination))
                {
                    sqlConnection.Open();
                    {
                        SqlDataAdapter da = new SqlDataAdapter(("SELECT * FROM " + _urjanetDetails.TableName + ""), sqlConnection);
                        da.Fill(dbTable);
                    }
                    sqlConnection.Close();
                }
                if (dbTable != null && dbTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(row["\"Statement_Id\""].ToString()))
                        {
                            DataRow[] dr = dbTable.Select("[Statement_Id] ='" + row["\"Statement_Id\""].ToString() + "'");
                            if (dr.Length == 0)
                            {
                                res.Rows.Add(row.ItemArray);
                            }
                        }
                    }
                }
                else
                    return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;
        }
        private bool SaveFileOnGoogleDrive(string url, string folderId)
        {
            string[] scopes = new string[] { DriveService.Scope.Drive,
                          DriveService.Scope.DriveAppdata,
                          //DriveService.Scope.DriveAppsReadonly,
                          DriveService.Scope.DriveFile,
                          DriveService.Scope.DriveMetadataReadonly,
                          DriveService.Scope.DriveReadonly,
                          DriveService.Scope.DriveScripts };
            var clientId = _googleDriveDetails.ClientId;    // From https://console.developers.google.com
            var clientSecret = _googleDriveDetails.ClientSecret;

            // From https://console.developers.google.com
            // here is where we Request the user to give us access, or use the Refresh Token that was previously stored in %AppData%
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            },
                                                                    scopes,
                                                                    Environment.UserName,
                                                                    CancellationToken.None,
                                                                    new FileDataStore("MyAppsToken")).Result;
            //Once consent is recieved, your token will be stored locally on the AppData directory, so that next time you wont be prompted for consent.

            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "CTM",
            });

            uploadFile(service, url, folderId, "");
            return true;
        }
        private static void uploadFile(DriveService _service, string _uploadFile, string _parent, string _descrp)
        {
            if (System.IO.File.Exists(_uploadFile))
            {
                var body = new Google.Apis.Drive.v3.Data.File();
                //File body = new File();
                body.Name = System.IO.Path.GetFileName(_uploadFile);
                body.Description = _descrp;
                //body.Parents = "Urjanet";
                body.MimeType = GetMimeType(_uploadFile);
                List<string> s = new List<string>();
                s.Add(_parent);
                body.Parents = s;

                FilesResource.CreateMediaUpload request;
                try
                {
                    using (var stream = new System.IO.FileStream(_uploadFile, System.IO.FileMode.Open))
                    {
                        request = _service.Files.Create(body, stream, body.MimeType);
                        request.Fields = "id";
                        request.Upload();
                    }
                    //var file = request.ResponseBody;
                    //var fili = file.Id;
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                Console.WriteLine("File not exists in the path - " + _uploadFile);
            }
        }
        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
        public async Task<bool> OneDriveUpload(string Path)
        {
            IConfidentialClientApplication app;
            AuthenticationResult result = null;
            string Authority = String.Format(CultureInfo.InvariantCulture, _oneDriveDetails.Instance, _oneDriveDetails.Tenant);
            string[] scopes = new string[] { $"{_oneDriveDetails.ApiUrl}.default" };
            {
                app = ConfidentialClientApplicationBuilder.Create(_oneDriveDetails.ClientId)
                    .WithClientSecret(_oneDriveDetails.ClientSecret)
                    .WithAuthority(new Uri(Authority))
                    .Build();
            }
            result = await app.AcquireTokenForClient(scopes)
                  .ExecuteAsync();
            string token = result.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                Console.Write("One drive token is empty.");
            }
            byte[] data = System.IO.File.ReadAllBytes(Path);
            Stream stream = new MemoryStream(data);
            GraphServiceClient client = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    (requestMessage) =>
                    {
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                        requestMessage.Headers.Add("ContentType", "application/json");
                        //requestMessage.Headers.Add("Accept", "application/json");
                        //Client.DefaultRequestHeaders.Add("ContentType", "application/json");
                        return Task.FromResult(0);
                    }));
            try
            {
                //var folderToCreate = new DriveItem { Folder = new Folder() };
                //var createdFolder = await client
                //          .Drive
                //          .Root
                //          .ItemWithPath("Hoganvest_files/subfolder")
                //          .Request()
                //          .CreateAsync(folderToCreate);

                await client.Me.ItemWithPath("path/to/file.pdf").Content.Request().PutAsync<DriveItem>(stream);
            }
            catch (ServiceException ex)
            {
                //throw ex;
            }
            return true;
        }

        private List<PropertyDirectory> GetPropertyDirectories()
        {
            List<PropertyDirectory> propertyDirectories = null;
            DataTable dataTable = new DataTable();
            try
            {
                using (var sqlConnection = new SqlConnection(_connectionStrings.AccountReceivableDBString))
                {
                    sqlConnection.Open();
                    {
                        SqlDataAdapter da = new SqlDataAdapter(("select PropertyId,PropertyName,PropertyAddress from PropertyDirectory order by PropertyId"), sqlConnection);
                        da.Fill(dataTable);
                    }
                    sqlConnection.Close();
                }
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    propertyDirectories = new List<PropertyDirectory>();
                    propertyDirectories = (from DataRow dr in dataTable.Rows
                                           select new PropertyDirectory()
                                           {
                                               PropertyId = Convert.ToInt32(dr["PropertyId"]),
                                               PropertyName = dr["PropertyName"].ToString(),
                                               PropertyAddress = dr["PropertyAddress"].ToString()
                                           }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return propertyDirectories;
        }

        private string GetFileName(List<PropertyDirectory> propertyDirectories, DataRow dataRow)
        {
            string fileName;
            try
            {
                string delimeter = "_";
                string propertyName = propertyDirectories.FirstOrDefault(i => i.PropertyId == Convert.ToInt32(dataRow["PropertyID"]))?.PropertyName;
                if (!string.IsNullOrEmpty(propertyName))
                    propertyName = propertyName.Replace("/", " ") + delimeter;
                string providerName = dataRow["\"Provider_Name\""].ToString() + delimeter;
                string rawAccountNumber = dataRow["\"Raw_Account_Number\""].ToString();
                rawAccountNumber = rawAccountNumber.Replace("-", "").Replace(" ","") + delimeter;
                DateTime statementDate = Convert.ToDateTime(dataRow["\"Statement_Date\""]);
                string statementMonth = statementDate.ToString("MM") + delimeter;
                string statementyear = statementDate.ToString("yy") + delimeter;
                string amount = dataRow["\"Total_due\""].ToString();
                fileName = propertyName + statementMonth + statementyear + providerName + rawAccountNumber + amount;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return fileName;

        }

        #endregion

    }


}
