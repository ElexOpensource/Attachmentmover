using AttachmentMover.Utilities;
using AttachmentMover.Utilities.Extensions;
using Microsoft.SharePoint.Client;
using PnP.Framework;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection; 
using System.Text;
using System.Threading.Tasks;  
using System.Security; 

namespace AttachmentMover.Factory
{
    public class SharepointFactory : LocalStorageFactory
    {
        public string siteURL { get; set; }
        public string documentLibrary { get; set; }
        public string customerFolder { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string SecretId { get; set; }
        public string appId { get; set; }

          
 
        public SharepointFactory(ILogger _logger) : base(_logger)
        {
            siteURL = ConfigurationManager.AppSettings["Sharepoint.SiteURL"];
            userName = ConfigurationManager.AppSettings["Sharepoint.Username"];
            password = ConfigurationManager.AppSettings["Sharepoint.Password"];
            customerFolder = ConfigurationManager.AppSettings["Sharepoint.CustomerFolder"];
            documentLibrary = ConfigurationManager.AppSettings["Sharepoint.DocumentLibrary"];
            appId = ConfigurationManager.AppSettings["StagingApproach.appId"];
            SecretId = ConfigurationManager.AppSettings["Sharepoint.SecretId"];
        }
        
        public override bool TransmitFiles()
        {
            //var context = new AuthenticationManager().GetACSAppOnlyContext(siteURL, appId, SecretId);
            var securePassword = new SecureString();
            foreach (char c in password)
            { securePassword.AppendChar(c); }

            var authenticationManager = new OfficeDevPnP.Core.AuthenticationManager();

            var onlineCredentials = new SharePointOnlineCredentials(userName, securePassword);
 
            ClientContext context = authenticationManager.GetWebLoginClientContext(siteURL, null);

            Folder Clientfolder = null;

            foreach (var path in base.QueuedFiles)
            {
                try
                {
                    FileInfo fileName = new FileInfo(path.FullName);
                    var localFileName = fileName.Name;
                    FileCreationInformation newFile = new FileCreationInformation();
                    byte[] FileContent = System.IO.File.ReadAllBytes(path.FullName);
                    newFile.ContentStream = new MemoryStream(FileContent);
                    newFile.Url = Path.GetFileName(localFileName);
                    Web web = context.Web;
                    List DocumentLibrary = web.Lists.GetByTitle(documentLibrary);
                    Clientfolder = DocumentLibrary.RootFolder.Folders.Add(customerFolder);
                    Clientfolder.Update();
                    Microsoft.SharePoint.Client.File uploadFile = Clientfolder.Files.Add(newFile);
                    context.Load(DocumentLibrary);
                    context.Load(uploadFile);
                    context.ExecuteQuery();
                }
                catch (Exception FileUploadException)
                {
                    string strError = string.Format("{0} failed to upload with error {1}", path.FullName, FileUploadException.Message);
                    ProcessingErrors.Add(strError);
                    Log.Error(FileUploadException, strError);
                }
            }

            if (Clientfolder != null)
                Clientfolder.DeleteObject();

            return (true);
        }
         
    }
}
