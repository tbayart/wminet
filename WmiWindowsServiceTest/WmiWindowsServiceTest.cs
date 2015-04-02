using System;
using System.Management;
using System.Threading;
using NUnit.Framework;
using mk.libWmiNet;
using mk.libWmiNet.Facade;
using mk.libWmiNet.Facade.Service;


namespace WmiWindowsServiceTest
{
    [TestFixture]
    class WmiWindowsServiceTest
    {
        static void Main(string[] args)
        {
            
        }


        #region Standard Service Start/Stop

        [TestCase]
        public void TestStandardServiceOps()
        {
            WmiFacade wmi = new WmiFacade();
            wmi.Service.Start("TestWinSvc");
        }

        #endregion



        #region WMI Service Start Stop with Callback

        public void TestServiceOpCallback()
        {
            //Provider p = new Provider();
            ServiceOpHandler h = new ServiceOpHandler();
            ServiceMethods.ServiceMethodObserver o = new ServiceMethods.ServiceMethodObserver();
            o.ObjectPut += h.HandleWmiObjectPutOp;
            o.ObjectReady += h.HandleWmiObjectReadyOp;
            o.Progress += h.HandleWmiProgressOp;
            o.Completed += h.HandleWmiCompletedOp;

            WmiFacade wmi = new WmiFacade();
            Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Attempting op on TestWinSvc ");
            //wmi.Service.Start("TestWinSvc", o);
            //wmi.Service.Stop("TestWinSvc", o);

            while (true)
            {
                Thread.Sleep(100);
            }
        }


        public class ServiceOpHandler
        {
            public void HandleWmiObjectPutOp(object sender, ObjectPutEventArgs e)
            {
                Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Object Put: " + e);
            }

            public void HandleWmiObjectReadyOp(object sender, ObjectReadyEventArgs e)
            {
                Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Object Ready: " + e);

                ServiceMethods.ResponseCode retval = ServiceMethods.ResponseCode.ResponseCodeUninitialised;

                ManagementBaseObject o = null == e ? null : e.NewObject;

                if (null != o)
                {
                    PropertyDataCollection props = o.Properties;
                    uint r = (uint)o["ReturnValue"];
                    retval = (ServiceMethods.ResponseCode)r;
                    Console.WriteLine(" -- " + retval);
                }

                if (ServiceMethods.ResponseCode.Success == retval
                     || ServiceMethods.ResponseCode.ServiceAlreadyAtRequestedState == retval // if op=start
                     || ServiceMethods.ResponseCode.ServiceCannotAcceptControl == retval // if op=stop
                    )
                {
                    // Everything is fine
                }
                else
                {
                    // Didn't work
                }
            }

            public void HandleWmiProgressOp(object sender, ProgressEventArgs e)
            {
                Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Progress: " + e);
            }

            public void HandleWmiCompletedOp(object sender, CompletedEventArgs e)
            {
                Console.WriteLine(DateTime.UtcNow.ToString("o") + " : Completed:  " + e);
            }
        }

        #endregion WMI Service Start Stop with Callback
    }


}
