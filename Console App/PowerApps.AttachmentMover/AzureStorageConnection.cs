using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using AttachmentMover.Properties;
using AttachmentMover.Utilities;

namespace DynamicsHelper
{
    [Obsolete("This functionality is moving into Factory implementations")]
    class AzureStorageConnection : Constants
        
    {
        public void uploadFilesToAzureStorage()
        {
            //_logger.LogInformation("Warming Up");
            
            //var config = GetConfiguration();
            Console.WriteLine(Resources.FilesWillBeFetchedFrom + sourceFolder);
            Console.WriteLine(Resources.DoYouWantToContinue);
            var res = Console.ReadLine();
            if(res == "Y") { Console.WriteLine(Resources.WhichApproachToProceed + Resources.AAzureBlob + Resources.BDynamics + "?");
                var approach = Console.ReadLine();
               
                if (approach == "dynamics")
                {
                    var movingFiles = new MovingFilesToDynamics();
                    movingFiles.ConnectWithOAuth();
                    return;
                }

            }
            else { return; }
            var files = GetFiles(sourceFolder);
            if (!files.Any())
            {
                //_logger.LogInformation(Constants.NoFiles);
                 return;
            }
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //get a BlobContainerClient
            var container = blobServiceClient.GetBlobContainerClient(containerName);

            //checking if the container exists or not, then determine to create it or not
            bool isExist = container.Exists();
            if (!isExist)
            {
                container.Create();
            }

            //UploadFilesToAzureBlob._logger = _logger;
            UploadFilesToAzureBlob uploadFilesToAzureBlob = new UploadFilesToAzureBlob();
            
            uploadFilesToAzureBlob.Upload(files, connectionString, containerName, sourceFolder);

        }

   
        static IEnumerable<FileInfo> GetFiles(string sourceFolder)
        {
            return new DirectoryInfo(sourceFolder)
                .GetFiles()
                .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        }
    }
}
