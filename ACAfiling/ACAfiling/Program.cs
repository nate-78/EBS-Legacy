using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;
using ACAfiling.Controllers;
using EbsAzureVaultManager;

using Microsoft.Azure; // Namespace for Azure Configuration Manager
using Microsoft.WindowsAzure.Storage; // Namespace for Storage Client Library
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage
using Microsoft.WindowsAzure.Storage.File; // Namespace for File storage

namespace ACAfiling {
    class Program {

        private static string email; // global variable to hold email address
        private static CloudFile emailFile = null;
        private static User user = new User(); // the user making the submission
        private static CorrectionInfo corrInfo = new CorrectionInfo(); // holds all the data pertinent to a correction/replacement submission
        private static string subType; // holds the submission type: "O", "C", "R"

        private static void write(string text) {
            // comment out the body of this function for production
            Console.WriteLine(text);
        }

        static void Main(string[] args) {
            // ORIGINALLY, THE ACA FILING FUNCTIONS WERE PART OF THE  ACA FILNG WEB APPLICATION (which includes a dashboard, form, etc).   
            // HOWEVER, TIMEOUT ISSUES SHOWED THAT RUNNING THIS FUNCTION IN THE BACKGROUND AS A WEB JOB MADE MUCH MORE SENSE. THAT'S WHAT THIS APPLICATION IS FOR.


            // TODO: spreadsheet should be saveable.  Maybe that's not something to worry about here, but wanted to mention it


            // run this continually, checking for files every 5 minutes
            //while (true) {
            write("Looking for a file...");
            // check for spreadsheet in Azure FileStorage and set the email address

            try {
                CloudFile file = getSpreadsheetSetEmailSetCorrInfo();
                user = HubDataManager.GetUser(email);

                if (file != null) { // if it exists...
                    // convert data into Class Objects
                    write("File exists...");
                    MemoryStream stream = new MemoryStream();
                    file.DownloadToStream(stream);
                    Record submissionRec = ReportBuilder.CreateSubmissionRecord(stream, subType, corrInfo);
                    write("Report built...");
                    submissionRec.Directory = createNewLocalDirectory(submissionRec.Company.Name);

                    // store data in db
                    // create form data detail XML file
                    // create manifest XML file
                    // submit XML files to IRS  and wait for Receipt ID
                    // store Receipt ID in db
                    var responses = TransmissionController.SubmitAcaRecord(submissionRec, corrInfo, user);
                    write("Files built");

                    // delete the spreadsheet and email files
                    file.Delete();
                    if (emailFile != null) {
                        emailFile.Delete();
                    }

                    // if the form data and manifest file paths are populated, save them in the Vault and email user
                    //if (responses != null && responses.Count > 0 && !String.IsNullOrEmpty(responses.First().FormDataFilePath)) {
                    //    string msg = buildVaultStorageResponseMsg(responses, submissionRec);
                    //    SendAzureMail.SendMail(user.Email, "ACA Files Ready for Submission", msg);
                    //} else {
                    //    // email Receipt ID to user
                    //    // will need to build subject maybe with RID, need to build msg... should I have an email method in this class that bundles everything and submits to SendAzureMail?
                    //    string msg = buildResponseMsg(responses);
                    //    SendAzureMail.SendMail(user.Email, "ACA Submission Complete", msg);
                    //}
                    write("Finished");
                    
                } // end if

                // reset global variables
                email = "";
                user = new User();
                corrInfo = new CorrectionInfo();
                subType = "";

                // wait for 1 minute, then see if PhantomJS is still running as a process.  If it is, kill it.
                write("Sleeping for 1 minute...");
                System.Threading.Thread.Sleep(60000);

                // check for the process
                write("Checking for PhantomJS open process...");
                Process[] processes = Process.GetProcesses();
                string emsg = "Processes: ";
                foreach (Process p in processes) {
                    write(p.ProcessName);
                    emsg += p.ProcessName + "; ";
                    if (p.ProcessName.ToLower().Contains("phantomjs")) {
                        p.Kill();
                        write(p.ProcessName + " found and killed.");
                    }
                }
                //SendAzureMail.SendMail("nathan@owensdev.com", "Azure process audit", emsg);

                // wait for 5 minutes
                //Console.WriteLine("Sleeping for 5 minutes...");
                //System.Threading.Thread.Sleep(60000); // time in miliseconds (300,000 ms = 5 min)
                //} // end while

            } catch (Exception e) {
                SendAzureMail.SendMail("nathan@owensdev.com", "Error encountered when submitting ACA", e.Message + " \n\rSource: " + e.Source + " \n\rStackTrace: " + e.StackTrace);
            }

        } // end MAIN


