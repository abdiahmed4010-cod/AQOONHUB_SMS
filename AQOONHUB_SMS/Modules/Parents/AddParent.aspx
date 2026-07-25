<%@ Page Title="Add Parent | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AddParent.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Parents.AddParent" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 1000px; margin: 0 auto; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .alert-warning { background:#FFFBEB; color:#92400E; border:1px solid #FDE68A; }
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
            <a href="~/Modules/Parents/Parents.aspx" runat="server" class="hover:text-brand-600">Parents</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Add Parent</span>
        </nav>
        <div class="mb-6">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Add Parent / Guardian</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Register a new guardian record.</p>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
            <i data-lucide="check-circle-2" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblSuccess" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlDuplicateWarning" runat="server" CssClass="alert alert-warning" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblDuplicateWarning" runat="server" />
        </asp:Panel>

        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" ValidationGroup="Save" />

        <asp:Panel ID="pnlFormBody" runat="server">
        <div class="card p-6">
            <div class="form-grid two-col">
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtFullName" Text="Full Name *" />
                    <asp:TextBox ID="txtFullName" runat="server" CssClass="input" MaxLength="100" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFullName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Full name is required." Text="Full name is required." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlRelationship" Text="Relationship *" />
                    <asp:DropDownList ID="ddlRelationship" runat="server" CssClass="input">
                        <asp:ListItem Text="Select Relationship" Value="" />
                        <asp:ListItem Text="Mother" Value="Mother" />
                        <asp:ListItem Text="Father" Value="Father" />
                        <asp:ListItem Text="Guardian" Value="Guardian" />
                        <asp:ListItem Text="Other" Value="Other" />
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlRelationship" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a relationship." Text="Please select a relationship." InitialValue="" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtPhone" Text="Phone *" />
                    <asp:TextBox ID="txtPhone" runat="server" CssClass="input" MaxLength="30" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPhone" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Phone is required." Text="Phone is required." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtAlternatePhone" Text="Alternate Phone" />
                    <asp:TextBox ID="txtAlternatePhone" runat="server" CssClass="input" MaxLength="30" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtEmail" Text="Email" />
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="input" MaxLength="100" TextMode="Email" />
                    <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ValidationGroup="Save"
                        ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="Please enter a valid email address." Text="Please enter a valid email address." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtNationalId" Text="National ID" />
                    <asp:TextBox ID="txtNationalId" runat="server" CssClass="input" MaxLength="30" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtOccupation" Text="Occupation" />
                    <asp:TextBox ID="txtOccupation" runat="server" CssClass="input" MaxLength="100" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtEmergencyContact" Text="Emergency Contact" />
                    <asp:TextBox ID="txtEmergencyContact" runat="server" CssClass="input" MaxLength="100" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtAddress" Text="Address" />
                    <asp:TextBox ID="txtAddress" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" MaxLength="200" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="Active" Value="1" Selected="True" />
                        <asp:ListItem Text="Inactive" Value="0" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="form-actions">
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnReset_Click">
                    <i data-lucide="rotate-ccw" class="w-4 h-4"></i> Reset
                </asp:LinkButton>
                <asp:LinkButton ID="btnSaveAndAddAnother" runat="server" CssClass="btn btn-secondary" ValidationGroup="Save" OnClick="btnSaveAndAddAnother_Click">
                    <i data-lucide="repeat" class="w-4 h-4"></i> Save and Add Another
                </asp:LinkButton>
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Save" OnClick="btnSave_Click">
                    <i data-lucide="check" class="w-4 h-4"></i> Save Parent
                </asp:LinkButton>
            </div>
        </div>
        </asp:Panel>
    </div>
</asp:Content>
