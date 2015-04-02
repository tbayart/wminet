using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.Serialization;
using System.ServiceProcess;
using mk.libWmiNet.Proxy.Win32;


namespace mk.libWmiNet.Facade.Service
{
    public class ServiceMethods
    {
        internal ServiceMethods(WmiFacade facade)
        {
            if (null == facade)
            {
                throw new WmiException("Wmi Facade not initialised.");
            }

            m_facade = facade;
        }


        /// <summary>
        /// Start a Windows Service.
        /// </summary>
        /// <param name="serviceName">Name of the service to start.</param>
        /// <param name="waitUntilStartCompleted">Optional Parameter. If set to true, this method will not return until service has started, unless there is an exception. By default it will not wait.</param>
        /// <param name="timeoutInSeconds">Optional Parameter. 0 means wait forever.</param>
        public void Start(string serviceName, bool waitUntilStartCompleted = false, int timeoutInSeconds = 0)
        {
            ServiceController sc = GetServiceController(serviceName);
            if (ServiceControllerStatus.Running != sc.Status)
            {
                sc.Start();
                WaitIfNeeded(waitUntilStartCompleted, timeoutInSeconds, sc, ServiceControllerStatus.Running);
            }
        }

        /// <summary>
        /// Stop a Windows Service.
        /// </summary>
        /// <param name="serviceName">Name of the service to stop.</param>
        /// <param name="waitUntilStopCompleted">Optional Parameter. If set to true, this method will not return until service has stopped, unless there is an exception. By default it will not wait.</param>
        /// <param name="timeoutInSeconds">Optional Parameter. 0 means wait forever.</param>
        public void Stop(string serviceName, bool waitUntilStopCompleted = false, int timeoutInSeconds = 0)
        {
            ServiceController sc = GetServiceController(serviceName);
            if (ServiceControllerStatus.Stopped != sc.Status)
            {
                sc.Start();
                WaitIfNeeded(waitUntilStopCompleted, timeoutInSeconds, sc, ServiceControllerStatus.Stopped);
            }
        }


        private ServiceController GetServiceController(string serviceName)
        {
            if (string .IsNullOrWhiteSpace(serviceName))
            {
                throw new ServiceMethodException("The provided servicename was null or empty.");
            }

            try
            {
                return m_facade.Provider.IsLocalhost
                    ? new ServiceController(serviceName)
                    : new ServiceController(serviceName, m_facade.Provider.Host)
                    ;
            }
            catch (Exception x)
            {
                throw new ServiceMethodException("Encountered " + x.GetType() + " while obtaining service " + serviceName + " on " + m_facade.Provider.Host, x);
            }
        }

        private void WaitIfNeeded(bool wait, int timeout, ServiceController sc, ServiceControllerStatus targetStatus)
        {
            if (wait)
            {
                if (timeout > 0)
                {
                    TimeSpan ts = new TimeSpan(0,0,0,timeout);
                    sc.WaitForStatus(targetStatus, ts);                    
                }
                else
                {
                    sc.WaitForStatus(targetStatus);
                }
            }
        }


        private void InvokeMethod(string serviceName, ServiceOperation op, ServiceMethodObserver o)
        {
            if ( !string.IsNullOrWhiteSpace(serviceName) )
            {
                ManagementPath path = new ManagementPath("Win32_Service.Name='" + serviceName + "'");
                ManagementObject obj = new ManagementObject(m_facade.Scope, path, new ObjectGetOptions());

                try
                {
                    obj.InvokeMethod(o, op.ToString(), new object[0]);
                }
                catch (ManagementException x)
                {
                    string msg = "Envountered " + x.GetType() + " while attempting operation " + op + " on service " + serviceName + " at " + m_facade.Scope.Path;
                    throw new ServiceMethodException(msg, x);
                }
            }
        }



        #region Service data and proxy

        public ServiceProxy GetServiceProxy(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return null;
            }

            string condition = "Name=" + serviceName;
            ServiceProxy.ServiceCollection sc = ServiceProxy.GetInstances(m_facade.Scope, condition);

            if (sc.Count > 0)
            {
                foreach (ServiceProxy s in sc)
                {
                    s.StartService();
                    if (s.Name == serviceName)
                    {
                        return s;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="getProcessData"></param>
        /// <param name="getFileData"></param>
        /// <returns></returns>
        public List<libInstrumentation.dto.Service> GetList(bool getProcessData = false, bool getFileData = false)
        {
            return GetList(null, getProcessData, getFileData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matchCondition"></param>
        /// <param name="getProcessData"></param>
        /// <param name="getFileData"></param>
        /// <returns></returns>
        public List<libInstrumentation.dto.Service> GetList(string matchCondition, bool getProcessData = false, bool getFileData = false)
        {
            List<libInstrumentation.dto.Service> services = new List<libInstrumentation.dto.Service>();

            ServiceProxy.ServiceCollection svcCol = string.IsNullOrWhiteSpace(matchCondition)
                ? ServiceProxy.GetInstances(m_facade.Scope, (EnumerationOptions)null)
                : ServiceProxy.GetInstances(m_facade.Scope, matchCondition)
                ;

            foreach (ServiceProxy ws in svcCol)
            {
                libInstrumentation.dto.Service s = new libInstrumentation.dto.Service();
                services.Add(s);

                s.Name = ws.Name;
                s.DisplayName = ws.DisplayName;
                s.Caption = ws.Caption;
                s.Description = ws.Description;
                s.FilePath = ws.PathName;
                s.PID = ws.ProcessId;
                s.IsRunning = ws.Started;
                s.State = ws.State;
                s.LifecycleCheckpoint = ws.CheckPoint;
                s.ErrorcControl = ws.ErrorControl;
                s.ExitCode = ws.ExitCode;
                s.ServiceSpecificExitCode = ws.ServiceSpecificExitCode;
                s.InstallDate = ws.InstallDate;
                s.RunningAsUser = ws.StartName;
            }

            return services;
        }

        #endregion Service data and proxy



        private readonly WmiFacade m_facade;


        #region TypeDefs

        public enum ServiceOperation
        {
            StartService,
            StopService
        }


        public enum ResponseCode
        {
            ResponseCodeUninitialised = -1,
            Success = 0,
            NotSupported,
            AccessDenied,
            DependentServicesRunning,
            InvalidServiceControl,
            ServiceCannotAcceptControl,
            ServiceNotActive,
            ServiceRequestTimeout,
            UnknownFailure,
            PathNotFound,
            ServiceAlreadyAtRequestedState,
            ServiceDatabaseLocked,
            ServiceDependencyDeleted,
            ServiceDependencyFailure,
            ServiceDisabled,
            ServiceLogonFailed,
            ServiceMarkedForDeletion,
            ServiceNoThread,
            StatusCircularDependency,
            StatusDuplicateName,
            StatusInvalidName,
            StatusInvalidParameter,
            StatusInvalidServiceAccount,
            StatusServiceExists,
            ServiceAlreadyPaused
        }

        public class ServiceMethodObserver : ManagementOperationObserver { }

        #endregion TypeDefs
    }



    [Serializable]
    public class ServiceMethodException : WmiException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //
        /*
        public ServiceMethodException(uint code, string message = null, Exception inner = null)
            : this(message, inner)
        {
            m_errorCode = code;
        }
        */
        public ServiceMethodException(string message = null, Exception inner = null)
            : base(message, inner)
        {
        }

        protected ServiceMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public uint ErrorCode { get { return m_errorCode; } }


        private readonly uint m_errorCode = 0;
    }
}
