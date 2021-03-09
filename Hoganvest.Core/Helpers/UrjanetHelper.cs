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

        public async ValueTask<bool> DownloadStatemet(string statementId, string fileName, string companyName)
        {
            bool isdownloaded = false;
            try
            {
                if (!string.IsNullOrEmpty(_token))
                {
                    statementId = statementId.Replace('"', ' ').Trim();
                    string filePath = Path.Combine(_urjanetDetails.StatementPath, companyName, fileName + ".pdf");
                    if (File.Exists(filePath))
                    {
                        Console.WriteLine("Statement - " + fileName + " already exists in - " + filePath);
                        isdownloaded = false;
                    }
                    else
                    {
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
