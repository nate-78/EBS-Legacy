using System.Web.Hosting;
using System;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;
using System.Configuration;
using System.IO;

namespace ACAfiling.Controllers {
    public class BuildManifestForUiSubmission {
        public static string BuildSoapEnvelope(Record rec, string checkSum, string byteSize, string replacementReceiptId) {
            // get the necessary sections of the envelope
            string transmissionManifestReqDtl = buildTransmitterManifestReqDtl(rec, checkSum, byteSize, replacementReceiptId);
            string bulkBusinessHeader = buildBulkBusinessHeader(rec);
            // NOTE: THE FOLLOWING FUNCTION MUST BE UPDATED EVERY YEAR!!!!!!!!!!!!!!!!!
            string namespaceVal = getNamespaceVal(rec);

            // build envelope
            string form = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<p:ACAUIBusinessHeader xmlns:p=\"urn:us:gov:treasury:irs:msg:acauibusinessheader\" " + 
                    "xmlns:p1=\"urn:us:gov:treasury:irs:msg:acabusinessheader\" " + // before TY2020, prefix was "acaBusHeader"
                    "xmlns=\"" + namespaceVal + "\" xmlns:irs=\"urn:us:gov:treasury:irs:common\" " + // TODO: xmlns is different for 2016
                    "xmlns:p2=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurityutility-1.0.xsd\" " + // before TY2020, prefix was "wsu"
                    "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                    "xsi:schemaLocation=\"urn:us:gov:treasury:irs:msg:acauibusinessheader IRS-ACAUserInterfaceHeaderMessage.xsd \">" + // TY2020, added "-"
                          bulkBusinessHeader + transmissionManifestReqDtl +
                "</p:ACAUIBusinessHeader>";

            // save envelope to file (logging purposes)
            //string file = build1094UiFileName(rec.FileName);
            string file = rec.Directory + "/" + rec.CurrentSubDirectory + "/manifest.xml";
            File.WriteAllText(file, form);

            return form;
        } // END BUILD SOAP ENVELOPE

        // build the ACATransmitterManifestReqDtl
        private static string buildTransmitterManifestReqDtl(Record rec, string checkSum, string byteSize, string replacementReceiptId) {
            // get necessary variables
            Company ebs = HubDataManager.GetCompany_Admin(1); // ebs's id is 1
            var transmissionTypeCode = ACASvc.TransmissionTypeCdType.O;
            // if correction or replacement, update transmissionTypeCode
            if (rec.SubmissionType == "C") {
                transmissionTypeCode = ACASvc.TransmissionTypeCdType.C;
            } else if (rec.SubmissionType == "R") {
                transmissionTypeCode = ACASvc.TransmissionTypeCdType.R;
            }
            string testFileCode = ConfigurationManager.AppSettings["TestingOrProduction"].ToString(); // values are "T" for testing and "P" for production
            Person ebsContact = new Person();
            ebsContact.FirstName = "Annette";
            ebsContact.LastName = "Wade";
            string contactPhone = "2052914040";
            int ct1094 = 1; // will always be 1, unless we update the system one day
            string binaryFormat = "application/xml";
            string priorYrData = getPriorYrData(rec);
            //if (rec.PriorYrDataInd) {
            //    priorYrData = "1";
            //}

            string manifestDtl = @"<ACATransmitterManifestReqDtl>" +
           @"<PaymentYr>" + rec.TaxYr.ToString() + @"</PaymentYr>" +
           @"<PriorYearDataInd>" + priorYrData + @"</PriorYearDataInd>" +
           @"<irs:EIN>" + ebs.EIN + @"</irs:EIN>" +
           @"<TransmissionTypeCd>" + transmissionTypeCode.ToString() + @"</TransmissionTypeCd>";
            manifestDtl +=
           @"<TestFileCd>" + testFileCode + @"</TestFileCd>";
            if (rec.TaxYr >= 2017 && rec.SubmissionType == "R") {
                manifestDtl += @"<OriginalReceiptId>" + replacementReceiptId + "</OriginalReceiptId>";
            }
            manifestDtl += 
           @"<TransmitterNameGrp>" +
                @"<BusinessNameLine1Txt>" + ebs.Name + @"</BusinessNameLine1Txt>" +
           @"</TransmitterNameGrp>" +
           @"<CompanyInformationGrp>" +
                @"<CompanyNm>" + ebs.Name + @"</CompanyNm>" +
                @"<MailingAddressGrp>" +
                     @"<USAddressGrp>" +
                          @"<AddressLine1Txt>" + ebs.Address.Address1 + @"</AddressLine1Txt>" +
                          @"<irs:CityNm>" + ebs.Address.City + @"</irs:CityNm>" +
                          @"<USStateCd>" + ebs.Address.State + @"</USStateCd>" +
                          @"<irs:USZIPCd>" + ebs.Address.Zip + @"</irs:USZIPCd>" + // if zip code extension... etc
                     @"</USAddressGrp>" +
                @"</MailingAddressGrp>" +
                @"<ContactNameGrp>" +
                     @"<PersonFirstNm>" + ebsContact.FirstName + @"</PersonFirstNm>" +
                     @"<PersonLastNm>" + ebsContact.LastName + @"</PersonLastNm>" +
                @"</ContactNameGrp>" +
                @"<ContactPhoneNum>" + contactPhone + @"</ContactPhoneNum>" +
           @"</CompanyInformationGrp>" +
           @"<VendorInformationGrp>" +
                @"<VendorCd>I</VendorCd>" +
                @"<ContactNameGrp>" +
                     @"<PersonFirstNm>" + ebsContact.FirstName + @"</PersonFirstNm>" +
                     @"<PersonLastNm>" + ebsContact.LastName + @"</PersonLastNm>" +
                @"</ContactNameGrp>" +
                @"<ContactPhoneNum>" + contactPhone + @"</ContactPhoneNum>" +
           @"</VendorInformationGrp>" +
           @"<TotalPayeeRecordCnt>" + rec.Num1095RecordsProcessed.ToString() + @"</TotalPayeeRecordCnt>" + 
           @"<TotalPayerRecordCnt>" + ct1094.ToString() + @"</TotalPayerRecordCnt>" +
           @"<SoftwareId>" + getSoftwareID(rec.TaxYr) + @"</SoftwareId>" +
           @"<FormTypeCd>1094/1095C</FormTypeCd>" +
           @"<irs:BinaryFormatCd>" + binaryFormat + @"</irs:BinaryFormatCd>" + // was urn1
           @"<irs:ChecksumAugmentationNum>" + checkSum + @"</irs:ChecksumAugmentationNum>" + // was urn1
           getAttachmentByteSizeElem(byteSize, rec.TaxYr) + // was urn1
           @"<DocumentSystemFileNm>" + rec.IrsFileName + @"</DocumentSystemFileNm>" +
      @"</ACATransmitterManifestReqDtl>";

            return manifestDtl;
        }


