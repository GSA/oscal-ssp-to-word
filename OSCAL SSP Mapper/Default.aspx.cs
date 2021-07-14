using DocumentFormat.OpenXml.Packaging;
using OSCALHelperClasses;
using Microsoft.Office.Tools.Word;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Schema;
using Word = Microsoft.Office.Interop.Word;
//using AWord = Aspose.Words;
using System.Text;
//using AsposeWord = Aspose.Words;
//using Aspose.Words.Markup;
//using Aspose.Words;

namespace OSCAL_SSP_Mapper
{
    public partial class _Default : Page
    {
        public const string XMLNamespace = @"http://csrc.nist.gov/ns/oscal/1.0";
        public const string SSPschema = "oscal_ssp_schema.xsd";
        protected Dictionary<string, int> ImplementationStatusDict;
        protected Dictionary<string, int> OriginationStatusDict;
        protected Dictionary<string, int> OriginationStatusShortDict;
        public List<SecurityControl> SecurityControls;
        public List<IndicatorRank> TableIndicators;

        //Tables functionality will be implemented later
        public List<Word.Table> DefaultTables;
        public Word.Document Document;

        //public AsposeWord.Document Document;

        public Word.Application App;
        public string CurrentTableName;
        public string WordTempFilePath;
        public string TemplateFile = "FedRAMP-SSP-Moderate-Baseline-Template.docx";
        public string BaselinePropCountFile = "ModerateBaselineControlsToPropCount.txt";
        protected private bool OverwriteXMLMapping;
        string message = "";
        int percent = 0;
        delegate string ProcessTask(string id);
        CurrentTask longRunningClass = new CurrentTask();
        string ProcessingPage;
        string Filename;


        protected void Page_Load(object sender, EventArgs e)
        {
      
             InitPropertyStatusDictionaries();
          
        }

        protected void UploadButton_Click(object sender, EventArgs e)
        {
            // Tom 07-01-2021
            var convertPageTop = HttpContext.Current.Server.MapPath(string.Format(@"~\Templates\{0}", "Processing2.html"));
            StreamReader sr = new StreamReader(convertPageTop);

            ProcessingPage = sr.ReadToEnd();
            sr.Close();

            string myXMLElement = "";
            StatusLabel1.Text = "";
            OpenMyFile.Visible = false;

            if (FileUpload1.HasFile)
            {
                try
                {
                    string filename = Path.GetFileName(FileUpload1.FileName);

                    Filename = filename;
                    message = string.Format("Starting the conversion to Word of the OSCAL SSP {0}", filename);
                    percent = 2;
                    PrintProgressBar(message, percent, true);
                    CollapseDiv("mainForm");
                    string fileExtension = System.IO.Path.GetExtension(FileUpload1.FileName).ToLower();
                    if (fileExtension != ".xml")
                    {
                        StatusLabel1.ForeColor = System.Drawing.Color.Red;
                        StatusLabel1.Text = "Invalid File Extension - Not an XML File!";
                    }
                    else
                    {

                        FileUpload1.SaveAs(Server.MapPath("~/Uploads/") + filename);
                        StatusLabel1.ForeColor = System.Drawing.Color.Green;
                        StatusLabel1.Text = "Upload status: File sucessfully uploaded...  Processing File...Please stand by.";
                        myXMLElement = GetXMLElement(filename, "security-sensitivity-level");
                        if (myXMLElement == "low")
                        {
                            TemplateFile = "FedRAMP-SSP-Low-Baseline-Template.docx";
                            BaselinePropCountFile = "LowBaselineControlsToPropCount.txt";
                        }
                        else if (myXMLElement == "high")
                        {
                            TemplateFile = "FedRAMP-SSP-High-Baseline-Template.docx";
                            BaselinePropCountFile = "HighBaselineControlsToPropCount.txt";
                        }

                        string wordDocumentPath;
                        string xmlSchemaPath = HttpContext.Current.Server.MapPath(string.Format(@"~\Templates\{0}", SSPschema));

                        PseudoValidator(filename, xmlSchemaPath);
                        message = string.Format("Conversion of {0}: Successful validation of the xml file  {0} against OSCAL schema with namespace {1}",Filename,XMLNamespace); 
                        percent = 10;
                        PrintProgressBar(message, percent);


                        ProcessData(filename, TemplateFile, out wordDocumentPath);
                        //CollapseDiv("mainForm");
                        message = string.Format("Successfully mapped the Metadata, System Characteristics and System Implementation Data using basic XML Mappings");
                        percent = 20;
                        PrintProgressBar(message, percent);

                        ProcessWordDocument(wordDocumentPath, filename, BaselinePropCountFile);

                        StatusLabel1.Text = "Processing Complete.. Click below to open file.";
                        
                        OpenMyFile.Visible = true;
                        Cache["outputFile"] = TemplateFile;
                       
                    }
                }
                catch (Exception ex)
                {


                    StatusLabel1.ForeColor = System.Drawing.Color.Red;
                    StatusLabel1.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;



                    if (App != null)
                    {
                       // Document.Close();
                        App.Quit();                   
                    }

                    if (File.Exists(WordTempFilePath))
                    {
                        File.Delete(WordTempFilePath);
                    }

                }

                
            }

        


        }

      

