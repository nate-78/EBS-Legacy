using System.Web.Hosting;
using System;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;
using System.Configuration;
using System.IO;

namespace ACAfiling_Web.Controllers {
    public class BuildManifestForUiSubmission {
        public static string BuildSoapEnvelope(Record rec, string checkSum, string byteSize, string replacementReceiptId) {
            // get the necessary sections of the envelope
            string transmissionManifestReqDtl = buildTransmitterManifestReqDtl(rec, checkSum, byteSize);
            string bulkBusinessHeader = buildBulkBusinessHeader(rec);

            // build envelope
            string form = @"<?xml version='1.0' encoding='utf-8'?>" +
                @"<p:ACAUIBusinessHeader xmlns:p='urn:us:gov:treasury:irs:msg:acauibusinessheader' " + 
                    @"xmlns:acaBusHeader='urn:us:gov:treasury:irs:msg:acabusinessheader' " +
                    @"xmlns='urn:us:gov:treasury:irs:ext:aca:air:7.0' xmlns:irs='urn:us:gov:treasury:irs:common' " +
                    @"xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurityutility-1.0.xsd' " + 
                    @"xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                    @"xsi:schemaLocation='urn:us:gov:treasury:irs:msg:acauibusinessheader IRSACAUserInterfaceHeaderMessage.xsd'>" + // done
                          bulkBusinessHeader + transmissionManifestReqDtl +
                @"</p:ACAUIBusinessHeader>";

            // save envelope to file (logging purposes)
            //string file = build1094UiFileName(rec.FileName);
            string file = rec.Directory + "/" + rec.CurrentSubDirectory + "/manifest.xml";
            File.WriteAllText(file, form);

            return form;
        } // END BUILD SOAP ENVELOPE

        // build the ACATransmitterManifestReqDtl
        private static string buildTransmitterManifestReqDtl(Record rec, string checkSum, string byteSize) {
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
            string priorYrData = "0";
            if (rec.PriorYrDataInd) {
                priorYrData = "1";
            }

            string manifestDtl = @"<ACATransmitterManifestReqDtl>" +
           @"<PaymentYr>" + rec.TaxYr.ToString() + @"</PaymentYr>" +
           @"<PriorYearDataInd>" + priorYrData + @"</PriorYearDataInd>" +
           @"<irs:EIN>" + ebs.EIN + @"</irs:EIN>" +
           @"<TransmissionTypeCd>" + transmissionTypeCode.ToString() + @"</TransmissionTypeCd>" +
           @"<TestFileCd>" + testFileCode + @"</TestFileCd>" + // need conditional "original transmission" stuff here
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
           @"<SoftwareId>" + ConfigurationManager.AppSettings["SoftwareID"].ToString() + @"</SoftwareId>" +
           @"<FormTypeCd>1094/1095C</FormTypeCd>" +
           @"<irs:BinaryFormatCd>" + binaryFormat + @"</irs:BinaryFormatCd>" + // was urn1
           @"<irs:ChecksumAugmentationNum>" + checkSum + @"</irs:ChecksumAugmentationNum>" + // was urn1
           @"<irs:AttachmentByteSizeNum>" + byteSize + @"</irs:AttachmentByteSizeNum>" + // was urn1
           @"<DocumentSystemFileNm>" + rec.IrsFileName + @"</DocumentSystemFileNm>" +
      @"</ACATransmitterManifestReqDtl>";

            return manifestDtl;
        }


        private static string buildBulkBusinessHeader(Record rec) { // done
            rec.UniqueId = ReportBuilder.CreateUniqueId(rec.TCC);
            string text = @"<acaBusHeader:ACABusinessHeader>" +
           @"<UniqueTransmissionId>" + rec.UniqueId + @"</UniqueTransmissionId>" +
           @"<irs:Timestamp>" + TransmissionController.FormatTimestampForBusinessHeader(rec.TimeStampGMT) + @"Z</irs:Timestamp>" + 
        @"</acaBusHeader:ACABusinessHeader>";
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
    }
}