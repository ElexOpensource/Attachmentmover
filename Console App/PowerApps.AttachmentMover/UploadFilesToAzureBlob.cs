using AttachmentMover.Utilities;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace DynamicsHelper
{
 
    class UploadFilesToAzureBlob : Constants
    {
    
        public void Upload(IEnumerable<FileInfo> files, string ConnectionString, string container, string sourceFolder)
        {
            var containerClient = new BlobContainerClient(ConnectionString, container);
            foreach (var file in files)
            {
                try
                {
                    if (file.Name.Contains(".json"))
                    {
                        var jsonDeserialize = new JsonDeserialization();
                        string result = jsonDeserialize.GetMetadata(file.Name, sourceFolder, containerClient);

                    }

                    var blobClient = containerClient.GetBlobClient(file.Name);
                    bool isFileExists = blobClient.Exists();
                    if (isFileExists == true)
                    {
                        //_logger.LogWarning($"{file.Name} "+ Constants.existingMessage);
                    }
                    else
                    {
                        using (var fileStream = File.OpenRead(file.FullName))
                        {
                            blobClient.Upload(fileStream);
                        }

                        //_logger.LogInformation($"{file.Name} "+ Constants.successMessage);
                    }
                }
                catch (Exception e)
                {
                    //_logger.LogError(e.Message);
                }
            }

        }

    }
}
