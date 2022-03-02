using Hoganvest.Business;
using Hoganvest.Business.Interfaces;
using Hoganvest.Core.Common;
using Hoganvest.Data.Interfaces;
using Hoganvest.Data.Repository.Base;
using Hoganvest.Data.Repository.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Hoganvest.App.Configurations
{
    public static class ConfigureServices
    {
        public static IConfigurationRoot _configuration;
        private static IServiceProvider _serviceProvider;
        private static string _token;
        public static async ValueTask SetConfigureServices(string[] args)
        {
            try
            {

                Console.WriteLine("SetConfigureServices function execution started...");
                Log.Information("SetConfigureServices function execution started...");
                Log.Verbose("SetConfigureServices function execution started...");

                LoadConfiguration();

                RegisterServices();

                await RunServices(args);

                DisposeServices();

                Console.WriteLine("SetConfigureServices function execution completed...");
                Log.Information("SetConfigureServices function execution completed...");
                Log.Verbose("SetConfigureServices function execution completed...");

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SetConfigureServices function" + ex.Message.ToString());
                Log.Fatal("Error in SetConfigureServices function" + ex.Message.ToString());
                Log.Verbose(ex, ex.Message);
            }

        }
        public static IConfiguration LoadConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("AppSettings.json", optional: true,
                             reloadOnChange: true);

                _configuration = builder.Build();


                return _configuration;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                Log.Fatal("Error :" + ex.Message.ToString());
                Log.Verbose(ex, ex.Message);
                throw ex;
            }
        }
        private static async ValueTask RunServices(string[] args)
        {
            try
            {
                var urjanetStatementBusiness = _serviceProvider.GetService<IUrjanetStatementBusiness>();
                _token = await urjanetStatementBusiness.getToken();
                if (!string.IsNullOrEmpty(_token))
                {

                    await urjanetStatementBusiness.AddStatement(_token, args);

                    Console.WriteLine("Started Fetching Credentials...");
                    Log.Information("Started Fetching Credentials...");
                    await urjanetStatementBusiness.GetAllCredentials(_token);
                    Console.WriteLine("Completed Fetching Credentials...");
                    Log.Information("Completed Fetching Credentials...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                Log.Fatal("Error :" + ex.Message.ToString());
                Log.Verbose(ex, ex.Message);
                var mailBusiness = _serviceProvider.GetService<IMailBusiness>();
                mailBusiness.SendMail(ex.Message);
                throw ex;
            }
        }
        private static IServiceProvider RegisterServices()
        {
            try
            {
                var urjanetDetails = new UrjanetDetails();
                var connectionStrings = new ConnectionStrings();
                var mailSettings = new MailSettings();
                var googleDriveDetails = new GoogleDriveDetails();
                var oneDriveDetails = new OneDriveDetails();
                _configuration.GetSection("UrjanetDetails").Bind(urjanetDetails);
                _configuration.GetSection("ConnectionStrings").Bind(connectionStrings);
                _configuration.GetSection("MailSettings").Bind(mailSettings);
                _configuration.GetSection("GoogleDriveDetails").Bind(googleDriveDetails);
                _configuration.GetSection("OneDriveDetails").Bind(oneDriveDetails);


                _serviceProvider = new ServiceCollection()
                  .AddSingleton<IUnitOfWork, UnitOfWork>()
                  .AddSingleton<HoganvestContext>()
                  .AddSingleton<IUrjanetStatementBusiness, UrjanetStatementBusiness>()
                  .AddSingleton<IMailBusiness, MailBusiness>()
                  .AddScoped<UrjanetDetails>(svcs => urjanetDetails)
                  .AddScoped<ConnectionStrings>(svcs => connectionStrings)
                   .AddScoped<MailSettings>(svcs => mailSettings)
                   .AddScoped<GoogleDriveDetails>(svcs => googleDriveDetails)
                   .AddScoped<OneDriveDetails>(svcs => oneDriveDetails)
                  .BuildServiceProvider();
                return _serviceProvider;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
                Log.Fatal("Error :" + ex.Message.ToString());
                Log.Verbose(ex, ex.Message);
                throw ex;
            }
        }
        private static void DisposeServices()
        {
            try
            {
                if (_serviceProvider == null)
                {
                    return;
                }
                if (_serviceProvider is IDisposable)
                {
                    ((IDisposable)_serviceProvider).Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error :" + ex.Message.ToString());
            }
        }
    }
}
