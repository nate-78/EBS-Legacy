using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;

using Microsoft.Azure; // Namespace for Azure Configuration Manager
using Microsoft.WindowsAzure.Storage; // Namespace for Storage Client Library
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage
using Microsoft.WindowsAzure.Storage.File; // Namespace for File storage

namespace PdfCreatorWebJob {
    class Program {
        private static string email; // global variable to hold email address


        // comment out the interior of this function for production
        // also comment out the Console.SetWindowSize(84, 84); line below (line 30)
        static void writeLine(string text) {
            Console.WriteLine(text);
            //EbsClassPkg.Controllers.SendAzureMail.SendMail("nathan@owensdev.com", "PDF Builder: console log", text);
        }

        static void Main(string[] args) {
            // ORIGINALLY, THE PDF CREATION FUNCTIONS WERE PART OF THE EBS HUB.  HOWEVER, TIMEOUT ISSUES SHOWED THAT RUNNING 
            // THIS FUNCTION IN THE BACKGROUND AS A WEB JOB MADE MUCH MORE SENSE. THAT'S WHAT THIS APPLICATION IS FOR.

            //Console.SetWindowSize(84, 84);

            // run this process continually with 5 minutes in between each check
            while (true) {
                try {
                    // check for spreadsheet in the appropriate directory and set the email address
                    writeLine("Beginning...");
                    CloudFile file = getSpreadsheetAndSetEmail();

                    //EbsClassPkg.Controllers.SendAzureMail.SendMail("nathan@owensdev.com", "test message", "Got to line 40");

                    // if not exists, return
                    if (file != null) {
                        // else, build Record object
                        MemoryStream stream = new MemoryStream();
                        file.DownloadToStream(stream);
                        CorrectionInfo ci = new CorrectionInfo();
                        List<string> nums = new List<string>();
                        ci.RecordIds = nums; // instantiates the RecordIds object
                        Record rec = PdfReportBuilder.CreateSubmissionRecord(stream, "O", ci); // for some reason, when running this through the EbsClassPkg ReportBuilder, it just won't work

                        if (rec != null) {
                            writeLine("Record created from data for " + rec.Company.Name + "."); 
                        }

                        // create a directory name for this collection of files
                        string newDir = createNewLocalDirectory(rec.Company.Name);
                        writeLine("Created new local directory: " + newDir);
                        //CloudFileDirectory dir = createNewDirectory(rec.Company.Name);

                        // build PDFs
                        var template = getTemplateFileByteArray(rec);

                        // TODO: 3/24/24
                        // Unfortunately, the code that's been working for years will no longer work, and it fails at the following function.
                        // Instead of putting these files into Azure Storage, I need to just build them locally, then email manually.
                        //string consolidatedFileLink = PdfCreator.Fill1095cToLocalFile(template, rec, newDir, email);

                        bool isFileCreated = PdfCreator.Fill1095cToTrulyLocalFile(template, rec, newDir, email);

                        writeLine("Task completed. Files created.");

                        // delete spreadsheet if we've gotten a usable file link
                        if (isFileCreated) {
                            file.Delete();
                            writeLine("Spreadsheet deleted.");
                        }

                        // email link to consolidated PDF to user // if this works okay, then I can remove the SendMail class (and SendGrid pkgs) from PdfCreator project
                        //string msg = "You can retrieve your file from myVault in the EBSHub.  Just look for the file named '" + consolidatedFileLink + "'";
                        //EbsClassPkg.Controllers.SendAzureMail.SendMail(email, "Your form is ready!", msg);

                        writeLine("Email sent. Will now sleep for several seconds and check again.\r\n\r\r");

                    } else {
                        writeLine("No files present.  Will sleep for several seconds and check again. \r\n\r\r");
                    } // end if-else

                    // reset email variable
                    email = "";
                    System.Threading.Thread.Sleep(10000); // num of miliseconds (300,000 ms = 5 minutes)

                } catch (Exception e) {
                    writeLine("Error encountered:\r\n");
                    writeLine(e.Message);
                    writeLine(e.Source);
                    writeLine(e.StackTrace);
                    // email error
                    string msg = "Error Details: \n" + e.Message + "\n\n||| \n\n" + e.Source + "\n\n||| \n\n" + e.StackTrace;
                    EbsClassPkg.Controllers.SendAzureMail.SendMail("nathan@owensdev.com", "EBS FormBuilder Error", msg);
                }
                
            } // end while

        }

