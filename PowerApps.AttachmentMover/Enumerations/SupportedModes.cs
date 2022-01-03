using System;
namespace AttachmentMover.Enumerations
{
    /// <summary>
    ///    The supported options for import of local files through this application.
    /// </summary>
    public enum SupportedModes
    {
        /// <summary>
        ///    Publishing to Dynamics 365 through Azure Blob
        /// </summary>
        AzureBlob =1,
        /// <summary>
        ///   Publishing to Dynamics 365 through Intermediate Staging Entity
        /// </summary>
        StagingEntity = 2,
        /// <summary>
        ///   Publishing to Dynamics 365 through Sharepoint Site
        /// </summary>
        Sharepoint = 3,
    }
}