        static string buildResponseMsg(List<CustomResponse> responses) {
            string msg = "";

            // TODO: need to build better error checks into this
            if (responses != null && responses.Count > 0) { // if response objects were created
                msg = "The submission was sent to the IRS in " + responses.Count.ToString() + " transmission(s). ";
                int counter = 1;
                foreach (CustomResponse r in responses) {
                    msg += getMsgIntro(r, counter); // gives the intro: "The ____ transmission "
                    if (r.Successful) {
                        msg += "was submitted to the IRS successfully with a Receipt ID of " + r.ReceiptId + ". It should now be visible in the ezACAfiling dashboard.";
                    } else {
                        msg += "encountered an error. " + getErrorText(r);
                    }

                    counter++;
                }
            } else { // no response objects, for some reason...
                msg = "The application encountered an error. Please notify application support.";
            }

            return msg;
        }

        static string buildVaultStorageResponseMsg(List<CustomResponse> responses, Record submissionRec) {
            StringBuilder sbuilder = new StringBuilder();

            // TODO: need to build better error checks into this
            if (responses != null && responses.Count > 0) { // if response objects were created
                sbuilder.Append("The ACA files have been generated and are ready for submission to the IRS.  The Submission's Unique Transmission ID is " + 
                    submissionRec.UniqueId + " /r/n/r/nSubmitting the data will require " + 
                    responses.Count.ToString() + " transmission(s). ");
                sbuilder.Append("\r\n\r\n" + "Links to the IRS's UI Portal can be found at the following URL:\r\n");
                sbuilder.Append("https://www.irs.gov/e-file-providers/air/affordable-care-act-information-return-air-program \r\n\r\n");
                int counter = 1;
                foreach (CustomResponse r in responses) {
                    if (!String.IsNullOrEmpty(r.FormDataFilePath) && !String.IsNullOrEmpty(r.ManifestFilePath)) {

                        // TODO: save these in the vault and provide the links in the next lines
                        var vaultLoc = saveSubmissionFilesInVault(submissionRec, r, counter);

                        sbuilder.Append(getMsgIntro(r, counter)); // gives the intro: "The __nth__ transmission "
                        if (r.Successful) {
                            sbuilder.Append("was successfully created. It should now be visible in the ezACAfiling dashboard at the following path: \r\n " + vaultLoc + "\r\n\r\n");
                        } else {
                            sbuilder.Append("encountered an error. " + getErrorText(r));
                        }

                        counter++;
                    } // end if
                } // end foreach
            } else { // no response objects, for some reason...
                sbuilder.Append("The application encountered an error. Please notify application support.");
            }

            return sbuilder.ToString();
        }



