using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.IO.Compression;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;
using ACAfiling_Web.Models;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.Azure; // Namespace for Azure Configuration Manager
using Microsoft.WindowsAzure.Storage; // Namespace for Storage Client Library
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage
using Microsoft.WindowsAzure.Storage.File; // Namespace for File storage

namespace ACAfiling_Web.Controllers {
    public class HomeController : Controller {
        [RequireHttps]
        public ActionResult Index() {
            if (Request.IsAuthenticated) {
                ViewBag.TaxYrs = AcaDataManager.GetAllAcaTaxYrs();

                return View("Index");
            } else {
                return Redirect("//ezebshub.com");
            }
        }


        // get form for submitting data
        [RequireHttps]
        public ActionResult SubmitData() {
            if (Request.IsAuthenticated) {
                return PartialView("_SubmitData");
            } else {
                return new EmptyResult();
            }
        }


        // enter the Receipt ID (gotten manually from IRS site)
        [RequireHttps]
        public ActionResult EnterReceiptID() {
            if (Request.IsAuthenticated) {
                // get recent submissions that need a receipt id
                var user = CurrentUserSingleton.GetUser();
                var submissions = AcaDataManager.GetAllSubmissionsWithoutRecId(user);
                ViewBag.Submissions = submissions;
                return PartialView("_ReceiptIdEntry");
            } else {
                return new EmptyResult();
            }
        }


        // track submissions (manually from IRS site)
        [RequireHttps]
        public ActionResult TrackSubmissionForm() {
            if (Request.IsAuthenticated) {
                ViewBag.StatusTypes = AcaDataManager.GetAllStatusTypes();
                return PartialView("_TrackSubmissionForm");
            } else {
                return new EmptyResult();
            }
        }


        // get pending submissions
        [RequireHttps]
        public ActionResult StatusReporting(string sortType, string sortParam, bool showArchive, string taxYr) { // finish making these changes all the way through the code
            if (Request.IsAuthenticated) {
                ViewBag.Message = "No submissions to check at this time."; // show this if no records are pulled
                var currUser = CurrentUserSingleton.GetUser();
                var records = AcaDataManager.GetAllSubmissions(sortType, sortParam, showArchive, taxYr, currUser);
                if (records != null && records.Count > 0) {
                    if (records.Count == 1) {
                        ViewBag.Message = "The following submission is currently being processed by the IRS:";
                    } else {
                        ViewBag.Message = "The following submissions are currently being processed by the IRS:";
                    }
                    ViewBag.Instructions = "Click a submission to retrieve its details.";
                    ViewBag.Submissions = records;
                }

                return PartialView("_StatusReporting");
            } else {
                return new EmptyResult();
            }
        }
        

        // add the receipt id to a submission (manual process via IRS UI)
        [RequireHttps]
        public void AddReceiptId(string uid, string receiptId) {
            if (Request.IsAuthenticated) {
                if (!String.IsNullOrEmpty(uid) && !String.IsNullOrEmpty(receiptId)) {
                    AcaDataManager.AddReceiptIdToSubmission(uid, receiptId.ToUpper());
                }
            }
        }


        [HttpPost]
        [RequireHttps]
        public void TrackStatusUpdate_Manual(FormCollection collection) {
            if (Request.IsAuthenticated) {
                var update = buildUpdateFromForm(collection);
                var errors = buildErrorsFromUpdate(collection);
                
                // insert the update
                AcaDataManager.InsertStatusUpdate(update);
                // save any errors
                if (errors.Count > 0) {
                    AcaDataManager.InsertIrsErrors(errors, !update.Has1094Errors, !update.Has1095Errors, update.StatusType);
                }
            }
        }


