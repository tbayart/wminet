using System.Management;
using mk.libUtils;


namespace mk.libWmiNet
{
    public class Provider
    {
        public Provider(Namespace ns = Namespace.CIMv2, string hostname = _LOCALHOST, Architecture arch = Architecture.Default, bool onlyTrySpecifiedArch=true)
        {
            if (string.IsNullOrWhiteSpace(hostname) || _LOCALHOST == hostname)
            {
                m_providerPath = EnumExtentions.GetCaption(ns);
            }
            else
            {
                m_isLocalhost = false;
                m_host = hostname;
                m_providerPath = @"\\" + hostname + @"\" + EnumExtentions.GetCaption(ns);
            }

            m_arch = arch;
            m_onlyTrySpecifiedArch = onlyTrySpecifiedArch;
        }


        internal void ApplyArchToOptions(ConnectionOptions o)
        {
            if (null != o)
            {
                if (Architecture.Default != m_arch)
                {
                    o.Context.Add("__ProviderArchitecture", Architecture.X64==m_arch ? 64 : 32);
                    o.Context.Add("__RequiredArchitecture", m_onlyTrySpecifiedArch);
                }
            }
        }


        internal bool IsLocalhost { get { return m_isLocalhost; } }
        internal string Host { get { return m_host; } }
        internal string ProviderPath { get { return m_providerPath; } }
        internal Architecture Arch { get { return m_arch; } }


        protected readonly bool m_isLocalhost = true;
        protected readonly string m_host = _LOCALHOST;
        protected readonly string m_providerPath;
        protected readonly Architecture m_arch;
        protected readonly bool m_onlyTrySpecifiedArch;

        private const string _LOCALHOST = "localhost";


        #region Enum Definitions
        public enum Architecture
        {
            Default,
            X64,
            X86
        }

        public enum Namespace
        {
            [Caption(@"root\CIMv2")]
            CIMv2,
            
            [Caption(@"root\Default")]
            Default,
            
            [Caption(@"root\Default:StdRegProv")]
            StdRegProv,
        }
        #endregion Enum Definitions
    }
}
