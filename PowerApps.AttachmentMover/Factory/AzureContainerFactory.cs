using AttachmentMover.Properties;
using Azure.Storage.Blobs;
using Serilog;
using System;
using System.IO;
using Constants = AttachmentMover.Utilities.Constants;

namespace AttachmentMover.Factory
{
    /// <summary>
    ///    Implementation for Azure Container route for publishing files from Local Storage to Dynamics 365
    /// </summary>
    public class AzureContainerFactory : LocalStorageFactory
    {
        /// <summary>
        ///    Initialization of Azure Container Factory
        /// </summary>
        /// <param name="_logger">Injected version of Logger</param>
        public AzureContainerFactory(ILogger _logger) : base (_logger)
        {
            base.strLocalPath = Constants.sourceFolder;
        }
     
        /// <summary>
        ///    Worker Process to begin transmission of files from Local to Dynamics 365
        /// </summary>
        /// <returns>True, if successful</returns>
        public override bool TransmitFiles()
        {
            Logger.Debug("Cloud Transmission Started");
            BlobServiceClient blobServiceClient = new BlobServiceClient(Constants.connectionString);

            //get a BlobContainerClient
            var container = blobServiceClient.GetBlobContainerClient(Constants.containerName);

            //checking if the container exists or not, then determine to create it or not
            bool isExist = container.Exists();
            if (!isExist)
            {
                container.Create();
            }

            if (base.QueuedFiles is null)
            {
                Logger.Warning("No files to process was found");
                return (false);
            }
                

            foreach (var file in base.QueuedFiles)
            {
                try
                {
                    var blobClient = container.GetBlobClient(file.Name);
                    if (blobClient.Exists())
                    {
                        Logger.Warning($"{file.Name} " + Resources.AlreadyExisting);
                    }
                    else
                    {
                        using (var fileStream = File.OpenRead(file.FullName))
                        {
                            blobClient.Upload(fileStream);
                        }

                        Logger.Information($"{file.Name} " + Resources.UploadedSuccessfully);
                    }
                }
                catch (Exception FileUploadException)
                {
                    string strError = string.Format("{0} failed to upload with error {1}", file.Name, FileUploadException.Message);
                    ProcessingErrors.Add(strError);
                    Log.Error(FileUploadException, strError);
                }
            }

            Logger.Debug("Cloud Transmission Ended");
            return (true);
        }
    }
}