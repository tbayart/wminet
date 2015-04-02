using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using NUnit.Framework;
using mk.libWmiNet;
using mk.libWmiNet.Facade;
using mk.libInstrumentation.dto;


namespace WmiLibTests
{
    class WmiLibTests
    {
        static void Main(string[] args)
        {
        }


        [TestCase]
        public static void TestRemoteEventLog()
        {
            //Provider p = new Provider(Provider.Namespace.CIMv2, "", null);
            EventLog log = new EventLog("Application", "hostname", "TestSource");
            log.WriteEntry("Blah");
        }


        [TestCase]
        public static void TestWmiRegQueries()
        {
            WmiFacade wmi = new WmiFacade();
            wmi.Registry.GetInstalledPackageList();
        }


        [TestCase]
        public static void TestHostQueries()
        {
            /*
            ObjectQuery q = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            using (ManagementObjectSearcher ws = new ManagementObjectSearcher(scope, q))
            {
                foreach (ManagementObject o in ws)
                {
                    string a;
                    a = o["TotalVisibleMemorySize"];
                    a = o["FreePhysicalMemory"];
                    a = o["FreeVirtualMemory"];
                    a = o["LocalDateTime"];
                }
            }


            q = new ObjectQuery("SELECT * FROM Win32_ComputerSystem");
            using (ManagementObjectSearcher ws = new ManagementObjectSearcher(scope, q))
            {
                foreach (ManagementObject o in ws)
                {
                    string a = o["TotalPhysicalMemory"];
                }
            }
            */
        }


        [TestCase]
        public static void TestProcessQueries()
        {
            /*
            // File
            ManagementPath dfp = new ManagementPath("CIM_DataFile.Name='c:\blah.exe'");
            ManagementObject o = new ManagementObject(scope, q, null);
            o.Get();
            string date = o["CreationDate"] as string;
            */
        }

        
        [TestCase]
        public static void TestServiceController()
        {
            WmiFacade wmi = GetLocalConnection();
            //WmiFacade wmi = GetRemoteConnection();
            
            List<Service> sl = wmi.Service.GetList();
            Console.WriteLine(sl);

            ServiceController c = new ServiceController("TestWinSvc", "localhost");

            Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Running ...");

            try
            {
                TimeSpan t = new TimeSpan(0,0,0,10);
                switch (c.Status)
                {
                    case ServiceControllerStatus.Stopped:
                        c.Start();
                        c.WaitForStatus(ServiceControllerStatus.Running, t);
                        break;
                    case ServiceControllerStatus.Running:
                        c.Stop();
                        c.WaitForStatus(ServiceControllerStatus.Stopped, t);
                        break;
                }
            }
            catch (Exception x)
            {
                Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Encountered " + x.GetType() + " : " + x.Message);
            }

            Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Done.");
            Console.WriteLine(".");
        }


        static WmiFacade GetLocalConnection()
        {
            return new WmiFacade();
        }

        static WmiFacade GetRemoteConnection()
        {
            Provider p = new Provider(Provider.Namespace.CIMv2, "bcvm");
            LoginCredentials l = new NtlmLoginCredentials("userx", "letmein", "bcvm");
            return new WmiFacade(p, l, true);
        }
    }
}
