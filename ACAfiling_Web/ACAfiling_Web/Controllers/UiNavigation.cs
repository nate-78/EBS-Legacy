using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;
using OpenQA.Selenium.PhantomJS;

namespace ACAfiling_Web.Controllers {
    // this is a screen-scraper class that will login to the IRS's UI portal and perform submissions there.
    // it will upload a manifest file as well as a form data file.
    // this class will also perform status update requests.
    // in the event a status update has a downloadable XML file with error detail, this class will download said file
    // and update the db accordingly
    public class UiNavigation {

        // global driver object
        static PhantomJSDriver driver = new PhantomJSDriver();
        
        /// <summary>
        /// upload the manifest and formadata files -- and return IRS's response;
        /// or get status update on a previous submission from IRS 
        /// </summary>
        /// <param name="manifestPath">required for making a submission</param>
        /// <param name="formdataPath">required for making a submission</param>
        /// <param name="isProduction">production or testing environment?</param>
        /// <param name="isSubmission">submission or status update?</param>
        /// <param name="receiptId">only needed for status update</param>
        /// <param name="defaultDirectoryPath">only needed for status update. Tells application where to save error detail file</param>
        /// <returns></returns>
        public static StatusUpdate NavigateUI(string manifestPath, string formdataPath, bool isProduction, bool isSubmission, 
            string receiptId, string defaultDirectoryPath) {

            // status update that we'll return
            StatusUpdate update = new StatusUpdate();

            // wrap whole thing in try-catch block, because this junk is so error prone (connectivity from IRS)
            try {
                // get login credentials from web.config
                string user = ConfigurationManager.AppSettings["IrsUserId"].ToString();
                string pass = ConfigurationManager.AppSettings["IrsPw"].ToString();
                // get appropriate url
                string url = ConfigurationManager.AppSettings["UiUri"].ToString();


                // navigate to form upload 
                // THESE OPTIONS MAY HELP INCREASE TIMEOUT THRESHOLD -- has not been published yet... doesn't seem to create problems
                //var options = new PhantomJSOptions();
                //options.AddAdditionalCapability("phantomjs.page.settings.resourceTimeout", "900");
                using (driver) {
                    //driver.Manage().Timeouts().
                    driver.Navigate().GoToUrl(url);

                    // find inputs
                    IWebElement userNameField = null;
                    IWebElement userPasswordField = null;
                    IWebElement loginButton = null;

                    int attempts = 0;

                    while ((userNameField == null || userPasswordField == null || loginButton == null) && attempts < 5) {
                        // if we've already made attempts at this, logout and navigate back to the URL
                        if (attempts > 0) {
                            logOutAca();
                            driver.Navigate().GoToUrl(url);
                        }

                        // assign values to the variables (will only work if on the correct page)
                        if (elementExists(By.Id("Username"))) {
                            userNameField = driver.FindElementById("Username");
                        }
                        if (elementExists(By.Id("Password"))) {
                            userPasswordField = driver.FindElementById("Password");
                        }
                        if (elementExists(By.XPath("//input[@value='LOGIN']"))) {
                            loginButton = driver.FindElementByXPath("//input[@value='LOGIN']");
                        }

                        attempts++;
                    }

                    // only proceed if those elements exist
                    if (userNameField != null || userPasswordField != null || loginButton != null) {
                        // input values to login
                        userNameField.SendKeys(user);
                        userPasswordField.SendKeys(pass);
                        loginButton.Click();

                        goToLandingPage();
                        if (!isProduction) {
                            handleTestingPage();
                        }

                        //-----------------
                        // ALL LOGIN PROCEDURES ARE FINISHED. ACTUAL FUNCTIONALITY FROM THIS POINT ON:
                        //-----------------

                        if (isSubmission) {
                            // submit the forms
                            submitTheForms(manifestPath, formdataPath);
                            update = getSubmissionResults(update);

                        } else { // check status update
                            // check the status
                            checkStatusUpdate(receiptId);
                            update = getStatUpdateResults(update, receiptId, defaultDirectoryPath);
                        }

                    } else {
                        // did it direct us to another page?
                        var redirect = false;
                        if (elementExists(By.Name("USER"))) { // user select screen
                            redirect = true;
                        }
                        if (elementExists(By.XPath("//input[@value='Check Transmission Status']"))) { // landing page
                            redirect = true;
                        }
                        try {
                            var body = driver.FindElementByTagName("body");
                            redirect = true;
                        } catch (Exception e) {
                            redirect = true;
                        }
                        update = getGenericErrorUpdateResponse();
                    } // end username, password, etc check

                    logOutAca();

                    driver.Close();
                    driver.Quit();

                } // end using(driver) statement
                
            } catch {
                update = getGenericErrorUpdateResponse();
            }

            return update;
        } // end function


