using ConnectionDiagnostic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ConnectionDiagnostic
{
    public class LoginInfo
    {
        public static string Url = string.Empty;
        public static string UserNameField = string.Empty;
        public static string PasswordField = string.Empty;
        public static string UserNameValue = string.Empty;
        public static string PasswordValue = string.Empty;
        public static string ButtonName = string.Empty;
        public static string SSID = string.Empty;
        public static System.Collections.Generic.List<string> Checkboxes = new();
        public static bool SaveToHardDisk = false;

        public static string ReadFromFile(System.IO.FileInfo LoginInfoFile)
        {

            StringBuilder sb = new();
            if (!LoginInfoFile?.Exists ?? false)
            {
                sb.AppendLine($"Login info file '{LoginInfoFile?.FullName ?? "null"}' was not found. No login attempt will be made.");
                return sb.ToString();
            }
            using (StreamReader _info = LoginInfoFile.OpenText())
            {
                string line = string.Empty;
                while (!_info.EndOfStream)
                {
                    line = _info.ReadLine() ?? string.Empty;
                    if (line.StartsWith("#")) continue;
                    if (line.StartsWith("//")) continue;
                    if (line.Trim().Length < 2) continue;
                    var words = line.Split('=');
                    switch (words[0].Trim().ToLower())
                    {
                        case "url":
                            ConnectionDiagnostic.LoginInfo.Url = words[1].Trim();
                            break;
                        case "user":
                        case "username":
                            ConnectionDiagnostic.LoginInfo.UserNameValue = words[1].Trim();
                            break;
                        case "password":
                        case "pwd":
                            ConnectionDiagnostic.LoginInfo.PasswordValue = words[1].Trim();
                            break;
                        case "flduser":
                        case "userfield":
                        case "usernamefield":
                            ConnectionDiagnostic.LoginInfo.UserNameField = words[1].Trim();
                            break;

                        case "pwdfield":
                        case "passwordfield":
                        case "fldpwd":
                            ConnectionDiagnostic.LoginInfo.PasswordField = words[1].Trim();
                            break;
                        case "wificode":
                        case "ssid":
                            ConnectionDiagnostic.LoginInfo.SSID = words[1].Trim();
                            break;
                        case "buttonname":
                            ConnectionDiagnostic.LoginInfo.ButtonName = words[1].Trim();
                            break;
                        case "checkbox":
                            ConnectionDiagnostic.LoginInfo.Checkboxes.Add(words[1].Trim());
                            break;
                        case "savetodisk":
                            ConnectionDiagnostic.LoginInfo.SaveToHardDisk= words[1].Contains("true",StringComparison.OrdinalIgnoreCase);
                            break;
                        default:
                            sb.AppendLine($"Error: I do not understand the command '{words[0]}");
                            break;
                    }
                }
                sb.AppendLine("---I read these instructions:".PadRight(80, '-'));
                const string _ne = "<not entered>";

                string sv = string.IsNullOrEmpty(ConnectionDiagnostic.LoginInfo.SSID) ? _ne : ConnectionDiagnostic.LoginInfo.SSID;
                sb.AppendLine($"- Wi-Fi network to connect to: {sv}");

                sv = string.IsNullOrEmpty(ConnectionDiagnostic.LoginInfo.UserNameValue) ? _ne : ConnectionDiagnostic.LoginInfo.UserNameValue;
                sb.AppendLine($"- Username to type in: {sv}");

                sv = string.IsNullOrEmpty(ConnectionDiagnostic.LoginInfo.PasswordValue) ? _ne : ConnectionDiagnostic.LoginInfo.PasswordValue;
                sb.AppendLine($"- Password to type in: {sv}");

                sv = string.IsNullOrEmpty(ConnectionDiagnostic.LoginInfo.UserNameField) ? _ne : ConnectionDiagnostic.LoginInfo.UserNameField;
                sb.AppendLine($"- Name of the textbox where to enter the username: {sv}");

                sv = string.IsNullOrEmpty(ConnectionDiagnostic.LoginInfo.PasswordField) ? _ne : ConnectionDiagnostic.LoginInfo.PasswordField;
                sb.AppendLine($"- Name of the textbox where to enter the password: {sv}");

                if (LoginInfo.Checkboxes.Count > 0)
                {
                    sb.Append("- Name of all checkboxes that have to be checked: ");
                    foreach (var checkbox in LoginInfo.Checkboxes)
                    {
                        sb.Append($" {checkbox},");
                    }
                    sb.AppendLine(".");

                    sb.AppendLine("---End of instructions:".PadRight(80, '-'));
                }



                return sb.ToString();
            }
        }

        public static void WriteToFile(System.IO.FileInfo file)
        {
            if (file is null) throw new ArgumentNullException("file");

            try
            {
                if (File.Exists(file.FullName)) file.Delete();
                if (!Directory.Exists(file.DirectoryName)) Directory.CreateDirectory(file.DirectoryName);
                using (FileStream fs = file.OpenWrite())
                {
                    using (StreamWriter sw = new StreamWriter(fs, encoding: Encoding.UTF8))
                    {
                        string s = string.Empty;
                        sw.WriteLine("#--------Login info file".PadRight(80, '-'));
                        sw.WriteLine("#  - Lines starting with '#' are ignored");
                        sw.WriteLine("# This is NOT for the Wifi password, but for the webpage where you login to a hotel or guest network.");
                        sw.WriteLine("# Write login info below.");
                        sw.WriteLine("# Remove the lines if not needed (e.g. for airport wifi that just needs a 'I agree' checkbox.).");
                        s = string.IsNullOrWhiteSpace(LoginInfo.UserNameValue) ? "# user=my_user_name" : $"user={LoginInfo.UserNameValue}";
                        sw.WriteLine(s);
                        s = string.IsNullOrWhiteSpace(LoginInfo.PasswordValue) ? "# password=my_password" : $"password={LoginInfo.PasswordValue}";
                        sw.WriteLine(s);
                        sw.WriteLine("#---- Description of login page HTML".PadRight(80, '-'));
                        sw.WriteLine("# Use values from the 'logfile.txt' file to describe which textboxes to fill.");
                        
                        sw.WriteLine("# --- Name of the textbox where the username should be entered:");
                        s = string.IsNullOrWhiteSpace(LoginInfo.UserNameField) ? "# userfield=something" : $"userfield={LoginInfo.UserNameField}";
                        sw.WriteLine(s);

                        sw.WriteLine("# --- Name of the textbox where the password should be entered:");
                        s = string.IsNullOrWhiteSpace(LoginInfo.PasswordField) ? "# passwordfield=something" : $"passwordfield={LoginInfo.PasswordField}";
                        sw.WriteLine(s);

                        sw.WriteLine("# --- A list of all the 'I agree' checkboxes that should be clicked. Write only one per line.");
                        sw.WriteLine("# checkbox=something");
                        foreach (var x in LoginInfo.Checkboxes) {
                            sw.WriteLine($"checkbox={x}");
                        }

                        sw.WriteLine("# --- Name of the 'continue' button that should be clicked':");
                        s = string.IsNullOrWhiteSpace(LoginInfo.ButtonName) ? "# buttonname=something" : $"buttonname={LoginInfo.ButtonName}";
                        sw.WriteLine(s);


                        sw.WriteLine("# --- URL of webpage to log in to.  Normally you should NOT use this, as the portal will redirect traffic.:");
                        sw.WriteLine("# --- If you leave this blank, software will do standard Microsoft connect test.(Recommended)");
                        s = string.IsNullOrWhiteSpace(LoginInfo.Url) ? "# url=http://use.default.page.com/login.html" : $"url={LoginInfo.Url}";
                        sw.WriteLine(s);

                        sw.WriteLine("#---- End of captive web portal login description.".PadRight(80, '-'));
                        sw.WriteLine("#---- Wi-Fi network to choose.".PadRight(80, '-'));
                        sw.WriteLine("# --- Only use this if the system is not set up to connect automatically.");
                        sw.WriteLine("# --- This only works for open Wi-Fi networks. (No password needed.)");
                        sw.WriteLine("# --- Name of the Wi-Fi network (SSID):");
                        s = string.IsNullOrWhiteSpace(LoginInfo.SSID) ? "# ssid=my_wifi" : $"ssid={LoginInfo.SSID}";
                        sw.WriteLine(s);
                        sw.WriteLine("#---- Save to hard disk.".PadRight(80, '-'));
                        sw.WriteLine("# --- Uncomment the line below if you want to write the settings from the USB drive");
                        sw.WriteLine("#     to the hard disk. You can then reboot without USB drive.");
                        sw.WriteLine("#savetodisk=true");




                        sw.Flush();


                    }
                }



            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}

