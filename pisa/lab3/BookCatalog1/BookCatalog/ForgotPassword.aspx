<%@ Page Language="C#" AutoEventWireup="true"
    CodeFile="ForgotPassword.aspx.cs" Inherits="ForgotPassword" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Восстановление пароля</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <asp:Panel ID="pnlMain" runat="server"
        CssClass="search-panel"
        Style="max-width:450px; margin:80px auto;">

        <h2>▍ ВОССТАНОВЛЕНИЕ ДОСТУПА</h2>

        <div style="margin-bottom:12px;">
            <asp:HyperLink runat="server" NavigateUrl="~/Login.aspx"
                CssClass="link">← НАЗАД К ВХОДУ</asp:HyperLink>
        </div>

        <%-- Шаг 1: ввод email --%>
        <asp:Panel ID="pnlStep1" runat="server">
            <p class="mono" style="margin-bottom:10px;">
                Введите email, привязанный к аккаунту.
            </p>
            <table class="search-form">
                <tr>
                    <td><asp:Label runat="server" Text="Email:"
                        AssociatedControlID="txtEmail" /></td>
                    <td>
                        <asp:TextBox ID="txtEmail" runat="server"
                            CssClass="input" TextMode="Email"
                            placeholder="user@example.com" />
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                            ControlToValidate="txtEmail"
                            ErrorMessage="Email обязателен"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="Step1Group" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server"
                            ControlToValidate="txtEmail"
                            ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                            ErrorMessage="Неверный формат email"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="Step1Group" />
                    </td>
                </tr>
            </table>
            <asp:Button ID="btnFind" runat="server"
                Text="[>> НАЙТИ АККАУНТ]"
                OnClick="btnFind_Click"
                CssClass="btn btn-primary"
                ValidationGroup="Step1Group" />
            <asp:ValidationSummary runat="server"
                CssClass="error-summary" ShowMessageBox="false"
                ValidationGroup="Step1Group" />
            <asp:Label ID="lblStep1Error" runat="server"
                CssClass="message error" Visible="false" />
        </asp:Panel>

        <%-- Шаг 2: новый пароль (показывается после успешного поиска) --%>
        <asp:Panel ID="pnlStep2" runat="server" Visible="false">
            <asp:Label ID="lblFoundUser" runat="server"
                CssClass="message" Style="margin-bottom:10px;" />
            <table class="search-form">
                <tr>
                    <td><asp:Label runat="server" Text="Новый пароль:"
                        AssociatedControlID="txtNewPassword" /></td>
                    <td>
                        <asp:TextBox ID="txtNewPassword" runat="server"
                            CssClass="input" TextMode="Password"
                            placeholder="Мин. 6 символов" />
                        <asp:RequiredFieldValidator runat="server"
                            ControlToValidate="txtNewPassword"
                            ErrorMessage="Введите новый пароль"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="Step2Group" />
                        <asp:RegularExpressionValidator runat="server"
                            ControlToValidate="txtNewPassword"
                            ValidationExpression="^.{6,20}$"
                            ErrorMessage="Пароль: от 6 до 20 символов"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="Step2Group" />
                    </td>
                </tr>
                <tr>
                    <td><asp:Label runat="server" Text="Повтор:"
                        AssociatedControlID="txtConfirmPassword" /></td>
                    <td>
                        <asp:TextBox ID="txtConfirmPassword" runat="server"
                            CssClass="input" TextMode="Password" />
                        <asp:RequiredFieldValidator runat="server"
                            ControlToValidate="txtConfirmPassword"
                            ErrorMessage="Подтвердите пароль"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="Step2Group" />
                        <asp:CompareValidator runat="server"
                            ControlToValidate="txtConfirmPassword"
                            ControlToCompare="txtNewPassword"
                            ErrorMessage="Пароли не совпадают"
                            CssClass="error" Display="Dynamic"
                            ValidationGroup="Step2Group" />
                    </td>
                </tr>
            </table>
            <asp:Button ID="btnSetPassword" runat="server"
                Text="[OK СМЕНИТЬ ПАРОЛЬ]"
                OnClick="btnSetPassword_Click"
                CssClass="btn btn-primary"
                ValidationGroup="Step2Group" />
            <asp:ValidationSummary runat="server"
                CssClass="error-summary" ShowMessageBox="false"
                ValidationGroup="Step2Group" />
        </asp:Panel>

    </asp:Panel>
</form>
</body>
</html>