        private static void logOutAca() {
            // logout procedure
            var menu = driver.FindElementByClassName("menu");
            var lastLink = menu.FindElement(By.ClassName("last-child"));
            var logoutLink = lastLink.FindElement(By.LinkText("Logout"));
            logoutLink.Click();
            var buttonYes = driver.FindElementById("btnYes");
            buttonYes.Click();
        }


        // navigate to the landing page
        private static void goToLandingPage() {
            // see if password expiration warning shows up
            handlePwordExpiringPage();

            // select user
            //var userRadio = driver.FindElement(By.XPath("//input[@name='USER'][position()=2]"));
            var userRadio = driver.FindElementsByName("USER");
            var radioTwo = userRadio.Last();
            // the submit button has been hard to grab, for some reason. Here are 3 different
            // ways to get it
            var continueButton = driver.FindElementByXPath("//input[@type='SUBMIT']");
            if (continueButton == null) {
                continueButton = driver.FindElementByXPath("//input[@value='Submit Selected Organization']");
            }
            if (continueButton == null) {
                var inputs = driver.FindElementsByTagName("input");
                continueButton = inputs.Last();
            }

            radioTwo.Click();
            continueButton.Click();
        }


        // handle "password expiring" screen
        private static void handlePwordExpiringPage() {
            // check for password expiration screen
            var passwordChangeLinks = driver.FindElementsByLinkText("change your password");
            if (passwordChangeLinks.Count > 0) { // then we're on the password expiration screen
                // email Nathan that password is expiring
                // TODO
                //string subject = "Your IRS password is about to expire, which will prevent the ACA Filing application from " +
                //    "working correctly. Contact EBS. When they're ready, update your IRS password, and then IMMEDIATELY update the " +
                //    "password being used by the ACA Filing application, which can be found within the web.config file.";
                //SendMail.SendEmail("nathan@owensdev.com", subject, "IRS Password About to Expire", null);
                // click 'continue' link
                var continueLink = driver.FindElementByLinkText("continue");
                continueLink.Click();
            }
        }

        // handle testing page
        private static void handleTestingPage() {
            // if testing mode, there's an additional screen to contend with
            // select first test environment (may need to change this for future version testing)
            var testVersionRadio = driver.FindElement(By.XPath("//input[@name='aatsVersion'][position()=1]"));
            var testVersionSubmit = driver.FindElementByXPath("//input[@value='Submit Selected Version']");
            testVersionRadio.Click();
            testVersionSubmit.Click();
        }


        // submit the forms
        private static void submitTheForms(string manifestPath, string formdataPath) {
            var uploadNav = driver.FindElementByXPath("//input[@value='Upload ACA Forms']");
            uploadNav.Click();

            // upload files: Manifest, then Form data (< 100MB)
            IWebElement manifest = driver.FindElement(By.Id("manifest"));
            manifest.SendKeys(manifestPath);

            IWebElement formData = driver.FindElement(By.Id("file"));
            formData.SendKeys(formdataPath);

            var uploadButton = driver.FindElementByXPath("//input[@value='Transmit']");
            uploadButton.Click();
        }

        private static StatusUpdate getSubmissionResults(StatusUpdate update) {
            // screen scrape results
            // NEED AN ERROR CATCH RIGHT HERE...
            try {
                var results = driver.FindElementByXPath("//form[@action='/airp/aca/ui/app/upload']");
                update = buildUpdateFromHtml(results.GetAttribute("innerHTML")
                    .Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim(), true);
            } catch {
                // must be an error.  grab it
                try {
                    // actual error code from submission page
                    var results = driver.FindElementById("errors"); // that's the general error message
                    var specificError = driver.FindElementByXPath("//ol[@class='error_list']");
                    var specificErrorColl = specificError.FindElements(By.TagName("li"));
                    update.StatusType = "Error";
                    update.StatusCode = results.Text;
                    update.Timestamp = DateTime.Now;
                    var counter = 1;
                    foreach (var error in specificErrorColl) {
                        update.StatusMessage += counter.ToString() + ") " + error.Text + " ";
                        counter++;
                    }
                    update.StatusMessage = update.StatusMessage.Trim(); // remove trailing '; '
                } catch {
                    // unspecified error
                    update = getGenericErrorUpdateResponse();
                }

            }
            return update;
        }

