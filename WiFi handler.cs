using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WiFi = ManagedNativeWifi.NativeWifi;

namespace ConnectionDiagnostic
{
    internal class WiFi_handler
    {
        public string WifiScanLog = string.Empty;
        public bool WifiIsConnected = false;
        private bool _debug = false;
        public string WifiSSID = string.Empty;
        public Guid InterfaceID = Guid.Empty;
        public WiFi_handler(bool debug = false)
        {
            StringBuilder sb = new StringBuilder();
            _debug = debug;
            if (string.IsNullOrWhiteSpace(LoginInfo.SSID))
            {
                sb.AppendLine($"Preferred SSID: <none specified>");
            }
            else
            {
                sb.AppendLine($"Preferred SSID: {LoginInfo.SSID}");
                this.WifiSSID = LoginInfo.SSID;
            }
            Task scan = WiFi.ScanNetworksAsync(TimeSpan.FromSeconds(10));
            System.Collections.Generic.List<ManagedNativeWifi.InterfaceInfo> infs = new List<ManagedNativeWifi.InterfaceInfo>();


            try
            {
                infs.AddRange(WiFi.EnumerateInterfaces());
            }
            catch (Exception ex)
            {
                sb.AppendLine("No functioning WiFi receivers were found on this system.");
                sb.AppendLine($"Error message: {ex.Message}, ({ex?.InnerException?.Message.TrimEnd()})");

                WifiScanLog = sb.ToString();
                return;
            }

            if (infs.Count() < 1)
            {
                sb.AppendLine("No functioning WiFi receivers were found on this system.");
            }
            else
            {

                sb.AppendLine("List of available WiFi receivers on this PC:".PadRight(80, '-'));
                foreach (var i in infs)
                {
                    sb.Append($"- {i.Description}; state: ");
                    if (i.State == ManagedNativeWifi.InterfaceState.Connected)
                    {
                        sb.AppendLine("connected.");
                        this.WifiIsConnected = true;
                        this.InterfaceID = i.Id;
                    }
                    else
                    {
                        sb.AppendLine($"not connected: {Enum.GetName(typeof(ManagedNativeWifi.InterfaceState), i.State)}");
                        if (this.InterfaceID == Guid.Empty) this.InterfaceID = i.Id;
                    }
                }
            }
            sb.AppendLine("-".PadRight(80, '-'));



            if (_debug || !this.WifiIsConnected)
            {
                scan.Wait(TimeSpan.FromSeconds(10));
                var nws = WiFi.EnumerateAvailableNetworks();
                sb.AppendLine("Available WiFi networks:");

                foreach (var n in nws)
                {
                    sb.Append(n.Ssid.ToString());
                    sb.AppendLine($"({n.ProfileName.PadRight(30)}); signal strength: {n.SignalQuality.ToString()}%.");
                    sb.AppendLine($"    - copy this to login.txt file:     ssid={n.Ssid.ToString()}");
                    if (n.Ssid.ToString() == LoginInfo.SSID)
                    {
                        sb.AppendLine("Found required SSID");

                    }
                }
                sb.AppendLine("-----------------------------------".PadRight(80, '-'));


            }
            else
            {
                sb.AppendLine("Wifi is connected, skipping this part......");

            }


            //try to connect
            if (!string.IsNullOrEmpty(LoginInfo.SSID))
            {
                var availableNetwork = NativeWifi.EnumerateAvailableNetworks()
                .Where(x => (x.ProfileName == LoginInfo.SSID))
                .OrderByDescending(x => x.SignalQuality)
                .FirstOrDefault();
                if (availableNetwork != null && !this.WifiIsConnected)
                {

                    {
                        sb.AppendLine($"--- Try to connect to Wifi network:{availableNetwork.Ssid.ToString()}");
                        var _success = WiFi.ConnectNetwork(
                            interfaceId: availableNetwork.Interface.Id,
                            profileName: availableNetwork.ProfileName,
                            bssType: availableNetwork.BssType
                                                    );

                        if (_success)
                        {
                            sb.AppendLine("Wifi API report successfull connection!");
                            this.WifiIsConnected = true;
                        }
                        else { sb.AppendLine("Wifi API reports WIFI connection failed!"); }
                    }

                }

            }
            WifiScanLog = sb.ToString();
        }






    }
}
