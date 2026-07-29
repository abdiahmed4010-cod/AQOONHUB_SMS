<%@ Page Title="Payments | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Payments.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.Payments" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .students-wrap { padding: 1.25rem; max-width: 1440px; margin: 0 auto; }
        .stat-tile { display: flex; align-items: center; gap: .875rem; }
        .stat-tile .ic { width: 2.5rem; height: 2.5rem; border-radius: .6rem; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
        .stat-tile .lbl { font-size:.7rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; color:#6B7280; }
        .dark .stat-tile .lbl { color:#94A3B8; }
        .stat-tile .val { font-size:1.3rem; font-weight:800; line-height:1.15; }
        .filter-bar { display:flex; flex-wrap:wrap; align-items:center; gap:.625rem; }
        .filter-bar .grow { flex:1; min-width:200px; position:relative; }
        .filter-bar .grow svg { position:absolute; left:.75rem; top:50%; transform:translateY(-50%); color:#9CA3AF; width:1rem; height:1rem; }
        .filter-bar .grow input { padding-left:2.25rem; }
        @media (max-width: 768px) { .students-wrap { padding: .875rem; } }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="students-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Payments</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Payments</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">All payments recorded across every invoice.</p>
            </div>
        </div>

        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-5">
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="wallet" class="w-5 h-5"></i></span>
                <div><p class="lbl">Collected Today</p><p class="val"><asp:Label ID="lblCollectedToday" runat="server" Text="$0.00" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="calendar" class="w-5 h-5"></i></span>
                <div><p class="lbl">Collected This Month</p><p class="val"><asp:Label ID="lblCollectedMonth" runat="server" Text="$0.00" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="receipt" class="w-5 h-5"></i></span>
                <div><p class="lbl">Total Payments</p><p class="val"><asp:Label ID="lblTotalCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#FFFBEB;color:#F59E0B"><i data-lucide="banknote" class="w-5 h-5"></i></span>
                <div><p class="lbl">All-Time Collected</p><p class="val"><asp:Label ID="lblCollectedAllTime" runat="server" Text="$0.00" /></p></div>
            </div>
        </div>

        <div class="card p-3.5 mb-4 filter-bar">
            <div class="grow">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search by receipt no., invoice no., or student name…" />
            </div>
            <asp:DropDownList ID="ddlMethod" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Methods" Value="" />
                <asp:ListItem Text="Cash" Value="Cash" />
                <asp:ListItem Text="Bank Transfer" Value="Bank Transfer" />
                <asp:ListItem Text="Zaad" Value="Zaad" />
                <asp:ListItem Text="eDahab" Value="eDahab" />
                <asp:ListItem Text="Card" Value="Card" />
            </asp:DropDownList>
            <asp:TextBox ID="txtFromDate" runat="server" CssClass="input !w-auto" TextMode="Date" placeholder="From" />
            <asp:TextBox ID="txtToDate" runat="server" CssClass="input !w-auto" TextMode="Date" placeholder="To" />
            <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-primary" OnClick="btnSearch_Click">Search</asp:LinkButton>
            <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false">Reset</asp:LinkButton>
        </div>

        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gvPayments" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full" DataKeyNames="PaymentID">
                    <Columns>
                        <asp:BoundField DataField="ReceiptNo" HeaderText="Receipt No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Student">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex items-center gap-3">
                                    <span class="avatar" style='<%# "width:28px;height:28px;font-size:11px;background:" + GetAvatarColor(Eval("StudentName")) %>'><%# GetInitials(Eval("StudentName")) %></span>
                                    <p class="font-semibold"><%# Eval("StudentName") %></p>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Amount">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>$<%# Convert.ToDecimal(Eval("Amount")).ToString("N2") %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="PaymentMethod" HeaderText="Method" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="PaymentDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="Notes" HeaderText="Notes" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <asp:HyperLink runat="server" CssClass="btn btn-secondary !py-1 !px-3 !text-xs" NavigateUrl='<%# "~/Modules/Finance/InvoiceDetails.aspx?id=" + Eval("InvoiceID") %>'>View Invoice</asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="flex flex-col items-center justify-center py-16 text-center">
                            <span class="w-14 h-14 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-300 flex items-center justify-center mb-4">
                                <i data-lucide="wallet" class="w-7 h-7"></i>
                            </span>
                            <h3 class="font-bold">No payments found</h3>
                            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1 mb-4 max-w-sm">Try adjusting your search or filters. Payments are recorded from an invoice's details page.</p>
                            <a href="~/Modules/Finance/Invoices.aspx" runat="server" class="btn btn-primary">Go to Invoices</a>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
            <div class="flex items-center justify-between px-4 py-3 border-t border-gray-100 dark:border-slate-700 flex-wrap gap-2">
                <asp:Label runat="server" ID="lblResultsSummary" Text="Showing 0 of 0" CssClass="text-xs text-gray-500 dark:text-slate-400" />
                <div class="flex items-center gap-2">
                    <asp:LinkButton ID="btnPrevPage" runat="server" CssClass="btn btn-ghost !p-1.5" OnClick="btnPrevPage_Click"><i data-lucide="chevron-left" class="w-4 h-4"></i></asp:LinkButton>
                    <asp:Label runat="server" ID="lblPageIndicator" Text="Page 1 of 1" CssClass="text-xs" />
                    <asp:LinkButton ID="btnNextPage" runat="server" CssClass="btn btn-ghost !p-1.5" OnClick="btnNextPage_Click"><i data-lucide="chevron-right" class="w-4 h-4"></i></asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
