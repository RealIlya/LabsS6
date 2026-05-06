<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AddBook.aspx.cs" Inherits="Admin_AddBook" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Добавление книги</title>
    <link href="../Styles/nier-theme.css" rel="stylesheet" />
    <style type="text/css">
        .isbn-status {
            font-family: monospace;
            font-size: 0.85em;
            margin-top: 4px;
            padding: 4px 8px;
            display: none;
        }
        .isbn-status.ok {
            display: block;
            color: #4CAF50;
            background: rgba(76, 175, 80, 0.1);
            border: 1px solid #4CAF50;
        }
        .isbn-status.duplicate {
            display: block;
            color: #ff9800;
            background: rgba(255, 152, 0, 0.1);
            border: 1px solid #ff9800;
        }
        .isbn-status.checking {
            display: block;
            color: #c8a96e;
            background: rgba(200, 169, 110, 0.1);
            border: 1px solid #c8a96e;
        }
        .ajax-loader {
            color: #c8a96e;
            font-family: monospace;
            margin-top: 8px;
        }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <asp:Panel ID="pnlAddBook" runat="server"
        CssClass="search-panel"
        Style="max-width:600px; margin:30px auto;">

        <h2>▍ ДОБАВЛЕНИЕ НОВОГО ЭКЗЕМПЛЯРА</h2>

        <div style="margin-bottom:12px;">
            <asp:HyperLink runat="server" NavigateUrl="~/Default.aspx"
                CssClass="link">← НАЗАД В КАТАЛОГ</asp:HyperLink>
        </div>

        <%-- AJAX: UpdatePanel для формы и дубликатов --%>
        <asp:UpdatePanel ID="upAddBook" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <%-- Основная форма --%>
                <asp:Panel ID="pnlForm" runat="server">
                    <table class="search-form">
                        <tr>
                            <td><asp:Label ID="lblTitle" runat="server"
                                Text="Название:" AssociatedControlID="txtTitle" /></td>
                            <td>
                                <asp:TextBox ID="txtTitle" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator ID="rfvTitle" runat="server"
                                    ControlToValidate="txtTitle"
                                    ErrorMessage="Название обязательно"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblAuthor" runat="server"
                                Text="Автор:" AssociatedControlID="txtAuthor" /></td>
                            <td>
                                <asp:TextBox ID="txtAuthor" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator ID="rfvAuthor" runat="server"
                                    ControlToValidate="txtAuthor"
                                    ErrorMessage="Автор обязателен"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblISBN" runat="server"
                                Text="ISBN:" AssociatedControlID="txtISBN" /></td>
                            <td>
                                <asp:TextBox ID="txtISBN" runat="server" CssClass="input" />
                                <%-- ISBN обязателен — по нему ищем дубликаты --%>
                                <asp:RequiredFieldValidator ID="rfvISBN" runat="server"
                                    ControlToValidate="txtISBN"
                                    ErrorMessage="ISBN обязателен"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                                <asp:RegularExpressionValidator ID="revISBN" runat="server"
                                    ControlToValidate="txtISBN"
                                    ValidationExpression="^(97[89]-)?\d{1,5}-\d{1,7}-\d{1,7}-[\dX]$"
                                    ErrorMessage="Неверный формат ISBN, должен содержать 13 цифр в формате 978-X-XX-XXXXXX-X"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                                <%-- AJAX: индикатор проверки ISBN --%>
                                <div id="isbnStatus" class="isbn-status"></div>
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblPublisher" runat="server"
                                Text="Издательство:" AssociatedControlID="txtPublisher" /></td>
                            <td>
                                <asp:TextBox ID="txtPublisher" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator ID="rfvPublisher" runat="server"
                                    ControlToValidate="txtPublisher"
                                    ErrorMessage="Издательство обязательно"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblYear" runat="server"
                                Text="Год:" AssociatedControlID="txtYear" /></td>
                            <td>
                                <asp:TextBox ID="txtYear" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator ID="rfvYear" runat="server"
                                    ControlToValidate="txtYear"
                                    ErrorMessage="Год обязателен"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                                <asp:RangeValidator ID="rvYear" runat="server"
                                    ControlToValidate="txtYear"
                                    Type="Integer"
                                    MinimumValue="1000" MaximumValue="2026"
                                    ErrorMessage="Год: от 1000 до 2026"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblGenre" runat="server"
                                Text="Жанр:" AssociatedControlID="ddlGenre" /></td>
                            <td>
                                <asp:DropDownList ID="ddlGenre" runat="server" CssClass="input">
                                    <asp:ListItem Value="fiction"   Text="Художественная" />
                                    <asp:ListItem Value="science"   Text="Научная" />
                                    <asp:ListItem Value="education" Text="Учебная" />
                                    <asp:ListItem Value="children"  Text="Детская" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblPages" runat="server"
                                Text="Страниц:" AssociatedControlID="txtPages" /></td>
                            <td>
                                <asp:TextBox ID="txtPages" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator ID="rfvPages" runat="server"
                                    ControlToValidate="txtPages"
                                    ErrorMessage="Количество страниц обязательно"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                                <asp:RangeValidator ID="rvPages" runat="server"
                                    ControlToValidate="txtPages"
                                    Type="Integer"
                                    MinimumValue="1" MaximumValue="99999"
                                    ErrorMessage="Страниц: от 1 до 99999"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblCount" runat="server"
                                Text="Экземпляров:" AssociatedControlID="txtCount" /></td>
                            <td>
                                <asp:TextBox ID="txtCount" runat="server"
                                    CssClass="input" Text="1" />
                                <asp:RequiredFieldValidator ID="rfvCount" runat="server"
                                    ControlToValidate="txtCount"
                                    ErrorMessage="Количество экземпляров обязательно"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                                <asp:RangeValidator ID="rvCount" runat="server"
                                    ControlToValidate="txtCount"
                                    Type="Integer"
                                    MinimumValue="1" MaximumValue="100"
                                    ErrorMessage="Экземпляров: от 1 до 100"
                                    CssClass="error" Display="Dynamic"
                                    ValidationGroup="AddBookGroup" />
                            </td>
                        </tr>
                    </table>

                    <asp:Button ID="btnAdd" runat="server"
                        Text="[>> ДОБАВИТЬ]"
                        OnClick="btnAdd_Click"
                        CssClass="btn btn-primary"
                        ValidationGroup="AddBookGroup" />

                    <asp:ValidationSummary ID="vsErrors" runat="server"
                        ShowMessageBox="false"
                        CssClass="error-summary"
                        ValidationGroup="AddBookGroup" />

                    <asp:Label ID="lblMessage" runat="server"
                        CssClass="message" Visible="false" />
                </asp:Panel>

                <%-- Панель подтверждения дубликата — скрыта по умолчанию --%>
                <asp:Panel ID="pnlDuplicate" runat="server" Visible="false"
                    Style="margin-top:20px;">

                    <h3 style="color:#c8a96e;">[!] ОБНАРУЖЕНЫ СОВПАДЕНИЯ В КАТАЛОГЕ</h3>

                    <p class="mono" style="margin:8px 0;">
                        Книга с таким ISBN уже существует в системе:
                    </p>

                    <%-- Список найденных дубликатов --%>
                    <asp:GridView ID="gvDuplicates" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="books-grid"
                        Style="margin:10px 0;">
                        <Columns>
                            <asp:BoundField DataField="Title"          HeaderText="Название" />
                            <asp:BoundField DataField="Author"         HeaderText="Автор" />
                            <asp:BoundField DataField="ISBN"           HeaderText="ISBN" />
                            <asp:BoundField DataField="TotalCount"     HeaderText="Всего" />
                            <asp:BoundField DataField="AvailableCount" HeaderText="Доступно" />
                        </Columns>
                    </asp:GridView>

                    <p class="mono" style="margin:12px 0;">
                        Добавить
                        <asp:Label ID="lblDupCount" runat="server"
                            CssClass="username" />
                        экз. к существующей записи?
                    </p>

                    <asp:Button ID="btnConfirmAdd" runat="server"
                        Text="[OK ДОБАВИТЬ ЭКЗЕМПЛЯРЫ]"
                        OnClick="btnConfirmAdd_Click"
                        CssClass="btn btn-primary"
                        CausesValidation="false" />

                    <asp:Button ID="btnCancelAdd" runat="server"
                        Text="[X ОТМЕНИТЬ]"
                        OnClick="btnCancelAdd_Click"
                        CssClass="btn btn-secondary"
                        CausesValidation="false"
                        Style="margin-left:10px;" />
                </asp:Panel>

                <%-- AJAX: UpdateProgress --%>
                <asp:UpdateProgress ID="updProgress" runat="server" AssociatedUpdatePanelID="upAddBook" DisplayAfter="100">
                    <ProgressTemplate>
                        <div class="ajax-loader">[~] Обработка...</div>
                    </ProgressTemplate>
                </asp:UpdateProgress>
            </ContentTemplate>
        </asp:UpdatePanel>

    </asp:Panel>
