using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq.SqlClient;
using System.Web;
using System.Web.Hosting;
using EZ_ACA_ClassPackage.Models;
using OfficeOpenXml;
using System.IO;
using System.Configuration;

namespace PdfCreatorWebJob {
    
    //*******************************************************************************************************************
    // IMPORTANT!!!! THIS FILE HAS A NUMBER OF FUNCTIONS AND SETTINGS COMMENTED OUT, BECAUSE THEY AREN'T NEEDED
    // IN THE PDF CREATOR, BUT ONLY APPLY IN THE ACA FILING STUFF AND MAYBE THE HUB. OBVIOUSLY, THIS MEANS THIS CLASS
    // IS TOO TIGHTLY COUPLED. SOME FUNCTIONALITY SHOULD BE PULLED FROM HERE AND ADDED TO ITS OWN CLASS. HOWEVER,
    // I SIMPLY DON'T HAVE THE TIME TO DO THOSE KINDS OF REVISIONS RIGHT NOW AND WILL HAVE TO DO THEM IN THE FUTURE. :(
    //*******************************************************************************************************************

    // this class builds the report off of the excel data
    public class ReportBuilder {

        // for testing...
        private static string narrative = "";//ConfigurationManager.AppSettings["TestNarrativeID"].ToString();
        private static int narrativeScenario = 0;

        public static Record CreateSubmissionRecord(MemoryStream fileStream, string subType = "O", CorrectionInfo correctionInfo = null) {
            Record record = new Record();

            if (correctionInfo == null) {
                correctionInfo = new CorrectionInfo();            }

            // create FileInfo object from filepath, then create excel package from FileInfo
            //FileInfo uploadedFile = new FileInfo(uploadedFilePath);
            using (ExcelPackage pkg = new ExcelPackage(fileStream)) {
                // get the workbook
                ExcelWorkbook workbook = pkg.Workbook;

                // if this workbook has worksheets (and it should...), dig into them
                if (workbook.Worksheets.Count > 0) {
                    ExcelWorksheet ws1094 = workbook.Worksheets[1]; // index is 1-based, not 0-based
                    record = createRecord(ws1094, subType);

                }
            }

            // if correction, set correction type
            if (record.SubmissionType == "C") {
                if (correctionInfo.FormType == FormType.Form1094) {
                    record.Is1094Correction = true;
                } else {
                    record.Is1094Correction = false;
                }
            }

            // add old record ids (for 1095 records) for any records being corrected
            //record = addOldRecordIds(record, correctionInfo);

            return record;
        } // end CreateSubmissionRecord

