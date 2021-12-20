using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using PowerApps.AttachmentMover.Utilities.Extensions;
using PowerApps.AttachmentMover.Properties;
using PowerApps.AttachmentMover.Utilities;

namespace PowerApps.AttachmentMover.Factory
{
    public abstract class LocalStorageFactory
    {
        protected ILogger Logger;

        protected IEnumerable<FileInfo> QueuedFiles;

        public string strLocalPath { get; set; }

        public LocalStorageFactory(ILogger _logger)
        {
            Logger = _logger;
            strLocalPath = Constants.sourceFolder;
        }

        public virtual void GetEligibleFiles()
        {

            Logger.Information("Warming Up");

            Console.WriteLine(Resources.FilesWillBeFetchedFrom + strLocalPath);
            Console.WriteLine(Resources.DoYouWantToContinue);
            var files = strLocalPath.GetFiles();

            var userInput = Console.ReadLine();

            if (!string.IsNullOrEmpty(userInput) && userInput.Trim().ToLower().StartsWith("y"))
            {
                QueuedFiles = files;

                if (!QueuedFiles.Any())
                {
                    Logger.Information(Resources.NoFilesToUpload);
                }

                if (QueuedFiles.Count(x => x.Extension == "." + Constants.JSON) > 1)
                {
                    QueuedFiles = Enumerable.Empty<FileInfo>();
                    Logger.Warning(Resources.UnsupportedFilesFound);
                }
            }
        }

        public abstract bool TransmitFiles();
    }
}