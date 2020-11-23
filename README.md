# OSCAL-Conversion for System Security Plans
This application renders an OSCAL XML SSP file to a Microsoft Word file utilizing the FedRAMP document template formats.  See https://www.fedramp.gov/templates/ for samples of these Microsoft Word templates.  Currently the application can support LOW, MODERATE and HIGH baseline System Security Plans aligned with a valid OSCAL Milestone 3 schema.
# Why the project is useful
NIST is developing the Open Security Controls Assessment Language (OSCAL), a set of hierarchical, formatted, XML- and JSON-based formats that provide a standardized representation for different categories of information pertaining to the publication, implementation, and assessment of security controls. OSCAL is being developed through a collaborative approach with the public. The OSCAL website (https://csrc.nist.gov/Projects/Open-Security-Controls-Assessment-Language) provides an overview of the OSCAL project, including an XML and JSON schema reference and examples
The application automates the manual mappings from OSCAL SSP to MS Word. The mappings includes 100's of pages in the document and maps most of the xml elements (source file) to word without any manual intervention. Only XML elements that match the content mappings in the included FedRAMP templates will render.
# Project Requirements
The application is coded in C# as an ASP.NET web application Visual Studio project and is meant to run in standalone mode ONLY.   This project utilizes the OpenXML (DocumentFormat.OpenXml) and the Microsoft Office Interop (Microsoft.Office.Interop.Word) namespaces to perform XML parsing and perform document rendering.
# System Software Requirements
Windows 10
Office 2016 or better
Visual Studio 2019 Community Edition

# Getting started with this project
1. Install Visual Studio 2019 Community Edition
2. Clone and checkout the Project("https://github.com/GSA/oscal-ssp-to-word”)
3. Clean and Build the Project
4. Run the Project
5. Select a valid OSCAL XML SSP file and click "Upload"
6. The FedRAMP Document rendering will take several minutes to complete and will automatically be     downloaded and available after rendering is complete.   Word should open automatically once it has fully downloaded the rendered document.

# Known Issues
You may have the following error when running the code for the first time
"Could not find a part of the path '\OSCAL-Conversion\OSCAL SSP Converter\bin\roslyn\csc.exe'".
This issue can be resolved by installing the Nuget Package:  Microsoft.CodeDom.Providers.DotNetComplierPlatform.NoInstall
# License Information
This project is being released under the GNU General Public License (GNU GPL) 3.0 model. Under that model there is no warranty on the open source code.   Software under the GPL may be run for all purposes, including commercial purposes and even as a tool for creating proprietary software.
# Disclaimer
This project will ONLY work with the Milestone release 3 of the OSCAL schema from the NIST website (https://csrc.nist.gov/Projects/Open-Security-Controls-Assessment-Language) and does not address gaps between the NIST schema and the FedRAMP template requirements.   It maps elements between the two that match and will ignore those that do not.  The mappings can be adjusted and edited in Microsoft Word for the template by using the Mapping Content Controls with the XML Panel within Microsoft Word.
# Getting help with this project
Contact the GSA FedRAMP Project Management Office for more information or support.
# Originator of Code
VITG, INC.  http://www.volpegroup.com

