using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;


namespace mk.libWmiNet.Facade.Registry
{
    public class RegistryMethods
    {
        internal RegistryMethods(WmiFacade facade)
        {
            if (null == facade)
            {
                throw new WmiException("Wmi Facade not initialised.");
            }

            m_wmi = facade;

            HKeyClassesRoot = new RegHiveProxy(facade, RegistryHive.ClassesRoot);
            HKeyCurrentConfig = new RegHiveProxy(facade, RegistryHive.CurrentConfig);
            HKeyCurrentUser = new RegHiveProxy(facade, RegistryHive.CurrentUser);
            HKeyLocalMachine = new RegHiveProxy(facade, RegistryHive.LocalMachine);
            HKeyUsers = new RegHiveProxy(facade, RegistryHive.Users);
        }


        public RegHiveProxy HKeyClassesRoot { get; private set; }
        public RegHiveProxy HKeyCurrentConfig { get; private set; }
        public RegHiveProxy HKeyCurrentUser { get; private set; }
        public RegHiveProxy HKeyLocalMachine { get; private set; }
        public RegHiveProxy HKeyUsers { get; private set; }


        #region Installed Packages

        public List<PackageInfo> GetInstalledPackageList(string regexFilter = null, bool includeWoW=true)
        {
            List<PackageInfo> pkgs = new List<PackageInfo>();

            Dictionary<string, RegKeyProxy> keys = HKeyLocalMachine.GetKeysAt(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            PopulatePackageList(regexFilter, false, pkgs, keys);

            if (includeWoW &&
                    (  Provider.Architecture.X64 == m_wmi.Provider.Arch
                     || (Provider.Architecture.Default == m_wmi.Provider.Arch && IntPtr.Size == 8)
                     )
                )
            {
                Dictionary<string, RegKeyProxy> wowKeys = new Dictionary<string, RegKeyProxy>();
                uint rv = HKeyLocalMachine.EnumKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall", ref wowKeys);
                if (0 == rv)
                {
                    PopulatePackageList(regexFilter, true, pkgs, wowKeys);
                }
            }
            
            return pkgs;
        }

        private void PopulatePackageList(string regexFilter, bool isWoW, List<PackageInfo> pkgs, Dictionary<string, RegKeyProxy> keys)
        {
            foreach (KeyValuePair<string, RegKeyProxy> pair in keys)
            {
                RegKeyProxy k = pair.Value;
                PackageInfo p = new PackageInfo();
                Dictionary<string, RegistryField> fields = k.GetValues();

                p.IsWoW = isWoW;
                p.Identifier = k.KeyName;
                p.RegKeyPath = k.PathReleativeToHive + "\\" + k.KeyName;
                p.DisplayName = GetField(fields, "DisplayName") as string ?? string.Empty;

                if ( !string.IsNullOrWhiteSpace(regexFilter) &&
                     !Regex.IsMatch(p.DisplayName, regexFilter, RegexOptions.IgnoreCase) &&
                     !Regex.IsMatch(p.Identifier, regexFilter, RegexOptions.IgnoreCase)
                    )
                {
                    continue;
                }
                
                p.DisplayVersion = GetField(fields, "DisplayVersion") as string;
                p.Comments = GetField(fields, "Comments") as string;
                p.InstallLocation = GetField(fields, "InstallLocation") as string;
                p.InstallSource = GetField(fields, "InstallSource") as string;
                p.InstallDate = GetField(fields, "InstallDate") as string;
                p.UninstallString = GetField(fields, "UninstallString") as string;
                p.QuietUninstallString = GetField(fields, "QuietUninstallString") as string;
                p.ModifyPath = GetField(fields, "ModifyPath") as string;

                pkgs.Add(p);
            }
        }
        private object GetField(Dictionary<string, RegistryField> fields, string name)
        {
            return fields.ContainsKey(name) ? fields[name].Value : null;
        }

        #endregion Installed Packages


        private readonly WmiFacade m_wmi;
    }


    public struct RegistryField
    {
        public string Name { get; set; }
        public RegistryValueKind ValueKind { get; set; }
        public object Value { get; set; }
    }


    public struct PackageInfo
    {
        public string Identifier { get; set; }
        public string RegKeyPath { get; set; }
        public bool IsWoW { get; set; }
        public bool IsSystemComponent { get; set; }
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public string Comments { get; set; }
        public string InstallLocation { get; set; }
        public string InstallSource { get; set; }
        public string InstallDate { get; set; }
        public string UninstallString { get; set; }
        public string QuietUninstallString { get; set; }
        public string ModifyPath { get; set; }
    }
}
