using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Hoganvest.Business.Interfaces;
using Hoganvest.Core.Common;
using Hoganvest.Core.Helpers;
using Hoganvest.Data.Interfaces;
using Hoganvest.Data.Repository.Models;
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
        private readonly IUnitOfWork _unitOfWork;

        public UrjanetStatementBusiness(UrjanetDetails urjanetDetails, ConnectionStrings connectionStrings, GoogleDriveDetails googleDriveDetails, OneDriveDetails oneDriveDetails, IUnitOfWork unitOfWork)
        {
            _urjanetDetails = urjanetDetails;
            _connectionStrings = connectionStrings;
            _googleDriveDetails = googleDriveDetails;
            _oneDriveDetails = oneDriveDetails;
            _unitOfWork = unitOfWork;
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
            //List<string> accountNumbers = _urjanetDetails.AccountNumbers.Split(',').ToList();
            try
            {
                int i = dt.Rows.Count;
                int hoganvestStatementsUploadCount = 0, structureStatementsUploadCount = 0, localFilesDownloadCount = 0;
                //List<PropertyDirectory> propertyDirectories = GetPropertyDirectories();
                List<PropertyDirectory> propertyDirectories = new List<PropertyDirectory>();
                foreach (DataRow row in dt.Rows)
                {
                    DateTime dueorstatementDate;
                    if (row["\"Due_Date\""] is DBNull)
                    {
                        dueorstatementDate = Convert.ToDateTime(row["\"Statement_Date\""]);
                    }
                    else
                    {
                        dueorstatementDate = Convert.ToDateTime(row["\"Due_Date\""]);
                    }
                    string folderName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dueorstatementDate.Month) + " " + dueorstatementDate.Year.ToString();
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (row[dt.Columns["\"Correlation_Id\""]].ToString().ToLower() == "structure")
                        {
                            //if (col.ColumnName.Replace('"', ' ').Trim() == "Raw_Account_Number")
                            //{
                            //    var v = accountNumbers.FirstOrDefault(x => x.Contains(row[col].ToString()));
                            //    if (v == null)
                            //    {
                            //        break;
                            //    }
                            //}
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
                                    bool isDownloaded = await urjanetHelper.DownloadStatemet((row[col].ToString()), fileName, "Structure", folderName);
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
                                    bool isDownloaded = await urjanetHelper.DownloadStatemet((row[col].ToString()), fileName, "Hoganvest", folderName);
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
                                    bool isDownloaded = await urjanetHelper.DownloadStatemet((row[col].ToString()), fileName, "Default", folderName);
                                    Console.WriteLine("File Downloaded successfully");
                                    localFilesDownloadCount++;

                                }
                            }
                        }
                    }
                }
                Console.WriteLine("Downloaded " + hoganvestStatementsUploadCount + " hoganvest statements");
                Console.WriteLine("Downloaded " + structureStatementsUploadCount + " structure statements");
                Console.WriteLine("Downloaded " + localFilesDownloadCount + " Default statements");
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
        public async ValueTask<Response> GetAllCredentials(string token)
        {
            Response response = new Response();
            try
            {
                UrjanetHelper urjanetHelper = new UrjanetHelper(_urjanetDetails, token);
                int pageNumber = -1;
                int totalCount = 0;
               // if (TruncateCredTables())
                //{
                    do
                    {
                        pageNumber++;
                        var urjanetCredentialsResponse = await urjanetHelper.GetAllCredentials(pageNumber);
                        if (urjanetCredentialsResponse?._embedded?.credentials?.Count > 0)
                        {
                            {
                                List<Credential> urjanetCredentials = new List<Credential>();
                                totalCount = urjanetCredentialsResponse.page.totalPages;

                                foreach (var item in urjanetCredentialsResponse._embedded.credentials)
                                {
                                    var urjanetCredential = _unitOfWork.UrjanetCredentials.SingleOrDefaultAsync(x => x.UserName == item.username && x.ProviderName == item.providerName).Result;
                                    var credentialId = 0;
                                    if (urjanetCredential == null)
                                    {
                                        string website = string.Empty, password = string.Empty;
                                        if (!string.IsNullOrEmpty(item._links.provider.href) && !string.IsNullOrEmpty(item._links.provider.href.Split("providers/")[1]))
                                        {
                                            website = await urjanetHelper.GetWebsiteByProviderId(item._links.provider.href.Split("providers/")[1]);
                                        }
                                        if (!string.IsNullOrEmpty(item._links.passwords.href) && !string.IsNullOrEmpty(item._links.passwords.href.Split("credentials/")[1]))
                                        {
                                            string passwordId = item._links.passwords.href.Split("credentials/")[1];
                                            password = await urjanetHelper.GetPasswordByPasswordId(passwordId.Split("/passwords")[0]);
                                        }
                                        Credential credential = new Credential()
                                        {
                                            UserName = item.username,
                                            CorrelationId = item.correlationId,
                                            Status = item.status,
                                            StatusDetail = item.statusDetail,
                                            Enabled = item.enabled,
                                            Password = password,
                                            Website = website,
                                            ProviderName = item.providerName,
                                            LastModified = item.lastModified,
                                            Created = item.created,
                                            CreatedBy = item.createdBy,
                                            LastModifiedBy = item.lastModifiedBy,
                                            RunHistory = item.runHistory,
                                            Mock = item.mock,
                                        };
                                        await _unitOfWork.UrjanetCredentials.AddAsync(credential);
                                        await _unitOfWork.CommitAsync();
                                        credentialId = credential.CredentialId;
                                        if (credentialId > 0)
                                        {
                                            if (!string.IsNullOrEmpty(item._links.passwords.href) && !string.IsNullOrEmpty(item._links.passwords.href.Split("credentials/")[1]))
                                            {
                                                string accountId = item._links.passwords.href.Split("credentials/")[1];
                                                var accountsResponse = await urjanetHelper.GetAllAccounts(accountId.Split("/passwords")[0]);
                                                if (accountsResponse?._embedded?.accounts?.Count > 0)
                                                {
                                                    List<CredentialDetails> credentialDetails = new List<CredentialDetails>();
                                                    foreach (var account in accountsResponse?._embedded?.accounts)
                                                    {
                                                        var CredentialDetails = _unitOfWork.CredentialDetails.SingleOrDefaultAsync(x => x.AccountNumber == account.accountNumber && x.PropertyId == account._embedded.customData.PropertyID && x.CredentialId == credentialId).Result;
                                                        if (CredentialDetails == null)
                                                        {
                                                            credentialDetails.Add(new CredentialDetails()
                                                            {
                                                                AccountNumber = account.accountNumber,
                                                                CredentialId = credentialId,
                                                                AccountStatus = account.status,
                                                                PropertyId = account._embedded.customData.PropertyID
                                                            });
                                                        }
                                                    }
                                                    if (credentialDetails.Count > 0)
                                                    {
                                                        await _unitOfWork.CredentialDetails.AddRangeAsync(credentialDetails);
                                                        await _unitOfWork.CommitAsync();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } while (pageNumber < totalCount);
                //}
            }
            catch (Exception ex)
            {
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

        private bool TruncateCredTables()
        {
            try
            {
                string csDestination = _connectionStrings.HoganvestDBString;
                using (var sqlConnection = new SqlConnection(csDestination))
                {
                    sqlConnection.Open();
                    SqlCommand command = new SqlCommand("truncate table CredentialDetails;delete from Credential;DBCC CHECKIDENT(Credential, RESEED, 0)", sqlConnection);
                    command.ExecuteNonQuery();
                    sqlConnection.Close();
                }
            }
            catch (Exception ex)
            {

            }
            return true;
        }
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
                //string propertyName = propertyDirectories.FirstOrDefault(i => i.PropertyId == Convert.ToInt32(dataRow["PropertyID"]))?.PropertyName;
                //if (!string.IsNullOrEmpty(propertyName))
                //    propertyName = propertyName.Replace("/", " ") + delimeter;
                string providerName = dataRow["\"Provider_Name\""].ToString() + delimeter;
                string rawAccountNumber = dataRow["\"Raw_Account_Number\""].ToString();
                rawAccountNumber = rawAccountNumber.Replace("-", "").Replace(" ", "") + delimeter;
                DateTime dueorstatementDate;
                bool IsDueDateNull = false;
                if (dataRow["\"Due_Date\""] is DBNull)
                {
                    dueorstatementDate = Convert.ToDateTime(dataRow["\"Statement_Date\""]);
                    IsDueDateNull = true;
                }
                else
                {
                    dueorstatementDate = Convert.ToDateTime(dataRow["\"Due_Date\""]);
                }
                string dueMonth = dueorstatementDate.ToString("MM") + delimeter;
                string dueYear = dueorstatementDate.ToString("yy");
                string amount = dataRow["\"Total_due\""].ToString() + delimeter;
                //fileName = propertyName + statementMonth + statementyear + providerName + rawAccountNumber + amount;
                if (IsDueDateNull)
                {
                    fileName = providerName + rawAccountNumber + amount + "StatementDate_" + dueMonth + dueYear;
                }
                else
                {
                    fileName = providerName + rawAccountNumber + amount + dueMonth + dueYear;
                }

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
