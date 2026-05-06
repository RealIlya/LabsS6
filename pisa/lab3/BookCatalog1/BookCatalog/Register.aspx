<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Регистрация</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
    <style type="text/css">
        .email-status {
            font-family: monospace;
            font-size: 0.85em;
            margin-top: 4px;
            padding: 4px 8px;
            display: none;
        }
        .email-status.available {
            display: block;
            color: #4CAF50;
            background: rgba(76, 175, 80, 0.1);
            border: 1px solid #4CAF50;
        }
        .email-status.taken {
            display: block;
            color: #f44336;
            background: rgba(244, 67, 54, 0.1);
            border: 1px solid #f44336;
        }
        .email-status.checking {
            display: block;
            color: #c8a96e;
            background: rgba(200, 169, 110, 0.1);
            border: 1px solid #c8a96e;
        }
        .password-strength {
            font-family: monospace;
            font-size: 0.85em;
            margin-top: 4px;
        }
        .strength-weak { color: #f44336; }
        .strength-medium { color: #ff9800; }
        .strength-strong { color: #4CAF50; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

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
                    <%-- AJAX: индикатор проверки email --%>
                    <div id="emailStatus" class="email-status"></div>
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
                    <%-- AJAX: индикатор сложности пароля --%>
                    <div id="passwordStrength" class="password-strength"></div>
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

        <%-- AJAX: UpdatePanel для результата регистрации --%>
        <asp:UpdatePanel ID="upRegister" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
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

                <%-- Панель: email уже существует --%>
                <asp:Panel ID="pnlEmailExists" runat="server" Visible="false"
                    CssClass="message error" Style="margin-top:10px;">
                    [!] Пользователь с таким email уже зарегистрирован.
                    <asp:HyperLink ID="hlForgotPassword" runat="server"
                        NavigateUrl="~/ForgotPassword.aspx"
                        CssClass="link">[Восстановить пароль →]</asp:HyperLink>
                </asp:Panel>

                <%-- AJAX: UpdateProgress --%>
                <asp:UpdateProgress ID="updProgress" runat="server" AssociatedUpdatePanelID="upRegister" DisplayAfter="100">
                    <ProgressTemplate>
                        <div style="color:#c8a96e; font-family:monospace; margin-top:8px;">[~] Регистрация...</div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
            </ContentTemplate>
        </asp:UpdatePanel>

    </asp:Panel>
</form>

<%-- AJAX: клиентский скрипт проверки email и пароля --%>
<script type="text/javascript">
    // Задержка перед AJAX-вызовом (debounce)
    var emailCheckTimer = null;

    function getTxtEmailId() {
        return '<%= txtEmail.ClientID %>';
    }

    function getTxtPasswordId() {
        return '<%= txtPassword.ClientID %>';
    }

    // AJAX: проверка email на уникальность через PageMethod
    function checkEmailAvailability() {
        var txtEmail = document.getElementById(getTxtEmailId());
        var statusDiv = document.getElementById('emailStatus');
        var email = txtEmail.value.trim();

        if (!email || email.length < 3) {
            statusDiv.className = 'email-status';
            statusDiv.style.display = 'none';
            return;
        }

        // Проверка формата на клиенте
        var emailRegex = /\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*/;
        if (!emailRegex.test(email)) {
            statusDiv.className = 'email-status';
            statusDiv.style.display = 'none';
            return;
        }

        statusDiv.className = 'email-status checking';
        statusDiv.style.display = 'block';
        statusDiv.innerHTML = '[~] Проверка email...';

        // AJAX вызов к серверу
        if (typeof PageMethods !== 'undefined' && PageMethods.CheckEmailAvailability) {
            PageMethods.CheckEmailAvailability(email,
                function (result) {
                    if (result === true) {
                        statusDiv.className = 'email-status available';
                        statusDiv.innerHTML = '[OK] Email доступен для регистрации';
                    } else {
                        statusDiv.className = 'email-status taken';
                        statusDiv.innerHTML = '[!] Email уже зарегистрирован';
                    }
                },
                function (error) {
                    statusDiv.className = 'email-status';
                    statusDiv.style.display = 'none';
                }
            );
        }
    }

    // AJAX: проверка сложности пароля
    function checkPasswordStrength() {
        var txtPwd = document.getElementById(getTxtPasswordId());
        var strengthDiv = document.getElementById('passwordStrength');
        var pwd = txtPwd.value;

        if (!pwd || pwd.length === 0) {
            strengthDiv.innerHTML = '';
            return;
        }

        var score = 0;
        if (pwd.length >= 6) score++;
        if (pwd.length >= 10) score++;
        if (/[A-Z]/.test(pwd)) score++;
        if (/[0-9]/.test(pwd)) score++;
        if (/[^A-Za-z0-9]/.test(pwd)) score++;

        if (score <= 2) {
            strengthDiv.innerHTML = 'Сложность: <span class="strength-weak">[▱▱▱░░] слабый</span>';
        } else if (score <= 3) {
            strengthDiv.innerHTML = 'Сложность: <span class="strength-medium">[▱▱▱▱░] средний</span>';
        } else {
            strengthDiv.innerHTML = 'Сложность: <span class="strength-strong">[▱▱▱▱▱] надёжный</span>';
        }
    }

    // Привязка событий после загрузки DOM
    document.addEventListener('DOMContentLoaded', function () {
        var txtEmail = document.getElementById(getTxtEmailId());
        var txtPwd = document.getElementById(getTxtPasswordId());

        if (txtEmail) {
            txtEmail.addEventListener('input', function () {
                clearTimeout(emailCheckTimer);
                emailCheckTimer = setTimeout(checkEmailAvailability, 500);
            });
        }

        if (txtPwd) {
            txtPwd.addEventListener('input', checkPasswordStrength);
        }
    });
</script>
</body>
</html>
