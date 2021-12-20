using PowerApps.AttachmentMover.Utilities;
using Azure.Storage.Blobs;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PowerApps.AttachmentMover.Properties;
using Constants = PowerApps.AttachmentMover.Utilities.Constants;

namespace PowerApps.AttachmentMover.Factory
{
    public class AzureContainerFactory : LocalStorageFactory
    {
        public AzureContainerFactory(ILogger _logger) : base (_logger)
        {
            base.strLocalPath = Utilities.Constants.sourceFolder;
        }
     
        public override bool TransmitFiles()
        {
            Logger.Debug("Cloud Transmission Started");
            BlobServiceClient blobServiceClient = new BlobServiceClient(Utilities.Constants.connectionString);

            //get a BlobContainerClient
            var container = blobServiceClient.GetBlobContainerClient(Utilities.Constants.containerName);

            //checking if the container exists or not, then determine to create it or not
            bool isExist = container.Exists();
            if (!isExist)
            {
                container.Create();
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
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }
            }

            Logger.Debug("Cloud Transmission Ended");
            return (true);
        }
    }
}