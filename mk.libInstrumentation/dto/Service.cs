using System;


namespace mk.libInstrumentation.dto
{
    public class Service
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }

        public uint PID { get; set; }
        public Process RuntimeInstance { get; set; }

        public bool IsRunning { get; set; }
        public string State { get; set; }

        public uint LifecycleCheckpoint { get; set; }
        public string ErrorcControl { get; set; }
        public uint ExitCode { get; set; }
        public uint ServiceSpecificExitCode { get; set; }

        public DateTime InstallDate { get; set; }

        public string RunningAsUser { get; set; }
    }


    public enum ServiceState
    {
        NotApplicable = 0,
        Stopped = 100,
        StartPending,
        StopPending,
        Running,
        ContinuePending,
        PausePending,
        Paused,
        Unknown
    }
}
