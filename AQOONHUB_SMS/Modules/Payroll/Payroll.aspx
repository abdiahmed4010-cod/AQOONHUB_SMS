<%@ Page Title="Payroll Management"
    Language="C#"
    MasterPageFile="~/MasterPages/MainMaster.master"
    AutoEventWireup="true"
    CodeBehind="Payroll.aspx.cs"
    Inherits="AQOONHUB_SMS.Modules.Payroll.Payroll" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .pr-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .kpi-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:640px){ .kpi-grid { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .kpi-grid { grid-template-columns:repeat(5,1fr); } }
        .kpi { padding:1.15rem; }
        .kpi .ic { width:2.75rem; height:2.75rem; border-radius:.8rem; display:flex; align-items:center; justify-content:center; }
        .kpi .lbl { font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; color:#6B7280; }
        .kpi .val { font-size:1.5rem; font-weight:800; line-height:1.1; letter-spacing:-.02em; }
        .card-head { display:flex; align-items:center; justify-content:space-between; gap:.75rem; padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .card-head h2 { font-size:.95rem; font-weight:800; }
        .card-head .sub { font-size:.72rem; color:#6B7280; margin-top:.1rem; }
        .tabbar { display:flex; gap:.25rem; overflow-x:auto; border-bottom:1px solid #E5E7EB; }
        .tabbar a { padding:.65rem 1rem; font-size:.83rem; font-weight:700; color:#64748B; border-bottom:2px solid transparent; white-space:nowrap; text-decoration:none; }
        .tabbar a.active { color:#2563EB; border-color:#2563EB; }
        .tabbar a:hover { color:#2563EB; }
        .sum-row { display:flex; justify-content:space-between; gap:1rem; font-size:.82rem; padding:.5rem 0; border-bottom:1px solid #F1F5F9; }
        .sum-row:last-child { border-bottom:none; }
        .sum-row .k { color:#475569; display:flex; align-items:center; gap:.5rem; }
        .sum-row .v { font-weight:800; }
        .donut { width:9.5rem; height:9.5rem; border-radius:50%; position:relative; flex-shrink:0; background:conic-gradient(#E5E7EB 0 100%); }
        .donut::after { content:''; position:absolute; inset:1.7rem; background:#fff; border-radius:50%; }
        .donut .center { position:absolute; inset:0; display:flex; flex-direction:column; align-items:center; justify-content:center; }
        .payroll-table { width:100%; border-collapse:collapse; }
        .payroll-table th { padding:.8rem 1rem; border-bottom:1px solid #e2e8f0; background:#f8fafc; color:#475569; font-size:.7rem; font-weight:700; letter-spacing:.05em; text-align:left; text-transform:uppercase; white-space:nowrap; }
        .payroll-table td { padding:.85rem 1rem; border-bottom:1px solid #f1f5f9; color:#334155; font-size:.85rem; white-space:nowrap; }
        .payroll-table tr:hover td { background:#f8fafc; }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="pr-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span>Finance</span>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Payroll</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Payroll Management</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage staff salaries, pay runs, adjustments, deductions, payments, and payslips.</p>
            </div>
            <div class="flex flex-wrap gap-2">
                <a href="PayrollPeriods.aspx" class="btn btn-secondary"><i data-lucide="calendar-range" class="w-4 h-4"></i> Manage Periods</a>
                <asp:LinkButton ID="btnGeneratePayroll" runat="server" CausesValidation="false" OnClick="btnGeneratePayroll_Click" CssClass="btn btn-primary">
                    <i data-lucide="plus" class="w-4 h-4"></i> Create Pay Run
                </asp:LinkButton>
            </div>
        </div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" role="alert" CssClass="rounded-xl border px-4 py-3 shadow-sm mb-5">
            <div class="flex items-start gap-3">
                <i data-lucide="info" class="mt-0.5 h-5 w-5 shrink-0"></i>
                <asp:Label ID="lblMessage" runat="server" CssClass="text-sm font-medium" />
            </div>
        </asp:Panel>

        <!-- ===== KPI CARDS ===== -->
        <div class="kpi-grid mb-5">
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Total Employees</p><asp:Label ID="lblTotalEmployees" runat="server" Text="0" CssClass="val block mt-1" /></div>
                    <span class="ic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="users" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">This Month Payroll</p><asp:Label ID="lblGrossSalary" runat="server" Text="$0.00" CssClass="val block mt-1 text-blue-700" /></div>
                    <span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="circle-dollar-sign" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Net Pay</p><asp:Label ID="lblNetSalary" runat="server" Text="$0.00" CssClass="val block mt-1 text-indigo-700" /></div>
                    <span class="ic" style="background:#EEF2FF;color:#4F46E5"><i data-lucide="banknote" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Paid Amount</p><asp:Label ID="lblPaidAmount" runat="server" Text="$0.00" CssClass="val block mt-1 text-emerald-700 js-paid" /></div>
                    <span class="ic" style="background:#ECFDF5;color:#16A34A"><i data-lucide="circle-check-big" class="w-5 h-5"></i></span>
                </div>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Pending Amount</p><asp:Label ID="lblPendingAmount" runat="server" Text="$0.00" CssClass="val block mt-1 text-amber-700 js-pending" /></div>
                    <span class="ic" style="background:#FFFBEB;color:#D97706"><i data-lucide="clock-3" class="w-5 h-5"></i></span>
                </div>
            </div>
        </div>

        <!-- ===== FILTERS ===== -->
        <div class="card p-4 mb-5">
            <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
                <div>
                    <label class="mb-1.5 block text-xs font-bold text-slate-700 dark:text-slate-300">Pay Period</label>
                    <asp:DropDownList ID="ddlPayrollPeriod" runat="server" CssClass="input" />
                </div>
                <div>
                    <label class="mb-1.5 block text-xs font-bold text-slate-700 dark:text-slate-300">Department</label>
                    <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="input" />
                </div>
                <div>
                    <label class="mb-1.5 block text-xs font-bold text-slate-700 dark:text-slate-300">Payment Status</label>
                    <asp:DropDownList ID="ddlPaymentStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="All Statuses" Value="" />
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Paid" Value="Paid" />
                        <asp:ListItem Text="Failed" Value="Failed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>
                <div>
                    <label class="mb-1.5 block text-xs font-bold text-slate-700 dark:text-slate-300">Search</label>
                    <asp:TextBox ID="txtSearch" runat="server" MaxLength="150" placeholder="Employee ID, department or position" CssClass="input" />
                </div>
            </div>
            <div class="mt-4 flex flex-wrap justify-end gap-2">
                <asp:LinkButton ID="btnReset" runat="server" CausesValidation="false" OnClick="btnReset_Click" CssClass="btn btn-secondary"><i data-lucide="rotate-ccw" class="w-4 h-4"></i> Reset</asp:LinkButton>
                <asp:LinkButton ID="btnSearch" runat="server" CausesValidation="false" OnClick="btnSearch_Click" CssClass="btn btn-primary"><i data-lucide="search" class="w-4 h-4"></i> Search</asp:LinkButton>
            </div>
        </div>

        <!-- ===== TABS ===== -->
        <div class="card overflow-hidden mb-5">
            <div class="tabbar px-2">
                <a class="active" href="#overview">Overview</a>
                <a href="#records">Employees</a>
                <a href="PayrollPeriods.aspx">Pay Runs</a>
                <a href="#records">Payslips</a>
                <a href="#records">Payments</a>
                <a href="PayrollReports.aspx">Reports</a>
                <a href="PayrollPeriods.aspx">Settings</a>
            </div>

            <!-- ===== OVERVIEW ===== -->
            <div id="overview" class="p-5 grid grid-cols-1 lg:grid-cols-3 gap-5">
                <!-- Payroll Summary -->
                <div class="lg:col-span-2 rounded-xl border border-slate-200 dark:border-slate-700 p-5">
                    <h3 class="font-bold mb-3 flex items-center gap-2"><i data-lucide="receipt-text" class="w-4 h-4 text-brand-600"></i> Payroll Summary</h3>
                    <div class="sum-row"><span class="k"><i data-lucide="files" class="w-4 h-4 text-slate-400"></i> Payroll Records</span><asp:Label ID="lblRecordCount" runat="server" Text="0" CssClass="v" /></div>
                    <div class="sum-row"><span class="k"><i data-lucide="circle-dollar-sign" class="w-4 h-4 text-blue-500"></i> Gross Salary</span><span class="v text-blue-700"><asp:Literal ID="litGross2" runat="server" Text="$0.00" /></span></div>
                    <div class="sum-row"><span class="k"><i data-lucide="circle-minus" class="w-4 h-4 text-rose-500"></i> Total Deductions</span><asp:Label ID="lblTotalDeductions" runat="server" Text="$0.00" CssClass="v text-rose-700" /></div>
                    <div class="sum-row"><span class="k"><i data-lucide="banknote" class="w-4 h-4 text-indigo-500"></i> Net Pay</span><span class="v text-indigo-700"><asp:Literal ID="litNet2" runat="server" Text="$0.00" /></span></div>
                    <div class="sum-row"><span class="k"><i data-lucide="circle-check-big" class="w-4 h-4 text-emerald-500"></i> Paid Amount</span><span class="v text-emerald-700"><asp:Literal ID="litPaid2" runat="server" Text="$0.00" /></span></div>
                    <div class="sum-row"><span class="k"><i data-lucide="clock-3" class="w-4 h-4 text-amber-500"></i> Pending Amount</span><span class="v text-amber-700"><asp:Literal ID="litPending2" runat="server" Text="$0.00" /></span></div>
                </div>
                <!-- Payment Status donut -->
                <div class="rounded-xl border border-slate-200 dark:border-slate-700 p-5">
                    <h3 class="font-bold mb-4 flex items-center gap-2"><i data-lucide="pie-chart" class="w-4 h-4 text-brand-600"></i> Payment Status</h3>
                    <div class="flex flex-col items-center gap-4">
                        <div class="donut" id="payDonut">
                            <div class="center"><span class="text-lg font-extrabold" id="donutPct">0%</span><span class="text-[10px] font-bold uppercase text-slate-400">Paid</span></div>
                        </div>
                        <div class="w-full text-sm">
                            <div class="flex items-center justify-between py-1"><span class="flex items-center gap-2"><span class="w-2.5 h-2.5 rounded-full bg-emerald-500"></span> Paid</span><span class="font-bold" id="legPaid">$0.00</span></div>
                            <div class="flex items-center justify-between py-1"><span class="flex items-center gap-2"><span class="w-2.5 h-2.5 rounded-full bg-amber-500"></span> Pending</span><span class="font-bold" id="legPending">$0.00</span></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- ===== PAYROLL RECORDS ===== -->
        <div id="records" class="card overflow-hidden">
            <div class="card-head">
                <div><h2>Payroll Records</h2><p class="sub">Review generated salary calculations and payment statuses.</p></div>
                <asp:Label ID="lblPagingSummary" runat="server" CssClass="text-sm font-medium text-slate-500" />
            </div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvPayroll" runat="server" AutoGenerateColumns="False" AllowPaging="True" AllowSorting="True"
                    ShowFooter="false" PageSize="20" DataKeyNames="PayrollRecordID" GridLines="None" CssClass="payroll-table"
                    OnPageIndexChanging="gvPayroll_PageIndexChanging" OnSorting="gvPayroll_Sorting">
                    <Columns>
                        <asp:BoundField DataField="PeriodName" HeaderText="Period" SortExpression="PeriodName" />
                        <asp:BoundField DataField="EmployeeID" HeaderText="Employee ID" SortExpression="EmployeeID" />
                        <asp:BoundField DataField="Department" HeaderText="Department" SortExpression="Department" />
                        <asp:BoundField DataField="Position" HeaderText="Position" SortExpression="Position" />
                        <asp:BoundField DataField="BasicSalary" HeaderText="Basic Salary" SortExpression="BasicSalary" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                        <asp:BoundField DataField="GrossSalary" HeaderText="Gross Salary" SortExpression="GrossSalary" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                        <asp:BoundField DataField="TotalDeductions" HeaderText="Deductions" SortExpression="TotalDeductions" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                        <asp:BoundField DataField="NetSalary" HeaderText="Net Salary" SortExpression="NetSalary" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                        <asp:TemplateField HeaderText="Status" SortExpression="PaymentStatus">
                            <ItemTemplate>
                                <span class='<%# GetPaymentStatusCss(Convert.ToString(Eval("PaymentStatus"))) %>'><%#: Convert.ToString(Eval("PaymentStatus")) %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:dd MMM yyyy}" HtmlEncode="false" NullDisplayText="—" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:HyperLink ID="lnkDetails" runat="server"
                                    NavigateUrl='<%# "PayrollDetails.aspx?id=" + Convert.ToInt32(Eval("PayrollRecordID")).ToString(System.Globalization.CultureInfo.InvariantCulture) %>'
                                    CssClass="inline-flex items-center gap-1.5 rounded-lg border border-blue-200 bg-blue-50 px-3 py-1.5 text-xs font-semibold text-blue-700 transition hover:bg-blue-100">
                                    Details
                                </asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="px-6 py-16 text-center">
                            <div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 text-slate-500"><i data-lucide="inbox" class="h-6 w-6"></i></div>
                            <p class="mt-4 text-sm font-semibold text-slate-700">No payroll records found.</p>
                            <p class="mt-1 text-sm text-slate-500">Change the filters or create a pay run for an eligible period.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
            <div class="flex flex-col gap-3 border-t border-slate-200 dark:border-slate-700 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
                <asp:Label ID="lblPageNumber" runat="server" CssClass="text-sm text-slate-500" />
                <div class="flex items-center gap-2">
                    <asp:LinkButton ID="btnPreviousPage" runat="server" CausesValidation="false" OnClick="btnPreviousPage_Click" CssClass="btn btn-secondary !py-2"><i data-lucide="chevron-left" class="w-4 h-4"></i> Previous</asp:LinkButton>
                    <asp:LinkButton ID="btnNextPage" runat="server" CausesValidation="false" OnClick="btnNextPage_Click" CssClass="btn btn-secondary !py-2">Next <i data-lucide="chevron-right" class="w-4 h-4"></i></asp:LinkButton>
                </div>
            </div>
        </div>

    </div>

    <script type="text/javascript">
        function initializePayrollIcons() { if (window.lucide) { window.lucide.createIcons(); } }
        function prNum(el) { if (!el) return 0; var n = parseFloat((el.textContent || '').replace(/[^0-9.]/g, '')); return isNaN(n) ? 0 : n; }
        function prDonut() {
            var paid = prNum(document.querySelector('.js-paid'));
            var pending = prNum(document.querySelector('.js-pending'));
            var total = paid + pending;
            var pct = total > 0 ? Math.round(paid / total * 100) : 0;
            var d = document.getElementById('payDonut');
            if (d) d.style.background = total > 0
                ? 'conic-gradient(#16A34A 0 ' + pct + '%, #D97706 ' + pct + '% 100%)'
                : 'conic-gradient(#E5E7EB 0 100%)';
            var pctEl = document.getElementById('donutPct'); if (pctEl) pctEl.textContent = pct + '%';
            var lp = document.getElementById('legPaid'); if (lp) lp.textContent = (document.querySelector('.js-paid') || {}).textContent || '$0.00';
            var lg = document.getElementById('legPending'); if (lg) lg.textContent = (document.querySelector('.js-pending') || {}).textContent || '$0.00';
        }
        document.addEventListener("DOMContentLoaded", function () { initializePayrollIcons(); prDonut(); });
        if (window.Sys && Sys.Application) { Sys.Application.add_load(function () { initializePayrollIcons(); prDonut(); }); }
    </script>
</asp:Content>
