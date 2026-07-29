<%@ Page Title="Create Invoice | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="CreateInvoice.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.CreateInvoice" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ci-wrap { padding:1.25rem; max-width:1400px; margin:0 auto; }
        .steps { display:flex; align-items:center; gap:.5rem; overflow-x:auto; }
        .steps .st { display:flex; align-items:center; gap:.65rem; padding:.5rem .25rem; flex-shrink:0; }
        .steps .st .n { width:2rem; height:2rem; border-radius:999px; display:flex; align-items:center; justify-content:center; font-weight:800; font-size:.8rem; }
        .steps .st.active .n { background:#2563EB; color:#fff; }
        .steps .st .n { background:#E2E8F0; color:#64748B; }
        .dark .steps .st .n { background:#334155; color:#94A3B8; }
        .steps .st .t { font-size:.82rem; font-weight:700; }
        .steps .st .d { font-size:.68rem; color:#9CA3AF; }
        .steps .sep { flex:1; min-width:1.5rem; color:#CBD5E1; display:flex; align-items:center; }
        .ci-cols { display:grid; grid-template-columns:1fr; gap:1.25rem; }
        @media (min-width:1024px){ .ci-cols { grid-template-columns:minmax(0,2.3fr) minmax(0,1fr); align-items:start; } }
        .sec-title { font-size:.9rem; font-weight:800; margin-bottom:.9rem; }
        .fld label, .fld > span.lbl { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .fld label, .dark .fld > span.lbl { color:#CBD5E1; }
        .fg3 { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .fg3 { grid-template-columns:repeat(3,1fr); } }
        .sum-row { display:flex; justify-content:space-between; gap:1rem; font-size:.82rem; padding:.4rem 0; }
        .sum-row .k { color:#6B7280; } .dark .sum-row .k { color:#94A3B8; }
        .sum-row .v { font-weight:700; }
        @media (max-width:768px){ .ci-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ci-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span>Finance</span>
            <span>/</span><a href="~/Modules/Finance/FeeManagement.aspx" runat="server" class="hover:text-brand-600">Fee Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Create Invoice</span>
        </nav>

        <!-- Step progress -->
        <div class="card p-4 mb-5">
            <div class="steps">
                <div class="st active"><span class="n">1</span><div><p class="t">Fee Collection</p><p class="d">Create invoices and collect fees</p></div></div>
                <span class="sep"><i data-lucide="arrow-right" class="w-4 h-4"></i></span>
                <div class="st"><span class="n">2</span><div><p class="t">Payment Recording</p><p class="d">Record payments from students</p></div></div>
                <span class="sep"><i data-lucide="arrow-right" class="w-4 h-4"></i></span>
                <div class="st"><span class="n">3</span><div><p class="t">Balance Tracking</p><p class="d">Track outstanding balances</p></div></div>
                <span class="sep"><i data-lucide="arrow-right" class="w-4 h-4"></i></span>
                <div class="st"><span class="n">4</span><div><p class="t">Fee Reports</p><p class="d">View reports and analytics</p></div></div>
            </div>
        </div>

        <div class="flex flex-wrap items-center justify-between gap-3 mb-5">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Create New Invoice</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Step 1 of 4 · Select a student to load their applicable fee structure.</p>
            </div>
            <a href="~/Modules/Finance/FeeManagement.aspx" runat="server" class="btn btn-secondary">Cancel</a>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="p-3 mb-4 rounded-lg bg-amber-50 text-amber-800 border border-amber-200 text-sm dark:bg-amber-500/10 dark:text-amber-300 dark:border-amber-500/30">
            <i data-lucide="alert-triangle" class="w-4 h-4 inline-block mr-1"></i><asp:Literal ID="msgText" runat="server" />
        </asp:Panel>

        <div class="ci-cols">
            <!-- Left form -->
            <div class="card p-6">
                <p class="sec-title">Student Information</p>
                <div class="fld mb-4">
                    <span class="lbl">Search Student by ID / Code</span>
                    <div class="flex gap-2">
                        <asp:TextBox ID="txtStudentSearch" runat="server" CssClass="input" placeholder="Enter the student number (e.g. 1 or 0001) or full code…" />
                        <asp:Button ID="btnFindStudent" runat="server" Text="Search" CssClass="btn btn-primary whitespace-nowrap" CausesValidation="false" OnClick="btnFindStudent_Click" />
                    </div>
                    <small class="text-[11px] text-gray-400 mt-1 block">Search a student first to load their applicable fee structure.</small>
                </div>
                <div class="fg3">
                    <div class="fld"><span class="lbl">Student *</span>
                        <asp:DropDownList ID="student" runat="server" AutoPostBack="true" OnSelectedIndexChanged="student_Changed" CssClass="input" />
                    </div>
                    <div class="fld"><span class="lbl">Invoice Date *</span>
                        <asp:TextBox ID="invoiceDate" runat="server" TextMode="Date" CssClass="input" />
                    </div>
                    <div class="fld"><span class="lbl">Due Date *</span>
                        <asp:TextBox ID="dueDate" runat="server" TextMode="Date" CssClass="input" />
                    </div>
                </div>

                <p class="sec-title mt-6">Invoice Details</p>
                <div class="fg3">
                    <div class="fld"><span class="lbl">Invoice Type *</span>
                        <asp:DropDownList ID="invoiceType" runat="server" CssClass="input">
                            <asp:ListItem>Regular</asp:ListItem>
                            <asp:ListItem>Admission</asp:ListItem>
                            <asp:ListItem>Supplementary</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="fld"><span class="lbl">Invoice Discount ($)</span>
                        <asp:TextBox ID="discount" runat="server" Text="0" CssClass="input" AutoPostBack="true" OnTextChanged="discount_Changed" />
                    </div>
                </div>

                <p class="sec-title mt-6">Fee Items</p>
                <p class="text-xs text-gray-500 dark:text-slate-400 mb-2">Fee items are loaded automatically from the student's active class fee structure.</p>
                <div class="overflow-x-auto">
                    <asp:GridView ID="items" runat="server" AutoGenerateColumns="false" CssClass="w-full" GridLines="None">
                        <Columns>
                            <asp:BoundField DataField="CategoryName" HeaderText="Fee Category" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="Description" HeaderText="Description" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount ($)" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="DiscountAmount" HeaderText="Discount ($)" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="TotalAmount" HeaderText="Total ($)" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="py-10 text-center text-sm text-gray-500 dark:text-slate-400">Select a student to load applicable fee items.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <div class="grid md:grid-cols-2 gap-4 mt-6">
                    <div class="fld"><span class="lbl">Remarks</span>
                        <asp:TextBox ID="remarks" runat="server" TextMode="MultiLine" Rows="3" CssClass="input" placeholder="Enter any remarks (optional)" />
                    </div>
                    <div class="fld"><span class="lbl">Payment Instructions</span>
                        <asp:TextBox ID="instructions" runat="server" TextMode="MultiLine" Rows="3" CssClass="input" placeholder="Enter payment instructions (optional)" />
                    </div>
                </div>

                <div class="flex justify-end mt-6 pt-4 border-t border-gray-100 dark:border-slate-700">
                    <asp:Button ID="create" runat="server" Text="Create Invoice" CssClass="btn btn-primary" OnClick="create_Click" />
                </div>
            </div>

            <!-- Right summary -->
            <div class="flex flex-col gap-5">
                <div class="card overflow-hidden">
                    <div class="p-4 border-b border-gray-100 dark:border-slate-700"><h2 class="font-bold text-sm">Invoice Summary</h2></div>
                    <div class="p-4">
                        <div class="sum-row"><span class="k">Total Items</span><span class="v"><asp:Literal ID="litItemCount" runat="server" Text="0" /></span></div>
                        <div class="sum-row"><span class="k">Subtotal</span><span class="v">$<asp:Literal ID="litSubtotal" runat="server" Text="0.00" /></span></div>
                        <div class="sum-row"><span class="k">Discount</span><span class="v">$<asp:Literal ID="litDiscount" runat="server" Text="0.00" /></span></div>
                        <div class="sum-row border-t border-gray-100 dark:border-slate-700 mt-1 pt-2">
                            <span class="k font-bold text-gray-800 dark:text-slate-100">Total Amount</span>
                            <span class="v text-brand-600 text-base">$<asp:Literal ID="litTotal" runat="server" Text="0.00" /></span>
                        </div>
                    </div>
                </div>
                <div class="card p-4 bg-blue-50/50 dark:bg-slate-800/50">
                    <p class="text-sm font-bold mb-2 flex items-center gap-1.5"><i data-lucide="info" class="w-4 h-4 text-brand-600"></i> What happens next?</p>
                    <ul class="text-xs text-gray-600 dark:text-slate-400 space-y-1.5">
                        <li class="flex gap-2"><i data-lucide="check" class="w-3.5 h-3.5 text-green-600 mt-0.5"></i> Invoice is created for the selected student</li>
                        <li class="flex gap-2"><i data-lucide="check" class="w-3.5 h-3.5 text-green-600 mt-0.5"></i> You can record payments against this invoice</li>
                        <li class="flex gap-2"><i data-lucide="check" class="w-3.5 h-3.5 text-green-600 mt-0.5"></i> Balance is tracked automatically</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
