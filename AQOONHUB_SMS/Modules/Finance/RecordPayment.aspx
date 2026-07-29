<%@ Page Title="Record Payment | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="RecordPayment.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.RecordPayment" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .rp-wrap { padding:1.25rem; max-width:860px; margin:0 auto; }
        .fld > span.lbl { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .fld > span.lbl { color:#CBD5E1; }
        .fg2 { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:640px){ .fg2 { grid-template-columns:1fr 1fr; } }
        .ro-grid { display:grid; grid-template-columns:1fr 1fr; gap:.5rem 1.5rem; }
        @media (min-width:640px){ .ro-grid { grid-template-columns:repeat(3,1fr); } }
        .ro .k { font-size:.66rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#6B7280; }
        .dark .ro .k { color:#94A3B8; }
        .ro .v { font-size:.9rem; font-weight:700; }
        .prev { display:grid; grid-template-columns:repeat(3,1fr); gap:.5rem; background:#ECFDF5; border:1px solid #BBF7D0; border-radius:.7rem; padding:.9rem 1rem; }
        .dark .prev { background:#052e1a; border-color:#14532d; }
        .prev .k { font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#15803D; }
        .prev .v { font-size:1.05rem; font-weight:800; color:#065F46; }
        .dark .prev .k { color:#4ade80; } .dark .prev .v { color:#bbf7d0; }
        @media (max-width:640px){ .rp-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rp-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span>Finance</span>
            <span>/</span><a href="~/Modules/Finance/BalanceTracking.aspx" runat="server" class="hover:text-brand-600">Balance Tracking</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Record Payment</span>
        </nav>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="p-3 mb-4 rounded-lg bg-amber-50 text-amber-800 border border-amber-200 text-sm dark:bg-amber-500/10 dark:text-amber-300 dark:border-amber-500/30">
            <i data-lucide="alert-triangle" class="w-4 h-4 inline-block mr-1"></i><asp:Literal ID="msgText" runat="server" />
        </asp:Panel>

        <!-- SUCCESS panel (after save) -->
        <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="card overflow-hidden">
            <div class="p-6 text-center">
                <span class="inline-flex items-center justify-center w-14 h-14 rounded-full mb-3" style="background:#DCFCE7;color:#15803D"><i data-lucide="check" class="w-7 h-7"></i></span>
                <h1 class="text-lg font-extrabold">Payment recorded successfully</h1>
                <div class="ro-grid mt-5 text-left max-w-lg mx-auto">
                    <div class="ro"><p class="k">Receipt Number</p><p class="v"><asp:Literal ID="litRcpNumber" runat="server" /></p></div>
                    <div class="ro"><p class="k">Amount Paid</p><p class="v">$<asp:Literal ID="litRcpAmount" runat="server" /></p></div>
                    <div class="ro"><p class="k">New Balance</p><p class="v">$<asp:Literal ID="litRcpBalance" runat="server" /></p></div>
                    <div class="ro"><p class="k">Invoice Status</p><p class="v"><asp:Literal ID="litRcpStatus" runat="server" /></p></div>
                </div>
                <div class="flex justify-center gap-2 mt-6">
                    <a href="~/Modules/Finance/BalanceTracking.aspx" runat="server" class="btn btn-primary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Balance Tracking</a>
                    <asp:HyperLink ID="lnkReceipt" runat="server" CssClass="btn btn-secondary"><i data-lucide="printer" class="w-4 h-4"></i> Print Receipt</asp:HyperLink>
                </div>
            </div>
        </asp:Panel>

        <!-- FORM panel -->
        <asp:Panel ID="pnlForm" runat="server" CssClass="card overflow-hidden">
            <div class="flex items-center gap-3 p-5 border-b border-gray-100 dark:border-slate-700">
                <span class="w-10 h-10 rounded-xl flex items-center justify-center" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="credit-card" class="w-5 h-5"></i></span>
                <div>
                    <h1 class="text-lg font-extrabold">Record Payment</h1>
                    <p class="text-xs text-gray-500 dark:text-slate-400">Record a payment made by a student against an open invoice.</p>
                </div>
            </div>

            <div class="p-6">
                <asp:HiddenField ID="hidInvoiceId" runat="server" />
                <asp:HiddenField ID="hidBalance" runat="server" />

                <!-- Free selection (only when not opened via Collect) -->
                <asp:Panel ID="pnlSelect" runat="server" CssClass="fg2 mb-5">
                    <div class="fld"><span class="lbl">Student *</span>
                        <asp:DropDownList ID="student" runat="server" AutoPostBack="true" OnSelectedIndexChanged="student_Changed" CssClass="input" />
                    </div>
                    <div class="fld"><span class="lbl">Invoice *</span>
                        <asp:DropDownList ID="invoice" runat="server" AutoPostBack="true" OnSelectedIndexChanged="invoice_Changed" CssClass="input" />
                    </div>
                </asp:Panel>

                <!-- Locked / read-only invoice + student info -->
                <asp:Panel ID="pnlInfo" runat="server" Visible="false" CssClass="rounded-xl border border-gray-200 dark:border-slate-700 p-4 mb-5 bg-gray-50/60 dark:bg-slate-800/40">
                    <div class="ro-grid">
                        <div class="ro"><p class="k">Student</p><p class="v"><asp:Literal ID="litStudent" runat="server" /></p></div>
                        <div class="ro"><p class="k">Student Code</p><p class="v"><asp:Literal ID="litStudentCode" runat="server" /></p></div>
                        <div class="ro"><p class="k">Class</p><p class="v"><asp:Literal ID="litClass" runat="server" /></p></div>
                        <div class="ro"><p class="k">Invoice No.</p><p class="v"><asp:Literal ID="litInvoiceNo" runat="server" /></p></div>
                        <div class="ro"><p class="k">Invoice Amount</p><p class="v">$<asp:Literal ID="litInvAmount" runat="server" /></p></div>
                        <div class="ro"><p class="k">Current Paid</p><p class="v">$<asp:Literal ID="litPaid" runat="server" /></p></div>
                        <div class="ro"><p class="k">Previous Balance</p><p class="v">$<asp:Literal ID="litPrevBalance" runat="server" /></p></div>
                        <div class="ro"><p class="k">Due Date</p><p class="v"><asp:Literal ID="litDueDate" runat="server" /></p></div>
                        <div class="ro"><p class="k">Status</p><p class="v"><asp:Literal ID="litStatus" runat="server" /></p></div>
                    </div>
                </asp:Panel>

                <!-- Editable payment fields -->
                <asp:Panel ID="pnlPay" runat="server" Visible="false">
                    <div class="fg2">
                        <div class="fld"><span class="lbl">Amount Paid *</span>
                            <asp:TextBox ID="amount" runat="server" CssClass="input" TextMode="Number" placeholder="Enter payment amount" onkeyup="rpPreview()" onchange="rpPreview()" />
                        </div>
                        <div class="fld"><span class="lbl">Payment Method *</span>
                            <asp:DropDownList ID="method" runat="server" CssClass="input">
                                <asp:ListItem Text="Select Payment Method" Value="" />
                                <asp:ListItem Text="Cash" Value="Cash" />
                                <asp:ListItem Text="Bank Transfer" Value="Bank Transfer" />
                                <asp:ListItem Text="Mobile Money" Value="Mobile Money" />
                                <asp:ListItem Text="Cheque" Value="Cheque" />
                                <asp:ListItem Text="Other" Value="Other" />
                            </asp:DropDownList>
                        </div>
                        <div class="fld"><span class="lbl">Payment Date *</span>
                            <asp:TextBox ID="date" runat="server" TextMode="Date" CssClass="input" />
                        </div>
                        <div class="fld"><span class="lbl">Reference / Transaction ID</span>
                            <asp:TextBox ID="reference" runat="server" CssClass="input" placeholder="Required for Bank Transfer, Mobile Money, Cheque" />
                        </div>
                        <div class="fld sm:col-span-2"><span class="lbl">Notes</span>
                            <asp:TextBox ID="notes" runat="server" CssClass="input" placeholder="Optional" />
                        </div>
                    </div>

                    <div class="prev mt-5">
                        <div><p class="k">Amount Paid</p><p class="v">$<span id="pvPaid">0.00</span></p></div>
                        <div><p class="k">Previous Balance</p><p class="v">$<span id="pvPrev">0.00</span></p></div>
                        <div><p class="k">New Balance</p><p class="v">$<span id="pvNew">0.00</span></p></div>
                    </div>

                    <div class="flex justify-end gap-2 mt-6 pt-4 border-t border-gray-100 dark:border-slate-700">
                        <a href="~/Modules/Finance/BalanceTracking.aspx" runat="server" class="btn btn-secondary">Cancel</a>
                        <asp:Button ID="save" runat="server" Text="Save Payment" CssClass="btn btn-primary" OnClick="save_Click" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </div>
</asp:Content>

<asp:Content ID="s" ContentPlaceHolderID="scripts" runat="server">
    <script>
        function rpNum(v){ var n=parseFloat(String(v||'').replace(/[^0-9.\-]/g,'')); return isNaN(n)?0:n; }
        function rpPreview(){
            var prevEl = document.getElementById('<%= hidBalance.ClientID %>');
            var amtEl = document.getElementById('<%= amount.ClientID %>');
            var prev = prevEl ? rpNum(prevEl.value) : 0;
            var amt = amtEl ? rpNum(amtEl.value) : 0;
            if (amt < 0) amt = 0;
            var nb = prev - amt; if (nb < 0) nb = 0;
            var f = function(n){ return n.toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2}); };
            var set=function(id,val){ var el=document.getElementById(id); if(el) el.textContent=val; };
            set('pvPaid', f(amt)); set('pvPrev', f(prev)); set('pvNew', f(nb));
        }
        document.addEventListener('DOMContentLoaded', rpPreview);
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(rpPreview);
        }
    </script>
</asp:Content>
