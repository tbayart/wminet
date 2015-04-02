using System;
using System.Management;
using System.Runtime.InteropServices;
using mk.libWmiNet.Facade.Registry;
using mk.libWmiNet.Facade.Service;


namespace mk.libWmiNet.Facade
{
    public class WmiFacade
    {
        public WmiFacade(string hostname, LoginCredentials credentials = null, bool enablePrivileges = false, int timeoutInSeconds = 0)
            : this(new Provider(hostname: hostname), credentials, enablePrivileges, timeoutInSeconds)
        {
        }

        public WmiFacade(Provider provider = null, LoginCredentials credentials = null, bool enablePrivileges = false, int timeoutInSeconds = 0)
        {
            ConnectionOptions o = new ConnectionOptions();
            o.EnablePrivileges = enablePrivileges;

            if (null != credentials)
            {
                credentials.ApplyToOptions(o);
            }

            if (null == provider)
            {
                provider = new Provider();
            }

            m_provider = provider;
            provider.ApplyArchToOptions(o);

            if (timeoutInSeconds > 0)
            {
                o.Timeout = new TimeSpan(0, 0, timeoutInSeconds);
            }


            m_scope.Path = new ManagementPath(provider.ProviderPath);
            m_scope.Options = o;

            try
            {
                m_scope.Connect();
            }
            catch (UnauthorizedAccessException x)
            {
                string msg = "Authentication failed for " + m_scope.Options.Username + " on " + m_provider.Host + ".";
                throw new WmiConnectionException(ComErrorCodes.COM_ACCESS_DENIED, msg, x);
            }
            catch(COMException x)
            {
                uint errorcode = unchecked((uint)x.ErrorCode);
                string msg = "";

                switch (errorcode)
                {
                    case ComErrorCodes.RPC_SERVER_UNAVAILABLE:
                        msg = "Check if you have a firewall running on the host. If so try opening the WMI ports ().";
                        break;
                    case ComErrorCodes.COM_ACCESS_DENIED:
                        if (!string.IsNullOrWhiteSpace(m_scope.Options.Username))
                        {
                            msg = "Could not connect to " + m_provider.Host + " with user " + m_scope.Options.Username + ".";
                        }
                        msg += "You need a user with Admin Rights or DCOM Remote Launch and Remote Activation privileges on the host PC (" + m_provider.Host + ").";
                        break;
                    case ComErrorCodes.NAMESPACE_ACCESS_DENIED:
                        msg = "Could not connect to WMI Namespace " + m_scope.Path;
                        if (!string.IsNullOrWhiteSpace(m_scope.Options.Username))
                        {
                            msg += " with user " + m_scope.Options.Username;
                        }
                        msg += ". You need a user with Admin Rights or a user that has been give access rights to this Namespace.";
                        break;
                    default:
                        msg = "COM Exception occured - Code: 0x" + errorcode.ToString("X8") + ", Message: " + x.Message;
                        break;
                }

                throw new WmiConnectionException(errorcode, msg, x);
            }
            catch (ManagementException x)
            {
                string msg = "Could not connect to " + m_provider.Host + " with user " + m_scope.Options.Username + ". Code: " +
                             x.ErrorCode + ", Message: " + x.Message;
                throw new WmiConnectionException(msg, x);
            }
            catch (Exception x)
            {
                string msg = "Could not connect to " + m_provider.Host + " with user " + m_scope.Options.Username +
                             ". Encountered " + x.GetType() + " with message: " + x.Message;
                throw new WmiConnectionException(msg, x);
            }


            m_serviceMethods = new ServiceMethods(this);
            m_registryMethods = new RegistryMethods(this);
        }


        public ServiceMethods Service { get { return m_serviceMethods; } }
        public RegistryMethods Registry { get { return m_registryMethods; } }


        internal ManagementScope Scope { get { return m_scope; } }
        internal Provider Provider { get { return m_provider; } }


        private readonly ManagementScope m_scope = new ManagementScope();
        private readonly Provider m_provider;
        private readonly ServiceMethods m_serviceMethods;
        private readonly RegistryMethods m_registryMethods;
    }


    public static class ComErrorCodes
    {
        public const uint RPC_SERVER_UNAVAILABLE = 0x800706BA;
        public const uint COM_ACCESS_DENIED = 0x80070005;
        public const uint NAMESPACE_ACCESS_DENIED = 0x80041003;
    }
}
