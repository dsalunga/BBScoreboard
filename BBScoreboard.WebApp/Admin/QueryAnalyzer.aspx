<%@ Page Language="C#" AutoEventWireup="true" CodeFile="QueryAnalyzer.aspx.cs" Inherits="Admin_QueryAnalyzer" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table width="100%">
                <tr>
                    <td>Connection String:
            <%--<asp:TextBox ID="txtCode" runat="server" Columns="10"></asp:TextBox>--%>&nbsp;
            <asp:TextBox ID="txtQS" runat="server" CssClass="col-md-6"></asp:TextBox>
                        <asp:CheckBox ID="chkCustom" runat="server" CssClass="aspnet-checkbox" Text="Custom" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="txtQuery" runat="server" Rows="20" Width="100%" TextMode="MultiLine"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td class="ControlBox">
                        <asp:Button ID="cmdExecute" CssClass="btn btn-default" runat="server" Text="Execute"
                            OnClick="cmdExecute_Click"></asp:Button>&nbsp;<asp:RadioButton ID="chkQuery" runat="server"
                                Text="Query" GroupName="radioOptions" CssClass="aspnet-radio" Checked="True" />&nbsp;<asp:RadioButton ID="chkDownload"
                                    runat="server" Text="Download" CssClass="aspnet-radio" GroupName="radioOptions" />&nbsp;
            <asp:RadioButton ID="chkUpload" CssClass="aspnet-radio" runat="server" Text="Upload" GroupName="radioOptions" />
                        <asp:FileUpload ID="FileUpload1" runat="server" />&nbsp;
            <asp:CheckBox ID="chkSchema" CssClass="aspnet-checkbox" runat="server" Text="Schema" />
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
                        <asp:PlaceHolder ID="phGrid" runat="server"></asp:PlaceHolder>
                    </td>
                </tr>
            </table>
 
        </div>
    </form>
</body>
</html>
