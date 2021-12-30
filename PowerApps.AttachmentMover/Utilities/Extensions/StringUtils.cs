using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace AttachmentMover.Utilities.Extensions
{
    /// <summary>
    ///    Common Utility Methods for String Object
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        ///    Convert an Input String into a SecureString
        /// </summary>
        /// <param name="strInputString">Input String</param>
        /// <returns>SecureString</returns>
        /// <exception cref="ArgumentNullException">If conversion fails an exception is returned</exception>
        public static SecureString ToSecureString(this string strInputString)
        {
            if (string.IsNullOrEmpty(strInputString))
                throw new ArgumentNullException($"{strInputString} is empty or null");

            var securePassword = new SecureString();

            foreach (char c in strInputString)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }

        /// <summary>
        ///   Returns a collection of FileInfo objects from the given Source Folder
        /// </summary>
        /// <param name="sourceFolder">Input Path</param>
        /// <returns>An IEnumerale collection of FileInfo objects</returns>
        public static IEnumerable<FileInfo> GetFiles(this string sourceFolder)
        {
            try
            {
                return new DirectoryInfo(sourceFolder)
                .GetFiles();
            }
            catch
            {
                return Enumerable.Empty<FileInfo>();
            }
        }
    }
}