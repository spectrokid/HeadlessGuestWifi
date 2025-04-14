using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using ABI.System;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Windows.Media.Capture;
using OpenQA.Selenium.DevTools.V133.Network;

namespace ConnectionDiagnostic
{
    internal partial class WebLoginHandler
    {
        public string LogString = string.Empty;
        public bool Headless = true;
        internal int _pagedumplevel = 0;
        public WebLoginHandler(bool Headless = true)
        {
            StringBuilder sb = new StringBuilder();
            this.Headless = Headless;
            string mode = Headless ? "headless" : "visible";
            Console.WriteLine($"----starting WebLoginHandler in {mode} mode".PadRight(80, '-'));
            sb.AppendLine($"----starting WebLoginHandler in {mode} mode".PadRight(80, '-'));

        }
        public bool TryLogin()
        {
            bool success = true;
            const string DEFLURL = "http://www.msftconnecttest.com/connecttest.txt";
            const string SUCCESS = "Microsoft Connect Test";
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(LoginInfo.Url))
            {
                sb.Append("No URL provided, will try standard Microsoft connect test.");
                sb.AppendLine("Use 'url=http://abc.com/xyz.html' in the login.txt file on the usb drive.");
                LoginInfo.Url = DEFLURL;
            }


            if (!string.IsNullOrEmpty(LoginInfo.Url))
            {
                sb.AppendLine($"Opening '{LoginInfo.Url}'....");

                // Load the HTML into HtmlDocument
                // Set up the Chrome WebDriver
                var options = new FirefoxOptions();
                if (this.Headless)
                {
                    options.AddArgument("--headless"); // Optional: Run in headless mode
                }
                using (var driver = new FirefoxDriver(options))
                {
                    try
                    {
                        WebDriverWait wait = RecursivePageLoad(sb, driver, LoginInfo.Url);

                        if (LoginInfo.Url == DEFLURL && driver.PageSource.Contains(SUCCESS))
                        {
                            success = true;
                            sb.AppendLine("The Internet connection test succeeded. You are online!".PadRight(80, '-'));
                            sb.AppendLine("-".PadRight(80, '-'));
                            this.LogString = sb.ToString();
                            return success;
                        }

                        // Find all input elements of type "text"
                        var txtboxes = driver.FindElements(By.XPath("//input[@type='text']"));
                        var pwdboxes = driver.FindElements(By.XPath("//input[@type='password']"));
                        var chkboxes = driver.FindElements(By.XPath("//input[@type='checkbox']"));
                        TryEnterText(sb, txtboxes, false);
                        TryEnterText(sb, pwdboxes, true);
                        TryClickCheckBoxes(sb, chkboxes);

                        // now click submit
                        var buttons = driver.FindElements(By.XPath("//input[@type='submit']"));
                        if (buttons?.Count == 0)
                        {
                            buttons = driver.FindElements(By.TagName("button"));
                        }
                        bool found = false;
                        if (buttons?.Count > 0)
                        {
                            sb.AppendLine($"Found {buttons.Count} buttons on the page:");
                            int i = 1;
                            string buttonName = string.Empty;
                            foreach (var button in buttons)
                            {
                                try
                                {
                                    wait.Until(d => button.Displayed);
                                    buttonName = button.Text;
                                    if (string.IsNullOrEmpty(buttonName)) buttonName += button.GetAttribute("name");
                                    if (string.IsNullOrEmpty(buttonName)) buttonName += button.GetAttribute("id");
                                    if (string.IsNullOrEmpty(buttonName)) buttonName += $"button{i}";


                                    if (LoginInfo.ButtonName.Length > 0 && buttonName.Contains(LoginInfo.ButtonName, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        sb.Append($"When trying to click button'{buttonName}':  ");
                                        button.Click();
                                        WebDriverWait _dWait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(3));
                                        found = true;
                                        sb.AppendLine("Success!");
                                        break;
                                    }
                                    else
                                    {
                                        sb.AppendLine($"Button Name: {buttonName}".PadRight(80, '-'));
                                        sb.AppendLine($"  Add this to login.txt:   buttonname={buttonName}");

                                    }
                                }
                                catch (ElementNotInteractableException) { sb.AppendLine($"Button {buttonName} is not interactable"); }
                                catch (WebDriverTimeoutException) { }
                                catch (System.Exception e) { sb.AppendLine($"button gives exception: {e.Message}"); }

                            }

                            if (!found)
                            {
                                sb.AppendLine($"No button with the name '{LoginInfo.ButtonName}' was found.  Trying to click a default button.");
                                try
                                {
                                    if ((!found) && (buttons.Count < 2))
                                    {// there is only one button
                                        foreach (var button in buttons) button.Click();
                                    }
                                    else
                                    {// try pick one
                                        foreach (var button in buttons)
                                        {
                                            if (button.Text.ToLower().Trim() == "ok") button.Click();
                                            if (button.Text.ToLower().Contains("submit")) button.Click();
                                            if (button.Text.ToLower().Contains("login")) button.Click();
                                            if (button.Text.ToLower().Contains("sign in")) button.Click();
                                            if (button.Text.ToLower().Contains("log in")) button.Click();
                                            if (button.Text.ToLower().Contains("continue")) button.Click();
                                            if (button.GetAttribute("type").Contains("submit", StringComparison.InvariantCultureIgnoreCase)) button.Click();
                                        }
                                    }
                                }
                                catch (ElementNotInteractableException) { sb.AppendLine($"Button {buttonName} is not interactable"); }
                                catch (System.Exception e) { sb.AppendLine($"button gives exception: {e.Message}"); }
                            }
                            //sb.AppendLine("Start of page dump.".PadRight(80, '-'));
                            //sb.AppendLine(driver.PageSource);
                            //sb.AppendLine("End of page dump.".PadRight(80, '-'));
                        }
                        else
                        {
                            sb.AppendLine("No buttons found on the page.".PadRight(80, '-'));
                            //sb.AppendLine("Start of page dump.".PadRight(80, '-'));
                            //sb.AppendLine(driver.PageSource);
                            //sb.AppendLine("End of page dump.".PadRight(80, '-'));

                        }

                    }
                    catch (System.Exception ex)
                    {
                        sb.AppendLine("An exception occured while trying to login to the website:");
                        sb.AppendLine(ex.Message);
                        success = false;
                    }
                    finally
                    {
                        // Close the browser
                        WebDriverWait wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(5));
                        driver.Quit();

                    }
                }
            }


