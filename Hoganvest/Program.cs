using Hoganvest.App.Configurations;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace Hoganvest.App
{
    class Program
    {
        static void Main(string[] args)
        {
            var startDateTime = DateTime.Now;
            try
            {
                FileStream filestream = new FileStream(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\logs.txt", FileMode.Create);
                var streamwriter = new StreamWriter(filestream);
                streamwriter.AutoFlush = true;
                Console.SetOut(streamwriter);
                Console.SetError(streamwriter);

                ConfigureLogging(args);
                Console.WriteLine("............................");
                Log.Information("............................");
                Log.Verbose("............................");

                Console.WriteLine("Hoganvest Application execution Started at " + startDateTime.ToString());
                Log.Information("Hoganvest Application execution Started  at " + startDateTime.ToString());
                //Log.Verbose("Hoganvest Application execution Started ...");

                ConfigureServices.SetConfigureServices(args).GetAwaiter().GetResult();
                var endDateTime = DateTime.Now;
                Console.WriteLine("Hoganvest Application execution Completed at " + endDateTime.ToString());
                Log.Information("Hoganvest Application execution Completed at " + endDateTime.ToString());
                //Log.Verbose("Hoganvest Application execution Completed...");
                Console.WriteLine("Total time took to complete the process is " + (endDateTime - startDateTime).TotalMinutes);
                Log.Information("Total time took to complete the process is " + (endDateTime - startDateTime).TotalMinutes);

                Console.WriteLine("............................");
                Log.Information("............................");
                //Log.Verbose("............................");
            }
            catch (Exception ex)
            {
                Log.Error("Error - " + ex.Message.ToString());
                Log.Verbose(ex, ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        private static void ConfigureLogging(string[] leveltypes)
        {
            if (leveltypes != null && leveltypes.Length > 0 && leveltypes[0] == "/verbose")
            {
                Log.Logger = new LoggerConfiguration()
                          .Enrich.FromLogContext()
                          .MinimumLevel.Verbose()
                          .WriteTo.File(
                              $"Logs\\Verbose-Log-.txt",
                              rollingInterval: RollingInterval.Day,
                              restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                          .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                          .Enrich.FromLogContext()
                          .MinimumLevel.Verbose()
                          .WriteTo.File(
                              $"Logs\\Log-.txt",
                              rollingInterval: RollingInterval.Day,
                              restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                          .CreateLogger();
            }
        }
    }
}
