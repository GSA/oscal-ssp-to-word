using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;


namespace OSCALHelperClasses
{
    public class SecurityControl : OSCALBaseClass
    {
        private string _id;
        public List<Property> Properties { get; set; }
        public List<Parameter> Parameters { get; set; }

        public List<Statement> Statements { get; set; }

        public List<ResponsibleRole> ResponsibleRoles { get; set; }

        public bool HasMultipleResponsibleRoles
        {
            get
            {
                bool truth = ResponsibleRoles.Count > 1 ? true : false;
                return truth;
            }
        }

        public bool HasTables
        {
            get
            {
                return HasATable();
            }
        }

        public List<int> XPathIDs { get; set; }

        public int InitPathID;

        private int _rollingID;

        public string ControlId
        {
            get
            {
                return _id;
            }
        }

        public SecurityControl(string controlId)
        {
            _id = controlId;
        }

        public SecurityControl(XmlNode node, int initPathID)
        {
            InitPathID = initPathID;
            _rollingID = initPathID;
            // This node has to be a security control node with a control id.
            if ((node.Attributes[0].Name == "control-id") || (node.Attributes.Count >= 2 && node.Attributes[1].Name == "control-id"))
            {
                _id = node.Attributes[0].Value;
                Parameters = new List<Parameter>();
                Statements = new List<Statement>();
                Properties = new List<Property>();
                ResponsibleRoles = new List<ResponsibleRole>();

                XPathIDs = new List<int>();

                BuildSecurityControl(node);
            }

        }

        public SecurityControl(XmlNode node, int initPathID, string XMLNamespace)
        {
            InitPathID = initPathID;
            _rollingID = initPathID;
            // This node has to be a security control node with a control id.
            if ((node.Attributes[0].Name == "control-id") || (node.Attributes.Count >= 2 && node.Attributes[1].Name == "control-id"))
            {
                if (node.Attributes[0].Name == "control-id")
                    _id = node.Attributes[0].Value;
                if ((node.Attributes.Count >= 2 && node.Attributes[1].Name == "control-id"))
                    _id = node.Attributes[1].Value;
                Parameters = new List<Parameter>();
                Statements = new List<Statement>();
                Properties = new List<Property>();
                ResponsibleRoles = new List<ResponsibleRole>();

                XPathIDs = new List<int>();

                BuildSecurityControl(node, XMLNamespace);
            }

        }


        private bool HasATable()
        {
            bool result = false;
            foreach (var st in Statements)
            {
                result = result || st.HasTable;
            }

            foreach (var par in Parameters)
            {
                result = result || par.HasTable;
            }

            return result;
        }


        protected void BuildSecurityControl(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "prop")
                {
                    _rollingID++;
                    var prop = new Property();
                    prop.Name = child.Attributes[0].Value;
                    if (child.InnerText == string.Empty)
                    {
                        // TGVSR - 07-19-2021 - Patch for element level specification of ns on prop element
                        for (int a = 1; a < child.Attributes.Count; a++)
                        {
                            if (child.Attributes[a].Name == "value")
                            {
                                prop.Value = child.Attributes[a].Value;
                            }
                        }
                    }
                    else
                    {
                        prop.Value = child.InnerText;
                    }
                    prop.XPath = FindDeepestPath(child);

                    XPathIDs.Add(_rollingID);

                    Properties.Add(prop);
                }

                if (child.Name == "responsible-role")
                {
                    _rollingID++;
                    var role = new ResponsibleRole();
                    role.Name = child.Attributes[0].Value;
                    role.Value = child.InnerText;
                    role.XPath = FindDeepestPath(child);

                    XPathIDs.Add(_rollingID);

                    ResponsibleRoles.Add(role);
                }

                if (child.Name == "set-param")
                {
                    _rollingID++;
                    var param = new Parameter();
                    param.ParamID = child.Attributes[0].Value;
                    param.Value = child.InnerText;
                    param.XPath = FindDeepestPath(child);
                    param.XmlTables = new List<XmlTable>();

                    XPathIDs.Add(_rollingID);
                    var xml = child.ChildNodes[0].InnerXml;
                    int position = 0;
                    foreach (XmlNode grandchild in child.ChildNodes[0].ChildNodes)
                    {
                        if (grandchild.Name == "table")
                        {
                            position = xml.IndexOf("<table>");
                            param.HasTable = true;
                            var table = new XmlTable(grandchild.InnerXml, position);
                            param.XmlTables.Add(table);

                        }

                    }


                    Parameters.Add(param);
                }