        static string saveSubmissionFilesInVault(Record submissionRec, CustomResponse resp, int transmissionCounter) {
            // create a list of the folder directories these files should be saved under
            var dirStructure = new List<string>();
            dirStructure.Add("EBS");
            dirStructure.Add("ACA");
            dirStructure.Add(submissionRec.TaxYr.ToString() + " Forms");
            dirStructure.Add(submissionRec.Company.Name);
            dirStructure.Add("Submission_" + DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss"));
            dirStructure.Add("transmission_" + transmissionCounter.ToString());

            string manifestAzurePath = AzureVaultManager.CreateVaultItem_Easy("manifest", resp.ManifestFilePath, "xml", dirStructure, false, true, user);
            string formDataAzurePath = AzureVaultManager.CreateVaultItem_Easy(resp.FormDataFileName, resp.FormDataFilePath, "xml", dirStructure, false, true, user);   

            return manifestAzurePath + " /r/n " + formDataAzurePath;
        }


        static CloudFile getSpreadsheetSetEmailSetCorrInfo() {
            CloudFile spreadsheet = null;

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("InputStorageConnectionString"));

            // Create a CloudFileClient object for credentialed access to File storage.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.
            CloudFileShare share = fileClient.GetShareReference("acafileinput");

            // Ensure that the share exists.
            if (share.Exists()) {
                // Get a reference to the root directory for the share.
                CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                // Ensure that the directory exists.
                if (rootDir.Exists()) {
                    // file appendage to match
                    string appendage = "";
                    // Get a reference to the file we created previously.
                    var contents = rootDir.ListFilesAndDirectories();

                    foreach (var item in contents) {
                        if (item.GetType() == typeof(CloudFile) && getFileExt(item.Uri.ToString()) == "xlsx") {
                            spreadsheet = (CloudFile)item;
                            appendage = getAppendage(spreadsheet.Uri.ToString());
                        }
                    }
                    // run through items again and find the email txt file that matches the spreadsheet's appendage
                    foreach (var item in contents) {
                        if (item.GetType() == typeof(CloudFile) && getFileExt(item.Uri.ToString()) == "txt"
                            && getAppendage(item.Uri.ToString()) == appendage) {
                            // then this is the email file
                            setEmail((CloudFile)item);
                            emailFile = (CloudFile)item;
                            // also contains sub type and correction info
                            setCorrectionInfoAndSubType((CloudFile)item);
                        }
                    }
                    // delete the emailFile
                    //if (emailFile != null) {
                    //    emailFile.Delete();
                    //}

                }
            }
            return spreadsheet;
        }

        static string getFileExt(string filename) {
            string ext = "";
            if (!String.IsNullOrEmpty(filename)) {
                ext = filename.Substring(filename.Length - 4);
                ext = ext.TrimStart('.');
            }

            return ext;
        }

        static string getAppendage(string spreadsheetUri) {
            if (spreadsheetUri.Contains('_') && spreadsheetUri.Contains('.')) {
                var firstArr = spreadsheetUri.Split('_');
                var secondArr = firstArr[1].Split('.');
                return secondArr[0].ToString();
            } else {
                return spreadsheetUri; // this whole thing will fail, though...
            }
        }

        static void setEmail(CloudFile file) {
            string contents = file.DownloadText();
            email = getItemFromText(contents.Trim(), "email");
        }

        static void setCorrectionInfoAndSubType(CloudFile file) {
            string contents = file.DownloadText();

            subType = getItemFromText(contents.Trim(), "subType");
            if (subType != "O") { // only assign corrInfo if it's a correction or replacement
                corrInfo.BadSubmissionReceiptId = getItemFromText(contents.Trim(), "badSubmissionReceiptId");
                corrInfo.IsReplacement = Convert.ToBoolean(getItemFromText(contents.Trim(), "isReplacement"));
                corrInfo.IsWholeTransReplacement = Convert.ToBoolean(getItemFromText(contents.Trim(), "isWholeTransReplacement"));
                // while CorrectionInfo has a RecordIds attribute, I don't think it's being used...
                // form type
                string formType = getItemFromText(contents.Trim(), "formType");
                if (!String.IsNullOrEmpty(formType) && formType == "Form1094") {
                    corrInfo.FormType = FormType.Form1094;
                } else {
                    corrInfo.FormType = FormType.Form1095;
                }
            }
        }

