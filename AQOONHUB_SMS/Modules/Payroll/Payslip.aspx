<%@ Page Title="Payslip | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Payslip.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Payroll.Payslip" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ps-wrap { padding:1.25rem; max-width:820px; margin:0 auto; }
        .payslip { border:1px solid #E5E7EB; border-radius:.9rem; overflow:hidden; background:#fff; }
        .payslip .hd { display:flex; align-items:center; justify-content:space-between; gap:1rem; padding:1.5rem; border-bottom:2px solid #1E3A8A; }
        .payslip .school { font-size:1.15rem; font-weight:800; color:#1E3A8A; }
        .payslip .sub { font-size:.75rem; color:#64748B; }
        .payslip .body { padding:1.5rem; }
        .grid2 { display:grid; grid-template-columns:1fr 1fr; gap:.4rem 1.5rem; }
        .kv { display:flex; justify-content:space-between; gap:1rem; font-size:.85rem; padding:.35rem 0; border-bottom:1px solid #F1F5F9; }
        .kv .k { color:#64748B; } .kv .v { font-weight:700; }
        .kv.total { border-top:2px solid #E5E7EB; font-weight:800; }
        .sec-title { font-size:.72rem; font-weight:800; text-transform:uppercase; letter-spacing:.05em; color:#475569; margin:1rem 0 .25rem; }
        .net-box { background:#ECFDF5; border:1px solid #BBF7D0; border-radius:.7rem; padding:1rem 1.25rem; display:flex; justify-content:space-between; align-items:center; margin-top:1rem; }
        .net-box .k { font-weight:800; color:#065F46; } .net-box .v { font-size:1.4rem; font-weight:800; color:#065F46; }
        @media print {
            #sidebar, header, footer, .no-print, nav.crumb { display:none !important; }
            .lg\:pl-64 { padding-left:0 !important; }
            body { background:#fff !important; }
            .payslip { border:none; }
        }
        @media (max-width:640px){ .ps-wrap { padding:.875rem; } .grid2 { grid-template-columns:1fr; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ps-wrap">

        <nav class="crumb flex items-center justify-between gap-2 mb-4 no-print">
            <a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back</a>
            <button type="button" class="btn btn-primary" onclick="window.print()"><i data-lucide="printer" class="w-4 h-4"></i> Print / Save PDF</button>
        </nav>

        <asp:Panel ID="pnlNotFound" runat="server" Visible="false" CssClass="card p-8 text-center">
            <p class="font-bold">Payslip not found.</p>
            <a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="btn btn-secondary mt-3">Back to Payroll</a>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server" CssClass="payslip">
            <div class="hd">
                <div>
                    <p class="school">AQOONHUB International School</p>
                    <p class="sub">Payslip &middot; <asp:Literal ID="litPeriod" runat="server" /></p>
                </div>
                <div class="text-right">
                    <p class="sub">Status</p>
                    <asp:Label ID="lblStatus" runat="server" CssClass="font-bold" />
                </div>
            </div>
            <div class="body">
                <div class="grid2">
                    <div class="kv"><span class="k">Employee ID</span><span class="v"><asp:Literal ID="litEmpId" runat="server" /></span></div>
                    <div class="kv"><span class="k">Department</span><span class="v"><asp:Literal ID="litDept" runat="server" /></span></div>
                    <div class="kv"><span class="k">Position</span><span class="v"><asp:Literal ID="litPosition" runat="server" /></span></div>
                    <div class="kv"><span class="k">Pay Period</span><span class="v"><asp:Literal ID="litPeriod2" runat="server" /></span></div>
                    <div class="kv"><span class="k">Pay Date</span><span class="v"><asp:Literal ID="litPayDate" runat="server" /></span></div>
                    <div class="kv"><span class="k">Payment Method</span><span class="v"><asp:Literal ID="litMethod" runat="server" /></span></div>
                    <div class="kv"><span class="k">Payment Reference</span><span class="v"><asp:Literal ID="litReference" runat="server" /></span></div>
                    <div class="kv"><span class="k">Paid Date</span><span class="v"><asp:Literal ID="litPaidDate" runat="server" /></span></div>
                </div>

                <div class="grid2 mt-2">
                    <div>
                        <p class="sec-title">Earnings</p>
                        <div class="kv"><span class="k">Basic Salary</span><span class="v"><asp:Literal ID="litBasic" runat="server" /></span></div>
                        <div class="kv"><span class="k">Other Allowance</span><span class="v"><asp:Literal ID="litOther" runat="server" /></span></div>
                        <div class="kv"><span class="k">Bonus</span><span class="v"><asp:Literal ID="litBonus" runat="server" /></span></div>
                        <div class="kv total"><span class="k">Gross Salary</span><span class="v"><asp:Literal ID="litGross" runat="server" /></span></div>
                    </div>
                    <div>
                        <p class="sec-title">Deductions</p>
                        <div class="kv"><span class="k">Tax Deduction</span><span class="v"><asp:Literal ID="litTax" runat="server" /></span></div>
                        <div class="kv"><span class="k">Other Deduction</span><span class="v"><asp:Literal ID="litOtherDed" runat="server" /></span></div>
                        <div class="kv total"><span class="k">Total Deductions</span><span class="v"><asp:Literal ID="litDeductions" runat="server" /></span></div>
                    </div>
                </div>

                <div class="net-box">
                    <span class="k">Net Salary</span>
                    <span class="v"><asp:Literal ID="litNet" runat="server" /></span>
                </div>

                <p class="text-[11px] text-slate-400 mt-4">This is a system-generated payslip and does not require a signature.</p>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
