using AttachmentMover.Properties;
using AttachmentMover.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using AttachmentMover.Utilities.Extensions;

namespace AttachmentMover.Factory
{
    /// <summary>
    ///     Abstraction for Container routes for publishing files from Local Storage to Dynamics 365
    /// </summary>
    public abstract class LocalStorageFactory
    {
        /// <summary>
        ///    Shared Logger Object for individual implementations
        /// </summary>
        protected ILogger Logger;

        /// <summary>
        ///    List of Acceptable files for Transmission
        /// </summary>
        protected IEnumerable<FileInfo> QueuedFiles;

        /// <summary>
        ///    Local Path which will be used for pickup of files
        /// </summary>
        public string strLocalPath { get; set; }

        /// <summary>
        ///   Collection of Errors that were confronted while processing the request(s)
        /// </summary>
        protected List<string> ProcessingErrors = new List<string>();

        /// <summary>
        ///    Worker Process to begin transmission of files from Local to Dynamics 365
        /// </summary>
        /// <param name="_logger">Injected version of Logger</param>
        public LocalStorageFactory(ILogger _logger)
        {
            Logger = _logger;
            strLocalPath = Constants.sourceFolder;
        }

        /// <summary>
        ///    Scan through the local files and prepare the ones acceptable for transmission
        /// </summary>
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

        /// <summary>
        ///    Abstraction for Local Files Transmission (Worker Process)
        /// </summary>
        /// <returns>True if transmission is successful</returns>
        public abstract bool TransmitFiles();
    }
}