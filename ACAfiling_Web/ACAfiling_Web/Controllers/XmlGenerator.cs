using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EbsClassPkg.Models;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.IO.Compression;

namespace ACAfiling_Web.Controllers
{
    public class XmlGenerator : Controller
    {
        // build the XML doc needed for submission
        public static byte[] BuildXml(Record rec) {
            XNamespace rootNs = "n1:Form109495CTransmittalUpstream";
            XNamespace n1Ns = "n1";
            XNamespace xmlNs = "xmlns";
            XNamespace xsiNs = "xsi";
            XNamespace irsNs = "irs";
            XDocument xdoc = new XDocument(
                new XElement(rootNs + "Form109495CTransmittalUpstream", 
                    new XAttribute("xmlns", "n1:Form109495CTransmittalUpstream"),
                    new XAttribute(xmlNs + "irs", "urn:us:gov:treasury:irs:common"),
                    new XAttribute(xmlNs + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(xmlNs + "n1", "urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage"),
                    new XAttribute(xsiNs + "schemaLocation", "urn:us:gov:treasury:irs:msg:form1094-1095Ctransmitterupstreammessage IRS-Form1094-1095CTransmitterUpstreamMessage.xsd"),
                    // child elements:
                    new XElement("Form1094CUpstreamDetail", 
                        new XAttribute("recordType", ""),
                        new XAttribute("lineNum", "0"),
                        new XElement("SubmissionId", "1"),
                        new XElement(irsNs + "TaxYr", rec.TaxYr.ToString())
                        // how to do fields for corrections and replacements??
                    )
                )
            );
            
            // save the XDocument
            xdoc.Save(rec.XmlFilePath);

            byte[] blob;

            using (MemoryStream ms = new MemoryStream()) {
                int origBytes = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                using (FileStream file = System.IO.File.OpenRead(rec.XmlFilePath)) {
                    byte[] buffer = new byte[file.Length];
                    int bytes;
                    while ((bytes = file.Read(buffer, 0, buffer.Length)) > 0) {
                        zip.Write(buffer, 0, bytes);
                        origBytes += bytes;
                    }
                }
                blob = ms.ToArray();
                //string asBase64 = Convert.ToBase64String(blob);
                //Console.WriteLine("Original: " + origBytes);
                //Console.WriteLine("Raw: " + blob.Length);
                //Console.WriteLine("Base64: " + asBase64.Length);
            }
            return blob;
        }
    }
}