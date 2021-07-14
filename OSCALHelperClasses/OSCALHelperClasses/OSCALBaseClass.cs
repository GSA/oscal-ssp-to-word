using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Text;
using SqlDataProvider;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Schema;
namespace OSCALHelperClasses
{
    public abstract class OSCALBaseClass
    {

        protected string ConnString;
        protected string ASAPConnString;
        protected CSharpDAL DAL;
        protected CSharpDAL ASAPDAL;
        public static Dictionary<string, int> ImplementationStatusDict;
        public static Dictionary<string, int> OriginationStatusDict;
        public const uint startingPropId = 4063805526; //4063805526
        public const uint startingTextId = 1783144542;
        // public ControlCollection Controls;

        public static string FindXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        builder.Insert(0, "/ns:" + node.Name + "[" + index + "]");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        {
                            var tp = builder.ToString();
                            int first = tp.IndexOf("/");
                            tp = tp.Remove(first, 1);
                            return tp;
                        }
                    case XmlNodeType.Comment:
                        int indexC = FindElementIndex((XmlComment)node);
                        builder.Insert(0, "/ns:" + node.Name + "[" + indexC + "]");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Text:
                        int indexT = FindElementIndex((XmlText)node);
                        builder.Insert(0, "/ns:" + node.Name + "[" + indexT + "]");
                        node = node.ParentNode;
                        break;
                    default:
                        //throw new ArgumentException("Only elements and attributes are supported");
                        return "";
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        public static List<SecurityControl> GetSecurityControls(string xmlFile, string xmlSchema, string XMLNamespace)
        {
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add(XMLNamespace, xmlSchema);
            schema.Compile();

            DataSet dataSet = new DataSet();
            dataSet.ReadXml(xmlFile, XmlReadMode.ReadSchema);
            XmlDocument doc = new XmlDocument();
            doc.Schemas = schema;
            doc.Load(xmlFile);

            var securityControls = new List<SecurityControl>();

            foreach (XmlNode node in doc.DocumentElement)  //XmlNode  doc.DocumentElement  Office.CustomXMLNode
            {
                if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Comment)
                    continue;


                if (node.Attributes != null && node.Attributes.Count > 0 && (node.Attributes[0].Name == "control-id" || node.Attributes[0].Name == "statement-id"))
                {
                    var control = new SecurityControl(node, 0);
                    if (control.ControlId != null)
                        securityControls.Add(control);
                }
            }

            return securityControls;
        }

        public static void BuildEntireCustomXMLNodes(string XmlFile, string OscalSchemaPath, string XMLNamespace, ref Metadata metadata,
                                                      ref SystemCharacteristics systemCharacteristics, List<SecurityControl> securityControls, Dictionary<int, string> nodeIdToXPath, List<string> xPaths)
        {
            string output = @"..\..\Trace\TraceDump.txt";

            string controlOutput = @"..\..\Trace\ControlDump.txt";

            string mapcontrolOutput = @"..\..\Trace\MappedSecurityControl.txt";



            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add(XMLNamespace, OscalSchemaPath);
            schema.Compile();



            DataSet dataSet = new DataSet();
            dataSet.ReadXml(XmlFile, XmlReadMode.ReadSchema);
            XmlDocument doc = new XmlDocument();
            doc.Schemas = schema;
            doc.Load(XmlFile);

            StreamWriter sw4 = new StreamWriter(controlOutput);

            StreamWriter sw5 = new StreamWriter(mapcontrolOutput);

            StreamWriter sw3 = new StreamWriter(output);

            foreach (XmlNode node in doc.DocumentElement)
            {
                if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Comment)
                    continue;

                if (node.Name == "metadata")
                    metadata = new Metadata(node,true);

                if (node.Name == "system-characteristics")
                    systemCharacteristics = new SystemCharacteristics(node, true);

                int index = 0;
                if (node.NodeType == XmlNodeType.Element)
                {
                    index = FindElementIndex((XmlElement)node);
                }
                if (node.NodeType == XmlNodeType.Comment)
                {
                    index = FindElementIndex((XmlComment)node);
                }
                if (node.NodeType == XmlNodeType.Text)
                {
                    index = FindElementIndex((XmlText)node);
                }
                string XPath = FindXPath(node);

                if (node.Attributes != null && node.Attributes.Count > 0 && (node.Attributes[0].Name == "control-id" || node.Attributes[0].Name == "statement-id" || (node.Attributes.Count > 1 && node.Attributes[1].Name == "control-id") ))
                {
                    var control = new SecurityControl(node, 0, XMLNamespace);
                    if (control.ControlId != null)
                        securityControls.Add(control);

                    string controlXPath = FindControlXPath(node);
                    sw4.WriteLine(string.Format("Node Id={3} Node index={0}: Node type={1}: Node XPath={2} ", index, node.NodeType, XPath, nodeIdToXPath.Count));
                }

                nodeIdToXPath.Add(nodeIdToXPath.Count, XPath);



                xPaths.Add(XPath);

                sw3.WriteLine(string.Format("Node Id={3} Node index={0}: Node type={1}: Node XPath={2} ", index, node.NodeType, XPath, nodeIdToXPath.Count));



                sw3.WriteLine(string.Format("Node index={0}: Node type={1}: Node XPath={2} ", index, node.NodeType, XPath));



                ProcessChildren(node, sw3, sw4, securityControls, nodeIdToXPath, xPaths, XMLNamespace);


            }