        // method called via Ajax
        // submits the data -- will either go via SOAP or UI, depending on the settings in TransmissionController within the ACAfiling Azure WebJob
        [HttpPost]
        [RequireHttps]
        public void SubmitData(HttpPostedFileBase uploadedFile, FormCollection collection) {
            if (Request.IsAuthenticated) {
                // borrowed idea from DayBroker: ListingController, uploadListingFile function

                // create filename for the attachment and save it
                string filePath = saveSubmissionAttachment(uploadedFile);

                // is file valid? is it .xls or .xlsx?
                if (isValidFileType(filePath)) {
                    // get radiobtn value
                    string subType = "";
                    string rejectionType = "";
                    if (collection.AllKeys.Contains("sub-type")) {
                        subType = collection["sub-type"].ToString();
                    }
                    if (collection.AllKeys.Contains("rejection-type")) {
                        rejectionType = collection["rejection-type"].ToString();
                    }
                    // get correction info. This is used whenever a submission is a replacement or correction
                    var correctionInfo = getCorrectionInfo(subType, rejectionType, collection);

                    // store file in Azure storage and let ACAfiling project (WebJob) handle the submission
                    var user = CurrentUserSingleton.GetUser();
                    string fileAppender = getRandomString();
                    uploadSpreadsheet(filePath, user.Email, fileAppender, subType, correctionInfo);
                } // if valid file type...
                
            } 
        }


        // OLD VERSION -- 11/23/16
        // method called via Ajax
        // submits the data -- will either go via SOAP or UI, depending on the settings in TransmissionController
        //[HttpPost]
        //[RequireHttps]
        //public ActionResult SubmitData(HttpPostedFileBase uploadedFile, FormCollection collection) {
        //    if (Request.IsAuthenticated) {
        //        // borrowed idea from DayBroker: ListingController, uploadListingFile function

        //        // create filename for the attachment and save it
        //        string filePath = saveSubmissionAttachment(uploadedFile);

        //        // is file valid? is it .xls or .xlsx?
        //        if (isValidFileType(filePath)) {
        //            // get radiobtn value
        //            string subType = "";
        //            string rejectionType = "";
        //            if (collection.AllKeys.Contains("sub-type")) {
        //                subType = collection["sub-type"].ToString();
        //            }
        //            if (collection.AllKeys.Contains("rejection-type")) {
        //                rejectionType = collection["rejection-type"].ToString();
        //            }
        //            // get correction info. This is used whenever a submission is a replacement or correction
        //            var correctionInfo = getCorrectionInfo(subType, rejectionType, collection);


        //            // PARSE DATA AND CONVERT INTO RECORD OBJECT
        //            Record submissionRecord = ReportBuilder.CreateSubmissionRecord(filePath, subType, correctionInfo);
        //            submissionRecord.Directory = createDirectory(submissionRecord);

        //            ViewBag.notes = echoRecordData(submissionRecord);

        //            // submit data
        //            //if (CustomError.ErrorCount == 0) { // if no errors...
        //            // submit
        //            var responses = TransmissionController.SubmitAcaRecord(submissionRecord, correctionInfo);
        //            ViewBag.Responses = responses;

        //            // log the submission
        //            var user = CurrentUserSingleton.GetUser();
        //            HubDataManager.LogActivity(user.ID, user.ImpersonatingAs, "submission", null, responses.First().ReceiptId, "receipt-id");

        //            //} // if no errors...
        //        } // if valid file type...

        //        ViewBag.errors = CustomError.ErrorMessage;
        //        ViewBag.errorCount = CustomError.ErrorCount;

        //        // return partial view (what if there's no data, because file isn't valid?)
        //        return PartialView("_ResponsePartial");
        //    } else {
        //        return new EmptyResult();
        //    }
        //}


        // get all saved updates for a given submission
        [HttpPost]
        [RequireHttps]
        public ActionResult GetUpdatesForSubmission(string uid) {
            if (Request.IsAuthenticated) {
                if (!String.IsNullOrEmpty(uid)) {
                    var records = AcaDataManager.GetAllUpdatesForSubmission(uid);
                    ViewBag.Records = records;
                } else {
                    ViewBag.Message = "There are no status updates for this submission.";
                }

                return PartialView("_StatusUpdatesNew");
            } else {
                return new EmptyResult();
            }
        }

        [HttpPost]
        [RequireHttps]
        public ActionResult GetErrorsForSubmission(string receiptId) {
            if (Request.IsAuthenticated) {
                ViewBag.Errors = AcaDataManager.GetSubmissionErrors(receiptId);
                return PartialView("_ErrorList");
            } else {
                return new EmptyResult();
            }
        }


