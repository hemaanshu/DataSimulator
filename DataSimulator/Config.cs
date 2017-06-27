using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;

namespace DataSimulator
{
    public class Config
    {
        private static Config instance;

        private Config()
        {
        }

        public static Config Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new Config();
                }

                return instance;
            }
        }

        public int LogLevel
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings.Get("App.LogLevel"));
            }
        }

        public string ConnectionString
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("MonitorDBConnectionString");
            }
        }

        public string VDAMachineIp
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("Simulation.VDA.Machine.IpAddress");
            }
        }

        public Guid[] MachineIds
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("Simulation.VDA.Machine.Ids")
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new Guid(x))
                    .ToArray();
            }
        }

        public int Iterations
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings.Get("Simulation.Iterations"));
            }
        }

        public int MaxSessions
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings.Get("Simulation.VDA.Machine.MaxSessions"));
            }
        }

        public int SimulationTimeRange
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings.Get("Simulation.TimeRange.Days"));
            }
        }

        public bool IsVDAMachineInRemoteDomain
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings.Get("Simulation.VDA.Machine.IsRemoteDomain"));
            }
        }

        public string VDAMachineAdminUsername
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("Simulation.VDA.Machine.Admin.Username");
            }
        }

        public string VDAMachineAdminPassword
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("Simulation.VDA.Machine.Admin.Password");
            }
        }

        public string VDAMachineAdminDomain
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("Simulation.VDA.Machine.Admin.Domain");
            }
        }
    }
}
