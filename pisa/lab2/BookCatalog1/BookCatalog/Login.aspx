<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Авторизация</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <asp:Panel ID="pnlLogin" runat="server"
        CssClass="search-panel"
        Style="max-width:400px; margin:80px auto;">

        <h2>▍ АВТОРИЗАЦИЯ В СИСТЕМЕ</h2>

        <%-- Кнопка выхода в каталог --%>
        <div style="margin-bottom:12px;">
            <asp:HyperLink runat="server" NavigateUrl="~/Default.aspx"
                CssClass="link">← НАЗАД В КАТАЛОГ</asp:HyperLink>
        </div>

        <%-- Сообщение об успешной регистрации --%>
        <asp:Panel ID="pnlRegisterSuccess" runat="server" Visible="false"
            CssClass="message" Style="margin-bottom:10px;">
            [OK] Аккаунт создан. Войдите в систему.
        </asp:Panel>

        <table class="search-form">
            <tr>
                <td><asp:Label ID="lblEmail" runat="server"
                    Text="Email:" AssociatedControlID="txtEmail" /></td>
                <td>
                    <asp:TextBox ID="txtEmail" runat="server"
                        CssClass="input" TextMode="Email" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                        ControlToValidate="txtEmail"
                        ErrorMessage="Email обязателен"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="LoginGroup" />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server"
                        ControlToValidate="txtEmail"
                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                        ErrorMessage="Неверный формат email"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="LoginGroup" />
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lblPassword" runat="server"
                    Text="Пароль:" AssociatedControlID="txtPassword" /></td>
                <td>
                    <asp:TextBox ID="txtPassword" runat="server"
                        CssClass="input" TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                        ControlToValidate="txtPassword"
                        ErrorMessage="Пароль обязателен"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="LoginGroup" />
                </td>
            </tr>
        </table>

        <asp:Button ID="btnLogin" runat="server"
            Text="[>> ВОЙТИ]"
            OnClick="btnLogin_Click"
            CssClass="btn btn-primary"
            ValidationGroup="LoginGroup" />

        <asp:HyperLink ID="hlRegister" runat="server"
            NavigateUrl="~/Register.aspx"
            Text="[← НЕТ АККАУНТА?]"
            CssClass="link"
            Style="margin-left:15px;" />

        <br /><br />

        <asp:HyperLink ID="hlForgotPassword" runat="server"
            NavigateUrl="~/ForgotPassword.aspx"
            Text="[? ЗАБЫЛИ ПАРОЛЬ]"
            CssClass="link" />

        <asp:ValidationSummary ID="vsLogin" runat="server"
            CssClass="error-summary"
            ShowMessageBox="false"
            ValidationGroup="LoginGroup" />

        <asp:Label ID="lblMessage" runat="server"
            CssClass="message error" Visible="false" />

    </asp:Panel>
</form>
</body>
</html>


