<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="OSCAL_SSP_Mapper._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
        function showProgress() {
            
            var updateProgress = $get("<%= UpdateProgress.ClientID %>");
            updateProgress.style.display = "block";
        }

       
    </script>
      
       
   
    <div id="mtop" class="row" onchange ="scrollUp()">
        <div class="col-md-8" runat="server"  >
            <h2>Upload OSCAL SSP XML File</h2>
            <p>
                &nbsp;Browse to the OSCAL SSP V1.0.0 XML file and click convert
                to start process.</p>
            <p>
                <strong>&nbsp;Note:</strong> <strong>&nbsp;The generation process can take several minutes to complete.</strong></p>
            <p>
                &nbsp;</p>
        
            <p>
           
            <asp:UpdatePanel runat="server" ID="updatepanel1" UpdateMode="Conditional">
            <ContentTemplate>
            <asp:FileUpload ID="FileUpload1" runat="server" width="500px" size="50"/>
            <asp:Button runat="server" id="UploadButton" controlid="UploadButton" text="Convert" onclick="UploadButton_Click" onclientclick="showProgress()"  />
            
            <asp:UpdateProgress ID="UpdateProgress" runat="server" AssociatedUpdatePanelID="updatepanel1">
            <ProgressTemplate>
            <div class="overlay">
            <div style=" z-index: 200; margin-left: 250px;margin-top:150px;opacity: 1;">
            <img alt="" src="../loading.gif" />
            </div>
            </div>
            </ProgressTemplate>      
            </asp:UpdateProgress>
            
            <asp:Label ID="StatusLabel1" runat="server" ForeColor="#009933"></asp:Label>
            </ContentTemplate>
            <Triggers>
            <asp:PostBackTrigger ControlID="UploadButton" />
            
            </Triggers>
            </asp:UpdatePanel>
            
             <asp:Button ID="OpenMyFile" runat="server" OnClick="OpenMyFile_Click" Text="Download Word SSP Document" Visible="False" />
                         
          </p>
           
        </div>

     </div>
        <div class="container" name="bar">
           <p id="realmessage"></p>
           <div class="progress">
             <div runat="server" id ="mainbar" class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0% ">
                <span id = "realbar" class="sr-only">0% Complete</span>
             </div>
           </div>
         </div>


   
    
   
 
    <script type="text/javascript">

        var uniqueId = '<%= Guid.NewGuid().ToString() %>';

        $(document).ready(function (event) {
            $('#startProcess').click(function () {
                $.post("Default/StartLongRunningProcess", { id: uniqueId }, function () {
                    $('#statusBorder').show();
                    getStatus();
                });
                event.preventDefault;
            });
        });

        function getStatus() {
            var url = 'Default/GetCurrentProgress/' + uniqueId;
            $.get(url, function (data) {
                if (data != "100") {
                    $('#status').html(data);
                    $('#statusFill').width(data);
                    window.setTimeout("getStatus()", 100);
                }
                else {
                    $('#status').html("Done");
                    $('#statusBorder').hide();
                    alert("The Long process has finished");
                };
            });
        }

    </script>
    

</asp:Content>
