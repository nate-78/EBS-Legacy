using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;
using System.Configuration;
using System.IO;
using System.Text;

namespace ACAfiling_Web.Controllers {
    // use this class to manually construct SOAP envelope for submission (still can't believe we have to do this)
    public class SoapEnvelopeBuilder {

        public static string BuildSoapEnvelope(Record rec, string checkSum, string byteSize, string attachmentFileName) {
            // get the necessary sections of the envelope
            string transmissionManifestReqDtl = buildTransmitterManifestReqDtl(rec, checkSum, byteSize);
            string bulkBusinessHeader = buildBulkBusinessHeader(rec);
            string timestampElement = buildTimestampeElement(rec);

            // build envelope
            string envelope = @"<?xml version='1.0' encoding='utf-8'?>" +
@"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:urn='urn:us:gov:treasury:irs:ext:aca:air:7.0' xmlns:urn1='urn:us:gov:treasury:irs:common' xmlns:urn2='urn:us:gov:treasury:irs:msg:acabusinessheader' xmlns:urn3='urn:us:gov:treasury:irs:msg:irsacabulkrequesttransmitter'>" +
    @"<soapenv:Header xmlns:wsa='http://www.w3.org/2005/08/addressing'>" +
          @"<wsse:Security xmlns:wsse='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>" +
               timestampElement +
          @"</wsse:Security>" +
          transmissionManifestReqDtl +
          bulkBusinessHeader + 
          @"<wsa:Action>BulkRequestTransmitterService</wsa:Action>" +
    @"</soapenv:Header>" +
    @"<soapenv:Body>" +
          @"<urn3:ACABulkRequestTransmitter version='1.0'>" +
            @"<urn1:BulkExchangeFile>" +
                @"<inc:Include href='cid:1095CTransBaseAttachment.xml' xmlns:inc='http://www.w3.org/2004/08/xop/include'/>" +
            @"</urn1:BulkExchangeFile>" +
          @"</urn3:ACABulkRequestTransmitter>" +
    @"</soapenv:Body>" +
@"</soapenv:Envelope>";

            // comment this line out when local testing
            envelope = X509CertSecurityManager.GetXmlSignature(envelope, "#manifest", "#timestamp", "#busHeader");

            // save envelope to file (logging purposes)
            //string file = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["LogLocation"]) + "/envelope.xml";
            string file = rec.Directory + "/manifest.xml";
            File.WriteAllText(file, envelope);

            return envelope;
        } // END BUILD SOAP ENVELOPE

