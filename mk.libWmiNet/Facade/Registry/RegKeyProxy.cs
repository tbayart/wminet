using System;
using System.Collections.Generic;
using Microsoft.Win32;


namespace mk.libWmiNet.Facade.Registry
{
    public class RegKeyProxy
    {
        public RegKeyProxy(RegHiveProxy regHiveProxy, string pathReleativeToHive, string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                throw new ArgumentException("The Keyname must be non-null and non-empty.");
            }

            m_regHiveProxy = regHiveProxy;
            HiveKey = regHiveProxy.HiveKey;
            PathReleativeToHive = pathReleativeToHive ?? string.Empty;
            KeyName = keyName;

            if (PathReleativeToHive.Length > 0)
            {
                m_fullPath = PathReleativeToHive + "\\" + KeyName;
            }
            else
            {
                m_fullPath = keyName;
            }
        }

        public RegistryHive HiveKey { get; private set; }
        public string PathReleativeToHive { get; private set; }
        public string KeyName { get; private set; }


        public Dictionary<string, RegKeyProxy> GetChildren()
        {
            return m_regHiveProxy.GetKeysAt(m_fullPath);
        }


        public Dictionary<string, RegistryField> GetValues()
        {
            return m_regHiveProxy.GetValuesAt(m_fullPath);
        }


        private readonly RegHiveProxy m_regHiveProxy;
        private readonly string m_fullPath;
    }
}
