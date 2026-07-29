<%@ Page Title="Invoice Details | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="InvoiceDetails.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.InvoiceDetails" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .details-wrap { padding: 1.25rem; max-width: 1000px; margin: 0 auto; }
        .detail-row { display:flex; justify-content:space-between; gap:1rem; padding:.5rem 0; border-bottom:1px solid #F1F5F9; font-size:.82rem; }
        .dark .detail-row { border-color:#263449; }
        .detail-row .k { color:#6B7280; font-weight:600; }
        .dark .detail-row .k { color:#94A3B8; }
        .detail-row .v { font-weight:700; text-align:right; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        @media (max-width:768px){ .details-wrap{padding:.875rem;} }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="details-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Finance/Invoices.aspx" runat="server" class="hover:text-brand-600">Invoices</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Details</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Invoice Details</h1>

        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
            <i data-lucide="check-circle-2" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblSuccess" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
            <div class="card p-8 text-center">
                <p class="font-bold">Invoice not found.</p>
                <a href="~/Modules/Finance/Invoices.aspx" runat="server" class="btn btn-secondary mt-3">Back to Invoices</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server">
            <div class="card p-6 mb-5">
                <div class="flex items-center justify-between flex-wrap gap-3 mb-4">
                    <div>
                        <h2 class="text-lg font-extrabold"><asp:Label ID="lblInvoiceNo" runat="server" /></h2>
                        <p class="text-xs text-gray-500 dark:text-slate-400 mt-0.5"><asp:Label ID="lblStudentInfo" runat="server" /></p>
                    </div>
                    <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" />
                </div>
                <div class="grid md:grid-cols-2 gap-x-8">
                    <div>
                        <div class="detail-row"><span class="k">Academic Year</span><span class="v"><asp:Label ID="lblAcademicYear" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Term</span><span class="v"><asp:Label ID="lblTerm" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Due Date</span><span class="v"><asp:Label ID="lblDueDate" runat="server" /></span></div>
                    </div>
                    <div>
                        <div class="detail-row"><span class="k">Total Amount</span><span class="v"><asp:Label ID="lblTotalAmount" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Paid Amount</span><span class="v"><asp:Label ID="lblPaidAmount" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Balance Due</span><span class="v"><asp:Label ID="lblBalance" runat="server" /></span></div>
                    </div>
                </div>
                <div class="flex gap-2 mt-4 pt-4 border-t border-gray-100 dark:border-slate-700">
                    <asp:HyperLink ID="lnkRecordPayment" runat="server" CssClass="btn btn-primary"><i data-lucide="credit-card" class="w-4 h-4"></i> Record Payment</asp:HyperLink>
                </div>
            </div>

            <div class="card overflow-hidden mb-5">
                <div class="p-4 border-b border-gray-100 dark:border-slate-700"><h3 class="font-bold text-sm">Invoice Items</h3></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvItems" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full">
                        <Columns>
                            <asp:BoundField DataField="Description" HeaderText="Description" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Amount">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate>$<%# Convert.ToDecimal(Eval("Amount")).ToString("N2") %></ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="text-center py-6 text-sm text-gray-500 dark:text-slate-400">No items on this invoice.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <div class="card overflow-hidden mb-5">
                <div class="p-4 border-b border-gray-100 dark:border-slate-700"><h3 class="font-bold text-sm">Payment History</h3></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvPayments" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full">
                        <Columns>
                            <asp:BoundField DataField="ReceiptNo" HeaderText="Receipt No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Amount">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate>$<%# Convert.ToDecimal(Eval("Amount")).ToString("N2") %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="PaymentMethod" HeaderText="Method" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="PaymentDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="Notes" HeaderText="Notes" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        </Columns>
                        <EmptyDataTemplate><div class="text-center py-6 text-sm text-gray-500 dark:text-slate-400">No payments recorded yet.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <a href="~/Modules/Finance/Invoices.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Invoices</a>
        </asp:Panel>
    </div>
</asp:Content>