        private static Record createRecord(ExcelWorksheet worksheet, string subType/*, string filePath*/) {
            Record record = new Record();

            // file name (see Composition and Reference Guide section 3.3)
            record.TimeStampGMT = DateTime.UtcNow.ToString();
            //record.FileName = filePath;
            record.IrsFileName = "1094C_Request_" + /*ConfigurationManager.AppSettings["TCC"] +*/ "_" + 
                formatTimeStampForFileNameString(record.TimeStampGMT) + ".xml";

            // add in test scenario
            record.TestScenarioId = narrative + narrativeScenario;
            narrativeScenario++; // increment the narrativeScenario
            // add tax year
            record.TaxYr = FormatChecker.ReturnInt(FormatChecker.NullToString(worksheet.Cells[8, 1], true), 8, 1);
            record.PriorYrDataInd = false;
            if (!String.IsNullOrEmpty(worksheet.Cells[8, 2].ToString())) {
                if (worksheet.Cells[8, 2].ToString().ToLower() == "x") {
                    record.PriorYrDataInd = true;
                }
            }

            // create unique identifier
            record.UniqueId = CreateUniqueId(); // do this even for C and R submissions

            // submission type
            record.SubmissionType = FormatChecker.ReturnSubmissionTypeCd(subType);
            

            // create company, then add it to the record
            Company comp = new Company();
            comp.Name = FormatChecker.NullToString(worksheet.Cells[18, 1], false);
            comp.Name2 = FormatChecker.NullToString(worksheet.Cells[18, 2], false);
            comp.EIN = FormatChecker.ReturnEIN(FormatChecker.NullToString(worksheet.Cells[18, 3], true), 18, 3); // check EIN format
            Address compAdd = new Address(); // for the company's address
            compAdd.Address1 = FormatChecker.NullToString(worksheet.Cells[18, 4], false);
            compAdd.Address2 = FormatChecker.NullToString(worksheet.Cells[18, 5], false);
            compAdd.City = FormatChecker.NullToString(worksheet.Cells[18, 6], true);
            compAdd.State = FormatChecker.GetStateAbb(FormatChecker.NullToString(worksheet.Cells[18, 7], true), 18, 7);
            compAdd.Zip = FormatChecker.ReturnZip(FormatChecker.NullToString(worksheet.Cells[18, 8], true), 18, 8); // need a check on the field...
            compAdd.ZipExt = FormatChecker.ReturnZipExt(FormatChecker.NullToString(worksheet.Cells[18, 9], true), 18, 9);
            comp.Address = compAdd;
            Person contactPerson = new Person(); // for the company's contact person
            contactPerson.FirstName = FormatChecker.NullToString(worksheet.Cells[18, 10], true);
            contactPerson.MiddleName = FormatChecker.NullToString(worksheet.Cells[18, 11], true);
            contactPerson.LastName = FormatChecker.NullToString(worksheet.Cells[18, 12], true);
            contactPerson.Suffix = FormatChecker.NullToString(worksheet.Cells[18, 13], true);
            comp.ContactPerson = contactPerson;
            comp.ContactPhone = FormatChecker.ReturnPhone(FormatChecker.NullToString(worksheet.Cells[18, 14], true), 18, 14); // need a function to check format
            comp.GovEntityName = FormatChecker.NullToString(worksheet.Cells[18, 15], false);
            comp.GovEntityName2 = FormatChecker.NullToString(worksheet.Cells[18, 16], false);
            comp.GovEntityEIN = FormatChecker.ReturnEIN(FormatChecker.NullToString(worksheet.Cells[18, 17], true), 18, 17, false); // check EIN format
            Address govAdd = new Address(); // for the government entity address
            govAdd.Address1 = FormatChecker.NullToString(worksheet.Cells[18, 18], false);
            govAdd.Address2 = FormatChecker.NullToString(worksheet.Cells[18, 19], false);
            govAdd.City = FormatChecker.NullToString(worksheet.Cells[18, 20], true);
            govAdd.State = FormatChecker.GetStateAbb(FormatChecker.NullToString(worksheet.Cells[18, 21], true), 18, 21);
            govAdd.Zip = FormatChecker.ReturnZip(FormatChecker.NullToString(worksheet.Cells[18, 22], true), 18, 22); // check format
            govAdd.ZipExt = FormatChecker.ReturnZipExt(FormatChecker.NullToString(worksheet.Cells[18, 23], true), 18, 23); // check format
            comp.GovEntityAddress = govAdd;
            Person govContactPerson = new Person(); // gov entity contact
            govContactPerson.FirstName = FormatChecker.NullToString(worksheet.Cells[18, 24], true);
            govContactPerson.MiddleName = FormatChecker.NullToString(worksheet.Cells[18, 25], true);
            govContactPerson.LastName = FormatChecker.NullToString(worksheet.Cells[18, 26], true);
            govContactPerson.Suffix = FormatChecker.NullToString(worksheet.Cells[18, 27], true);
            comp.GovContact = govContactPerson;
            comp.GovContactPhone = FormatChecker.ReturnPhone(FormatChecker.NullToString(worksheet.Cells[18, 28], true), 18, 28); // check format

            record.Company = comp;

            record.Ct1095Transmittal = FormatChecker.ReturnInt(FormatChecker.NullToString(worksheet.Cells[18, 30], true), 18, 30); // check format
            record.IsAuthoritative = isAuthoritative(FormatChecker.NullToString(worksheet.Cells[18, 31], true));
            record.Ct1095TotalALE = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 32], false), 18, 32); // check format
            record.IsMemberOfAggregatedGroup = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 33], true));
            // HACK: the above item will always be true (1) or false (2)
            // however, the function that assigns the value will sometimes assign it unanswered (0). Update that to 2
            if (record.IsMemberOfAggregatedGroup == "0") {
                record.IsMemberOfAggregatedGroup = "2";
            }
            record.IsQualOfferMethod = isYes(FormatChecker.NullToString(worksheet.Cells[18, 34], true));
            record.IsQualOfferMethodTransitionRelief = isYes(FormatChecker.NullToString(worksheet.Cells[18, 35], true));
            record.IsSec4980HTransitionRelief = isYes(FormatChecker.NullToString(worksheet.Cells[18, 36], true));
            record.Is98PercentOfferMethod = isYes(FormatChecker.NullToString(worksheet.Cells[18, 37], true));
            record.SignaturePIN = ""; // not used in 2015
            // Person Title Txt
            record.SignatureDate = ""; // not used in 2015

            // instantiate all Coverage objects
            record.CoverageYr = new Coverage();
            record.CoverageJan = new Coverage();
            record.CoverageFeb = new Coverage();
            record.CoverageMar = new Coverage();
            record.CoverageApr = new Coverage();
            record.CoverageMay = new Coverage();
            record.CoverageJun = new Coverage();
            record.CoverageJul = new Coverage();
            record.CoverageAug = new Coverage();
            record.CoverageSep = new Coverage();
            record.CoverageOct = new Coverage();
            record.CoverageNov = new Coverage();
            record.CoverageDec = new Coverage();

            // complete all the Coverage attributes
            record.CoverageYr.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 41], true));
            record.CoverageJan.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 42], true));
            record.CoverageFeb.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 43], true));
            record.CoverageMar.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 44], true));
            record.CoverageApr.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 45], true));
            record.CoverageMay.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 46], true));
            record.CoverageJun.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 47], true));
            record.CoverageJul.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 48], true));
            record.CoverageAug.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 49], true));
            record.CoverageSep.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 50], true));
            record.CoverageOct.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 51], true));
            record.CoverageNov.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 52], true));
            record.CoverageDec.IsMinEssentialCovOffer = FormatChecker.BoolToDigitCodeType(FormatChecker.NullToString(worksheet.Cells[18, 53], true));

            record.HadMinEssentialCoverageForTheYr = hadMinEssentialCvgForTheYr(record);
            record.DidNotHaveMinEssentialCoverageForTheYr = didNotHaveMinEssentialCvgForTheYr(record);

            record.CoverageYr.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 54], false), 18, 54); // check format
            record.CoverageJan.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 55], false), 18, 55);
            record.CoverageFeb.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 56], false), 18, 56);
            record.CoverageMar.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 57], false), 18, 57);
            record.CoverageApr.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 58], false), 18, 58);
            record.CoverageMay.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 59], false), 18, 59);
            record.CoverageJun.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 60], false), 18, 60);
            record.CoverageJul.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 61], false), 18, 61);
            record.CoverageAug.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 62], false), 18, 62);
            record.CoverageSep.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 63], false), 18, 63);
            record.CoverageOct.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 64], false), 18, 64);
            record.CoverageNov.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 65], false), 18, 65);
            record.CoverageDec.FTEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 66], false), 18, 66);

            record.CoverageYr.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 67], false), 18, 67); // check format
            record.CoverageJan.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 68], false), 18, 68);
            record.CoverageFeb.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 69], false), 18, 69);
            record.CoverageMar.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 70], false), 18, 70);
            record.CoverageApr.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 71], false), 18, 71);
            record.CoverageMay.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 72], false), 18, 72);
            record.CoverageJun.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 73], false), 18, 73);
            record.CoverageJul.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 74], false), 18, 74);
            record.CoverageAug.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 75], false), 18, 75);
            record.CoverageSep.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 76], false), 18, 76);
            record.CoverageOct.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 77], false), 18, 77);
            record.CoverageNov.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 78], false), 18, 78);
            record.CoverageDec.TotEmployeeCt = FormatChecker.ReturnNullableInt(FormatChecker.NullToString(worksheet.Cells[18, 79], false), 18, 79);

            record.CoverageYr.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 80], true));
            record.CoverageJan.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 81], true));
            record.CoverageFeb.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 82], true));
            record.CoverageMar.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 83], true));
            record.CoverageApr.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 84], true));
            record.CoverageMay.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 85], true));
            record.CoverageJun.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 86], true));
            record.CoverageJul.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 87], true));
            record.CoverageAug.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 88], true));
            record.CoverageSep.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 89], true));
            record.CoverageOct.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 90], true));
            record.CoverageNov.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 91], true));
            record.CoverageDec.IsAggregatedGroup = isYes(FormatChecker.NullToString(worksheet.Cells[18, 92], true));

            record.CoverageYr.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 93], true));
            record.CoverageJan.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 94], true));
            record.CoverageFeb.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 95], true));
            record.CoverageMar.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 96], true));
            record.CoverageApr.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 97], true));
            record.CoverageMay.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 98], true));
            record.CoverageJun.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 99], true));
            record.CoverageJul.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 100], true));
            record.CoverageAug.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 101], true));
            record.CoverageSep.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 102], true));
            record.CoverageOct.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 103], true));
            record.CoverageNov.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 104], true));
            record.CoverageDec.Sec4980HTransReliefIndicator = FormatChecker.ReturnReliefIndicator(FormatChecker.NullToString(worksheet.Cells[18, 105], true));

            // add in any other ALE Members
            record = getOtherALEMembers(record, worksheet, 106);

            // now add individual records (1095-C)
            record = addIndividual1095cRecords(record, worksheet);

            // create a directory in which to store the manifest and formdata files for this submission
            //record.Directory = createDirectory(record);

            // RESET narrativeScenario VARIABLE SO IT DOESN'T HOLD THE VALUE TOO LONG
            narrativeScenario = 0;

            return record;
        }

        //private static string createDirectory(Record r) {
        //    string d = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["FilesLocation"]) + "/" + 
        //        r.Company.Name.Replace(" ", "_").Replace(".", "").Replace(",", "") + "_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
        //    Directory.CreateDirectory(d); // create the directory

        //    return d;
        //}

        private static bool isAuthoritative(string field) {
            bool result = false;
            if (!String.IsNullOrEmpty(field)) {
                result = true;
            }

            return result;
        }

        private static bool isYes(string field) {
            bool result = false;
            if (!String.IsNullOrEmpty(field)) {
                if (field.Trim().ToLower() == "yes" || field.Trim().ToLower() == "x") {
                    result = true;
                }
            }
            return result;
        }

        private static Record getOtherALEMembers(Record record, ExcelWorksheet worksheet, int startingColumn) {
            // cycle through the remainder of the worksheet -- as long as the field has a value, keep building the report
            record.OtherMembers = new List<OtherMember>();
            while (!String.IsNullOrEmpty(FormatChecker.NullToString(worksheet.Cells[18, startingColumn], false))) {
                OtherMember member = new OtherMember();
                member.BusinessName = FormatChecker.NullToString(worksheet.Cells[18, startingColumn], false);
                member.BusinessName2 = FormatChecker.NullToString(worksheet.Cells[18, startingColumn + 1], false);
                member.EIN = FormatChecker.ReturnEIN(FormatChecker.NullToString(worksheet.Cells[18, startingColumn + 2], true), 18, startingColumn + 2, false); // test EIN format

                // add member to record
                record.OtherMembers.Add(member);

                // increment startingColumn by 3 so we can keep checking for the next record (3 fields each)
                startingColumn = startingColumn + 3;
            }

            return record;
        } // end getOtherALEMembers function

        private static Record addIndividual1095cRecords(Record record, ExcelWorksheet worksheet) {
            // switch to the second worksheet
            ExcelWorksheet worksheet2 = worksheet.Workbook.Worksheets[2];

            // instantiate IndividualReports attribute
            record.IndividualReports = new List<IndividualReport>();

            // each record is on a new row, so we need to stop when we encounter a blank row
            // the first record is on row 18
            Int32 row = 18;
            // use counter to see how many 1095 records we have
            Int32 count1095 = 0;
            while (!String.IsNullOrEmpty(FormatChecker.NullToString(worksheet2.Cells[row, 1], true))) {

                // if form indicates that this person should be processed, build a report.
                // I believe the indicator is in column 403 (OM)
                if (!String.IsNullOrEmpty(FormatChecker.NullToString(worksheet2.Cells[row, 403], true)) &&
                    FormatChecker.NullToString(worksheet2.Cells[row, 403], true).Trim().ToLower() == "yes") {

                    // increment counter
                    count1095++;
                    IndividualReport iReport = build1095cReport(worksheet2, row);
                    iReport.RecordId = count1095.ToString();
                    record.IndividualReports.Add(iReport);
                }
                                
                // increment row
                row++;
            } // end while

            // add 1095 count to overall record
            record.Ct1095Transmittal = record.IndividualReports.Count(); // i think this value is hardcoded in excel, but this should be more accurate

            return record;
        } // end addIndividual1095cRecord function

        private static IndividualReport build1095cReport(ExcelWorksheet wsheet, Int32 row) {
            IndividualReport report = new IndividualReport();
            // add individual info
            Person individual = new Person();
            individual.FirstName = FormatChecker.NullToString(wsheet.Cells[row, 1], true);
            individual.MiddleName = FormatChecker.NullToString(wsheet.Cells[row, 2], true);
            individual.LastName = FormatChecker.NullToString(wsheet.Cells[row, 3], true);
            individual.Suffix = FormatChecker.NullToString(wsheet.Cells[row, 4], true);
            individual.SSN = FormatChecker.ReturnSSN(FormatChecker.NullToString(wsheet.Cells[row, 5], true), row, 5);
            individual.LastFourSSN = FormatChecker.ReturnLastFour(individual.SSN);
            Address indAddress = new Address();
            indAddress.Address1 = FormatChecker.NullToString(wsheet.Cells[row, 6], false);
            indAddress.Address2 = FormatChecker.NullToString(wsheet.Cells[row, 7], false);
            indAddress.City = FormatChecker.NullToString(wsheet.Cells[row, 8], true);
            indAddress.State = FormatChecker.GetStateAbb(FormatChecker.NullToString(wsheet.Cells[row, 9], true), row, 9);
            indAddress.Zip = FormatChecker.ReturnZip(FormatChecker.NullToString(wsheet.Cells[row, 10], true), row, 10);
            indAddress.ZipExt = FormatChecker.ReturnZipExt(FormatChecker.NullToString(wsheet.Cells[row, 11], true), row, 11);
            individual.Address = indAddress;
            report.Person = individual;

            // add employer info
            report.EmployerName = FormatChecker.NullToString(wsheet.Cells[row, 12], false);
            report.EmployerEIN = FormatChecker.ReturnEIN(FormatChecker.NullToString(wsheet.Cells[row, 13], true), row, 13);
            report.EmployerPhone = FormatChecker.ReturnPhone(FormatChecker.NullToString(wsheet.Cells[row, 15], true), row, 15);
            Address empAddress = new Address();
            empAddress.Address1 = FormatChecker.NullToString(wsheet.Cells[row, 14], false);
            empAddress.City = FormatChecker.NullToString(wsheet.Cells[row, 16], true);
            empAddress.State = FormatChecker.GetStateAbb(FormatChecker.NullToString(wsheet.Cells[row, 17], true), row, 17);
            empAddress.Zip = FormatChecker.ReturnZip(FormatChecker.NullToString(wsheet.Cells[row, 18], false), row, 18);
            empAddress.ZipExt = FormatChecker.ReturnZipExt(FormatChecker.NullToString(wsheet.Cells[row, 19], false), row, 19);
            report.EmployerAddress = empAddress;

            var planStartMt = FormatChecker.NullToString(wsheet.Cells[row, 20], false);
            if (planStartMt.Length == 1) { // make sure it's 2 digits
                planStartMt = "0" + planStartMt;
            }
            report.PlanStartMonth = planStartMt;

            // coverage codes -- format check these??? Looks like it follows a 1-2 digit + 1 letter format...
            report.CoverageCodeYr = FormatChecker.NullToString(wsheet.Cells[row, 21], true);
            report.CoverageCodeJan = FormatChecker.NullToString(wsheet.Cells[row, 22], true);
            report.CoverageCodeFeb = FormatChecker.NullToString(wsheet.Cells[row, 23], true);
            report.CoverageCodeMar = FormatChecker.NullToString(wsheet.Cells[row, 24], true);
            report.CoverageCodeApr = FormatChecker.NullToString(wsheet.Cells[row, 25], true);
            report.CoverageCodeMay = FormatChecker.NullToString(wsheet.Cells[row, 26], true);
            report.CoverageCodeJun = FormatChecker.NullToString(wsheet.Cells[row, 27], true);
            report.CoverageCodeJul = FormatChecker.NullToString(wsheet.Cells[row, 28], true);
            report.CoverageCodeAug = FormatChecker.NullToString(wsheet.Cells[row, 29], true);
            report.CoverageCodeSep = FormatChecker.NullToString(wsheet.Cells[row, 30], true);
            report.CoverageCodeOct = FormatChecker.NullToString(wsheet.Cells[row, 31], true);
            report.CoverageCodeNov = FormatChecker.NullToString(wsheet.Cells[row, 32], true);
            report.CoverageCodeDec = FormatChecker.NullToString(wsheet.Cells[row, 33], true);

            // low cost
            report.LowCostYr = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 34], false), row, 34);
            report.LowCostJan = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 35], false), row, 35);
            report.LowCostFeb = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 36], false), row, 36);
            report.LowCostMar = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 37], false), row, 37);
            report.LowCostApr = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 38], false), row, 38);
            report.LowCostMay = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 39], false), row, 39);
            report.LowCostJun = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 40], false), row, 40);
            report.LowCostJul = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 41], false), row, 41);
            report.LowCostAug = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 42], false), row, 42);
            report.LowCostSep = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 43], false), row, 43);
            report.LowCostOct = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 44], false), row, 44);
            report.LowCostNov = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 45], false), row, 45);
            report.LowCostDec = FormatChecker.ReturnFloat(FormatChecker.NullToString(wsheet.Cells[row, 46], false), row, 46);

            // safe harbor codes
            report.SafeHarborYr = FormatChecker.NullToString(wsheet.Cells[row, 47], true);
            report.SafeHarborJan = FormatChecker.NullToString(wsheet.Cells[row, 48], true);
            report.SafeHarborFeb = FormatChecker.NullToString(wsheet.Cells[row, 49], true);
            report.SafeHarborMar = FormatChecker.NullToString(wsheet.Cells[row, 50], true);
            report.SafeHarborApr = FormatChecker.NullToString(wsheet.Cells[row, 51], true);
            report.SafeHarborMay = FormatChecker.NullToString(wsheet.Cells[row, 52], true);
            report.SafeHarborJun = FormatChecker.NullToString(wsheet.Cells[row, 53], true);
            report.SafeHarborJul = FormatChecker.NullToString(wsheet.Cells[row, 54], true);
            report.SafeHarborAug = FormatChecker.NullToString(wsheet.Cells[row, 55], true);
            report.SafeHarborSep = FormatChecker.NullToString(wsheet.Cells[row, 56], true);
            report.SafeHarborOct = FormatChecker.NullToString(wsheet.Cells[row, 57], true);
            report.SafeHarborNov = FormatChecker.NullToString(wsheet.Cells[row, 58], true);
            report.SafeHarborDec = FormatChecker.NullToString(wsheet.Cells[row, 59], true);

            // employer provided self-insured (not sure what that means)
            report.EmployerProvidedSelfInsured = false;
            if (!String.IsNullOrEmpty(FormatChecker.NullToString(wsheet.Cells[row, 60], true))) {
                report.EmployerProvidedSelfInsured = true;
            }

            // add test scenario id if in testing... TODO
            report.TestScenarioId = narrative + narrativeScenario.ToString();
            narrativeScenario++; // increment to next one...

            // TODO how to do "correctedInd"? Can I base it off the "O,C,R" thing for the 1094 form? OR do I actually need individual flags too?
            report.CorrectedInd = false;

            // add any covered individuals
            report = addCoveredIndividuals(report, wsheet, row, 61);

            return report;
        } // end build1095cReport function

        private static IndividualReport addCoveredIndividuals(IndividualReport report, ExcelWorksheet wsheet, Int32 row, Int32 column) {
            report.CoveredIndividuals = new List<CoveredIndividual>();
            report.HasCoveredIndividuals = false; // initializes the property
            // set flag that this employee has "covered individuals"
            report.HasCoveredIndividuals = true;
            while (!String.IsNullOrEmpty(FormatChecker.NullToString(wsheet.Cells[row, column], true))) {
                Person person = new Person();
                person.FirstName = FormatChecker.NullToString(wsheet.Cells[row, column], true);
                person.MiddleName = FormatChecker.NullToString(wsheet.Cells[row, column + 1], true);
                person.LastName = FormatChecker.NullToString(wsheet.Cells[row, column + 2], true);
                person.Suffix = FormatChecker.NullToString(wsheet.Cells[row, column + 3], true);
                person.SSN = FormatChecker.ReturnSSN(FormatChecker.NullToString(wsheet.Cells[row, column + 4], true), row, column + 4);
                person.LastFourSSN = FormatChecker.ReturnLastFour(person.SSN);
                person.DOB = FormatChecker.ReturnDateTime(FormatChecker.NullToString(wsheet.Cells[row, column + 5], false), row, column + 5);

                CoveredIndividual ind = new CoveredIndividual();
                ind.Person = person;
                ind.IsCoveredYr = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 6], true));
                ind.IsCoveredJan = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 7], true));
                ind.IsCoveredFeb = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 8], true));
                ind.IsCoveredMar = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 9], true));
                ind.IsCoveredApr = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 10], true));
                ind.IsCoveredMay = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 11], true));
                ind.IsCoveredJun = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 12], true));
                ind.IsCoveredJul = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 13], true));
                ind.IsCoveredAug = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 14], true));
                ind.IsCoveredSep = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 15], true));
                ind.IsCoveredOct = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 16], true));
                ind.IsCoveredNov = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 17], true));
                ind.IsCoveredDec = isChecked(FormatChecker.NullToString(wsheet.Cells[row, column + 18], true));
                // update 'column' for next pass
                column = column + 19;

                report.CoveredIndividuals.Add(ind);
            }

            return report;
        } // end addCoveredIndividuals function

        private static bool isChecked(string field) {
            bool result = false;
            if (!String.IsNullOrEmpty(field)) {
                result = true;
            }

            return result;
        }

        public static string CreateUniqueId() {
            string id = Guid.NewGuid().ToString() + ":SYS12:";
            // bring in TCC
            //id += ConfigurationManager.AppSettings["TCC"].ToString() + "::T"; // "T" is for "transactional"

            return id;
        }

        public static string GetStatusClass(string status) {
            string colorClass = "pending";
            if (!String.IsNullOrEmpty(status)) {
                if (status.ToLower() == "accepted") {
                    colorClass = "complete";
                } else if (status.ToLower() == "rejected" || status.ToLower() == "not found" || status.ToLower() == "error") {
                    colorClass = "rejected";
                }
            }

            return colorClass;
        }

        //private static Record addOldRecordIds(Record rec, CorrectionInfo correctionInfo) {
        //    // if this submission is a correction...
        //    if (rec.SubmissionType == "C" && correctionInfo.FormType == FormType.Form1095) {
        //        // get old records
        //        List<IndividualReport> oldIndReports = DataController.GetRecordIdsByReceiptId(correctionInfo.BadSubmissionReceiptId);

        //        // capture list of Record Ids that are being used to make sure none are duplicated
        //        List<int> matchingRecordIds = new List<int>();

        //        // compare them with the IndividualReports in 'rec'
        //        // if a match, add the ReceiptId from the 'oldIndReports' to the corresponding new one
        //        foreach (IndividualReport r in rec.IndividualReports) {
        //            // look for full name. Can't check SSN yet, because it's encrypted.  Will check that later
        //            var result = oldIndReports.Where(x => x.Person.FirstName == r.Person.FirstName &&
        //                                       (x.Person.MiddleName == r.Person.MiddleName || 
        //                                            (x.Person.MiddleName == null && r.Person.MiddleName == "")) &&
        //                                       x.Person.LastName == r.Person.LastName &&
        //                                       (x.Person.Suffix == r.Person.Suffix || 
        //                                            (x.Person.Suffix == null && r.Person.Suffix == "")))
        //                                    .Select(x => x.RecordId);
        //            // if that didn't work, leave off suffix
        //            if (result == null || result.Count() == 0) { // if that didn't work, try a less specific search?
        //                result = oldIndReports.Where(x => x.Person.FirstName == r.Person.FirstName &&
        //                                        (x.Person.MiddleName == r.Person.MiddleName || 
        //                                            (x.Person.MiddleName == null && r.Person.MiddleName == "")) &&
        //                                       x.Person.LastName == r.Person.LastName)
        //                                    .Select(x => x.RecordId);
        //            }
        //            // if that didn't work, try just first and last name
        //            if (result == null || result.Count() == 0) { // if that didn't work, try a less specific search?
        //                result = oldIndReports.Where(x => x.Person.FirstName == r.Person.FirstName &&
        //                                       x.Person.LastName == r.Person.LastName)
        //                                    .Select(x => x.RecordId);
        //            }
        //            // if those didn't work, match last 4 of social
        //            if (result == null || result.Count() == 0 && !String.IsNullOrEmpty(r.Person.LastFourSSN)) {
        //                List<string> recordIds = new List<string>();
        //                recordIds.Add(getMatchingReportId(oldIndReports, r));
        //                result = recordIds.AsEnumerable();
        //            }

        //            // if we got more than one result, narrow it down by comparing last 4 of SSN
        //            if (result != null && result.Count() > 1) {
        //                List<string> recordIds = new List<string>();
        //                foreach (string id in result) {
        //                    var record = oldIndReports.Where(x => x.RecordId == id);
        //                    if (!String.IsNullOrEmpty(r.Person.LastFourSSN) && !String.IsNullOrEmpty(record.First().RecordId)) {
        //                        if (PasswordStorage.VerifyPassword(r.Person.LastFourSSN, record.First().Person.LastFourSSN)) {
        //                            recordIds.Add(record.First().RecordId);
        //                        }
        //                    }
        //                }
        //                if (recordIds != null && recordIds.Count > 0) { 
        //                    result = recordIds.AsEnumerable();
        //                }
        //            }

        //            // finally, if there's still more than one result, try to pull one out if its record id has already been used
        //            if (result != null && result.Count() > 1) {
        //                var newResult = result; // this lets us compare the 'result' collection to itself
        //                foreach (string oldReportLast4 in result) {
        //                    if (matchingRecordIds.Contains(Convert.ToInt32(oldReportLast4))) {
        //                        newResult = newResult.Where(x => x != oldReportLast4); // remove this one
        //                    }
        //                }
        //                result = newResult; // reset the 'result' collection so the rest of the function can work
        //            }


        //            // if we got something, add it to the new record that matches
        //            if (result != null && result.Count() > 0) {
        //                r.IdOfRecordBeingReplaced = result.First();
        //                matchingRecordIds.Add(Convert.ToInt32(result.First()));
        //                // remove the matched record from the old list so we don't have to cycle through it again
        //                oldIndReports.RemoveAll(x => x.RecordId == r.IdOfRecordBeingReplaced);
        //            }
        //        }
        //    }
        //    return rec;
        //}


        private static string getMatchingReportId(List<IndividualReport> oldReports, IndividualReport r) {
            string recId = "";

            // first, try to minimize the number of records we have to sort through by pulling the ones with matching last names
            //var selectedReports = oldReports.Where(x => x.Person.LastName == r.Person.LastName);
            //recId = getMatchingReportIdFromCollection(selectedReports, r.Person.LastFourSSN);

            //// if that didn't work, try sending a list of the ones where the first name matches
            //if (String.IsNullOrEmpty(recId)) {
            //    selectedReports = oldReports.Where(x => x.Person.FirstName == r.Person.FirstName);
            //    recId = getMatchingReportIdFromCollection(selectedReports, r.Person.LastFourSSN);
            //}

            // if that doesn't work either, we'll have to run through all of them :(
            //if (String.IsNullOrEmpty(recId)) {
            //    recId = getMatchingReportIdFromCollection(oldReports.AsEnumerable(), r.Person.LastFourSSN);
            //}

            return recId;
        }


        //private static string getMatchingReportIdFromCollection(IEnumerable<IndividualReport> reports, string last4) {
        //    string recId = "";
        //    foreach (IndividualReport oldReport in reports) {
        //        if (!String.IsNullOrEmpty(last4) && !String.IsNullOrEmpty(oldReport.Person.LastFourSSN)) {
        //            if (PasswordStorage.VerifyPassword(last4, oldReport.Person.LastFourSSN)) {
        //                recId = oldReport.RecordId;
        //                break; // get out of this interminable loop!
        //            }
        //        }
        //    }

        //    return recId;
        //}


        private static bool hadMinEssentialCvgForTheYr(Record rec) {
            var result = false;

            if (rec.CoverageYr.IsMinEssentialCovOffer == "1" ||
                (rec.CoverageJan.IsMinEssentialCovOffer == "1" && rec.CoverageFeb.IsMinEssentialCovOffer == "1" &&
                rec.CoverageMar.IsMinEssentialCovOffer == "1" && rec.CoverageApr.IsMinEssentialCovOffer == "1" &&
                rec.CoverageMay.IsMinEssentialCovOffer == "1" && rec.CoverageJun.IsMinEssentialCovOffer == "1" &&
                rec.CoverageJul.IsMinEssentialCovOffer == "1" && rec.CoverageAug.IsMinEssentialCovOffer == "1" &&
                rec.CoverageSep.IsMinEssentialCovOffer == "1" && rec.CoverageOct.IsMinEssentialCovOffer == "1" &&
                rec.CoverageNov.IsMinEssentialCovOffer == "1" && rec.CoverageDec.IsMinEssentialCovOffer == "1")) {
                result = true;
            }

            return result;
        }


        private static bool didNotHaveMinEssentialCvgForTheYr(Record rec) {
            var result = false;

            if (rec.CoverageYr.IsMinEssentialCovOffer == "2" ||
                (rec.CoverageJan.IsMinEssentialCovOffer == "2" && rec.CoverageFeb.IsMinEssentialCovOffer == "2" &&
                rec.CoverageMar.IsMinEssentialCovOffer == "2" && rec.CoverageApr.IsMinEssentialCovOffer == "2" &&
                rec.CoverageMay.IsMinEssentialCovOffer == "2" && rec.CoverageJun.IsMinEssentialCovOffer == "2" &&
                rec.CoverageJul.IsMinEssentialCovOffer == "2" && rec.CoverageAug.IsMinEssentialCovOffer == "2" &&
                rec.CoverageSep.IsMinEssentialCovOffer == "2" && rec.CoverageOct.IsMinEssentialCovOffer == "2" &&
                rec.CoverageNov.IsMinEssentialCovOffer == "2" && rec.CoverageDec.IsMinEssentialCovOffer == "2")) {
                result = true;
            }

            return result;
        }

        // format timestamp for filename string
        private static string formatTimeStampForFileNameString(string timestamp) {
            var ts = Convert.ToDateTime(timestamp);
            // convert it to the requested format
            string timeString = String.Format("{0:yyyyMMdd HHmmss}", ts);
            timeString = timeString.Replace(" ", "T");
            timeString = timeString + "000Z";

            return timeString;
        }

        // get correct status text for the 1094 portion of a status update
        public static string Get1094Status(StatusUpdate up, Record rec) {
            string status = "";

            // only populate text if this submission relates to the 1094
            if (rec.SubmissionType == "O" || rec.SubmissionType == "R" ||
                (rec.SubmissionType == "C" && rec.Is1094Correction)) {
                // only populate text if the status has errors
                if (up.StatusType.Contains("Accepted with")) {
                    if (up.Has1094Errors) {
                        status = up.StatusType;
                    }
                } else if (up.StatusType.Contains("Rejected") || up.StatusType == "Accepted") {
                    status = up.StatusType;
                } 

                // now let's get a little more complicated...
                if (up.StatusType.Contains("Accepted with")) {
                    if (up.Has1095Errors && !up.Has1094Errors) {
                        status = "Accepted"; // the only errors were with 1095s
                    }
                }
            }

            return status;
        }

        // get correct status text for the 1095 portion of a status update
        public static string Get1095Status(StatusUpdate up, Record rec) {
            string status = "";

            // only populate text if this submission relates to 1095s
            if (rec.SubmissionType == "O" || rec.SubmissionType == "R" ||
                (rec.SubmissionType == "C" && !rec.Is1094Correction)) {
                // only populate text if the status has errors
                if (up.StatusType.Contains("Accepted with")) {
                    if (up.Has1095Errors) {
                        status = up.StatusType;
                    }
                } else if (up.StatusType.Contains("Rejected") || up.StatusType == "Accepted") {
                    status = up.StatusType;
                }

                // now let's get a little more complicated...
                if (up.StatusType.Contains("Accepted with")) {
                    if (!up.Has1095Errors && up.Has1094Errors) {
                        status = "Accepted"; // the only errors were with the 1094
                    }
                }
                
            }

            return status;
        }
    }
}