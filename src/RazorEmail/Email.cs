using System;
using System.Text;
using System.Xml.Serialization;

namespace RazorEmail
{
    [Serializable, XmlRoot("email")]
    public class Email
    {
        [XmlElement("subject")]
        public string Subject { get; set; }

        [XmlElement("from")]
        public Address From { get; set; }

        [XmlArray("views"), XmlArrayItem("add")]
        public View[] Views { get; set; }

        [XmlArray("bcc"), XmlArrayItem("add")]
        public Address[] Bcc { get; set; }

        [XmlArray("cc"), XmlArrayItem("add")]
        public Address[] CC { get; set; }

        [XmlArray("to"), XmlArrayItem("add")]
        public Address[] To { get; set; }

        [XmlArray("headers"), XmlArrayItem("add")]
        public Header[] Headers { get; set; }

        public class Header
        {
            [XmlAttribute("key")]
            public string Key { get; set; }

            [XmlText]
            public string Value { get; set; }

        }

        public class Address
        {
            [XmlText]
            public string Email { get; set; }

            [XmlAttribute("display")]
            public string Name { get; set; }
        }

        public class View 
        {
            [XmlAttribute("encoding")]
            public string EncodingText { get; set; }

            public Encoding Encoding
            {
                get { 
                    if (this.EncodingText == null) return Encoding.Default;
                    return Encoding.GetEncoding(this.EncodingText);
                }
            }

            [XmlText]
            public string Content { get; set; }

            [XmlAttribute("media-type")]
            public string MediaType { get; set; }
        }
    }
}