        private void CollapseDiv(string entity)
        {
            var sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append(string.Format("document.getElementById(\"{0}\").style.display='none';",entity));
            sb.Append("</script>");

            var update = sb.ToString();
            HttpContext.Current.Response.Write(update);

            HttpContext.Current.Response.Flush();

        }
        private void PrintProgressBar(string Message, int PercentComplete, bool first = false)
        {   
            var sb = new StringBuilder();
            sb.Append("<script>");
            var iis = string.Format("\"{0}%\"", PercentComplete);
            sb.AppendLine(string.Format("myFunction(\"{0}\",{1})", Message, iis));
            sb.Append("</script>");

            var file = "";
            var update = sb.ToString();
            if (first)
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearContent();
                file = ProcessingPage + update;  //// 
                HttpContext.Current.Response.Write(file);
            }
            else
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.ClearContent();
                file = update;
            }
            HttpContext.Current.Response.Write(file);

            HttpContext.Current.Response.Flush();
        }

        public static void ProcessData(string ssp_file, string word_template_file, out string wordDocumentPath)
        {
            string xmlDataFile = HttpContext.Current.Server.MapPath(string.Format(@"~\Uploads\{0}", ssp_file));
            string templateDocument = HttpContext.Current.Server.MapPath(string.Format(@"~/Templates/{0}", word_template_file));
            string tempDocument = HttpContext.Current.Server.MapPath(string.Format(@"~/Downloads/{0}", "MyGeneratedDocument.docx"));
            string outputDocument = HttpContext.Current.Server.MapPath(string.Format(@"~/Downloads/{0}", word_template_file.Replace("Template", "OSCAL")));

            if (File.Exists(tempDocument))
            {
                File.Delete(tempDocument);
            }
            File.Copy(templateDocument, tempDocument);

            if (File.Exists(outputDocument))
            {
                File.Delete(outputDocument);
            }

            string xmlDataFileWithoutTable = HttpContext.Current.Server.MapPath(string.Format(@"~\Uploads\{0}", "workingFile.xml"));
            OSCALBaseClass.CollapseDescriptionAndRemoveTable(xmlDataFile, xmlDataFileWithoutTable);

            //   using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(templateDocument, true))
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(tempDocument, true))
            {
                //get the main part of the document which contains CustomXMLParts
                MainDocumentPart mainPart = wordDoc.MainDocumentPart;

                //delete all CustomXMLParts in the document. If needed only specific CustomXMLParts can be deleted using the CustomXmlParts IEnumerable
                mainPart.DeleteParts<CustomXmlPart>(mainPart.CustomXmlParts);

                //add new CustomXMLPart with data from new XML file
                CustomXmlPart myXmlPart = mainPart.AddCustomXmlPart(CustomXmlPartType.CustomXml);
                using (FileStream stream = new FileStream(xmlDataFileWithoutTable, FileMode.Open))
                {
                    myXmlPart.FeedData(stream);

                }
            }

            File.Copy(tempDocument, outputDocument);
            File.Delete(tempDocument);
            wordDocumentPath = outputDocument;


        }
        public string GetXMLElement(string XMLFileName, string ElementName)
        {
            string xmlDataFile = HttpContext.Current.Server.MapPath(string.Format(@"~\Uploads\{0}", XMLFileName));
            string bytes = File.ReadAllText(xmlDataFile);
            System.IO.MemoryStream myStream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(bytes));
            string myElementValue = "";
            System.Xml.XmlReader xr = System.Xml.XmlReader.Create(myStream);
            while (xr.Read())
            {
                if (xr.NodeType == System.Xml.XmlNodeType.Element)
                    if (xr.Name == ElementName.ToString())
                    {
                        myElementValue = xr.ReadElementContentAsString();
                        break;
                    }
            }

            return (myElementValue);
        }

        public List<Word.ContentControl> GetStatusCheckBoxes(Word.ContentControls Controls)
        {
            var baselineCheckControls = new List<Word.ContentControl>();
            var totalControls = new List<Word.ContentControl>();
            foreach (Word.ContentControl u in Controls)
            {
                totalControls.Add(u);
            }

            for (int i = 0; i < totalControls.Count; i++)
            {

                var control = totalControls[i];
                var title = control.Title;
               
                if (title != null && title.Contains("status"))
                {
                    control.Checked = false;
                    baselineCheckControls.Add(control); 
                }

            }
          
            return baselineCheckControls;
        }


 


        public List<SecurityControl> GetSecurityControls(string xmlFile, string xmlSchema, string XMLNamespace, out SystemCharacteristics systemCharacteristics)
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
            systemCharacteristics = new SystemCharacteristics();
            foreach (XmlNode node in doc.DocumentElement)  //XmlNode  doc.DocumentElement  Office.CustomXMLNode
            {

                if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Comment)
                    continue;
                if (node.Name == "system-characteristics")
                    systemCharacteristics = new SystemCharacteristics(node, true);
                if (node.Name == "control-implementation")
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Text || child.NodeType == XmlNodeType.Comment)
                            continue;

                        if (child.Attributes != null && child.Attributes.Count > 0 && (child.Attributes[0].Name == "control-id" ||(child.Attributes.Count >= 2 && child.Attributes[1].Name == "control-id")))  ///|| child.Attributes[0].Name == "statement-id"
                        {
                            var control = new SecurityControl(child, 0, XMLNamespace);
                            if (control.ControlId != null)
                                securityControls.Add(control);
                        }

                    }
            }

            return securityControls;
        }

        public void FillPropertyCheckBoxes(List<Word.ContentControl> BaselineCheckControls, List<Word.ContentControl> StatusCheckControls, string xmlFile, string xmlSchema, string XMLNamespace, string securityIdPropFile)
        {

            string securityIdPropFilePath = HttpContext.Current.Server.MapPath(string.Format(@"~\Templates\{0}", securityIdPropFile));

            var securityIdToPropertyCount = new Dictionary<string, int>();
            var securityControlIdToRankBaseline = new Dictionary<string, int>();
            var rankBaselineToSecurityId = new Dictionary<int, string>();
            FileStream fs = new FileStream(securityIdPropFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);
            // StreamWriter sw = new StreamWriter(HttpContext.Current.Server.MapPath(string.Format(@"~\Dumptrace.txt")));


            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                int index = line.IndexOf(",");
                string id = line.Remove(index);
                int count;
                var temp = line.Substring(index + 1);
                int.TryParse(temp, out count);
                securityIdToPropertyCount.Add(id.ToLower(), count);
            }
            sr.Close();

            FileStream fs2 = new FileStream(securityIdPropFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sr2 = new StreamReader(fs2);

            int ct = 0;
            while (!sr2.EndOfStream)
            {
                var line = sr2.ReadLine();
                var index = line.IndexOf(",");
                string id = line.Remove(index);
                securityControlIdToRankBaseline.Add(id.ToLower(), ct);
                rankBaselineToSecurityId.Add(ct, id.ToLower());
                ct++;
            }

            sr2.Close();
            var systemCharacteristics = new SystemCharacteristics();
            SecurityControls = GetSecurityControls(xmlFile, xmlSchema, XMLNamespace, out systemCharacteristics);

            // int cting = 0;

            OverwriteXMLMapping = SecurityControls.Count < securityControlIdToRankBaseline.Count;

            var status = systemCharacteristics.Status.Value;
           

            if(status== "operational")
                    StatusCheckControls[0].Checked = true;
               
            if(status== "under-development")      
                    StatusCheckControls[1].Checked = true;

             if(status == "under-major-modification")
                    StatusCheckControls[2].Checked = true;
             if(status == "other")
                StatusCheckControls[3].Checked = true;
             
          

            for (int i = 0; i < SecurityControls.Count; i++)
            {
                var sc = SecurityControls[i];
                if (!securityControlIdToRankBaseline.ContainsKey(SecurityControls[i].ControlId))
                    continue;


                int rank = 0;
                var position = securityControlIdToRankBaseline[SecurityControls[i].ControlId];
                for (int j = 0; j < position; j++)
                {
                    rank += securityIdToPropertyCount[rankBaselineToSecurityId[j]];
                }

                Dictionary<string, int> originalStatusDict;

                if (securityIdToPropertyCount[sc.ControlId] == 11)
                    originalStatusDict = OriginationStatusShortDict;
                else
                    originalStatusDict = OriginationStatusDict;

                for (int k = 0; k < sc.Properties.Count; k++)
                {

                    if (sc.Properties[k].Name == "implementation-status")
                    {
                        if (!ImplementationStatusDict.ContainsKey(sc.Properties[k].Value))
                            continue;

                        var imp = ImplementationStatusDict[sc.Properties[k].Value];


                        int order = rank + imp;
                        BaselineCheckControls[order].Checked = true;
                        //sw.WriteLine(string.Format("{0} control= {4}  position= {2} rank= {3} imp= {5}  order= {1}", cting, order, position, rank, sc.ControlId, imp));
                        //cting++;

                    }

                    if (sc.Properties[k].Name == "control-origination")
                    {
                        if (!originalStatusDict.ContainsKey(sc.Properties[k].Value))
                            continue;

                        var imp = originalStatusDict[sc.Properties[k].Value];
                        int nber = securityIdToPropertyCount[sc.ControlId];
                        if (imp < nber)
                        {
                            int order2 = rank + imp;
                            BaselineCheckControls[order2].Checked = true;
                            //sw.WriteLine(string.Format("{0} control= {4} position= {2} rank= {3} imp= {5} order= {1}", cting, order2, position, rank, sc.ControlId, imp));
                            //cting++;
                        }
                    }
                }

            }
            // sw.Close();
        }

        protected void ProcessWordDocument(string wordDocumentPath, string uploadedFilename, string BaselinePropCountFile)
        {

            Guid guid = Guid.NewGuid();

            WordTempFilePath = HttpContext.Current.Server.MapPath(string.Format(@"~/Downloads/{0}Document.docx", guid.ToString()));


            File.Copy(wordDocumentPath, WordTempFilePath);

            message = string.Format("Conversion of {0}: Successfully mapped the Metadata, System Characteristics and System Implementation Data using basic XML Mappings. Opening the word document...", Filename);
            percent = 21;
            PrintProgressBar(message, percent);


            App = new Word.Application();
            Cache["1"] = " Opening Word Template Document";
            Document = App.Documents.Open(WordTempFilePath);


            message = string.Format("Conversion of {0}: Successfully opened Word Template Document fletching out controls", Filename);
            percent = 25;
            PrintProgressBar(message, percent);
            var Controls = Document.ContentControls;

            var checkControls = GetCheckBoxes(Controls);
            var statusControl = GetStatusCheckBoxes(Controls);
            var xmlFilePath = Server.MapPath("~/Uploads/") + uploadedFilename;
            message = string.Format("Conversion of {0}: Starting to fill all checkboxes controls", Filename);

            percent = 45;
            PrintProgressBar(message, percent);

            string xmlSchemaPath = HttpContext.Current.Server.MapPath(string.Format(@"~\Templates\{0}", SSPschema));
            FillPropertyCheckBoxes(checkControls, statusControl, xmlFilePath, xmlSchemaPath, XMLNamespace, BaselinePropCountFile);
            message = string.Format("Conversion of {0}: All control properties checkboxes have been properly filled. Starting to build tables ...", Filename);
            percent = 68;
            PrintProgressBar(message, percent);


            BuildTables();

            message = string.Format("Conversion of {0}: All tables  have been  successfully built. Adding multiple responsible roles where applicable ...", Filename);
            percent = 85;
            PrintProgressBar(message, percent);

            AddMultipleResponsibleRoles();

            if (OverwriteXMLMapping)
                OverWriteSecurityMapping();

            message = string.Format("Conversion of {0}: All multiple responsible roles have been properly inserted. Word conversion is almost complete", Filename);
            percent = 99;
            PrintProgressBar(message, percent);
            Document.Save();
            Document.Close();
            App.Quit();

            if (File.Exists(wordDocumentPath))
            {
                File.Delete(wordDocumentPath);
            }

            File.Copy(WordTempFilePath, wordDocumentPath);

            File.Delete(WordTempFilePath);
            mainbar.Style["width"] = "100%";
            CollapseDiv("maindiv");


        }
        public void InitPropertyStatusDictionaries()
        {
            ImplementationStatusDict = new Dictionary<string, int>();
            OriginationStatusDict = new Dictionary<string, int>();
            OriginationStatusShortDict = new Dictionary<string, int>();
            TableIndicators = new List<IndicatorRank>();

            ImplementationStatusDict.Add("implemented", 0);
            ImplementationStatusDict.Add("partially-implemented", 1);
            ImplementationStatusDict.Add("planned", 2);
            ImplementationStatusDict.Add("alternative-implementation", 3);
            ImplementationStatusDict.Add("not-applicable", 4);

            OriginationStatusDict.Add("service-provider-corporate", 5);
            OriginationStatusDict.Add("service-provider-system-specific", 6);
            OriginationStatusDict.Add("service-provider-hybrid", 7);
            OriginationStatusDict.Add("configured-by-customer", 8);
            OriginationStatusDict.Add("provided-by-customer", 9);
            OriginationStatusDict.Add("shared", 10);
            OriginationStatusDict.Add("inherited", 11);

            OriginationStatusShortDict.Add("service-provider-corporate", 5);
            OriginationStatusShortDict.Add("service-provider-system-specific", 6);
            OriginationStatusShortDict.Add("service-provider-hybrid", 7);

            OriginationStatusShortDict.Add("provided-by-customer", 8);
            OriginationStatusShortDict.Add("shared", 9);
            OriginationStatusShortDict.Add("inherited", 10);

        }

        public IndicatorRank FindTheCorrespondingTable(string controlId)
        {
            var result = new IndicatorRank
            {
                Rank = -1,
                Indicator = ""
            };

            for (int k = 33; k < TableIndicators.Count; k++)
            {
                var indRank = TableIndicators[k];
                var ind = indRank.Indicator.ToLower();
                var pureInd = ind.Replace("\r\a", "");
                pureInd = pureInd.Replace("(", ".");
                pureInd = pureInd.Replace(")", "");

                var rawInd = pureInd;

                pureInd = pureInd.Replace(" ", "");

                if (controlId == pureInd)
                {
                    result = TableIndicators[k];

                    break;
                }

                if (controlId == rawInd)
                {
                    result = TableIndicators[k];

                    break;
                }



            }
            return result;
        }
         public List<Word.ContentControl> GetCheckBoxes(Word.ContentControls Controls)
        {
            var baselineCheckControls = new List<Word.ContentControl>();
          

            var totalControls = new List<Word.ContentControl>();
            foreach (Word.ContentControl u in Controls)
            {
                totalControls.Add(u);
            }

            for (int i = 10; i < totalControls.Count; i++)
            {

                var control = totalControls[i];
                var title = control.Title;
                if (title != null && title.Length > 6)
                {
                    var sub = title.Substring(0, 5);
                    if (sub == "check")
                    {
                        baselineCheckControls.Add(control);
                    }
                }             

            }
          
            return baselineCheckControls;
        }

        public void BuildTables()
        {

            DefaultTables = new List<Word.Table>();
            foreach (Word.Table table in Document.Tables)
            {
                DefaultTables.Add(table);

            }

            for (int i = 0; i < DefaultTables.Count; i++)
            {

                var col = DefaultTables[i].Columns;
                var row = DefaultTables[i].Rows;
                if (row.Count >= 1 && col.Count >= 1)
                {
                    var a = DefaultTables[i].Cell(1, 1).Range.Text;
                    var ind = new IndicatorRank
                    {
                        Rank = i,
                        Indicator = a
                    };
                    TableIndicators.Add(ind);
                }

            }



            foreach (var sc in SecurityControls)
            {

                if (!sc.HasTables)
                    continue;

                IndicatorRank indicatorRank = FindTheCorrespondingTable(sc.ControlId.ToLower());

                if (indicatorRank.Rank >= 0)
                {
                    for (int i = 0; i < sc.Parameters.Count; i++)
                    {
                        var pureIndin = DefaultTables[indicatorRank.Rank].Cell(i + 1, 1).Range.Text;
                        pureIndin = pureIndin.Replace("\r\a", "");


                        if (sc.Parameters[i].HasTable)
                        {
                            CurrentTableName = sc.Parameters[i].ParamID;
                            var xmlTables = sc.Parameters[i].XmlTables;


                            var range = DefaultTables[indicatorRank.Rank].Cell(i + 3, 1).Range;
                            DeleteContentControls(range);

                            var mousrange = range;
                            var lastPosition = 0;
                            var initText = mousrange.Text;
                            var endTag = "</table>";
                            var value = sc.Parameters[i].InnerXml;
                            for (int j = 0; j < xmlTables.Count; j++)
                            {
                                var xmlTable = xmlTables[j];


                                var text = value.Substring(lastPosition, xmlTable.StartPosition - lastPosition);
                                text = OSCALBaseClass.RemoveTag("   " + text);


                                mousrange.InsertAfter(text);
                                lastPosition = xmlTable.EndPosition + endTag.Length;
                                mousrange.MoveStart(Word.WdUnits.wdCharacter, mousrange.Characters.Count - 2);
                                var xrange = mousrange;

                                var myTable = xrange.Tables.Add(xrange, xmlTable.Dimension.NumberRows, xmlTable.Dimension.NumberColumns);
                                myTable.AllowAutoFit = true;
                                myTable.AllowPageBreaks = true;
                                myTable.ApplyStyleLastRow = true;
                                myTable.ApplyStyleLastColumn = true;
                                myTable.Borders[Word.WdBorderType.wdBorderBottom].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderTop].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderVertical].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderLeft].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderRight].Visible = true;


                                for (int p = 0; p < xmlTable.Dimension.NumberRows; p++)
                                {
                                    for (int q = 0; q < xmlTable.Dimension.NumberColumns; q++)
                                    {
                                        if (xmlTable.Rows[p].RowElements.Count > q)
                                        {
                                            var val = xmlTable.Rows[p].RowElements[q].Value;

                                            myTable.Cell(p + 1, q + 1).Range.Text = val;
                                        }

                                    }

                                }

                                mousrange = xrange;

                            }

                            var lastText = "";
                            var count = value.Length - lastPosition;
                            if (count > 0)
                            {
                                lastText = value.Substring(lastPosition, count);
                                lastText = OSCALBaseClass.RemoveTag("   " + lastText);
                                mousrange.InsertAfter(lastText);
                            }

                        }


                    }
                }


                var longId = sc.ControlId.ToLower() + " What is the solution and how is it implemented?";

                indicatorRank = FindTheCorrespondingTable(longId.ToLower());



                if (indicatorRank.Rank >= 0)
                {
                    var rank = indicatorRank.Rank;

                    for (int i = 0; i < sc.Statements.Count; i++)
                    {
                        var pureIndin = DefaultTables[rank].Cell(i + 2, DefaultTables[rank].Columns.Count).Range.Text;
                        pureIndin = pureIndin.Replace("\r\a", "");


                        if (sc.Statements[i].HasTable)
                        {
                            CurrentTableName = sc.Statements[i].StatementID;
                            var xmlTables = sc.Statements[i].XmlTables;

                            var range = DefaultTables[rank].Cell(i + 2, DefaultTables[rank].Columns.Count).Range;
                            DeleteContentControls(range);
                            var lastPosition = 0;
                            var endTag = "</table>";
                            var mousrange = range;



                            var value = sc.Statements[i].InnerXml;
                            for (int j = 0; j < xmlTables.Count; j++)
                            {
                                var xmlTable = xmlTables[j];


                                var text = value.Substring(lastPosition, xmlTable.StartPosition - lastPosition);
                                text = OSCALBaseClass.RemoveTag("   " + text);


                                mousrange.InsertAfter(text);
                                lastPosition = xmlTable.EndPosition + endTag.Length;
                                mousrange.MoveStart(Word.WdUnits.wdCharacter, mousrange.Characters.Count - 2);
                                var xrange = mousrange;

                                var myTable = xrange.Tables.Add(xrange, xmlTable.Dimension.NumberRows, xmlTable.Dimension.NumberColumns);
                                myTable.AllowAutoFit = true;
                                myTable.AllowPageBreaks = true;
                                myTable.ApplyStyleLastRow = true;
                                myTable.ApplyStyleLastColumn = true;
                                myTable.Borders[Word.WdBorderType.wdBorderBottom].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderTop].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderHorizontal].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderVertical].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderLeft].Visible = true;
                                myTable.Borders[Word.WdBorderType.wdBorderRight].Visible = true;


                                for (int p = 0; p < xmlTable.Dimension.NumberRows; p++)
                                {
                                    for (int q = 0; q < xmlTable.Dimension.NumberColumns; q++)
                                    {
                                        if (xmlTable.Rows[p].RowElements.Count > q)
                                        {
                                            var val = xmlTable.Rows[p].RowElements[q].Value;

                                            myTable.Cell(p + 1, q + 1).Range.Text = val;
                                        }

                                    }

                                }

                                mousrange = xrange;

                            }

                            var lastText = "";
                            var count = value.Length - lastPosition;
                            if (count > 0)
                            {
                                lastText = value.Substring(lastPosition, count);
                                lastText = OSCALBaseClass.RemoveTag("   " + lastText);
                                mousrange.InsertAfter(lastText);
                            }
                        }

                    }
                }

            }
        }

        public void AddMultipleResponsibleRoles()
        {
            foreach (var sc in SecurityControls)
            {

                if (!sc.HasMultipleResponsibleRoles)
                    continue;

                IndicatorRank indicatorRank = FindTheCorrespondingTable(sc.ControlId.ToLower());
                var roles = sc.ResponsibleRoles[0].Value;

                if (indicatorRank.Rank >= 0)
                {
                    var range = DefaultTables[indicatorRank.Rank].Cell(2, 1).Range;
                    DeleteContentControls(range);

                    for (int i = 1; i < sc.ResponsibleRoles.Count; i++)
                    {
                        roles += ", " + sc.ResponsibleRoles[i].Value;
                    }

                    range.InsertAfter(roles);
                }
            }
        }

  void OverWriteSecurityMapping()
   {
       foreach (var sc in SecurityControls)
       {

           if (sc.HasTables)
               continue;

           IndicatorRank indicatorRank = FindTheCorrespondingTable(sc.ControlId.ToLower());

           if (indicatorRank.Rank >= 0)
           {
               if (!sc.HasMultipleResponsibleRoles && sc.ResponsibleRoles.Count > 0)
               {
                   var range = DefaultTables[indicatorRank.Rank].Cell(2, 1).Range;
                   DeleteContentControls(range);
                   range.InsertAfter(sc.ResponsibleRoles[0].Value);

               }

               for (int i = 0; i < sc.Parameters.Count; i++)
               {

                   var range = DefaultTables[indicatorRank.Rank].Cell(i + 3, 1).Range;
                   DeleteContentControls(range);

                   range.InsertAfter(sc.Parameters[i].Value);

               }
           }


           var longId = sc.ControlId.ToLower() + " What is the solution and how is it implemented?";

           indicatorRank = FindTheCorrespondingTable(longId.ToLower());

           if (indicatorRank.Rank >= 0)
           {
               var rank = indicatorRank.Rank;

               for (int i = 0; i < sc.Statements.Count; i++)
               {

                   var range = DefaultTables[rank].Cell(i + 2, DefaultTables[rank].Columns.Count).Range;
                   DeleteContentControls(range);
                   range.InsertAfter(sc.Statements[i].Value);

               }
           }


       }
   }

   public void StartLongRunningProcess(string id)
   {
       longRunningClass.Add(id);
       ProcessTask processTask = new ProcessTask(longRunningClass.ProcessLongRunningAction);
       processTask.BeginInvoke(id, new AsyncCallback(EndLongRunningProcess), processTask);
   }

   /// <summary>
   /// Ends the long running process.
   /// </summary>
   /// <param name="result">The result.</param>
   public void EndLongRunningProcess(IAsyncResult result)
   {
       ProcessTask processTask = (ProcessTask)result.AsyncState;
       string id = processTask.EndInvoke(result);
       longRunningClass.Remove(id);
   }

   /// <summary>
   /// Gets the current progress.
   /// </summary>
   /// <param name="id">The id.</param>
   public string GetCurrentProgress(string id)
   {
       // default.HttpContext.Response.AddHeader("cache-control", "no-cache");
       var currentProgress = longRunningClass.GetStatus(id).ToString();
       return currentProgress;
   }

   public void DeleteContentControls(Word.Range range)
   {
       var controls = new List<Word.ContentControl>();
       foreach (Word.ContentControl ct in range.ContentControls)
       {
           controls.Add(ct);
       }

       if (controls.Count > 0)
           controls[0].Delete(true);
   }
   protected void OpenMyFile_Click(object sender, EventArgs e)
   {
       TemplateFile = Cache["outputFile"].ToString(); ;
       Response.Redirect(string.Format(@"~/Downloads/{0}", TemplateFile.Replace("Template", "OSCAL")));
   }

   protected void HiddenButton_Click(object sender, EventArgs e)
   {
       Response.Redirect(Request.RawUrl);
   }

   public  void PseudoValidator(string XmlDocument, string XsdSchemaPath)
   {
       try
       {
          string XmlDocumentPath = HttpContext.Current.Server.MapPath(string.Format(@"~\Uploads\{0}", XmlDocument));

           XmlReaderSettings OscalSettings = new XmlReaderSettings();
           OscalSettings.Schemas.Add(XMLNamespace, XsdSchemaPath);
           OscalSettings.ValidationType = ValidationType.Schema;
           OscalSettings.ValidationEventHandler += new ValidationEventHandler(OSCALSettingsValidationEventHandler);
           XmlReader OscalDoc = XmlReader.Create(XmlDocumentPath, OscalSettings);

           while (OscalDoc.Read()) { }

           OscalDoc.Close();
          // SuccessfulValidation = true;
       }
       catch (Exception ex)
       {
          // SuccessfulValidation = false;
           throw ex;
       }
   }

   private void OSCALSettingsValidationEventHandler(object sender, ValidationEventArgs e)
   {
       if (e.Severity == XmlSeverityType.Warning)
       {

          // StatusLabel1.ForeColor = System.Drawing.Color.Yellow;
           StatusLabel1.Text = e.Message;
       }
       else if (e.Severity == XmlSeverityType.Error)
       {

          // StatusLabel1.ForeColor = System.Drawing.Color.Red;
           StatusLabel1.Text = e.Message;
       }
   }
}

}