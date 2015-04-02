using System;
using System.Management;

namespace mk.libWmiNet
{
    public abstract class LoginCredentials
    {
        protected LoginCredentials(string username, string password)
        {
            m_username = username;
            m_password = password;
        }


        internal void ApplyToOptions(ConnectionOptions o)
        {
            if (null != o)
            {
                if ( string.IsNullOrWhiteSpace(m_username) )
                {
                    throw new Exception("Invalid username!"); // TODO: create specific exception
                }

                o.Username = m_username;
                o.Password = m_password;

                if ( !string.IsNullOrWhiteSpace(m_authority) )
                {
                    o.Authority = m_authority;
                }
            }
        }


        public override string ToString()
        {
            return "Username: " + m_username + ", Authority: " + m_authority;
        }


        protected string m_username;
        protected string m_password;
        protected string m_authority;
    }


    /// <summary>
    /// 
    /// </summary>
    public class NtlmLoginCredentials : LoginCredentials
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public NtlmLoginCredentials(string username, string password, string domain = null)
            : base(username, password)
        {
            if (!string.IsNullOrWhiteSpace(domain))
            {
                m_authority = PREFIX + domain;
            }
        }

        private const string PREFIX = "NTLMDomain:";
    }


    // TODO: Get Kerberos login working
    /// <summary>
    /// 
    /// </summary>
    public class KerberosLoginCredentials : LoginCredentials
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="principal"></param>
        public KerberosLoginCredentials(string username, string password, string principal)
            : base(username, password)
        {
            m_authority = PREFIX + principal;
        }

        private const string PREFIX = "Kerberos:";
    }
}