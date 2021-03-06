﻿// SilverSim is distributed under the terms of the
// GNU Affero General Public License v3 with
// the following clarification and special exception.

// Linking this library statically or dynamically with other modules is
// making a combined work based on this library. Thus, the terms and
// conditions of the GNU Affero General Public License cover the whole
// combination.

// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module. An independent module is a module which is not derived from
// or based on this library. If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so. If you do not wish to do so, delete this
// exception statement from your version.

using SilverSim.Main.Common;
using SilverSim.Scene.Types.Script;
using SilverSim.Scripting.Lsl.Api.ByteString;
using SilverSim.Types;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.XPath;

namespace SilverSim.Scripting.Lsl.Api.Xml
{
    [ScriptApiName("XML")]
    [LSLImplementation]
    [PluginName("LSL_XML")]
    [Description("LSL XML API")]
    public sealed class XmlApi : IScriptApi, IPlugin
    {
        public void Startup(ConfigurationLoader loader)
        {
            /* intentionally left mpty */
        }

        [APIExtension(XmlExtensionName, "xmldocument")]
        [APIDisplayName("xmldocument")]
        [APIIsVariableType]
        [ImplementsCustomTypecasts]
        public sealed class Document
        {
            public readonly XmlDocument XmlDocument = new XmlDocument
            {
                XmlResolver = null
            };

            public Document()
            {
            }

            public Document(string xml)
            {
                XmlDocument.LoadXml(xml);
            }

            public Document(Stream s)
            {
                XmlDocument.Load(s);
            }

            public static explicit operator ByteArrayApi.ByteArray(Document doc) => new ByteArrayApi.ByteArray(doc.ToByteArray());

            public static explicit operator string(Document doc) => doc.ToByteArray().FromUTF8Bytes();

            public static explicit operator Document(string xml) => new Document(xml);

            public static explicit operator Document(ByteArrayApi.ByteArray byteArray)
            {
                using (var ms = new MemoryStream(byteArray.Data))
                {
                    return new Document(ms);
                }
            }

            public byte[] ToByteArray()
            {
                using (var ms = new MemoryStream())
                {
                    XmlDocument.Save(ms);
                    return ms.ToArray();
                }
            }

            public XmlNode SelectNode(string xpath, int index)
            {
                XmlNodeList nodeList = XmlDocument.SelectNodes(xpath);

                if (nodeList == null || nodeList.Count == 0)
                {
                    return null;
                }

                foreach (XmlNode node in nodeList)
                {
                    if(index--== 0)
                    {
                        return node;
                    }
                }

                return null;
            }

            public string this[string xpath]
            {
                get
                {
                    return this[xpath, 0];
                }

                set
                {
                    XmlNodeList nodeList = XmlDocument.SelectNodes(xpath);

                    if (nodeList == null || nodeList.Count == 0)
                    {
                        return;
                    }

                    foreach (XmlNode node in nodeList)
                    {
                        node.InnerXml = value;
                    }
                }
            }

            public string this[string xpath, int index]
            {
                get
                {
                    if(index < 0)
                    {
                        return string.Empty;
                    }
                    XPathNavigator nav = XmlDocument.CreateNavigator();
                    XPathExpression expr = nav.Compile(xpath);
                    object o = nav.Evaluate(expr);
                    if(o == null)
                    {
                        return string.Empty;
                    }
                    XPathNodeIterator xpathIt = o as XPathNodeIterator;
                    IFormattable formattable;
                    if (xpathIt != null)
                    {
                        if (xpathIt.Count <= index)
                        {
                            return string.Empty;
                        }

                        while (xpathIt.MoveNext())
                        {
                            if (xpathIt.CurrentPosition == index + 1)
                            {
                                XmlNode cNode = ((IHasXmlNode)xpathIt.Current).GetNode();
                                return cNode.InnerXml;
                            }
                        }
                        return string.Empty;
                    }
                    else if((formattable = o as IFormattable) != null)
                    {
                        return formattable.ToString(null, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return o.ToString();
                    }
                }
            }
        }

        private const string XmlExtensionName = "xml";

        [APIExtension(XmlExtensionName, "xmlCreateElement")]
        [APIExtension(APIExtension.MemberFunctions, APIUseAsEnum.MemberFunction, "CreateElement")]
        public void CreateElement(Document doc, string name)
        {
            doc.XmlDocument.AppendChild(doc.XmlDocument.CreateElement(name));
        }

        [APIExtension(XmlExtensionName, "xmlCreateElement")]
        [APIExtension(APIExtension.MemberFunctions, APIUseAsEnum.MemberFunction, "CreateElement")]
        public int CreateElement(Document doc, string xpath, int index, string name)
        {
            XmlElement node = doc.SelectNode(xpath, index) as XmlElement;
            bool success = false;
            if(node != null)
            {
                node.AppendChild(doc.XmlDocument.CreateElement(name));
                success = true;
            }
            return success.ToLSLBoolean();
        }

        [APIExtension(XmlExtensionName, "xmlCreateAttribute")]
        [APIExtension(APIExtension.MemberFunctions, APIUseAsEnum.MemberFunction, "CreateAttribute")]
        public int CreateElement(Document doc, string xpath, int index, string name, string value)
        {
            XmlElement node = doc.SelectNode(xpath, index) as XmlElement;
            bool success = false;
            if (node != null)
            {
                XmlAttribute attr = doc.XmlDocument.CreateAttribute(name);
                attr.Value = value;
                node.AppendChild(attr);
                success = true;
            }
            return success.ToLSLBoolean();
        }

        [APIExtension(XmlExtensionName, "xmlCreateCData")]
        [APIExtension(APIExtension.MemberFunctions, APIUseAsEnum.MemberFunction, "CreateCData")]
        public int CreateCData(Document doc, string xpath, int index, string data)
        {
            XmlElement node = doc.SelectNode(xpath, index) as XmlElement;
            bool success = false;
            if (node != null)
            {
                node.AppendChild(doc.XmlDocument.CreateCDataSection(data));
                success = true;
            }
            return success.ToLSLBoolean();
        }

        [APIExtension(XmlExtensionName, "xmlCreateText")]
        [APIExtension(APIExtension.MemberFunctions, APIUseAsEnum.MemberFunction, "CreateText")]
        public int CreateText(Document doc, string xpath, int index, string text)
        {
            XmlElement node = doc.SelectNode(xpath, index) as XmlElement;
            bool success = false;
            if (node != null)
            {
                node.AppendChild(doc.XmlDocument.CreateTextNode(text));
                success = true;
            }
            return success.ToLSLBoolean();
        }

        [APIExtension(XmlExtensionName, "xmlEscape")]
        [IsPure]
        public string XmlEscape(string input) => System.Security.SecurityElement.Escape(input);

        [APIExtension(XmlExtensionName, "xmlUnescape")]
        [IsPure]
        public string XmlUnescape(string input) => HttpUtility.HtmlDecode(input);
    }
}
