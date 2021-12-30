namespace AttachmentMover.Utilities
{
    /// <summary>
    ///    Application wide constants
    /// </summary>
    public static class Constants
    {  
        /// <summary>
        ///   Azure Connection String
        /// </summary>
        public static readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["azureConnection"].ConnectionString;

        // Azure Container Name 
        public static readonly string containerName = System.Configuration.ConfigurationManager.AppSettings["container"];

        /// <summary>
        ///   Local Source Folder 
        /// </summary>
        public static readonly string sourceFolder = System.Configuration.ConfigurationManager.AppSettings["sourceFolder"];

        /// <summary>
        ///   JSON indicator in file names
        /// </summary>
        public static string JSON = "json";
    }
}