        private static string getSoftwareID(int taxyr)
        {
            var swid = "24A0020888";
            if (taxyr == 2020) {
                swid = "20A0011905";
            } else if (taxyr == 2019)
            {
                swid = "19A0010315";
            } else if (taxyr == 2018)
            {
                swid = "18A0008321";
            } else if (taxyr == 2017)
            {
                swid = "17A0005785";
            } else if (taxyr == 2016)
            {
                swid = "16A0003289";
            } else if (taxyr == 2015)
            {
                swid = "15A0000133";
            } else if (taxyr == 2021)
            {
                swid = "21A0014005";
            } else if (taxyr == 2022)
            {
                swid = "22A0015479";
            } else if (taxyr == 2023)
            {
                swid = "23A0018266";
            }

            return swid;
        }


        private static string buildBulkBusinessHeader(Record rec) { // done
            rec.UniqueId = ReportBuilder.CreateUniqueId(rec.TCC);
            string text = @"<p1:ACABusinessHeader>" +
           @"<UniqueTransmissionId>" + rec.UniqueId + @"</UniqueTransmissionId>" +
           @"<irs:Timestamp>" + TransmissionController.FormatTimestampForBusinessHeader(rec.TimeStampGMT) + @"Z</irs:Timestamp>" + 
        @"</p1:ACABusinessHeader>";
            return text;
        }


        private static string build1094UiFileName(string fileName) {
            string uiFileName = "";
            if (fileName.Contains(".x")) {
                string[] arr = fileName.Split('.');
                uiFileName = arr[0];
                uiFileName += "_SoapUi1094.xml";
            }
            return uiFileName;
        }


        // will need to update this each year
        private static string getNamespaceVal(Record rec) {
            string val = "urn:us:gov:treasury:irs:ext:aca:air:7.0"; // 2015 tax year value
            //if (rec.TaxYr == 2016) {
            //    val = "urn:us:gov:treasury:irs:ext:aca:air:ty16";
            //} else if (rec.TaxYr == 2017) {
            //    val = "urn:us:gov:treasury:irs:ext:aca:air:ty17";
            //} else if (rec.TaxYr == 2018) {
            //    val = "urn:us:gov:treasury:irs:ext:aca:air:ty18";
            //}

            if (rec.TaxYr > 2015)
            {
                val = "urn:us:gov:treasury:irs:ext:aca:air:ty" + rec.TaxYr.ToString().Substring(2);
            }

            // for now, just return the most recent version, even if submitting older files (4/28/19)
            // changing back to using the last 2 years (1/22/20)
            // 4/22/20 -- the manifest should always return the current tax year (which is the previous year)
            val = "urn:us:gov:treasury:irs:ext:aca:air:ty" + DateTime.Now.AddYears(-1).Year.ToString().Substring(2); // this gets last 2 digits of the year 

            return val;
        }


        private static string getPriorYrData(Record rec)
        {
            var currTaxYr = DateTime.Now.Year - 1;
            if (rec.TaxYr < currTaxYr)
            {
                return "1";
            } else
            {
                return "0";
            }
        }


        private static string getAttachmentByteSizeElem(string byteSize, int taxyr)
        {
            var prefix = "";
            //if (taxyr < 2019)
            //{
            //    prefix = "irs:";
            //}

            return @"<" + prefix + "AttachmentByteSizeNum>" + byteSize + @"</" + prefix + "AttachmentByteSizeNum>";
        }
    }
}