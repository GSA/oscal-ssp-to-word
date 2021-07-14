using DocumentFormat.OpenXml.Packaging;
using OSCALHelperClasses;
using Microsoft.Office.Tools.Word;
using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Schema;
using Word = Microsoft.Office.Interop.Word;
using System.Threading;

namespace OSCAL_SSP_Mapper
{
    public partial class UploadCustomControl :  System.Web.UI.UserControl
    {
        //protected void Page_Load(object sender, EventArgs e)
        //{

        //}
         ConvertToWord ConvertToWord;
        public List<SecurityControl> SecurityControls;
        public List<IndicatorRank> TableIndicators;
        public List<Word.Table> DefaultTables;
        public Word.Document Document;
        public Word.Application App;
        public string CurrentTableName;
        public string WordTempFilePath;
        private string ConvertPageTopFile;
        private string ConvertPageBottomFile;

        private void PrintProgressBar(string Message, int PercentComplete, bool first =false)
        {
            Clear();
            var sb = new StringBuilder();
            sb.Append("<script>");
            var iis  = string.Format("\"{0}%\"", PercentComplete);
            sb.Append(string.Format("myFunction(\"{0}\",{1})", Message, iis));
            sb.Append("</script>");
            
            
            //sb.Append("<script type=\"text/javascript\">");



            //var man = string.Format("document.getElementById(\"realmessage\").innerText=\"{0}\"; document.getElementById(\"mainbar\").style.width= \"{1}%\";document.getElementById(\"realbar\").innerText=\"{1}% Complete\"", Message, PercentComplete);
            //sb.Append("function myFunction(){" + man + "}");
            //sb.Append("</script>");

            //sb.AppendLine("<div class=\"container\"  onload=\"myFunction()\">");
            //sb.AppendLine(string.Format("<p id=\"realmessage\">{0}</p>", Message));
            //sb.AppendLine("<div class=\"progress\">");
            //sb.AppendLine(string.Format("<div id =\"mainbar\" class=\"progress-bar\" role=\"progressbar\" aria-valuenow=\"{0}\" aria-valuemin=\"0\" aria-valuemax=\"100\" style=\"width: {0}% \">", PercentComplete));
            //sb.AppendLine(string.Format("<span id = \"realbar\" class=\"sr-only\">{0}% Complete</span>", PercentComplete));
            //sb.AppendLine("   </div>");
            //sb.AppendLine("  </div>");
            //sb.AppendLine("</div>");

            var text = sb.ToString();
            var file = "";

            if (first)
                file = ConvertPageTopFile + text + ConvertPageBottomFile;
            else
                file = text ;/// text;  //ConvertPageTopFile + text + ConvertPageBottomFile;


            //HttpContext.Current.Application.Contents.Clear();

            
           

            HttpContext.Current.Response.Write(file);
            var heads = HttpContext.Current.Items;

            Cache["text"] = Message;
            Cache["value"] = string.Format("{0}%", PercentComplete);


            var  sess = HttpContext.Current.Session;
            HttpContext.Current.Response.Flush();
          
 
        

        }
        void Clear()
        {
            HttpContext.Current.Response.Write("");
            HttpContext.Current.Response.Flush();
            
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

                if (title != null && title == "status")
                {
                    baselineCheckControls.Add(control);
                }

            }

            return baselineCheckControls;
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            string message = "";
            ConvertToWord = new ConvertToWord();
            ConvertToWord.InitPropertyStatusDictionaries();
            var convertPageTop = HttpContext.Current.Server.MapPath(string.Format(@"~\Templates\{0}", "ConvertPageTop.txt"));
            StreamReader sr = new StreamReader(convertPageTop);
           
            ConvertPageTopFile = sr.ReadToEnd();
            sr.Close();
            var convertPageBottom = HttpContext.Current.Server.MapPath(string.Format(@"~\Templates\{0}", "ConvertPageBottom.txt"));
            sr = new StreamReader(convertPageBottom);
          
            ConvertPageBottomFile = sr.ReadToEnd();
            sr.Close();

