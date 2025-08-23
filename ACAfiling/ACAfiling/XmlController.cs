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
    public class XmlController : Controller {

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
                writer.WriteLine("<n1:Form109495CTransmittalUpstream " +
                    "xmlns=\"urn:us:gov:treasury:irs:ext:aca:air:7.0\" " +
                    "xmlns:irs=\"urn:us:gov:treasury:irs:common\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                    "xmlns:n1=\"urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage\" " +
                    "xsi:schemaLocation=\"urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage " +
                    "IRS-Form1094-1095CTransmitterUpstreamMessage.xsd\">");
                writer.WriteLine("<Form1094CUpstreamDetail recordType=\"String\" lineNum=\"0\">");
                // SubmissionId should be 1, since we'll only submit a single 1094C in a transmission (though more are allowed)
                // OR this needs to be set programmatcially or through a variable...
                writer.WriteLine("<SubmissionId>1</SubmissionId>");
                // -------
                // TEST SCENARIO ID!!!!!
                if (!isProduction()) {
                    writer.WriteLine("<TestScenarioId>" + rec.TestScenarioId + "</TestScenarioId>");
                }
                //--------
                // REPLACEMENT STUFF!!!
                if (rec.SubmissionType == "R") {
                    if (correctionInfo.IsWholeTransReplacement) {
                        writer.WriteLine("<OriginalReceiptId>" + correctionInfo.BadSubmissionReceiptId + "</OriginalReceiptId>");
                    } else {
                        writer.WriteLine("<OriginalUniqueSubmissionId>" + correctionInfo.BadSubmissionReceiptId + "|1</OriginalUniqueSubmissionId>");
                    }
                }

                writer.WriteLine("<irs:TaxYr>" + rec.TaxYr.ToString() + "</irs:TaxYr>");

                // corrected info elements
                if (rec.SubmissionType == "C" && correctionInfo.FormType == FormType.Form1094) { // "O" is "original"
                    writer.WriteLine("<CorrectedInd>1</CorrectedInd>");
                    writer.WriteLine("<CorrectedSubmissionInfoGrp>");
                    // the following line expects a format of "1095C-16-00039344|1"
                    // the trailing "1" is the submission within a specific transmission.
                    // that will always be "1" in our case, since we only do one submission per transmission
                    writer.WriteLine("<CorrectedUniqueSubmissionId>" + correctionInfo.BadSubmissionReceiptId + "|1</CorrectedUniqueSubmissionId>");
                    writer.WriteLine("<CorrectedSubmissionPayerName>");
                    writer.WriteLine("<BusinessNameLine1Txt>" + rec.Company.Name + "</BusinessNameLine1Txt>");
                    if (!String.IsNullOrEmpty(rec.Company.Name2)) {
                        writer.WriteLine("<BusinessNameLine2Txt>" + rec.Company.Name2 + "</BusinessNameLine2Txt>");
                    }
                    writer.WriteLine("</CorrectedSubmissionPayerName>"); // end CorrectedSubmissionPayerName
                    writer.WriteLine("<CorrectedSubmissionPayerTIN>" + rec.Company.EIN + "</CorrectedSubmissionPayerTIN>"); 
                    writer.WriteLine("</CorrectedSubmissionInfoGrp>"); // end CorrectedSubmissionInfoGroup
                } else {
                    writer.WriteLine("<CorrectedInd>0</CorrectedInd>");
                }// end if

                // employer information group
                writer.WriteLine("<EmployerInformationGrp>");
                writer.WriteLine("<BusinessName>");
                writer.WriteLine("<BusinessNameLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Name) + "</BusinessNameLine1Txt>");
                if (!String.IsNullOrEmpty(rec.Company.Name2)) {
                    writer.WriteLine("<BusinessNameLine2Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Name2) + "</BusinessNameLine2Txt>");
                }
                writer.WriteLine("</BusinessName>"); // end BusinessName
                writer.WriteLine("<BusinessNameControlTxt>" + FormatChecker.GetNameControlCode(rec.Company.Name) + "</BusinessNameControlTxt>"); // not sure how to get this. Looks like the first 3 or 4 letters of a company name? Comp & Ref pg 20
                writer.WriteLine("<irs:TINRequestTypeCd>BUSINESS_TIN</irs:TINRequestTypeCd>"); // hard coding for now. could also be "INDIVIDUAL_TIN"
                writer.WriteLine("<irs:EmployerEIN>" + rec.Company.EIN + "</irs:EmployerEIN>"); // may need to remove dash
                writer.WriteLine("<MailingAddressGrp>");
                writer.WriteLine("<USAddressGrp>");
                writer.WriteLine("<AddressLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Address.Address1) + "</AddressLine1Txt>");
                if (!String.IsNullOrEmpty(rec.Company.Address.Address2)) {
                    writer.WriteLine("<AddressLine2Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.Address.Address2) + "</AddressLine2Txt>");
                }
                writer.WriteLine("<irs:CityNm>" + rec.Company.Address.City + "</irs:CityNm>");
                writer.WriteLine("<USStateCd>" + rec.Company.Address.State + "</USStateCd>");
                writer.WriteLine("<irs:USZIPCd>" + rec.Company.Address.Zip.ToString() + "</irs:USZIPCd>");
                if (rec.Company.Address.ZipExt != null) {
                    writer.WriteLine("<irs:USZIPExtensionCd>" + rec.Company.Address.ZipExt.ToString() + "</irs:USZIPExtensionCd>");
                }
                writer.WriteLine("</USAddressGrp>"); // end USAddressGrp
                writer.WriteLine("</MailingAddressGrp>"); // end MailingAddressGrp
                writer.WriteLine("<ContactNameGrp>");
                writer.WriteLine("<PersonFirstNm>" + rec.Company.ContactPerson.FirstName + "</PersonFirstNm>");
                if (!String.IsNullOrEmpty(rec.Company.ContactPerson.MiddleName)) {
                    writer.WriteLine("<PersonMiddleNm>" + rec.Company.ContactPerson.MiddleName + "</PersonMiddleNm>");
                }
                writer.WriteLine("<PersonLastNm>" + rec.Company.ContactPerson.LastName + "</PersonLastNm>");
                if (rec.SubmissionType == "R") {
                    writer.WriteLine("<SuffixNm>-" + rec.Company.ContactPerson.Suffix + "</SuffixNm>");
                } else if (!String.IsNullOrEmpty(rec.Company.ContactPerson.Suffix)) {
                    writer.WriteLine("<SuffixNm>" + rec.Company.ContactPerson.Suffix + "</SuffixNm>");
                }
                writer.WriteLine("</ContactNameGrp>"); // end ContactNameGrp
                writer.WriteLine("<ContactPhoneNum>" + rec.Company.ContactPhone + "</ContactPhoneNum>"); // remove dashes?
                writer.WriteLine("</EmployerInformationGrp>"); // end EmployerInformationGrp

                // gov entity info
                // is this whole block optional?
                if (!String.IsNullOrEmpty(rec.Company.GovEntityName)) {
                    writer.WriteLine("<GovtEntityEmployerInfoGrp>");
                    writer.WriteLine("<BusinessName>");
                    writer.WriteLine("<BusinessNameLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityName) + "</BusinessNameLine1Txt>");
                    if (!String.IsNullOrEmpty(rec.Company.GovEntityName2)) {
                        writer.WriteLine("<BusinessNameLine2Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityName2) + "</BusinessNameLine2Txt>");
                    }
                    writer.WriteLine("</BusinessName>"); // end BusinessName
                    writer.WriteLine("<BusinessNameControlTxt>" + FormatChecker.GetNameControlCode(rec.Company.GovEntityName) + "</BusinessNameControlTxt>"); // again... need to figure this out
                    writer.WriteLine("<irs:TINRequestTypeCd>BUSINESS_TIN</irs:TINRequestTypeCd>");
                    writer.WriteLine("<irs:EmployerEIN>" + rec.Company.GovEntityEIN + "</irs:EmployerEIN>");
                    writer.WriteLine("<MailingAddressGrp>");
                    writer.WriteLine("<USAddressGrp>");
                    if (rec.SubmissionType == "R") {
                        writer.WriteLine("<AddressLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityAddress.Address1) + " </AddressLine1Txt>");
                    } else {
                        writer.WriteLine("<AddressLine1Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityAddress.Address1) + "</AddressLine1Txt>");
                    }
                    
                    if (!String.IsNullOrEmpty(rec.Company.GovEntityAddress.Address2)) {
                        writer.WriteLine("<AddressLine2Txt>" + FormatChecker.StripCommasAndPeriods(rec.Company.GovEntityAddress.Address2) + "</AddressLine2Txt>");
                    }
                    writer.WriteLine("<irs:CityNm>" + rec.Company.GovEntityAddress.City + "</irs:CityNm>");
                    writer.WriteLine("<USStateCd>" + rec.Company.GovEntityAddress.State + "</USStateCd>");
                    writer.WriteLine("<irs:USZIPCd>" + rec.Company.GovEntityAddress.Zip.ToString() + "</irs:USZIPCd>");
                    if (rec.Company.GovEntityAddress.ZipExt != null) {
                        writer.WriteLine("<irs:USZIPExtensionCd>" + rec.Company.GovEntityAddress.ZipExt.ToString() + "</irs:USZIPExtensionCd>");
                    }
                    writer.WriteLine("</USAddressGrp>"); // end USAddressGrp
                    writer.WriteLine("</MailingAddressGrp>"); // end MailingAddressGrp
                    writer.WriteLine("<ContactNameGrp>");
                    writer.WriteLine("<PersonFirstNm>" + rec.Company.GovContact.FirstName + "</PersonFirstNm>");
                    if (!String.IsNullOrEmpty(rec.Company.GovContact.MiddleName)) {
                        writer.WriteLine("<PersonMiddleNm>" + rec.Company.GovContact.MiddleName + "</PersonMiddleNm>");
                    }
                    writer.WriteLine("<PersonLastNm>" + rec.Company.GovContact.LastName + "</PersonLastNm>");
                    if (!String.IsNullOrEmpty(rec.Company.GovContact.Suffix)) {
                        writer.WriteLine("<SuffixNm>" + rec.Company.GovContact.Suffix + "</SuffixNm>");
                    }
                    writer.WriteLine("</ContactNameGrp>"); // end ContactNameGrp
                    writer.WriteLine("<ContactPhoneNum>" + rec.Company.GovContactPhone + "</ContactPhoneNum>"); // may need to lose dashes
                    writer.WriteLine("</GovtEntityEmployerInfoGrp>"); // end GovtEntityEmployerInfoGrp
                }

                // misc info before 1095 records
                // several items will only show up if this is an "authoritativeTransmittal" -- rec.IsAuthoritative
                writer.WriteLine("<Form1095CAttachedCnt>" + rec.Ct1095Transmittal.ToString() + "</Form1095CAttachedCnt>");
                writer.WriteLine("<AuthoritativeTransmittalInd>" + FormatChecker.BoolToDigitBool(rec.IsAuthoritative) + "</AuthoritativeTransmittalInd>");
                if (rec.Ct1095TotalALE != null && rec.Ct1095TotalALE > 0 && rec.IsAuthoritative) {
                    writer.WriteLine("<TotalForm1095CALEMemberCnt>" + rec.Ct1095TotalALE.ToString() + "</TotalForm1095CALEMemberCnt>"); // don't think I have a value for this...
                }
                if (rec.IsAuthoritative) { // only show these if IsAuthoritative
                    writer.WriteLine("<AggregatedGroupMemberCd>" + rec.IsMemberOfAggregatedGroup + "</AggregatedGroupMemberCd>"); // true or false
                    writer.WriteLine("<QualifyingOfferMethodInd>" + FormatChecker.BoolToDigitBool(rec.IsQualOfferMethod) + "</QualifyingOfferMethodInd>"); // might need "X" for true...
                    writer.WriteLine("<QlfyOfferMethodTrnstReliefInd>" + FormatChecker.BoolToDigitBool(rec.IsQualOfferMethodTransitionRelief) + "</QlfyOfferMethodTrnstReliefInd>"); // might need "X" for true...
                    writer.WriteLine("<Section4980HReliefInd>" + FormatChecker.BoolToDigitBool(rec.IsSec4980HTransitionRelief) + "</Section4980HReliefInd>"); // might need "X" for true...
                    writer.WriteLine("<NinetyEightPctOfferMethodInd>" + FormatChecker.BoolToDigitBool(rec.Is98PercentOfferMethod) + "</NinetyEightPctOfferMethodInd>"); // might need "X" for true...
                }
                //writer.WriteLine("<JuratSignaturePIN>" + "</JuratSignaturePIN>"); // pg 61 Comp & Ref: not needed
                //writer.WriteLine("<irs:PersonTitleTxt>" + "</irs:PersonTitleTxt>"); // pg 61 Comp & Ref: not needed
                DateTime today = DateTime.Now;
                // following line not needed, according to IRS email 2/19/16
                //writer.WriteLine("<irs:SignatureDt>" + today.ToString("yyyy-MM-dd") + "</irs:SignatureDt>"); // YYYY-MM-DD (is using today okay?)

                // COVERAGE OBJECTS
                // ALE member info group
                // going to try to cut this out in the cases when the yearly amount is 0. that would mean that all the monthly
                // amounts are also 0, and therefore, not needed.  Maybe that will resolve the AIRSH100 error I'm getting...
                if (rec.Ct1095TotalALE != null && rec.Ct1095TotalALE > 0 && rec.IsAuthoritative) {
                    writer.WriteLine("<ALEMemberInformationGrp>");
                    writer.WriteLine("<YearlyALEMemberDetail>");
                    if (rec.HadMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageYr.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageYr.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageYr.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageYr.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageYr.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageYr.IsAggregatedGroup.ToString())) {
                        writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageYr.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageYr.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageYr.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</YearlyALEMemberDetail>"); // end YearlyALEMemberDetail

                    // January
                    writer.WriteLine("<JanALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else { // otherwise, just print whatever the value was from the form
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageJan.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJan.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageJan.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJan.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageJan.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJan.IsAggregatedGroup.ToString())) {
                        writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageJan.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJan.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageJan.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</JanALEMonthlyInfoGrp>"); // end JanALEMonthlyInfoGrp

                    // February
                    writer.WriteLine("<FebALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageFeb.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageFeb.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageFeb.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageFeb.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageFeb.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageFeb.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageFeb.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageFeb.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</FebALEMonthlyInfoGrp>"); // end FebALEMonthlyInfoGrp

                    // March
                    writer.WriteLine("<MarALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageMar.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMar.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageMar.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMar.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageMar.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageMar.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageMar.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageMar.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</MarALEMonthlyInfoGrp>"); // end MarALEMonthlyInfoGrp

                    // April
                    writer.WriteLine("<AprALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageApr.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageApr.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageApr.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageApr.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageApr.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageApr.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageApr.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageApr.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</AprALEMonthlyInfoGrp>"); // end AprALEMonthlyInfoGrp

                    // May
                    writer.WriteLine("<MayALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageMay.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMay.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageMay.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageMay.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageMay.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageMay.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageMay.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageMay.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</MayALEMonthlyInfoGrp>"); // end MayALEMonthlyInfoGrp

                    // June
                    writer.WriteLine("<JunALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageJun.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJun.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageJun.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJun.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageJun.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageJun.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageJun.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageJun.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</JunALEMonthlyInfoGrp>"); // end JunALEMonthlyInfoGrp

                    // July
                    writer.WriteLine("<JulALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageJul.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJul.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageJul.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageJul.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageJul.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageJul.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageJul.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageJul.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</JulALEMonthlyInfoGrp>"); // end JulALEMonthlyInfoGrp

                    // August
                    writer.WriteLine("<AugALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageAug.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageAug.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageAug.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageAug.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageAug.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageAug.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageAug.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageAug.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</AugALEMonthlyInfoGrp>"); // end AugALEMonthlyInfoGrp

                    // September
                    writer.WriteLine("<SeptALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageSep.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageSep.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageSep.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageSep.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageSep.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageSep.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageSep.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageSep.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</SeptALEMonthlyInfoGrp>"); // end SeptALEMonthlyInfoGrp

                    // October
                    writer.WriteLine("<OctALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageOct.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageOct.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageOct.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageOct.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageOct.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageOct.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageOct.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageOct.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</OctALEMonthlyInfoGrp>"); // end OctALEMonthlyInfoGrp

                    // November
                    writer.WriteLine("<NovALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageNov.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageNov.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageNov.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageNov.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageNov.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageNov.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageNov.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageNov.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</NovALEMonthlyInfoGrp>"); // end NovALEMonthlyInfoGrp

                    // December
                    writer.WriteLine("<DecALEMonthlyInfoGrp>");
                    if (rec.HadMinEssentialCoverageForTheYr) { // flag each month true, if they had it for the year
                        writer.WriteLine("<MinEssentialCvrOffrCd>1</MinEssentialCvrOffrCd>");
                    } else if (rec.DidNotHaveMinEssentialCoverageForTheYr) {
                        writer.WriteLine("<MinEssentialCvrOffrCd>2</MinEssentialCvrOffrCd>");
                    } else {
                        writer.WriteLine("<MinEssentialCvrOffrCd>" + rec.CoverageDec.IsMinEssentialCovOffer + "</MinEssentialCvrOffrCd>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageDec.FTEmployeeCt.ToString())) {
                        writer.WriteLine("<ALEMemberFTECnt>" + rec.CoverageDec.FTEmployeeCt.ToString() + "</ALEMemberFTECnt>");
                    }
                    if (!String.IsNullOrEmpty(rec.CoverageDec.TotEmployeeCt.ToString())) {
                        writer.WriteLine("<TotalEmployeeCnt>" + rec.CoverageDec.TotEmployeeCt.ToString() + "</TotalEmployeeCnt>");
                    }
                    writer.WriteLine("<AggregatedGroupInd>" + FormatChecker.BoolToDigitBool(rec.CoverageDec.IsAggregatedGroup) + "</AggregatedGroupInd>"); // might need "X" for true...
                    if (!String.IsNullOrEmpty(rec.CoverageDec.Sec4980HTransReliefIndicator)) {
                        writer.WriteLine("<ALESect4980HTrnstReliefCd>" + rec.CoverageDec.Sec4980HTransReliefIndicator + "</ALESect4980HTrnstReliefCd>");
                    }
                    writer.WriteLine("</DecALEMonthlyInfoGrp>"); // end DecALEMonthlyInfoGrp
                    writer.WriteLine("</ALEMemberInformationGrp>"); // ALEMemberInformationGrp
                }

                // other ALE members group info
                if (rec.OtherMembers != null && rec.OtherMembers.Count > 0 && rec.IsAuthoritative) {
                    // populate each one
                    foreach (OtherMember mem in rec.OtherMembers) {
                        writer.WriteLine("<OtherALEMembersGrp>");
                        writer.WriteLine("<BusinessName>");
                        writer.WriteLine("<BusinessNameLine1Txt>" + FormatChecker.StripCommasAndPeriods(mem.BusinessName) + "</BusinessNameLine1Txt>");
                        if (!String.IsNullOrEmpty(mem.BusinessName2)) {
                            writer.WriteLine("<BusinessNameLine2Txt>" + FormatChecker.StripCommasAndPeriods(mem.BusinessName2) + "</BusinessNameLine2Txt>");
                        }
                        writer.WriteLine("</BusinessName>"); // end BusinessName
                        writer.WriteLine("<BusinessNameControlTxt>" + FormatChecker.GetNameControlCode(mem.BusinessName) + "</BusinessNameControlTxt>"); // need this...
                        writer.WriteLine("<irs:TINRequestTypeCd>BUSINESS_TIN</irs:TINRequestTypeCd>");
                        writer.WriteLine("<irs:EIN>" + mem.EIN + "</irs:EIN>");
                        writer.WriteLine("</OtherALEMembersGrp>"); // end OtherALEMembersGrp
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
                            writer.WriteLine("<Form1095CUpstreamDetail recordType=\"String\" lineNum=\"0\">"); // is this right?
                            writer.WriteLine("<RecordId>" + counter.ToString() + "</RecordId>");
                            if (!String.IsNullOrEmpty(report.TestScenarioId) && !isProduction()) {
                                writer.WriteLine("<TestScenarioId>" + report.TestScenarioId + "</TestScenarioId>"); // make this a variable? Form field? ("1-0")
                            }

                            // TODO how to do "correctedInd"?
                            // corrected ind should only be '1' if we're correcting the 1094. 1095 corrections should leave this as '0'
                            bool individualRecordIsCorrection = isIndividualRecordACorrection(correctionInfo);
                            if (individualRecordIsCorrection && !String.IsNullOrEmpty(report.IdOfRecordBeingReplaced)) {
                                writer.WriteLine("<CorrectedInd>1</CorrectedInd>"); // correction stuff... 
                                writer.WriteLine("<CorrectedRecordInfoGrp>");
                                writer.WriteLine("<CorrectedUniqueRecordId>" + correctionInfo.BadSubmissionReceiptId + "|1|"
                                    + report.IdOfRecordBeingReplaced + "</CorrectedUniqueRecordId>"); // will I need to pull this in differently? what if not correction?
                                writer.WriteLine("<CorrectedRecordPayeeName>");
                                writer.WriteLine("<PersonFirstNm>" + report.Person.FirstName + "</PersonFirstNm>"); // is this right? Or is it looking for something else?
                                if (!String.IsNullOrEmpty(report.Person.MiddleName)) {
                                    writer.WriteLine("<PersonMiddleNm>" + report.Person.MiddleName + "</PersonMiddleNm>");
                                }
                                writer.WriteLine("<PersonLastNm>" + report.Person.LastName + "</PersonLastNm>");
                                if (!String.IsNullOrEmpty(report.Person.Suffix)) {
                                    writer.WriteLine("<SuffixNm>" + report.Person.Suffix + "</SuffixNm>");
                                }
                                writer.WriteLine("</CorrectedRecordPayeeName>"); // end CorrectedRecordPayeeName
                                if (!String.IsNullOrEmpty(report.Person.SSN)) {
                                    writer.WriteLine("<CorrectedRecordPayeeTIN>" + report.Person.SSN + "</CorrectedRecordPayeeTIN>"); // hmm...
                                }
                                writer.WriteLine("</CorrectedRecordInfoGrp>"); // end CorrectedRecordInfoGrp
                            } else {
                                writer.WriteLine("<CorrectedInd>0</CorrectedInd>");
                            }
                            writer.WriteLine("<irs:TaxYr>" + rec.TaxYr.ToString() + "</irs:TaxYr>");

                            // employee info grp
                            writer.WriteLine("<EmployeeInfoGrp>");
                            writer.WriteLine("<OtherCompletePersonName>");
                            writer.WriteLine("<PersonFirstNm>" + report.Person.FirstName + "</PersonFirstNm>"); // didn't I just enter this?
                            if (!String.IsNullOrEmpty(report.Person.MiddleName)) {
                                writer.WriteLine("<PersonMiddleNm>" + report.Person.MiddleName + "</PersonMiddleNm>");
                            }
                            writer.WriteLine("<PersonLastNm>" + report.Person.LastName + "</PersonLastNm>");
                            if (!String.IsNullOrEmpty(report.Person.Suffix)) {
                                writer.WriteLine("<SuffixNm>" + report.Person.Suffix + "</SuffixNm>");
                            }
                            writer.WriteLine("</OtherCompletePersonName>"); // end irs:OtherCompletePersonName
                            writer.WriteLine("<PersonNameControlTxt>" + FormatChecker.GetNameControlIndividual(report.Person.LastName) + "</PersonNameControlTxt>");
                            writer.WriteLine("<irs:TINRequestTypeCd>INDIVIDUAL_TIN</irs:TINRequestTypeCd>");
                            writer.WriteLine("<irs:SSN>" + report.Person.SSN + "</irs:SSN>");
                            writer.WriteLine("<MailingAddressGrp>");
                            writer.WriteLine("<USAddressGrp>");
                            writer.WriteLine("<AddressLine1Txt>" + FormatChecker.StripCommasAndPeriods(report.Person.Address.Address1) + "</AddressLine1Txt>");
                            if (!String.IsNullOrEmpty(report.Person.Address.Address2)) {
                                writer.WriteLine("<AddressLine2Txt>" + FormatChecker.StripCommasAndPeriods(report.Person.Address.Address2) + "</AddressLine2Txt>");
                            }
                            writer.WriteLine("<irs:CityNm>" + report.Person.Address.City + "</irs:CityNm>");
                            writer.WriteLine("<USStateCd>" + report.Person.Address.State + "</USStateCd>");
                            writer.WriteLine("<irs:USZIPCd>" + report.Person.Address.Zip.ToString() + "</irs:USZIPCd>");
                            if (report.Person.Address.ZipExt != null && report.Person.Address.ZipExt != 0) {
                                writer.WriteLine("<irs:USZIPExtensionCd>" + report.Person.Address.ZipExt.ToString() + "</irs:USZIPExtensionCd>");
                            }
                            writer.WriteLine("</USAddressGrp>"); // end USAddressGrp
                            writer.WriteLine("</MailingAddressGrp>"); // end MailingAddressGrp
                            writer.WriteLine("</EmployeeInfoGrp>"); // end EmployeeInfoGrp
                            if (!String.IsNullOrEmpty(report.EmployerPhone)) { // using Employer Phone here... hope that's okay
                                writer.WriteLine("<ALEContactPhoneNum>" + report.EmployerPhone + "</ALEContactPhoneNum>"); // not sure if I captured this
                            }

                            // start month number code
                            if (!String.IsNullOrEmpty(report.PlanStartMonth.ToString())) {
                                writer.WriteLine("<StartMonthNumberCd>" + report.PlanStartMonth.ToString() + "</StartMonthNumberCd>");
                            }

                            // employee offer and coverage grp
                            writer.WriteLine("<EmployeeOfferAndCoverageGrp>");
                            if (!String.IsNullOrEmpty(report.CoverageCodeYr)) {
                                writer.WriteLine("<AnnualOfferOfCoverageCd>" + report.CoverageCodeYr + "</AnnualOfferOfCoverageCd>");
                            }
                            // only print the following if one of the months has a value
                            if (isMonthyCoverage(report)) {
                                writer.WriteLine("<MonthlyOfferCoverageGrp>");
                                if (!String.IsNullOrEmpty(report.CoverageCodeJan)) {
                                    writer.WriteLine("<JanOfferCd>" + report.CoverageCodeJan + "</JanOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeFeb)) {
                                    writer.WriteLine("<FebOfferCd>" + report.CoverageCodeFeb + "</FebOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeMar)) {
                                    writer.WriteLine("<MarOfferCd>" + report.CoverageCodeMar + "</MarOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeApr)) {
                                    writer.WriteLine("<AprOfferCd>" + report.CoverageCodeApr + "</AprOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeMay)) {
                                    writer.WriteLine("<MayOfferCd>" + report.CoverageCodeMay + "</MayOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeJun)) {
                                    writer.WriteLine("<JunOfferCd>" + report.CoverageCodeJun + "</JunOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeJul)) {
                                    writer.WriteLine("<JulOfferCd>" + report.CoverageCodeJul + "</JulOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeAug)) {
                                    writer.WriteLine("<AugOfferCd>" + report.CoverageCodeAug + "</AugOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeSep)) {
                                    writer.WriteLine("<SepOfferCd>" + report.CoverageCodeSep + "</SepOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeOct)) {
                                    writer.WriteLine("<OctOfferCd>" + report.CoverageCodeOct + "</OctOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeNov)) {
                                    writer.WriteLine("<NovOfferCd>" + report.CoverageCodeNov + "</NovOfferCd>");
                                }
                                if (!String.IsNullOrEmpty(report.CoverageCodeDec)) {
                                    writer.WriteLine("<DecOfferCd>" + report.CoverageCodeDec + "</DecOfferCd>");
                                }
                                writer.WriteLine("</MonthlyOfferCoverageGrp>"); // end MonthlyOfferCoverageGrp
                            }

                            // low cost section
                            if (!String.IsNullOrEmpty(report.LowCostYr.ToString()) &&
                                report.LowCostYr > 0) {
                                writer.WriteLine("<AnnlShrLowestCostMthlyPremAmt>" + report.LowCostYr.ToString("n2") + "</AnnlShrLowestCostMthlyPremAmt>");
                            }
                            if (isMonthlyShare(report)) {
                                // instead of an annual value, there are monthly ones
                                writer.WriteLine("<MonthlyShareOfLowestCostMonthlyPremGrp>");
                                if (!String.IsNullOrEmpty(report.LowCostJan.ToString()) &&
                                    report.LowCostJan > 0) {
                                    writer.WriteLine("<JanuaryAmt>" + report.LowCostJan.ToString("n2") + "</JanuaryAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostFeb.ToString()) &&
                                    report.LowCostFeb > 0) {
                                    writer.WriteLine("<FebruaryAmt>" + report.LowCostFeb.ToString("n2") + "</FebruaryAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostMar.ToString()) &&
                                    report.LowCostMar > 0) {
                                    writer.WriteLine("<MarchAmt>" + report.LowCostMar.ToString("n2") + "</MarchAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostApr.ToString()) &&
                                    report.LowCostApr > 0) {
                                    writer.WriteLine("<AprilAmt>" + report.LowCostApr.ToString("n2") + "</AprilAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostMay.ToString()) &&
                                    report.LowCostMay > 0) {
                                    writer.WriteLine("<MayAmt>" + report.LowCostMay.ToString("n2") + "</MayAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostJun.ToString()) &&
                                    report.LowCostJun > 0) {
                                    writer.WriteLine("<JuneAmt>" + report.LowCostJun.ToString("n2") + "</JuneAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostJul.ToString()) &&
                                    report.LowCostJul > 0) {
                                    writer.WriteLine("<JulyAmt>" + report.LowCostJul.ToString("n2") + "</JulyAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostAug.ToString()) &&
                                    report.LowCostAug > 0) {
                                    writer.WriteLine("<AugustAmt>" + report.LowCostAug.ToString("n2") + "</AugustAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostSep.ToString()) &&
                                    report.LowCostSep > 0) {
                                    writer.WriteLine("<SeptemberAmt>" + report.LowCostSep.ToString("n2") + "</SeptemberAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostOct.ToString()) &&
                                    report.LowCostOct > 0) {
                                    writer.WriteLine("<OctoberAmt>" + report.LowCostOct.ToString("n2") + "</OctoberAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostNov.ToString()) &&
                                    report.LowCostNov > 0) {
                                    writer.WriteLine("<NovemberAmt>" + report.LowCostNov.ToString("n2") + "</NovemberAmt>");
                                }
                                if (!String.IsNullOrEmpty(report.LowCostDec.ToString()) &&
                                    report.LowCostDec > 0) {
                                    writer.WriteLine("<DecemberAmt>" + report.LowCostDec.ToString("n2") + "</DecemberAmt>");
                                }
                                writer.WriteLine("</MonthlyShareOfLowestCostMonthlyPremGrp>"); // end MonthlyShareOfLowestCostMonthlyPremGrp
                            }

                            if (!String.IsNullOrEmpty(report.SafeHarborYr)) {
                                writer.WriteLine("<AnnualSafeHarborCd>" + report.SafeHarborYr + "</AnnualSafeHarborCd>");
                            }

                            // if monthly safe harbor is needed...
                            if (isMonthlySafeHarbor(report)) {
                                writer.WriteLine("<MonthlySafeHarborGrp>");
                                if (!String.IsNullOrEmpty(report.SafeHarborJan)) {
                                    writer.WriteLine("<JanSafeHarborCd>" + report.SafeHarborJan + "</JanSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborFeb)) {
                                    writer.WriteLine("<FebSafeHarborCd>" + report.SafeHarborFeb + "</FebSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborMar)) {
                                    writer.WriteLine("<MarSafeHarborCd>" + report.SafeHarborMar + "</MarSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborApr)) {
                                    writer.WriteLine("<AprSafeHarborCd>" + report.SafeHarborApr + "</AprSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborMay)) {
                                    writer.WriteLine("<MaySafeHarborCd>" + report.SafeHarborMay + "</MaySafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborJun)) {
                                    writer.WriteLine("<JunSafeHarborCd>" + report.SafeHarborJun + "</JunSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborJul)) {
                                    writer.WriteLine("<JulSafeHarborCd>" + report.SafeHarborJul + "</JulSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborAug)) {
                                    writer.WriteLine("<AugSafeHarborCd>" + report.SafeHarborAug + "</AugSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborSep)) {
                                    writer.WriteLine("<SepSafeHarborCd>" + report.SafeHarborSep + "</SepSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborOct)) {
                                    writer.WriteLine("<OctSafeHarborCd>" + report.SafeHarborOct + "</OctSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborNov)) {
                                    writer.WriteLine("<NovSafeHarborCd>" + report.SafeHarborNov + "</NovSafeHarborCd>");
                                }
                                if (!String.IsNullOrEmpty(report.SafeHarborDec)) {
                                    writer.WriteLine("<DecSafeHarborCd>" + report.SafeHarborDec + "</DecSafeHarborCd>");
                                }
                                writer.WriteLine("</MonthlySafeHarborGrp>"); // end MonthlySafeHarborGrp
                            }
                            writer.WriteLine("</EmployeeOfferAndCoverageGrp>"); // end EmployeeOfferAndCoverageGrp

                            // covered individual stuff
                            if (report.CoveredIndividuals != null && report.CoveredIndividuals.Count > 0) {
                                // this line may need to be in the following foreach loop
                                writer.WriteLine("<CoveredIndividualInd>1</CoveredIndividualInd>");
                                // needs to repeat
                                foreach (CoveredIndividual ind in report.CoveredIndividuals) {
                                    writer.WriteLine("<CoveredIndividualGrp>"); // should this be outside the loop?
                                    writer.WriteLine("<CoveredIndividualName>");
                                    writer.WriteLine("<PersonFirstNm>" + ind.Person.FirstName + "</PersonFirstNm>");
                                    if (!String.IsNullOrEmpty(ind.Person.MiddleName)) {
                                        writer.WriteLine("<PersonMiddleNm>" + ind.Person.MiddleName + "</PersonMiddleNm>");
                                    }
                                    writer.WriteLine("<PersonLastNm>" + ind.Person.LastName + "</PersonLastNm>");
                                    if (!String.IsNullOrEmpty(ind.Person.Suffix)) {
                                        writer.WriteLine("<SuffixNm>" + ind.Person.Suffix + "</SuffixNm>");
                                    }
                                    writer.WriteLine("</CoveredIndividualName>"); // end CoveredIndividualName
                                    writer.WriteLine("<PersonNameControlTxt>" + FormatChecker.GetNameControlIndividual(ind.Person.LastName) + "</PersonNameControlTxt>"); // not sure about this
                                    if (!String.IsNullOrEmpty(ind.Person.SSN)) {
                                        writer.WriteLine("<irs:TINRequestTypeCd>INDIVIDUAL_TIN</irs:TINRequestTypeCd>");
                                        writer.WriteLine("<irs:SSN>" + ind.Person.SSN + "</irs:SSN>");
                                    }
                                    if (ind.Person.DOB != null && ind.Person.DOB.Year.ToString() != "1") {
                                        writer.WriteLine("<irs:BirthDt>" + ind.Person.DOB.ToString("yyyy-MM-dd") + "</irs:BirthDt>"); // YYYY-MM-DD
                                    }
                                    writer.WriteLine("<CoveredIndividualAnnualInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredYr).ToString() + "</CoveredIndividualAnnualInd>"); // might need an X
                                    writer.WriteLine("<CoveredIndividualMonthlyIndGrp>");
                                    writer.WriteLine("<JanuaryInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredJan).ToString() + "</JanuaryInd>"); // might need an X for true...
                                    writer.WriteLine("<FebruaryInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredFeb).ToString() + "</FebruaryInd>"); // might need an X for true...
                                    writer.WriteLine("<MarchInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredMar).ToString() + "</MarchInd>"); // might need an X for true...
                                    writer.WriteLine("<AprilInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredApr).ToString() + "</AprilInd>"); // might need an X for true...
                                    writer.WriteLine("<MayInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredMay).ToString() + "</MayInd>"); // might need an X for true...
                                    writer.WriteLine("<JuneInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredJun).ToString() + "</JuneInd>"); // might need an X for true...
                                    writer.WriteLine("<JulyInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredJul).ToString() + "</JulyInd>"); // might need an X for true...
                                    writer.WriteLine("<AugustInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredAug).ToString() + "</AugustInd>"); // might need an X for true...
                                    writer.WriteLine("<SeptemberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredSep).ToString() + "</SeptemberInd>"); // might need an X for true...
                                    writer.WriteLine("<OctoberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredOct).ToString() + "</OctoberInd>"); // might need an X for true...
                                    writer.WriteLine("<NovemberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredNov).ToString() + "</NovemberInd>"); // might need an X for true...
                                    writer.WriteLine("<DecemberInd>" + FormatChecker.BoolToDigitBool(ind.IsCoveredDec).ToString() + "</DecemberInd>"); // might need an X for true...
                                    writer.WriteLine("</CoveredIndividualMonthlyIndGrp>"); // end CoveredIndividualMonthlyInd
                                    writer.WriteLine("</CoveredIndividualGrp>"); // end CoveredIndividualGrp
                                } // end foreach (inner)
                            } // end if

                            writer.WriteLine("</Form1095CUpstreamDetail>"); // end Form1095CUpstreamDetail

                            // track number of records submitted in this transmission
                            rec.Num1095RecordsProcessed = counter;

                            counter++; // increment counter
                        } // end if filesize check...
                    } // end foreach (outer)
                    // reset counter (seems to stay hung up sometimes...)
                    counter = 0;
                } // end if

                writer.WriteLine("</Form1094CUpstreamDetail>"); // end 1094C
                writer.Write("</n1:Form109495CTransmittalUpstream>"); // end doc

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
                        if (line.Contains("<Form1095CAttachedCnt>")) {
                            line = "<Form1095CAttachedCnt>" + rec.Num1095RecordsProcessed.ToString() + "</Form1095CAttachedCnt>";
                        } 
                        // write the lines back to the original file (all lines)
                        if (line.Contains("</n1:Form109495CTransmittalUpstream>")) { // if last line, just use Write()
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
                (!String.IsNullOrEmpty(rep.LowCostDec.ToString()) && rep.LowCostDec > 0) ) {
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