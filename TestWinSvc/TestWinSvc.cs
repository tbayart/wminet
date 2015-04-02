using System.ServiceProcess;
using System.Threading;


namespace TestWinSvc
{
    public partial class TestWinSvc : ServiceBase
    {
        public TestWinSvc()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("Starting TestWinSvc");
            Thread.Sleep(5000);
            EventLog.WriteEntry("Started TestWinSvc.");
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Stopping TestWinSvc");
            Thread.Sleep(5000);
            EventLog.WriteEntry("Stopped TestWinSvc.");
        }

        protected override void OnPause()
        {
            EventLog.WriteEntry("Stopping TestWinSvc");
            Thread.Sleep(5000);
            base.OnPause();
            EventLog.WriteEntry("Stopped TestWinSvc.");
        }

        protected override void OnContinue()
        {
            EventLog.WriteEntry("Resuming TestWinSvc");
            Thread.Sleep(5000);
            base.OnContinue();
            EventLog.WriteEntry("Resumed TestWinSvc.");
        }

        protected override void OnShutdown()
        {
            EventLog.WriteEntry("Shutting Down TestWinSvc");
            Thread.Sleep(5000);
            base.OnShutdown();
            EventLog.WriteEntry("Shut Down TestWinSvc.");
        }
    }
}
