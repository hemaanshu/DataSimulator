
namespace DataSimulator
{
    public class Program
    {
        static void Main(string[] args)
        {
            Trace.Instance.WriteInfo(">> Data simulator started...");

            ApplicationFailureMonitoring.Run();

            Trace.Instance.WriteInfo("<< Data simulator ended...");
        }
    }
}
