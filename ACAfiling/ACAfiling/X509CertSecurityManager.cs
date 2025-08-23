using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.Services2.Security;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using EbsClassPkg.Models;
using System.Xml;
using System.Xml.Linq;

namespace ACAfiling.Controllers {
    // a collection of helper functions to comput hashes, access the X509 certificate, etc
    public class X509CertSecurityManager {

        public static string GetXmlSignature(string envelope, string manifestId, string timestampId, string busHeaderId) {
            // create XmlDoc out of envelope
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(envelope);

            // create references for each element using IDs: #manifest, #busHeader, #timestamp
            var signedXml = new SignedXmlWithId(xmlDoc);
            var cert = getX509Cert();
            signedXml.SigningKey = cert.PrivateKey;
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NWithCommentsTransformUrl;

            // add references
            signedXml.AddReference(createReference(timestampId, "wsse wsa soapenv urn urn1 urn2 urn3"));
            signedXml.AddReference(createReference(manifestId, "wsa soapenv urn1 urn2 urn3"));
            signedXml.AddReference(createReference(busHeaderId, "wsa soapenv urn urn1 urn3"));

            // keyInfo
            KeyInfo keyInfo = new KeyInfo();
            //SecurityTokenReference tokenRef = new SecurityTokenReference();
            //KeyIdentifier keyIdentifier = new KeyIdentifier();
            KeyInfoX509Data keyInfoData = new KeyInfoX509Data(cert);
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            // sign doc
            signedXml.ComputeSignature();

            // get the signed element
            XmlElement signedElement = signedXml.GetXml();

            // grab keyinfo stuff from the new element
            XmlNamespaceManager nsm = new XmlNamespaceManager(xmlDoc.NameTable);
            nsm.AddNamespace("ab", "http://www.w3.org/2000/09/xmldsig#");
            XmlNode keyInfoNode = signedElement.SelectSingleNode("//ab:KeyInfo", nsm);
            if (keyInfoNode == null) {
                throw new Exception("Still not getting keyinfo node");
            }
            XmlNode x509Node = keyInfoNode.SelectSingleNode("//ab:X509Data", nsm);
            if (x509Node == null) {
                throw new Exception("Still not getting x509 node...");
            }
            XmlNode x509CertNode = x509Node.SelectSingleNode("//ab:X509Certificate", nsm);
            if (x509CertNode == null) {
                throw new Exception("Still not getting x509 CERT node...");
            }

            // create new KeyInfo child elements
            XmlElement securityTokenReference = xmlDoc.CreateElement("wsse", "SecurityTokenReference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            XmlElement keyIdentifier = xmlDoc.CreateElement("wsse", "KeyIdentifier", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            // add attributes to KeyIdentifier
            keyIdentifier.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
            keyIdentifier.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
            keyIdentifier.InnerText = getX509CertValForKeyId(); // x509CertNode.InnerText;
            // nest the elements
            securityTokenReference.AppendChild(keyIdentifier);

            // add new KeyInfo elements to the KeyInfo element
            keyInfoNode.RemoveAll();
            keyInfoNode.AppendChild(securityTokenReference);

            // apply security element to the doc within Security element
            nsm.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            nsm.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

            XmlNode securityNode = xmlDoc.SelectSingleNode("//wsse:Security", nsm);
            XmlNode timestampNode = xmlDoc.SelectSingleNode("//wsu:Timestamp", nsm);
            XmlNode root = xmlDoc.DocumentElement;
            securityNode.InsertBefore(signedElement, timestampNode);
            

            return xmlDoc.OuterXml;
        }

        private static string getX509CertValForKeyId() {
            string val = "";
            var cert = getX509Cert();
            byte[] certBytes = ConvertFromHexToByteArr(cert.GetRawCertDataString());
            if (certBytes == null || certBytes.Length < 1) {
                throw new Exception("Not getting byte array from certificate");
            }
            val = Convert.ToBase64String(certBytes);

            return val;
        }

        public static byte[] ConvertFromHexToByteArr(string inputHex) {
            inputHex = inputHex.Replace("-", "");

            byte[] resultantArray = new byte[inputHex.Length / 2];
            for (int i = 0; i < resultantArray.Length; i++) {
                resultantArray[i] = Convert.ToByte(inputHex.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }


        private static System.Security.Cryptography.Xml.Reference createReference(string elementId, string namespaceList) {
            var transform = new XmlDsigExcC14NTransform();
            transform.Algorithm = "http://www.w3.org/2001/10/xml-exc-c14n#";
            transform.InclusiveNamespacesPrefixList = namespaceList;
            var r = new System.Security.Cryptography.Xml.Reference();
            r.Uri = elementId;
            r.AddTransform(transform);

            // no digest algorithm or value? is that done automagically when sig created?
            return r;
        }


        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj) {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        // compute the checksum
        public static string ComputeSHA256CheckSum(byte[] blob) {
            StringBuilder sbuilder = new StringBuilder();
            var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(blob);

            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            for (int i = 0; i < hash.Length; i++) {
                sbuilder.Append(hash[i].ToString("x2"));
            }

            return sbuilder.ToString();
        }

        // THE 2 FOLLOWING FUNCTIONS APPLIED TO TAX YEARS BEFORE 2017, WHICH ALLOWED MD5 ENCRYPTION
        // compute checksum
        //public static string ComputeCheckSum(byte[] blob) {
        //    string checkSum = "";
        //    using (var md5 = MD5.Create()) {
        //        checkSum = GetMd5HashString(md5, blob);
        //    }

        //    return checkSum;
        //}

        //public static string GetMd5HashString(MD5 md5Hash, byte[] data) {
        //    // Create a new Stringbuilder to collect the bytes and create a string.
        //    // this post might be helpful: https://blogs.msdn.microsoft.com/csharpfaq/2006/10/09/how-do-i-calculate-a-md5-hash-from-a-string/
        //    StringBuilder sBuilder = new StringBuilder();

        //    // compute hash from byte[] data
        //    byte[] hashedData = md5Hash.ComputeHash(data);

        //    // Loop through each byte of the hashed data and format each one as a hexadecimal string.
        //    for (int i = 0; i < hashedData.Length; i++) {
        //        sBuilder.Append(hashedData[i].ToString("x2"));
        //    }

        //    // Return the hexadecimal string.
        //    return sBuilder.ToString();
        //}


        // get hashed value of certificate // NOT CURRENTLY USING THIS
        public static string GetHashedCertValue() {
            var cert = getX509Cert();
            string hashedCertVal = "NO_VALUE_FOR_TESTING";

            if (cert != null) {
                hashedCertVal = GetHashString(cert);

                // new approach: http://stackoverflow.com/questions/32404687/c-sharp-add-wssesecurity-and-binarysecuritytoken-to-envelope-xml-file-programma
                var export = cert.Export(X509ContentType.Cert);
                hashedCertVal = Convert.ToBase64String(export);
            }

            return hashedCertVal;
        }


        // compute SHA1 hash
        public static byte[] ComputeSha1Hash(Object obj) {
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] digestValue = sha.ComputeHash(ObjectToByteArray(obj));

            return digestValue;
        }


        public static string Base64Encode(string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        // NOT CURRENTLY USING
        public static string GetHashString(Object obj) {
            // first, get the object's digest value
            byte[] digest = ComputeSha1Hash(obj);

            return Convert.ToBase64String(digest);
        }

        // NOT CURRENTLY USING
        private static string getStringFromHash(byte[] bytes) {
            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++) {
                sBuilder.Append(bytes[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }


        // get the X.509 Certificate
        private static X509Certificate2 getX509Cert() {
            X509Certificate2 cert = new X509Certificate2();
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            if (store != null) {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates.Find(
                    X509FindType.FindByThumbprint, "46B20B24A3AE296CCFB91D2701D10601D43E2C3C", false);

                if (certCollection.Count > 0) {
                    cert = certCollection[0];
                } else {
                    CustomError.AddError("The X.509 Certificate can not be found on the server. Contact the Server Administrator.");
                }
            }
            
            return cert;
        }

    } // END CLASS
} // END NAMESPACE