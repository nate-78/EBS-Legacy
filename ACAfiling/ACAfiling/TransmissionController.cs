using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EbsClassPkg.Models;
using EbsClassPkg.Controllers;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;
using System.Configuration;
using Microsoft.Web.Services2;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Microsoft.Web.Services2.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;

namespace ACAfiling.Controllers {

    // use this class to interact with the SOAP service
    public class TransmissionController {
        // create SOAP client for use within the following functions
        private static ACASvc.BulkRequestTransmitterPortTypeClient soapClient = 
            new ACASvc.BulkRequestTransmitterPortTypeClient("ACASvc.BulkRequestTransmitterPortType");

        // SOAP client to use for status update functionality
        private static ACAStatus.ACATransmitterStatusReqPortTypeClient statusUpdateClient =
            new ACAStatus.ACATransmitterStatusReqPortTypeClient();

        // NEW TRANSMISSION FUNCTION
        public static List<CustomResponse> SubmitAcaRecord(Record rec, CorrectionInfo correctionInfo, User currUser) {

            // create CustomResponse List to hold all the submission responses (will usually just be one)
            List<CustomResponse> responses = new List<CustomResponse>();

            int transmissionCount = 0;

            while (rec.HasRecordsToSubmit) { // as long as there are records to submit, keep doing it. Will usually just be one submission, though
                // now that we're in the loop, reset  HasRecordsToSubmit to 'false'.
                // that way, if we're able to build out all the data into one transmission, we'll do it.
                // if not, the XmlController class will change HasRecordsToSubmit to 'true' if it notices
                // the file size is getting too close to the threshold
                rec.HasRecordsToSubmit = false;

                // add the xml file name to the Record object
                transmissionCount++;
                rec.CurrentSubDirectory = "submission" + transmissionCount.ToString();
                rec.XmlFilePath = rec.Directory + "/" + rec.CurrentSubDirectory + "/" + rec.IrsFileName;

                // build the xml data file (1095)
                byte[] blob;
                if (rec.TaxYr == 2015) {
                    blob = XmlController.BuildXmlDoc(rec, correctionInfo);
                } else if (rec.TaxYr == 2016) {
                    blob = Xml2016Controller.BuildXmlDoc(rec, correctionInfo);
                } else if (rec.TaxYr == 2017) {
                    blob = Xml2017Controller.BuildXmlDoc(rec, correctionInfo);
                } else
                {
                    blob = Xml2018Controller.BuildXmlDoc(rec, correctionInfo); // NOTE: the 2018 controller has been set to work with 2018 and beyond, until they make changes to the XML
                }
                // do I still need a 2017 controller?


                // get checksum on the form data file
                string checkSum = X509CertSecurityManager.ComputeSHA256CheckSum(blob);


                // SOAP A2A METHOD
                //// build the SOAP envelope
                //string envelope = SoapEnvelopeBuilder.BuildSoapEnvelope(rec, checkSum, rec.ByteSz.ToString(), "attachmentFileName");
                //// submit data
                //var response = SoapHttpClientPost(envelope, blob);

                // MANUAL UI METHOD
                bool isProduction = true;
                if (ConfigurationManager.AppSettings["TestingOrProduction"].ToString() == "T") {
                    isProduction = false;
                }

                string envelopeUi = BuildManifestForUiSubmission.BuildSoapEnvelope(rec, checkSum, blob.Length.ToString(), correctionInfo.BadSubmissionReceiptId);
                var updateAttempts = 0;
                StatusUpdate update = new StatusUpdate();

                // IF HAVING TIMEOUT ISSUES, CAN COMMENT OUT THIS BLOCK AND UNCOMMENT LINES 110-111 TO BUILD THE FILES AND CAPTURE DATA IN DB -- SUBMIT FILES MANUALLY
                //while (update == null || String.IsNullOrEmpty(update.ReceiptId)) {
                //    update = UiNavigation.NavigateUI(rec.Directory + "/" + rec.CurrentSubDirectory + "/manifest.xml", rec.XmlFilePath,
                //        isProduction, true, "", ""); // receipt ID will be empty on a submission
                //                                     //update = buildMockSuccessUpdate(); // for local testing only!

                //    updateAttempts++;
                //    // if we've tried three times, or this is the first transmission (whether it only takes one transmission, or it will take several)
                //    if (updateAttempts == 3 || transmissionCount == 1) { // || transmissionCount == 1
                //        // break out of loop
                //        break;
                //    }
                //}

                // insert the submission's records into db
                // even though the submission itself hasn't been added to the db yet, we're still okay.
                // these records refer to the submission via the UTID. So if the submission is stored, they'll match
                AcaDataManager.InsertIndividualRecords(rec.IndividualReports, rec.UniqueId);

                var response = new CustomResponse();

                // for testing or manual process  ////////////////////////
                update.StatusType = "Processing";
                // IMPORTANT: "TempID_" is not arbitrary. The other applications know to look for that
                update.ReceiptId = "TempID_" + rec.UniqueId;
                // store manifest and form data file paths to response object so they can be stored in the Vault
                response.ManifestFilePath = rec.Directory + "/" + rec.CurrentSubDirectory + "/manifest.xml";
                response.FormDataFilePath = rec.XmlFilePath;
                response.FormDataFileName = rec.IrsFileName;
                //////////////////////////////////////////////////////////

                // if successful...
                if (update.StatusType != "Error" && !String.IsNullOrEmpty(update.ReceiptId)) {
                    response.Successful = true;
                    response.StatusMessage = "This submission contained " + rec.Num1095RecordsProcessed.ToString() + " 1095-C records.";

                    // insert submission into db (also inserts an update with "Processing" as the status)
                    try {
                        AcaDataManager.InsertSubmission(rec, correctionInfo, currUser);
                    } catch (Exception ex) {
                        CustomError.AddError("An error occurred while trying to add this Submission to the database. PLEASE SAVE THE " +
                            "RECEIPT ID that was generated by the IRS. Then notify technical support that you experienced this issue. " +
                            "Please provide them with the following error report: " +
                            "Error Message: " + ex.Message + ". Stack Trace: " + ex.StackTrace);
                    }
                
                    
                } else { // error...
                    response.Successful = false;
                    if (String.IsNullOrEmpty(update.ReceiptId)) {
                        // no receipt id provided by UI
                        response.StatusMessage = "The submission containing Records " + rec.RecordStartedWith.ToString() +
                            " through " + rec.Num1095RecordsProcessed.ToString() + " were not successfully received by the IRS despite " +
                            updateAttempts.ToString() + " attempts.";
                    }
                }
                response.StatusCode = update.StatusCode;
                //response.StatusReason = update.StatusMessage; // too repetitive
                response.ReceiptId = update.ReceiptId;
                response.TimeStamp = update.Timestamp;
                response.StatusMessage += update.StatusMessage;

                // store receipt id in db
                AcaDataManager.AddReceiptIdToSubmission(rec.UniqueId, update.ReceiptId);

                responses.Add(response);
            }

            return responses;
        }

