using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using System.Web.Hosting;
using EbsClassPkg.Models;

// got the idea for this class from this post:
// http://blogs.msdn.com/b/endpoint/archive/2011/04/23/wcf-extensibility-message-inspectors.aspx
// in order for the request to be picked up by this class, it must be connected using
// a custom endpoint behavior.  Chck out the "ContractBehavior" class and the "behaviors"
// element in Web.config to see how it all wires up together.
// http://stackoverflow.com/questions/5493639/how-do-i-get-the-xml-soap-request-of-an-wcf-web-service-request

namespace ACAfiling_Web.Controllers {
    public class SoapMessageInspector : IClientMessageInspector {
        public void AfterReceiveReply(ref Message reply, object correlationState) {
            return; // do nothing
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel) {
            // export request message to log file for testing purposes
            //string file = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["LogLocation"]) + "/request.xml";

            //using (XmlWriter writer = XmlWriter.Create(file)) {
            //    File.SetAttributes(file, FileAttributes.Normal);
            //    request.WriteMessage(writer); // consuming the request here
            //    writer.Flush();
            //    writer.Close();
            //}

            //// rebuild request so message can continue
            //XmlDocument doc = new XmlDocument();
            //doc.Load(file);
            // commented out the above lines 10/14/16

            ////========
            //// this code is an interrupt that rebuilds the xml by adding a custom KeyInfo element.
            //// Just remove this section to return the code to normal
            //doc = addKeyInfo(doc);
            //// save file again for review purposes
            //string newFile = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["LogLocation"]) + "/request_edited.xml";
            //using (XmlWriter writer = XmlWriter.Create(newFile)) {
            //    File.SetAttributes(file, FileAttributes.Normal);
            //    doc.WriteTo(writer); // consuming the request here
            //    writer.Flush();
            //    writer.Close();
            //}
            //// now rebuild request again so message can continue
            //doc.Load(newFile);
            ////========

            // commented out 10/14/16
            //MemoryStream ms = new MemoryStream();
            //doc.Save(ms);
            //ms.Position = 0;
            //XmlReader reader = XmlReader.Create(ms);
            //Message newMsg = Message.CreateMessage(reader, int.MaxValue, request.Version);
            //newMsg.Properties.CopyProperties(request.Properties);
            //request = newMsg;

            return null;
        }

        private XmlDocument addKeyInfo(XmlDocument xml) {
            X509Certificate2 cert = getX509Cert();
            var certVal = cert.GetPublicKeyString();

            KeyInfoNode keyInfoNode = new KeyInfoNode();
            XmlElement securityRef = xml.CreateElement("SecurityTokenReference"); // xml.CreateElement("wsse:SecurityTokenReference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            XmlElement keyIdentifier = xml.CreateElement("KeyIdentifier"); // xml.CreateElement("wsse:KeyIdentifier", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            keyIdentifier.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
            keyIdentifier.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
            keyIdentifier.InnerText = certVal;

            XmlAttribute valueType = xml.CreateAttribute("ValueType");
            valueType.Value = "http://docs.oasis-open.org/wss/oasis-wss-soap-message-security-1.1#ThumbprintSHA1";
            keyIdentifier.Attributes.Append(valueType);

            XmlAttribute encodingType = xml.CreateAttribute("EncodingType");
            encodingType.Value = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
            keyIdentifier.Attributes.Append(encodingType);

            securityRef.AppendChild(keyIdentifier);
            keyInfoNode.Value = securityRef;

            // find the nodes that we need to work with
            XmlNamespaceManager nsm = new XmlNamespaceManager(xml.NameTable);
            nsm.AddNamespace("h", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            XmlNode secNode = xml.SelectSingleNode("//h:Security", nsm);
            if (secNode == null) {
                throw new Exception("Path to Security element is not correct [SoapMessageInspector.cs --> addKeyInfo()]");
            }
            // get signature node
            XmlNode sigNode = secNode.FirstChild;
            if (sigNode == null) {
                throw new Exception("Path to Signature element is not correct [SoapMessageInspector.cs --> addKeyInfo()]");
            }
            // get keyinfo node
            XmlNode kiNode = sigNode.LastChild;
            if (kiNode == null) {
                throw new Exception("Path to KeyInfo element is not correct [SoapMessageInspector.cs --> addKeyInfo()]");
            }
            var securityRefElement = MyExtensions.ToXElement(securityRef);
            var secRefNode = MyExtensions.ToXmlNode(securityRefElement);
            XmlElement newKeyInfo = xml.CreateElement("KeyInfo");
            try { 
                newKeyInfo.InnerXml = secRefNode.OuterXml;
            } catch {
                throw new Exception("newKeyInfo NodeType: " + newKeyInfo.NodeType.ToString() + " and secRefNode NodeType: " + secRefNode.NodeType.ToString());
            }
            var newKiElement = MyExtensions.ToXElement(newKeyInfo);
            var newKiNode = MyExtensions.ToXmlNode(newKiElement);
            kiNode.InnerXml = secRefNode.OuterXml;
            //try {
            //    sigNode.ReplaceChild(newKiNode, kiNode);
            //} catch {
            //    throw new Exception("breaking at sigNode.ReplaceChild()");
            //}

            return xml;
        }

        private X509Certificate2 getX509Cert() {
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
    }
}