        private static void checkStatusUpdate(string receiptId) {
            var statusCheck = driver.FindElementByXPath("//input[@value='Check Transmission Status']");
            statusCheck.Click();

            // enter TCC, receiptId, and submit
            var tcc = ConfigurationManager.AppSettings["TCC"].ToString();
            if (ConfigurationManager.AppSettings["TestingOrProduction"] == "T") { // use the test tcc
                tcc = ConfigurationManager.AppSettings["TCC_Test"].ToString();
            }
            var tccInput = driver.FindElementById("tcc");
            var receiptIdInput = driver.FindElementById("receiptId");
            var submitBtn = driver.FindElementByXPath("//input[@name='find']");

            tccInput.SendKeys(tcc);
            receiptIdInput.SendKeys(receiptId);
            submitBtn.Click();
        }

        // TODO: refactor this function. too much here
        private static StatusUpdate getStatUpdateResults(StatusUpdate update, string receiptId, string defaultDirectoryPath) {
            // get status update
            try {
                // if download is available, do it
                IWebElement downloadBtn = null;
                if (elementExists(By.XPath("//input[@value='Download']"))) {
                    downloadBtn = driver.FindElementByXPath("//input[@value='Download']");
                }

                var downloadLink = "";
                var hasDownload = false;
                if (downloadBtn != null) {
                    downloadLink = "https://la1.www4.irs.gov/airp/aca/ui/app/submissionStatus/download?";
                    hasDownload = true;
                }

                if (hasDownload) { // if download, build update from that
                                   //WebRequest req = WebRequest.Create(downloadLink);
                                   //var response = req.GetResponse();
                                   //update = buildUpdateFromXml(response);
                    var wclient = new WebClient();
                    // add cookies
                    var allCookies = driver.Manage().Cookies.AllCookies;
                    string cookieString = "";
                    foreach (var cookie in allCookies) {
                        cookieString += cookie.Name + "=" + cookie.Value + ";";
                    }
                    cookieString = cookieString.TrimEnd(';');
                    wclient.Headers.Add(HttpRequestHeader.Cookie, cookieString);
                    // add other headers
                    wclient.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    wclient.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, sdch, br");
                    wclient.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
                    wclient.Headers.Add(HttpRequestHeader.Host, "la1.www4.irs.gov");
                    wclient.Headers.Add(HttpRequestHeader.Referer, "https://la1.www4.irs.gov/airp/aca/ui/app/submissionStatus/find");
                    wclient.Headers.Add(HttpRequestHeader.KeepAlive, "true");
                    wclient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 8.0)");

                    string path = defaultDirectoryPath + "/errors-" + receiptId + "-" + DateTime.Now.ToFileTime() + ".xml";
                    wclient.DownloadFile(downloadLink, path);

                    if (fileHasContent(path)) {
                        update = buildUpdateFromXml(path, receiptId);
                    }
                } // end hasDownload check

                // no error, or no download (for some other reason), or building the update via xml failed
                if (!hasDownload/* || update == null*/) { // generate the results the other way... (below)
                    var results = driver.FindElementById("content");
                    update = buildUpdateFromHtml(results.GetAttribute("innerHTML")
                        .Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim(), false);
                }

                if (update == null || (String.IsNullOrEmpty(update.StatusMessage) && (String.IsNullOrEmpty(update.StatusType)))) {
                    update = getGenericErrorUpdateResponse();
                } else {
                    update.ReceiptId = receiptId; // make sure this remains attached to the update
                }

            } catch (Exception e) {
                // encountered unspecified error
                // add in a mechanism to check for more specific errors?  Like if it's not finding the 
                // tcc / receipt id combination?
                update = getGenericErrorUpdateResponse();
            }
            return update;
        }


        // does element exist?
        private static bool elementExists(By by) {
            try {
                driver.FindElement(by);
                return true;
            } catch (Exception e) { // NoSuchElementException
                // TODO: need to capture this error...
                return false;
            }
        }


