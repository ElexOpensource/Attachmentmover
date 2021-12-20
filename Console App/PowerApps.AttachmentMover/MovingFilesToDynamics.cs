using AttachmentMover.Properties;
using AttachmentMover.Utilities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace DynamicsHelper
{
    [Obsolete("This functionality is moving into Factory implementations")]
    class MovingFilesToDynamics :Constants
    {   
        public void ConnectWithOAuth()
        {          
            Console.WriteLine("Connecting to D365 Server");
            string authType = "OAuth";
            string userName = "demouser10@dynamicsITsolutions.onmicrosoft.com";
            string password = "Somu123Somu";
            string url = "https://orgde6774c7.crm8.dynamics.com/";
            string appId = "3bc4c9ac-a6be-4f2f-851c-73217b611628";
            string reDirectURL = "https://localhost";
            string loginPrompt = "Auto";
            string ConnectionString = string.Format("AuthType = {0}; Username = {1};Password = {2}; Url = {3}; AppId={4}; RedirectUri = {5}; LoginPrompt = {6}",
                                                    authType, userName, password, url, appId, reDirectURL, loginPrompt);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            CrmServiceClient svc = new CrmServiceClient(ConnectionString);
                
                if (svc.IsReady)
                {
                    Console.WriteLine(Resources.ConnectedToD365Server);
                    getFilesFromLocalFolder(svc);
                }
                else
                {
                    Console.WriteLine(Resources.NotConnectedToD365Server);
                }
            }
        public void getFilesFromLocalFolder(CrmServiceClient svc)
        {

            foreach (var path in Directory.GetFiles(sourceFolder))
            {
                Console.WriteLine(System.IO.Path.GetFileName(path));
                FileInfo fileName = new FileInfo(path);
                var localFileName = fileName.Name;
                if (localFileName.Contains(".json"))
                {
                    using (StreamReader filess = File.OpenText(sourceFolder + '/' + localFileName))
                    using (JsonTextReader reader = new JsonTextReader(filess))
                    {
                        string logicalName = "", guid = "", documentFileName = "", isAnnoatation = "", filefieldname = "";

                        JArray jsonArray = (JArray)JArray.ReadFrom(reader);

                        foreach (var item in jsonArray)
                        {
                            logicalName = item["logicalname"].ToString();
                            guid = item["entityguid"].ToString();
                            documentFileName = item["filename"].ToString();
                            isAnnoatation = item["isannotation"].ToString();
                            filefieldname = item["filefieldname"].ToString();
                            string attachment = sourceFolder + '/' + documentFileName;
                            var myContact = new Entity("doc_staging");
                            myContact.Attributes["doc_entityname"] = logicalName;
                            myContact.Attributes["doc_recordguid"] = guid;
                            myContact.Attributes["doc_name"] = logicalName + guid;
                            myContact.Attributes["doc_documentfilename"] = documentFileName;
                            myContact.Attributes["doc_isannotation"] = isAnnoatation;
                            myContact.Attributes["doc_filefieldname"] = filefieldname;
                            Guid RecordID = svc.Create(myContact);


                            Byte[] bytes = File.ReadAllBytes(attachment);
                            String base64String = Convert.ToBase64String(bytes);


                            Entity note = new Entity("annotation");
                            note["subject"] = documentFileName;
                            note["filename"] = documentFileName;
                            note["documentbody"] = base64String;

                            note["objectid"] = new EntityReference("doc_staging", RecordID);
                            RecordID = svc.Create(note);

                        }

                    }
                }
            }
        }

    }
}
