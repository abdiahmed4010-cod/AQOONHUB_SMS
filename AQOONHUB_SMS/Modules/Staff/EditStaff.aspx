<%@ Page Title="Edit Staff | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="EditStaff.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Staff.EditStaff" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 1000px; margin: 0 auto; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .readonly-pill { display:inline-flex; align-items:center; gap:.4rem; background:#EFF6FF; color:#1D4ED8; font-weight:700; font-size:.85rem; padding:.55rem .8rem; border-radius:.6rem; border:1px solid #DBEAFE; }
        .dark .readonly-pill { background:#1E293B; color:#93C5FD; border-color:#334155; }
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
            <a href="~/Modules/Staff/Staff.aspx" runat="server" class="hover:text-brand-600">Staff &amp; HR</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Edit</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Edit Staff Member</h1>

        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
            <i data-lucide="check-circle-2" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblSuccess" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>

        <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-danger" DisplayMode="BulletList" ValidationGroup="Save" />

        <asp:Panel ID="pnlNotFound" runat="server" Visible="false">
            <div class="card p-8 text-center">
                <p class="font-bold">Staff member not found.</p>
                <a href="~/Modules/Staff/Staff.aspx" runat="server" class="btn btn-secondary mt-3">Back to Staff</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlFormBody" runat="server">
        <div class="card p-6">
            <div class="form-grid two-col">
                <div class="field">
                    <label>Employee ID</label>
                    <asp:Label ID="lblEmployeeId" runat="server" CssClass="readonly-pill" />
                </div>
                <div class="field">
                    <label>User Account</label>
                    <asp:Label ID="lblUserAccount" runat="server" CssClass="readonly-pill" />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtDepartment" Text="Department *" />
                    <asp:TextBox ID="txtDepartment" runat="server" CssClass="input" MaxLength="50" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDepartment" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Department is required." Text="Department is required." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtPosition" Text="Position *" />
                    <asp:TextBox ID="txtPosition" runat="server" CssClass="input" MaxLength="100" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPosition" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Position is required." Text="Position is required." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtHireDate" Text="Hire Date *" />
                    <asp:TextBox ID="txtHireDate" runat="server" CssClass="input" TextMode="Date" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtHireDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Hire date is required." Text="Hire date is required." />
                    <asp:CustomValidator ID="cvHireDate" runat="server" ControlToValidate="txtHireDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" OnServerValidate="cvHireDate_ServerValidate" ErrorMessage="Hire date cannot be in the future." Text="Hire date cannot be in the future." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtSalary" Text="Salary *" />
                    <asp:TextBox ID="txtSalary" runat="server" CssClass="input" TextMode="Number" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtSalary" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Salary is required." Text="Salary is required." />
                    <asp:CompareValidator runat="server" ControlToValidate="txtSalary" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" Operator="GreaterThan" ValueToCompare="0" Type="Currency" ErrorMessage="Salary must be greater than 0." Text="Salary must be greater than 0." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtLeaveBalance" Text="Leave Balance (days) *" />
                    <asp:TextBox ID="txtLeaveBalance" runat="server" CssClass="input" TextMode="Number" />
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLeaveBalance" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Leave balance is required." Text="Leave balance is required." />
                    <asp:CompareValidator runat="server" ControlToValidate="txtLeaveBalance" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" Operator="GreaterThanEqual" ValueToCompare="0" Type="Integer" ErrorMessage="Leave balance cannot be negative." Text="Leave balance cannot be negative." />
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="ddlStatus" Text="Status" />
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="On Leave" Value="On Leave" />
                        <asp:ListItem Text="Inactive" Value="Inactive" />
                        <asp:ListItem Text="Retired" Value="Retired" />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="form-actions">
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Save" OnClick="btnSave_Click">
                    <i data-lucide="check" class="w-4 h-4"></i> Save Changes
                </asp:LinkButton>
            </div>
        </div>
        </asp:Panel>
    </div>
</asp:Content>