        private static bool fileHasContent(string filePath) {
            bool result = false;

            if (new System.IO.FileInfo(filePath).Length > 0) {
                result = true;
            }

            return result;
        }


        private static string getRecordNum(string text) {
            string result = "";

            if (text.Contains("|")) {
                var textArray = text.Split('|');
                if (textArray.Length == 3) { // there should be 3 items in the array: ReceiptID, SubmissionID, RecordID
                    result = textArray[2]; // return last item
                }
            }

            return result;
        }


        // build the update from the XML provided by the UI (as download)
        private static StatusUpdate buildUpdateFromXml(string filePath, string receiptId) {

            StatusUpdate update = new StatusUpdate();

            XElement doc = XElement.Load(filePath);

            if (doc != null) {
                // namespaces
                XNamespace ns = "urn:us:gov:treasury:irs:ext:aca:air:7.0";
                XNamespace ns2 = "urn:us:gov:treasury:irs:common";
                XNamespace ns3 = "urn:us:gov:treasury:irs:msg:form1094-1095BCtransmittermessage";

                XElement root = doc.Element(ns + "ACATransmitterSubmissionDetail");

                IEnumerable<XElement> errorDetailGrpElems = root.Elements(ns + "TransmitterErrorDetailGrp");
                List<ErrorFromIrs> errors = new List<ErrorFromIrs>(); // to submit to db
                bool accepted1094 = true; // will be passed to db with error list
                bool all1095sAccepted = true; // will be passed to db with error list

                // iterate through each to build out error results
                foreach (var elem in errorDetailGrpElems) {
                    if (elem.Descendants(ns + "SubmissionLevelStatusCd").Any()) { // first element -- gives overall submission status
                        update.StatusType = elem.Element(ns + "SubmissionLevelStatusCd").Value;
                    } else {
                        // the specific errors
                        ErrorFromIrs err = new ErrorFromIrs();
                        // for now, just save them as part of the message
                        string recordId = "";
                        if (elem.Elements(ns + "UniqueRecordId").Any()) {
                            recordId = getRecordNum(elem.Element(ns + "UniqueRecordId").Value);
                        }
                        var detail = elem.Element(ns2 + "ErrorMessageDetail");
                        // if this is a record-level error (1095), note it as such
                        if (!String.IsNullOrEmpty(recordId)) {
                            err.Is1095Error = true;
                            err.RecordId = Convert.ToInt32(recordId);
                            update.StatusMessage += "Record " + recordId + ": "; // update status message
                            all1095sAccepted = false; // at least one error relates to a 1095
                        } else {
                            err.Is1095Error = false;
                            accepted1094 = false; // at least one error relates to the 1094
                        }
                        // build rest of the message
                        update.StatusMessage += detail.Element(ns2 + "ErrorMessageCd").Value + " - " +
                            detail.Element(ns2 + "ErrorMessageTxt").Value;
                        if (detail.Descendants(ns2 + "XpathContent").Any()) {
                            update.StatusMessage += " (" + detail.Element(ns2 + "XpathContent").Value + "); ";
                            // also add the xpath content to the err object
                            err.XpathContent = detail.Element(ns2 + "XpathContent").Value;
                        } else {
                            update.StatusMessage += "; ";
                        }

                        // assign each value so we can save these individually in db...
                        // (already assigned recordId above)
                        err.ReceiptId = receiptId;
                        err.ErrorCode = detail.Element(ns2 + "ErrorMessageCd").Value;
                        err.ErrorMessage = DataManager.SqlSafe(detail.Element(ns2 + "ErrorMessageTxt").Value);
                        errors.Add(err);
                    }
                }
                update.StatusMessage.TrimEnd(';');

                update.Timestamp = DateTime.Now;

                // submit record errors, if they've been populated
                if (errors != null && errors.Count > 0) {
                    // irs submission...
                    // send error list and accepted1094 variable
                    AcaDataManager.InsertIrsErrors(errors, accepted1094, all1095sAccepted, update.StatusType);
                }
            } else { // saved error file is empty (this is an error)
                update = getGenericErrorUpdateResponse();
            } // end if doc != null

            return update;
        }

