using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using NetInf = System.Net.NetworkInformation;

namespace ConnectionDiagnostic
{
    internal class TeamViewerMonitor
    {
        internal StringBuilder _sb = new StringBuilder();
        public string LogString = string.Empty;
        public TeamViewerMonitor()
        {
            var pcs = Process.GetProcesses();
            _sb.AppendLine(string.Empty.PadRight(80, '-'));
            _sb.AppendLine("---Looking for the TeamViewer software".PadRight(80, '-'));
            bool fnd = false;
            foreach (var p in pcs)
            {
                if (p.ProcessName.Contains("teamviewer", StringComparison.InvariantCultureIgnoreCase))
                {
                    _sb.AppendLine($" The windows process {p.ProcessName} was found.");
                    _sb.AppendLine($"Process responding? : {p.Responding}.");
                    fnd = true;
                }
            }
            if (!fnd)
            {
                _sb.AppendLine("TeamViewer is NOT running on this PC.");

            }
            LogString = _sb.ToString();
        }

        public static bool PingServer()
        {
            try
            {
                NetInf.Ping ping = new();
                NetInf.PingOptions pingOptions = new NetInf.PingOptions();
                NetInf.PingReply reply = ping.Send("www.teamvierer.com", 10000);
                if (reply != null)
                {
                    return (reply.Status == NetInf.IPStatus.Success);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false ;
            }


        }
    }
}
