using System;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace DataSimulator
{
    public class Impersonator : IDisposable
    {
        private WindowsImpersonationContext impersonationContext = null;

        public Impersonator(string userName, string domainName, string password)
        {
            ImpersonateValidUser(userName, domainName, password);
        }

        public void Dispose()
        {
            UndoImpersonation();
        }

        #region Native Methods

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON_TYPE_NEW_CREDENTIALS = 9;
        private const int LOGON32_PROVIDER_WINNT50 = 3;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int LogonUser(
            string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(
            IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        #endregion

        private void ImpersonateValidUser(string userName, string domain, string password)
        {
            WindowsIdentity tempWindowsIdentity = null;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            try
            {
                if (RevertToSelf())
                {
                    if (0 != LogonUser(
                                    userName,
                                    domain,
                                    password,
                                    Config.Instance.IsVDAMachineInRemoteDomain ? LOGON_TYPE_NEW_CREDENTIALS : LOGON32_LOGON_INTERACTIVE,
                                    Config.Instance.IsVDAMachineInRemoteDomain ? LOGON32_PROVIDER_WINNT50 : LOGON32_PROVIDER_DEFAULT,
                                    ref token))
                    {
                        Trace.Instance.WriteVerbose(">>  >> LogonUser call succeeded.");

                        if (0 != DuplicateToken(token, 2, ref tokenDuplicate))
                        {
                            Trace.Instance.WriteVerbose(">>  >> DuplicateToken call succeeded.");

                            tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                            impersonationContext = tempWindowsIdentity.Impersonate();

                            Trace.Instance.WriteVerbose(">>  >> Impersonating user {0}\\{1}", domain, userName);
                        }
                        else
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }
                    else
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
                if (tokenDuplicate != IntPtr.Zero)
                {
                    CloseHandle(tokenDuplicate);
                }
            }
        }

        private void UndoImpersonation()
        {
            if (impersonationContext != null)
            {
                impersonationContext.Undo();

                Trace.Instance.WriteVerbose(">>  << Impersonation reverted to self.");
            }
        }
    }
}