            string myXMLElement = "";
            //StatusLabel1.Text = "";
            //OpenMyFile.Visible = false;
            // this.FileUpload1
            if(FileUpload1.HasFile)
            {
                string filename = Path.GetFileName(FileUpload1.FileName);
                string fileExtension = System.IO.Path.GetExtension(FileUpload1.FileName).ToLower();
                if (fileExtension != ".xml")
                {
                    //StatusLabel1.ForeColor = System.Drawing.Color.Red;
                    //StatusLabel1.Text = "Invalid File Extension - Not an XML File!";
                }
                else
                {

                    FileUpload1.SaveAs(Server.MapPath("~/Uploads/") + filename);

                    message = string.Format("Starting the Word conversion of the file {0}",filename);

                    PrintProgressBar(message, 2, true);

                    //StatusLabel1.ForeColor = System.Drawing.Color.Green;
                    //StatusLabel1.Text = "Upload status: File sucessfully uploaded...  Processing File...Please stand by.";
                    myXMLElement = ConvertToWord.GetXMLElement(filename, "security-sensitivity-level");
                    if (myXMLElement == "low")
                    {
                        ConvertToWord.TemplateFile = "FedRAMP-SSP-Low-Baseline-Template.docx";
                        ConvertToWord.BaselinePropCountFile = "LowBaselineControlsToPropCount.txt";
                    }
                    else if (myXMLElement == "high")
                    {
                        ConvertToWord.TemplateFile = "FedRAMP-SSP-High-Baseline-Template.docx";
                        ConvertToWord.BaselinePropCountFile = "HighBaselineControlsToPropCount.txt";
                    }

                    string wordDocumentPath;
                  


                    ConvertToWord.ProcessData(filename, ConvertToWord.TemplateFile, out wordDocumentPath);

                    message = " Successfully mapped the Metadata, System Characteristics, System Implementation, ...";
                    PrintProgressBar(message, 10);
                

                    Guid guid = Guid.NewGuid();

                    WordTempFilePath = HttpContext.Current.Server.MapPath(string.Format(@"~/Downloads/{0}Document.docx", guid.ToString()));


                    File.Copy(wordDocumentPath, WordTempFilePath);

                    message = " Opening the Word Document for further processing, ...";
                    PrintProgressBar(message, 20);
                    App = new Word.Application();
                    Cache["1"] = " Opening Word Template Document";
                    Document = App.Documents.Open(WordTempFilePath);


                    //ConvertToWord.Document = Document;

                    message = " Sucessfully Opened the Word Document ...";
                    PrintProgressBar(message, 40);

                    Cache["1"] = " Successfully opened Word Template Document fletching out controls";
                   /* var Controls = Document.ContentControls;

                    var checkControls =ConvertToWord.GetCheckBoxes(Controls);
                    var statusControl = GetStatusCheckBoxes(Controls);*/
                    message = " Processing Content Controls ...";
                    PrintProgressBar(message, 60);

                    var xmlFilePath = Server.MapPath("~/Uploads/") + filename;
                    Cache["1"] = " Starting to fill all checkboxes controls";
                    string xmlSchemaPath = HttpContext.Current.Server.MapPath(string.Format(@"~\Templates\{0}", ConvertToWord.SSPschema));
                   // ConvertToWord.FillPropertyCheckBoxes(checkControls,statusControl, xmlFilePath, xmlSchemaPath, ConvertToWord.XMLNamespace, ConvertToWord.BaselinePropCountFile);

                    message = "Successfully Added all checkboxes. Starting to build tables ...";
                    PrintProgressBar(message, 80);
                   // ConvertToWord.BuildTables();
                    Cache["1"] = "Almost done thanks for your patience ....";

                    message = "Successfully built  all tables. Adding multiples responsible roles where needed ...";
                    PrintProgressBar(message, 90);

                    //ConvertToWord.AddMultipleResponsibleRoles();

                    //if (OverwriteXMLMapping)
                    //    OverWriteSecurityMapping();
                    message = "Rendering the Word document. Wrapping generation ...";
                    PrintProgressBar(message, 98);

                    Document.Save();
                    Document.Close();
                    App.Quit();

                    if (File.Exists(wordDocumentPath))
                    {
                        File.Delete(wordDocumentPath);
                    }

                    File.Copy(WordTempFilePath, wordDocumentPath);

                    File.Delete(WordTempFilePath);

                   
                   // Response.Redirect(string.Format(@"~/Downloads/{0}", ConvertToWord.TemplateFile.Replace("Template", "OSCAL")));


                }
            }
            
        }
    }
}