            this.LogString = sb.ToString();
            return success;

            WebDriverWait RecursivePageLoad(StringBuilder sb, FirefoxDriver driver, string _url)
            {
                try
                {
                    // Navigate to the webpage
                    driver.Navigate().GoToUrl(_url);
                    //driver.Navigate().GoToUrl("");
                    OpenQA.Selenium.VirtualAuth.VirtualAuthenticatorOptions opts = new();
                    opts.SetHasResidentKey(true);
                    opts.SetHasUserVerification(true);
                    //driver.AddVirtualAuthenticator(opts);
                }
                catch (System.Exception ex)
                {
                    sb.AppendLine("An exception occured while trying to login to the website:");
                    sb.AppendLine(ex.Message);
                    sb.AppendLine("I will try with the current page anyway");
                }
                WebDriverWait wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(2));

                // Try to dump page
                try
                {
                    if (!string.IsNullOrWhiteSpace(DriveEnumerator.PageDumpPath))
                    {
                        _pagedumplevel++;
                        string s = DriveEnumerator.PageDumpPath.Replace("_X", $"_{_pagedumplevel}");
                        System.IO.FileInfo fi = new(s);
                        using (FileStream fs = fi.OpenWrite())
                        {
                            using (StreamWriter sw = new StreamWriter(fs, encoding: Encoding.UTF8))
                            {
                                sw.WriteLine(driver.PageSource.ToString());
                                sw.Flush();
                            }
                        }


                    }
                }
                catch (System.Exception ex)
                {

                    sb.AppendLine("Exception when trying to write pagedump file.");
                    sb.AppendLine(DriveEnumerator.PageDumpPath);
                    sb.AppendLine(ex.Message);
                }


                // Find redirect instruct
                sb.AppendLine("Try to detect a redirect".PadRight(80, '-'));
                var redir = driver.FindElements(By.XPath("//meta[@http-equiv=\"refresh\"]"));
                string _redirurl = string.Empty;
                if (redir?.Count > 0)
                {
                    foreach (var element in redir)
                    {
                        sb.AppendLine($"Found redirect:  {element.Text}");
                        sb.AppendLine($"content:       {element.GetAttribute("content")}");
                        sb.AppendLine("End of redirect instruction".PadRight(80, '-'));
                        _redirurl += element.GetAttribute("content");
                        _redirurl = _redirurl.Substring(_redirurl.IndexOf("http"));

                    }
                }
                else
                {
                    sb.AppendLine("No redirects detected.");
                }

