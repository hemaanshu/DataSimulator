using System;
using System.Collections.Generic;

namespace DataSimulator
{
    public class ApplicationError : IInit
    {
        //public Int64 Id { get; set; }
        public string FaultingApplicationPath { get; set; }
        public string ProcessName { get; set; }
        public Guid SessionKey { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public DateTime ErrorReportedDate { get; set; }
        public string BrowserNames { get; set; }
        public Guid MachineId { get; set; }

        public void Initialize(Random rnd, Guid machineId, Guid sessionKey)
        {
            var apps = new List<AppData>()
            {
                new AppData() { Name = "Notepad", Path = @"C:\Windows\System32\notepad.exe"},
                new AppData() { Name = "Calculator", Path = @"C:\Windows\System32\calc.exe"},
                new AppData() { Name = "Command Prompt", Path = @"C:\Windows\System32\cmd.exe"},
                new AppData() { Name = "Wordpad", Path = @"C:\Windows\System32\wordpad.exe"},
            };

            var appData = apps[rnd.Next(0, 4)];
            var severity = Convert.ToBoolean(rnd.Next(0, 2));

            this.SessionKey = sessionKey;
            this.MachineId = machineId;

            this.FaultingApplicationPath = appData.Path;
            this.ProcessName = appData.Name;
            this.Version = (new Version(10, 0, 14393, 594)).ToString();
            this.ErrorReportedDate = DateTime.UtcNow;
            this.BrowserNames = "Notepad, Calculator, Command, Wordpad";
            this.Description = "Faulting application name: " + appData.Name +
                                    "version: 10.0.14393.594" +
                                    "time stamp: 0x5850c96b" +
                                    "Faulting module name: ntdll.dll" +
                                    "version: 10.0.14393.479" +
                                    "time stamp: 0x5825887f" +
                                    "Exception code: 0xc0000008" +
                                    "Fault offset: 0x00000000000a9d2a" +
                                    "Faulting process id: 0x4c7c" +
                                    "Faulting application start time: 0x01d2803f35911a61" +
                                    "Faulting application path: " + appData.Path +
                                    "Faulting module path: C:\\WINDOWS\\SYSTEM32\\ntdll.dll" +
                                    "Report Id: c453528c-bdb3-4995-8d2a-a125bcab82be" +
                                    "Faulting package full name: " +
                                    "Faulting package-relative application ID";
        }
    }
}
