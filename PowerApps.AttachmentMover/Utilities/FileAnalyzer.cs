using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.Storage.Blobs;


namespace AttachmentMover.Utilities
{

    /// <summary>
    ///    File Analyzer Class inspecting the properties of the file name as per enclosed JSON definitions
    /// </summary>
    public class FileAnalyzer 
    {
        /// <summary>
        ///    Extracting the Meta Data for the given file from the specified path and attaching the same to Container Client
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <param name="path">Path</param>
        /// <param name="containerClient">Container Client Instance</param>
        /// <returns>A string representation of Meta Data</returns>
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