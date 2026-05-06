<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Booking.aspx.cs" Inherits="Booking" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Бронирование</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
    <style type="text/css">
        .booking-loader {
            color: #c8a96e;
            font-family: monospace;
            margin: 10px 0;
        }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

    <asp:Panel ID="pnlBooking" runat="server"
        CssClass="search-panel"
        Style="max-width:500px; margin:50px auto;">

        <h2>▍ ПОДТВЕРЖДЕНИЕ БРОНИРОВАНИЯ</h2>

        <%-- AJAX: UpdatePanel для всей панели бронирования --%>
        <asp:UpdatePanel ID="upBooking" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <%-- Панель подтверждения --%>
                <asp:Panel ID="pnlConfirm" runat="server" Visible="false">
                    <p class="mono">Вы собираетесь забронировать:</p>
                    <p style="font-size:1.2rem; margin:10px 0;">
                        <asp:Label ID="lblBookTitle" runat="server" />
                    </p>
                    <p class="mono">Автор:
                        <asp:Label ID="lblBookAuthor" runat="server" />
                    </p>
                    <p class="mono">Доступно экземпляров:
                        <%-- Исправление #1: CssClass вместо ForeColor --%>
                        <asp:Label ID="lblAvailable" runat="server" CssClass="status-available" />
                    </p>
                    <br />
                    <p class="mono label-key">[!] Срок брони: 3 дня с момента подтверждения</p>
                    <br />
                    <asp:Button ID="btnConfirm" runat="server"
                        Text="[>> ПОДТВЕРДИТЬ БРОНЬ]"
                        OnClick="btnConfirm_Click"
                        CssClass="btn btn-primary" />
                    <asp:HyperLink ID="hlCancel" runat="server"
                        NavigateUrl="~/Default.aspx"
                        Text="[X ОТМЕНА]"
                        CssClass="link"
                        Style="margin-left:15px;" />
                </asp:Panel>

                <%-- Панель успеха --%>
                <asp:Panel ID="pnlSuccess" runat="server" Visible="false">
                    <p class="mono status-available">[OK] Бронирование успешно оформлено!</p>
                    <p class="mono">Срок получения:
                        <asp:Label ID="lblExpiryDate" runat="server" />
                    </p>
                    <br />
                    <asp:HyperLink ID="hlBack" runat="server"
                        NavigateUrl="~/Default.aspx"
                        Text="[← В КАТАЛОГ]"
                        CssClass="link" />
                </asp:Panel>

                <%-- Панель ошибки --%>
                <asp:Panel ID="pnlError" runat="server" Visible="false">
                    <p class="mono error-text">
                        <asp:Label ID="lblError" runat="server" />
                    </p>
                    <br />
                    <asp:HyperLink ID="hlBack2" runat="server"
                        NavigateUrl="~/Default.aspx"
                        Text="[← В КАТАЛОГ]"
                        CssClass="link" />
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

        <%-- AJAX: UpdateProgress для бронирования --%>
        <asp:UpdateProgress ID="updProgress" runat="server" AssociatedUpdatePanelID="upBooking" DisplayAfter="100">
            <ProgressTemplate>
                <div class="booking-loader">[~] Оформление бронирования...</div>
            </ProgressTemplate>
        </asp:UpdateProgress>

    </asp:Panel>
</form>
</body>
</html>
