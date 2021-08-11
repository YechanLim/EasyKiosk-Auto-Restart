using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

namespace ProcessWorkingCheckService
{
    [Serializable]
    [XmlRoot("POS")]
    public class SMSXmlModel
    {
        [XmlElement]
        public string HEAD_OFFICE_NO { get; set; }
        [XmlElement]
        public string SHOP_NO { get; set; }
        [XmlElement]
        public string MSG_GB { get; set; }
        [XmlElement]
        public string SND_PHN_ID { get; set; }
        [XmlElement]
        public string RCV_PHN_ID { get; set; }
        [XmlElement]
        public string SND_MSG { get; set; }
    }
}