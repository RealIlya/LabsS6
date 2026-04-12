<%@ Page Language="C#" AutoEventWireup="true"
    CodeFile="ManageUsers.aspx.cs" Inherits="Admin_ManageUsers" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>[SYS] Управление пользователями</title>
    <link href="../Styles/nier-theme.css" rel="stylesheet" />
</head>
<body>
<form id="form1" runat="server">

    <asp:Panel ID="pnlMain" runat="server"
        CssClass="search-panel"
        Style="max-width:900px; margin:30px auto;">

        <h2>▍ УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ</h2>

        <div style="margin-bottom:20px;">
            <asp:HyperLink ID="hlBack" runat="server"
                NavigateUrl="~/Default.aspx"
                Text="[← В КАТАЛОГ]"
                CssClass="link" />
        </div>

        <asp:Label ID="lblUsersCount" runat="server" CssClass="results-count" />

        <asp:GridView ID="gvUsers" runat="server"
            AutoGenerateColumns="false"
            CssClass="books-grid"
            OnRowCommand="gvUsers_RowCommand"
            EmptyDataText="Пользователи не найдены в системе"
            Style="margin-top:12px;">
            <Columns>

                <asp:TemplateField HeaderText="ID" ItemStyle-Width="40px">
                    <ItemTemplate>
                        <asp:Label runat="server" Text='<%# Eval("UserID") %>'
                            CssClass="mono"
                            style="color:var(--nier-pale); font-size:.78rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="ФИО">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# Server.HtmlEncode((string)Eval("FullName")) %>'
                            style="color:var(--nier-white);" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Email">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# Server.HtmlEncode((string)Eval("Email")) %>'
                            CssClass="mono" style="font-size:.8rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Дата рег." ItemStyle-Width="120px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# ((DateTime)Eval("CreatedAt")).ToString("dd.MM.yyyy HH:mm") %>'
                            CssClass="mono"
                            style="color:var(--nier-pale); font-size:.78rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Роль" ItemStyle-Width="70px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# (string)Eval("Role") == "Admin" ? "ADMIN" : "USER" %>'
                            CssClass='<%# (string)Eval("Role") == "Admin"
                                ? "mono label-key" : "mono" %>'
                            style="font-size:.75rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Статус" ItemStyle-Width="110px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# (bool)Eval("IsActive") ? "АКТИВЕН" : "ЗАБЛОКИРОВАН" %>'
                            CssClass='<%# (bool)Eval("IsActive")
                                ? "mono status-available" : "mono status-unavailable" %>'
                            style="font-size:.78rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Броней" ItemStyle-Width="55px">
                    <ItemTemplate>
                        <asp:Label runat="server"
                            Text='<%# DataStore.GetUserActiveBookingsCount((int)Eval("UserID")) %>'
                            CssClass="mono"
                            style="color:var(--nier-gold); font-size:.82rem;" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Действия" ItemStyle-Width="260px">
                    <ItemTemplate>

                        <%-- Просмотр информации --%>
                        <asp:LinkButton runat="server"
                            CommandName="ViewInfo"
                            CommandArgument='<%# Eval("UserID") %>'
                            Text="[ИНФО]"
                            CssClass="action-link"
                            CausesValidation="false" />

                        <%-- Блокировка / разблокировка — только для не-админов --%>
                        <asp:LinkButton runat="server"
                            CommandName="ToggleActive"
                            CommandArgument='<%# Eval("UserID") %>'
                            Text='<%# (bool)Eval("IsActive")
                                ? "[ДЕАКТИВ.]" : "[АКТИВИР.]" %>'
                            CssClass='<%# (bool)Eval("IsActive")
                                ? "action-link status-unavailable"
                                : "action-link status-available" %>'
                            CausesValidation="false"
                            Visible='<%# (string)Eval("Role") != "Admin" %>'
                            OnClientClick='<%# (bool)Eval("IsActive")
                                ? "return confirm(\"Деактивировать пользователя?\");"
                                : "return confirm(\"Активировать пользователя?\");" %>' />

                        <%-- Выдача / снятие прав администратора --%>
                        <asp:LinkButton runat="server"
                            CommandName="ToggleAdmin"
                            CommandArgument='<%# Eval("UserID") %>'
                            Text='<%# (string)Eval("Role") == "Admin"
                                ? "[СНЯТЬ ПРАВА]" : "[СДЕЛАТЬ АДМИНОМ]" %>'
                            CssClass='<%# (string)Eval("Role") == "Admin"
                                ? "action-link status-unavailable"
                                : "action-link" %>'
                            CausesValidation="false"
                            OnClientClick='<%# (string)Eval("Role") == "Admin"
                                ? "return confirm(\"Снять права администратора?\");"
                                : "return confirm(\"Назначить администратором?\");" %>' />

                        <%-- Просмотр броней --%>
                        <asp:LinkButton runat="server"
                            CommandName="ViewBookings"
                            CommandArgument='<%# Eval("UserID") %>'
                            Text="[БРОНИ]"
                            CssClass="action-link"
                            CausesValidation="false"
                            Visible='<%# DataStore.GetUserActiveBookingsCount(
                                (int)Eval("UserID")) > 0 %>' />

                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
        </asp:GridView>

        <%-- Панель информации о пользователе --%>
        <asp:Panel ID="pnlUserInfo" runat="server" Visible="false"
            Style="margin-top:24px; border-top:1px solid var(--nier-gray);
                   padding-top:20px;">

            <p class="mono label-key" style="margin-bottom:14px;">
                ИНФОРМАЦИЯ О ПОЛЬЗОВАТЕЛЕ
            </p>

            <table class="search-form" style="margin-bottom:14px;">
                <tr>
                    <td style="color:var(--nier-pale); width:160px;">ФИО:</td>
                    <td><asp:Label ID="lblInfoName" runat="server"
                        style="color:var(--nier-white);" /></td>
                </tr>
                <tr>
                    <td style="color:var(--nier-pale);">Email:</td>
                    <td><asp:Label ID="lblInfoEmail" runat="server"
                        CssClass="mono" style="font-size:.9rem;" /></td>
                </tr>
                <tr>
                    <td style="color:var(--nier-pale);">Дата регистрации:</td>
                    <td><asp:Label ID="lblInfoCreated" runat="server"
                        CssClass="mono" style="font-size:.9rem;" /></td>
                </tr>
                <tr>
                    <td style="color:var(--nier-pale);">Роль:</td>
                    <td><asp:Label ID="lblInfoRole" runat="server" CssClass="mono" /></td>
                </tr>
                <tr>
                    <td style="color:var(--nier-pale);">Статус:</td>
                    <td><asp:Label ID="lblInfoStatus" runat="server" CssClass="mono" /></td>
                </tr>
                <tr>
                    <td style="color:var(--nier-pale);">Активных броней:</td>
                    <td><asp:Label ID="lblInfoBookings" runat="server"
                        CssClass="mono" style="color:var(--nier-gold);" /></td>
                </tr>
            </table>

            <asp:Button ID="btnCloseInfo" runat="server"
                Text="[X ЗАКРЫТЬ]"
                OnClick="btnCloseInfo_Click"
                CssClass="btn btn-secondary"
                CausesValidation="false" />
        </asp:Panel>

        <%-- Панель активных броней пользователя --%>
        <asp:Panel ID="pnlUserBookings" runat="server" Visible="false"
            Style="margin-top:24px; border-top:1px solid var(--nier-gray);
                   padding-top:20px;">

            <p class="mono label-key" style="margin-bottom:14px;">
                АКТИВНЫЕ БРОНИРОВАНИЯ:
                <asp:Label ID="lblSelectedUser" runat="server"
                    style="color:var(--nier-gold-lt); margin-left:10px;" />
            </p>

            <asp:GridView ID="gvBookings" runat="server"
                AutoGenerateColumns="false"
                CssClass="books-grid"
                EmptyDataText="Активных бронирований нет">
                <Columns>
                    <asp:TemplateField HeaderText="Книга">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# GetBookTitle((int)Eval("BookID")) %>'
                                style="color:var(--nier-white);" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Дата брони">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# ((DateTime)Eval("BookingDate")).ToString("dd.MM.yyyy") %>'
                                CssClass="mono"
                                style="color:var(--nier-pale); font-size:.8rem;" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Срок до">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# ((DateTime)Eval("ExpiryDate")).ToString("dd.MM.yyyy") %>'
                                CssClass="mono"
                                style="color:var(--nier-warn); font-size:.8rem;" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Статус">
                        <ItemTemplate>
                            <asp:Label runat="server"
                                Text='<%# (string)Eval("Status") %>'
                                CssClass="mono status-available"
                                style="font-size:.78rem;" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <div style="margin-top:12px;">
                <asp:Button ID="btnCloseBookings" runat="server"
                    Text="[X ЗАКРЫТЬ]"
                    OnClick="btnCloseBookings_Click"
                    CssClass="btn btn-secondary"
                    CausesValidation="false" />
            </div>
        </asp:Panel>

        <asp:Label ID="lblMessage" runat="server"
            CssClass="message" Visible="false"
            Style="margin-top:16px; display:block;" />

    </asp:Panel>
</form>
</body>
</html>

