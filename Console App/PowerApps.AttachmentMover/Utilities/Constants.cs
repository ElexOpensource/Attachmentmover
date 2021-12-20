using PowerApps.AttachmentMover.Properties;

namespace PowerApps.AttachmentMover.Utilities
{
    public class Constants
    {  
        public static readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["azureConnection"].ConnectionString;
        public static readonly string containerName = System.Configuration.ConfigurationManager.AppSettings["container"];
        public static readonly string sourceFolder = System.Configuration.ConfigurationManager.AppSettings["sourceFolder"];

        public static string JSON = "json";
    }
}
