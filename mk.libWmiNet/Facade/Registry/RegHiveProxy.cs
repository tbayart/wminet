using System;
using System.Collections.Generic;
using System.Management;
using Microsoft.Win32;


namespace mk.libWmiNet.Facade.Registry
{
    public class RegHiveProxy
    {
        internal RegHiveProxy(WmiFacade facade, RegistryHive hiveKey)
        {
            if (null == facade)
            {
                throw new WmiException("Wmi Facade not initialised.");
            }
            
            m_facade = facade;
            m_hiveKey = hiveKey;

            try
            {
                m_registry = new ManagementClass(m_facade.Scope, m_stdRegProvider, null);
            }
            catch (ManagementException x)
            {
                throw new WmiException("Encountered " + x.GetType() + " while initializing Registry Provider.\nCode: " + x.ErrorCode + "\nInfo: " + x.ErrorInformation, x);
            }
            catch (Exception x)
            {
                throw new WmiException("Encountered " + x.GetType() + " while initializing Registry Provider.");
            }
        }




        public Dictionary<string, RegKeyProxy> GetKeysAt(string keyPath)
        {
            string[] keys;
            uint rv = EnumKey(keyPath, out keys);

            Dictionary<string, RegKeyProxy> proxies = new Dictionary<string, RegKeyProxy>();

            if (0 != rv)
            {
                throw new RegistryException(rv, "Encountered error while getting keys at path " + m_hiveKey + "\\" + keyPath);
            }
            else
            {
                foreach (string key in keys)
                {
                    proxies[key] = new RegKeyProxy(this, keyPath, key);
                }
            }

            return proxies;
        }


        public uint EnumKey(string keyPath, ref Dictionary<string, RegKeyProxy> keys)
        {
            string[] keyNames;
            uint rv = EnumKey(keyPath, out keyNames);

            if (0 == rv)
            {
                foreach (string k in keyNames)
                {
                    keys[k] = new RegKeyProxy(this, keyPath, k);
                }
            }

            return rv;
        }

        public uint EnumKey(string keyPath, ref List<RegKeyProxy> keys)
        {
            string[] keyNames;
            uint rv = EnumKey(keyPath, out keyNames);

            if (0 == rv)
            {
                foreach (string k in keyNames)
                {
                    keys.Add( new RegKeyProxy(this, keyPath, k) );
                }
            }

            return rv;
        }


        /// <summary>
        /// This is basically the WMI StdRegProv.EnumKey method
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public uint EnumKey(string keyPath, out string[] keys)
        {
            const string enumKey = "EnumKey", names = "sNames", retval = "ReturnValue";

            ManagementBaseObject inParams = m_registry.GetMethodParameters(enumKey);
            inParams["hDefKey"] = (uint)m_hiveKey;
            inParams["sSubKeyName"] = keyPath;
            ManagementBaseObject outParams = m_registry.InvokeMethod(enumKey, inParams, null);

            if (null == outParams)
            {
                throw new WmiException("The output-parameters object returned by the EnumKey WMI method was null.");
            }

            keys = outParams.Properties[names].Value as string[];

            try
            {
                return Convert.ToUInt32(outParams.Properties[retval].Value);
            }
            catch (Exception)
            {
                return uint.MaxValue;
            }
        }


        /// <summary>
        /// This will get all the fields and values at the given registry path in this hive
        /// </summary>
        /// <param name="keyPath"></param>
        /// <returns></returns>
        public Dictionary<string, RegistryField> GetValuesAt(string keyPath)
        {
            Dictionary<string, RegistryField> fields = new Dictionary<string, RegistryField>();

            string[] fieldNames;
            int[] fieldTypes;
            uint retval = EnumValues(keyPath, out fieldNames, out fieldTypes);
            if (0 == retval)
            {
                if (null != fieldNames && fieldNames.Length > 0)
                {
                    if (fieldNames.Length != fieldTypes.Length)
                    {
                        throw new WmiException("The WMI EnumValues did not return a matching number of field names (" + fieldNames.Length + ") and types (" + fieldTypes.Length + ").");
                    }

                    for (int i = 0; i < fieldNames.Length; ++i)
                    {
                        RegistryField f = new RegistryField();
                        
                        f.Name = fieldNames[i];
                        int ft = fieldTypes[i];

                        if (Enum.IsDefined(typeof(RegistryValueKind), ft))
                        {
                            f.ValueKind = (RegistryValueKind)ft;
                        }

                        f.Value = GetValue(keyPath, f.ValueKind, f.Name);

                        fields[f.Name] = f;
                    }
                }
            }
            else if (uint.MaxValue == retval)
            {
                throw new WmiException("The WMI EnumValues method returned with ErrorCode " + retval);
            }
            else
            {
                throw new WmiException("The WMI EnumValues method did not return an ErroCode.");
            }

            return fields;
        }


