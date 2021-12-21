using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace AttachmentMover.Utilities
{
    class FileAnalyzer : Constants
    {
        public string ExtractMetaData(string fileName, string path,BlobContainerClient containerClient)
        {
            string logicalName = "", guid = "";

            using (StreamReader filess = File.OpenText(Path.Combine(path, fileName)))

            using (JsonTextReader reader = new JsonTextReader(filess))
            {
                JArray jsonArray = (JArray)JArray.ReadFrom(reader);
                foreach (var item in jsonArray)
                {
                    logicalName = item["logicalname"].ToString();
                    guid = item["entityguid"].ToString();
                    Dictionary<string, string> metadataProperties = new Dictionary<string, string>();
                    metadataProperties.Add(logicalName, guid);
                    containerClient.SetMetadata(metadataProperties);
                }
            }
            return logicalName+","+guid;
        }
    }
}