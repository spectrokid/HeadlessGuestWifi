// See https://aka.ms/new-console-template for more information
using ConnectionDiagnostic;


ConsoleColor _def = Console.ForegroundColor;
Console.ForegroundColor = ConsoleColor.Blue;
Console.WriteLine("This program will try to login to a guest-Wifi by opening a webpage and typing in username and password.".PadRight(80, '-'));
Console.WriteLine("- Use commandline option '-debug' to run the Firefox browser visible".PadRight(80, '-'));
Console.WriteLine("- This program will only work with Firefox.".PadRight(80, '-'));
Console.WriteLine("- Use instructions below to make a 'Login.txt' file,on a USB stick or on D:\\Products\\Startup\\login.txt.".PadRight(80, '-'));
Console.ForegroundColor = _def;

DriveEnumerator _de = new();
bool _headless = true;
foreach (var s in args)
{
    if (s.Contains("debug", StringComparison.OrdinalIgnoreCase)) _headless = false;
    if (s.Contains("show", StringComparison.OrdinalIgnoreCase)) _headless = false;
}
using (StreamWriter _writer = _de.LogWriter)
{
    Console.WriteLine(_de.DriveScanLog);
    _writer?.WriteLine(_de.DriveScanLog);


    WiFi_handler wiFi = new WiFi_handler(debug: !_headless);
    Console.WriteLine(wiFi.WifiScanLog);
    _writer?.WriteLine(wiFi.WifiScanLog);

    TeamViewerMonitor monitor = new TeamViewerMonitor();
    Console.WriteLine(monitor.LogString);
    _writer?.WriteLine(monitor.LogString);

    if (wiFi.WifiIsConnected)
    {
        DNSredirectDetector dnsredirectdetector = new DNSredirectDetector();
        dnsredirectdetector.Rundetector();
        Console.WriteLine(dnsredirectdetector.logbook);
        _writer?.WriteLine(dnsredirectdetector.logbook);
    }
    else
    {
        Console.WriteLine("Wifi not connected, skipping DNS redirection detection...");
        _writer?.WriteLine("Wifi not connected, skipping DNS redirection detection...");
    }

    //if (TeamViewerMonitor.PingServer())
    //{
    //    _writer?.WriteLine("I have contact with the TeamViewer server! Exiting...".PadRight(80, '-'));
    //    _writer?.Flush();
    //    return;
    //}
    //else
    //{
    //    _writer?.WriteLine("No connection to TeamViewer server, trying to log in to webclient...".PadRight(80, '-'));
    //}


    if (wiFi.WifiIsConnected)
    {
        WebLoginHandler _wli = new WebLoginHandler(_headless);
        Console.WriteLine(_wli.LogString);
        _writer?.WriteLine(_wli.LogString);

        Console.WriteLine($"Login attempt success: {_wli.TryLogin().ToString()}");
        Console.WriteLine(_wli.LogString);
        _writer?.WriteLine(_wli.LogString);

    }
    else
    {
        Console.WriteLine("Wifi not connected, skipping login attempt...");
        _writer?.WriteLine("Wifi not connected, skipping login attempt...");
    }


    if (TeamViewerMonitor.PingServer())
    {
        _writer?.WriteLine("I have contact with the TeamViewer server! Exiting...".PadRight(80, '-'));
    }
    else
    {
        _writer?.WriteLine("No connection to TeamViewer server, attempt failed...".PadRight(80, '-'));
    }

    _writer?.Flush();

}