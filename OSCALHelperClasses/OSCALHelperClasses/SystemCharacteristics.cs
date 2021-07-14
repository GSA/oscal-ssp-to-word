using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Data;

namespace OSCALHelperClasses
{
    public class SystemCharacteristics : OSCALBaseClass
    {
        public Item SystemID;
        public Item SystemName;
        public Item SystemNameShort;
        public Item SystemSensitivityLevel;
        public List<InformationType> InformationTypes;
        public ImpactLevel SecurityImpactLevel;
        public Item Status;

        public SystemCharacteristics()
        {

        }
        public SystemCharacteristics(XmlNode node)
        {
            if (node.Name != "system-characteristics")
                return;
            InformationTypes = new List<InformationType>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "system-id")
                {
                    SystemID = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "system-name")
                {
                    SystemName = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "system-name-short")
                {
                    SystemNameShort = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "system-sensitivity-level")
                {
                    SystemSensitivityLevel = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }
                if (child.Name == "system-information")
                {
                    foreach (XmlNode grandchild in child.ChildNodes)
                    {


                        if (grandchild.Name == "information-type")
                        {
                            InformationType informationType = new InformationType();
                            var impacts = new List<Impact>();
                            var xpaths = new List<string>();
                            foreach (XmlNode greatgrandchild in grandchild.ChildNodes)
                            {
                                if (greatgrandchild.Name == "description")
                                    informationType.Name = new Item
                                    {
                                        Value = greatgrandchild.InnerText,
                                        XPath = FindDeepestPath(greatgrandchild)
                                    };

                                if (greatgrandchild.Name == "information-type-id")
                                    informationType.InformationTypeID = new Item
                                    {
                                        Value = greatgrandchild.InnerText,
                                        XPath = FindDeepestPath(greatgrandchild)
                                    };
                                if (greatgrandchild.Name != "adjustment-justification" && greatgrandchild.Name != "title" && greatgrandchild.Name != "description" && greatgrandchild.Name != "information-type-id" && greatgrandchild.Name != "prop")
                                {
                                    XmlNode basenode = null;
                                    XmlNode selectNode = null;
                                    foreach (XmlNode e in greatgrandchild.ChildNodes)
                                    {
                                        if (e.Name == "base")
                                            basenode = e;
                                        if (e.Name == "selected")
                                            selectNode = e;
                                    }

                                    var impact = new Impact
                                    {
                                        Name = new Item
                                        {
                                            Value = greatgrandchild.Name
                                        },

                                        Base = new Item
                                        {
                                            Value = basenode.InnerText,
                                            XPath = FindDeepestPath(basenode)
                                        },

                                        Selected = new Item
                                        {
                                            Value = selectNode.InnerText,
                                            XPath = FindDeepestPath(selectNode)
                                        }
                                    };
                                    impacts.Add(impact);
                                }
                                informationType.Impacts = impacts;

                            }
                            InformationTypes.Add(informationType);
                        }

                    }


                }

                if (child.Name == "security-sensitivity-level")
                {
                    SystemSensitivityLevel = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }
                if (child.Name == "status")
                {
                    Status = new Item
                    {
                        Value = child.Attributes[0].Value,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "security-impact-level")
                {
                    SecurityImpactLevel = new ImpactLevel();
                    foreach (XmlNode grandchild in child.ChildNodes)
                    {
                        if (grandchild.Name == "security-objective-confidentiality")
                        {
                            var confi = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            SecurityImpactLevel.Confidentiality = confi;
                        }

                        if (grandchild.Name == "security-objective-integrity")
                        {
                            var tem = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            SecurityImpactLevel.Integrity = tem;
                        }

                        if (grandchild.Name == "security-objective-availability")
                        {
                            var temp = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            SecurityImpactLevel.Availability = temp;
                        }

                    }
                }
            }







        }