        public uint EnumValues(string keyPath, out string[] fieldNames, out int[] fieldTypes)
        {
            const string
                enumValues = "EnumValues",
                names = "sNames",
                types = "Types",
                retval = "ReturnValue";

            ManagementBaseObject inParams = m_registry.GetMethodParameters(enumValues);
            inParams["hDefKey"] = (uint)m_hiveKey;
            inParams["sSubKeyName"] = keyPath;
            ManagementBaseObject outParams = m_registry.InvokeMethod(enumValues, inParams, null);

            if (null == outParams)
            {
                throw new WmiException("The output-parameters object returned by the EnumKey WMI method was null.");
            }

            fieldNames = outParams.Properties[names].Value as string[];
            fieldTypes = outParams.Properties[types].Value as int[];

            try
            {
                return Convert.ToUInt32(outParams.Properties[retval].Value);
            }
            catch (Exception)
            {
                return uint.MaxValue;
            }
        }


        public string GetStringValue(string keyPath, string field)
        {
            try
            {
                return (string)GetValue(keyPath, new RegMethodInfo(_GET_STRING_VAL, _STRING), field);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string GetExpandedStringValue(string keyPath, string field)
        {
            try
            {
                return (string)GetValue(keyPath, new RegMethodInfo(_GET_EXPANDED_STRING_VAL, _STRING), field);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        internal object GetValue(string keyPath, RegMethodInfo methodInfo, string field)
        {
            ManagementBaseObject inParams = m_registry.GetMethodParameters(methodInfo.MethodName);
            inParams["hDefKey"] = (uint)m_hiveKey;
            inParams["sSubKeyName"] = keyPath;
            inParams["sValueName"] = field;
            ManagementBaseObject outParams = m_registry.InvokeMethod(methodInfo.MethodName, inParams, null);

            if (null == outParams)
            {
                return null;
            }

            try
            {
                return outParams.Properties[methodInfo.TypeName].Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        

        private object GetValue(string keyPath, RegistryValueKind valueKind, string field)
        {
            return GetValue(keyPath, GetMethodInfoFromValueKind(valueKind), field);
        }

        private RegMethodInfo GetMethodInfoFromValueKind(RegistryValueKind valueType)
        {
            switch (valueType)
            {
                case RegistryValueKind.String:
                    return new RegMethodInfo(_GET_STRING_VAL, _STRING);
                case RegistryValueKind.ExpandString:
                    return new RegMethodInfo(_GET_EXPANDED_STRING_VAL, _STRING);
                case RegistryValueKind.MultiString:
                    return new RegMethodInfo(_GET_MULTI_STRING_VAL, _STRING_ARRAY);
                case RegistryValueKind.Binary:
                    return new RegMethodInfo(_GET_BINARY_VAL, _BYTE_ARRAY);
                case RegistryValueKind.DWord:
                    return new RegMethodInfo(_GET_DWORD_VAL, _UINT);
                case RegistryValueKind.QWord:
                    return new RegMethodInfo(_GET_QWORD_VAL, _UINT);
                default:
                    throw new WmiException("Unknown Registry DataType " + valueType + ".");
            }
        }

        internal struct RegMethodInfo
        {
            internal RegMethodInfo(string methodName, string typeName)
            {
                m_method = methodName;
                m_type = typeName;
            }

            public string MethodName { get { return m_method; } }
            public string TypeName { get { return m_type; } }

            private readonly string m_method;
            private readonly string m_type;
        }





        public void BlahGetInstalledComponents()
        {
            // TODO: Does this make a difference, or does it depend just on the provider?
            const string softwareNode = @"Software";
            //const string softwareNodeWow = @"Software\Wow6432Node";

            //const string swUninstallSubnode = @"\Microsoft\Windows\CurrentVersion\Uninstall";
            const string swUninstallSubnode = @"\Asus";

            //ManagementClass reg = new ManagementClass(@"\\" + m_facade.Provider.Host + @"\root\Default", _REGISTRY_PROVIDER, null);

            RegistryHive hklm = RegistryHive.LocalMachine;
            const string subKey = softwareNode + swUninstallSubnode;


            // Set up in+out params and call EnumKey
            //
            string method = "EnumKey";
            ManagementBaseObject inParams = m_registry.GetMethodParameters(method);
            inParams["hDefKey"] = (uint)hklm;
            inParams["sSubKeyName"] = subKey;
            ManagementBaseObject outParams = m_registry.InvokeMethod(method, inParams, null);
            if (null == outParams)
            {
                return;
            }

            string[] props = outParams.Properties["sNames"].Value as string[];
            if (null == props)
            {
                return;
            }


            foreach (string pkgID in props)
            {
                string pkgKeyPath = subKey + "\\" + pkgID;

                // Set up in+out params and call GetStringValue
                //
                //string method = "GetExpandedStringValue";
                const string method2 = "GetStringValue";
                //const string field = "DisplayName";
                const string field = "Version";
                ManagementBaseObject inParams2 = m_registry.GetMethodParameters(method2);
                inParams2["hDefKey"] = (uint)hklm;
                inParams2["sSubKeyName"] = pkgKeyPath;
                inParams2["sValueName"] = field;
                ManagementBaseObject outParams2 = m_registry.InvokeMethod(method2, inParams2, null);

                string pkgName = (null == outParams2)
                                     ? string.Empty
                                     : (string)outParams2.Properties["sValue"].Value;

                // Other StringValue fields: DisplayVersion, InstallDate
                // Other ExpandedStringValue fields: InstallSource, UninstallString, QuietUninstallString
            }

            Console.WriteLine("Done.");
        }



        public void BlahGetSubKeyValues()
        {
            ManagementPath stdRegProvider = new ManagementPath(_REGISTRY_PROVIDER);
            ManagementClass reg = new ManagementClass(m_facade.Scope, stdRegProvider, null); // NOTE: Can throw exception
            RegistryHive givenHive = RegistryHive.LocalMachine;
            string method = "EnumValues";
            string subKey = "";

            ManagementBaseObject inParams = reg.GetMethodParameters(method);
            inParams["hDefKey"] = (uint)givenHive;
            inParams["sSubKeyName"] = subKey;
            ManagementBaseObject outParams = reg.InvokeMethod(method, inParams, null);
            if (null == outParams)
            {
                return;
            }
            string[] props = outParams.Properties["sNames"].Value as string[];
            if (null == props)
            {
                return;
            }

            foreach (string valName in props)
            {
                string regKeyVal = valName + " : " + "GetExpandedStringValue(valName)";
            }
        }


        public RegistryHive HiveKey { get { return m_hiveKey; } }


        private readonly WmiFacade m_facade;
        private readonly RegistryHive m_hiveKey;
        private readonly ManagementPath m_stdRegProvider = new ManagementPath(_REGISTRY_PROVIDER);
        private readonly ManagementClass m_registry;

        private const string _REGISTRY_PROVIDER = "StdRegProv";

        private const string _GET_BINARY_VAL = "GetBinaryValue";
        private const string _GET_DWORD_VAL = "GetDWORDValue";
        private const string _GET_QWORD_VAL = "GetQWORDValue";
        private const string _GET_STRING_VAL = "GetStringValue";
        private const string _GET_EXPANDED_STRING_VAL = "GetExpandedStringValue";
        private const string _GET_MULTI_STRING_VAL = "GetMultiStringValue";

        private const string _STRING = "sValue";
        private const string _STRING_ARRAY = "sValue";
        private const string _UINT = "uValue";
        private const string _BYTE_ARRAY = "uValue";
    }
}
