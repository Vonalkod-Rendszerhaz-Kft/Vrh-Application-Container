<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="ReportViewerWebForm.aspx.cs" Inherits="Vrh.OneReport.Lib.ReportViewerForMvc.ReportViewerWebForm" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="margin: 0px; padding: 0px;">
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" AsyncPostBackTimeOut="56000">
                <Scripts>
                    <asp:ScriptReference Assembly="Vrh.OneReport.Lib" Name="Vrh.OneReport.Lib.ReportViewerForMvc.Scripts.PostMessage.js" />
                </Scripts>                                                  
            </asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server"></rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>