</form>

<%-- AJAX: клиентский скрипт для проверки ISBN через PageMethod --%>
<%-- jQuery нужен для AJAX-проверки ISBN --%>
<script src="../../Scripts/jquery-3.7.0.min.js"></script>
<script type="text/javascript">
    var isbnCheckTimer = null;

    function getTxtISBNId() {
        return '<%= txtISBN.ClientID %>';
    }

    // AJAX: проверка ISBN на дубликаты (через jQuery, т.к. PageMethods не работает с FriendlyUrls)
    function checkISBNDuplicate() {
        var txtISBN = document.getElementById(getTxtISBNId());
        var statusDiv = document.getElementById('isbnStatus');
        var isbn = txtISBN.value.trim();

        if (!isbn || isbn.length < 5) {
            statusDiv.className = 'isbn-status';
            statusDiv.style.display = 'none';
            return;
        }

        statusDiv.className = 'isbn-status checking';
        statusDiv.style.display = 'block';
        statusDiv.innerHTML = '[~] Проверка ISBN...';

        $.ajax({
            type: 'POST',
            url: 'CheckISBNHandler.ashx',
            data: { isbn: isbn },
            dataType: 'json',
            success: function (result) {
                if (result.exists) {
                    statusDiv.className = 'isbn-status duplicate';
                    statusDiv.innerHTML = '[!] Найдено совпадений: ' + result.count +
                        ' шт. (можно добавить экземпляры)';
                } else {
                    statusDiv.className = 'isbn-status ok';
                    statusDiv.innerHTML = '[OK] ISBN уникален, дубликатов нет';
                }
            },
            error: function () {
                statusDiv.className = 'isbn-status';
                statusDiv.style.display = 'none';
            }
        });
    }

    // Привязка событий
    document.addEventListener('DOMContentLoaded', function () {
        var txtISBN = document.getElementById(getTxtISBNId());

        if (txtISBN) {
            txtISBN.addEventListener('input', function () {
                clearTimeout(isbnCheckTimer);
                isbnCheckTimer = setTimeout(checkISBNDuplicate, 600);
            });
        }
    });
</script>
</body>
</html>
