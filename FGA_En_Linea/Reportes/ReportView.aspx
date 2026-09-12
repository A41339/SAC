<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportView.aspx.cs" Inherits="FGA.Reportes.ReportView" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Reporte</title>
    <style>
        #ReportViewerRSFReports_ctl09 {
            margin-bottom: 10px;
        }
    </style>
</head>
<body>
    <div class="box container-fluid">
        <div class="row">
            <div class="col-lg-12">
                <form id="form1" runat="server">
                    <asp:ScriptManager runat="server"></asp:ScriptManager>

                    <%if (Session["TipoReporte"] != null)
                        {  %>
                    <rsweb:ReportViewer ID="ReportViewerRSFReports" runat="server"  EnableEventValidation="false" AsyncRendering="false" 
                        Width="100%" > 
                    </rsweb:ReportViewer>
                    <% } %>
                </form>
            </div>
        </div>
    </div>
</body>
</html>
