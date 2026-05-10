<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Авторизация</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
    <style type="text/css">
        .login-status {
            font-family: monospace;
            font-size: 0.85em;
            margin-top: 4px;
            padding: 4px 8px;
            display: none;
        }
        .login-status.error {
            display: block;
            color: #f44336;
            background: rgba(244, 67, 54, 0.1);
            border: 1px solid #f44336;
        }
        .login-status.loading {
            display: block;
            color: #c8a96e;
            background: rgba(200, 169, 110, 0.1);
            border: 1px solid #c8a96e;
        }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

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

        <%-- AJAX: UpdatePanel для формы входа --%>
        <asp:UpdatePanel ID="upLogin" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
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
                            <%-- AJAX: индикатор Caps Lock --%>
                            <div id="capsLockWarning" class="login-status error" style="display:none;">
                                [!] Caps Lock включён
                            </div>
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

                <%-- AJAX: UpdateProgress --%>
                <asp:UpdateProgress ID="updProgress" runat="server" AssociatedUpdatePanelID="upLogin" DisplayAfter="100">
                    <ProgressTemplate>
                        <div style="color:#c8a96e; font-family:monospace; margin-top:8px;">[~] Авторизация...</div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
            </ContentTemplate>
        </asp:UpdatePanel>

    </asp:Panel>
</form>

<%-- AJAX: клиентский скрипт для проверки Caps Lock --%>
<script type="text/javascript">
    document.addEventListener('DOMContentLoaded', function () {
        var pwdField = document.getElementById('<%= txtPassword.ClientID %>');
        var capsWarning = document.getElementById('capsLockWarning');

        if (pwdField) {
            pwdField.addEventListener('keypress', function (e) {
                var charCode = e.charCode || e.keyCode;
                var charStr = String.fromCharCode(charCode);
                var isUpperCase = charStr === charStr.toUpperCase() && charStr !== charStr.toLowerCase();

                // Если Shift не нажат и символ — верхний регистр, или Shift нажат и символ — нижний
                if ((isUpperCase && !e.shiftKey) || (!isUpperCase && e.shiftKey && charCode >= 65)) {
                    capsWarning.style.display = 'block';
                    capsWarning.innerHTML = '[!] Caps Lock включён';
                } else {
                    capsWarning.style.display = 'none';
                }
            });

            pwdField.addEventListener('blur', function () {
                capsWarning.style.display = 'none';
            });
        }
    });
</script>
</body>
</html>