        public SystemCharacteristics(XmlNode node, bool Milestone)
        {
            if (node.Name != "system-characteristics")
                return;
            InformationTypes = new List<InformationType>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "system-id")
                {
                    SystemID = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "system-name")
                {
                    SystemName = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "system-name-short")
                {
                    SystemNameShort = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "system-sensitivity-level")
                {
                    SystemSensitivityLevel = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }
                if (child.Name == "system-information")
                {
                    foreach (XmlNode grandchild in child.ChildNodes)
                    {


                        if (grandchild.Name == "information-type")
                        {
                            InformationType informationType = new InformationType();
                            var impacts = new List<Impact>();
                            var xpaths = new List<string>();
                            foreach (XmlNode greatgrandchild in grandchild.ChildNodes)
                            {
                                if (greatgrandchild.Name == "title")
                                    informationType.Name = new Item
                                    {
                                        Value = greatgrandchild.InnerText,
                                        XPath = FindDeepestPath(greatgrandchild)
                                    };

                                if (greatgrandchild.Name == "information-type-id")
                                    informationType.InformationTypeID = new Item
                                    {
                                        Value = greatgrandchild.InnerText,
                                        XPath = FindDeepestPath(greatgrandchild)
                                    };
                                if (greatgrandchild.Name != "categorization" && greatgrandchild.Name != "adjustment-justification" && greatgrandchild.Name != "title" && greatgrandchild.Name != "description" && greatgrandchild.Name != "information-type-id" && greatgrandchild.Name != "prop")
                                {
                                    XmlNode basenode = null;
                                    XmlNode selectNode = null;
                                    foreach (XmlNode e in greatgrandchild.ChildNodes)
                                    {
                                        if (e.Name == "base")
                                            basenode = e;
                                        if (e.Name == "selected")
                                            selectNode = e;
                                    }

                                    var impact = new Impact
                                    {
                                        Name = new Item
                                        {
                                            Value = greatgrandchild.Name
                                        },

                                        Base = new Item
                                        {
                                            Value = basenode.InnerText,
                                            XPath = FindDeepestPath(basenode)
                                        },

                                        Selected = new Item
                                        {
                                            Value = selectNode.InnerText,
                                            XPath = FindDeepestPath(selectNode)
                                        }
                                    };
                                    impacts.Add(impact);
                                }
                                informationType.Impacts = impacts;

                            }
                            InformationTypes.Add(informationType);
                        }

                    }


                }

                if (child.Name == "security-sensitivity-level")
                {
                    SystemSensitivityLevel = new Item
                    {
                        Value = child.InnerText,
                        XPath = FindDeepestPath(child)
                    };
                }
                if (child.Name == "status")
                {
                    Status = new Item
                    {
                        Value = child.Attributes[0].Value,
                        XPath = FindDeepestPath(child)
                    };
                }

                if (child.Name == "security-impact-level")
                {
                    SecurityImpactLevel = new ImpactLevel();
                    foreach (XmlNode grandchild in child.ChildNodes)
                    {
                        if (grandchild.Name == "security-objective-confidentiality")
                        {
                            var confi = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            SecurityImpactLevel.Confidentiality = confi;
                        }

                        if (grandchild.Name == "security-objective-integrity")
                        {
                            var tem = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            SecurityImpactLevel.Integrity = tem;
                        }

                        if (grandchild.Name == "security-objective-availability")
                        {
                            var temp = new Item
                            {
                                Value = grandchild.InnerText,
                                XPath = FindDeepestPath(grandchild)
                            };
                            SecurityImpactLevel.Availability = temp;
                        }

                    }
                }
            }







        }
    }

    public struct DocInfoType
    {
        public string Name { get; set; }
        public string InfoId { get; set; }
        public string Description { get; set; }
        public string InfoTypeSytemId { get; set; }
        public string InfoTypeSytemName { get; set; }
        public string ConfidentialityImpactBase { get; set; }
        public string ConfidentialityImpactSelected { get; set; }
        public string IntegrityImpactBase { get; set; }
        public string IntegrityImpactSelected { get; set; }
        public string AvailabilityImpactBase { get; set; }
        public string AvailabilityImpactSelected { get; set; }
    }

    public struct InformationType
    {
        public Item Name;
        public Item InformationTypeID;
        public List<Impact> Impacts;
        public List<string> XPaths;
    }

    public struct Impact
    {
        public Item Name;
        public Item Base;
        public Item Selected;
    }
    public struct ImpactLevel
    {
        public Item Confidentiality;
        public Item Integrity;
        public Item Availability;
    }
}


