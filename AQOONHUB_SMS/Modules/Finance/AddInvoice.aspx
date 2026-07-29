<%@ Page Title="New Invoice | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AddInvoice.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.AddInvoice" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 900px; margin: 0 auto; }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .fee-item-row { display:flex; align-items:center; justify-content:space-between; padding:.6rem 0; border-bottom:1px solid #F1F5F9; font-size:.85rem; }
        .dark .fee-item-row { border-color:#263449; }
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
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">New Invoice</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Generate New Invoice</h1>

        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>
        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" ValidationGroup="Save" />

        <div class="card p-6 mb-5">
            <div class="form-grid two-col">
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtStudentSearch" Text="Student *" />
                    <div class="flex gap-2">
                        <asp:TextBox ID="txtStudentSearch" runat="server" CssClass="input" placeholder="Search by name or student code…" />
                        <asp:LinkButton ID="btnFindStudent" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnFindStudent_Click">Find</asp:LinkButton>
                    </div>
                    <asp:CustomValidator ID="cvStudentSelected" runat="server" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" OnServerValidate="cvStudentSelected_ServerValidate" ErrorMessage="Please select a student." Text="Please select a student." />
                    <asp:HiddenField ID="hdnStudentId" runat="server" />
                    <asp:Label ID="lblSelectedStudent" runat="server" CssClass="badge" Style="margin-top:.5rem;display:inline-block;background:#EFF6FF;color:#1D4ED8;" Visible="false" />
                </div>
                <div class="field"></div>
            </div>

            <asp:GridView ID="gvStudentResults" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full mt-3" OnRowCommand="gvStudentResults_RowCommand" Visible="false">
                <Columns>
                    <asp:BoundField DataField="StudentCode" HeaderText="Student Code" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                    <asp:BoundField DataField="FullName" HeaderText="Name" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                    <asp:BoundField DataField="ClassName" HeaderText="Class" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                    <asp:TemplateField HeaderText="">
                        <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                        <ItemTemplate><asp:LinkButton runat="server" CssClass="btn btn-primary !py-1 !px-3 !text-xs" CommandName="Select" CommandArgument='<%# Eval("StudentID") + "|" + Eval("ClassID") %>'>Select</asp:LinkButton></ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Panel ID="pnlInvoiceForm" runat="server" Visible="false">
            <div class="card p-6 mb-5">
                <div class="form-grid two-col mb-4">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlAcademicYear" Text="Academic Year *" />
                        <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlAcademicYear" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select an academic year." Text="Please select an academic year." InitialValue="0" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlTerm" Text="Term *" />
                        <asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlTerm_SelectedIndexChanged" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlTerm" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a term." Text="Please select a term." InitialValue="0" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtDueDate" Text="Due Date *" />
                        <asp:TextBox ID="txtDueDate" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDueDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Due date is required." Text="Due date is required." />
                    </div>
                </div>

                <h3 class="font-bold mb-2 text-sm">Applicable Fees</h3>
                <asp:CheckBoxList ID="cblFees" runat="server" CssClass="w-full" />
                <asp:Label ID="lblNoFees" runat="server" Text="No active fee structures found for this student's class and academic year." Visible="false" CssClass="text-sm text-gray-500 dark:text-slate-400" />

                <div class="flex justify-end mt-4 pt-3 border-t border-gray-100 dark:border-slate-700">
                    <p class="text-sm font-bold">Total: <asp:Label ID="lblTotalPreview" runat="server" Text="$0.00" /></p>
                </div>
            </div>

            <div class="form-actions">
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                <asp:LinkButton ID="btnGenerate" runat="server" CssClass="btn btn-primary" ValidationGroup="Save" OnClick="btnGenerate_Click">
                    <i data-lucide="check" class="w-4 h-4"></i> Generate Invoice
                </asp:LinkButton>
            </div>
        </asp:Panel>
    </div>
</asp:Content>