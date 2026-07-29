<%@ Page Title="Fee Reports | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="FeeReports.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.FeeReports" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .fr-wrap { padding:1.25rem; max-width:1440px; margin:0 auto; }
        .kpi-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:1024px){ .kpi-grid { grid-template-columns:repeat(4,1fr); } }
        .kpi { padding:1.15rem; }
        .kpi .ic { width:2.75rem; height:2.75rem; border-radius:.8rem; display:flex; align-items:center; justify-content:center; }
        .kpi .lbl { font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; color:#6B7280; }
        .dark .kpi .lbl { color:#94A3B8; }
        .kpi .val { font-size:1.55rem; font-weight:800; line-height:1.1; letter-spacing:-.02em; }
        .card-head { display:flex; align-items:center; justify-content:space-between; gap:.75rem; padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .dark .card-head { border-color:#334155; }
        .card-head h2 { font-size:.95rem; font-weight:800; }
        @media (max-width:768px){ .fr-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fr-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span>Finance</span>
            <span>/</span><a href="~/Modules/Finance/FeeManagement.aspx" runat="server" class="hover:text-brand-600">Fee Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Fee Reports</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Fee Reports</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Collection, outstanding balance and invoice status reporting.</p>
            </div>
            <div class="flex items-center gap-2">
                <asp:Button ID="csv" runat="server" Text="Export CSV" CssClass="btn btn-primary" OnClick="csv_Click" />
                <button type="button" class="btn btn-secondary" onclick="window.print()"><i data-lucide="printer" class="w-4 h-4"></i> Print / PDF</button>
            </div>
        </div>

        <!-- KPI cards -->
        <div class="kpi-grid mb-5">
            <div class="card kpi"><div class="flex items-start justify-between"><div><p class="lbl">Total Invoices</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div><span class="ic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="file-text" class="w-5 h-5"></i></span></div></div>
            <div class="card kpi"><div class="flex items-start justify-between"><div><p class="lbl">Collected This Month</p><p class="val"><asp:Literal ID="litCollected" runat="server" Text="$0.00" /></p></div><span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="dollar-sign" class="w-5 h-5"></i></span></div></div>
            <div class="card kpi"><div class="flex items-start justify-between"><div><p class="lbl">Outstanding Balance</p><p class="val"><asp:Literal ID="litOutstanding" runat="server" Text="$0.00" /></p></div><span class="ic" style="background:#FFFBEB;color:#F59E0B"><i data-lucide="wallet" class="w-5 h-5"></i></span></div></div>
            <div class="card kpi"><div class="flex items-start justify-between"><div><p class="lbl">Payment Success Rate</p><p class="val"><asp:Literal ID="litSuccess" runat="server" Text="0.00%" /></p></div><span class="ic" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="pie-chart" class="w-5 h-5"></i></span></div></div>
        </div>

        <div class="card overflow-hidden">
            <div class="card-head">
                <div><h2>Invoice Report</h2></div>
                <div class="flex items-center gap-2">
                    <asp:DropDownList ID="status" runat="server" CssClass="input !w-auto">
                        <asp:ListItem Value="">All Statuses</asp:ListItem>
                        <asp:ListItem>Paid</asp:ListItem>
                        <asp:ListItem>Partial</asp:ListItem>
                        <asp:ListItem>Unpaid</asp:ListItem>
                        <asp:ListItem>Overdue</asp:ListItem>
                    </asp:DropDownList>
                    <asp:Button ID="view" runat="server" Text="View Report" CssClass="btn btn-primary" OnClick="view_Click" />
                </div>
            </div>
            <div class="overflow-x-auto">
                <asp:GridView ID="grid" runat="server" AutoGenerateColumns="false" CssClass="w-full" GridLines="None">
                    <Columns>
                        <asp:HyperLinkField DataTextField="InvoiceNumber" DataNavigateUrlFields="InvoiceID" DataNavigateUrlFormatString="ViewInvoice.aspx?id={0}" HeaderText="Invoice" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" ControlStyle-CssClass="text-brand-600 font-semibold hover:underline" />
                        <asp:BoundField DataField="StudentName" HeaderText="Student" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="ClassName" HeaderText="Class" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="FeeType" HeaderText="Fee Type" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="PaidAmount" HeaderText="Paid" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="Balance" HeaderText="Balance" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="DueDate" HeaderText="Due Date" DataFormatString="{0:dd MMM yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Status">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate><span class="badge" style='<%# StatusStyle(Eval("StatusText")) %>'><%#: Eval("StatusText") %></span></ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="py-16 text-center text-sm text-gray-500 dark:text-slate-400">No invoices match the selected filter.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
