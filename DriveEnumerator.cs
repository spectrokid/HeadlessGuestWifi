using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionDiagnostic
{
    internal class DriveEnumerator
    {
        public string DriveScanLog = string.Empty;
        public string LogFilePath = string.Empty;
        public static string PageDumpPath = string.Empty;
        public StreamWriter LogWriter = new StreamWriter(new MemoryStream());
        internal InfoFile info_c = new("Q:\\doesnotexist\\");
        internal InfoFile info_d = new("Q:\\doesnotexist\\");
        internal InfoFile info_usb = new("Q:\\doesnotexist\\");


        internal DriveEnumerator()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Diagnosis run at {DateTime.Now.ToLongTimeString()}");
            sb.AppendLine($"Name of this machine: {Environment.MachineName}");
            var DrivesPresent = System.IO.DriveInfo.GetDrives();
            info_usb.exists = false;
            foreach (var Drive in DrivesPresent)
            {
                sb.Append($"Drive found: {Drive.Name} : ");
                if (Drive.IsReady)
                {
                    var myspace = ByteSizeLib.ByteSize.FromBytes(Drive.AvailableFreeSpace);
                    sb.Append($"({Drive.VolumeLabel}) available space on drive: {myspace.ToString()}; ");
                }
                else sb.Append("; drive is not ready.");
                if (Drive.DriveType == DriveType.Removable)
                {
                    if (Drive.IsReady)
                    {
                        if (Drive.AvailableFreeSpace > 1000000)
                        {

                            //try to write
                            try
                            {
                                if (info_usb.exists) continue;
                                info_usb = new(System.IO.Path.Combine(Drive.RootDirectory.FullName, "login.txt"));
                            }
                            catch (Exception ex)
                            {
                                sb.Append($"Error while trying to write to {info_usb.File.FullName}:  {ex.ToString()}.");
                            }

                        }
                    }
                    else sb.Append($"Drive is full: only {Drive.AvailableFreeSpace} bytes free space left.");

                }
                else sb.Append("drive is not removable. Not accepted as USB stick!");

                sb.AppendLine(" ");
            }
            //find login info file

            info_d = new("D:\\Products\\Startup\\login.txt");
            info_c = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "login.txt"));

            bool _loaded = false;
            if (info_usb.exists)// usb stick trumps all others
            {
                sb.AppendLine($"Found login info: {info_usb.File.FullName}");
                LogFilePath = info_usb.DeriveLogFileName();
                PageDumpPath= info_usb.PageDumpFileName();
                sb.AppendLine(info_usb.Load());
                _loaded = true;
            }
            if (!_loaded && info_d.exists)// look in D drive
            {
                sb.AppendLine($"Found login info: {info_d.File.FullName}");
                LogFilePath = info_d.DeriveLogFileName();
                PageDumpPath = info_d.PageDumpFileName();
                sb.AppendLine(info_d.Load());
                _loaded = true;
            }
            if (!_loaded && info_c.exists)// look in C drive
            {
                sb.AppendLine($"Found login info: {info_c.File.FullName}");
                LogFilePath = info_c.DeriveLogFileName();
                sb.AppendLine(info_c.Load());
                _loaded = true;
            }

            //make a new one on the USB drive
            if (info_usb.canwrite)
            {
                if (!info_usb.exists)
                {
                    sb.AppendLine($"No login file found on the USB drive, creating a new one at {info_usb.File.FullName}.");
                    try
                    {
                        info_usb.Save();
                        LogFilePath = info_usb.DeriveLogFileName();
                        PageDumpPath = info_usb.PageDumpFileName();
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine(ex.Message);
                    }
                }
            }
            else
            {
                sb.AppendLine($"No USB drive found to create a new Login.txt file.");
            }


            //save a copy to C: or D:
            if (LoginInfo.SaveToHardDisk)
            {
                if (info_d.canwrite)
                {
                    sb.AppendLine($"Saving a copy to {info_d.File.FullName}");
                    LoginInfo.SaveToHardDisk = false; //dont write HD to HD
                    info_d.Save();
                }
                else
                {
                    if (info_c.canwrite)
                    {
                        sb.AppendLine($"Saving a copy to {info_c.File.FullName}");
                        LoginInfo.SaveToHardDisk = false; //dont write HD to HD
                        info_c.Save();
                    }
                }
            }


            //open logfile
            if (!string.IsNullOrEmpty(LogFilePath))
            {
                try
                {
                    System.IO.FileInfo dsl = new System.IO.FileInfo(LogFilePath);
                    if (dsl.Exists) dsl.Delete();
                    FileStream fs = dsl.OpenWrite();
                    StreamWriter fs2 = new StreamWriter(fs, Encoding.UTF8);
                    fs2.Write($"{DateTime.Now.ToString()}  Log file");
                    fs2.Flush();
                    LogWriter = fs2;
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"Error opening {LogFilePath}; log: {ex.Message}");
                }
            }
            this.DriveScanLog = sb.ToString();
        }

    }





    public class InfoFile
    {
        public FileInfo File { get; set; }
        public bool exists;
        public bool canwrite;
       
        public InfoFile(string fileName)
        {

            File = new FileInfo(fileName);
            exists = false;
            canwrite = false;
            if (File.Directory is not null) { canwrite = File.Directory.Exists; }
            exists = File.Exists;

        }
        public string Load()
        {
            return LoginInfo.ReadFromFile(File);
        }
        public void Save()
        {
            LoginInfo.WriteToFile(File);
        }
        public string DeriveLogFileName()
        {
            if (File.Directory != null) { return Path.Combine(File.Directory.FullName, "Logfile.txt"); }
            else { return string.Empty; }
        }
        public string PageDumpFileName()
        {

            if (File.Directory != null)
            {
                return Path.Combine(File.Directory.FullName, $"Web_Pagedump_X.html");
            }
            else { return string.Empty; }
        }
    }
}

