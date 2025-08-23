using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using EbsClassPkg.Models;


namespace EbsClassPkg.Controllers {
    // functions to interact with all the data that the ACA functions need
    public class AcaDataManager {
        // ACA FILING 
        // archive submission
        public static void ArchiveSubmission(string receiptId, bool isArchived = true) {
            SqlDataReader reader;
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_ArchiveSubmission", conn);
                // now assign parameters
                if (cmd != null) {
                    cmd.Parameters.Add("@receiptId", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(receiptId) ? DBNull.Value : (object)receiptId;
                    cmd.Parameters.Add("@isArchived", SqlDbType.Bit).Value = isArchived; // will always have a value, no need to check for null

                } // end if
                // execute it
                reader = cmd.ExecuteReader();
            } // end using statement
        }


        // get client list (for acafiling)
        public static List<Company> GetClientList_Aca(User currUser) {
            List<Company> companies = new List<Company>();
            SqlDataReader reader;
            DataSet compInfo = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetClientList", conn);
                cmd.Parameters.Add("@client_of", SqlDbType.Int).Value = String.IsNullOrEmpty(currUser.Company.ID.ToString()) ? DBNull.Value : (object)currUser.Company.ID;
                // execute it
                reader = cmd.ExecuteReader();
                compInfo.Load(reader, LoadOption.OverwriteChanges, "companies"); // this overwrites the name of the datatable(s)
            }
            // add data to User object
            if (compInfo.Tables[0].Rows.Count > 0) {
                foreach (DataRow row in compInfo.Tables[0].Rows) {
                    Company comp = new Company();
                    comp = HubDataManager.BuildCompany(row);
                    companies.Add(comp);
                }
            }

            return companies;
        }


