<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BookDetails.aspx.cs" Inherits="BookDetails" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Информация о книге</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <asp:Panel ID="pnlDetails" runat="server"
        CssClass="search-panel"
        Style="max-width:700px; margin:30px auto;">

        <h2>▍ ИНФОРМАЦИЯ ОБ ЭКЗЕМПЛЯРЕ</h2>

        <asp:Panel ID="pnlBookInfo" runat="server" Visible="false">
            <table class="search-form">
                <tr>
                    <td class="mono label-key">НАЗВАНИЕ:</td>
                    <td><asp:Label ID="lblTitle"     runat="server" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">АВТОР:</td>
                    <td><asp:Label ID="lblAuthor"    runat="server" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">ISBN:</td>
                    <td><asp:Label ID="lblISBN"      runat="server" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">ИЗДАТЕЛЬСТВО:</td>
                    <td><asp:Label ID="lblPublisher" runat="server" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">ГОД:</td>
                    <td><asp:Label ID="lblYear"      runat="server" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">ЖАНР:</td>
                    <td><asp:Label ID="lblGenre"     runat="server" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">СТРАНИЦ:</td>
                    <td><asp:Label ID="lblPages"     runat="server" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">ДОСТУПНО:</td>
                    <%-- Исправление #1: CssClass вместо ForeColor с hex --%>
                    <td><asp:Label ID="lblAvailable" runat="server" CssClass="status-available" /></td>
                </tr>
                <tr>
                    <td class="mono label-key">СТАТУС:</td>
                    <td><asp:Label ID="lblStatus"    runat="server" /></td>
                </tr>
            </table>

            <br />
            <asp:HyperLink ID="hlBack" runat="server"
                NavigateUrl="~/Default.aspx"
                Text="[← НАЗАД К ПОИСКУ]"
                CssClass="link" />

            <%-- Исправление #4: кнопка "Забронировать" --%>
            <asp:LinkButton ID="lbBook" runat="server"
                Text="[ЗАБРОНИРОВАТЬ]"
                OnClick="lbBook_Click"
                CssClass="btn btn-primary"
                Style="margin-left:20px;"
                Visible="false" />

            <%-- Исправление #4: кнопка очереди (сценарий 4, шаг 5а.2–5а.3) --%>
            <asp:LinkButton ID="lbQueue" runat="server"
                Text="[ВСТАТЬ В ОЧЕРЕДЬ]"
                OnClick="lbQueue_Click"
                CssClass="btn btn-secondary"
                Style="margin-left:20px;"
                Visible="false"
                OnClientClick="return confirm('Встать в очередь на эту книгу?');" />

            <asp:Label ID="lblActionMessage" runat="server"
                CssClass="message" Visible="false" />
        </asp:Panel>

        <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
            <p class="mono error-text">[!] Запись не найдена в архивах</p>
            <asp:HyperLink ID="hlBack2" runat="server"
                NavigateUrl="~/Default.aspx"
                Text="[← ВЕРНУТЬСЯ]"
                CssClass="link" />
        </asp:Panel>

    </asp:Panel>
</form>
</body>
</html>