                if (!string.IsNullOrEmpty(_redirurl))
                {
                    sb.AppendLine($"Trying to redirect to:  {_redirurl}");
                    RecursivePageLoad(sb, driver, _redirurl);
                }
                sb.AppendLine("End of HTML redirect attempts".PadRight(80, '-'));
                return wait;
            }
        }

        private void TryEnterText(StringBuilder sb, System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> textboxes, bool isPasswordStyle)
        {
            if (textboxes.Count > 0)
            {
                sb.AppendLine($"Found {textboxes.Count} textboxes on the page. Entering text...");
                sb.AppendLine("Textboxes found on the page:");
                int i = 1;
                // Enter "abc" into each textbox
                foreach (var textbox in textboxes)
                {
                    try
                    {
                        string name = string.Empty;
                        name += textbox.GetAttribute("name");
                        if (string.IsNullOrEmpty(name)) name += textbox.GetAttribute("id");
                        if (string.IsNullOrEmpty(name)) name += $"field{i.ToString()}";
                        sb.AppendLine($"+ Textbox '{name}'");


                        //attempt to fill out textbox
                        if (name == LoginInfo.UserNameField)
                        {
                            string s = string.Empty;
                            s += LoginInfo.UserNameValue;
                            sb.AppendLine($"Entering '{s}' into '{name}.");
                            if (!string.IsNullOrEmpty(s)) textbox.SendKeys(s);
                            continue;
                        }
                        if ((name == LoginInfo.PasswordField) || (isPasswordStyle))
                        {
                            string s = string.Empty;
                            s += LoginInfo.PasswordValue;
                            sb.AppendLine($"Entering '{s}' into '{name}.");
                            if (!string.IsNullOrEmpty(s)) textbox.SendKeys(s);
                            continue;
                        }
                        //----this only runs if textbox is not in the login.txt
                        if (isPasswordStyle)
                        {
                            sb.AppendLine($"  Add this to login.txt: passwordfield={name}");
                            if (string.IsNullOrWhiteSpace(LoginInfo.PasswordValue)) sb.AppendLine($"  Add this to login.txt: password=my_password");
                        }
                        else
                        {
                            sb.AppendLine($"  Add this to login.txt: userfield={name}");
                            if (string.IsNullOrWhiteSpace(LoginInfo.UserNameValue)) sb.AppendLine($"  Add this to login.txt: user=my_user_name");
                        }
                    }
                    catch (System.Exception ex)
                    {

                        sb.AppendLine($"An exception occured while finding textboxes: {ex.Message} ");
                    }
                    finally { i++; }

                }

                sb.AppendLine("End of text entry.".PadRight(80, '-'));
            }




            else
            {
                sb.AppendLine("No textboxes found on the page.");
            }
        }



        void TryClickCheckBoxes(StringBuilder sb, System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> chkboxes)
        {
            if (chkboxes.Count == 0)
            {
                sb.AppendLine("No checkboxes were found on the page.");
                return;
            }
            sb.AppendLine($"Found {chkboxes.Count} checkboxes on the page. Trying to check them...");
            sb.AppendLine("Checkboxes found on the page:");
            int i = 1;
            foreach (var cbx in chkboxes)
            {
                string name = string.Empty;

                try
                {
                    name += cbx.GetAttribute("name");
                    if (string.IsNullOrEmpty(name)) name += cbx.GetAttribute("id");
                    if (string.IsNullOrEmpty(name)) name += $"field{i.ToString()}";
                    sb.AppendLine($"+ Checkbox '{name}'");
                    sb.AppendLine($"   To check this box, add this to login.txt: Checkbox={name}");
                    // try to click
                    if (LoginInfo.Checkboxes.Contains(name))
                    {
                        sb.Append("When trying to click this checkbox:  ");
                        cbx.Click();
                        sb.AppendLine("Success!");
                    }



                }
                catch (ElementNotInteractableException) { sb.AppendLine($"Checkbox {name} is not interactable."); }
                catch (WebDriverTimeoutException) { sb.AppendLine("Firefox timed out."); }
                catch (System.Exception e) { sb.AppendLine($"Checkbox gives exception: {e.Message}"); }
                finally { i++; }





            }


        }
    }
}