        static CloudFile getSpreadsheetAndSetEmail() {
            CloudFile spreadsheet = null;

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("InputStorageConnectionString"));

            // Create a CloudFileClient object for credentialed access to File storage.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.
            CloudFileShare share = fileClient.GetShareReference("formbuilderinput");

            // Ensure that the share exists.
            if (share.Exists()) {
                writeLine("Form Builder Input fileshare exists. Begin looking for files...");
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
                            writeLine("Found spreadsheet.");
                            spreadsheet = (CloudFile)item;
                            appendage = getAppendage(spreadsheet.Uri.ToString());
                            writeLine("File appendage is " + appendage);
                            break; // break out of the loop when something's found
                        }
                    }
                    // run through items again and find the email txt file that matches the spreadsheet's appendage
                    CloudFile emailFile = null;
                    foreach (var item in contents) {
                        if (item.GetType() == typeof(CloudFile) && getFileExt(item.Uri.ToString()) == "txt" 
                            && getAppendage(item.Uri.ToString()) == appendage) {
                            // then this is the email file
                            setEmail((CloudFile)item);
                            emailFile = (CloudFile)item;
                            writeLine("Found the txt file containing the user's email: " + email + ".");
                            break; // break out of the loop when match is found
                        }
                    }
                    // delete the emailFile
                    if (emailFile != null) {
                        emailFile.Delete();
                        writeLine("Email txt file deleted.");
                    }
                    
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
            email = contents.Trim();
        }

        static byte[] getTemplateFileByteArray(Record rec) {
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
                        if (( item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c.pdf") && rec.TaxYr == 2016 ) ||
                            ( item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c--2017.pdf") && rec.TaxYr == 2017 ) ||
                            ( item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c--2018.pdf") && rec.TaxYr == 2018 ) || 
                            ( item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c--2019.pdf") && rec.TaxYr == 2019 ) ||
                            ( item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c--2020.pdf") && rec.TaxYr == 2020 ) ||
                            ( item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c--2021.pdf") && rec.TaxYr == 2021 ) ||
                            // let's stop hardcoding this and let the software do the work!
                            ( item.GetType() == typeof(CloudFile) && item.Uri.ToString().Contains("f1095c--" + rec.TaxYr.ToString()) )) {

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
                writeLine("dir created, I think?");
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
            var filesDir = rootDir + @"\files";
            var newDir = filesDir + "\\" + removeSpecChars(companyName) + "_" + DateTime.Now.ToFileTime();
            //var newDir = filesDir + "\\" + companyName + "_" + DateTime.Now.ToString("yyyy-MM-dd_H-mm-ss");
            Directory.CreateDirectory(newDir);
            return newDir;
        }


        static string removeSpecChars(string text) {
            string result = text.Replace(",", "").Replace(".", "").Replace("&", "").Replace("|", "").Replace("(", "").Replace(")", "").Replace("'", "").Replace("\"", "");
            result = result.Replace("\\", "").Replace("/", "").Replace("?", "").Replace("<", "").Replace(">", "").Replace("%", "").Replace("$", "").Replace("#", "").Replace("@", "");
            result = result.Replace("!", "").Replace("^", "").Replace("~", "").Replace("*", "").Replace(";", "").Replace(":", "").Replace("-", "").Replace("+", "");
            return result;
        }

    }
}
