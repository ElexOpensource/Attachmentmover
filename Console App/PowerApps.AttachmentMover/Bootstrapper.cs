using CrashReporterDotNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using PowerApps.AttachmentMover.Enumerations;
using PowerApps.AttachmentMover.Factory;
using PowerApps.AttachmentMover.Properties;

namespace PowerApps.AttachmentMover
{
    public partial class Bootstrapper
    {
        static void Main(string[] args)
        {
            Init();
        }

        private string GetChoiceString()
        {
            StringBuilder sbChoiceBuider = new StringBuilder(string.Empty);

            int intArg = 1;

            foreach (var modes in Enum.GetValues(typeof(SupportedModes)))
            {
                sbChoiceBuider.AppendFormat("{0} {1}  ",intArg++,modes);
            }

            return (sbChoiceBuider.ToString());
        }

        private void GatewayProcess()
        {
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

        public void RunWorkflow(LocalStorageFactory lFactory)
        {
            Log.Debug("Workflow Began Executing");
            lFactory.GetEligibleFiles();
            lFactory.TransmitFiles();
            Log.Debug("Workflow Ended Executing");

            Log.CloseAndFlush();
        }
    }
}