        [HttpPost]
        [RequireHttps]
        public ActionResult GetStatusUpdateFromIrs(string receiptId) {
            if (Request.IsAuthenticated) {
                //var update = TransmissionController.GetStatusUpdate(receiptId); // SOAP method
                //var update = TransmissionController.GetStatusUpdateUI(receiptId, getDefaultDirectoryPath());  // UI method
                uploadStatusUpdateFile(receiptId);

                // submit update to DB if the status update came back correctly
                //if (update != null && update.StatusType != "Error" &&
                //    (update.StatusMessage == null ||
                //    (update.StatusMessage != null && !update.StatusMessage.Contains("encountered an unspecified error")))) {
                //    AcaDataManager.InsertStatusUpdate(update, "");
                //}

                // log activity
                var user = CurrentUserSingleton.GetUser();
                //HubDataManager.LogActivity(user.ID, user.ImpersonatingAs, "status_update", null, update.ID.ToString(), "status-update-id");
                HubDataManager.LogActivity(user.ID, user.ImpersonatingAs, "status_update", null, receiptId, "receipt-id");

                // display status update details in partial view
                //ViewBag.Update = update;
                //return PartialView("_StatusUpdateSingle");
                return new EmptyResult();
            } else {
                return new EmptyResult();
            }
        }


        // this function is called via AJAX from the browser every minute, but only if at least one 
        // submission in the dashboard has an ellipsis gif sitting in it.  That means the user 
        // has recently requested a status update, so we need to check the DB to see if it's
        // gone through yet.
        [HttpPost]
        [RequireHttps]
        public ActionResult GetLatestStatusUpdateByReceiptId(string receiptId) {
            try {
                if (Request.IsAuthenticated) {
                    List<Record> recs = AcaDataManager.GetAllUpdatesForSubmissionByReceiptId(receiptId);
                    Record latestRec = recs.First();
                    if (hasUpdate(latestRec)) {
                        ViewBag.LatestRecord = latestRec;
                        return PartialView("_StatusUpdateSingleNew");
                    } else {
                        return new EmptyResult();
                    }


                } else {
                    return new EmptyResult();
                }
            } catch {
                return new EmptyResult();
            }
        }


        public void ArchiveSubmission(string receiptId, bool isArchived) {
            if (Request.IsAuthenticated && !String.IsNullOrEmpty(receiptId)) {
                AcaDataManager.ArchiveSubmission(receiptId, isArchived);

                // log the activity
                var user = CurrentUserSingleton.GetUser();
                HubDataManager.LogActivity(user.ID, user.ImpersonatingAs, "archived_submission", null, receiptId, "receipt-id");
            }
        }


        // NOT NEEDED RIGHT NOW
        // get 'sort by' options 
        public string GetSortByOptions() {
            string options = "";

            return options;
        }


        // get terms that a user can sort by
        public string GetSearchTerms(string sortType, string searchTerm) {
            if (Request.IsAuthenticated) {
                string result = AcaDataManager.GetSortByTerms(sortType, searchTerm);

                return result;
            } else {
                return "";
            }
        }


        // get receipt ids of submissions that might need a correction or replacement
        //[HttpPost]
        public ActionResult GetReceiptIds(string subType) { // based on submission type
            if (Request.IsAuthenticated) {
                if (!String.IsNullOrEmpty(subType)) {
                    List<Record> recs = AcaDataManager.GetReceiptIdsForMakingCorrOrReplacements(subType);
                    ViewBag.Records = recs;
                    ViewBag.SubType = subType;

                    return PartialView("_ReceiptIdList");
                } else {
                    return new EmptyResult();
                }
            } else {
                return new EmptyResult();
            }
        }
        


        // HELPER FUNCTIONS

        // save the initial attachment and return filename
        private string saveSubmissionAttachment(HttpPostedFileBase uploadedFile) {
            string filePath = Server.MapPath(System.Configuration.ConfigurationManager.AppSettings["FilesLocation"]) + "/" +
                getTimestamp(DateTime.Now) + "_" + uploadedFile.FileName.Replace("\\", "").Replace("/", "");
            if (uploadedFile.ContentLength > 0) {
                using (FileStream fileToSave = new FileStream(filePath, FileMode.Create)) {
                    uploadedFile.InputStream.CopyTo(fileToSave);
                }
            }

            // if .zip, extract to a location and determine contents
            if (filePath.IndexOf(".zip") > -1) {
                filePath = unzip(filePath); // having trouble with this method... something in the decompression stream
            }

            return filePath;
        }


