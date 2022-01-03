using System;
using System.Configuration;
using System.IO;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Serilog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AttachmentMover.Utilities;

namespace AttachmentMover.Factory
{
    public class StagingFactory : LocalStorageFactory
    {
        /// <summary>
        ///    Authentication Type 
        /// </summary>
        public string authType { get; set; }

        /// <summary>
        ///   Username
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        ///   Password
        /// </summary>
        public string password { get; set; }

        /// <summary>
        ///   URL
        /// </summary>
        public string url { get; set; }

        /// <summary>
        ///    Application ID
        /// </summary>
        public string appId { get; set; }

        /// <summary>
        ///   Redirector URL
        /// </summary>
        public string reDirectURL { get; set; }

        /// <summary>
        ///    Login Prompt
        /// </summary>
        public string loginPrompt  { get; set; }

        /// <summary>
        ///   Connection String
        /// </summary>
        public string ConnectionString { get; set; }
        
          SecurityProtocolType securityProtocolType { get; set; }

        /// <summary>
        ///    Crm Service Client Instance
        /// </summary>
        CrmServiceClient svc = null;

        /// <summary>
        ///    Initialization of Staging Factory
        /// </summary>
        /// <param name="_logger">Injected version of Logger</param>

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

        /// <summary>
        ///    Worker Process to begin transmission of files from Local to Dynamics 365
        /// </summary>
        /// <returns>True, if successful</returns>
        public override bool TransmitFiles()
        {
            if (base.QueuedFiles is null)
            {
                Logger.Warning("No files to process was found");
                return (false);
            }

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
                            string strFileToUpload = string.Empty;

                            try
                            {
                                logicalName = item[StagingFields.LOGICAL_NAME].ToString();
                                guid = item[StagingFields.ENTITY_GUID].ToString();
                                documentFileName = item[StagingFields.FILE_NAME].ToString();
                                isAnnoatation = item[StagingFields.IS_ANNOTATION].ToString();
                                filefieldname = item[StagingFields.FILE_FIELD_NAME].ToString();
                                strFileToUpload = Path.Combine(strLocalPath, documentFileName);
                                string attachment = strFileToUpload;
                                var myContact = new Entity(StagingFields.NEW_DOCUMENT_ATTACHMENT);
                                myContact.Attributes[StagingFields.NEW_ENTITY_NAME] = logicalName;
                                myContact.Attributes[StagingFields.NEW_RECORD_GUID] = guid;
                                myContact.Attributes[StagingFields.NEW_FILE_NAME] = documentFileName;
                                myContact.Attributes[StagingFields.NEW_IS_ANNOTATION] = isAnnoatation;
                                myContact.Attributes[StagingFields.NEW_FILE_FIELD_NAME] = filefieldname;
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
                            catch (Exception FileUploadException)
                            {
                                string strError = string.Format("{0} failed to upload with error {1}", strFileToUpload, FileUploadException.Message);
                                ProcessingErrors.Add(strError);
                                Log.Error(FileUploadException, strError);
                            }
                        }
                    }
                }
            }

            return (true);
        }
    }
}
