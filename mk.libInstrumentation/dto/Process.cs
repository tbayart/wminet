using System;


namespace mk.libInstrumentation.dto
{
    public class Process
    {
        public string Filename { get; set; }
        public string ExecutablePath { get; set; }
        public string CommandLine { get; set; }

        public string Name { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }

        public DateTime InstallDate { get; set; }

        public int PID { get; set; }
        public int ParentPID { get; set; }
        public string Handle { get; set; }
        public DateTime StartTime { get; set; }

        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }

        public long MemoryWorkingSet { get; set; }
        public long MemoryVirtual { get; set; }
        public int MemoryPageFileKB { get; set; }

        public DataUsage DataRead { get; set; }
        public DataUsage DataWritten { get; set; }
        public DataUsage DataOtherTransfer { get; set; }
    }


    public struct DataUsage
    {
        public long OperationCount { get; set; }
        public long Bytes { get; set; }
    }
}
