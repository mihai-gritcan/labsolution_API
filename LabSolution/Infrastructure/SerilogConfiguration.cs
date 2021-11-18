using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Debugging;
using System;
using System.IO;
using System.Reflection;

namespace LabSolution.Infrastructure
{
    internal static class SerilogConfiguration
    {
        internal const int LOG_FLUSH_DELAY_MILISECONDS = 2000;

        const string SERILOG_DEFAULT_SELF_PATH_APPSETTINGS_KEY = "Serilog:DefaultSelfPath";
        const string SERILOG_DEFAULT_SELF_PATH = "labsolutionapi-self-log.txt";

        internal static void SetupLoggingConfiguration(HostBuilderContext context, ILoggingBuilder builder)
        {
            EnableSelfLog(context.Configuration);

            Log.Logger = GetLogger(context.Configuration);

        }
        private static Serilog.ILogger GetLogger(IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("HostName", Environment.MachineName)
                .Enrich.WithProperty("Release", Assembly.GetEntryAssembly()?.GetName().Version.ToString())
                .CreateLogger();
        }

        private static void EnableSelfLog(IConfiguration configuration)
        {
            var defaultLogPath = configuration.GetValue<string>(SERILOG_DEFAULT_SELF_PATH_APPSETTINGS_KEY);

            if (!PathExists(defaultLogPath))
            {
                defaultLogPath = Path.Combine(Path.GetTempPath(), SERILOG_DEFAULT_SELF_PATH);
            }

            SelfLog.Enable(message => File.AppendAllText(defaultLogPath, message));
        }

        private static bool PathExists(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);

                if (!fileInfo.Directory.Exists && fileInfo.Directory.Parent.Exists)
                {
                    fileInfo.Directory.Create();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
