This program will log a PC in into a Wi-fi network with captive portal.  Examples are airport wifi networks, hotels and company guest networks.  Special about this software is you can do it without screen and keyboard.
How to use this on a headless PC:
+ Install the software on the PC and test it.  Make sure it runs on startup, e.g. by using the task scheduler.
+ Make sure the PC has the Firefox browser installed, and any .NET libraries needed. 
+ Ship the PC to the location where you will use it.
+ Start the PC with an empty USB memory stick in the PC.
+ After about a minute, you will find several files on the USB stick:
    + Login.txt :  a blank settings file you can edit to enter username, password and other settings.
    + Logfile.txt: tells you how it is going.  Here you will find values you can use in the Login.txt file, like the HTML name of checkboxes to click.
    + webdump file(s):  The webpages the program is downloading.  Mostly for debugging.
Follow instructions in the login.txt file.  You may have to reboot the PC several times to get all the settings right.
