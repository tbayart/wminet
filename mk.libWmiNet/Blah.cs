using System;
using System.Collections.Generic;
using System.Management;
using mk.libInstrumentation.dto;


namespace mklib.wminet
{
    public class Blah
    {
        public void MakeCall()
        {
            
        }


        public static List<Service> GetServiceList(string hostname="localhost", string queryClause="", bool fullDetails=false)
        {
            List<Service> services = new List<Service>();

            ConnectionOptions co = new ConnectionOptions();
            //co.Username = "";
            //co.Password = "";
            //co.Authority = "ntlmdomain:INTRANET";
            //co.EnablePrivileges = true;
            //co.Context.Add("__ProviderArchitecture", 64);
            //co.Context.Add("__RequiredArchitecture", true);

            //string path = @"\\" + hostname + @"\root\default:StdRegProv";
            string path = @"\\" + hostname + @"\root\CIMV2";

            try
            {
                ManagementScope scope = new ManagementScope(path, co);
                scope.Connect();


                // select * from Win32_ComputerSystem where Name like 'blah%'
                // select * from Win32_OperatingSystem

                string queryStr = "select * from Win32_Service";
                if ( !string.IsNullOrWhiteSpace(queryClause) )
                {
                    queryStr += " where " + queryClause;
                }
                
                ObjectQuery query = new ObjectQuery(queryStr);
                //WqlObjectQuery

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        Service s = new Service();
                        services.Add(s);

                        s.Name = (string) mo["Name"];
                    }
                }

                return services;
            }
            catch (Exception)
            {
                //log blah
                throw;
            }
        }
    }
}
