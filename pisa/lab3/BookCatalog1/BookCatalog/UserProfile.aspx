<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UserProfile.aspx.cs" Inherits="UserProfile" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Профиль пользователя</title>
    <link href="Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">
    <asp:Panel ID="pnlProfile" runat="server"
        CssClass="search-panel"
        Style="max-width:800px; margin:30px auto;">

        <h2>▍ ЛИЧНЫЙ КАБИНЕТ</h2>

        <div style="margin-bottom:16px;">
            <asp:HyperLink runat="server" NavigateUrl="~/Default.aspx"
                CssClass="link">← НАЗАД В КАТАЛОГ</asp:HyperLink>
        </div>

        <%-- Информация о пользователе --%>
        <asp:Panel ID="pnlUserInfo" runat="server"
            Style="margin-bottom:24px; padding-bottom:16px;
                   border-bottom:1px solid var(--nier-gray);">
            <table class="search-form">
                <tr>
                    <td><span class="label-key">ФИО:</span></td>
                    <td><asp:Label ID="lblFullName" runat="server"
                        style="color:var(--nier-white); font-family:var(--font-mono);" /></td>
                </tr>
                <tr>
                    <td><span class="label-key">Email:</span></td>
                    <td><asp:Label ID="lblEmail" runat="server"
                        CssClass="mono" /></td>
                </tr>
                <tr>
                    <td><span class="label-key">Дата регистрации:</span></td>
                    <td><asp:Label ID="lblCreatedAt" runat="server"
                        CssClass="mono" /></td>
                </tr>
                <tr>
                    <td><span class="label-key">Активных броней:</span></td>
                    <td><asp:Label ID="lblActiveCount" runat="server"
                        CssClass="mono"
                        style="color:var(--nier-gold);" /></td>
                </tr>
            </table>
        </asp:Panel>

        <%-- Активные брони --%>
        <h3 style="margin-bottom:12px; letter-spacing:2px;">
            АКТИВНЫЕ БРОНИРОВАНИЯ
        </h3>

        <asp:Label ID="lblBookingsCount" runat="server"
            CssClass="results-count"
            Style="display:block; margin-bottom:8px;" />

        <asp:GridView ID="gvBookings" runat="server"
            AutoGenerateColumns="false"
            CssClass="books-grid"
            OnRowCommand="gvBookings_RowCommand"
            EmptyDataText="[OK] Активных бронирований нет"
            Style="table-layout:fixed; width:100%; word-wrap:break-word;">
            <Columns>

                <asp:TemplateField HeaderText="Книга" ItemStyle-Width="35%">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# GetBookTitle((int)Eval("BookID")) %>'
                            style="color:var(--nier-white);
                                   font-family:var(--font-mono);" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Автор">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# GetBookAuthor((int)Eval("BookID")) %>'
                            CssClass="mono" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Дата брони" ItemStyle-Width="110px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# ((DateTime)Eval("BookingDate")).ToString("dd.MM.yyyy") %>'
                            CssClass="mono"
                            style="color:var(--nier-pale); font-size:.8rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Действует до" ItemStyle-Width="110px">
                    <ItemTemplate>
                        <%-- Подсвечиваем красным если истекает сегодня-завтра --%>
                        <asp:Label runat="server"
                            Text='<%# ((DateTime)Eval("ExpiryDate")).ToString("dd.MM.yyyy") %>'
                            CssClass="mono"
                            style='<%# (DateTime)Eval("ExpiryDate") <= DateTime.Now.AddDays(1)
                                ? "color:var(--nier-error); font-size:.8rem; font-weight:700;"
                                : "color:var(--nier-warn); font-size:.8rem;" %>' />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Осталось" ItemStyle-Width="90px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# GetDaysLeft((DateTime)Eval("ExpiryDate")) %>'
                            CssClass="mono"
                            style="font-size:.8rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Статус" ItemStyle-Width="100px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# (string)Eval("Status") == "Active"
                                ? "АКТИВНА" : (string)Eval("Status") %>'
                            CssClass="mono status-available"
                            style="font-size:.78rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Действие" ItemStyle-Width="100px">
                    <ItemTemplate>
                        <asp:LinkButton runat="server"
                            CommandName="CancelBooking"
                            CommandArgument='<%# Eval("BookingID") %>'
                            Text="[ОТМЕНИТЬ]"
                            CssClass="action-link status-unavailable"
                            CausesValidation="false"
                            OnClientClick="return confirm('Отменить бронирование этой книги?');" />
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>

        <%-- История броней (завершённые/отменённые) --%>
        <asp:Panel ID="pnlHistory" runat="server"
            Style="margin-top:32px; border-top:1px solid var(--nier-gray);
                   padding-top:20px;">

            <h3 style="margin-bottom:12px; letter-spacing:2px;">
                ИСТОРИЯ БРОНИРОВАНИЙ
            </h3>

            <asp:GridView ID="gvHistory" runat="server"
                AutoGenerateColumns="false"
                CssClass="books-grid"
                EmptyDataText="[OK] История пуста">
                <Columns>

                    <asp:TemplateField HeaderText="Книга">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# GetBookTitle((int)Eval("BookID")) %>'
                                CssClass="mono"
                                style="color:var(--nier-pale);" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Дата брони" ItemStyle-Width="110px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# ((DateTime)Eval("BookingDate")).ToString("dd.MM.yyyy") %>'
                                CssClass="mono"
                                style="color:var(--nier-pale); font-size:.8rem;" />
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Статус" ItemStyle-Width="110px">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# GetStatusRussian((string)Eval("Status")) %>'
                                CssClass='<%# GetStatusCssClass((string)Eval("Status")) %>'
                                style="font-size:.78rem;" />
                        </ItemTemplate>
                    </asp:TemplateField>

                </Columns>
            </asp:GridView>
        </asp:Panel>

        <asp:Label ID="lblMessage" runat="server"
            CssClass="message" Visible="false"
            Style="margin-top:16px; display:block;" />

    </asp:Panel>
</form>
</body>
</html>


