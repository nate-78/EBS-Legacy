using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using System.Diagnostics;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;

namespace ErrorImporter {
    class Program {

        //###########################################################
        // THESE VARIABLES MUST BE SET BEFORE RUNNING THE APPLICATION
        // TAKE THE TIME TO READ THEM CAREFULLY BEFORE SUBMITTING!!!
        static string filePath = @"C:\owensdev-svn\EBS - ACAfiling\submissions\TY2024\03-23_corrs\"; // READ ALL COMMENTS FIRST!!!!!!!!
        static string fileName = "wcb-err.xml";
        static bool valid1094 = true;
        static bool validAll1095s = false;
        static string overallStatus = "Accepted with errors"; // Accepted with errors, etc
        //static string overallStatus = "Rejected";
        static string taxYrStr = "24";
        //############################################################


        //#######################  NOTE   NOTE   NOTE   NOTE   NOTE   NOTE
        // NOTE: some elements may have to be changed below EACH YEAR as the IRS makes their changes.
        // Specifically, the namespaces and XML elements could change, making the application dangerously
        // out of sync.  The Tax Year namespace will definitely need to be updated.  Also make sure the 
        // location this is being run from can sign in to the Azure database before running this app.
        //#######################
        
        /// <summary>
        /// Read error XML file from IRS and import the error details into the db
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args) {
            // get file
            var doc = XElement.Load(filePath + fileName);
            // build errors
            var errors = buildErrorList(doc);
            // insert errors and status update into db
            AcaDataManager.InsertIrsErrors(errors, valid1094, validAll1095s, overallStatus);
            var update = getStatUpdateDetails(errors.FirstOrDefault().ReceiptId);
            AcaDataManager.InsertStatusUpdate(update);
            Console.WriteLine("Updates completed.");
            Console.ReadLine();
        }


        static StatusUpdate getStatUpdateDetails(string receiptId) {
            var update = new StatusUpdate();
            update.ReceiptId = receiptId;
            update.Has1094Errors = !valid1094;
            update.Has1095Errors = !validAll1095s;
            update.StatusMessage = "";
            update.StatusType = overallStatus;
            update.StatusCode = overallStatus;
            return update;
        }

        static List<ErrorFromIrs> buildErrorList(XElement doc) {
            List<ErrorFromIrs> errors = new List<ErrorFromIrs>();

            // process the document to get the Receipt ID and all pertinent error data
            if (doc != null) {
                // namespaces
                XNamespace ns = "urn:us:gov:treasury:irs:ext:aca:air:ty" + taxYrStr;
                XNamespace ns2 = "urn:us:gov:treasury:irs:common";
                XNamespace ns3 = "urn:us:gov:treasury:irs:msg:form1094-1095BCtransmittermessage";

                XElement root = doc.Element(ns + "ACATransmitterSubmissionDetail");

                IEnumerable<XElement> errorDetailGrpElems = root.Elements(ns + "TransmitterErrorDetailGrp");

                // iterate through each to build out error results
                foreach (var elem in errorDetailGrpElems) {
                    if (elem.Descendants(ns + "SubmissionLevelStatusCd").Any()) { // first element -- gives overall submission status
                        // don't need this for now, but could be useful one day
                        //update.StatusType = elem.Element(ns + "SubmissionLevelStatusCd").Value;
                    } else {
                        // the specific errors
                        ErrorFromIrs err = new ErrorFromIrs();
                        // get receipt id and record id (part of same element)
                        string recordElemText = "";
                        string recordId = "";
                        if (elem.Elements(ns + "UniqueRecordId").Any()) {
                            recordElemText = elem.Element(ns + "UniqueRecordId").Value;
                            var arr = recordElemText.Split('|');
                            err.ReceiptId = arr[0]; // get receipt id
                            // try to get record id
                            if (arr.Length > 2) {
                                err.RecordId = Convert.ToInt32(arr[2]);
                                err.Is1095Error = true;
                            } else {
                                err.Is1095Error = false;
                                Console.WriteLine("Error encountered. UniqueRecordId does not include Receipt ID, Submission ID, and/or Record ID: " + recordElemText);
                                Console.ReadLine();
                            }
                        }

                        // get Error Code, Error Text, and Xpath
                        var detail = elem.Element(ns2 + "ErrorMessageDetail");
                        err.ErrorCode = detail.Element(ns2 + "ErrorMessageCd").Value;
                        err.ErrorMessage = DataManager.SqlSafe(detail.Element(ns2 + "ErrorMessageTxt").Value);
                        err.XpathContent = detail.Element(ns2 + "XpathContent").Value;
                        
                        errors.Add(err);
                        Console.WriteLine("Error Added:\r\n\t" + err.ErrorCode + ": " + err.ErrorMessage + " at " + err.XpathContent + "\r\n\tRecord: " + err.RecordId.ToString());
                        Console.WriteLine("");
                    }
                }
            }

            return errors;
        }
    }
}