                if (child.Name == "statement")
                {
                    _rollingID++;
                    var statement = new Statement();
                    statement.StatementID = child.Attributes[0].Value;
                    statement.Value = child.InnerText;
                    statement.XPath = FindDeepestPath(child);
                    statement.XmlTables = new List<XmlTable>();

                    XPathIDs.Add(_rollingID);

                    var xml = child.ChildNodes[0].InnerXml;
                    int position = 0;

                    foreach (XmlNode grandchild in child.ChildNodes[0].ChildNodes)
                    {
                        if (grandchild.Name == "table")
                        {
                            position = xml.IndexOf("<table>");
                            statement.HasTable = true;
                            var table = new XmlTable(grandchild.InnerXml, position);
                            statement.XmlTables.Add(table);

                        }

                    }

                    Statements.Add(statement);
                }
            }

        }


        protected void BuildSecurityControl(XmlNode node, string XMLNamespace)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "prop")
                {
                    _rollingID++;
                    var prop = new Property();
                    prop.Name = child.Attributes[0].Value;
                    if (child.InnerText == string.Empty)
                    {
                        // TGVSR - 07-19-2021 - Patch for element level specification of ns on prop element
                        for (int a = 1; a < child.Attributes.Count; a++)
                        {
                            if (child.Attributes[a].Name == "value")
                            {
                                prop.Value = child.Attributes[a].Value;
                            }
                        }
                    }
                    else
                    {
                        prop.Value = child.InnerText;
                    }

                    prop.XPath = FindDeepestPath(child);

                    XPathIDs.Add(_rollingID);

                    Properties.Add(prop);
                }

                if (child.Name == "responsible-role")
                {
                    _rollingID++;
                    var role = new ResponsibleRole();
                    role.Name = child.Attributes[0].Value;
                    role.Value = child.InnerText;
                    role.XPath = FindDeepestPath(child);

                    XPathIDs.Add(_rollingID);

                    ResponsibleRoles.Add(role);
                }

                if (child.Name == "set-param" || child.Name == "set-parameter")
                {
                    _rollingID++;
                    var param = new Parameter();
                    param.ParamID = child.Attributes[0].Value;
                    if (child.InnerText == string.Empty)
                    {
                        // TGVSR - 07-19-2021 - Patch for element level value specification 
                        for (int a = 1; a < child.Attributes.Count; a++)
                        {
                            if (child.Attributes[a].Name == "value")
                            {
                                param.Value = child.Attributes[a].Value;
                            }
                        }
                    }
                    else
                    {
                        param.Value = child.InnerText;
                    }

                    //param.Value = child.InnerText;
                    param.XPath = FindDeepestPath(child);
                    param.XmlTables = new List<XmlTable>();
                    XPathIDs.Add(_rollingID);


                    var xml = child.ChildNodes[0].InnerXml;
                    param.InnerXml = xml;
                    int position = 0, endPosition = 0;
                    foreach (XmlNode grandchild in child.ChildNodes[0].ChildNodes)
                    {

                        if (grandchild.Name == "table")
                        {
                            var tbTag = string.Format("<table xmlns=\"{0}\">", XMLNamespace);
                            position = xml.IndexOf(tbTag, endPosition + 3);
                            endPosition = xml.IndexOf("</table>", position);
                            param.HasTable = true;
                            var table = new XmlTable(grandchild.InnerXml, position, endPosition, XMLNamespace);
                            param.XmlTables.Add(table);

                        }

                    }


                    Parameters.Add(param);
                }

                if (child.Name == "statement")
                {
                    _rollingID++;
                    var statement = new Statement();
                    statement.StatementID = child.Attributes[0].Value;
                    statement.Value = child.InnerText;
                    statement.XPath = FindDeepestPath(child);
                    statement.XmlTables = new List<XmlTable>();
                    XPathIDs.Add(_rollingID);

                    var xml = child.ChildNodes[0].InnerXml;
                    statement.InnerXml = xml;
                    int position = 0, endPosition = 0;

                    foreach (XmlNode grandchild in child.ChildNodes[0].ChildNodes)
                    {
                        if (grandchild.Name == "table")
                        {
                            var tbTag = string.Format("<table xmlns=\"{0}\">", XMLNamespace);
                            position = xml.IndexOf(tbTag, endPosition + 3);
                            endPosition = xml.IndexOf("</table>", position);
                            statement.HasTable = true;
                            var table = new XmlTable(grandchild.InnerXml, position, endPosition, XMLNamespace);
                            statement.XmlTables.Add(table);

                        }

                    }

                    Statements.Add(statement);
                }
            }

        }
    }


    public struct ResponsibleRole
    {
        public string Name;
        public string Value;
        public string XPath;
    }


    public struct Property
    {
        public string Name;
        public string Value;
        public string XPath;
    }


    public struct DocControl
    {
        public string ControlID { get; set; }
        public string ResponsibleRole { get; set; }
        public string Parameters { get; set; }
        public string ImplementationStatus { get; set; }
        public string ControlOrigination { get; set; }
        public string Statements { get; set; }
    }

    public enum ImplementationStatus
    {
        Implemented,
        PartiallyImplemented,
        Planned,
        AlternativeImplementation,
        NotApplicable

    }

    public enum ControlOrigination
    {
        ServiceProviderCorporate,
        ServiceProviderSystemSpecific,
        ServiceProviderHybrid,
        ConfiguredByCustomer,
        ProvidedByCustomer,
        Shared,
        Inherited

    }

    public struct Parameter
    {
        public string ParamID;
        public string Value;
        public string XPath;
        public string InnerXml;
        public bool HasTable;
        public List<XmlTable> XmlTables;
    }

    public struct Statement
    {
        public string StatementID;
        public string Value;
        public string XPath;
        public string InnerXml;
        public bool HasTable;
        public List<XmlTable> XmlTables;
    }

    public struct ControlParamStatement
    {
        public string ControlID;
        public List<string> ParamIDs;
        public List<string> StatementIDs;

    }



}
