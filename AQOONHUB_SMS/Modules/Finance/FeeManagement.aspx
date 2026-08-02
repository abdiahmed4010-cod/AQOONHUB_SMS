<%@ Page Title="Fee Management | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="FeeManagement.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.FeeManagement" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .fin-wrap { padding: 1.25rem; max-width: 1500px; margin: 0 auto; }
        .kpi-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:640px){ .kpi-grid { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .kpi-grid { grid-template-columns:repeat(5,1fr); } }
        .kpi { padding:1.15rem; }
        .kpi .ic { width:2.75rem; height:2.75rem; border-radius:.8rem; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
        .kpi .lbl { font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; color:#6B7280; }
        .dark .kpi .lbl { color:#94A3B8; }
        .kpi .val { font-size:1.55rem; font-weight:800; line-height:1.1; letter-spacing:-.02em; }
        .card-head { display:flex; align-items:center; justify-content:space-between; gap:.75rem; padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .dark .card-head { border-color:#334155; }
        .card-head h2 { font-size:.95rem; font-weight:800; }
        .card-head .sub { font-size:.72rem; color:#6B7280; margin-top:.1rem; }
        .dark .card-head .sub { color:#94A3B8; }
        .filter-bar { display:flex; flex-wrap:wrap; align-items:center; gap:.6rem; }
        .filter-bar .grow { flex:1; min-width:180px; position:relative; }
        .filter-bar .grow svg { position:absolute; left:.75rem; top:50%; transform:translateY(-50%); color:#9CA3AF; width:1rem; height:1rem; }
        .filter-bar .grow input { padding-left:2.25rem; }
        /* workflow cards */
        .wf-grid { display:grid; grid-template-columns:1fr; gap:.75rem; }
        @media (min-width:768px){ .wf-grid { grid-template-columns:repeat(4,1fr); } }
        .wf { display:flex; align-items:center; gap:.75rem; padding:1rem 1.15rem; text-decoration:none; transition:all .15s; }
        .wf:hover { border-color:#2563EB; transform:translateY(-1px); }
        .wf .num { width:2.1rem; height:2.1rem; border-radius:999px; display:flex; align-items:center; justify-content:center; font-weight:800; font-size:.85rem; flex-shrink:0; }
        .wf .t { font-size:.85rem; font-weight:700; }
        .wf .d { font-size:.7rem; color:#6B7280; }
        .dark .wf .d { color:#94A3B8; }
        .badge.Paid { background:#DCFCE7; color:#15803D; }
        .badge.Partial { background:#FEF3C7; color:#B45309; }
        .badge.Unpaid { background:#E2E8F0; color:#475569; }
        .badge.Overdue { background:#FEE2E2; color:#DC2626; }
        .badge.Cancelled { background:#F1F5F9; color:#64748B; }
        /* flow */
        .flow { display:flex; gap:.5rem; overflow-x:auto; padding:.25rem; }
        .flow .step { flex:1; min-width:130px; display:flex; flex-direction:column; align-items:center; text-align:center; gap:.5rem; padding:.5rem; }
        .flow .step .sic { width:2.6rem; height:2.6rem; border-radius:.8rem; display:flex; align-items:center; justify-content:center; background:#EFF6FF; color:#2563EB; }
        .dark .flow .step .sic { background:#1E293B; color:#93C5FD; }
        .flow .step .stt { font-size:.75rem; font-weight:700; }
        .flow .step .std { font-size:.66rem; color:#9CA3AF; line-height:1.25; }
        .flow .arrow { display:flex; align-items:center; color:#CBD5E1; flex-shrink:0; }
        .dark .flow .arrow { color:#475569; }
        @media (max-width:768px){ .fin-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fin-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span>Finance</span>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Fee Management</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Fee Management</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage fee collection, payments, balances and generate reports.</p>
            </div>
            <div class="flex items-center gap-2 flex-wrap">
                <a href="~/Modules/Finance/CreateInvoice.aspx" runat="server" class="btn btn-primary"><i data-lucide="plus" class="w-4 h-4"></i> Create Invoice</a>
                <a href="~/Modules/Finance/RecordPayment.aspx" runat="server" class="btn" style="background:#7C3AED;color:#fff"><i data-lucide="credit-card" class="w-4 h-4"></i> Record Payment</a>
                <a href="~/Modules/Finance/FeeReports.aspx" runat="server" class="btn btn-secondary"><i data-lucide="download" class="w-4 h-4"></i> Export</a>
            </div>
        </div>

        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="mb-4 rounded-lg bg-amber-50 text-amber-800 border border-amber-200 p-3 text-sm dark:bg-amber-500/10 dark:text-amber-300 dark:border-amber-500/30">
            <i data-lucide="alert-triangle" class="w-4 h-4 inline-block mr-1"></i><asp:Literal ID="litError" runat="server" />
        </asp:Panel>

        <!-- KPI cards -->
        <div class="kpi-grid mb-5">
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Total Invoices</p><p class="val"><asp:Literal ID="litTotalInvoices" runat="server" /></p></div>
                    <span class="ic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="file-text" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Collected This Month</p><p class="val"><asp:Literal ID="litCollected" runat="server" /></p></div>
                    <span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="dollar-sign" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Outstanding Balance</p><p class="val"><asp:Literal ID="litOutstanding" runat="server" /></p></div>
                    <span class="ic" style="background:#FFFBEB;color:#F59E0B"><i data-lucide="wallet" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Overdue Invoices</p><p class="val"><asp:Literal ID="litOverdue" runat="server" /></p></div>
                    <span class="ic" style="background:#FEF2F2;color:#EF4444"><i data-lucide="alarm-clock" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Payment Success Rate</p><p class="val"><asp:Literal ID="litSuccess" runat="server" /></p></div>
                    <span class="ic" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="pie-chart" class="w-5 h-5"></i></span>
                </div>
            </div>
        </div>

        <!-- Filter row -->
        <div class="card p-3.5 mb-5 filter-bar">
            <div class="grow">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search by student name, invoice no., category…" />
            </div>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input !w-auto">
                <asp:ListItem Value="">All Statuses</asp:ListItem>
                <asp:ListItem>Unpaid</asp:ListItem>
                <asp:ListItem>Partial</asp:ListItem>
                <asp:ListItem>Paid</asp:ListItem>
                <asp:ListItem>Overdue</asp:ListItem>
                <asp:ListItem>Cancelled</asp:ListItem>
            </asp:DropDownList>
            <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" />
        </div>

        <!-- Workflow cards -->
        <div class="wf-grid mb-5">
            <a href="~/Modules/Finance/CreateInvoice.aspx" runat="server" class="card wf">
                <span class="num" style="background:#EFF6FF;color:#2563EB">1</span>
                <div><p class="t">Fee Collection</p><p class="d">Create invoices and collect fees</p></div>
            </a>
            <a href="~/Modules/Finance/RecordPayment.aspx" runat="server" class="card wf">
                <span class="num" style="background:#ECFDF5;color:#22C55E">2</span>
                <div><p class="t">Payment Recording</p><p class="d">Record payments from students</p></div>
            </a>
            <a href="~/Modules/Finance/BalanceTracking.aspx" runat="server" class="card wf">
                <span class="num" style="background:#FFFBEB;color:#F59E0B">3</span>
                <div><p class="t">Balance Tracking</p><p class="d">Track outstanding balances</p></div>
            </a>
            <a href="~/Modules/Finance/FeeReports.aspx" runat="server" class="card wf">
                <span class="num" style="background:#F5F3FF;color:#7C3AED">4</span>
                <div><p class="t">Fee Reports</p><p class="d">View reports and analytics</p></div>
            </a>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-5 mb-5">
            <!-- Left: quick links -->
            <div class="flex flex-col gap-5">
                <div class="card overflow-hidden">
                    <div class="card-head">
                        <div><h2>Fee Categories &amp; Structure</h2><p class="sub">Manage all fee types</p></div>
                        <a href="~/Modules/Finance/FeeStructures.aspx" runat="server" class="text-xs font-semibold text-brand-600 hover:underline">View All</a>
                    </div>
                    <div class="p-4 flex flex-col gap-2">
                        <a href="~/Modules/Finance/FeeCategories.aspx" runat="server" class="flex items-center gap-3 p-2.5 rounded-lg hover:bg-gray-50 dark:hover:bg-slate-700/40">
                            <span class="w-9 h-9 rounded-lg flex items-center justify-center" style="background:#EFF6FF;color:#2563EB"><i data-lucide="tags" class="w-4 h-4"></i></span>
                            <div><p class="text-sm font-semibold">Fee Categories</p><p class="text-[11px] text-gray-400">Create &amp; manage fee types</p></div>
                        </a>
                        <a href="~/Modules/Finance/FeeStructures.aspx" runat="server" class="flex items-center gap-3 p-2.5 rounded-lg hover:bg-gray-50 dark:hover:bg-slate-700/40">
                            <span class="w-9 h-9 rounded-lg flex items-center justify-center" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="layers" class="w-4 h-4"></i></span>
                            <div><p class="text-sm font-semibold">Fee Structures</p><p class="text-[11px] text-gray-400">Per-class fee setup</p></div>
                        </a>
                        <a href="~/Modules/Finance/Invoices.aspx" runat="server" class="flex items-center gap-3 p-2.5 rounded-lg hover:bg-gray-50 dark:hover:bg-slate-700/40">
                            <span class="w-9 h-9 rounded-lg flex items-center justify-center" style="background:#ECFDF5;color:#22C55E"><i data-lucide="receipt" class="w-4 h-4"></i></span>
                            <div><p class="text-sm font-semibold">All Invoices</p><p class="text-[11px] text-gray-400">Browse &amp; manage invoices</p></div>
                        </a>
                    </div>
                </div>
            </div>

            <!-- Center: Student Invoices -->
            <div class="card overflow-hidden lg:col-span-1">
                <div class="card-head">
                    <div><h2>Student Invoices</h2><p class="sub">Current invoice and balance activity</p></div>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full">
                        <thead>
                            <tr>
                                <th class="th" scope="col">Invoice</th><th class="th" scope="col">Student</th><th class="th" scope="col">Fee Type</th>
                                <th class="th" scope="col">Amount</th><th class="th" scope="col">Balance</th><th class="th" scope="col">Status</th><th class="th" scope="col">Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptInvoices" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td class="td"><a class="font-semibold text-brand-600 hover:underline" href='ViewInvoice.aspx?id=<%# Eval("InvoiceID") %>'><%#: Eval("InvoiceNumber") %></a></td>
                                        <td class="td"><%#: Eval("StudentName") %><span class="block text-[11px] text-gray-400"><%#: Eval("StudentCode") %> · <%#: Eval("ClassName") %></span></td>
                                        <td class="td"><%#: Eval("FeeType") %></td>
                                        <td class="td"><%# Money(Eval("TotalAmount")) %></td>
                                        <td class="td"><%# Money(Eval("Balance")) %></td>
                                        <td class="td"><span class='badge <%# Eval("StatusText") %>'><%#: Eval("StatusText") %></span></td>
                                        <td class="td whitespace-nowrap">
                                            <div class="flex items-center gap-2">
                                                <a class="text-brand-600 hover:underline" href='ViewInvoice.aspx?id=<%# Eval("InvoiceID") %>'>View</a>
                                                <a class="text-brand-600 hover:underline" href='RecordPayment.aspx?invoiceId=<%# Eval("InvoiceID") %>'>Pay</a>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- Right: Recent Payments -->
            <div class="card overflow-hidden">
                <div class="card-head">
                    <div><h2>Recent Payments</h2><p class="sub">Latest recorded payments</p></div>
                    <a href="~/Modules/Finance/Payments.aspx" runat="server" class="text-xs font-semibold text-brand-600 hover:underline">View All</a>
                </div>
                <div class="p-2">
                    <asp:Repeater ID="rptPayments" runat="server">
                        <ItemTemplate>
                            <div class="flex items-center justify-between gap-3 p-2.5 border-b border-gray-100 dark:border-slate-700 last:border-0">
                                <div class="min-w-0">
                                    <p class="text-sm font-semibold truncate"><%#: Eval("StudentName") %></p>
                                    <p class="text-[11px] text-gray-400"><%#: Eval("PaymentMethod") %> · <%# Date(Eval("PaymentDate")) %></p>
                                </div>
                                <b class="text-green-600 whitespace-nowrap"><%# Money(Eval("AmountPaid")) %></b>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <!-- Process flow -->
        <div class="card overflow-hidden">
            <div class="card-head">
                <div><h2>Fee Management Process Flow</h2><p class="sub">From fee structure to reports</p></div>
            </div>
            <div class="p-4">
                <div class="flow">
                    <div class="step"><span class="sic"><i data-lucide="layers" class="w-5 h-5"></i></span><span class="stt">1. Fee Structure</span><span class="std">Define fee categories and amounts</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="file-plus" class="w-5 h-5"></i></span><span class="stt">2. Create Invoice</span><span class="std">Generate invoice for students</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="credit-card" class="w-5 h-5"></i></span><span class="stt">3. Record Payment</span><span class="std">Record payments against invoice</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="scale" class="w-5 h-5"></i></span><span class="stt">4. Track Balance</span><span class="std">Monitor outstanding balances</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="bell" class="w-5 h-5"></i></span><span class="stt">5. Reminders</span><span class="std">Send reminders for due payments</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="bar-chart-3" class="w-5 h-5"></i></span><span class="stt">6. Reports</span><span class="std">Generate reports and analytics</span></div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
