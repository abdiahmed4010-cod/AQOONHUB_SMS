<%@ Page Title="Assign Student | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AssignStudent.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Parents.AssignStudent" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 1000px; margin: 0 auto; }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .alert-info { background:#EFF6FF; color:#1D4ED8; border:1px solid #DBEAFE; }
        .filter-bar { display:flex; flex-wrap:wrap; gap:.625rem; align-items:end; }
        .detail-row { display:flex; justify-content:space-between; gap:1rem; padding:.5rem 0; border-bottom:1px solid #F1F5F9; font-size:.82rem; }
        .dark .detail-row { border-color:#263449; }
        .detail-row .k { color:#6B7280; font-weight:600; }
        .dark .detail-row .k { color:#94A3B8; }
        .detail-row .v { font-weight:700; text-align:right; }
        @media (max-width:768px){ .form-wrap{padding:.875rem;} }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Parents/Parents.aspx" runat="server" class="hover:text-brand-600">Parents</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Assign Student</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Assign Student to Guardian</h1>

        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
            <i data-lucide="check-circle-2" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblSuccess" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>

        <div class="card p-6 mb-5">
            <h3 class="font-bold mb-3 text-sm">Target Guardian</h3>
            <div class="field">
                <asp:Label runat="server" AssociatedControlID="ddlGuardian" Text="Guardian *" />
                <asp:DropDownList ID="ddlGuardian" runat="server" CssClass="input" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlGuardian" CssClass="field-error" Display="Dynamic" ValidationGroup="Assign" ErrorMessage="Please select a guardian." Text="Please select a guardian." InitialValue="0" />
            </div>
        </div>

        <div class="card p-6 mb-5">
            <h3 class="font-bold mb-3 text-sm">Find Student</h3>
            <div class="filter-bar">
                <div class="field" style="flex:1;min-width:250px;">
                    <asp:Label runat="server" AssociatedControlID="txtSearch" Text="Search by Student Code, Admission No. or Name" />
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="input" />
                </div>
                <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnSearch_Click">Search</asp:LinkButton>
            </div>

            <div class="overflow-x-auto mt-4">
                <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full" OnRowCommand="gvResults_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="StudentCode" HeaderText="Student Code" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="AdmissionNo" HeaderText="Admission No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="FullName" HeaderText="Name" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="ClassName" HeaderText="Class" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="CurrentGuardianName" HeaderText="Current Guardian" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CssClass="btn btn-primary !py-1 !px-3 !text-xs" CommandName="Select" CommandArgument='<%# Eval("StudentID") %>'>Select</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="text-center py-8 text-sm text-gray-500 dark:text-slate-400">Search for a student above.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <asp:Panel ID="pnlConfirm" runat="server" Visible="false">
            <div class="card p-6 mb-5">
                <h3 class="font-bold mb-3 text-sm">Confirm Assignment</h3>
                <div class="detail-row"><span class="k">Student</span><span class="v"><asp:Label ID="lblConfirmStudent" runat="server" /></span></div>
                <div class="detail-row"><span class="k">Current Guardian</span><span class="v"><asp:Label ID="lblConfirmCurrentGuardian" runat="server" /></span></div>
                <div class="detail-row"><span class="k">New Guardian</span><span class="v"><asp:Label ID="lblConfirmNewGuardian" runat="server" /></span></div>
                <div class="alert alert-info mt-4">
                    <i data-lucide="info" class="w-4 h-4 mt-0.5"></i>
                    This will replace the student's current primary guardian.
                </div>
                <div class="flex gap-2 justify-end mt-4">
                    <asp:LinkButton ID="btnCancelConfirm" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancelConfirm_Click">Cancel</asp:LinkButton>
                    <asp:LinkButton ID="btnConfirmAssign" runat="server" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnConfirmAssign_Click"
                        OnClientClick="return confirm('Replace this student\'s guardian?');">
                        <i data-lucide="check" class="w-4 h-4"></i> Confirm Assignment
                    </asp:LinkButton>
                </div>
            </div>
        </asp:Panel>

        <a href="~/Modules/Parents/Parents.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Parents</a>
    </div>
</asp:Content>
