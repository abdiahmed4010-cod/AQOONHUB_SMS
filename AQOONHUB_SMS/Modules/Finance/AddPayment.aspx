<%@ Page Title="Record Payment | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AddPayment.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.AddPayment" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 800px; margin: 0 auto; }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .detail-row { display:flex; justify-content:space-between; gap:1rem; padding:.5rem 0; border-bottom:1px solid #F1F5F9; font-size:.82rem; }
        .dark .detail-row { border-color:#263449; }
        .detail-row .k { color:#6B7280; font-weight:600; }
        .dark .detail-row .k { color:#94A3B8; }
        .detail-row .v { font-weight:700; text-align:right; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .form-actions { display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; padding-top:1rem; border-top:1px solid #E5E7EB; margin-top:.5rem; }
        .dark .form-actions { border-color:#334155; }
        @media (max-width:768px){ .form-wrap{padding:.875rem;} .form-actions{justify-content:stretch;} .form-actions .btn{flex:1;justify-content:center;} }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Finance/Invoices.aspx" runat="server" class="hover:text-brand-600">Invoices</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Record Payment</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Record Payment</h1>

        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>
        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" ValidationGroup="Save" />

        <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
            <div class="card p-8 text-center">
                <p class="font-bold">Invoice not found.</p>
                <a href="~/Modules/Finance/Invoices.aspx" runat="server" class="btn btn-secondary mt-3">Back to Invoices</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlFormBody" runat="server">
            <div class="card p-6 mb-5">
                <h3 class="font-bold mb-3 text-sm"><asp:Label ID="lblInvoiceNo" runat="server" /> — <asp:Label ID="lblStudentInfo" runat="server" /></h3>
                <div class="detail-row"><span class="k">Total Amount</span><span class="v"><asp:Label ID="lblTotalAmount" runat="server" /></span></div>
                <div class="detail-row"><span class="k">Paid So Far</span><span class="v"><asp:Label ID="lblPaidSoFar" runat="server" /></span></div>
                <div class="detail-row"><span class="k">Balance Due</span><span class="v"><asp:Label ID="lblBalanceDue" runat="server" /></span></div>
            </div>

            <div class="card p-6">
                <div class="form-grid two-col">
                    <div class="field">
                        <label>Receipt No.</label>
                        <asp:Label ID="lblReceiptNo" runat="server" CssClass="badge" Style="background:#EFF6FF;color:#1D4ED8;padding:.55rem .8rem;" />
                    </div>
                    <div class="field"></div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtAmount" Text="Amount *" />
                        <asp:TextBox ID="txtAmount" runat="server" CssClass="input" TextMode="Number" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAmount" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Amount is required." Text="Amount is required." />
                        <asp:CompareValidator runat="server" ControlToValidate="txtAmount" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" Operator="GreaterThan" ValueToCompare="0" Type="Currency" ErrorMessage="Amount must be greater than 0." Text="Amount must be greater than 0." />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlPaymentMethod" Text="Payment Method *" />
                        <asp:DropDownList ID="ddlPaymentMethod" runat="server" CssClass="input">
                            <asp:ListItem Text="Select Method" Value="" />
                            <asp:ListItem Text="Cash" Value="Cash" />
                            <asp:ListItem Text="Bank Transfer" Value="Bank Transfer" />
                            <asp:ListItem Text="Zaad" Value="Zaad" />
                            <asp:ListItem Text="eDahab" Value="eDahab" />
                            <asp:ListItem Text="Card" Value="Card" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlPaymentMethod" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a payment method." Text="Please select a payment method." InitialValue="" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtPaymentDate" Text="Payment Date *" />
                        <asp:TextBox ID="txtPaymentDate" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPaymentDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Payment date is required." Text="Payment date is required." />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtNotes" Text="Notes" />
                        <asp:TextBox ID="txtNotes" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" MaxLength="1000" />
                    </div>
                </div>

                <div class="form-actions">
                    <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                    <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Save" OnClick="btnSave_Click">
                        <i data-lucide="check" class="w-4 h-4"></i> Record Payment
                    </asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