        // insert submission
        public static void InsertSubmission(Record rec, CorrectionInfo ci, User currUser) {
            SqlDataReader reader;
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_InsertSubmission1", conn);
                // now assign parameters
                if (cmd != null) {
                    cmd.Parameters.Add("@submissionTypeId", SqlDbType.NVarChar);
                    if (cmd.Parameters["@submissionTypeId"] != null) {
                        if (String.IsNullOrEmpty(rec.SubmissionType)) {
                            cmd.Parameters["@submissionTypeId"].Value = DBNull.Value;
                        } else {
                            cmd.Parameters["@submissionTypeId"].Value = rec.SubmissionType;
                        }
                    }
                    cmd.Parameters.Add("@numRecords", SqlDbType.Int).Value = rec.Ct1095Transmittal;
                    // parent receipt id (will use it to get parent's actual submission id)
                    if (rec.SubmissionType != "O" && ci != null && !String.IsNullOrEmpty(ci.BadSubmissionReceiptId)) {
                        cmd.Parameters.Add("@parentReceiptId", SqlDbType.NVarChar).Value = ci.BadSubmissionReceiptId;
                    }

                    cmd.Parameters.Add("@uid", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.UniqueId) ? DBNull.Value : (object)rec.UniqueId;
                    cmd.Parameters.Add("@fileName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.FileName) ? DBNull.Value : (object)rec.FileName;
                    cmd.Parameters.Add("@irsFileName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.IrsFileName) ? DBNull.Value : (object)rec.IrsFileName;
                    cmd.Parameters.Add("@compName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.Name) ? DBNull.Value : (object)rec.Company.Name;
                    cmd.Parameters.Add("@compName2", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.Name2) ? DBNull.Value : (object)rec.Company.Name2;
                    cmd.Parameters.Add("@ein", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.EIN) ? DBNull.Value : (object)rec.Company.EIN;
                    cmd.Parameters.Add("@street1", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.Address.Address1) ? DBNull.Value : (object)rec.Company.Address.Address1;
                    cmd.Parameters.Add("@street2", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.Address.Address2) ? DBNull.Value : (object)rec.Company.Address.Address2;
                    cmd.Parameters.Add("@city", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.Address.City) ? DBNull.Value : (object)rec.Company.Address.City;
                    cmd.Parameters.Add("@state", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.Address.State) ? DBNull.Value : (object)rec.Company.Address.State;
                    cmd.Parameters.Add("@zip", SqlDbType.Int).Value = String.IsNullOrEmpty(rec.Company.Address.Zip.ToString()) ? DBNull.Value : (object)rec.Company.Address.Zip;
                    cmd.Parameters.Add("@zipExt", SqlDbType.Int).Value = String.IsNullOrEmpty(rec.Company.Address.ZipExt.ToString()) ? DBNull.Value : (object)rec.Company.Address.ZipExt;
                    cmd.Parameters.Add("@country", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.Address.Country) ? DBNull.Value : (object)rec.Company.Address.Country;
                    cmd.Parameters.Add("@contactFirstName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.ContactPerson.FirstName) ? DBNull.Value : (object)rec.Company.ContactPerson.FirstName;
                    cmd.Parameters.Add("@contactMidName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.ContactPerson.MiddleName) ? DBNull.Value : (object)rec.Company.ContactPerson.MiddleName;
                    cmd.Parameters.Add("@contactLastName", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.ContactPerson.LastName) ? DBNull.Value : (object)rec.Company.ContactPerson.LastName;
                    cmd.Parameters.Add("@contactSuffix", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.ContactPerson.Suffix) ? DBNull.Value : (object)rec.Company.ContactPerson.Suffix;
                    cmd.Parameters.Add("@phone", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(rec.Company.ContactPhone) ? DBNull.Value : (object)rec.Company.ContactPhone;
                    cmd.Parameters.Add("@taxYr", SqlDbType.Int).Value = String.IsNullOrEmpty(rec.TaxYr.ToString()) ? DBNull.Value : (object)rec.TaxYr;
                    cmd.Parameters.Add("@managing_companySubscriber_id", SqlDbType.Int).Value = String.IsNullOrEmpty(currUser.Company.ID.ToString()) ? DBNull.Value : (object)currUser.Company.ID;

                    // for corrections, add the correction type
                    if (rec.SubmissionType == "C") {
                        if (rec.Is1094Correction) {
                            cmd.Parameters.Add("@correctionType", SqlDbType.NVarChar).Value = "1094-C";
                        } else {
                            cmd.Parameters.Add("@correctionType", SqlDbType.NVarChar).Value = "1095-C";
                        }
                    }

                } // end if
                // execute it
                reader = cmd.ExecuteReader();
            } // end using statement
        }


        // get all receipt ids (and other data) for submissions that could qualify for replacement or correction submissions,
        // based on the type of submission the user is performing (correction or replacement). Each of those has specific 
        // status-types that apply... search for those
        public static List<Record> GetReceiptIdsForMakingCorrOrReplacements(string submissionType) {
            var recs = new List<Record>();
            var statType = "Accepted%"; // the '%' will let us grab "Accepted" and "Accepted with Errors"

            if (submissionType == "R") { // "R" for "Replacement"
                statType = "Rejected";
            }

            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetReceiptIdsByStatusType", conn);
                cmd.Parameters.Add("@statusType", SqlDbType.NVarChar).Value = statType;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            // convert data to record object
            if (ds.Tables.Count > 0) {
                recs = convertDataToRecordCollection(ds.Tables[0]);
            }

            return recs;
        }


        // insert status update
        /// <summary>
        /// Inserts a Status Update
        /// </summary>
        /// <param name="update"></param>
        /// <param name="utid">This parameter is only needed if there's no ReceiptID available in the preceding parameter.</param>
        public static void InsertStatusUpdate(StatusUpdate update, string utid = null) {
            SqlDataReader reader;
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_InsertStatusUpdate", conn);
                // now assign parameters
                cmd.Parameters.Add("@uid", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(utid) ? DBNull.Value : (object)utid;
                cmd.Parameters.Add("@statusType", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(update.StatusType) ? DBNull.Value : (object)update.StatusType;
                cmd.Parameters.Add("@message", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(update.StatusMessage) ? DBNull.Value : (object)update.StatusMessage;
                cmd.Parameters.Add("@statusCode", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(update.StatusCode) ? DBNull.Value : (object)update.StatusCode;
                cmd.Parameters.Add("@receiptId", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(update.ReceiptId) ? DBNull.Value : (object)update.ReceiptId;

                // execute it
                reader = cmd.ExecuteReader();
            }
        }


        // get all Status Types
        public static List<string> GetAllStatusTypes() {
            var types = new List<string>();
            SqlDataReader reader;
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAllStatusTypes", conn);

                // execute it
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            if (ds.Tables != null && ds.Tables.Count > 0) {
                foreach (DataRow row in ds.Tables[0].Rows) {
                    types.Add(row["status_type"].ToString());
                }
            }
            return types;
        }


        // add the receipt id to the submission
        public static void AddReceiptIdToSubmission(string uid, string receiptId) {
            SqlDataReader reader;
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_UpdateSubmission", conn);
                // now assign parameters
                cmd.Parameters.Add("@uid", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(uid) ? DBNull.Value : (object)uid;
                cmd.Parameters.Add("@receiptId", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(receiptId) ? DBNull.Value : (object)receiptId;

                // execute it
                reader = cmd.ExecuteReader();
            }
        }

        // insert errors from IRS status update
        public static void InsertIrsErrors(List<ErrorFromIrs> errors, bool form1094Accepted, bool all1095sAccepted, string overallStatus) {
            string sql = "";
            // Maybe send to db every 500 records or so?
            int counter = 0;
            // build string of stored proc executions to add all the records to the db
            if (errors != null && errors.Count > 0) {
                // first, update overall 1094 an 1095 status for the submission, if appropriate
                if (overallStatus == "Rejected") {
                    sql = "exec p_Set1094Status 'Rejected', '" + errors.First().ReceiptId + "'; ";
                    sql += "exec p_Set1095Status 'Rejected', '" + errors.First().ReceiptId + "';";
                } else {


                    if (form1094Accepted) { // need to change these to calling a stored proc, because we need to 
                                            // first check and see what kind of submission this was (using receipt id). Was it an 
                                            // original, correction, replacement...?
                                            // we'll only update 1094 and 1095 columns if the submission is an original or replacement.
                                            // a correction won't work: 1094 corrections will only get this far if they have an error. Same for 1095 corrections
                        sql = "exec p_Set1094Status 'Accepted', '" + errors.First().ReceiptId + "'; ";
                    } else {
                        sql = "exec p_Set1094Status '" + overallStatus + "', '" + errors.First().ReceiptId + "'; ";
                    }
                    if (all1095sAccepted) {
                        sql += "exec p_Set1095Status 'Accepted', '" + errors.First().ReceiptId + "'; ";
                    } else {
                        sql += "exec p_Set1095Status '" + overallStatus + "', '" + errors.First().ReceiptId + "'; ";
                    }
                }
                foreach (ErrorFromIrs err in errors) {
                    // the 'p_InsertIrsError' stored proc expects the following parameters:
                    // @receiptId, @errorCode, @errorMsg, @xpath, @recordId
                    sql += "exec p_InsertIrsError '" + err.ReceiptId + "', '" + err.ErrorCode + "', '" + DataManager.SqlSafe(err.ErrorMessage) + "', '" +
                        DataManager.SqlSafe(err.XpathContent) + "', " + err.RecordId.ToString() + "; ";

                    // increment counter
                    counter++;
                    // if we've hit about 500 records, go ahead and submit
                    if (counter >= 500) {
                        DataManager.IssueStatement(sql);
                        counter = 0; // reset counter
                        sql = ""; // reset sql string
                    }
                }

                DataManager.IssueStatement(sql); // issue any statements that didn't break 500 threshold
            }
        }

        // IMPORTANT: This function might cause an issue if it grows too long... will probably need to build in a governor to make 
        // multiple calls, if needed.  
        public static void InsertIndividualRecords(List<IndividualReport> records, string utid) {
            string sql = "";
            // Maybe send to db every 500 records or so?
            int counter = 0;
            // build string of stored proc executions to add all the records to the db
            if (records != null && records.Count > 0) {
                foreach (IndividualReport r in records) {
                    // the 'p_InsertRecord' stored proc expects the following parameters:
                    // @utid, @irsRecordId, @firstName, @midName, @lastName, @suffix, @lastn
                    sql += "exec p_InsertRecord '" + utid + "', '" + DataManager.SqlSafe(r.RecordId) + "', '" + DataManager.SqlSafe(r.Person.FirstName) + "', '" +
                        DataManager.SqlSafe(r.Person.MiddleName) + "', '" + DataManager.SqlSafe(r.Person.LastName) + "', '" + DataManager.SqlSafe(r.Person.Suffix) + "', '" +
                        PasswordStorage.CreateHash(r.Person.LastFourSSN) + "'; "; // encrypt last 4 ssn before storing

                    // increment counter
                    counter++;
                    // if we've hit about 500 records, go ahead and submit
                    if (counter >= 30) {
                         DataManager.IssueStatement(sql);
                        counter = 0; // reset counter
                        sql = ""; // reset sql string
                    }
                }
                // issue any statements that didn't break 500 threshold
                if (!String.IsNullOrEmpty(sql))
                {
                    DataManager.IssueStatement(sql); 
                }
                
            }
        }


        // get record ids for any records that might need correction via a specific receiptid
        public static List<IndividualReport> GetRecordIdsByReceiptId(string receiptId) {
            List<IndividualReport> reports = new List<IndividualReport>();

            SqlDataReader reader;
            DataSet records = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetRecordIdsByReceiptId", conn);
                // now assign parameters
                cmd.Parameters.Add("@receiptId", SqlDbType.NVarChar).Value = receiptId;
                // execute it
                reader = cmd.ExecuteReader();
                records.Load(reader, LoadOption.OverwriteChanges, "records"); // this overwrites the name of the datatable(s)
            }
            // add data to Record object
            foreach (DataRow row in records.Tables[0].Rows) {
                IndividualReport report = new IndividualReport();
                // build out the report
                // don't need the whole thing, just a few parameters, so I won't
                // break this out to a different function
                Person p = new Person();
                p.FirstName = row["first_name"].ToString();
                p.LastName = row["last_name"].ToString();
                if (!String.IsNullOrEmpty(row["middle_name"].ToString())) {
                    p.MiddleName = row["middle_name"].ToString();
                }
                if (!String.IsNullOrEmpty(row["suffix"].ToString())) {
                    p.Suffix = row["suffix"].ToString();
                }
                if (!String.IsNullOrEmpty(row["lastn"].ToString())) {
                    p.LastFourSSN = row["lastn"].ToString(); // last 4 of ssn
                }
                report.Person = p;
                report.RecordId = row["irs_record_id"].ToString();

                reports.Add(report);
            }

            return reports;
        }
        


        // get the root submissions based on company id and tax year
        public static List<Record> GetRootSubsForCompAndTaxYr(int compId, string taxYr) {
            int taxYear = 0;
            if (taxYr.ToLower() != "all") { // then it's a year
                taxYear = Convert.ToInt32(taxYr);
            }

            List<Record> recs = new List<Record>();
            SqlDataReader reader;
            DataSet subs = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetRootSubsForCompAndTaxYr", conn);
                cmd.Parameters.Add("@compId", SqlDbType.Int).Value = String.IsNullOrEmpty(compId.ToString()) ?
                    DBNull.Value : (object)compId;
                cmd.Parameters.Add("@taxYr", SqlDbType.Int).Value = String.IsNullOrEmpty(taxYear.ToString()) ?
                    DBNull.Value : (object)taxYear;
                // execute it
                reader = cmd.ExecuteReader();
                subs.Load(reader, LoadOption.OverwriteChanges, "subs"); // this overwrites the name of the datatable(s)
            }
            if (subs.Tables != null && subs.Tables.Count > 0) {
                recs = convertDataToRecordCollection(subs.Tables[0]);
            }

            return recs;
        }


        // get all errors for a submission (by receipt id)
        public static List<ErrorFromIrs> GetSubmissionErrors(string receiptId) {
            List<ErrorFromIrs> errorList = new List<ErrorFromIrs>();

            SqlDataReader reader;
            DataSet errors = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetSubmissionErrors", conn);
                // now assign parameters
                cmd.Parameters.Add("@receiptId", SqlDbType.NVarChar).Value = receiptId;
                // execute it
                reader = cmd.ExecuteReader();
                errors.Load(reader, LoadOption.OverwriteChanges, "errors"); // this overwrites the name of the datatable(s)
            }
            // add data to Record object
            foreach (DataRow row in errors.Tables[0].Rows) {
                ErrorFromIrs error = buildErrorFromIrs(row, receiptId);
                errorList.Add(error);
            }

            return errorList;
        }


        // get all submissions, period
        public static List<Record> GetAllSubmissions(string sortType, string sortParam, bool showArchived, string taxYr, User currUser) {
            List<Record> recs = new List<Record>();
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAllSubmissions", conn);
                cmd.Parameters.Add("@client_of", SqlDbType.Int).Value = String.IsNullOrEmpty(currUser.Company.ID.ToString()) ? DBNull.Value : (object)currUser.Company.ID;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            // convert data to record object
            if (ds.Tables.Count > 0) {
                recs = convertDataToRecordCollection(ds.Tables[0]);
            }

            recs = sortRecordsBy(recs, sortType, sortParam, showArchived, taxYr);

            return recs;
        }


        // get all submissions that need a receipt id
        public static List<Record> GetAllSubmissionsWithoutRecId(User currUser) {
            List<Record> recs = new List<Record>();
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAllSubmissions", conn);
                cmd.Parameters.Add("@client_of", SqlDbType.Int).Value = String.IsNullOrEmpty(currUser.Company.ID.ToString()) ? DBNull.Value : (object)currUser.Company.ID;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            // convert data to record object
            if (ds.Tables.Count > 0) {
                recs = convertDataToRecordCollection(ds.Tables[0]);
            }

            // only return submissions with "TempID_" in the ReceiptID field 
            recs = recs.Where(r => r.ReceiptId.Contains("TempID_"))
                .OrderBy(r => r.Company.Name).ToList();

            return recs;
        }


        // get tax years that we have submissions for
        // currently, this doesn't care about test or archived data -- it just grabs it all
        public static List<string> GetAllAcaTaxYrs() {
            List<string> taxyrs = new List<string>();
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAcaTaxYrs", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                foreach (DataRow row in ds.Tables[0].Rows) {
                    taxyrs.Add(row["tax_yr"].ToString());
                }
            }
            return taxyrs;
        }



        // get terms that user can sort by
        public static string GetSortByTerms(string sortType, string searchTerm) {
            string result = "";
            if (sortType.ToLower() == "status") {
                result = getSortTermsFromDb(sortType, searchTerm, "p_GetStatusTypes");
            } else if (sortType.ToLower() == "client") {
                result = getSortTermsFromDb(sortType, searchTerm, "p_GetClientsByName");
            }

            return result;
        }


        private static string getSortTermsFromDb(string sortType, string searchTerm, string storedProc) {
            var terms = "";
            // get the terms
            DataSet ds = new DataSet();

            if (!String.IsNullOrEmpty(searchTerm)) {
                searchTerm += "%";
            }

            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand(storedProc, conn);
                // now assign parameters
                cmd.Parameters.Add("@searchTerm", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(searchTerm) ? DBNull.Value : (object)searchTerm;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }

            DataTable data = ds.Tables[0];

            // terms must be built out differently depending on which sortType this is
            if (sortType.ToLower() == "status") {
                foreach (DataRow row in data.Rows) {
                    terms += "<option value='" + row["status_type"].ToString() + "'>" + row["status_type"].ToString() + "</option>";
                }
            } else {
                foreach (DataRow row in data.Rows) {
                    terms += "<span id='id_" + row["id"].ToString() + "'>" + row["name"].ToString() + " " + row["name2"] + "</span>";
                }
            }

            return terms;
        }




        // get root submissions
        public static List<Record> GetRootSubmissions() {
            var recs = new List<Record>();
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetRootSubmissions", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            // convert the data to Record object collection
            if (ds.Tables.Count > 0) {
                recs = convertDataToRecordCollection(ds.Tables[0]);
            }
            return recs;
        }



        /// <summary>
        /// get all submissions that have errors (resulted in "Accepted with Errors" or "Rejected".
        /// each will only have one status update attached (the final one)
        /// </summary>
        public static List<Record> GetSubmissionsThatHaveErrors() {
            var recs = new List<Record>();
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetSubmissionsWithErrors", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            // convert the data to Record object collection
            if (ds.Tables.Count > 0) {
                recs = convertDataToRecordCollection(ds.Tables[0]);
            }
            return recs;
        }



        // get all submissions by company


        // get all status updates for a submission (like for a dashboard...)
        /// <summary>
        /// Returns not only Status Updates, but also child Submissions
        /// </summary>
        /// <param name="uniqueId">The parent's Unique ID</param>
        /// <returns></returns>
        public static List<Record> GetAllUpdatesForSubmission(string uniqueId) {
            // get the status update data from the db
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAllStatusUpdates", conn);
                // now assign parameters
                cmd.Parameters.Add("@uid", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(uniqueId) ? DBNull.Value : (object)uniqueId;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }

            var records = new List<Record>();
            // build each record
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                records = convertDataToRecordCollection(ds.Tables[0]);
            }

            // sort the records
            records = sortRecords(records);

            return records;
        }

        // get all the updates for a submission by its receipt id
        public static List<Record> GetAllUpdatesForSubmissionByReceiptId(string receiptId) {
            // get the status update data from the db
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetAllStatusUpdatesByReceiptId", conn);
                // now assign parameters
                cmd.Parameters.Add("@receiptId", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(receiptId) ? DBNull.Value : (object)receiptId;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }

            var records = new List<Record>();
            // build each record
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                records = convertDataToRecordCollection(ds.Tables[0]);
            }

            // sort the records
            records = sortRecords(records, false);

            return records;
        }


        public static Record GetSubmission(string receiptId) {
            Record rec = new Record();
            // get the status update data from the db
            DataSet ds = new DataSet();
            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand("p_GetSubmission", conn);
                // now assign parameters
                cmd.Parameters.Add("@receiptId", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(receiptId) ? DBNull.Value : (object)receiptId;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0) {
                rec = convertDataToRecord(ds.Tables[0].Rows[0]);
            }
            return rec;

        }

        // reorder records in a collection
        // sorting by different parameters isn't necessary, b/c this function is only being used 
        // by functions that have pulled data for a SPECIFIC submission, not ALL submissions
        private static List<Record> sortRecords(List<Record> records, bool reverseOrder = true) {
            List<Record> newList = new List<Record>();
            List<Record> children = new List<Record>();
            List<StatusUpdate> statUpdates = new List<StatusUpdate>();

            // pull out all StatusUpdates so they can be shuffled back in as needed
            // because the stored proc pulls the submission each time it has a matching status update
            foreach (Record rr in records) {
                if (rr.StatusUpdates.Count > 0) {
                    foreach (StatusUpdate su in rr.StatusUpdates) {
                        statUpdates.Add(su);
                    }
                }
            }

            // SHUFFLE BACK IN STATUS UPDATES!!!!
            foreach (Record record in records) {
                List<StatusUpdate> updateList = new List<StatusUpdate>();
                List<Int32> updateIds = new List<int>(); // track ids... make sure we don't duplicate
                // spin through each status update
                foreach (StatusUpdate sup in statUpdates) {
                    if (sup.SubmissionID == record.ID && !updateIds.Contains(sup.ID)) {
                        updateList.Add(sup);
                        updateIds.Add(sup.ID);
                    }
                }
                record.StatusUpdates = updateList; // replaces any status update(s) with the full list
            }

            foreach (Record rec in records) {
                // if the record has a parent sub id, then it's a child
                if (!String.IsNullOrEmpty(rec.ParentSubId.ToString()) && rec.ParentSubId > 0) {
                    children.Add(rec);
                }
            }

            // spin through records again and build them into the newList collection
            // foreach record, spin through children and assign as needed
            // NOTE: there could be duplicate records -- make sure they're ignored
            // track IDs

            List<int> ids = new List<int>();
            foreach (Record r in records) {
                // make sure it's not a child... and not a duplicate...
                if ((String.IsNullOrEmpty(r.ParentSubId.ToString()) || r.ParentSubId == 0) && !ids.Contains(r.ID)) {
                    newList.Add(r);
                    ids.Add(r.ID);

                    // add in any children
                    foreach (Record child in children) {
                        if ((child.ParentSubId == r.ID || child.RootParentSubId == r.ID.ToString()) && !ids.Contains(child.ID)) {
                            newList.Add(child);
                            ids.Add(child.ID);
                        }
                    }
                }
            }

            // reverse order
            if (reverseOrder) {
                newList.Reverse();
            }

            return newList;
        } // end sortRecords


        // sort records by...
        /// <summary>
        /// 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="sortType">Designate the type that's being sorted by: "status", "client", etc</param>
        /// <param name="sortParam">Usually a string identifier.  In the case of Client, supply CompanyId as a string.</param>
        /// <returns></returns>
        /// // TODO: refactor parts of this?
        private static List<Record> sortRecordsBy(List<Record> records, string sortType, string sortParam, bool showArchived, string taxYr = "") {
            List<Record> sortedRecords = new List<Record>();
                                                                    
            // do we need to show test data?
            // what about production data?
            bool showTestData = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowTestData"].ToString());
            bool showProdData = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowProductionData"].ToString());
            bool showSpecificTaxYr = false;
            if (!String.IsNullOrEmpty(taxYr)) {
                showSpecificTaxYr = true;
            }

            if (sortType == "status") { // sort by status
                foreach (Record rec in records) {
                    // if the status matches and the archived state matches...
                    if (rec.StatusUpdates.Last().StatusType.ToLower() == sortParam.ToLower() && rec.IsArchived == showArchived) {
                        // was a tax year selected, and does this record match it?
                        if (String.IsNullOrEmpty(taxYr) || (!String.IsNullOrEmpty(taxYr) && rec.TaxYr == Convert.ToInt32(taxYr))) {
                            // if it's testing or production data
                            if ((showTestData && rec.IsTestData) || (showProdData && !rec.IsTestData)) {
                                sortedRecords.Add(rec);
                            }
                        }
                    }
                }
            } else if (sortType == "client") { // sort by client
                int compId = Convert.ToInt32(sortParam);
                foreach (Record rec in records) {
                    // company matches and archived state matches
                    if (rec.Company.ID == compId && rec.IsArchived == showArchived) {
                        // was a tax year selected, and does this record match it?
                        if (String.IsNullOrEmpty(taxYr) || (!String.IsNullOrEmpty(taxYr) && rec.TaxYr == Convert.ToInt32(taxYr))) {
                            // if it's testing or production data
                            if ((showTestData && rec.IsTestData) || (showProdData && !rec.IsTestData)) {
                                sortedRecords.Add(rec);
                            }
                        }
                    }
                }
            } else { // no sorting
                foreach (Record rec in records) {
                    if (rec.IsArchived == showArchived) { // if archived state matches
                        // was a tax year selected, and does this record match it?
                        if (String.IsNullOrEmpty(taxYr) || (!String.IsNullOrEmpty(taxYr) && rec.TaxYr == Convert.ToInt32(taxYr))) {
                            // if it's testing or production data
                            if ((showTestData && rec.IsTestData) || (showProdData && !rec.IsTestData)) {
                                sortedRecords.Add(rec);
                            }
                        }
                    }
                }
            }

            return sortedRecords;
        }


        // build error from irs
        private static ErrorFromIrs buildErrorFromIrs(DataRow row, string receiptId) {
            ErrorFromIrs error = new ErrorFromIrs();
            if (!String.IsNullOrEmpty(row["error_code"].ToString())) {
                error.ErrorCode = row["error_code"].ToString();
            }
            if (!String.IsNullOrEmpty(row["error_message"].ToString())) {
                error.ErrorMessage = row["error_message"].ToString();
            }
            if (!String.IsNullOrEmpty(row["error_id"].ToString())) {
                error.ID = Convert.ToInt32(row["error_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["record_id"].ToString())) {
                error.Is1095Error = true;
            } else {
                error.Is1095Error = false;
            }
            error.ReceiptId = receiptId;
            if (!String.IsNullOrEmpty(row["record_id"].ToString())) {
                error.RecordId = Convert.ToInt32(row["record_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["xpath_content"].ToString())) {
                error.XpathContent = row["xpath_content"].ToString();
            }
            Person p = HubDataManager.BuildPerson(row);
            error.Person = p;

            return error;
        }


        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // OBJECT BUILDERS
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        
            // get terms that user can sort by
        public static string GetSortByTerms(string sortType, string searchTerm, User currUser) {
            string result = "";
            if (sortType.ToLower() == "status") {
                result = getSortTermsFromDb(sortType, searchTerm, "p_GetStatusTypes", currUser);
            } else if (sortType.ToLower() == "client") {
                result = getSortTermsFromDb(sortType, searchTerm, "p_GetClientsByName", currUser);
            }

            return result;
        }


        private static string getSortTermsFromDb(string sortType, string searchTerm, string storedProc, User currUser) {
            var terms = "";
            // get the terms
            DataSet ds = new DataSet();

            if (!String.IsNullOrEmpty(searchTerm)) {
                searchTerm += "%";
            }

            using (SqlConnection conn = DataManager.BuildConnection()) {
                conn.Open();
                SqlCommand cmd = DataManager.PrepareCommand(storedProc, conn);
                // now assign parameters
                cmd.Parameters.Add("@searchTerm", SqlDbType.NVarChar).Value = String.IsNullOrEmpty(searchTerm) ? DBNull.Value : (object)searchTerm;
                cmd.Parameters.Add("@client_of", SqlDbType.Int).Value = String.IsNullOrEmpty(currUser.Company.ID.ToString()) ? DBNull.Value : (object)currUser.Company.ID;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds);
            }

            DataTable data = ds.Tables[0];

            // terms must be built out differently depending on which sortType this is
            if (sortType.ToLower() == "status") {
                foreach (DataRow row in data.Rows) {
                    terms += "<option value='" + row["status_type"].ToString() + "'>" + row["status_type"].ToString() + "</option>";
                }
            } else {
                foreach (DataRow row in data.Rows) {
                    terms += "<span id='id_" + row["id"].ToString() + "'>" + row["name"].ToString() + " " + row["name2"] + "</span>";
                }
            }

            return terms;
        }


        // convert data result into Record objects
        private static List<Record> convertDataToRecordCollection(DataTable data) {
            var recs = new List<Record>();
            foreach (DataRow row in data.Rows) {
                Record rec = convertDataToRecord(row);
                recs.Add(rec);
            }

            return recs;
        }

        // convert data into one Record object
        private static Record convertDataToRecord(DataRow row) {
            Record rec = new Record();
            // build the record
            if (row.Table.Columns.Contains("subId")) {
                rec.ID = Convert.ToInt32(row["subId"].ToString());
            } else if (row.Table.Columns.Contains("submission_id")) {
                rec.ID = Convert.ToInt32(row["submission_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["num_records"].ToString())) {
                rec.Ct1095Transmittal = Convert.ToInt32(row["num_records"].ToString());
            }
            if (!String.IsNullOrEmpty(row["receipt_id"].ToString())) {
                rec.ReceiptId = row["receipt_id"].ToString();
            }
            if (!String.IsNullOrEmpty(row["parent_submission_id"].ToString())) {
                rec.ParentSubId = Convert.ToInt32(row["parent_submission_id"].ToString());
            }
            if (!String.IsNullOrEmpty(row["timestamp"].ToString())) {
                rec.TimeStampGMT = Convert.ToDateTime(row["timestamp"].ToString()).ToString("MM/dd/yy hh:mm tt");
            }
            if (!String.IsNullOrEmpty(row["submission_type_id"].ToString())) {
                rec.SubmissionType = row["submission_type_id"].ToString();
            }
            if (!String.IsNullOrEmpty(row["correction_type"].ToString()) && !String.IsNullOrEmpty(rec.SubmissionType)) {
                if (row["correction_type"].ToString() == "1094-C") {
                    rec.Is1094Correction = true;
                } else {
                    rec.Is1094Correction = false;
                }
            }
            if (row.Table.Columns.Contains("stat1094Type")) {
                rec.Current1094Status = row["stat1094Type"].ToString();
                rec.Current1095Status = row["stat1095Type"].ToString();
            }
            if (!String.IsNullOrEmpty(row["unique_id"].ToString())) {
                rec.UniqueId = row["unique_id"].ToString();
            }
            if (!String.IsNullOrEmpty(row["filename"].ToString())) {
                rec.FileName = row["filename"].ToString();
            }
            if (!String.IsNullOrEmpty(row["filename_irs"].ToString())) {
                rec.IrsFileName = row["filename_irs"].ToString();
            }
            if (!String.IsNullOrEmpty(row["tax_yr"].ToString())) {
                rec.TaxYr = Convert.ToInt32(row["tax_yr"].ToString());
            }
            if (!String.IsNullOrEmpty(row["is_archived"].ToString())) {
                rec.IsArchived = Convert.ToBoolean(row["is_archived"].ToString());
            }
            if (!String.IsNullOrEmpty(row["is_test_data"].ToString())) {
                rec.IsTestData = Convert.ToBoolean(row["is_test_data"].ToString());
            }
            if (row.Table.Columns.Contains("root_parent_submission_id")) {
                if (!String.IsNullOrEmpty(row["root_parent_submission_id"].ToString())) {
                    rec.RootParentSubId = row["root_parent_submission_id"].ToString();
                }
            }

            // add in current status
            var status = convertDataToStatusUpdate(row);
            List<StatusUpdate> stats = new List<StatusUpdate>();
            stats.Add(status);
            rec.StatusUpdates = stats;

            // add in company and address
            var comp = new Company();
            comp = HubDataManager.BuildCompany(row);

            rec.Company = comp;

            return rec;
        }


        private static StatusUpdate convertDataToStatusUpdate(DataRow row) {
            var update = new StatusUpdate();
            // build the update
            if (row != null) {
                if (row.Table.Columns.Contains("up_id") && !String.IsNullOrEmpty(row["up_id"].ToString())) {
                    update.ID = Convert.ToInt32(row["up_id"].ToString());
                }
                if (row.Table.Columns.Contains("up_timestamp") && !String.IsNullOrEmpty(row["up_timestamp"].ToString())) {
                    update.Timestamp = Convert.ToDateTime(row["up_timestamp"].ToString());
                }
                if (row.Table.Columns.Contains("up_status_type") && !String.IsNullOrEmpty(row["up_status_type"].ToString())) {
                    update.StatusType = row["up_status_type"].ToString();
                }
                if (row.Table.Columns.Contains("receipt_id") && !String.IsNullOrEmpty(row["receipt_id"].ToString())) {
                    update.ReceiptId = row["receipt_id"].ToString();
                }
                if (row.Table.Columns.Contains("up_status_code") && !String.IsNullOrEmpty(row["up_status_code"].ToString())) {
                    update.StatusCode = row["up_status_code"].ToString();
                }
                if (row.Table.Columns.Contains("up_message") && !String.IsNullOrEmpty(row["up_message"].ToString())) {
                    update.StatusMessage = row["up_message"].ToString();
                }
                if (row.Table.Columns.Contains("submission_id") && !String.IsNullOrEmpty(row["submission_id"].ToString())) {
                    update.SubmissionID = Convert.ToInt32(row["submission_id"].ToString());
                }
                // if this status is either "Rejected" or "Accepted with errors"
                if (row.Table.Columns.Contains("up_status_type") && !String.IsNullOrEmpty(row["up_status_type"].ToString())) {
                    if (update.StatusType == "Rejected" || update.StatusType.Contains("Accepted with")) {
                        // if it has 1095 errors
                        if (row.Table.Columns.Contains("has1095err")) {
                            // does the submission have 1095 errors?
                            if (!String.IsNullOrEmpty(row["has1095err"].ToString()) && Convert.ToInt32(row["has1095err"].ToString()) > 0) {
                                update.Has1095Errors = true;
                            } else {
                                update.Has1095Errors = false;
                            }
                        }
                        // if it has 1094 errors
                        if (row.Table.Columns.Contains("has1094err")) {
                            // does the submission have 1094 errors?
                            if (!String.IsNullOrEmpty(row["has1094err"].ToString()) && Convert.ToInt32(row["has1094err"].ToString()) > 0) {
                                update.Has1094Errors = true;
                            } else {
                                update.Has1094Errors = false;
                            }
                        }
                    } // end if "Rejected" or "Accepted with Errors"
                } // end if column contains up_status_type
            }

            return update;
        }


    } // end class
} // end namespace