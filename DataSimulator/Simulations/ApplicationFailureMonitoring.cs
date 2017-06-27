using System;
using System.Collections.Generic;
using System.Linq;

namespace DataSimulator
{
    public class ApplicationFailureMonitoring
    {
        public static void Run()
        {
            INIT:
            Trace.Instance.WriteInfo(">> ApplicationFailureMonitoring: Initializing simulation data...");
            var rnd = new Random();

            // Prepare table schema.
            var faultDataTable = DBUtil.GetDataTable(typeof(ApplicationFault));
            var errorDataTable = DBUtil.GetDataTable(typeof(ApplicationError));

            //var sessions = SessionUtil.GetSimulatedSessions();
            var sessions = new List<Guid>() { Guid.Empty };

            START:
            Trace.Instance.WriteInfo(">> ApplicationFailureMonitoring: Preparing data...");

            if (sessions.Any())
            {
                var appFaults = new List<ApplicationFault>();
                var appErrors = new List<ApplicationError>();

                int max = Config.Instance.MachineIds.Count();
                int count = Config.Instance.Iterations;
                while (count > 0)
                {
                    // Prepare data
                    foreach (var session in sessions)
                    {
                        var appFault = new ApplicationFault();
                        appFault.Initialize(rnd, Config.Instance.MachineIds[rnd.Next(0, max)], session);
                        appFaults.Add(appFault);

                        var appError = new ApplicationError();
                        appError.Initialize(rnd, Config.Instance.MachineIds[rnd.Next(0, max)], session);
                        appErrors.Add(appError);
                    }

                    --count;
                }

                DBUtil.FillData<ApplicationFault>(appFaults, faultDataTable);
                DBUtil.FillData<ApplicationError>(appErrors, errorDataTable);

                DBUtil.BulkInsert(faultDataTable);
                DBUtil.BulkInsert(errorDataTable);
            }
            else
            {
                Trace.Instance.WriteWarning(">> ApplicationFailureMonitoring: Found NO simulated sessions to insert data.");
            }

            // Next round

            Trace.Instance.WriteInfo(">> ApplicationFailureMonitoring: Data simulation finished. \n\n\tPress: \n\t'I' to re-initialize \n\t'R' to insert more data or, \n\tAny other key to close the run.");
            string key = Console.ReadLine();
            switch (key)
            {
                case "r":
                case "R":
                    Console.Clear();
                    goto START;

                case "i":
                case "I":
                    Console.Clear();
                    goto INIT;
            }
        }
    }
}
