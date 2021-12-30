using System;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading;
using AttachmentMover.Enumerations;
using AttachmentMover.Factory;
using AttachmentMover.Properties;
using CrashReporterDotNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AttachmentMover
{
    /// <summary>
    ///    Worker Process for Application Launcher
    /// </summary>
    public class BootstrapperWorker
    {
        /// <summary>
        ///    Crash Reporter for Application
        /// </summary>
        private static ReportCrash _reportCrash;

        /// <summary>
        ///    Decision Maker method to route to the appropriate Factory
        /// </summary>
        public void GatewayProcess()
        {
            Init();

            Console.WriteLine(Resources.ModeOfImport + GetChoiceString());
            string ChosenTemplate = Console.ReadLine();

            if (string.IsNullOrEmpty(ChosenTemplate))
            {
                Console.WriteLine(Resources.ModeOfImportInvalid);
            }
            else
            {
                int choiceOpted = 0;
                if (Int32.TryParse(ChosenTemplate, out choiceOpted))
                {
                    SupportedModes enumSupportedModes = (SupportedModes)choiceOpted;
                    switch (enumSupportedModes)
                    {
                        case SupportedModes.AzureBlob:
                            RunWorkflow(new AzureContainerFactory(Log.Logger));
                            break;
                        case SupportedModes.StagingEntity:
                            RunWorkflow(new StagingFactory(Log.Logger));
                            break;
                        case SupportedModes.Sharepoint:
                            RunWorkflow(new SharepointFactory(Log.Logger));
                            break;
                        default:
                            Console.WriteLine(Resources.ModeOfImportInvalid);
                            Log.Warning(Resources.ModeOfImportInvalid);
                            break;
                    }
                }
                else
                    Console.WriteLine(Resources.ModeOfImportInvalid);
            }
        }

        /// <summary>
        ///    Initialize the Workflow Engine
        /// </summary>
        /// <param name="lFactory">Templated Factory</param>
        public void RunWorkflow(LocalStorageFactory lFactory)
        {
            Log.Debug("Workflow Began Executing");
            lFactory.GetEligibleFiles();
            lFactory.TransmitFiles();
            Log.Debug("Workflow Ended Executing");

            Log.CloseAndFlush();
        }

        /// <summary>
        ///    Prepares the supported options of the application as an input string
        /// </summary>
        /// <returns>Display string of supported options</returns>
        private string GetChoiceString()
        {
            StringBuilder sbChoiceBuider = new StringBuilder(string.Empty);

            int intArg = 1;

            foreach (var modes in Enum.GetValues(typeof(SupportedModes)))
            {
                sbChoiceBuider.AppendFormat("{0} {1}  ", intArg++, modes);
            }

            return (sbChoiceBuider.ToString());
        }

        /// <summary>
        ///    Common Initiliazion Routines
        /// </summary>
        private static void Init()
        {
            #region Preparing Pre-requisites
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(ConfigurationManager.AppSettings["Log Path"])
                .CreateLogger();
            #endregion

            #region Culture and Locale 
            string language = CultureInfo.CurrentCulture.Name;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            #endregion

            #region Preparing Pre-requisites
            var services = new ServiceCollection();
            ConfigureServices(services);
            #endregion

        }

        /// <summary>
        ///   Hook up the required middleware and services for the application
        /// </summary>
        /// <param name="services">Service Collection</param>
        private static void ConfigureServices(ServiceCollection services)
        {

            services.AddLogging(configure => configure.AddConsole())
            .AddTransient<BootstrapperWorker>();

            services.AddLogging(configure => configure.AddSerilog());
        }

        /// <summary>
        ///    Attach a Crash Reorting Handler to the application along with its desired configuration options
        /// </summary>
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

        /// <summary>
        ///    File the unhandled reports in the application as per Crash Reporter configuration
        /// </summary>
        /// <param name="exception">Exception Message</param>
        /// <param name="developerMessage">Optional Developer Message</param>
        /// <param name="silentReport">Should message autoprepared be sent or displayed as UI to the user for confirmation</param>
        private static void SendReport(Exception exception, string developerMessage = "", bool silentReport = false)
        {
            _reportCrash.DeveloperMessage = developerMessage;
            _reportCrash.Silent = silentReport;
            _reportCrash.Send(exception);
        }
    }
}
