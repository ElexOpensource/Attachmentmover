using CrashReporterDotNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerApps.AttachmentMover
{
    public partial class Bootstrapper
    {
        private static ReportCrash _reportCrash;

        private static void Init()
        {
            #region Preparing Pre-requisites
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(ConfigurationManager.AppSettings["Log Path"])
                .CreateLogger();

            var services = new ServiceCollection();
            ConfigureServices(services);
            #endregion

            #region Culture and Locale 
            string language = CultureInfo.CurrentCulture.Name;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            #endregion

            #region Execution Point
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.GatewayProcess();
            #endregion 
        }

        private static void ConfigureServices(ServiceCollection services)
        {

            services.AddLogging(configure => configure.AddConsole())
            .AddTransient<Bootstrapper>();

            services.AddLogging(configure => configure.AddSerilog());
        }

        private void HookCrashHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, crashargs) =>
            {
                SendReport((Exception)crashargs.ExceptionObject, String.Empty, true);
            };

            _reportCrash = new ReportCrash(String.Empty)
            {
                Silent = true,
                ShowScreenshotTab = true,
                IncludeScreenshot = false,
                AnalyzeWithDoctorDump = true,
                DoctorDumpSettings = new DoctorDumpSettings
                {
                    ApplicationID = new Guid(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(ConfigurationManager.AppSettings["CoreDump ReferId"]))),
                    OpenReportInBrowser = true
                }
            };
        }
        private static void SendReport(Exception exception, string developerMessage = "", bool silentReport = false)
        {
            _reportCrash.DeveloperMessage = developerMessage;
            _reportCrash.Silent = silentReport;
            _reportCrash.Send(exception);
        }
    }
}