            sw3.Close();
            sw4.Close();
            sw5.Close();

        }


        protected ElementTypeId GetElementTypeId(Type eltTypeId)
        {
            ElementTypeId realEltTypeId;

            if (eltTypeId == typeof(String))
                realEltTypeId = ElementTypeId.String;
            else
            {
                if (eltTypeId == typeof(Int32))
                    realEltTypeId = ElementTypeId.Int;
                else
                    realEltTypeId = ElementTypeId.XML;
            }

            return realEltTypeId;
        }





        DataSet AddElementHeaderAndValue(int deid, int doid, string eltValue, int uid, int active)
        {




            //Create Parameters
            SqlParameter[] oParams = new SqlParameter[5];
            oParams[0] = new SqlParameter("USERID", SqlDbType.Int, 10);
            oParams[0].Value = uid;
            oParams[1] = new SqlParameter("DOID", SqlDbType.Int, 10);
            oParams[1].Value = doid; //OSCAL-SSP
            oParams[2] = new SqlParameter("ElementValue", SqlDbType.NVarChar, 255);
            oParams[2].Value = eltValue;


            oParams[3] = new SqlParameter("DEID", SqlDbType.Int, 10);
            oParams[3].Value = deid;

            oParams[4] = new SqlParameter("Active", SqlDbType.Int, 10);
            oParams[4].Value = active;

            //Execute Procedure With Parameters
            //Fill DataSet
            DataSet _ds = DAL.FillDataset("[OWT_DEV].[dbo].[OWT_DocElement_Put]", CommandType.StoredProcedure, oParams);


            return _ds;
        }

      
        public static void ProcessChildren(XmlNode node, StreamWriter sw3, StreamWriter sw4, List<SecurityControl> SecurityControls, Dictionary<int, string> NodeIdToXPath, List<string> XPaths, string XMLNamespace)
        {
            if (!node.HasChildNodes)
            {
                return;
            }

            int index = 0;
            foreach (XmlNode child in node.ChildNodes)
            {
                //  var customPart = BuildCustomXMLNode(child);
                if (child.NodeType == XmlNodeType.Comment || child.NodeType == XmlNodeType.Text)
                    continue;

                if (child.NodeType == XmlNodeType.Element)
                {
                    index = FindElementIndex((XmlElement)child);
                }
                if (child.NodeType == XmlNodeType.Comment)
                {
                    index = FindElementIndex((XmlComment)child);
                }
                if (child.NodeType == XmlNodeType.Text)
                {
                    index = FindElementIndex((XmlText)child);
                }
                string XPath = FindXPath(child);

                if (child.Attributes != null && child.Attributes.Count > 0 && (child.Attributes[0].Name == "control-id" || child.Attributes[0].Name == "statement-id" || (child.Attributes.Count > 1  && child.Attributes[1].Name == "control-id")))
                {
                    var control = new SecurityControl(child, NodeIdToXPath.Count, XMLNamespace);
                    if (control.ControlId != null)
                        SecurityControls.Add(control);
                    string controlXPath = OSCALBaseClass.FindControlXPath(child);
                    sw4.WriteLine(string.Format("Node Id={3} Node index={0}: Node type={1}: Node XPath={2} ", index, child.NodeType, controlXPath, NodeIdToXPath.Count));
                }


                NodeIdToXPath.Add(NodeIdToXPath.Count, XPath);

                XPaths.Add(XPath);

                sw3.WriteLine(string.Format("Node Id={3} Node index={0}: Node type={1}: Node XPath={2} ", index, child.NodeType, XPath, NodeIdToXPath.Count));





                ProcessChildren(child, sw3, sw4, SecurityControls, NodeIdToXPath, XPaths, XMLNamespace);


            }
        }

        public static string RemoveTag(string expression)
        {
            while(expression.IndexOf("<")>0)
            {
                var start = expression.IndexOf("<");
                var end = expression.IndexOf(">");
                var dist = Math.Min(expression.Length - start+1,  end -start+1);
                if (dist <= 0)
                    break;

                var tag = expression.Substring(start, dist);
                expression = expression.Replace(tag, "");

            }

            while (expression.IndexOf("</") > 0)
            {
                var start = expression.IndexOf("</");
                var end = expression.IndexOf(">");
                var dist = Math.Min(expression.Length - start, start - end);

                if (dist <= 0)
                    break;

                var tag = expression.Substring(start, dist);
                expression = expression.Replace(tag, "");

            }

            expression = expression.Replace("<", "");
            expression = expression.Replace(">", "");

            return expression;
        }

        public string FindDeepestPath(XmlNode node)
        {
            var result = FindDeepestChild(node);
            var path = FindXPath(result);
            return path;

        }

        public XmlNode FindDeepestChild(XmlNode node)
        {
            XmlNode result = null;
            if (!node.HasChildNodes)
            {
                if (node.NodeType == XmlNodeType.Comment || node.NodeType == XmlNodeType.Text)
                {
                    if (node.PreviousSibling != null && node.PreviousSibling.NodeType != XmlNodeType.Comment && node.PreviousSibling.NodeType != XmlNodeType.Text)
                        return node.PreviousSibling;

                    return node.ParentNode;
                }
                else
                    return node;
            }
            else
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    //if (child.Name == "p")
                    //    return child.ParentNode;

                    result = FindDeepestChild(child);
                }

                return result;
            }
        }

        protected string GetStringTable(string xmlTable)
        {
            var result = "";



            return result;
        }

        public static void RemoveEntries(string inputFile, string outputFile)
        {
            bool done = false;
            var sw = new StreamWriter(outputFile);
            using(var sr =  new StreamReader(inputFile))
            {
               while(!sr.EndOfStream)
               {
                    var line = sr.ReadLine();
                    var index = line.IndexOf(">");
                    var end = line.IndexOf("</");
                    var gog = "";
                    if ( index<line.Length-1 && index >0 && end >0 && index<end)
                    {
                        var entrie = line.Substring(index + 1, end - index - 1);
                        if(entrie.Length > 0)
                            gog = line.Replace(entrie, "");

                        done = true;
                        sw.WriteLine(gog);
                    }
                    if(index>0 && end<0)
                    {
                        var begin = line.Substring(0, index + 1);
                        sw.WriteLine(begin);
                    }
                    if (end > 0 && end<index )
                    {
                        var ending = line.Substring(end);
                        var n = line.Length - end;
                        var empty = Space(Math.Max(n, 0));
                        sw.WriteLine(empty+ending);
                    }


                }
            }

            sw.Close();
                      
        }

        static string Space(int n)
        {
            var end = "";
            for(int i=0; i<n; i++)
            {
                end += " ";
            }
            return end;
        }
        public static void CollapseDescriptionAndRemoveTable(string inputFilePath, string outputFilePath)
        {
            var fileString = "";
            using (StreamReader sr = new StreamReader(inputFilePath))
            {
                fileString = sr.ReadToEnd();
                sr.Close();
            }

            using (StreamWriter sw = new StreamWriter(outputFilePath))
            {
                while (fileString.IndexOf("<description>") >= 0)
                {

                    int start = fileString.IndexOf("<description>");
                    int end = fileString.IndexOf("</description>", start);
                    var tem = fileString.Substring(start, end - start + 14);
                    fileString = fileString.Remove(start, end - start + 14);
                    tem = tem.Replace("<p>", "");
                    tem = tem.Replace("</p>", "\n");
                    tem = tem.Replace(" <p/>", "");
                    tem = tem.Replace("<ul>", "");
                    tem = tem.Replace("</ul>", "");

                    tem = tem.Replace("<li>", "");
                    tem = tem.Replace("</li>", "");

                    tem = tem.Replace("<h1>", "");
                    tem = tem.Replace("</h1>", "");

                    tem = tem.Replace("<h2>", "");
                    tem = tem.Replace("</h2>", "");

                    tem = tem.Replace("<h3>", "");
                    tem = tem.Replace("</h3>", "");

                    tem = tem.Replace("<h4>", "");
                    tem = tem.Replace("</h4>", "");

                    tem = tem.Replace("<h5>", "");
                    tem = tem.Replace("</h5>", "");

                    tem = tem.Replace("<description>", "<pan>");
                    tem = tem.Replace("</description>", "</pan>");
                    fileString = fileString.Insert(start, tem);

                }
                //fileString = fileString.Replace("<p>", "");
                //fileString = fileString.Replace("</p>", "");
                fileString = fileString.Replace("<pan>", "<description> <p>"); ///"<description> \n <p>
                fileString = fileString.Replace("</pan>", "</p> </description>");  //</p> \n </description>

                //fileString = fileString.Replace("<remarks>", "<remarks> <p>");
                //fileString = fileString.Replace("</remarks>", "</p> </remarks>");

                while (fileString.IndexOf("<table>") >= 0)
                {
                    int start = fileString.IndexOf("<table>");
                    int end = fileString.IndexOf("</table>", start);
                    // var tem = fileString.Substring(start, end - start + 8);
                    // var table = new XmlTable(tem);
                    // var tableString = table.BuildTableString();


                    fileString = fileString.Remove(start, end - start + 8);

                    // fileString = fileString.Insert(start, tableString);                                                              


                }

                sw.Write(fileString);
                sw.Close();
            }
        }

        public List<string> FindNodeWrapper(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builder2 = new StringBuilder();
            List<string> result = new List<string>();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "<@" + node.Name + ">");
                        builder2.Insert(0, "</@" + node.Name + ">");
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:

                        builder.Insert(0, "<" + node.Name + ">");
                        builder2.Insert(0, "</" + node.Name + ">");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        result.Add(builder.ToString());
                        result.Add(builder2.ToString());
                        return result;
                    case XmlNodeType.Comment:
                        builder.Insert(0, "<" + node.Name + ">");
                        builder2.Insert(0, "</" + node.Name + ">");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Text:

                        builder.Insert(0, "<" + node.Name + ">");
                        builder2.Insert(0, "</" + node.Name + ">");
                        node = node.ParentNode;
                        break;
                    default:
                        //throw new ArgumentException("Only elements and attributes are supported");
                        return new List<string> { "", "" };
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        public static string FindXPathWithoutIndex(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:

                        builder.Insert(0, "/" + node.Name);
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString();
                    case XmlNodeType.Comment:

                        builder.Insert(0, "/" + node.Name);
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Text:

                        builder.Insert(0, "/" + node.Name);
                        node = node.ParentNode;
                        break;
                    default:
                        //throw new ArgumentException("Only elements and attributes are supported");
                        return "";
                }
            }
            throw new ArgumentException("Node was not in a document");
        }




        public static void InitMetadataIndex(List<int> MetadataIndex)
        {

            string indexPath = @"..\..\MetadataIndex.txt";

            using (StreamReader sr = new StreamReader(indexPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    int index = int.Parse(line);
                    MetadataIndex.Add(index);
                }
                sr.Close();
            }
        }

        public static string FindControlXPath(XmlNode node)
        {
            StringBuilder builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        break;
                    case XmlNodeType.Element:
                        int index = FindElementIndex((XmlElement)node);
                        if (node.Attributes != null && node.Attributes.Count > 0)
                            builder.Insert(0, "/ns:" + node.Name + " " + node.Attributes[0].Value + "[" + index + "]");
                        else
                            builder.Insert(0, "/ns:" + node.Name + "[" + index + "]");

                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        {
                            var tp = builder.ToString();
                            int first = tp.IndexOf("/");
                            tp = tp.Remove(first, 1);
                            return tp;
                        }
                    case XmlNodeType.Comment:
                        int indexC = FindElementIndex((XmlComment)node);
                        builder.Insert(0, "/ns:" + node.Name + "[" + indexC + "]");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Text:
                        int indexT = FindElementIndex((XmlText)node);
                        builder.Insert(0, "/ns:" + node.Name + "[" + indexT + "]");
                        node = node.ParentNode;
                        break;
                    default:
                        //throw new ArgumentException("Only elements and attributes are supported");
                        return "";
                }
            }
            throw new ArgumentException("Node was not in a document");
        }

        public static int FindElementIndex(XmlElement element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlElement && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

        public static int FindElementIndex(XmlComment element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlComment && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

        public static int FindElementIndex(XmlText element)
        {
            XmlNode parentNode = element.ParentNode;
            if (parentNode is XmlDocument)
            {
                return 1;
            }
            XmlElement parent = (XmlElement)parentNode;
            int index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate is XmlText && candidate.Name == element.Name)
                {
                    if (candidate == element)
                    {
                        return index;
                    }
                    index++;
                }
            }
            throw new ArgumentException("Couldn't find element within parent");
        }

        public static List<string> Components(string XPath)
        {
            var result = new List<string>();
            while (XPath.Contains("/"))
            {
                int i = XPath.LastIndexOf("/");

                if (i < 0)
                    return result;


                var nber = XPath.Length - i - 1;
                var man = XPath.Substring(i + 1, nber);
                result.Add(man);
                XPath = XPath.Remove(i);
            }

            return result;
        }

        public static int FindCorrespondingXPath(string title, string tag, Dictionary<int, string> NodeIdToXPath)
        {
#pragma warning disable CS0219 // The variable 'repeated' is assigned but its value is never used
            bool repeated = false;
#pragma warning restore CS0219 // The variable 'repeated' is assigned but its value is never used
            ///Key variant
            ///
            ///
            if (tag == "company")
            {
                tag = "org-name";
                repeated = true;
            }
            if (title == "Company/Organization")
            {
                title = "org";
                tag = "short";
                repeated = true;
            }

            if (tag == "revisiondate")
            {
                tag = "publication-date";
                repeated = true;
            }

            if (tag == "cspname")
            {
                tag = "short";
                repeated = true;
            }

            if (tag == "informationsystemname")
            {
                title = "system-characteristics1system-name1";
                repeated = true;
            }

            if (tag == "informationsystemabbreviation")
            {
                title = "system-characteristics1system-name-short1";
                repeated = true;
            }

            if (tag == "versiondate")
            {
                title = "metadata1last-modified1";
                repeated = true;
            }

            if (tag == "versionnumber")
            {
                tag = "version";
                repeated = true;
            }

            if (tag == "approvaldate")
            {
                tag = "publication-date";
                repeated = true;
            }

            if (title == "title")
            {
                tag = "metadata[1]/ns:title[1] ";
                repeated = true;
            }
            if (tag == "company")
            {
                // tag = "org-name";
                tag = "metadata[1]/party[1]/ns:party-name[1]";
                repeated = true;
            }
            if (title == "Company/Organization")
            {
                title = "org";
                tag = "short";
                tag = "metadata[1]/ns:party[1]/ns:name[1]";
                repeated = true;
            }

            if (tag == "revisiondate")
            {
                title = "metadata[1]/ns:last-modified[1]";
                repeated = true;
            }

            if (tag == "cspname")
            {
                //tag = "short";
                tag = "metadata[1]/ns:party[1]/ns:short-name[1]";
                repeated = true;
            }



            for (int i = 0; i < NodeIdToXPath.Keys.Count; i++)
            {
                //if (MappedIndices.Contains(i))
                //    continue;

                var xpath = NodeIdToXPath[i];

                var cleantitle = xpath.Replace("ns:system-security-plan[1]/ns:", "");

                cleantitle = cleantitle.Replace("/ns:", "");
                cleantitle = cleantitle.Replace("[", "");
                cleantitle = cleantitle.Replace("]", "");

                if (tag == "CityStateZip1" && ((title == "metadata1party1org1address1city1") || (title == "metadata1party1org1address1state1 ") || (title == "metadata1party1org1address1postal-code1")))
                {


                    return i;
                }

                if (cleantitle == title)
                {


                    return i;
                }

                //if (!xpath.Contains("/#text[1]"))
                //    continue;
                var components = Components(xpath);
                foreach (var x in components)
                {
                    if (x.Contains(title))  //||x.Contains(tag)
                    {


                        return i;
                    }

                    if (tag != null && tag.Length > 0 && x.Contains(tag))
                    {


                        return (i);
                    }

                }

            }

            return -1;
        }

    }

    public class ElementHeader
    {
        public string Name { get; set; }
        public ElementTypeId TypeId { get; set; }
        public string Tag { get; set; }
        public string Desc { get; set; }

        public int Active { get; set; }

        public string Detail { get; set; }

        public ElementHeader(string name, ElementTypeId typeId, string tag, string desc, string detail, int active)
        {
            Name = name;
            TypeId = typeId;
            Tag = tag;
            Desc = desc;
            Active = active;
            Detail = detail;
        }

    }

    public enum ElementTypeId
    {
        String = 1,
        Int = 2,
        XML = 5,

    }
}