        // build the ACATransmitterManifestReqDtl
        private static string buildTransmitterManifestReqDtl(Record rec, string checkSum, string byteSize) {
            // get necessary variables
            Company ebs = HubDataManager.GetCompany_Admin(1); // ebs's id is 1
            var transmissionTypeCode = ACASvc.TransmissionTypeCdType.O;
            string testFileCode = "T"; // values are "T" for testing and "P" for production
            Person ebsContact = new Person();
            ebsContact.FirstName = "Annette";
            ebsContact.LastName = "Wade";
            string contactPhone = "2052914040";
            string vendorCode = "I"; // "I" for "in-house"
            int ct1094 = 1; // will always be 1, unless we update the system one day
            string binaryFormat = "application/xml";
            string priorYrData = "0";
            if (rec.PriorYrDataInd) {
                priorYrData = "1";
            }

            string manifestDtl = @"<urn:ACATransmitterManifestReqDtl wsu:Id='manifest' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>" +
           @"<urn:PaymentYr>" + rec.TaxYr.ToString() + @"</urn:PaymentYr>" +
           @"<urn:PriorYearDataInd>" + priorYrData + @"</urn:PriorYearDataInd>" +
           @"<urn1:EIN>" + ebs.EIN + @"</urn1:EIN>" +     
           @"<urn:TransmissionTypeCd>" + transmissionTypeCode.ToString() + @"</urn:TransmissionTypeCd>" +
           @"<urn:TestFileCd>" + testFileCode + @"</urn:TestFileCd>" +                
           @"<urn:TransmitterNameGrp>" +
                @"<urn:BusinessNameLine1Txt>" + ebs.Name + @"</urn:BusinessNameLine1Txt>" +
           @"</urn:TransmitterNameGrp>" +
           @"<urn:CompanyInformationGrp>" +
                @"<urn:CompanyNm>" + ebs.Name + @"</urn:CompanyNm>" +
                @"<urn:MailingAddressGrp>" +
                     @"<urn:USAddressGrp>" +   
                          @"<urn:AddressLine1Txt>" + ebs.Address.Address1 + @"</urn:AddressLine1Txt>" +
                          @"<urn1:CityNm>" + ebs.Address.City + @"</urn1:CityNm>" +
                          @"<urn:USStateCd>" + ebs.Address.State + @"</urn:USStateCd>" +
                          @"<urn1:USZIPCd>" + ebs.Address.Zip + @"</urn1:USZIPCd>" +
                     @"</urn:USAddressGrp>" +
                @"</urn:MailingAddressGrp>" +
                @"<urn:ContactNameGrp>" +
                     @"<urn:PersonFirstNm>" + ebsContact.FirstName + @"</urn:PersonFirstNm>" +
                     @"<urn:PersonLastNm>" + ebsContact.LastName + @"</urn:PersonLastNm>" +
                @"</urn:ContactNameGrp>" +
                @"<urn:ContactPhoneNum>" + contactPhone + @"</urn:ContactPhoneNum>" +
           @"</urn:CompanyInformationGrp>" +
           @"<urn:VendorInformationGrp>" +
                @"<urn:VendorCd>" + vendorCode + @"</urn:VendorCd>" +
                @"<urn:ContactNameGrp>" +
                     @"<urn:PersonFirstNm>" + ebsContact.FirstName + @"</urn:PersonFirstNm>" +
                     @"<urn:PersonLastNm>" + ebsContact.LastName + @"</urn:PersonLastNm>" +
                @"</urn:ContactNameGrp>" + 
                @"<urn:ContactPhoneNum>" + contactPhone + @"</urn:ContactPhoneNum>" +
           @"</urn:VendorInformationGrp>" +
           @"<urn:TotalPayeeRecordCnt>" + rec.IndividualReports.Count.ToString() + @"</urn:TotalPayeeRecordCnt>" +
           @"<urn:TotalPayerRecordCnt>" + ct1094.ToString() + @"</urn:TotalPayerRecordCnt>" +
           @"<urn:SoftwareId>" + ConfigurationManager.AppSettings["SoftwareID"].ToString() + @"</urn:SoftwareId>" +
           @"<urn:FormTypeCd>1094/1095C</urn:FormTypeCd>" +
           @"<urn1:BinaryFormatCd>" + binaryFormat + @"</urn1:BinaryFormatCd>" + // was urn1
           @"<urn1:ChecksumAugmentationNum>" + checkSum + @"</urn1:ChecksumAugmentationNum>" + // was urn1
           @"<urn1:AttachmentByteSizeNum>" + byteSize + @"</urn1:AttachmentByteSizeNum>" + // was urn1
           @"<urn:DocumentSystemFileNm>" + rec.IrsFileName + @"</urn:DocumentSystemFileNm>" +  
      @"</urn:ACATransmitterManifestReqDtl>";

            return manifestDtl;
        }


        private static string buildBulkBusinessHeader(Record rec) {
            string text = @"<urn2:ACABusinessHeader wsu:Id='busHeader' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>" +    
           @"<urn:UniqueTransmissionId>" + rec.UniqueId + @"</urn:UniqueTransmissionId>" +
           @"<urn1:Timestamp>" + TransmissionController.FormatTimestampForBusinessHeader(rec.TimeStampGMT) + @"Z</urn1:Timestamp>" + // was urn1
        @"</urn2:ACABusinessHeader>";
            return text;
        }


        private static string buildTimestampeElement(Record rec) {
            // get the timestamp element
            var timestamp = TransmissionController.GetSecurityTimestamp(rec.TimeStampGMT);

            string text = @"<wsu:Timestamp Id='timestamp'>" +
                @"<wsu:Created>" + timestamp.Created.Value + @"</wsu:Created>" +
                @"<wsu:Expires>" + timestamp.Expires.Value + @"</wsu:Expires>" +
           @"</wsu:Timestamp>";

            return text;
        }

    } // END CLASS
} // END NAMESPACE