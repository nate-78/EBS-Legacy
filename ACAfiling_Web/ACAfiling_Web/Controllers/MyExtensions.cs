using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace ACAfiling_Web.Controllers {
    public static class MyExtensions {
        public static XElement ToXElement(this XmlNode node) {
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
                node.WriteTo(xmlWriter);
            return xDoc.Root;
        }

        public static XmlNode ToXmlNode(this XElement element) {
            using (XmlReader xmlReader = element.CreateReader()) {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }

        public static XElement ToXElement(this XmlElement xml) {
            XmlDocument doc = new XmlDocument();

            doc.AppendChild(doc.ImportNode(xml, true));

            return XElement.Parse(doc.InnerXml);

        }
    }
}