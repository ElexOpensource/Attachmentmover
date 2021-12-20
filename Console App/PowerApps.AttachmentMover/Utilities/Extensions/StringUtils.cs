using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PowerApps.AttachmentMover.Utilities.Extensions
{
    public static class StringUtils
    {
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
