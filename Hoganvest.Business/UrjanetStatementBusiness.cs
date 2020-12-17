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
                        bulkCopy.WriteToServer(urjanetStatementsDto);
                    }
                    destinationConnection.Close();
                    response.IsSuccess = true;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Messages.Add(ex.InnerException.ToString());
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
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn col in dt.Columns)
                    {
                        //check if corelation id is hoganvest, download all and upload to one drive
                        //if (row[dt.Columns["\"Correlation_Id\""]].ToString() == "Hoganvest")
                        //{
                        //    if (col.ColumnName.Replace('"', ' ').Trim() == "Statement_Id")
                        //    {
                        //        if (!string.IsNullOrEmpty(row[col].ToString()))
                        //        {
                        //            {
                        //                UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                        //                string filePath = await urjanetHelper.DownEachStatement((row[col].ToString()), dateTime, true);
                        //                if (!string.IsNullOrEmpty(filePath))
                        //                {
                        //                    var oneDrive = new OneDriveGraphApi("e208a66b-6ef1-4ed2-bdbb-eeb6680ca4fa");
                        //                    oneDrive.ProxyConfiguration = System.Net.WebRequest.DefaultWebProxy;
                        //                    var v = await oneDrive.GetDriveRoot();
                        //                    await oneDrive.UploadFile(filePath, v);
                        //                    //await OneDriveUpload(filePath);
                        //                    System.IO.File.Delete(filePath);
                        //                    //byte[] b = System.IO.File.ReadAllBytes(filePath);
                        //                    //UploadFileBySession(ondriveUrl, b);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
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
                                    UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                                    string filePath = await urjanetHelper.DownEachStatement((row[col].ToString()), dateTime, true);
                                    SaveFileOnGoogleDrive(filePath, _googleDriveDetails.StructureFolderId);
                                    Console.WriteLine("Uploaded to google drive successfully");
                                    System.IO.File.Delete(filePath);
                                }
                            }
                        }
                        else if (row[dt.Columns["\"Correlation_Id\""]].ToString().ToLower() == "hoganvest")
                        {
                            if (col.ColumnName.Replace('"', ' ').Trim() == "Statement_Id")
                            {
                                if (!string.IsNullOrEmpty(row[col].ToString()))
                                {
                                    UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                                    string filePath = await urjanetHelper.DownEachStatement((row[col].ToString()), dateTime, true);
                                    SaveFileOnGoogleDrive(filePath, _googleDriveDetails.HoganvestFolderId);
                                    Console.WriteLine("Uploaded to google drive successfully");
                                    System.IO.File.Delete(filePath);
                                }
                            }
                        }
                        else
                        {
                            if (col.ColumnName.Replace('"', ' ').Trim() == "Statement_Id")
                            {
                                if (!string.IsNullOrEmpty(row[col].ToString()))
                                {
                                    UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                                    await urjanetHelper.DownEachStatement((row[col].ToString()), dateTime);

                                }
                            }
                        }
                    }
                }
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                throw ex;
            }
            return response;
        }
        public async ValueTask<Response> AddStatement(string token)
        {
            Console.WriteLine("Accessing statement started.....");
            DataTable dt = new DataTable();
            Response response = new Response();
            try
            {
                UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                var result = await urjanetHelper.StatementResponse(_urjanetDetails.Search);
                if (result.Item1 != null && result.Item1.Rows.Count > 0 && result.Item2 != null)
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
                }
                else
                {
                    Console.WriteLine("Statement not found for given search - " + _urjanetDetails.Search);
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
                        foreach (DataColumn col in dt.Columns)
                        {
                            if (col.ColumnName.Replace('"', ' ').Trim() == "Statement_Id")
                            {
                                if (!string.IsNullOrEmpty(row[col].ToString()))
                                {
                                    DataRow[] dr = dbTable.Select("[Statement_Id] ='" + row[col].ToString() + "'");
                                    if (dr.Length == 0)
                                    {
                                        res.Rows.Add(row.ItemArray);
                                        break;
                                    }
                                }
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

        #endregion

    }


}
