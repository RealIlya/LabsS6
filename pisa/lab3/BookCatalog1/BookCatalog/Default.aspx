<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Книжный каталог</title>
    <link href="Styles/nier-theme.css?v=7" rel="stylesheet" />
    <style type="text/css">
        .ajax-loader {
            display: inline-block;
            margin-left: 10px;
            color: #c8a96e;
            font-family: monospace;
        }
        .ajax-info {
            background: rgba(200, 169, 110, 0.1);
            border: 1px solid #c8a96e;
            padding: 8px 12px;
            margin: 5px 0;
            font-family: monospace;
            color: #c8a96e;
        }
        .search-hint {
            font-size: 0.85em;
            color: #888;
            font-family: monospace;
            margin-top: 4px;
        }
        .page-layout {
            display: flex;
            gap: 20px;
            max-width: 1400px;
            margin: 0 auto;
            padding: 10px 20px;
        }
        .ad-slot {
            width: 220px;
            min-width: 220px;
            background: #d4d0c4;
            border: 1px solid #8a8679;
            padding: 12px;
            font-family: monospace;
            font-size: 0.8em;
            color: #3a3630;
            text-align: center;
            height: fit-content;
            position: sticky;
            top: 20px;
        }
        .ad-slot-title {
            color: #3a3630;
            font-size: 0.85em;
            margin-bottom: 8px;
            text-transform: uppercase;
            letter-spacing: 2px;
            font-weight: bold;
        }
        .ad-placeholder {
            background: #b8b4a7;
            border: 1px solid #8a8679;
            padding: 15px 10px;
            margin: 10px 0;
            color: #141410;
            font-size: 0.75em;
        }
        .content-center {
            flex: 1;
            min-width: 0;
        }
        @media (max-width: 1100px) {
            .ad-slot { display: none; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />

        <asp:Label ID="lblDbStatus" runat="server" Visible="false" CssClass="info-message" Style="display:block; margin:0 20px 10px;" />
        
        <asp:Panel ID="pnlHeader" runat="server" CssClass="header">
            <h1>// КНИЖНЫЙ КАТАЛОГ</h1>
            <asp:PlaceHolder ID="phAuth" runat="server">
                <asp:HyperLink ID="hlLogin" runat="server" NavigateUrl="~/Login.aspx" CssClass="link">Войти</asp:HyperLink> |
                <asp:HyperLink ID="hlRegister" runat="server" NavigateUrl="~/Register.aspx" CssClass="link">Регистрация</asp:HyperLink>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phUser" runat="server" Visible="false">
                Привет, <asp:Label ID="lblUserName" runat="server" CssClass="username" /> |
                <asp:HyperLink runat="server" NavigateUrl="~/UserProfile.aspx" CssClass="link">[МОЙ ПРОФИЛЬ]</asp:HyperLink> |
                <asp:LinkButton ID="lbLogout" runat="server" OnClick="lbLogout_Click" CssClass="link" CausesValidation="false">Выйти</asp:LinkButton>
                <asp:PlaceHolder ID="phAdmin" runat="server" Visible="false">
                    | <asp:HyperLink ID="hlAddBook" runat="server" NavigateUrl="~/Admin/AddBook.aspx" CssClass="link admin">[ДОБАВИТЬ КНИГУ]</asp:HyperLink>
                    | <asp:HyperLink ID="hlWriteOff" runat="server" NavigateUrl="~/WriteOff.aspx" CssClass="link admin">[СПИСАНИЕ]</asp:HyperLink>
                    | <asp:HyperLink ID="hlManageUsers" runat="server" NavigateUrl="~/Admin/ManageUsers.aspx" CssClass="link admin">[ПОЛЬЗОВАТЕЛИ]</asp:HyperLink>
                </asp:PlaceHolder>
            </asp:PlaceHolder>
        </asp:Panel>

        <div class="page-layout">
            <%-- Левый рекламный слот: Писатель Aboba --%>
            <aside class="ad-slot">
                <div class="ad-slot-title">📚 НОВИНКА</div>
                <div class="ad-placeholder" style="border-color: #6a5a40;">
                    <div style="font-size: 13px; color: #3a3630; margin-bottom: 6px;">АВТОР БЕСТСЕЛЛЕРОВ</div>
                    <div style="font-size: 16px; color: #0a0a0a; font-weight: bold; margin-bottom: 10px;">ᅠАBOBAᅠ</div>
                    <div style="font-size: 11px; color: #2a2620; text-align: left; line-height: 1.8;">
                        📖 «Абоба Непоколебимая»<br />
                        📖 «Абоба и Тёмный Лес»<br />
                        📖 «Абоба: Последний Рассвет»<br />
                        📖 «Великая Абоба»<br />
                        📖 «Абоба на Краю Мира»
                    </div>
                    <div style="margin-top: 10px; font-size: 10px; color: #6a5a40;">— «Читается залпом!» — Критик</div>
                </div>
                <div class="ad-placeholder" style="border-color: #6a5a40;">
                    <div style="font-size: 12px; color: #3a3630; margin-bottom: 6px;">🔥 СКИДКА 90%</div>
                    <div style="font-size: 11px; color: #141410; line-height: 1.6;">
                        Все книги Абобы<br />
                        <span style="font-size: 18px; color: #0a0a0a; font-weight: bold;">за 99₽</span><br />
                        <span style="font-size: 10px; color: #6a5a40;">*навсегда</span>
                    </div>
                </div>
            </aside>

            <%-- Центральный контент --%>
            <div class="content-center">

        <%-- AJAX: UpdatePanel для поиска и результатов без перезагрузки --%>
        <asp:UpdatePanel ID="upSearchAndResults" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:Panel ID="pnlSearch" runat="server" CssClass="search-panel">
                    <h2>>> ПОИСК ДАННЫХ</h2>
                    <table class="search-form">
                        <tr>
                            <td><asp:Label ID="lblTitle" runat="server" Text="Название:" AssociatedControlID="txtTitle" /></td>
                            <td>
                                <asp:TextBox ID="txtTitle" runat="server" CssClass="input" placeholder="Введите название..." />
                                <div class="search-hint">Поиск по частичному совпадению</div>
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblAuthor" runat="server" Text="Автор:" AssociatedControlID="txtAuthor" /></td>
                            <td>
                                <asp:TextBox ID="txtAuthor" runat="server" CssClass="input" placeholder="Имя автора..." />
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblGenre" runat="server" Text="Жанр:" AssociatedControlID="ddlGenre" /></td>
                            <td>
                                <asp:DropDownList ID="ddlGenre" runat="server" CssClass="input">
                                    <asp:ListItem Value="" Text="-- Все жанры --" />
                                    <asp:ListItem Value="fiction" Text="Художественная" />
                                    <asp:ListItem Value="science" Text="Научная" />
                                    <asp:ListItem Value="education" Text="Учебная" />
                                    <asp:ListItem Value="children" Text="Детская" />
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td><asp:Label ID="lblYear" runat="server" Text="Год:" AssociatedControlID="txtYearFrom" /></td>
                            <td>
                                <asp:TextBox ID="txtYearFrom" runat="server" CssClass="input" placeholder="от..." Style="width:45%; display:inline-block;" />
                                <span style="margin:0 4px;">—</span>
                                <asp:TextBox ID="txtYearTo" runat="server" CssClass="input" placeholder="до..." Style="width:45%; display:inline-block;" />
                                <asp:RangeValidator ID="rvYearFrom" runat="server" ControlToValidate="txtYearFrom" Type="Integer" MinimumValue="1000" MaximumValue="2100" ErrorMessage="Год «от»: 1000–2100" Display="Dynamic" EnableClientScript="true" CssClass="error" ValidationGroup="SearchGroup" />
                                <asp:RangeValidator ID="rvYearTo" runat="server" ControlToValidate="txtYearTo" Type="Integer" MinimumValue="1000" MaximumValue="2100" ErrorMessage="Год «до»: 1000–2100" Display="Dynamic" EnableClientScript="true" CssClass="error" ValidationGroup="SearchGroup" />
                                <div class="search-hint">Интервал годов издания (можно оставить пустым)</div>
                            </td>
                        </tr>
                    </table>
                    <asp:Button ID="btnSearch" runat="server" Text=">> ИНИЦИИРОВАТЬ ПОИСК" OnClick="btnSearch_Click" CssClass="btn btn-primary" ValidationGroup="SearchGroup" />
                    <asp:Button ID="btnReset" runat="server" Text="[X] СБРОС" OnClick="btnReset_Click" CssClass="btn btn-secondary" CausesValidation="false" />

                    <%-- AJAX: индикатор загрузки UpdateProgress --%>
                    <asp:UpdateProgress ID="updProgress" runat="server" AssociatedUpdatePanelID="upSearchAndResults" DisplayAfter="100">
                        <ProgressTemplate>
                            <div class="ajax-loader">[~] Загрузка данных...</div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>
                </asp:Panel>

                <asp:Panel ID="pnlResults" runat="server" CssClass="results-panel">
                    <asp:Label ID="lblResultsCount" runat="server" CssClass="results-count mono" />
                    <asp:GridView ID="gvBooks" runat="server" AutoGenerateColumns="false" OnRowCommand="gvBooks_RowCommand" CssClass="books-grid" EmptyDataText="">
                        <EmptyDataTemplate>
                            <asp:Label ID="lblEmpty" runat="server" CssClass="mono" Style="color:#aaa; display:block; padding:20px 0;"></asp:Label>
                            <asp:Panel runat="server" CssClass="mono" Style="margin-top:8px;">
                                [?] Попробуйте: изменить поисковый запрос, выбрать другой жанр или сбросить фильтры кнопкой [X] СБРОС
                            </asp:Panel>
                        </EmptyDataTemplate>
                        <Columns>
                            <asp:BoundField DataField="Title" HeaderText="Название" ItemStyle-CssClass="col-title" HeaderStyle-CssClass="col-title" />
                            <asp:BoundField DataField="Author" HeaderText="Автор" ItemStyle-CssClass="col-author" HeaderStyle-CssClass="col-author" />
                            <asp:BoundField DataField="Year" HeaderText="Год" ItemStyle-CssClass="col-year" HeaderStyle-CssClass="col-year" />
                            <asp:TemplateField HeaderText="Жанр" ItemStyle-CssClass="col-genre" HeaderStyle-CssClass="col-genre">
                                <ItemTemplate>
                                    <asp:Label runat="server" Text='<%# _Default.GenreToRussian(Eval("Genre").ToString()) %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Доступно" ItemStyle-CssClass="col-available" HeaderStyle-CssClass="col-available">
                                <ItemTemplate>
                                    <asp:Label ID="lblAvailable" runat="server" Text='<%# Eval("AvailableCount") %>' CssClass='<%# (int)Eval("AvailableCount") > 0 ? "status-available" : "status-unavailable" %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Действия" ItemStyle-CssClass="col-actions" HeaderStyle-CssClass="col-actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lbDetails" runat="server" CommandName="ViewDetails" CommandArgument='<%# Eval("BookID") %>' Text="[ПОДРОБНЕЕ]" CssClass="action-link mono" CausesValidation="false" /><br />
                                    <asp:LinkButton ID="lbBook" runat="server" CommandName="Book" CommandArgument='<%# Eval("BookID") %>' Text="[ЗАБРОНИРОВАТЬ]" CssClass="action-link book-btn mono" CausesValidation="false" Enabled='<%# (int)Eval("AvailableCount") > 0 %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </asp:Panel>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="btnReset" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>

        <asp:ValidationSummary ID="vsErrors" runat="server" ShowMessageBox="false" CssClass="error-summary" ValidationGroup="SearchGroup" />

            </div><%-- /content-center --%>

            <%-- Правый рекламный слот: Казино Абобы --%>
            <aside class="ad-slot">
                <div class="ad-slot-title">🎰 КАЗИНО АБОБЫ</div>
                <div class="ad-placeholder" style="border-color: #6a3a0a;">
                    <div style="font-size: 14px; color: #3a3630; font-weight: bold; margin-bottom: 8px;">🎰 ABOBA CASINO 🎰</div>
                    <div style="font-size: 11px; color: #141410; line-height: 1.7;">
                        Играй в слоты<br />
                        по мотивам книг<br />
                        <span style="color: #3a3630;">«Абоба Непоколебимая»</span><br />
                        <span style="font-size: 16px; color: #0a0a0a; font-weight: bold;">ДЖЕКПОТ: 777₽</span>
                    </div>
                </div>
                <div class="ad-placeholder" style="border-color: #6a3a0a; padding: 12px 8px;">
                    <div style="font-size: 11px; color: #3a3630; margin-bottom: 8px;">🎰 ИСПЫТАЙ УДАЧУ</div>
                    <div id="slot-machine" style="display: flex; justify-content: center; gap: 6px; margin-bottom: 10px;">
                        <div class="slot-reel" style="width: 36px; height: 44px; background: #0a0a0a; border: 2px solid #6a5a40; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 22px; font-weight: bold; color: #c9c5b8; font-family: monospace;">?</div>
                        <div class="slot-reel" style="width: 36px; height: 44px; background: #0a0a0a; border: 2px solid #6a5a40; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 22px; font-weight: bold; color: #c9c5b8; font-family: monospace;">?</div>
                        <div class="slot-reel" style="width: 36px; height: 44px; background: #0a0a0a; border: 2px solid #6a5a40; border-radius: 4px; display: flex; align-items: center; justify-content: center; font-size: 22px; font-weight: bold; color: #c9c5b8; font-family: monospace;">?</div>
                    </div>
                    <button type="button" id="btn-spin" onclick="spinSlots()" style="background: #6a3a0a; color: #c9c5b8; border: none; padding: 6px 18px; font-family: monospace; font-size: 12px; font-weight: bold; cursor: pointer; letter-spacing: 1px; text-transform: uppercase;">🎲 Крутить</button>
                    <div id="slot-result" style="margin-top: 8px; font-size: 11px; color: #141410; min-height: 16px;"></div>
                    <div style="margin-top: 6px; font-size: 10px; color: #8a8679;">18+ | Играйте ответственно</div>
                </div>
            </aside>

        </div><%-- /page-layout --%>
    </form>

    <script type="text/javascript" src="Scripts/ajax-validation.js"></script>
    <script type="text/javascript">
        (function() {
            var reels = document.querySelectorAll('.slot-reel');
            var btn = document.getElementById('btn-spin');
            var result = document.getElementById('slot-result');
            var spinning = false;
            var symbols = ['0','1','2','3','4','5','6','7','8','9'];
            var intervals = [];

            window.spinSlots = function() {
                if (spinning) return;
                spinning = true;
                btn.disabled = true;
                btn.style.opacity = '0.5';
                result.innerHTML = '';

                // Анимация вращения
                for (var i = 0; i < 3; i++) {
                    (function(idx) {
                        intervals[idx] = setInterval(function() {
                            reels[idx].textContent = symbols[Math.floor(Math.random() * symbols.length)];
                        }, 80);
                    })(i);
                }

                // Остановка по очереди
                var finalNums = [
                    Math.floor(Math.random() * 10),
                    Math.floor(Math.random() * 10),
                    Math.floor(Math.random() * 10)
                ];

                setTimeout(function() { clearInterval(intervals[0]); reels[0].textContent = finalNums[0]; }, 800);
                setTimeout(function() { clearInterval(intervals[1]); reels[1].textContent = finalNums[1]; }, 1200);
                setTimeout(function() {
                    clearInterval(intervals[2]);
                    reels[2].textContent = finalNums[2];
                    // Результат
                    var sum = finalNums[0] + finalNums[1] + finalNums[2];
                    var discount = sum * 3;
                    var triple = (finalNums[0] === finalNums[1] && finalNums[1] === finalNums[2]);
                    if (triple) {
                        result.innerHTML = '<b style="font-size:14px;">🎉 ДЖЕКПОТ! Скидка 99%!</b>';
                    } else {
                        result.innerHTML = 'Ваша скидка: <b>' + discount + '%</b> на книги Абобы!';
                    }
                    spinning = false;
                    btn.disabled = false;
                    btn.style.opacity = '1';
                }, 1600);
            };
        })();
    </script>
</body>
</html>
