using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Hosting;
using System.Xml;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ACAfiling.Controllers {
    // this class builds out the XML document that must be submitted to the SOAP service
    // once the XML doc is completed, it can be converted into an MTOM attachment
    public class Xml2017Controller : Controller {

        // ------------------------------
        // REMOVE TEST SCENARIO LINES WHEN IN PRODUCTION -- THEY ARE IN 2 DIFFERENT PLACES
        // ------------------------------

        private static bool isProduction() {
            bool isProduction = true;
            if (System.Configuration.ConfigurationManager.AppSettings["TestingOrProduction"].ToString() == "T") {
                isProduction = false;
            }
            return isProduction;
        }

        // return the XML (should this return XmlDocument or XmlWriter?)
        // the sample xml doc can be found in the AIR WSDL folder/MSG/IRS-Form1094-1095CTransmitterUpstreamMessage.xml
        public static byte[] BuildXmlDoc(Record rec, CorrectionInfo correctionInfo) { // (string filePath was old param)
            //XmlDocument xml = new XmlDocument();
            byte[] blob;
            Directory.CreateDirectory(rec.Directory + "/" + rec.CurrentSubDirectory);
            using (var writer = new StreamWriter(rec.XmlFilePath, false, new UTF8Encoding(false))) { // file name... (parameter used to be 'filePath')
                writer.AutoFlush = true; // this way, whenever I check the estimated file length, it should be pretty accurate.
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>".Replace((char)(0x1F), ' ').Trim());
                writer.WriteLine("<p:Form109495CTransmittalUpstream " +
                    "xmlns:p=\"urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage\" " +
                    "xmlns:p1=\"urn:us:gov:treasury:irs:ext:aca:air:ty17\" xmlns:p2=\"urn:us:gov:treasury:irs:common\" " +
                    "xmlns:p3=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\" " +
                    "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                    "xsi:schemaLocation=\"urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage " +
                    "IRS-Form1094-1095CTransmitterUpstreamMessage.xsd \">");

                writer.WriteLine("<p1:Form1094CUpstreamDetail lineNum=\"0\" recordType=\"\">"); // 2015 ver has recordType=\"String\"

                // SubmissionId should be 1, since we'll only submit a single 1094C in a transmission (though more are allowed)
                // OR this needs to be set programmatcially or through a variable...
                writer.WriteLine("<p1:SubmissionId>1</p1:SubmissionId>");
                // -------
                // TEST SCENARIO ID!!!!!
                if (!isProduction()) {
                    writer.WriteLine("<p1:TestScenarioId>" + rec.TestScenarioId + "</p1:TestScenarioId>");
                }
                //--------
                // REPLACEMENT STUFF!!!
                if (rec.SubmissionType == "R") {
                    // leave this section out in 2017...
                    //if (correctionInfo.IsWholeTransReplacement) {
                    //    writer.WriteLine("<p1:OriginalReceiptId>" + correctionInfo.BadSubmissionReceiptId + "</p1:OriginalReceiptId>");
                    //} else {
                    //    writer.WriteLine("<p1:OriginalUniqueSubmissionId>" + correctionInfo.BadSubmissionReceiptId + "|1</p1:OriginalUniqueSubmissionId>");
                    //}
                } 

                writer.WriteLine("<p1:TaxYr>" + rec.TaxYr.ToString() + "</p1:TaxYr>");

                // corrected info elements
                if (rec.SubmissionType == "C" && correctionInfo.FormType == FormType.Form1094) { // "O" is "original"
                    writer.WriteLine("<p1:CorrectedInd>1</p1:CorrectedInd>");
                    writer.WriteLine("<p1:CorrectedSubmissionInfoGrp>");
                    // the following line expects a format of "1095C-16-00039344|1"
                    // the trailing "1" is the submission within a specific transmission.
                    // that will always be "1" in our case, since we only do one submission per transmission
                    writer.WriteLine("<p1:CorrectedUniqueSubmissionId>" + correctionInfo.BadSubmissionReceiptId + "|1</p1:CorrectedUniqueSubmissionId>");
                    writer.WriteLine("<p1:CorrectedSubmissionPayerName>");
                    writer.WriteLine("<p1:BusinessNameLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Name) + "</p1:BusinessNameLine1Txt>");
                    if (!String.IsNullOrEmpty(rec.Company.Name2)) {
                        writer.WriteLine("<p1:BusinessNameLine2Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Name2) + "</p1:BusinessNameLine2Txt>");
                    }
                    writer.WriteLine("</p1:CorrectedSubmissionPayerName>"); // end CorrectedSubmissionPayerName
                    writer.WriteLine("<p1:CorrectedSubmissionPayerTIN>" + rec.Company.EIN + "</p1:CorrectedSubmissionPayerTIN>");
                    writer.WriteLine("</p1:CorrectedSubmissionInfoGrp>"); // end CorrectedSubmissionInfoGroup
                } else {
                    writer.WriteLine("<p1:CorrectedInd>0</p1:CorrectedInd>");
                }// end if

                // employer information group
                writer.WriteLine("<p1:EmployerInformationGrp>");
                writer.WriteLine("<p1:BusinessName>");
                writer.WriteLine("<p1:BusinessNameLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Name) + "</p1:BusinessNameLine1Txt>");
                if (!String.IsNullOrEmpty(rec.Company.Name2)) {
                    writer.WriteLine("<p1:BusinessNameLine2Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Name2) + "</p1:BusinessNameLine2Txt>");
                }
                writer.WriteLine("</p1:BusinessName>"); // end BusinessName
                writer.WriteLine("<p1:BusinessNameControlTxt>" + FormatChecker.GetNameControlCode(rec.Company.Name) + "</p1:BusinessNameControlTxt>"); // not sure how to get this. Looks like the first 3 or 4 letters of a company name? Comp & Ref pg 20
                writer.WriteLine("<p2:TINRequestTypeCd>BUSINESS_TIN</p2:TINRequestTypeCd>"); // hard coding for now. could also be "INDIVIDUAL_TIN"
                writer.WriteLine("<p2:EmployerEIN>" + rec.Company.EIN + "</p2:EmployerEIN>"); // may need to remove dash
                writer.WriteLine("<p1:MailingAddressGrp>");
                writer.WriteLine("<p1:USAddressGrp>");
                writer.WriteLine("<p1:AddressLine1Txt>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.Address.Address1)) + "</p1:AddressLine1Txt>");
                if (!String.IsNullOrEmpty(rec.Company.Address.Address2)) {
                    writer.WriteLine("<p1:AddressLine2Txt>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.Address.Address2)) + "</p1:AddressLine2Txt>");
                }
                writer.WriteLine("<p2:CityNm>" + FormatChecker.FormatCityName(FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.Address.City))) + "</p2:CityNm>");
                writer.WriteLine("<p1:USStateCd>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.Address.State)) + "</p1:USStateCd>");
                writer.WriteLine("<p2:USZIPCd>" + FormatChecker.ReturnZipToString((int)rec.Company.Address.Zip) + "</p2:USZIPCd>");
                if (rec.Company.Address.ZipExt != null && rec.Company.Address.ZipExt > 0 && rec.Company.Address.ZipExt.ToString().Length == 4) {
                    writer.WriteLine("<p2:USZIPExtensionCd>" + FormatChecker.ReturnZipExtToString((int)rec.Company.Address.ZipExt) + "</p2:USZIPExtensionCd>");
                }
                writer.WriteLine("</p1:USAddressGrp>"); // end USAddressGrp
                writer.WriteLine("</p1:MailingAddressGrp>"); // end MailingAddressGrp
                writer.WriteLine("<p1:ContactNameGrp>");
                writer.WriteLine("<p1:PersonFirstNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.ContactPerson.FirstName)) + "</p1:PersonFirstNm>");
                if (!String.IsNullOrEmpty(rec.Company.ContactPerson.MiddleName)) {
                    writer.WriteLine("<p1:PersonMiddleNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.ContactPerson.MiddleName)) + "</p1:PersonMiddleNm>");
                }
                writer.WriteLine("<p1:PersonLastNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.ContactPerson.LastName)) + "</p1:PersonLastNm>");
                if (rec.SubmissionType == "R") {
                    writer.WriteLine("<p1:SuffixNm>-" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.ContactPerson.Suffix)) + "</p1:SuffixNm>"); // the dash is there because in a replacement, the IRS needs something in the text to be different
                } else if (!String.IsNullOrEmpty(rec.Company.ContactPerson.Suffix)) {
                    writer.WriteLine("<p1:SuffixNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.ContactPerson.Suffix)) + "</p1:SuffixNm>");
                }
                writer.WriteLine("</p1:ContactNameGrp>"); // end ContactNameGrp
                writer.WriteLine("<p1:ContactPhoneNum>" + rec.Company.ContactPhone + "</p1:ContactPhoneNum>"); // remove dashes?
                writer.WriteLine("</p1:EmployerInformationGrp>"); // end EmployerInformationGrp

                // gov entity info
                // is this whole block optional?
                if (!String.IsNullOrEmpty(rec.Company.GovEntityName)) {
                    writer.WriteLine("<p1:GovtEntityEmployerInfoGrp>");
                    writer.WriteLine("<p1:BusinessName>");
                    writer.WriteLine("<p1:BusinessNameLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityName) + "</p1:BusinessNameLine1Txt>");
                    if (!String.IsNullOrEmpty(rec.Company.GovEntityName2)) {
                        writer.WriteLine("<p1:BusinessNameLine2Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityName2) + "</p1:BusinessNameLine2Txt>");
                    }
                    writer.WriteLine("</p1:BusinessName>"); // end BusinessName
                    writer.WriteLine("<p1:BusinessNameControlTxt>" + FormatChecker.GetNameControlCode(rec.Company.GovEntityName) + "</p1:BusinessNameControlTxt>"); // again... need to figure this out
                    writer.WriteLine("<p2:TINRequestTypeCd>BUSINESS_TIN</p2:TINRequestTypeCd>");
                    writer.WriteLine("<p2:EmployerEIN>" + rec.Company.GovEntityEIN + "</p2:EmployerEIN>");
                    writer.WriteLine("<p1:MailingAddressGrp>");
                    writer.WriteLine("<p1:USAddressGrp>");
                    if (rec.SubmissionType == "R") {
                        writer.WriteLine("<p1:AddressLine1Txt>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityAddress.Address1)) + " </p1:AddressLine1Txt>");
                    } else {
                        writer.WriteLine("<p1:AddressLine1Txt>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityAddress.Address1)) + "</p1:AddressLine1Txt>");
                    }

                    if (!String.IsNullOrEmpty(rec.Company.GovEntityAddress.Address2)) {
                        writer.WriteLine("<p1:AddressLine2Txt>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityAddress.Address2)) + "</p1:AddressLine2Txt>");
                    }
                    writer.WriteLine("<p2:CityNm>" + FormatChecker.FormatCityName(FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityAddress.City))) + "</p2:CityNm>");
                    writer.WriteLine("<p1:USStateCd>" + rec.Company.GovEntityAddress.State + "</p1:USStateCd>");
                    writer.WriteLine("<p2:USZIPCd>" + FormatChecker.ReturnZipToString((int)rec.Company.GovEntityAddress.Zip) + "</p2:USZIPCd>");
                    if (rec.Company.GovEntityAddress.ZipExt != null && rec.Company.GovEntityAddress.ZipExt > 0 && rec.Company.GovEntityAddress.ZipExt.ToString().Length == 4) {
                        writer.WriteLine("<p2:USZIPExtensionCd>" + FormatChecker.ReturnZipExtToString((int)rec.Company.GovEntityAddress.ZipExt) + "</p2:USZIPExtensionCd>");
                    }
                    writer.WriteLine("</p1:USAddressGrp>"); // end USAddressGrp
                    writer.WriteLine("</p1:MailingAddressGrp>"); // end MailingAddressGrp
                    writer.WriteLine("<p1:ContactNameGrp>");
                    writer.WriteLine("<p1:PersonFirstNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovContact.FirstName)) + "</p1:PersonFirstNm>");
                    if (!String.IsNullOrEmpty(rec.Company.GovContact.MiddleName)) {
                        writer.WriteLine("<p1:PersonMiddleNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovContact.MiddleName)) + "</p1:PersonMiddleNm>");
                    }
                    writer.WriteLine("<p1:PersonLastNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovContact.LastName)) + "</p1:PersonLastNm>");
                    if (!String.IsNullOrEmpty(rec.Company.GovContact.Suffix)) {
                        writer.WriteLine("<p1:SuffixNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(rec.Company.GovContact.Suffix)) + "</p1:SuffixNm>");
                    }
                    writer.WriteLine("</p1:ContactNameGrp>"); // end ContactNameGrp
                    writer.WriteLine("<p1:ContactPhoneNum>" + rec.Company.GovContactPhone + "</p1:ContactPhoneNum>"); // may need to lose dashes
                    writer.WriteLine("</p1:GovtEntityEmployerInfoGrp>"); // end GovtEntityEmployerInfoGrp
                }

                // misc info before 1095 records
                // several items will only show up if this is an "authoritativeTransmittal" -- rec.IsAuthoritative
                writer.WriteLine("<p1:Form1095CAttachedCnt>" + rec.Ct1095Transmittal.ToString() + "</p1:Form1095CAttachedCnt>");
                writer.WriteLine("<p1:AuthoritativeTransmittalInd>" + FormatChecker.BoolToDigitBool(rec.IsAuthoritative) + "</p1:AuthoritativeTransmittalInd>");
                if (rec.Ct1095TotalALE != null && rec.Ct1095TotalALE > 0 && rec.IsAuthoritative) {
                    writer.WriteLine("<p1:TotalForm1095CALEMemberCnt>" + rec.Ct1095TotalALE.ToString() + "</p1:TotalForm1095CALEMemberCnt>"); // don't think I have a value for this...
                }
                if (rec.IsAuthoritative) { // only show these if IsAuthoritative
                    writer.WriteLine("<p1:AggregatedGroupMemberCd>" + rec.IsMemberOfAggregatedGroup + "</p1:AggregatedGroupMemberCd>"); // true or false
                    writer.WriteLine("<p1:QualifyingOfferMethodInd>" + FormatChecker.BoolToDigitBool(rec.IsQualOfferMethod) + "</p1:QualifyingOfferMethodInd>"); // might need "X" for true...
                    // GONE FOR 2016 AND 2017
                    //writer.WriteLine("<QlfyOfferMethodTrnstReliefInd>" + FormatChecker.BoolToDigitBool(rec.IsQualOfferMethodTransitionRelief) + "</QlfyOfferMethodTrnstReliefInd>"); //TODO: is this gone for 2016?
                    // GONE FOR 2017
                    //writer.WriteLine("<p1:Section4980HReliefInd>" + FormatChecker.BoolToDigitBool(rec.IsSec4980HTransitionRelief) + "</p1:Section4980HReliefInd>"); // might need "X" for true...
                    writer.WriteLine("<p1:NinetyEightPctOfferMethodInd>" + FormatChecker.BoolToDigitBool(rec.Is98PercentOfferMethod) + "</p1:NinetyEightPctOfferMethodInd>"); // might need "X" for true...
                }
                //writer.WriteLine("<JuratSignaturePIN>" + "</JuratSignaturePIN>"); // pg 61 Comp & Ref: not needed // TODO: for 2016, these fields are added back but don't seem to be required
                //writer.WriteLine("<irs:PersonTitleTxt>" + "</irs:PersonTitleTxt>"); // pg 61 Comp & Ref: not needed
                DateTime today = DateTime.Now;
                // following line not needed, according to IRS email 2/19/16
                //writer.WriteLine("<irs:SignatureDt>" + today.ToString("yyyy-MM-dd") + "</irs:SignatureDt>"); // YYYY-MM-DD (is using today okay?)

                // COVERAGE OBJECTS
                // ALE member info group
                // going to try to cut this out in the cases when the yearly amount is 0. that would mean that all the monthly
                // amounts are also 0, and therefore, not needed.  Maybe that will resolve the AIRSH100 error I'm getting...
                if (rec.Ct1095TotalALE != null && rec.Ct1095TotalALE > 0 && rec.IsAuthoritative) {
                    writer.WriteLine("<p1:ALEMemberInformationGrp>");
                    writer.WriteLine("<p1:YearlyALEMemberDetail>");
                    if (rec.HadMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageYr.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageYr.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageYr.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageYr.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageYr.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageYr.IsAggregatedGroup.ToString())) {
                        writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageYr.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...
                    }
                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageYr.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageYr.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:YearlyALEMemberDetail>"); // end YearlyALEMemberDetail

                    // January
                    writer.WriteLine("<p1:JanALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else { // otherwise, just print whatever the value was from the form
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageJan.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJan.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageJan.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJan.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageJan.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJan.IsAggregatedGroup.ToString())) {
                        writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageJan.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...
                    }
                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageJan.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageJan.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:JanALEMonthlyInfoGrp>"); // end JanALEMonthlyInfoGrp

                    // February
                    writer.WriteLine("<p1:FebALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageFeb.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageFeb.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageFeb.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageFeb.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageFeb.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageFeb.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageFeb.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageFeb.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:FebALEMonthlyInfoGrp>"); // end FebALEMonthlyInfoGrp

                    // March
                    writer.WriteLine("<p1:MarALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageMar.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMar.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageMar.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMar.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageMar.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageMar.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageMar.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageMar.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:MarALEMonthlyInfoGrp>"); // end MarALEMonthlyInfoGrp

                    // April
                    writer.WriteLine("<p1:AprALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageApr.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageApr.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageApr.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageApr.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageApr.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageApr.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageApr.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageApr.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:AprALEMonthlyInfoGrp>"); // end AprALEMonthlyInfoGrp

                    // May
                    writer.WriteLine("<p1:MayALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageMay.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMay.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageMay.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMay.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageMay.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageMay.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageMay.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageMay.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:MayALEMonthlyInfoGrp>"); // end MayALEMonthlyInfoGrp

                    // June
                    writer.WriteLine("<p1:JunALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageJun.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJun.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageJun.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJun.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageJun.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageJun.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageJun.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageJun.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:JunALEMonthlyInfoGrp>"); // end JunALEMonthlyInfoGrp

                    // July
                    writer.WriteLine("<p1:JulALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageJul.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJul.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageJul.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJul.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageJul.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageJul.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageJul.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageJul.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:JulALEMonthlyInfoGrp>"); // end JulALEMonthlyInfoGrp

                    // August
                    writer.WriteLine("<p1:AugALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageAug.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageAug.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageAug.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageAug.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageAug.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageAug.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageAug.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageAug.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:AugALEMonthlyInfoGrp>"); // end AugALEMonthlyInfoGrp

                    // September
                    writer.WriteLine("<p1:SeptALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageSep.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageSep.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageSep.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageSep.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageSep.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageSep.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageSep.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageSep.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:SeptALEMonthlyInfoGrp>"); // end SeptALEMonthlyInfoGrp

                    // October
                    writer.WriteLine("<p1:OctALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageOct.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageOct.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageOct.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageOct.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageOct.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageOct.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageOct.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageOct.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:OctALEMonthlyInfoGrp>"); // end OctALEMonthlyInfoGrp

                    // November
                    writer.WriteLine("<p1:NovALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageNov.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageNov.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageNov.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageNov.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageNov.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageNov.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageNov.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageNov.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:NovALEMonthlyInfoGrp>"); // end NovALEMonthlyInfoGrp

                    // December
                    writer.WriteLine("<p1:DecALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>1</p1:MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>2</p1:MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<p1:MinEssentialCvrOffrCd>" + rec.CoverageDec.IsMinEssentialCovOffer + "</p1:MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageDec.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:ALEMemberFTECnt>" + rec.CoverageDec.FTEmployeeCt.ToString() + "</p1:ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageDec.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<p1:TotalEmployeeCnt>" + rec.CoverageDec.TotEmployeeCt.ToString() + "</p1:TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<p1:AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageDec.IsAggregatedGroup) + "</p1:AggregatedGroupInd>"); // might need "X" for true...

                    // removed for 2017
                    //if (!String.IsNullOrEmpty(rec.CoverageDec.Sec4980HTransReliefIndicator)) {
                    //    writer.WriteLine("<p1:ALESect4980HTrnstReliefCd>" + rec.CoverageDec.Sec4980HTransReliefIndicator + "</p1:ALESect4980HTrnstReliefCd>");
                    //}
                    writer.WriteLine("</p1:DecALEMonthlyInfoGrp>"); // end DecALEMonthlyInfoGrp
                    writer.WriteLine("</p1:ALEMemberInformationGrp>"); // ALEMemberInformationGrp
                }

                // other ALE members group info
                if (rec.OtherMembers != null && rec.OtherMembers.Count > 0 && rec.IsAuthoritative) {
                    // populate each one
                    foreach (OtherMember mem in rec.OtherMembers) {
                        writer.WriteLine("<p1:OtherALEMembersGrp>");
                        writer.WriteLine("<p1:BusinessName>");
                        writer.WriteLine("<p1:BusinessNameLine1Txt>" + FormatChecker.StripCommasAndPeriods(mem.BusinessName) + "</p1:BusinessNameLine1Txt>");
                        if (!String.IsNullOrEmpty(mem.BusinessName2)) {
                            writer.WriteLine("<p1:BusinessNameLine2Txt>" + FormatChecker.StripCommasAndPeriods(mem.BusinessName2) + "</p1:BusinessNameLine2Txt>");
                        }
                        writer.WriteLine("</p1:BusinessName>"); // end BusinessName
                        writer.WriteLine("<p1:BusinessNameControlTxt>" + FormatChecker.GetNameControlCode(mem.BusinessName) + "</p1:BusinessNameControlTxt>"); // need this...
                        writer.WriteLine("<p2:TINRequestTypeCd>BUSINESS_TIN</p2:TINRequestTypeCd>");
                        writer.WriteLine("<p2:EIN>" + mem.EIN + "</p2:EIN>");
                        writer.WriteLine("</p1:OtherALEMembersGrp>"); // end OtherALEMembersGrp
                    }
                }

                // create 1095C elements for each 1095C record
                if (need1095Records(rec, correctionInfo)) {
                    Int32 counter = 1; // use for report IDs
                    // however, if this is not the initial transmission for these records, then we need to start the counter ahead
                    //if (rec.Num1095RecordsProcessed > 0) {
                    //    counter = rec.RecordEndedWith + 1;
                    //}
                    // the Skip() function here should do this:
                    // initially, it should skip 0.  Next time through, it should skip all the records that
                    // have already been processed... etc...
                    foreach (IndividualReport report in rec.IndividualReports.Skip(rec.RecordEndedWith)) {
                        // check file length.  If we're getting close to the 100MB threshold (99.9MB), we need to break up this submission.
                        var currentLength = writer.BaseStream.Length;
                        if (currentLength >= 99990000) { // 99990000 // 50000 for testing?
                            // save current record number (counter) so we'll know where we left off
                            rec.RecordEndedWith += counter - 1; // have to subtract 1, because the counter reflects the NEXT record
                            // how many did we process?
                            //rec.Num1095RecordsProcessed = rec.RecordEndedWith - rec.RecordStartedWith + 1;
                            // tell the record that there are more records left to process
                            rec.HasRecordsToSubmit = true;
                            // set report "Authoritative Transmission" flag to false, for the follow up transmission(s)
                            rec.IsAuthoritative = false;

                            // break out of the loop
                            break;

                        } else { // not close to threshold... keep building!
                            if (rec.RecordStartedWith == 0) { // if this hasn't been set yet, set it
                                rec.RecordStartedWith = counter;
                            }
                            writer.WriteLine("<p1:Form1095CUpstreamDetail recordType=\"\" lineNum=\"0\">"); // is this right? // recordType=\"String\"
                            writer.WriteLine("<p1:RecordId>" + counter.ToString() + "</p1:RecordId>");
                            if (!String.IsNullOrEmpty(report.TestScenarioId) && !isProduction()) {
                                writer.WriteLine("<p1:TestScenarioId>" + report.TestScenarioId + "</p1:TestScenarioId>"); // make this a variable? Form field? ("1-0")
                            }

                            // TODO how to do "correctedInd"?
                            // corrected ind should only be '1' if we're correcting the 1094. 1095 corrections should leave this as '0'
                            bool individualRecordIsCorrection = isIndividualRecordACorrection(correctionInfo);
                            if (individualRecordIsCorrection && !String.IsNullOrEmpty(report.IdOfRecordBeingReplaced)) {
                                writer.WriteLine("<p1:CorrectedInd>1</p1:CorrectedInd>"); // correction stuff... 
                                writer.WriteLine("<p1:CorrectedRecordRecipientGrp>");
                                writer.WriteLine("<p1:CorrectedUniqueRecordId>" + correctionInfo.BadSubmissionReceiptId + "|1|"
                                    + report.IdOfRecordBeingReplaced + "</p1:CorrectedUniqueRecordId>"); // will I need to pull this in differently? what if not correction?
                                writer.WriteLine("<p1:CorrectedRecRecipientPrsnName>");
                                writer.WriteLine("<p1:PersonFirstNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.FirstName)) + "</p1:PersonFirstNm>"); // is this right? Or is it looking for something else?
                                if (!String.IsNullOrEmpty(report.Person.MiddleName)) {
                                    writer.WriteLine("<p1:PersonMiddleNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.MiddleName)) + "</p1:PersonMiddleNm>");
                                }
                                writer.WriteLine("<p1:PersonLastNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.LastName)) + "</p1:PersonLastNm>");
                                if (!String.IsNullOrEmpty(report.Person.Suffix)) {
                                    writer.WriteLine("<p1:SuffixNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.Suffix)) + "</p1:SuffixNm>");
                                }
                                writer.WriteLine("</p1:CorrectedRecRecipientPrsnName>"); // end CorrectedRecRecipientPrsnName
                                if (!String.IsNullOrEmpty(report.Person.SSN)) {
                                    writer.WriteLine("<p1:CorrectedRecRecipientTIN>" + report.Person.SSN + "</p1:CorrectedRecRecipientTIN>"); // hmm...
                                }
                                writer.WriteLine("</p1:CorrectedRecordRecipientGrp>"); // end CorrectedRecRecipientName
                            } else {
                                writer.WriteLine("<p1:CorrectedInd>0</p1:CorrectedInd>");
                            }
                            writer.WriteLine("<p1:TaxYr>" + rec.TaxYr.ToString() + "</p1:TaxYr>");

                            // employee info grp
                            writer.WriteLine("<p1:EmployeeInfoGrp>");
                            writer.WriteLine("<p1:OtherCompletePersonName>");
                            writer.WriteLine("<p1:PersonFirstNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.FirstName)) + "</p1:PersonFirstNm>"); // didn't I just enter this?
                            if (!String.IsNullOrEmpty(report.Person.MiddleName)) {
                                writer.WriteLine("<p1:PersonMiddleNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.MiddleName)) + "</p1:PersonMiddleNm>");
                            }
                            writer.WriteLine("<p1:PersonLastNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.LastName)) + "</p1:PersonLastNm>");
                            if (!String.IsNullOrEmpty(report.Person.Suffix)) {
                                writer.WriteLine("<p1:SuffixNm>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.Suffix)) + "</p1:SuffixNm>");
                            }
                            writer.WriteLine("</p1:OtherCompletePersonName>"); // end irs:OtherCompletePersonName
                            writer.WriteLine("<p1:PersonNameControlTxt>" + FormatChecker.GetNameControlIndividual(FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.LastName))) + "</p1:PersonNameControlTxt>");
                            writer.WriteLine("<p2:TINRequestTypeCd>INDIVIDUAL_TIN</p2:TINRequestTypeCd>");
                            writer.WriteLine("<p2:SSN>" + report.Person.SSN + "</p2:SSN>");
                            writer.WriteLine("<p1:MailingAddressGrp>");
                            writer.WriteLine("<p1:USAddressGrp>");
                            writer.WriteLine("<p1:AddressLine1Txt>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.Address.Address1)) + "</p1:AddressLine1Txt>");
                            if (!String.IsNullOrEmpty(report.Person.Address.Address2)) {
                                writer.WriteLine("<p1:AddressLine2Txt>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.Address.Address2)) + "</p1:AddressLine2Txt>");
                            }
                            writer.WriteLine("<p2:CityNm>" + FormatChecker.FormatCityName(FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.Address.City))) + "</p2:CityNm>");
                            writer.WriteLine("<p1:USStateCd>" + FormatChecker.RemoveEscapedChars(FormatChecker.StripCommasAndPeriods(report.Person.Address.State)) + "</p1:USStateCd>");
                            writer.WriteLine("<p2:USZIPCd>" + FormatChecker.ReturnZipToString((int)report.Person.Address.Zip) + "</p2:USZIPCd>");
                            if (report.Person.Address.ZipExt != null && report.Person.Address.ZipExt != 0 && report.Person.Address.ZipExt.ToString().Length == 4) {
                                writer.WriteLine("<p2:USZIPExtensionCd>" + FormatChecker.ReturnZipExtToString((int)report.Person.Address.ZipExt) + "</p2:USZIPExtensionCd>");
                            }
                            writer.WriteLine("</p1:USAddressGrp>"); // end USAddressGrp
                            writer.WriteLine("</p1:MailingAddressGrp>"); // end MailingAddressGrp
                            writer.WriteLine("</p1:EmployeeInfoGrp>"); // end EmployeeInfoGrp
                            if (!String.IsNullOrEmpty(report.EmployerPhone)) { // using Employer Phone here... hope that's okay
                                writer.WriteLine("<p1:ALEContactPhoneNum>" + report.EmployerPhone + "</p1:ALEContactPhoneNum>"); // not sure if I captured this
                            }

                            // start month number code
                            if (!String.IsNullOrEmpty(report.PlanStartMonth.ToString())) {
                                writer.WriteLine("<p1:StartMonthNumberCd>" + report.PlanStartMonth.ToString() + "</p1:StartMonthNumberCd>");
                            }

                            // employee offer and coverage grp
                            writer.WriteLine("<p1:EmployeeOfferAndCoverageGrp>");
                            if (!String.IsNullOrEmpty(report.CoverageCodeYr)) {
                                writer.WriteLine("<p1:AnnualOfferOfCoverageCd>" + report.CoverageCodeYr + "</p1:AnnualOfferOfCoverageCd>");
                            }
                            // only print the following if one of the months has a value
                            if (isMonthyCoverage(report)) {
                                writer.WriteLine("<p1:MonthlyOfferCoverageGrp>");
                                if (!String.IsNullOrEmpty(report.CoverageCodeJan)) {
                                    writer.WriteLine("<p1:JanOfferCd>" + report.CoverageCodeJan + "</p1:JanOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeFeb)) {
                                    writer.WriteLine("<p1:FebOfferCd>" + report.CoverageCodeFeb + "</p1:FebOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeMar)) {
                                    writer.WriteLine("<p1:MarOfferCd>" + report.CoverageCodeMar + "</p1:MarOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeApr)) {
                                    writer.WriteLine("<p1:AprOfferCd>" + report.CoverageCodeApr + "</p1:AprOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeMay)) {
                                    writer.WriteLine("<p1:MayOfferCd>" + report.CoverageCodeMay + "</p1:MayOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeJun)) {
                                    writer.WriteLine("<p1:JunOfferCd>" + report.CoverageCodeJun + "</p1:JunOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeJul)) {
                                    writer.WriteLine("<p1:JulOfferCd>" + report.CoverageCodeJul + "</p1:JulOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeAug)) {
                                    writer.WriteLine("<p1:AugOfferCd>" + report.CoverageCodeAug + "</p1:AugOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeSep)) {
                                    writer.WriteLine("<p1:SepOfferCd>" + report.CoverageCodeSep + "</p1:SepOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeOct)) {
                                    writer.WriteLine("<p1:OctOfferCd>" + report.CoverageCodeOct + "</p1:OctOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeNov)) {
                                    writer.WriteLine("<p1:NovOfferCd>" + report.CoverageCodeNov + "</p1:NovOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeDec)) {
                                    writer.WriteLine("<p1:DecOfferCd>" + report.CoverageCodeDec + "</p1:DecOfferCd>");
                                }
                                writer.WriteLine("</p1:MonthlyOfferCoverageGrp>"); // end MonthlyOfferCoverageGrp
                            }

                            // low cost section
                            if (!String.IsNullOrEmpty(report.LowCostYr.ToString()) &&
                                report.LowCostYr > 0) {
                                writer.WriteLine("<p1:AnnlEmployeeRequiredContriAmt>" + report.LowCostYr.ToString("n2") + "</p1:AnnlEmployeeRequiredContriAmt>");
                            }
                            if (isMonthlyShare(report)) {
                                // instead of an annual value, there are monthly ones
                                writer.WriteLine("<p1:MonthlyEmployeeRequiredContriGrp>");
                                if (!String.IsNullOrEmpty(report.LowCostJan.ToString()) &&
                                    report.LowCostJan > 0) {
                                    writer.WriteLine("<p1:JanuaryAmt>" + report.LowCostJan.ToString("n2") + "</p1:JanuaryAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostFeb.ToString()) &&
                                    report.LowCostFeb > 0) {
                                    writer.WriteLine("<p1:FebruaryAmt>" + report.LowCostFeb.ToString("n2") + "</p1:FebruaryAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostMar.ToString()) &&
                                    report.LowCostMar > 0) {
                                    writer.WriteLine("<p1:MarchAmt>" + report.LowCostMar.ToString("n2") + "</p1:MarchAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostApr.ToString()) &&
                                    report.LowCostApr > 0) {
                                    writer.WriteLine("<p1:AprilAmt>" + report.LowCostApr.ToString("n2") + "</p1:AprilAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostMay.ToString()) &&
                                    report.LowCostMay > 0) {
                                    writer.WriteLine("<p1:MayAmt>" + report.LowCostMay.ToString("n2") + "</p1:MayAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostJun.ToString()) &&
                                    report.LowCostJun > 0) {
                                    writer.WriteLine("<p1:JuneAmt>" + report.LowCostJun.ToString("n2") + "</p1:JuneAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostJul.ToString()) &&
                                    report.LowCostJul > 0) {
                                    writer.WriteLine("<p1:JulyAmt>" + report.LowCostJul.ToString("n2") + "</p1:JulyAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostAug.ToString()) &&
                                    report.LowCostAug > 0) {
                                    writer.WriteLine("<p1:AugustAmt>" + report.LowCostAug.ToString("n2") + "</p1:AugustAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostSep.ToString()) &&
                                    report.LowCostSep > 0) {
                                    writer.WriteLine("<p1:SeptemberAmt>" + report.LowCostSep.ToString("n2") + "</p1:SeptemberAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostOct.ToString()) &&
                                    report.LowCostOct > 0) {
                                    writer.WriteLine("<p1:OctoberAmt>" + report.LowCostOct.ToString("n2") + "</p1:OctoberAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostNov.ToString()) &&
                                    report.LowCostNov > 0) {
                                    writer.WriteLine("<p1:NovemberAmt>" + report.LowCostNov.ToString("n2") + "</p1:NovemberAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostDec.ToString()) &&
                                    report.LowCostDec > 0) {
                                    writer.WriteLine("<p1:DecemberAmt>" + report.LowCostDec.ToString("n2") + "</p1:DecemberAmt>");
                                }
                                writer.WriteLine("</p1:MonthlyEmployeeRequiredContriGrp>"); // end MonthlyEmployeeRequiredContriGrp
                            }

                            if (!String.IsNullOrEmpty(report.SafeHarborYr)) {
                                writer.WriteLine("<p1:AnnualSafeHarborCd>" + report.SafeHarborYr + "</p1:AnnualSafeHarborCd>");
                            }

                            // if monthly safe harbor is needed...
                            if (isMonthlySafeHarbor(report)) {
                                writer.WriteLine("<p1:MonthlySafeHarborGrp>");
                                if (!String.IsNullOrEmpty(report.SafeHarborJan)) {
                                    writer.WriteLine("<p1:JanSafeHarborCd>" + report.SafeHarborJan + "</p1:JanSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborFeb)) {
                                    writer.WriteLine("<p1:FebSafeHarborCd>" + report.SafeHarborFeb + "</p1:FebSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborMar)) {
                                    writer.WriteLine("<p1:MarSafeHarborCd>" + report.SafeHarborMar + "</p1:MarSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborApr)) {
                                    writer.WriteLine("<p1:AprSafeHarborCd>" + report.SafeHarborApr + "</p1:AprSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborMay)) {
                                    writer.WriteLine("<p1:MaySafeHarborCd>" + report.SafeHarborMay + "</p1:MaySafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborJun)) {
                                    writer.WriteLine("<p1:JunSafeHarborCd>" + report.SafeHarborJun + "</p1:JunSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborJul)) {
                                    writer.WriteLine("<p1:JulSafeHarborCd>" + report.SafeHarborJul + "</p1:JulSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborAug)) {
                                    writer.WriteLine("<p1:AugSafeHarborCd>" + report.SafeHarborAug + "</p1:AugSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborSep)) {
                                    writer.WriteLine("<p1:SepSafeHarborCd>" + report.SafeHarborSep + "</p1:SepSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborOct)) {
                                    writer.WriteLine("<p1:OctSafeHarborCd>" + report.SafeHarborOct + "</p1:OctSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborNov)) {
                                    writer.WriteLine("<p1:NovSafeHarborCd>" + report.SafeHarborNov + "</p1:NovSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborDec)) {
                                    writer.WriteLine("<p1:DecSafeHarborCd>" + report.SafeHarborDec + "</p1:DecSafeHarborCd>");
                                }
                                writer.WriteLine("</p1:MonthlySafeHarborGrp>"); // end MonthlySafeHarborGrp
                            }
                            writer.WriteLine("</p1:EmployeeOfferAndCoverageGrp>"); // end EmployeeOfferAndCoverageGrp

                            // covered individual stuff
                            if (report.CoveredIndividuals != null && report.CoveredIndividuals.Count > 0) {
                                // this line may need to be in the following foreach loop
                                writer.WriteLine("<p1:CoveredIndividualInd>1</p1:CoveredIndividualInd>");
                                // needs to repeat
                                foreach (CoveredIndividual ind in report.CoveredIndividuals) {
                                    writer.WriteLine("<p1:CoveredIndividualGrp>"); // should this be outside the loop?
                                    writer.WriteLine("<p1:CoveredIndividualName>");
                                    writer.WriteLine("<p1:PersonFirstNm>" + FormatChecker.StripCommasAndPeriods(FormatChecker.RemoveEscapedChars(ind.Person.FirstName)) + "</p1:PersonFirstNm>");
                                    if (!String.IsNullOrEmpty(ind.Person.MiddleName)) {
                                        writer.WriteLine("<p1:PersonMiddleNm>" + FormatChecker.StripCommasAndPeriods(FormatChecker.RemoveEscapedChars(ind.Person.MiddleName)) + "</p1:PersonMiddleNm>");
                                    }
                                    writer.WriteLine("<p1:PersonLastNm>" + FormatChecker.StripCommasAndPeriods(FormatChecker.RemoveEscapedChars(ind.Person.LastName)) + "</p1:PersonLastNm>");
                                    if (!String.IsNullOrEmpty(ind.Person.Suffix)) {
                                        writer.WriteLine("<p1:SuffixNm>" + FormatChecker.StripCommasAndPeriods(FormatChecker.RemoveEscapedChars(ind.Person.Suffix)) + "</p1:SuffixNm>");
                                    }
                                    writer.WriteLine("</p1:CoveredIndividualName>"); // end CoveredIndividualName
                                    writer.WriteLine("<p1:PersonNameControlTxt>" + FormatChecker.GetNameControlIndividual(FormatChecker.StripCommasAndPeriods(FormatChecker.RemoveEscapedChars(ind.Person.LastName))) + "</p1:PersonNameControlTxt>"); // not sure about this
                                    if (!String.IsNullOrEmpty(ind.Person.SSN)) {
                                        writer.WriteLine("<p2:TINRequestTypeCd>INDIVIDUAL_TIN</p2:TINRequestTypeCd>");
                                        writer.WriteLine("<p2:SSN>" + ind.Person.SSN + "</p2:SSN>");
                                    }
                                    if (ind.Person.DOB != null && ind.Person.DOB.Year.ToString() != "1") {
                                        writer.WriteLine("<p1:BirthDt>" + ind.Person.DOB.ToString("yyyy-MM-dd") + "</p1:BirthDt>"); // YYYY-MM-DD
                                    }
                                    // if the user was covered all year, don't print months.  If they weren't covered all year, print months
                                    if (ind.IsCoveredYr) {
                                        writer.WriteLine("<p1:CoveredIndividualAnnualInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredYr).ToString() + 
                                            "</p1:CoveredIndividualAnnualInd>"); // might need an X
                                    } else {
                                        writer.WriteLine("<p1:CoveredIndividualMonthlyIndGrp>");
                                        writer.WriteLine("<p1:JanuaryInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredJan).ToString() + "</p1:JanuaryInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:FebruaryInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredFeb).ToString() + "</p1:FebruaryInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:MarchInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredMar).ToString() + "</p1:MarchInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:AprilInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredApr).ToString() + "</p1:AprilInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:MayInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredMay).ToString() + "</p1:MayInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:JuneInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredJun).ToString() + "</p1:JuneInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:JulyInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredJul).ToString() + "</p1:JulyInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:AugustInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredAug).ToString() + "</p1:AugustInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:SeptemberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredSep).ToString() + "</p1:SeptemberInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:OctoberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredOct).ToString() + "</p1:OctoberInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:NovemberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredNov).ToString() + "</p1:NovemberInd>"); // might need an X for true...
                                        writer.WriteLine("<p1:DecemberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredDec).ToString() + "</p1:DecemberInd>"); // might need an X for true...
                                        writer.WriteLine("</p1:CoveredIndividualMonthlyIndGrp>"); // end CoveredIndividualMonthlyInd
                                    }
                                    
                                    writer.WriteLine("</p1:CoveredIndividualGrp>"); // end CoveredIndividualGrp
                                } // end foreach (inner)
                            } // end if

                            writer.WriteLine("</p1:Form1095CUpstreamDetail>"); // end Form1095CUpstreamDetail

                            // track number of records submitted in this transmission
                            rec.Num1095RecordsProcessed = counter;

                            counter++; // increment counter
                        } // end if filesize check...
                    } // end foreach (outer)
                    // reset counter (seems to stay hung up sometimes...)
                    counter = 0;
                } // end if

                writer.WriteLine("</p1:Form1094CUpstreamDetail>"); // end 1094C
                writer.Write("</p:Form109495CTransmittalUpstream>"); // end doc

                writer.Flush();
                writer.Close();

                // change the Form1095CAttachedCnt to reflect the actual number of records being sent, not the total number
                // do this if there are still records left to submit, or if it's obvious that we've submitted records in a 
                // previous transmission
                if (rec.HasRecordsToSubmit || rec.RecordEndedWith > 0) {
                    changeNum1095Records(rec);
                }

                // get file size
                FileInfo fi = new FileInfo(rec.XmlFilePath);
                rec.ByteSz = Convert.ToInt32(fi.Length);

                // get file in bytes
                blob = System.IO.File.ReadAllBytes(rec.XmlFilePath);

            }


            return blob;
        } // end BuildXmlDoc()



        // HELPER FUNCTIONS
        // change the number of 1095 records being sent in the transmission
        private static void changeNum1095Records(Record rec) {
            // first, move the file to a new file name
            string newFilePath = rec.Directory + "/" + rec.CurrentSubDirectory + "/tempfile.xml";
            System.IO.File.Move(rec.XmlFilePath, newFilePath);

            // pull in the file, edit it, resave as original filename
            using (FileStream fs = System.IO.File.Open(newFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader reader = new StreamReader(bs)) {
                using (StreamWriter writer = new StreamWriter(rec.XmlFilePath)) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        if (line.Contains("<p1:Form1095CAttachedCnt>")) {
                            line = "<p1:Form1095CAttachedCnt>" + rec.Num1095RecordsProcessed.ToString() + "</p1:Form1095CAttachedCnt>";
                        }
                        // write the lines back to the original file (all lines)
                        if (line.Contains("</p:Form109495CTransmittalUpstream>")) { // if last line, just use Write()
                            writer.Write(line);
                        } else { // else, use WriteLine()
                            writer.WriteLine(line);
                        }
                    }
                } // closes StreamWriter
            } // closes StreamReader, BufferedStream, and FileStream

            // delete new file
            System.IO.File.Delete(newFilePath);
        }


        // see if monthly coverage is needed
        private static bool isMonthyCoverage(IndividualReport rep) {
            bool result = false;
            if (!String.IsNullOrEmpty(rep.CoverageCodeJan) || !String.IsNullOrEmpty(rep.CoverageCodeFeb) ||
                !String.IsNullOrEmpty(rep.CoverageCodeMar) || !String.IsNullOrEmpty(rep.CoverageCodeApr) ||
                !String.IsNullOrEmpty(rep.CoverageCodeMay) || !String.IsNullOrEmpty(rep.CoverageCodeJun) ||
                !String.IsNullOrEmpty(rep.CoverageCodeJul) || !String.IsNullOrEmpty(rep.CoverageCodeAug) ||
                !String.IsNullOrEmpty(rep.CoverageCodeSep) || !String.IsNullOrEmpty(rep.CoverageCodeOct) ||
                !String.IsNullOrEmpty(rep.CoverageCodeNov) || !String.IsNullOrEmpty(rep.CoverageCodeDec)) {
                result = true;
            }

            return result;
        }

        // see if monthly safe harbor is needed
        private static bool isMonthlySafeHarbor(IndividualReport rep) {
            bool result = false;
            if (!String.IsNullOrEmpty(rep.SafeHarborJan) || !String.IsNullOrEmpty(rep.SafeHarborFeb) ||
                !String.IsNullOrEmpty(rep.SafeHarborMar) || !String.IsNullOrEmpty(rep.SafeHarborApr) ||
                !String.IsNullOrEmpty(rep.SafeHarborMay) || !String.IsNullOrEmpty(rep.SafeHarborJun) ||
                !String.IsNullOrEmpty(rep.SafeHarborJul) || !String.IsNullOrEmpty(rep.SafeHarborAug) ||
                !String.IsNullOrEmpty(rep.SafeHarborSep) || !String.IsNullOrEmpty(rep.SafeHarborOct) ||
                !String.IsNullOrEmpty(rep.SafeHarborNov) || !String.IsNullOrEmpty(rep.SafeHarborDec)) {
                result = true;
            }

            return result;
        }

        // see if monthly share is needed
        private static bool isMonthlyShare(IndividualReport rep) {
            bool result = false;
            if ((!String.IsNullOrEmpty(rep.LowCostJan.ToString()) && rep.LowCostJan > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostFeb.ToString()) && rep.LowCostFeb > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostMar.ToString()) && rep.LowCostMar > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostApr.ToString()) && rep.LowCostApr > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostMay.ToString()) && rep.LowCostMay > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostJun.ToString()) && rep.LowCostJun > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostJul.ToString()) && rep.LowCostJul > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostAug.ToString()) && rep.LowCostAug > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostSep.ToString()) && rep.LowCostSep > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostOct.ToString()) && rep.LowCostOct > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostNov.ToString()) && rep.LowCostNov > 0) ||
                (!String.IsNullOrEmpty(rep.LowCostDec.ToString()) && rep.LowCostDec > 0)) {
                result = true;
            }
            return result;
        }


        // see if 1095 records are needed
        private static bool need1095Records(Record rec, CorrectionInfo ci) {
            var result = false;

            // if the record has individual reports AND there's either no correction info, or 
            // there's correction info but it's for 1095 record(s), then return 'true'
            // if there's correction info, but it's for a replacement, then 1095s are always required
            if ((rec.IndividualReports != null && rec.IndividualReports.Count > 0) &&
                (ci.BadSubmissionReceiptId == null || (ci.BadSubmissionReceiptId != null &&
                (ci.FormType == FormType.Form1095 || rec.SubmissionType == "R")))) {
                result = true;
            }

            return result;
        }


        private static bool isIndividualRecordACorrection(CorrectionInfo ci) {
            var result = false;

            // if there's a "bad submission id" and the form type has been set to 1095, then this individual record
            // is a correction
            if (!String.IsNullOrEmpty(ci.BadSubmissionReceiptId) && ci.FormType == FormType.Form1095) {
                result = true;
            }

            return result;
        }

    } // end class
} // end namespace