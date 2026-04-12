<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Регистрация</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <asp:Panel ID="pnlRegister" runat="server"
        CssClass="search-panel"
        Style="max-width:500px; margin:50px auto;">

        <h2>▍ СОЗДАНИЕ НОВОГО ПОЛЬЗОВАТЕЛЯ</h2>

        <%-- Кнопка выхода в каталог --%>
        <div style="margin-bottom:12px;">
            <asp:HyperLink runat="server" NavigateUrl="~/Default.aspx"
                CssClass="link">← НАЗАД В КАТАЛОГ</asp:HyperLink>
        </div>

        <table class="search-form">
            <tr>
                <td><asp:Label ID="lblName" runat="server"
                    Text="ФИО:" AssociatedControlID="txtName" /></td>
                <td>
                    <asp:TextBox ID="txtName" runat="server"
                        CssClass="input" placeholder="Ваше полное имя..." />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server"
                        ControlToValidate="txtName"
                        ErrorMessage="ФИО обязательно"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="RegisterGroup" />
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lblEmail" runat="server"
                    Text="Email:" AssociatedControlID="txtEmail" /></td>
                <td>
                    <asp:TextBox ID="txtEmail" runat="server"
                        CssClass="input" TextMode="Email"
                        placeholder="user@example.com" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                        ControlToValidate="txtEmail"
                        ErrorMessage="Email обязателен"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="RegisterGroup" />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server"
                        ControlToValidate="txtEmail"
                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                        ErrorMessage="Неверный формат email"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="RegisterGroup" />
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lblPassword" runat="server"
                    Text="Пароль:" AssociatedControlID="txtPassword" /></td>
                <td>
                    <asp:TextBox ID="txtPassword" runat="server"
                        CssClass="input" TextMode="Password"
                        placeholder="Мин. 6 символов" />
                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                        ControlToValidate="txtPassword"
                        ErrorMessage="Пароль обязателен"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="RegisterGroup" />
                    <asp:RegularExpressionValidator ID="revPassword" runat="server"
                        ControlToValidate="txtPassword"
                        ValidationExpression="^.{6,20}$"
                        ErrorMessage="Пароль: от 6 до 20 символов"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="RegisterGroup" />
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lblConfirm" runat="server"
                    Text="Повтор:" AssociatedControlID="txtConfirm" /></td>
                <td>
                    <asp:TextBox ID="txtConfirm" runat="server"
                        CssClass="input" TextMode="Password" />
                    <asp:RequiredFieldValidator ID="rfvConfirm" runat="server"
                        ControlToValidate="txtConfirm"
                        ErrorMessage="Подтверждение пароля обязательно"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="RegisterGroup" />
                    <asp:CompareValidator ID="cvPassword" runat="server"
                        ControlToValidate="txtConfirm"
                        ControlToCompare="txtPassword"
                        ErrorMessage="Пароли не совпадают"
                        CssClass="error" Display="Dynamic"
                        ValidationGroup="RegisterGroup" />
                </td>
            </tr>
        </table>

        <asp:Button ID="btnRegister" runat="server"
            Text="[>> ЗАРЕГИСТРИРОВАТЬ]"
            OnClick="btnRegister_Click"
            CssClass="btn btn-primary"
            ValidationGroup="RegisterGroup" />

        <asp:HyperLink ID="hlLogin" runat="server"
            NavigateUrl="~/Login.aspx"
            Text="[← УЖЕ ЕСТЬ АККАУНТ]"
            CssClass="link"
            Style="margin-left:15px;" />

        <asp:ValidationSummary ID="vsErrors" runat="server"
            ShowMessageBox="false"
            CssClass="error-summary"
            ValidationGroup="RegisterGroup" />

        <asp:Label ID="lblMessage" runat="server"
            CssClass="message" Visible="false" />

        <%-- Панель: email уже существует → предложить восстановление пароля --%>
        <asp:Panel ID="pnlEmailExists" runat="server" Visible="false"
            CssClass="message error" Style="margin-top:10px;">
            [!] Пользователь с таким email уже зарегистрирован.
            <asp:HyperLink ID="hlForgotPassword" runat="server"
                NavigateUrl="~/ForgotPassword.aspx"
                CssClass="link">[Восстановить пароль →]</asp:HyperLink>
        </asp:Panel>

    </asp:Panel>
</form>
</body>
</html>

