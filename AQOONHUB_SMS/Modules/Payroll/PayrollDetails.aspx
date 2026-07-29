<%@ Page Title="Payroll Record | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="PayrollDetails.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Payroll.PayrollDetails" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .pd-wrap { padding:1.25rem; max-width:1100px; margin:0 auto; }
        .card-head { padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .card-head h2 { font-size:.95rem; font-weight:800; }
        .kv { display:flex; justify-content:space-between; gap:1rem; padding:.5rem 0; border-bottom:1px solid #F1F5F9; font-size:.85rem; }
        .kv:last-child { border-bottom:none; }
        .kv .k { color:#64748B; } .kv .v { font-weight:700; }
        .kv.total { border-top:2px solid #E5E7EB; margin-top:.25rem; padding-top:.75rem; }
        .fld label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .badge2 { display:inline-flex; align-items:center; padding:.25rem .7rem; border-radius:999px; font-size:.72rem; font-weight:700; }
        @media (max-width:768px){ .pd-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="pd-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="hover:text-brand-600">Payroll</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Payroll Record</span>
        </nav>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="p-3 mb-4 rounded-lg text-sm">
            <asp:Literal ID="msgText" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlNotFound" runat="server" Visible="false" CssClass="card p-8 text-center">
            <p class="font-bold">Payroll record not found.</p>
            <a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="btn btn-secondary mt-3">Back to Payroll</a>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server">
            <div class="flex flex-wrap items-center justify-between gap-3 mb-5">
                <div>
                    <h1 class="text-xl md:text-2xl font-bold tracking-tight">Payroll Record</h1>
                    <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">
                        <asp:Literal ID="litEmployee" runat="server" /> · <asp:Literal ID="litPeriod" runat="server" />
                    </p>
                </div>
                <div class="flex items-center gap-2">
                    <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge2" />
                    <asp:HyperLink ID="lnkPayslip" runat="server" CssClass="btn btn-secondary"><i data-lucide="file-text" class="w-4 h-4"></i> Payslip</asp:HyperLink>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-5">
                <!-- Employee + earnings/deductions -->
                <div class="card overflow-hidden">
                    <div class="card-head"><h2>Employee &amp; Salary</h2></div>
                    <div class="p-5">
                        <div class="kv"><span class="k">Employee ID</span><span class="v"><asp:Literal ID="litEmpId" runat="server" /></span></div>
                        <div class="kv"><span class="k">Department</span><span class="v"><asp:Literal ID="litDept" runat="server" /></span></div>
                        <div class="kv"><span class="k">Position</span><span class="v"><asp:Literal ID="litPosition" runat="server" /></span></div>
                        <div class="kv"><span class="k">Pay Period</span><span class="v"><asp:Literal ID="litPeriod2" runat="server" /></span></div>
                        <div class="kv"><span class="k">Pay Date</span><span class="v"><asp:Literal ID="litPayDate" runat="server" /></span></div>

                        <h3 class="text-xs font-bold uppercase tracking-wide text-slate-500 mt-4 mb-1">Earnings</h3>
                        <div class="kv"><span class="k">Basic Salary</span><span class="v"><asp:Literal ID="litBasic" runat="server" /></span></div>
                        <div class="kv"><span class="k">Other Allowance</span><span class="v"><asp:Literal ID="litOther" runat="server" /></span></div>
                        <div class="kv"><span class="k">Bonus</span><span class="v"><asp:Literal ID="litBonus" runat="server" /></span></div>
                        <div class="kv total"><span class="k font-bold">Gross Salary</span><span class="v text-blue-700"><asp:Literal ID="litGross" runat="server" /></span></div>

                        <h3 class="text-xs font-bold uppercase tracking-wide text-slate-500 mt-4 mb-1">Deductions</h3>
                        <div class="kv"><span class="k">Tax Deduction</span><span class="v"><asp:Literal ID="litTax" runat="server" /></span></div>
                        <div class="kv"><span class="k">Other Deduction</span><span class="v"><asp:Literal ID="litOtherDed" runat="server" /></span></div>
                        <div class="kv total"><span class="k font-bold">Total Deductions</span><span class="v text-rose-700"><asp:Literal ID="litDeductions" runat="server" /></span></div>

                        <div class="kv total"><span class="k font-extrabold text-base">Net Salary</span><span class="v text-emerald-700 text-base"><asp:Literal ID="litNet" runat="server" /></span></div>
                    </div>
                </div>

                <!-- Payment + actions -->
                <div class="flex flex-col gap-5">
                    <div class="card overflow-hidden">
                        <div class="card-head"><h2>Payment</h2></div>
                        <div class="p-5">
                            <div class="kv"><span class="k">Status</span><span class="v"><asp:Literal ID="litStatus" runat="server" /></span></div>
                            <div class="kv"><span class="k">Payment Method</span><span class="v"><asp:Literal ID="litMethod" runat="server" /></span></div>
                            <div class="kv"><span class="k">Payment Reference</span><span class="v"><asp:Literal ID="litReference" runat="server" /></span></div>
                            <div class="kv"><span class="k">Paid Date</span><span class="v"><asp:Literal ID="litPaidDate" runat="server" /></span></div>
                        </div>
                    </div>

                    <!-- Mark as Paid -->
                    <asp:Panel ID="pnlPay" runat="server" Visible="false" CssClass="card overflow-hidden">
                        <div class="card-head"><h2>Record Payment</h2></div>
                        <div class="p-5 space-y-4">
                            <div class="fld"><label>Payment Method *</label>
                                <asp:DropDownList ID="ddlPayMethod" runat="server" CssClass="input">
                                    <asp:ListItem Text="Bank Transfer" Value="Bank Transfer" />
                                    <asp:ListItem Text="Mobile Money" Value="Mobile Money" />
                                    <asp:ListItem Text="Cash" Value="Cash" />
                                    <asp:ListItem Text="Cheque" Value="Cheque" />
                                    <asp:ListItem Text="Other" Value="Other" />
                                </asp:DropDownList>
                            </div>
                            <div class="fld"><label>Payment Reference / Transaction ID</label>
                                <asp:TextBox ID="txtReference" runat="server" CssClass="input" MaxLength="100" placeholder="Bank/mobile transaction reference" />
                            </div>
                            <div class="fld"><label>Paid Date *</label>
                                <asp:TextBox ID="txtPaidDate" runat="server" CssClass="input" TextMode="Date" />
                            </div>
                            <div class="flex flex-wrap gap-2 pt-2">
                                <asp:Button ID="btnMarkPaid" runat="server" Text="Mark as Paid" CssClass="btn btn-primary" OnClick="btnMarkPaid_Click" />
                                <asp:Button ID="btnMarkFailed" runat="server" Text="Mark as Failed" CssClass="btn btn-secondary !text-red-600" OnClick="btnMarkFailed_Click"
                                    OnClientClick="return confirm('Mark this payment as Failed?');" />
                            </div>
                            <div class="fld"><label>Failure Note (optional)</label>
                                <asp:TextBox ID="txtFailNote" runat="server" CssClass="input" MaxLength="500" placeholder="Reason if the payment failed" />
                            </div>
                        </div>
                    </asp:Panel>
                </div>
            </div>

            <div class="mt-5">
                <a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Payroll</a>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
