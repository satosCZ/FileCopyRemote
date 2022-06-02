using System.Runtime.InteropServices;
using System.Security.Principal;

namespace FileCopyRemote
{
    class Program
    {
        #region LogonType
        /// <summary>
        /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on
        /// by a terminal server, remote shell, or similar process. This logon type has the additional expense of caching logon
        /// information for disconnected operations; therefore, it is inappropriate for some client/server applications,
        /// such as a mail server.
        /// </summary>
        const int LOGON32_LOGON_INTERACTIVE = 2;

        /// <summary>
        /// This logon type is intended for high performance servers to authenticate plaintext passwords.
        /// The LogonUser function does not cache credentials for this logon type.
        /// </summary>
        const int LOGON32_LOGON_NETWORK = 3;

        /// <summary>
        /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without
        /// their direct intervention. This type is also for higher performance servers that process many plaintext
        /// authentication attempts at a time, such as mail or Web servers.
        /// The LogonUser function does not cache credentials for this logon type.
        /// </summary>
        const int LOGON32_LOGON_BATCH = 4;

        /// <summary>
        /// Indicates a service-type logon. The account provided must have the service privilege enabled.
        /// </summary>
        const int LOGON32_LOGON_SERVICE = 5;

        /// <sumary>
        /// This logon type is for GINA DLLs that log on users who will be interactively using the computer.
        /// This logon type can generate a unique audit record that shows when the workstation was unlocked.
        /// </sumary>
        const int LOGON32_LOGON_UNLOCK = 7;

        /// <sumary>
        /// This logon type preserves the name and password in the authentication package, which allows the server to make
        /// connections to other network servers while impersonating the client. A server can accept plaintext credentials
        /// from a client, call LogonUser, verify that the user can access the system across the network, and still
        /// communicate with other servers.
        /// NOTE: Windows NT:  This value is not supported.
        /// </sumary>
        const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;

        /// <sumary>
        /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
        /// The new logon session has the same local identifier but uses different credentials for other network connections.
        /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
        /// NOTE: Windows NT:  This value is not supported.
        /// </sumary>
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        #endregion

        #region LogonProvider
        /// <summary>
        /// Use the standard logon provider for the system. The default security provider is negotiate, unless you pass NULL for the domain name and the user name
        /// is not in UPN format. In this case, the default provider is NTLM. 
        /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
        /// </summary>
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_PROVIDER_WINNT35 = 1;
        const int LOGON32_PROVIDER_WINNT40 = 2;
        const int LOGON32_PROVIDER_WINNT50 = 3;
        #endregion
        
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            // Check if the user put 5 arguments (source file, destination file, admin username and password, and the domain name)
            if (args.Count() == 5)
            {
                // Arguments for executing
                string sourceFile = args[0];
                string destinationFile = args[1];
                string userName = args[2];
                string password = args[3];
                string domain = args[4];
                Console.WriteLine("Copy file....");
                CopyFile(sourceFile, destinationFile, domain, userName, password);
            }
        }
        
        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        ///<summary>
        /// Copy file to remote computer
        /// </summary>'
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destination">Destination path</param>
        private static void CopyFile(string sourceFile, string destination, string domain, string userName, string password)
        {
            // Check if file exists
            if (System.IO.File.Exists(sourceFile))
            {
                // temporary string with file name
                string fileName;

                // Admin token
                IntPtr adminToken = default(IntPtr);

                // Current user
                WindowsIdentity widCurrent = WindowsIdentity.GetCurrent();

                // Administrator
                WindowsIdentity widAdmin = null;

                // Creating Windows nation for admin level
                WindowsImpersonationContext wic = null;
                
                try
                {
                    if (LogonUser(userName, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, ref adminToken) != 0)
                    {
                        // Get admin user
                        widAdmin = new WindowsIdentity(adminToken);
                        // Get admin user context
                        wic = widAdmin.Impersonate();
                        // Get current user context
                        FileInfo fi = new FileInfo(sourceFile);

                        // Setting new file name with date of copy
                        fileName = fi.Name.Split('.')[0] + "_" + DateTime.Now.ToString("yyyyMMdd") + "." + fi.Name.Split('.')[1];

                        Console.WriteLine("File: {0}\n Source: {1}\n Destination: {2}", fileName, sourceFile, destination);

                        // Copy file to remote computer
                        System.IO.File.Copy(sourceFile, destination + "\\" + fileName);

                        Console.WriteLine("File copied");
                    }
                    else
                    {
                        Console.WriteLine("Error: " + Marshal.GetLastWin32Error());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    // Release admin user context
                    if (wic != null)
                    {
                        wic.Undo();
                    }
                    // Release admin user
                    if (widAdmin != null)
                    {
                        widAdmin.Dispose();
                    }
                }
            }
            else
            {
                Console.WriteLine("File not found");
            }
        }
    }
}