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

namespace OSCAL_SSP_Mapper
{
    public partial class ConvertToWord : _Default
    {
        //protected void Page_Load(object sender, EventArgs e)
        //{

        //}

        public int PercentComplete;
        public string StatusMessage;
        
        public  void Process()
        {
            
            string myXMLElement = "";
            StatusLabel1.Text = "";
            OpenMyFile.Visible = false;

            if (UploadCustomControl1.FileUpload1.HasFile)
            {
                try
                {
                    string filename = Path.GetFileName(FileUpload1.FileName);
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
                        Cache["1"] = "Init Complete";


                        ProcessData(filename, TemplateFile, out wordDocumentPath);
                        Cache["1"] = "Basic XML Mapping Complete ...";





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
                        App.Quit();

                    if (File.Exists(WordTempFilePath))
                    {
                        File.Delete(WordTempFilePath);
                    }

                }

            }  
        }
    }
}