using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EbsClassPkg.Models;
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

namespace ACAfiling_Web.Controllers {
    // CLASS TO INTERACT WITH THE IRS'S SOAP SERVICE
    // GETS THE LATEST STATUS UPDATE FROM THE IRS
    // BASED ON A TRANSMISSION'S RECEIPT ID
    public class StatusRequestController {

        // get the security header for making the SOAP request
        public static ACAStatus.SecurityHeaderType GetStatusSecHeader(ACAStatus.ACABulkBusinessHeaderRequestType busHeader,
            ACAStatus.ACABulkRequestTransmitterStatusDetailRequestType statusDtlReq) {

            // the security header consists of a 'Timestamp' element and a 'Signature' element

            var secHeader = new ACAStatus.SecurityHeaderType();

            // build the timestamp
            var stamp = GetSecurityTimestamp(DateTime.UtcNow.ToString());
            secHeader.Timestamp = stamp;

            // build the signature
            // COMMENT OUT THIS LINE WHEN RUNNING LOCAL TESTS
            var sig = getXmlSignature(busHeader, statusDtlReq, stamp);
            secHeader.Signature = sig;

            return secHeader;
        }


        // sign XML elements
        private static ACAStatus.SignatureType getXmlSignature(ACAStatus.ACABulkBusinessHeaderRequestType bulkBusHeader,
            ACAStatus.ACABulkRequestTransmitterStatusDetailRequestType dtlType, ACAStatus.TimestampType timestamp) {

            var sig = new ACAStatus.SignatureType();

            // sign timestamp
            var canonType = new ACAStatus.CanonicalizationMethodType();
            canonType.Algorithm = "http://www.w3.org/2001/10/xml-exc-c14n#WithComments";
            var digest = new ACAStatus.DigestMethodType();
            digest.Algorithm = "http://www.w3.org/2000/09/xmldsig#sha1";
            var transformAlg = new ACAStatus.TransformType();
            transformAlg.Algorithm = "http://www.w3.org/2001/10/xml-exc-c14n";
            // CRG shows an element called InclusiveNamespaces inside the Transforms element.  Not sure how to create that

            var referenceTimeStamp = buildSigRef(digest, "#" + timestamp.Id, transformAlg, timestamp);
            var referenceBusHeader = buildSigRef(digest, "#" + bulkBusHeader.Id, transformAlg, bulkBusHeader);
            var referenceManifestDtl = buildSigRef(digest, "#" + dtlType.Id, transformAlg, dtlType);

            var refs = new List<ACAStatus.ReferenceType>();
            refs.Add(referenceTimeStamp);
            refs.Add(referenceBusHeader);
            refs.Add(referenceManifestDtl);

            var signedInfo1 = new ACAStatus.SignedInfoType();
            signedInfo1.CanonicalizationMethod = canonType;
            signedInfo1.Reference = refs.ToArray();
            var sigMethodType = new ACAStatus.SignatureMethodType();
            sigMethodType.Algorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            signedInfo1.SignatureMethod = sigMethodType;
            sig.SignedInfo = signedInfo1;

            // keyinfo
            //sig.KeyInfo = getKeyInfo(); // this builds KeyInfo using RSA key, etc
            sig.KeyInfo = getKeyInfo_New(); // this builds KeyInfo with KeyIdentifier (preferred by IRS)

            var sigval = new ACAStatus.SignatureValueType();
            sigval.Value = TransmissionController.ObjectToByteArray(signedInfo1);
            sig.SignatureValue = sigval;

            return sig;
        }


        private static ACAStatus.ReferenceType buildSigRef(ACAStatus.DigestMethodType digest, string uri, ACAStatus.TransformType transform, Object obj) {
            var reference = new ACAStatus.ReferenceType();
            reference.DigestMethod = digest;
            reference.URI = uri;

            var refTransforms = new List<ACAStatus.TransformType>();
            refTransforms.Add(transform);
            reference.Transforms = refTransforms.ToArray();

            SHA1 sha = new SHA1CryptoServiceProvider();
            reference.DigestValue = sha.ComputeHash(TransmissionController.ObjectToByteArray(obj));

            return reference;
        }