        private string getDefaultDirectoryPath() {
            return Server.MapPath(System.Configuration.ConfigurationManager.AppSettings["FilesLocation"]) + "/";
        }


        private string unzip(string filePath) {
            // figure out how to uncompress...
            string newPath = filePath;
            FileInfo file = new FileInfo(filePath);
            using (FileStream originalFileStream = file.OpenRead()) {
                originalFileStream.Seek(0, SeekOrigin.Begin);
                string currentFileName = file.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - file.Extension.Length);
                using (FileStream decompressedFileStream = System.IO.File.Create(newFileName)) {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress)) {
                        decompressionStream.CopyTo(decompressedFileStream);
                        newPath = decompressedFileStream.Name;
                    }
                }
            }

            return newPath;
        }

        // build the correctioninfo object
        private CorrectionInfo getCorrectionInfo(string submissionType, string rejectionType, FormCollection collection) {
            var ci = new CorrectionInfo();
            if (submissionType != "O") {
                // set receipt id for submission that's being corrected/replaced
                if (collection.AllKeys.Contains("receipt-id-list")) {
                    ci.BadSubmissionReceiptId = collection["receipt-id-list"].ToString();
                }
                // set form type
                if (collection.AllKeys.Contains("form-type")) {
                    string formType = "";
                    formType = collection["form-type"].ToString();
                    if (formType == "1094") {
                        ci.FormType = FormType.Form1094;
                    } else {
                        ci.FormType = FormType.Form1095;
                    }
                }

                // if rejection, find out which kind
                if (submissionType == "R" && !String.IsNullOrEmpty(rejectionType)) {
                    // need receipt id. will either build this as receipt id or submission id (receiptId|1)
                    if (rejectionType == "transmission") {
                        ci.IsWholeTransReplacement = true;
                    } else {
                        ci.IsWholeTransReplacement = false;
                    }
                }
            }

            return ci;
        }

        private bool isValidFileType(string fileName) {
            bool answer = false;
            if (fileName.IndexOf(".xlsx") > -1) {
                answer = true;
            } else if (fileName.IndexOf(".xls") > -1) {
                answer = true;
            }

            return answer;
        }

        private string getTimestamp(DateTime value) {
            return value.ToString("yyyyMMddHHmmssfff");
        }

        private string echoRecordData(Record record) {
            string message = "<div class='result'><p>This submission was for <strong> " + record.Company.Name +
                "</strong> and consisted of <strong>" + record.Ct1095Transmittal.ToString() + " records</strong>.</p></div>";

            return message;
        }

        private string singleMessage(string label, string value) {
            string msg = "<strong>" + label + ":</strong> " + value + "<br />";

            return msg;
        }


        private string createDirectory(Record r) {
            // Server.MapPath(System.Configuration.ConfigurationManager.AppSettings["FilesLocation"])
            // HostingEnvironent.MapPath(...
            string d = Server.MapPath(System.Configuration.ConfigurationManager.AppSettings["FilesLocation"]) + "/" +
                r.Company.Name.Replace(" ", "_").Replace(".", "").Replace(",", "") + "_" + DateTime.UtcNow.ToString("yyyy-MM-dd_hh-mm-ss");
            Directory.CreateDirectory(d); // create the directory

            return d;
        }

        private bool hasUpdate(Record rec) {
            bool result = false;
            if ((!String.IsNullOrEmpty(rec.Current1094Status) && rec.Current1094Status != "Processing")
                || (!String.IsNullOrEmpty(rec.Current1095Status) && rec.Current1095Status != "Processing")) {

                result = true;
            }
            return result;
        }

        private List<ErrorFromIrs> buildErrorsFromUpdate(FormCollection col) {
            var errors = new List<ErrorFromIrs>();
            var errCounter = 1;
            while (col.AllKeys.Contains("stat-update-error-code_" + errCounter.ToString())) {
                if (!String.IsNullOrEmpty(col["stat-update-error-code_" + errCounter.ToString()])) {
                    var err = new ErrorFromIrs();
                    // these are the only fields necessary for adding these to the db
                    err.ErrorCode = col["stat-update-error-code_" + errCounter.ToString()];
                    err.ErrorMessage = col["stat-update-error-msg_" + errCounter.ToString()];
                    err.ReceiptId = col["stat-update-rid"];
                    err.XpathContent = col["stat-update-xpath_" + errCounter.ToString()];
                    err.RecordId = Convert.ToInt32(col["stat-update-record-id_" + errCounter.ToString()]);
                    errors.Add(err);
                }
                errCounter++;
            }

            return errors;
        }

        private StatusUpdate buildUpdateFromForm(FormCollection col) {
            var s = new StatusUpdate();
            if (col.AllKeys.Contains("stat-update-rid")) {
                s.ReceiptId = col["stat-update-rid"];
                s.StatusCode = col["stat-update-status"];
                s.StatusType = col["stat-update-status"];
                s.StatusMessage = col["stat-update-msg"];

                // were there any 1094 or 1095 errors?
                if (col["stat-update-has-errors"] == "hasErrors") {
                    foreach (var k in col.AllKeys) {
                        if (k.Contains("stat-update-record-id")) {
                            if (!String.IsNullOrEmpty(col[k])) {
                                s.Has1095Errors = true;
                            } else {
                                s.Has1094Errors = true;
                            }
                        }
                    }
                    if (s.Has1095Errors != true) { // not sure this is necessary...
                        s.Has1095Errors = false;
                    }
                    if (s.Has1094Errors != true) { // not sure this is necessary...
                        s.Has1094Errors = false;
                    }
                } else {
                    s.Has1094Errors = false;
                    s.Has1095Errors = false;
                }

            }
            return s;
        }

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // the following functions help interact with the Vault
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private static string getRandomString() {
            int length = 20;
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void uploadSpreadsheet(string spreadsheetPath, string email, string fileAppender, string subType, CorrectionInfo corrInfo) {
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
                    // upload the spreadsheet
                    CloudFile spreadsheet = rootDir.GetFileReference("spreadsheet_" + fileAppender + ".xlsx");
                    Stream fileStream = System.IO.File.OpenRead(spreadsheetPath);
                    // upload it to Azure
                    spreadsheet.UploadFromStreamAsync(fileStream).Wait();
                    fileStream.Dispose();

                    // create the email file (will include other info, too)
                    CloudFile emailFile = rootDir.GetFileReference("email_" + fileAppender + ".txt");
                    Stream emailFileStream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(emailFileStream);
                    writer.WriteLine("email:" + email + "|");
                    writer.WriteLine("subType:" + subType + "|");
                    if (corrInfo == null) {
                        writer.WriteLine("badSubmissionReceiptId:|isReplacement:|isWholeTransReplacement:|formType:|");
                    } else {
                        writer.WriteLine("badSubmissionReceiptId:" + corrInfo.BadSubmissionReceiptId + "|");
                        writer.WriteLine("isReplacement:" + corrInfo.IsReplacement.ToString() + "|");
                        writer.WriteLine("isWholeTransReplacement:" + corrInfo.IsWholeTransReplacement.ToString() + "|");
                        writer.WriteLine("formType:" + corrInfo.FormType.ToString() + "|");
                    }

                    writer.Flush();
                    emailFileStream.Position = 0; // position the stream back to the beginning
                    emailFile.UploadFromStreamAsync(emailFileStream).Wait(); // upload the file
                    emailFileStream.Dispose();
                } // end if
            } // end if
        }


        private static void uploadStatusUpdateFile(string receiptId) {
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

                    // create the update file 
                    CloudFile updateFile = rootDir.GetFileReference("update_" + DateTime.Now.ToFileTime() + ".txt");
                    Stream updateFileStream = new MemoryStream();
                    StreamWriter writer = new StreamWriter(updateFileStream);
                    writer.WriteLine(receiptId);

                    writer.Flush();
                    updateFileStream.Position = 0; // position the stream back to the beginning
                    updateFile.UploadFromStreamAsync(updateFileStream).Wait(); // upload the file
                    updateFileStream.Dispose();
                } // end if
            } // end if
        }

    }
}