using Hoganvest.Core.Common;
using Hoganvest.Model.Responses;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hoganvest.Core.Helpers
{
    public class UrjanetHelper
    {
        private readonly UrjanetDetails _urjanetDetails;
        private readonly string _token;

        public UrjanetHelper(UrjanetDetails urjanetDetails, string token)
        {
            _urjanetDetails = urjanetDetails;
            _token = token;
        }
        public async ValueTask<(DataTable, DateTime)> StatementResponse(string search, string[] args)
        {
            StatementResponse statementResponse = new StatementResponse();
            DataTable dt = new DataTable();
            try
            {
                TokenResponse tokenResponse = await connectUrjanet();
                if (!string.IsNullOrEmpty(_token))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(_urjanetDetails.BaseAddress);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                        if (args != null && args.Length > 0)
                        {
                            DateTime statementDate;
                            if (DateTime.TryParse(args[0], out statementDate))
                            {
                                search = search + statementDate.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                search = search + DateTime.Now.Date.AddDays(_urjanetDetails.StatementDaysDownload).ToString("yyyy-MM-dd");
                            }
                        }
                        else
                        {
                            search = search + DateTime.Now.Date.AddDays(_urjanetDetails.StatementDaysDownload).ToString("yyyy-MM-dd");
                        }
                        var data = new { search = search };
                        StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                        Console.WriteLine("Statement Request Data: " + data);
                        var result = await client.PostAsync("apautomation/statements/downloads", content);
                        result.EnsureSuccessStatusCode();

                        string resultContent = await result.Content.ReadAsStringAsync();
                        Console.WriteLine("Statement Response Data: " + resultContent);
                        statementResponse = JsonConvert.DeserializeObject<StatementResponse>(resultContent);

                    }
                    if (statementResponse != null && statementResponse._links != null && statementResponse._links.download != null && statementResponse._links.download.href != null)
                    {
                        dt = await ReadFile(statementResponse);
                    }
                    else
                    {
                        Console.WriteLine("Statement Response does not contain  href details");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            return (dt, statementResponse.createdDate);
        }

        public async ValueTask<UrjanetCredentialsResponse> GetAllCredentials(int pageNumber)
        {
            UrjanetCredentialsResponse urjanetCredentialsResponse = new UrjanetCredentialsResponse();
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(_urjanetDetails.BaseAddress);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                        var result = await client.GetAsync($"apautomation/credentials?sort=username,asc&size=100&page={pageNumber}& search=mock==false");
                        string resultContent = await result.Content.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        urjanetCredentialsResponse = JsonConvert.DeserializeObject<UrjanetCredentialsResponse>(resultContent);
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
            return urjanetCredentialsResponse;
        }

        public async ValueTask<string> GetWebsiteByProviderId(string providerId)
        {
            string password = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(_urjanetDetails.BaseAddress);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                        var result = await client.GetAsync($"apautomation/providers/{providerId}");
                        string resultContent = await result.Content.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        var urjanetCredentialsResponse = JsonConvert.DeserializeObject<UrjanetProviderResponse>(resultContent);
                        password = urjanetCredentialsResponse?.Website;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return password;
        }

        public async ValueTask<AccountsResponse> GetAllAccounts(string accountId)
        {
            AccountsResponse accountsResponse = new AccountsResponse();
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(_urjanetDetails.BaseAddress);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                        var result = await client.GetAsync($"apautomation/credentials/{accountId}/accounts?sort=status,desc&size=100&page=0");
                        string resultContent = await result.Content.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        accountsResponse = JsonConvert.DeserializeObject<AccountsResponse>(resultContent);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return accountsResponse;
        }
        public async ValueTask<string> GetPasswordByPasswordId(string passwordId)
        {
            string password = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(_urjanetDetails.BaseAddress);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                        var result = await client.GetAsync($"apautomation/credentials/{passwordId}/passwords");
                        string resultContent = await result.Content.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        var urjanetCredentialsResponse = JsonConvert.DeserializeObject<UrjanetPasswordResponse>(resultContent);
                        password = urjanetCredentialsResponse?.password;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return password;
        }

        public async ValueTask<DataTable> ReadFile(StatementResponse StatementResponse)
        {
            DataTable dt = new DataTable();
            int times = 1;
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                    StamentDownload: Stream stream = await client.GetStreamAsync(StatementResponse._links.download.href);
                        Console.WriteLine("Fetching Statements Details from Urjanet API for " + times + " time(s)");
                        Thread.Sleep(_urjanetDetails.StatementResponseWaitTime);
                        using (var sr = new StreamReader(stream))
                        {
                            string line = sr.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                string[] headers = line.Split(',');
                                foreach (string header in headers)
                                {
                                    string replaced = header.Replace(" ", "_");
                                    dt.Columns.Add(replaced);
                                }
                                while (!sr.EndOfStream)
                                {
                                    string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                                    DataRow dr = dt.NewRow();
                                    for (int i = 0; i < headers.Length; i++)
                                    {
                                        string data = rows[i].Replace('"', ' ').Trim();
                                        if (string.IsNullOrEmpty(data))
                                            data = null;
                                        dr[i] = data;
                                    }
                                    dt.Rows.Add(dr);
                                }
                            }
                            else
                            {
                                times++;
                                if (times <= _urjanetDetails.ResponseFailureRetriveCount)
                                    goto StamentDownload;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dt;
        }
        public async ValueTask<TokenResponse> connectUrjanet()
        {
            TokenResponse tokenResponse = new TokenResponse();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_urjanetDetails.BaseAddress);
                    var data = new { username = _urjanetDetails.UserName, password = _urjanetDetails.Password };
                    StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync("auth/login", content);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(resultContent);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return tokenResponse;
        }

        public async ValueTask<bool> DownloadStatemet(string statementId, string fileName, string companyName, string folderName)
        {
            bool isdownloaded = false;
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {  
                    statementId = statementId.Replace('"', ' ').Trim();
                    string filePath = Path.Combine(_urjanetDetails.StatementPath, companyName, folderName, fileName + ".pdf");
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine("Statement - " + fileName + " already exists in - " + filePath);
                        isdownloaded = false;
                    }
                    else
                    {
                        if (!Directory.Exists(Path.Combine(_urjanetDetails.StatementPath, companyName, folderName)))
                            Directory.CreateDirectory(Path.Combine(_urjanetDetails.StatementPath, companyName, folderName));
                        Console.WriteLine("Downloading statement for statementid - " + statementId + " started");
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(_urjanetDetails.BaseAddress);
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                            var str = await client.GetAsync("apautomation/statements/" + statementId + "/source");
                            System.Net.Http.HttpContent content = str.Content; // actually a System.Net.Http.StreamContent instance but you do not need to cast as the actual type does not matter in this case

                            using (var file = System.IO.File.Create(filePath))
                            {
                                var contentStream = await content.ReadAsStreamAsync(); // get the actual content stream
                                await contentStream.CopyToAsync(file); // copy that stream to the file stream
                                Console.WriteLine("Statement - " + fileName + " downloaded in - " + filePath);
                                Console.WriteLine("Downloading statement for statementid - " + fileName + " completed");
                                isdownloaded = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                throw ex;
            }
            return isdownloaded;
        }
    }
}
