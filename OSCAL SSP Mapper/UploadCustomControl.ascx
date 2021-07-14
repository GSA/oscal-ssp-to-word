<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UploadCustomControl.ascx.cs" Inherits="OSCAL_SSP_Mapper.UploadCustomControl" %>
<%--<head>
  <title>Bootstrap Example</title>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css">
  <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
  <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js"></script>
</head>--%>
<body onload="myFunction()">

<p>
    &nbsp;</p>
<asp:FileUpload ID="FileUpload1" runat="server" />
<p>
    <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
</p>
<p>
    &nbsp;</p>

<div class="container" onload="myFunction()">
  <h2>Basic Progress Bar</h2>
  <div class="progress">
    <div class="progress-bar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width:0%">
      <span class="sr-only">0% Complete</span>
    </div>
  </div>
</div>
</body>