        // parses a string and gets the value for the provided key
        static string getItemFromText(string text, string key) {
            string value = "";
            if (!String.IsNullOrEmpty(text) && !String.IsNullOrEmpty(key)) { 
                int iStart = text.IndexOf(key + ":");
                string snippet = text.Substring(iStart);
                int iEnd = snippet.IndexOf('|'); 
                if (!String.IsNullOrEmpty(snippet) && iEnd >= 0) {
                    value = snippet.Substring(0, iEnd);
                    // remove key from the value
                    value = value.Remove(0, key.Length + 1); // the 1 accounts for the colon
                }
            }
            return value;
        }

        static byte[] getTemplateFileByteArray() {
            var template = new MemoryStream();

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("InputStorageConnectionString"));

            // Create a CloudFileClient object for credentialed access to File storage.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.
            CloudFileShare share = fileClient.GetShareReference("formbuildertemplates");

            // Ensure that the share exists.
            if (share.Exists()) {
                // Get a reference to the root directory for the share.
                CloudFileDirectory rootDir = share.GetRootDirectoryReference();

                // Ensure that the directory exists.
                if (rootDir.Exists()) {
                    // Get a reference to the file we created previously.
                    var contents = rootDir.ListFilesAndDirectories();

                    foreach (var item in contents) {
                        if (item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c.pdf")) {
                            CloudFile templateFile = (CloudFile)item;
                            templateFile.DownloadToStream(template);
                        }
                    }
                }
            }
            return template.ToArray();
        }

        static CloudFileDirectory createNewDirectory(string companyName) {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("InputStorageConnectionString"));

            // Create a CloudFileClient object for credentialed access to File storage.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.
            CloudFileShare share = fileClient.GetShareReference("formbuilderworkarea");

            // get root dir
            CloudFileDirectory rootDir = share.GetRootDirectoryReference();
            string newDirName = companyName + "_" + DateTime.Now.ToString();
            Uri newDirUri = new Uri(rootDir.Uri + "/" + newDirName);
            //CloudFileDirectory dir = new CloudFileDirectory(newDirUri);

            CloudFileDirectory dir = rootDir.GetDirectoryReference(newDirName);
            var opts = new FileRequestOptions();
            opts.DisableContentMD5Validation = true;
            if (dir.CreateIfNotExists(opts)) {
                write("dir created, I think?");
            }



            return dir;
        }


        static string createNewLocalDirectory(string companyName) {
            //var rootDir = Path.GetDirectoryName("\\\\files");
            var rootDir = AppDomain.CurrentDomain.BaseDirectory;
            if (rootDir.Contains("\\bin")) {
                var binIndex = rootDir.IndexOf(@"\bin");
                rootDir = rootDir.Substring(0, binIndex);
            }
            var filesDir = rootDir + @"\submissions";
            var newDir = filesDir + "\\" + companyName.Replace(" ", "_").Replace(".", "").Replace(",", "") + "_" + DateTime.Now.ToFileTime();
            Directory.CreateDirectory(newDir);
            return newDir;
        }


        static string getErrorText(CustomResponse r) {
            string err = "";
            if (r != null && !String.IsNullOrEmpty(r.StatusCode)) {
                err = "Error Code: " + r.StatusCode + ". ";
            }
            if (r != null && !String.IsNullOrEmpty(r.StatusMessage)) {
                err += "Message Details: " + r.StatusMessage + ". ";
            }
            return err;
        }


        static string getMsgIntro(CustomResponse r, int counter) {
            string intro = "";

            if (counter == 1) {
                intro = "The transmission ";
            } else {
                intro = "The " + getNthDescFromInt(counter) + " transmission ";
            }
            return intro;
        }


        static string getNthDescFromInt(int i) {
            string desc = "";
            int lastNum = Convert.ToInt32(i.ToString().Substring(i.ToString().Length - 1)); // gets the last character
            if (lastNum == 1) {
                desc = i.ToString() + "st";
            } else if (lastNum == 2) {
                desc = i.ToString() + "nd";
            } else if (lastNum == 3) {
                desc = i.ToString() + "rd";
            } else {
                desc = i.ToString() + "th";
            }
            return desc;
        }
    } // end class
} // end namespace