        // OLD SCHOOL HTTP POST SOAP SUBMISSION
        private static CustomResponse SoapHttpClientPost(string envelope, byte[] blob) {
            var soapUri = new Uri(ConfigurationManager.AppSettings["SoapUri"].ToString());
            using (HttpClient client = new HttpClient()) {
                // add headers to client
                client.DefaultRequestHeaders.Clear();
                //client.DefaultRequestHeaders.Add("Content-Encoding", "gzip"); // doesn't seem to work here. might need to add a different way later?
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.Add("SOAPAction", "\"BulkRequestTransmitter\"");
                client.DefaultRequestHeaders.Add("MIME-Version", "1.0");
                //client.DefaultRequestHeaders.Add("Connection", "Keep-Alive"); // don't need it anywhere. app is adding it automagically

                using (var content = new MultipartFormDataContent()) {
                    // add envelope to content
                    content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "\"application/xop+xml\""));
                    content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("start", "\"<rootpart>\""));
                    content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("start-info", "\"text/xml\""));
                    
                    var contentPart = new StringContent(envelope);
                    contentPart.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                    contentPart.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", "\"application/xop+xml\""));
                    contentPart.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("charset", "\"UTF-8\""));
                    contentPart.Headers.Add("Content-ID", "<rootpart>");
                    // leave the following line commented. CRG says it should be a header, but adding it 
                    // removes the DigestValues and SignatureValue from the transmission
                    //contentPart.Headers.Add("Content-Transfer-Encoding", "8bit");
                    // add this section to the content
                    content.Add(contentPart);

                    // add formdata to content
                    var formDataContentPart = new StreamContent(new MemoryStream(blob));

                    formDataContentPart.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                    formDataContentPart.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("charset", "\"us-ascii\""));
                    formDataContentPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    formDataContentPart.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("name", 
                        "\"1095CTransBaseAttachment.xml\""));
                    
                    formDataContentPart.Headers.Add("Content-ID", "<1095CTransBaseAttachment.xml>");
                    formDataContentPart.Headers.Add("Content-Transfer-Encoding", "7bit");
                    //content.Add(formDataContentPart, "1095CTransBaseAttachment.xml", "1095CTransBaseAttachment.xml");
                    content.Add(formDataContentPart);

                    // compress via gzip when sending
                    // '.Result' forces an async method to run synchronously
                    using (var message = client.PostAsync(soapUri, new CompressedContent(content, "gzip")).Result) {
                        // convert message to ACAResponse object
                        var response = new CustomResponse();
                        response.StatusCode = message.StatusCode.ToString();
                        response.StatusReason = message.ReasonPhrase;
                        response.StatusMessage = message.Content.ReadAsStringAsync().Result;
                        response.Successful = message.IsSuccessStatusCode;
                        
                        return response;
                    } // disposes 'message'
                } // disposes 'content'
            } // disposes 'client'
        }


        // GET STATUS UPDATE ON SPECIFIC SUBMISSION via UI (screen scraping)
        public static StatusUpdate GetStatusUpdateUI(string receiptId, string defaultDirectoryPath) {
            bool isProduction = false;
            if (ConfigurationManager.AppSettings["TestingOrProduction"].ToString() == "P") {
                isProduction = true;
            }
            StatusUpdate update = UiNavigation.NavigateUI("", "", isProduction, false, receiptId, defaultDirectoryPath);

            return update;
        }


        // GET STATUS UPDATE ON SPECIFIC SUBMISSION via SOAP
        public static StatusUpdate GetStatusUpdate(string receiptId) {
            StatusUpdate update = new StatusUpdate();
            //var response = new ACAStatus.ACABulkRequestTransmitterStatusDetailResponseType();

            //// using SOAP transmission, the SOAP envelope expects a header with a security element, an ACABusinessHeader element,
            //// and an Action element
            //// the body contains the ACABulkRequestTransmitterStatusDetailRequest element, which holds the ReceiptId

            //// parameters to pass in the request
            //// pg 46: https://www.irs.gov/pub/info_return/AIR%20Submission%20Composition%20and%20Reference%20Guide%20TY2015_Revised%20v4.2%20February%2026,%202016.pdf
            //var acaSecHeader = new ACAStatus.TransmitterACASecurityHeaderType(); // for userId, but not needed in A2A
            //var busHeader = new ACAStatus.ACABulkBusinessHeaderRequestType(); // UTID and timestamp
            //var statusDtlReq = new ACAStatus.ACABulkRequestTransmitterStatusDetailRequestType(); // receiptId
            //// also needs secHeader, which is created later in this method

            //// assign receipt id
            //// this is the body of the SOAP msg
            //var body = new ACAStatus.ACABulkReqTrnsmtStsReqGrpDtlType();
            //body.ReceiptId = receiptId;
            //statusDtlReq.ACABulkReqTrnsmtStsReqGrpDtl = body;
            //statusDtlReq.Id = "requestDetail";

            //// build bulk business header, which goes in the SOAP header
            //busHeader.Timestamp = FormatTimestamp(DateTime.UtcNow.ToString());
            //busHeader.Id = "bulkBusHeader";
            //busHeader.UniqueTransmissionId = ReportBuilder.CreateUniqueId();

            //// security stuff, like signature
            //var secHeader = StatusRequestController.GetStatusSecHeader(busHeader, statusDtlReq);

            //// add endpointbehavior to client so we can examine the outgoing SOAP message
            //statusUpdateClient.Endpoint.EndpointBehaviors.Add(new EndpointBehavior());
            //using (var context = new OperationContextScope(statusUpdateClient.InnerChannel)) {
            //    response = statusUpdateClient.GetACATransmitterStatusReqOperation(acaSecHeader, secHeader, ref busHeader, statusDtlReq);
            //}

            //// if response is not null, populate the update object
            //if (response != null) {
            //    // convert ACA response to a status update
            //    update.StatusCode = response.ACABulkRequestTransmitterResponse.TransmissionStatusCd.ToString();
            //    //update.ReceiptId = response.ACABulkRequestTransmitterResponse.ReceiptId;
            //    update.StatusType = response.ACABulkRequestTransmitterResponse.TransmissionStatusCd.ToString();
            //} else { // otherwise, populate it as an error
            //    update.StatusCode = "Error";
            //    update.StatusType = "Error";
            //    update.StatusMessage = "No 'response' was returned by the IRS.";
            //}
            //// set timestamp and receipt id
            //update.ReceiptId = receiptId;
            //update.Timestamp = DateTime.UtcNow;

            return update;
        }


        private static ACAStatus.SecurityHeaderType getStatusSecHeader(ACAStatus.ACABulkBusinessHeaderRequestType busHeader, 
            ACAStatus.ACABulkRequestTransmitterStatusDetailRequestType statusDtlReq) {

            var secHeader = new ACAStatus.SecurityHeaderType();

            // build the timestamp
            var stamp = new ACAStatus.TimestampType();
            var attributedDateTime = new ACAStatus.AttributedDateTime();
            var dt = DateTime.UtcNow.ToString();
            attributedDateTime.Value = String.Format("{0:d/M/yyyy HH:mm:ss}", dt);
            stamp.Created = attributedDateTime;
            // need any of the other attributes?
            secHeader.Timestamp = stamp;

            // build the signature
            //var sig = getXmlSignature(busHeader, statusDtlReq, stamp);

            return secHeader;
        }


        // build status update and record it in db
        private static StatusUpdate buildStatusUpdate(ACASvc.ACABulkRequestTransmitterResponseType response, string utid) {
            StatusUpdate update = new StatusUpdate();
            update.StatusCode = response.TransmissionStatusCd.ToString();
            update.Timestamp = DateTime.Now;
            update.StatusMessage = "Response from Submission attempt. Unique Trans ID: " + utid;
            update.ReceiptId = response.ReceiptId;
            if (response.ErrorMessageDetail != null) {
                update.StatusType = "Error";
            } else {
                update.StatusType = "Processing";
                update.StatusMessage += "<br />Receipt ID: " + response.ReceiptId;
            }

            AcaDataManager.InsertStatusUpdate(update, utid);

            return update;
        }


        // format the timestamp
        public static DateTime FormatTimestamp(string timestamp) {
            var ts = Convert.ToDateTime(timestamp);
            // convert it to the requested format
            string timeString = String.Format("{0:yyyy-MM-dd HH:mm:ss}", ts);
            //timeString = timeString.Replace(" ", "T");
            //timeString = timeString + "Z";
            ts = Convert.ToDateTime(timeString);

            return ts;
        }

        public static string FormatTimestampForBusinessHeader(string timestamp) {
            var ts = Convert.ToDateTime(timestamp);
            // convert it to the requested format
            string timeString = String.Format("{0:yyyy-MM-dd HH:mm:ss}", ts);
            timeString = timeString.Replace(" ", "T");

            return timeString.Trim();
        }

        public static string FormatTimeStampString(string timestamp) {
            var ts = Convert.ToDateTime(timestamp);
            // convert it to the requested format
            string timeString = String.Format("{0:yyyy-MM-dd HH:mm:ss}", ts);
            timeString = timeString.Replace(" ", "T");
            timeString = timeString + "Z";
            return timeString;
        }

        public static string FormatTimeStampForSecurityString(string timestamp) {
            var ts = Convert.ToDateTime(timestamp);
            // convert it to the requested format
            string timeString = String.Format("{0:yyyy-MM-dd HH:mm:ss.FFF}", ts);
            if (timeString.Substring(timeString.Length - 3, 1) == ":") {
                timeString += ".000";
            }
            timeString = timeString.Replace(" ", "T");
            timeString = timeString + "Z";
            return timeString;
        }

        //public static string FormatTimeStampForFileNameString(string timestamp) {
        //    var ts = Convert.ToDateTime(timestamp);
        //    // convert it to the requested format
        //    string timeString = String.Format("{0:yyyyMMdd HHmmss}", ts);
        //    timeString = timeString.Replace(" ", "T");
        //    timeString = timeString + "000Z";

        //    return timeString;
        //}

        public static string getExpire(string timestamp) {
            var ts = Convert.ToDateTime(timestamp);
            ts = ts.AddMinutes(10);
            return ts.ToString();
        }

        public static ACASvc.TimestampType GetSecurityTimestamp(string timestamp) {
            var timestampy = new ACASvc.TimestampType();
            var timestampyString = new ACASvc.AttributedDateTime();
            timestampyString.Value = FormatTimeStampForSecurityString(timestamp);
            timestampy.Created = timestampyString;
            var expireString = new ACASvc.AttributedDateTime();
            expireString.Value = FormatTimeStampForSecurityString(getExpire(timestamp));
            timestampy.Expires = expireString;
            timestampy.Id = "timestamp"; 

            return timestampy;
        }


        // build a successful mock update for testing
        private static StatusUpdate buildMockSuccessUpdate() {
            StatusUpdate update = new StatusUpdate();
            update.ReceiptId = "1095C-TestReceiptID";
            update.StatusCode = "Processing";
            update.StatusType = "Processing";
            update.StatusMessage = "This is just a fake Status Update to assist with testing. Please ignore.";
            update.Timestamp = DateTime.Now;

            return update;
        }


        // sign XML elements
        private static ACASvc.SignatureType getXmlSignature(ACASvc.ACABulkBusinessHeaderRequestType bulkBusHeader, 
            ACASvc.ACATrnsmtManifestReqDtlType dtlType, ACASvc.TimestampType timestamp) {

            var sig = new ACASvc.SignatureType();

            // sign timestamp
            var canonType = new ACASvc.CanonicalizationMethodType();
            canonType.Algorithm = "http://www.w3.org/2001/10/xml-excc14n#WithComments";
            var digest = new ACASvc.DigestMethodType();
            digest.Algorithm = "http://www.w3.org/2000/09/xmldsig#sha1";
            var transformAlg = new ACASvc.TransformType();
            transformAlg.Algorithm = "http://www.w3.org/2001/10/xml-exc-c14n";
            // CRG shows an element called InclusiveNamespaces inside the Transforms element.  Not sure how to create that

            var referenceTimeStamp = buildSigRef(digest, "#" + timestamp.Id, transformAlg, timestamp);
            var referenceBusHeader = buildSigRef(digest, "#" + bulkBusHeader.Id, transformAlg, bulkBusHeader); 
            var referenceManifestDtl = buildSigRef(digest, "#" + dtlType.Id, transformAlg, dtlType); 

            var refs = new List<ACASvc.ReferenceType>();
            refs.Add(referenceTimeStamp);
            refs.Add(referenceBusHeader);
            refs.Add(referenceManifestDtl);

            var signedInfo1 = new ACASvc.SignedInfoType();
            signedInfo1.CanonicalizationMethod = canonType;
            signedInfo1.Reference = refs.ToArray();
            var sigMethodType = new ACASvc.SignatureMethodType();
            sigMethodType.Algorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            signedInfo1.SignatureMethod = sigMethodType;
            sig.SignedInfo = signedInfo1;

            // keyinfo
            sig.KeyInfo = getKeyInfo(); // this builds KeyInfo using RSA key, etc
            //sig.KeyInfo = getKeyInfo_New(); // this builds KeyInfo with KeyIdentifier (preferred by IRS)

            var sigval = new ACASvc.SignatureValueType();
            sigval.Value = ObjectToByteArray(signedInfo1);
            sig.SignatureValue = sigval;

            return sig;
        }

        private static ACASvc.ReferenceType buildSigRef(ACASvc.DigestMethodType digest, string uri, ACASvc.TransformType transform, Object obj) {
            var reference = new ACASvc.ReferenceType();
            reference.DigestMethod = digest;
            reference.URI = uri;

            var refTransforms = new List<ACASvc.TransformType>();
            refTransforms.Add(transform);
            reference.Transforms = refTransforms.ToArray();

            SHA1 sha = new SHA1CryptoServiceProvider();
            reference.DigestValue = sha.ComputeHash(ObjectToByteArray(obj));

            return reference;
        }


        private static ACASvc.KeyInfoType getKeyInfo() {
            //var key = new RSACryptoServiceProvider();
            //KeyInfo keyInfoVanilla = new KeyInfo();
            //keyInfoVanilla.AddClause(new RSAKeyValue((RSA)key)); // vanilla .NET way

            ACASvc.KeyInfoType keyInfo = new ACASvc.KeyInfoType();
            //var choiceList = new List<ACASvc.ItemsChoiceType2>();
            var choiceList = new ACASvc.ItemsChoiceType2[1];
            choiceList[0] = ACASvc.ItemsChoiceType2.KeyValue;
            //choiceList.Add(ACASvc.ItemsChoiceType2.X509Data);
            //keyInfo.ItemsElementName = choiceList.ToArray();
            keyInfo.ItemsElementName = choiceList;
            //keyInfo.Items = addCert();

            // not sure what to do with this stuff...
            ACASvc.KeyValueType keyval = new ACASvc.KeyValueType();
            var cert = getX509Cert();

            RSACryptoServiceProvider key = cert.PublicKey.Key as RSACryptoServiceProvider;
            RSAParameters para = key.ExportParameters(false);
            ACASvc.RSAKeyValueType rsaKeyVal = new ACASvc.RSAKeyValueType();
            rsaKeyVal.Exponent = para.Exponent;
            rsaKeyVal.Modulus = para.Modulus;

            keyval.Item = rsaKeyVal;
            var keyValArr = new ACASvc.KeyValueType[1];
            keyValArr[0] = keyval;

            keyInfo.Items = keyValArr;

            return keyInfo;
        }

        private static ACASvc.KeyInfoType getKeyInfo_New() {
            //var key = new RSACryptoServiceProvider();
            //KeyInfo keyInfoVanilla = new KeyInfo();
            //keyInfoVanilla.AddClause(new RSAKeyValue((RSA)key)); // vanilla .NET way
            // idea taken from https://social.msdn.microsoft.com/Forums/vstudio/en-US/118861d0-b3f2-4953-9ae8-f53900959fa7/generating-securitytokenreference-tag-in-the-soap-message?forum=wcf

            ACASvc.KeyInfoType keyInfo = new ACASvc.KeyInfoType();
            //var choiceList = new List<ACASvc.ItemsChoiceType2>();
            var choiceList = new ACASvc.ItemsChoiceType2[1];
            choiceList[0] = ACASvc.ItemsChoiceType2.X509Data;
            keyInfo.ItemsElementName = choiceList;

            X509Certificate2 cert = getX509Cert();
            var certVal = cert.GetPublicKeyString();

            //Microsoft.Web.Services2.Security.AsymmetricEncryptionKey
            XmlDocument soapXmlDoc = new XmlDocument(); 
            KeyInfoNode keyInfoNode = new KeyInfoNode();
            XmlElement securityRef = soapXmlDoc.CreateElement("wsse", "SecurityTokenReference");
            XmlElement keyIdentifier = soapXmlDoc.CreateElement("wsse:KeyIdentifier", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            keyIdentifier.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
            keyIdentifier.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
            keyIdentifier.InnerText = certVal;

            XmlAttribute valueType = soapXmlDoc.CreateAttribute("ValueType");
            valueType.Value = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1";
            keyIdentifier.Attributes.Append(valueType);

            XmlAttribute encodingType = soapXmlDoc.CreateAttribute("EncodingType");
            encodingType.Value = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
            keyIdentifier.Attributes.Append(encodingType);

            securityRef.AppendChild(keyIdentifier);
            keyInfoNode.Value = securityRef;

            KeyInfoNode[] nodes = { keyInfoNode };
            
            keyInfo.Items = nodes;

            return keyInfo;
        }


        private static ACASvc.X509DataType[] addCert() {
            ACASvc.X509DataType x509Irs = new ACASvc.X509DataType();
            var certList = new List<ACASvc.X509DataType>();

            X509Certificate2 cert = getX509Cert();

            var cList = new List<X509Certificate2>();
            cList.Add(cert);
            x509Irs.Items = cList.ToArray();
            certList.Add(x509Irs);
            return certList.ToArray();
        }

        private static X509Certificate2 getX509Cert() {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = store.Certificates.Find(
                X509FindType.FindByThumbprint, "46B20B24A3AE296CCFB91D2701D10601D43E2C3C", false);

            X509Certificate2 cert = new X509Certificate2();
            if (certCollection.Count > 0) {
                cert = certCollection[0];
            } else {
                CustomError.AddError("The X.509 Certificate can not be found on the server. Contact the Server Administrator.");
            }
            return cert;
        }

        private static string GetMd5Hash(MD5 md5Hash, byte[] data) {
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            // this post might be helpful: https://blogs.msdn.microsoft.com/csharpfaq/2006/10/09/how-do-i-calculate-a-md5-hash-from-a-string/
            StringBuilder sBuilder = new StringBuilder();

            // compute hash from byte[] data
            byte[] hashedData = md5Hash.ComputeHash(data);

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < hashedData.Length; i++) {
                sBuilder.Append(hashedData[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }


        // get StateType from text
        private static ACASvc.StateType getStateType(string state) {
            // instantiate stateCode
            var stateCode = ACASvc.StateType.AA;
            // make sure text field is uppercase
            state = state.ToUpper();

            // get state codes
            IEnumerable<ACASvc.StateType> stateList = Enum.GetValues(typeof(ACASvc.StateType)).Cast<ACASvc.StateType>();
            // check each one: if the code matches the "state" entered by user, use that code
            foreach (var s in stateList) {
                if (s.ToString() == state) {
                    stateCode = s;
                }
            }

            return stateCode;
        }

        // create XML Document from byte array
        private static XmlDocument buildXmlDoc(byte[] blob, Record rec) {
            XmlDocument xmlDoc = new XmlDocument();
            //using (var stream = new MemoryStream(blob, false)) {
                //var xDoc = XDocument.Load(stream);
                /*using (var xmlReader = xDoc.CreateReader()) {
                    xmlDoc.Load(xmlReader);
                }*/
                XmlReaderSettings settings = new XmlReaderSettings { CheckCharacters = false };
                using (XmlReader xmlReader = XmlReader.Create(rec.XmlFilePath, settings)) {
                    xmlReader.MoveToContent();
                    xmlDoc.Load(xmlReader);
                }
            //}
            //string xml = System.Text.Encoding.UTF8.GetString(blob);
            //xmlDoc.LoadXml(xml);
            return xmlDoc;
        }


        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj) {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }



        //======================================
        // OLD FUNCTION -- WILL BE DELETED
        //======================================

        // TRANSMIT THE SUBMISSION
        public static ACASvc.ACABulkRequestTransmitterResponseType SubmitTransmission(Record rec) {

            //try {
            //    // instantiate variables required for data submission
            //    var acaSecHeader = new ACASvc.TransmitterACASecurityHeaderType();
            //    var secHeader = new ACASvc.SecurityHeaderType();
            //    var bulkBusHeader = new ACASvc.ACABulkBusinessHeaderRequestType();
            //    var dtlType = new ACASvc.ACATrnsmtManifestReqDtlType();
            //    var bulkReqTrans = new ACASvc.ACABulkRequestTransmitterType();

            //    acaSecHeader.Item = ConfigurationManager.AppSettings["IrsUserId"];

            //    // SOAP Header
            //    // SECURITY 
            //    //acaSecHeader.
            //    //Microsoft.Web.Services2.Security.X509.X509CertificateStore store = new Microsoft.Web.Services2.Security.X509.X509CertificateStore(Store);
            //    //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //    //store.Open(OpenFlags.ReadOnly);
            //    //X509Certificate2Collection certCollection = store.Certificates.Find(
            //    //    X509FindType.FindByThumbprint, "46B20B24A3AE296CCFB91D2701D10601D43E2C3C", false);
            //    //// get the first cert with that thumbprint
            //    //if (certCollection.Count > 0) {
            //    //    X509Certificate2 cert = certCollection[0];
            //    //    // use cert...
            //    //    ACASvc.X509DataType x509Irs = new ACASvc.X509DataType();
            //    //    soapClient.ClientCredentials.ClientCertificate.Certificate = cert; // hooray! :D
            //    //} else {
            //    //    CustomError.AddError("The X.509 Certificate can not be found on the server. Contact the Server Administrator.");
            //    //}

            //    // REFER TO COMPOSITION AND REFERENCE GUIDE, PAGE 54 FOR DATA MAPPING
            //    // PAGE 18 FOR SOAP HEADER MANIFEST XML
            //    // https://www.irs.gov/PUP/for_taxpros/software_developers/information_returns/AIR_Composition_and_Reference_Guide.pdf

            //    // SOAP Body


            //    // 1094-C doc
            //    // these header elements are found on page 10 of Comp and Ref Guide
            //    dtlType.Id = "manifest"; 
            //    dtlType.PaymentYr = rec.TaxYr.ToString();
            //    dtlType.PriorYearDataInd = FormatChecker.BoolToDigitBoolType(rec.PriorYrDataInd);
            //    dtlType.EIN = "474038601"; // for Transmitter!
            //    // TransmissionTypeCd... "O" means "original."
            //    dtlType.TransmissionTypeCd = ACASvc.TransmissionTypeCdType.O; // create function to switch this as needed
            //    // TestFieldInd shows "P", which means "production"
            //    dtlType.TestFileCd = "T"; // testing for now
            //    // TransmitterForeignEntityInd... not needed, since EBS is not a foreign entity
            //    var transName = new ACASvc.BusinessNameType();
            //    transName.BusinessNameLine1Txt = "Employer Benefits Services LLC";
            //    //transName.BusinessNameLine2Txt = "";
            //    dtlType.TransmitterNameGrp = transName;

            //    // company info -- still for transmitter (change out these values!!!!)
            //    ACASvc.CompanyInformationGrpType compInfo = new ACASvc.CompanyInformationGrpType();
            //    compInfo.CompanyNm = "Employer Benefits Services LLC"; // should second line of the name be added?
            //    // create company address
            //    ACASvc.USAddressGrpType compAdd = new ACASvc.USAddressGrpType();
            //    compAdd.AddressLine1Txt = "8841 Helena Rd";
            //    //compAdd.AddressLine2Txt = "";
            //    compAdd.CityNm = "Pelham";
            //    ACASvc.StateType state = ACASvc.StateType.AL; //getStateType(rec.Company.Address.State);
            //    compAdd.USStateCd = state;
            //    compAdd.USZIPCd = "35124";
            //    //compAdd.USZIPExtensionCd = "";
            //    // add company address to object
            //    var mailGrp = new ACASvc.BusinessAddressGrpType();
            //    mailGrp.Item = compAdd;
            //    compInfo.MailingAddressGrp = mailGrp;
            //    // contact person info
            //    var contactNameGrp = new ACASvc.OtherCompletePersonNameType();
            //    contactNameGrp.PersonFirstNm = "Annette";
            //    //contactNameGrp.PersonMiddleNm = "";
            //    contactNameGrp.PersonLastNm = "Wade";
            //    //contactNameGrp.SuffixNm = "";
            //    compInfo.ContactNameGrp = contactNameGrp;
            //    compInfo.ContactPhoneNum = "2052914040";
            //    dtlType.CompanyInformationGrp = new ACASvc.CompanyInformationGrpType();
            //    dtlType.CompanyInformationGrp = compInfo;

            //    // vendor info (for the software pkg if this were off-the-shelf)
            //    // may not need this since same entity is transmitter and vendor (software developer)...
            //    ACASvc.VendorInformationGrpType vendorInfo = new ACASvc.VendorInformationGrpType();
            //    var vendorContact = new ACASvc.OtherCompletePersonNameType();
            //    vendorContact.PersonFirstNm = "Annette";
            //    //vendorContact.PersonMiddleNm = "";
            //    vendorContact.PersonLastNm = "Wade";
            //    //vendorContact.SuffixNm = "";
            //    vendorInfo.ContactNameGrp = vendorContact;
            //    vendorInfo.ContactPhoneNum = "2052914040";
            //    vendorInfo.VendorCd = "I"; // I = "in house"; V = "vendor"
            //    dtlType.VendorInformationGrp = vendorInfo;
            //    dtlType.TotalPayeeRecordCnt = rec.IndividualReports.Count.ToString(); // total number of 1095Cs
            //    dtlType.TotalPayerRecordCnt = "1"; // total number of 1094Cs
            //    dtlType.SoftwareId = ConfigurationManager.AppSettings["SoftwareID"];
            //    dtlType.FormTypeCd = ACASvc.FormNameType.Item10941095C; // this can be hardcoded for now, but might need to be changeable one day
            //    dtlType.BinaryFormatCd = ACASvc.BinaryFormatCodeType.applicationxml;

            //    // bulk business header stuff
            //    // UTID (unique transmission  identifier)
            //    bulkBusHeader.UniqueTransmissionId = rec.UniqueId;
            //    // assign business header timestamp (not security timestamp)
            //    bulkBusHeader.Timestamp = FormatTimestamp(rec.TimeStampGMT);
            //    bulkBusHeader.Id = "bulkBusHeader";

            //    // create file path for xml doc
            //    //rec.IrsFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + rec.Company.Name.Replace(' ', '_') + ".xml";
            //    //string filePath = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["FilesLocation"]) + "/" + rec.IrsFileName;

            //    string xmlFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + rec.Company.Name.Replace(' ', '_') + ".xml";
            //    rec.XmlFilePath = HostingEnvironment.MapPath(System.Configuration.ConfigurationManager.AppSettings["FilesLocation"]) + "/" + xmlFileName;
            //    // build the xml doc (1094 and 1095)

            //    byte[] blob = XmlController.BuildXmlDoc(rec);
            //    //byte[] blob = XmlGenerator.BuildXml(rec);
            //    bulkReqTrans.BulkExchangeFile = blob;
            //    // do the MD5 CheckSum using the above file.  Same for ByteSizeNum and DocSystemFileNm
            //    using (var md5 = MD5.Create()) {
            //        dtlType.ChecksumAugmentationNum = GetMd5Hash(md5, blob); 
            //    }
            //    dtlType.AttachmentByteSizeNum = blob.Length.ToString();
            //    dtlType.DocumentSystemFileNm = rec.IrsFileName; // need to get this

            //    // build security timestamp
            //    secHeader.Timestamp = GetSecurityTimestamp(rec.TimeStampGMT);
            //    // add signatures through the ACASvc object:
            //    // COMMENT OUT FOLLOWING LINE FOR LOCAL TESTING
            //    secHeader.Signature = getXmlSignature(bulkBusHeader, dtlType, secHeader.Timestamp);

            //    // store this submission in the db
            //    DataController.InsertSubmission(rec);
            //    // store the individual records in the db as well
            //    DataController.InsertIndividualRecords(rec.IndividualReports, rec.UniqueId);


            //    using (var context = new OperationContextScope(soapClient.InnerChannel)) {
            //        // the request msg
            //        HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
            //        // NOTE: first wrote this using HttpRequestMessageProperty class.  That's what lets line 204 work. But in order to use the 
            //        // MultipartFormDataContent as seen in line 190, I had to go with HttpRequestMessage class.  Causing other issues...

            //        // headers
            //        requestMessage.Headers.Add("Content-Encoding", "gzip");
            //        requestMessage.Headers.Add("Accept", "text/xml;charset=utf-8");
            //        requestMessage.Headers.Add("Content-Transfer-Encoding", "8bit");
            //        requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate");
            //        //requestMessage.Headers.Add("SOAPAction", "BulkRequestTransmitter"); // throwing error
            //        requestMessage.Headers.Add("MIME-Version", "1.0");

            //        // apply the headers
            //        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;

            //        // where the actual IRS submission occurs
            //        var response = soapClient.BulkRequestTransmitter(acaSecHeader, secHeader, ref bulkBusHeader, dtlType, bulkReqTrans);

            //        // create the status update AND record it in the db
            //        StatusUpdate update = buildStatusUpdate(response, rec.UniqueId);
            //        // add update to record
            //        rec.StatusUpdates.Add(update);

            //        return response;
            //    }
            //} catch (Exception e) {
            //    CustomError.AddError("The submission failed due to the following error: " + e.ToString());
            ACASvc.ACABulkRequestTransmitterResponseType response = new ACASvc.ACABulkRequestTransmitterResponseType();
            return response;
            //}
        } // end SubmitTransmission function

    } // end TransmissionController class
} // end namespace