        // build the update from the UI's HTML
        private static StatusUpdate buildUpdateFromHtml(string html, bool isSubmission) {
            StatusUpdate update = new StatusUpdate();

            if (isSubmission) {
                var startIndex = html.IndexOf("Receipt ID:</strong>");
                var endIndex = html.IndexOf("<strong>Date and Time:");
                var receiptIdText = html.Substring(startIndex, endIndex - startIndex);
                // now refine even further to get the actual receipt id
                //if (receiptIdText.Contains("<br")) { 
                var receiptIdTextArr = receiptIdText.Split(new string[] { "<br" }, StringSplitOptions.RemoveEmptyEntries);
                var receiptIdTextArr2 = receiptIdTextArr[0].Split(new string[] { "</strong>" }, StringSplitOptions.RemoveEmptyEntries);
                var receiptId = receiptIdTextArr2[1].Trim();

                update.ReceiptId = receiptId;
                update.StatusType = "Processing";
                update.StatusMessage = "The 1094C and 1095C forms were successfully created and submitted to the IRS. The Receipt ID is "
                    + receiptId;
            } else {
                // not a submission -- checking up on a past submission
                // build out the status update

                // did we get a random technical error?
                if (randomTechnicalError(html)) {
                    update = getGenericErrorUpdateResponse();
                } else {

                    // get the .formrow divs
                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    HtmlNode root = doc.DocumentNode;
                    var rows = root.Descendants("div").Where(d => d.Attributes.Contains("class")
                        && d.Attributes["class"].Value.Contains("formrow"));

                    // if everything worked and we've got some data to work with...
                    if (rows != null && rows.Count() > 0) {
                        // get the status code/type
                        var summary = rows.First();
                        var statusText = summary.LastChild.OuterHtml;
                        var statusArr = statusText.Split(new string[] { "<strong>" }, StringSplitOptions.None);
                        foreach (var stat in statusArr) {
                            if (stat.Contains("Status")) {
                                var arr = stat.Split(new string[] { "</strong>" }, StringSplitOptions.None);
                                var arr2 = arr[1].Split(new string[] { "<br" }, StringSplitOptions.None);
                                var status = arr2[0];
                                update.StatusCode = status.Trim();
                                update.StatusType = status.Trim();
                            }
                        }

                        // get the details
                        var detailsBlock = rows.Skip(1).Take(1);
                        if (detailsBlock.First().InnerHtml.Contains("Error")) { // if errors...
                            update.StatusMessage = "Error Code: ";
                            // look for file download containing details...
                            
                        } // end if
                        var element = detailsBlock.First();
                        var tableBody = element.SelectSingleNode("//tbody");
                        if (tableBody != null) {
                            var data = tableBody.SelectNodes("//tr");

                            foreach (var row in data) { // the first of these will contain "error" in it... can use this...
                                update.StatusMessage += row.ChildNodes.First().InnerHtml;
                                update.StatusMessage += " - " + row.ChildNodes.Last().InnerHtml + "\r\n";
                            } // end foreach
                        } // end if tableBody...
                    } else {
                        // some weird error
                        update.StatusType = "Error";
                        update.StatusMessage = "The status update request encountered an unspecified error from the IRS. Please attempt " +
                            "the status update again in just a moment.";
                    }
                } // end if-else 'randomTechnicalError'

                //update.StatusMessage += data.First().InnerHtml;
                //update.StatusMessage += " - " + data.Last().InnerHtml;

                // timestamp
                update.Timestamp = DateTime.Now;
            } // end if-else (isSubmission) block

            return update;
        }


        // did the browser get redirected to the landing page? 
        // not using this currently. may not need it
        private static bool redirectedToLandingPage(string html) {
            bool result = false;
            if (html.ToLower().Contains("upload aca forms") && html.ToLower().Contains("check transmission status") && 
                html.ToLower().Contains("once you have completed your transmission in step a")) {
                result = true;
            }

            return result;
        }

        // random technical error
        private static bool randomTechnicalError(string html) {
            bool result = false;
            if (html.ToLower().Contains("experiencing a technical issue")) {
                result = true;
            }

            return result;
        }

        private static StatusUpdate getGenericErrorUpdateResponse() {
            StatusUpdate update = new StatusUpdate();
            update.StatusType = "Error";
            update.Timestamp = DateTime.Now;
            update.StatusMessage = "The submission encountered an unspecified error from the IRS. Please attempt " +
                "the submission again in just a moment.";

            return update;
        }

        
    }
}