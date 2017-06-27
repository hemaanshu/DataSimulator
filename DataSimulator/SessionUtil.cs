using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace DataSimulator
{
    public class SessionUtil
    {
        internal const String ADVAPI32 = "advapi32.dll";

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor, Inherited = false)]
        public sealed class ResourceExposureAttribute : Attribute
        {
            private ResourceScope _resourceExposureLevel;

            public ResourceExposureAttribute(ResourceScope exposureLevel)
            {
                _resourceExposureLevel = exposureLevel;
            }

            public ResourceScope ResourceExposureLevel
            {
                get { return _resourceExposureLevel; }
            }
        }

        [Flags]
        public enum ResourceScope
        {
            None = 0,
            // Resource type
            Machine = 0x1,
            Process = 0x2,
            AppDomain = 0x4,
            Library = 0x8,
            // Visibility
            Private = 0x10,  // Private to this one class.
            Assembly = 0x20,  // Assembly-level, like C#'s "internal"
        }

        [System.Runtime.InteropServices.DllImport(ADVAPI32, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        internal unsafe static extern int RegEnumValue(Microsoft.Win32.SafeHandles.SafeRegistryHandle hKey, int dwIndex,
                    char* lpValueName, ref int lpcbValueName,
                    IntPtr lpReserved_MustBeZero, int[] lpType, byte[] lpData,
                    int[] lpcbData);

        public static List<Guid> GetSimulatedSessions()
        {
            using (new Impersonator(Config.Instance.VDAMachineAdminUsername, Config.Instance.VDAMachineAdminDomain, Config.Instance.VDAMachineAdminPassword))
            {
                return GetSimulatedSessionsInternal();
            }
        }

        private static List<Guid> GetSimulatedSessionsInternal()
        {
            var sessionKeys = new List<Guid>();

            try
            {
                string[] enumeratedSessions = null;
                string machineIp = Config.Instance.VDAMachineIp;

                bool isLocal = string.IsNullOrWhiteSpace(machineIp);
                if (!isLocal)
                {
                    Trace.Instance.WriteInfo(">> Reading registry data from remote machine {0}", machineIp);
                }

                using (var baseKey = isLocal
                                        ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                        : RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineIp, RegistryView.Registry64))
                {
                    Trace.Instance.WriteVerbose(">> >> Opened base key HKLM");

                    using (var key = baseKey.OpenSubKey(@"Software\Citrix\Ica\Session\CtxSessions", false))
                    {
                        if (key != null)
                        {
                            Trace.Instance.WriteVerbose(">> >> Opened sub key Software\\Citrix\\Ica\\Session\\CtxSessions");

                            int count = key.ValueCount;
                            Trace.Instance.WriteVerbose(">> >> There are {0} values in the key.", count);

                            int maxSessions = Config.Instance.MaxSessions;
                            int limit = count > maxSessions ? maxSessions : count;

                            unsafe
                            {
                                enumeratedSessions = new string[limit];
                                char[] name = new char[255 + 1];
                                int namelen;

                                fixed (char* namePtr = &name[0])
                                {
                                    for (int i = 0; i < limit; i++)
                                    {
                                        namelen = name.Length;

                                        int ret = RegEnumValue(
                                                        key.Handle,
                                                        i,
                                                        namePtr,
                                                        ref namelen,
                                                        IntPtr.Zero,
                                                        null,
                                                        null,
                                                        null);

                                        var strValue = new string(namePtr);
                                        Trace.Instance.WriteVerbose(">> >> Read value {0} at index {1}", i, strValue);

                                        enumeratedSessions[i] = strValue;
                                    }
                                }
                            }

                            //enumeratedSessions = key.GetValueNames(); // doesn't work on remote machine, hangs...
                            Trace.Instance.WriteVerbose(">> Read {0} simulated sessions.", enumeratedSessions != null ? enumeratedSessions.Length : 0);

                            if (enumeratedSessions != null && enumeratedSessions.Any())
                            {
                                sessionKeys.AddRange(enumeratedSessions.Select(sessionKey => new Guid(sessionKey)));
                            }
                        }
                        else
                        {
                            Trace.Instance.WriteError(">> >> Sub key Software\\Citrix\\Ica\\Session\\CtxSessions not found.");
                        }

                        Trace.Instance.WriteVerbose(">> << Disposing sub key Software\\Citrix\\Ica\\Session\\CtxSessions");
                    }

                    Trace.Instance.WriteVerbose(">> << Disposing base key HKLM");
                }
            }
            catch (Exception ex)
            {
                Trace.Instance.WriteError(">> Failed to enumerate simulated sessions. Exception: {0}", ex.Message);
            }

            Trace.Instance.WriteInfo(">> Read {0} simulated sessions from machine {1}", sessionKeys.Count, Config.Instance.VDAMachineIp);
            return sessionKeys;
        }
    }
}
