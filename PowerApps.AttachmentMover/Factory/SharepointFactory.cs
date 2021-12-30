using System;
using System.Configuration;
using System.IO;
using System.Security;
using Microsoft.SharePoint.Client;
using Serilog;

namespace AttachmentMover.Factory
{
    /// <summary>
    ///     Implementation for Sharepoint route for publishing files from Local Storage to Dynamics 365
    /// </summary>
    public class SharepointFactory : LocalStorageFactory
    {
        /// <summary>
        ///    Sharepoint Site URL
        /// </summary>
        public string siteURL { get; set; }

        /// <summary>
        ///   Sharepoint Document Library
        /// </summary>
        public string documentLibrary { get; set; }

        /// <summary>
        ///   Sharepoint Customer Folder 
        /// </summary>
        public string customerFolder { get; set; }

        /// <summary>
        ///    Sharepoint User Name
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        ///    Sharepoint Password
        /// </summary>
        public string password { get; set; }

        /// <summary>
        ///    Sharepoint Customer Secret
        /// </summary>
        public string SecretId { get; set; }

        /// <summary>
        ///    Sharepoint App Id
        /// </summary>
        public string appId { get; set; }



        /// <summary>
        ///    Initialization of Sharepoint Factory
        /// </summary>
        /// <param name="_logger">Injected version of Logger</param>
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

        /// <summary>
        ///   Worker Process to begin transmission of files from Local to Dynamics 365
        /// </summary>
        /// <returns>True, if successful</returns>
        public override bool TransmitFiles()
        {
            var securePassword = new SecureString();
            foreach (char c in password)
            { securePassword.AppendChar(c); }

            var authenticationManager = new OfficeDevPnP.Core.AuthenticationManager();

            var onlineCredentials = new SharePointOnlineCredentials(userName, securePassword);
 
            ClientContext context = authenticationManager.GetWebLoginClientContext(siteURL, null);

            Folder Clientfolder = null;


            if (context is null)
            {
                string strError = "Authentication was not completed properly";
                Console.WriteLine(strError);
                Logger.Error(strError);
            }

            if (base.QueuedFiles is null)
            {
                Logger.Warning("No files to process was found");
            }

            if (context is null || base.QueuedFiles is null)
            {
                return (false);
            }

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

            try
            {
                if (Clientfolder != null)
                    Clientfolder.DeleteObject();
            }
            catch (Exception ClientFolderDeleteException) 
            {
                Logger.Error(ClientFolderDeleteException, ClientFolderDeleteException.Message);
            }

            return (true);
        }
         
    }
}
