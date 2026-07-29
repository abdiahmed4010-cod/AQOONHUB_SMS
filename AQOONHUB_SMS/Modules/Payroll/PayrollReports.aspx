<%@ Page Title="Payroll Reports | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="PayrollReports.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Payroll.PayrollReports" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .prep-wrap { padding:1.25rem; max-width:1400px; margin:0 auto; }
        .kpi-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .kpi-grid { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .kpi-grid { grid-template-columns:repeat(6,1fr); } }
        .kpi { padding:1rem; }
        .kpi .lbl { font-size:.64rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#6B7280; }
        .kpi .val { font-size:1.15rem; font-weight:800; line-height:1.1; margin-top:.25rem; }
        .card-head { padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .card-head h2 { font-size:.95rem; font-weight:800; }
        .rep-table { width:100%; border-collapse:collapse; }
        .rep-table th { padding:.7rem 1rem; background:#f8fafc; text-align:left; font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .rep-table td { padding:.7rem 1rem; border-bottom:1px solid #f1f5f9; font-size:.85rem; white-space:nowrap; }
        @media (max-width:768px){ .prep-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="prep-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="hover:text-brand-600">Payroll</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Reports</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-5">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Payroll Reports</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Salary totals, deductions and payment status by period and department.</p>
            </div>
            <div class="flex gap-2">
                <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn btn-primary" OnClick="btnExport_Click" />
                <button type="button" class="btn btn-secondary" onclick="window.print()"><i data-lucide="printer" class="w-4 h-4"></i> Print</button>
            </div>
        </div>

        <div class="card p-4 mb-5">
            <div class="grid grid-cols-1 gap-4 md:grid-cols-4">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Pay Period</label><asp:DropDownList ID="ddlPeriod" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Department</label><asp:DropDownList ID="ddlDept" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Payment Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="All Statuses" Value="" />
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Paid" Value="Paid" />
                        <asp:ListItem Text="Failed" Value="Failed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>
                <div class="flex items-end"><asp:Button ID="btnView" runat="server" Text="View Report" CssClass="btn btn-primary w-full justify-center" OnClick="btnView_Click" /></div>
            </div>
        </div>

        <!-- Summary cards -->
        <div class="kpi-grid mb-5">
            <div class="card kpi"><p class="lbl">Employees</p><p class="val"><asp:Literal ID="litEmployees" runat="server" Text="0" /></p></div>
            <div class="card kpi"><p class="lbl">Total Basic</p><p class="val"><asp:Literal ID="litBasic" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Other Allowance</p><p class="val"><asp:Literal ID="litOtherAllow" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Bonus</p><p class="val"><asp:Literal ID="litBonus" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Gross</p><p class="val text-blue-700"><asp:Literal ID="litGross" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Net</p><p class="val text-indigo-700"><asp:Literal ID="litNet" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Tax</p><p class="val"><asp:Literal ID="litTax" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Other Deduction</p><p class="val"><asp:Literal ID="litOtherDed" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Total Deductions</p><p class="val text-rose-700"><asp:Literal ID="litDeductions" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Paid</p><p class="val text-emerald-700"><asp:Literal ID="litPaid" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Pending</p><p class="val text-amber-700"><asp:Literal ID="litPending" runat="server" /></p></div>
            <div class="card kpi"><p class="lbl">Failed</p><p class="val text-rose-700"><asp:Literal ID="litFailed" runat="server" Text="0" /></p></div>
        </div>

        <!-- Department breakdown -->
        <div class="card overflow-hidden">
            <div class="card-head"><h2>Payroll by Department</h2></div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvDept" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="rep-table">
                    <Columns>
                        <asp:BoundField DataField="Department" HeaderText="Department" />
                        <asp:BoundField DataField="Records" HeaderText="Employees" />
                        <asp:BoundField DataField="Gross" HeaderText="Gross" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                        <asp:BoundField DataField="Deductions" HeaderText="Deductions" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                        <asp:BoundField DataField="Net" HeaderText="Net" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                        <asp:BoundField DataField="Paid" HeaderText="Paid" DataFormatString="{0:$#,##0.00}" HtmlEncode="false" />
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No payroll data for the selected filters.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
