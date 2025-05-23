// using System.Xml.Serialization;
// XmlSerializer serializer = new XmlSerializer(typeof(RosterConfig));
// using (StringReader reader = new StringReader(xml))
// {
//    var test = (RosterConfig)serializer.Deserialize(reader);
// }

using System.Collections.Generic;
using System.Xml.Serialization;
using System;

[XmlRoot(ElementName = "decoder")]
public class Decoder
{

    [XmlAttribute(AttributeName = "model")]
    public string Model { get; set; }

    [XmlAttribute(AttributeName = "family")]
    public string Family { get; set; }

    [XmlAttribute(AttributeName = "comment")]
    public string Comment { get; set; }

}

[XmlRoot(ElementName = "dcclocoaddress")]
public class Dcclocoaddress
{

    [XmlAttribute(AttributeName = "number")]
    public string Number { get; set; }

    [XmlAttribute(AttributeName = "longaddress")]
    public string Longaddress { get; set; }
}

[XmlRoot(ElementName = "locoaddress")]
public class Locoaddress
{

    [XmlElement(ElementName = "dcclocoaddress")]
    public Dcclocoaddress Dcclocoaddress { get; set; }

    [XmlElement(ElementName = "number")]
    public string Number { get; set; }

    [XmlElement(ElementName = "protocol")]
    public string Protocol { get; set; }
}

[XmlRoot(ElementName = "keyvaluepair")]
public class Keyvaluepair
{

    [XmlElement(ElementName = "key")]
    public string Key { get; set; }

    [XmlElement(ElementName = "value")]
    public string Value { get; set; }
}

[XmlRoot(ElementName = "attributepairs")]
public class Attributepairs
{

    [XmlElement(ElementName = "keyvaluepair")]
    public List<Keyvaluepair> Keyvaluepair { get; set; }
}

[XmlRoot(ElementName = "speed")]
public class Speed
{

    [XmlElement(ElementName = "step")]
    public string Step { get; set; }

    [XmlElement(ElementName = "forward")]
    public string Forward { get; set; }

    [XmlElement(ElementName = "reverse")]
    public string Reverse { get; set; }
}

[XmlRoot(ElementName = "speeds")]
public class Speeds
{

    [XmlElement(ElementName = "speed")]
    public List<Speed> Speed { get; set; }
}

[XmlRoot(ElementName = "speedprofile")]
public class Speedprofile
{
    [XmlElement(ElementName = "speeds")]
    public Speeds Speeds { get; set; }
}

[XmlRoot(ElementName = "locomotive")]
public class Locomotive
{

    [XmlElement(ElementName = "attributepairs")]
    public Attributepairs Attributepairs { get; set; }

    [XmlElement(ElementName = "speedprofile")]
    public Speedprofile Speedprofile { get; set; }

    [XmlAttribute(AttributeName = "id")]
    public string Id { get; set; }

    [XmlAttribute(AttributeName = "fileName")]
    public string FileName { get; set; }

    [XmlAttribute(AttributeName = "roadName")]
    public string RoadName { get; set; }

    [XmlAttribute(AttributeName = "mfg")]
    public string Mfg { get; set; }

    [XmlAttribute(AttributeName = "owner")]
    public string Owner { get; set; }

    [XmlAttribute(AttributeName = "model")]
    public string Model { get; set; }

    [XmlAttribute(AttributeName = "dccAddress")]
    public string DccAddress { get; set; }

    [XmlAttribute(AttributeName = "comment")]
    public string Comment { get; set; }

    [XmlText]
    public string Text { get; set; }

    [XmlElement(ElementName = "dateUpdated")]
    public DateTime DateUpdated { get; set; }

    [XmlElement(ElementName = "decoder")]
    public Decoder Decoder { get; set; }

    [XmlElement(ElementName = "locoaddress")]
    public Locoaddress Locoaddress { get; set; }
}

[XmlRoot(ElementName = "roster")]
public class Roster
{

    [XmlElement(ElementName = "locomotive")]
    public List<Locomotive> Locomotive { get; set; }
}

[XmlRoot(ElementName = "rosterGroup")]
public class RosterGroup
{

    [XmlElement(ElementName = "group")]
    public string Group { get; set; }
}

[XmlRoot(ElementName = "roster-config")]
public class Rosterconfig
{

    [XmlElement(ElementName = "roster")]
    public Roster Roster { get; set; }

    [XmlElement(ElementName = "rosterGroup")]
    public RosterGroup RosterGroup { get; set; }

    [XmlAttribute(AttributeName = "xsi")]
    public string Xsi { get; set; }

    [XmlAttribute(AttributeName = "noNamespaceSchemaLocation")]
    public string NoNamespaceSchemaLocation { get; set; }

    [XmlText]
    public string Text { get; set; }
}

