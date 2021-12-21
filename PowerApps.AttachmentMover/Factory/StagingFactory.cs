using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using AttachmentMover.Properties;
using Microsoft.Xrm.Tooling.Connector;
using Serilog.Core;
using Serilog;
using System.Reflection;

namespace AttachmentMover.Factory
{
    public class StagingFactory : LocalStorageFactory
    {
        public string authType { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string url { get; set; }
        public string appId { get; set; }
        public string reDirectURL { get; set; }
        public string loginPrompt  { get; set; }
        public string ConnectionString = String.Empty;
        
          SecurityProtocolType securityProtocolType { get; set; }

        CrmServiceClient svc = null;
        public StagingFactory(ILogger _logger) : base(_logger)
        {
            authType = ConfigurationManager.AppSettings["StagingApproach.AuthType"];
            userName = ConfigurationManager.AppSettings["StagingApproach.UserName"];
            password = ConfigurationManager.AppSettings["StagingApproach.Password"];
            url = ConfigurationManager.AppSettings["StagingApproach.URL"];
            appId = ConfigurationManager.AppSettings["StagingApproach.appId"];
            reDirectURL = ConfigurationManager.AppSettings["StagingApproach.RedirectURL"];
            loginPrompt = ConfigurationManager.AppSettings["StagingApproach.LoginPrompt"];
           
            ConnectionString = ($"AuthType = {authType}; Username = {userName};Password = {password}; Url = {url}; AppId={appId}; RedirectUri = {reDirectURL}; LoginPrompt = {loginPrompt}");


            var securityProtocol = ConfigurationManager.AppSettings["StagingApproach.SecurityProtocol"];
            if (string.IsNullOrEmpty(securityProtocol))
                Logger.Warning("A null or empty security protocol was specified in the configuation");
            else
            {
                securityProtocol = securityProtocol.Trim().ToLower();
                SecurityProtocolType secOutputType;


                if (!Enum.TryParse<SecurityProtocolType>(securityProtocol, out secOutputType))
                {
                    Logger.Warning("An unknown security protocol was configured to be set.");
                }
                else
                {
                    securityProtocolType = secOutputType;
                }
            }

            svc = new CrmServiceClient(ConnectionString);
        }

        public override bool TransmitFiles()
        {
            foreach (var eachFileInQueue in QueuedFiles)
            {
                if (eachFileInQueue.Extension.Contains(".json"))
                {
                    using (StreamReader filess = File.OpenText(eachFileInQueue.FullName))
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
                            string attachment = Path.Combine(strLocalPath, documentFileName);
                            var myContact = new Entity("new_documentattachment");
                            myContact.Attributes["new_entityname"] = logicalName;
                            myContact.Attributes["new_recordguid"] = guid;
                            //myContact.Attributes["new_name"] = logicalName + guid;
                            myContact.Attributes["new_filename"] = documentFileName;
                            myContact.Attributes["new_isannotation"] = isAnnoatation;
                            myContact.Attributes["new_filefieldname"] = filefieldname;
                            Guid RecordID = svc.Create(myContact);


                            Byte[] bytes = File.ReadAllBytes(attachment);
                            String base64String = Convert.ToBase64String(bytes);


                            Entity note = new Entity("annotation");
                            note["subject"] = documentFileName;
                            note["filename"] = documentFileName;
                            note["documentbody"] = base64String;

                            note["objectid"] = new EntityReference("new_documentattachment", RecordID);
                            RecordID = svc.Create(note);

                            Logger.Debug(RecordID + " was created");
                        }
                    }
                }
            }

            return (true);
        }
    }
}
