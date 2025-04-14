using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNS = DnsClient;

namespace ConnectionDiagnostic
{
    class DNSredirectDetector
    {
        public string logbook = string.Empty;
        public bool Rundetector()
        {
            StringBuilder sb = new StringBuilder();
            DNS.LookupClient Client;
            bool result = true;
            Client = new DNS.LookupClient();
            try
            {
                sb.AppendLine("DNS A Query:".PadRight(80, '_'));
                var resp = Client.Query("www.msftconnecttest.com", DNS.QueryType.A, DNS.QueryClass.IN);
                foreach (var item in resp.Answers)
                {
                    sb.AppendLine(item.ToString());
                }

            }
            catch (Exception ex)
            {

                sb.AppendLine(ex.Message);
            }

            try
            {
                sb.AppendLine("DNS PTR Query:".PadRight(80, '_'));
                var pntr = Client.Query("www.msftconnecttest.com", DNS.QueryType.PTR, DNS.QueryClass.IN);

                foreach (var item in pntr.Answers)
                {
                    sb.AppendLine(item.ToString());
                    if (item.ToString().ToLower().Contains("trafficmanager")) result = false;
                    if (item.ToString().ToLower().Contains("msftncsi")) result = false;
                    if (item.ToString().ToLower().Contains("akamai.net")) result = false;

                }
                if (result) { sb.AppendLine("It looks like DNS is being redirected."); }
                else { sb.AppendLine("It looks like DNS is NOT being redirected."); }
            }
            catch (Exception ex)
            {

                sb.AppendLine(ex.Message);
            }


            sb.AppendLine("End of DNS".PadRight(80, '-'));
            logbook = sb.ToString();
            return result;
        }

    }
}
