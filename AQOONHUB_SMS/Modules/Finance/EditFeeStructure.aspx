<%@ Page Title="Edit Fee Structure | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="EditFeeStructure.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.EditFeeStructure" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 900px; margin: 0 auto; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
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
            <a href="~/Modules/Finance/FeeStructures.aspx" runat="server" class="hover:text-brand-600">Fee Structure</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Edit</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Edit Fee Structure</h1>

        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>
        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" ValidationGroup="Save" />

        <div class="card p-6">
            <div class="form-grid two-col">
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtFeeName" Text="Fee Name *" />
                    <asp:TextBox ID="txtFeeName" runat="server" CssClass="input" MaxLength="200" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFeeName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Fee name is required." Text="Fee name is required." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlCategory" Text="Category *" />
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="input" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlCategory" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a category." Text="Please select a category." InitialValue="" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlAcademicYear" Text="Academic Year *" />
                    <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="input" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlAcademicYear" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select an academic year." Text="Please select an academic year." InitialValue="0" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlClass" Text="Class *" />
                    <asp:DropDownList ID="ddlClass" runat="server" CssClass="input">
                        <asp:ListItem Text="Select Class" Value="0" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlClass" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a class." Text="Please select a class." InitialValue="0" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtAmount" Text="Amount *" />
                    <asp:TextBox ID="txtAmount" runat="server" CssClass="input" TextMode="Number" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtAmount" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Amount is required." Text="Amount is required." />
                    <asp:CompareValidator runat="server" ControlToValidate="txtAmount" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" Operator="GreaterThan" ValueToCompare="0" Type="Currency" ErrorMessage="Amount must be greater than 0." Text="Amount must be greater than 0." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlBillingTerm" Text="Billing Term *" />
                    <asp:DropDownList ID="ddlBillingTerm" runat="server" CssClass="input">
                        <asp:ListItem Text="Select Billing Term" Value="" />
                        <asp:ListItem Text="Termly" Value="Termly" />
                        <asp:ListItem Text="Annual" Value="Annual" />
                        <asp:ListItem Text="Monthly" Value="Monthly" />
                        <asp:ListItem Text="One-Time" Value="One-Time" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlBillingTerm" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a billing term." Text="Please select a billing term." InitialValue="" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlIsActive" Text="Status" />
                    <asp:DropDownList ID="ddlIsActive" runat="server" CssClass="input">
                        <asp:ListItem Text="Active" Value="1" Selected="True" />
                        <asp:ListItem Text="Inactive" Value="0" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="form-actions">
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Save" OnClick="btnSave_Click">
                    <i data-lucide="check" class="w-4 h-4"></i> Save
                </asp:LinkButton>
            </div>
        </div>
    </div>
</asp:Content>