        private static ACAStatus.KeyInfoType getKeyInfo() {
            //var key = new RSACryptoServiceProvider();
            //KeyInfo keyInfoVanilla = new KeyInfo();
            //keyInfoVanilla.AddClause(new RSAKeyValue((RSA)key)); // vanilla .NET way

            ACAStatus.KeyInfoType keyInfo = new ACAStatus.KeyInfoType();
            //var choiceList = new List<ACASvc.ItemsChoiceType2>();
            var choiceList = new ACAStatus.ItemsChoiceType2[1];
            choiceList[0] = ACAStatus.ItemsChoiceType2.KeyValue;
            //choiceList.Add(ACASvc.ItemsChoiceType2.X509Data);
            //keyInfo.ItemsElementName = choiceList.ToArray();
            keyInfo.ItemsElementName = choiceList;
            //keyInfo.Items = addCert();

            // not sure what to do with this stuff...
            ACAStatus.KeyValueType keyval = new ACAStatus.KeyValueType();
            var cert = getX509Cert();

            RSACryptoServiceProvider key = cert.PublicKey.Key as RSACryptoServiceProvider;
            RSAParameters para = key.ExportParameters(false);
            ACAStatus.RSAKeyValueType rsaKeyVal = new ACAStatus.RSAKeyValueType();
            rsaKeyVal.Exponent = para.Exponent;
            rsaKeyVal.Modulus = para.Modulus;

            keyval.Item = rsaKeyVal;
            var keyValArr = new ACAStatus.KeyValueType[1];
            keyValArr[0] = keyval;

            keyInfo.Items = keyValArr;

            return keyInfo;
        }

        private static ACAStatus.KeyInfoType getKeyInfo_New() {
            //var key = new RSACryptoServiceProvider();
            //KeyInfo keyInfoVanilla = new KeyInfo();
            //keyInfoVanilla.AddClause(new RSAKeyValue((RSA)key)); // vanilla .NET way
            // idea taken from https://social.msdn.microsoft.com/Forums/vstudio/en-US/118861d0-b3f2-4953-9ae8-f53900959fa7/generating-securitytokenreference-tag-in-the-soap-message?forum=wcf

            ACAStatus.KeyInfoType keyInfo = new ACAStatus.KeyInfoType();
            //var choiceList = new List<ACASvc.ItemsChoiceType2>();
            //var choiceList = new ACAStatus.ItemsChoiceType2[1];
            //choiceList[0] = ACAStatus.ItemsChoiceType2.X509Data;
            var choiceList = new List<ACAStatus.ItemsChoiceType2>();
            choiceList.Add(ACAStatus.ItemsChoiceType2.Item);
            keyInfo.ItemsElementName = choiceList.ToArray();

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

            //keyInfo.Items = nodes;

            XmlElement kiElement = keyInfoNode.GetXml();
            string keyInfoText = kiElement.Value;
            string[] kitArr = { keyInfoText };
            //keyInfo.Items = kitArr;
            keyInfo.Text = kitArr;

            //var keyVal = new ACAStatus.KeyValueType();
            //keyVal.Text = kitArr;
            //var keyValArr = new ACAStatus.KeyValueType[1];
            //keyValArr[0] = keyVal;
            //keyInfo.Items = keyValArr;

            return keyInfo;
        }


        private static ACAStatus.X509DataType[] addCert() {
            ACAStatus.X509DataType x509Irs = new ACAStatus.X509DataType();
            var certList = new List<ACAStatus.X509DataType>();

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


        private static ACAStatus.TimestampType GetSecurityTimestamp(string timestamp) {
            var timestampy = new ACAStatus.TimestampType();
            var timestampyString = new ACAStatus.AttributedDateTime();
            timestampyString.Value = TransmissionController.FormatTimeStampForSecurityString(timestamp);
            timestampy.Created = timestampyString;
            var expireString = new ACAStatus.AttributedDateTime();
            expireString.Value = TransmissionController.FormatTimeStampForSecurityString(TransmissionController.getExpire(timestamp));
            timestampy.Expires = expireString;
            timestampy.Id = "timestamp";

            return timestampy;
        }

    } // end class
} // end namespace