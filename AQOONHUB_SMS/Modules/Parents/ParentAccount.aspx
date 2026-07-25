<%@ Page Title="Parent Login Account | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ParentAccount.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Parents.ParentAccount" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 900px; margin: 0 auto; }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .alert-warning { background:#FFFBEB; color:#92400E; border:1px solid #FDE68A; }
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
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Login Account</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Parent Login Account — <asp:Label ID="lblGuardianName" runat="server" /></h1>

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
                <p class="font-bold">Guardian not found.</p>
                <a href="~/Modules/Parents/Parents.aspx" runat="server" class="btn btn-secondary mt-3">Back to Parents</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server">
            <div class="card p-6 mb-5">
                <h3 class="font-bold mb-3 text-sm">Current Status</h3>
                <div class="detail-row"><span class="k">Linked User Account</span><span class="v"><asp:Label ID="lblLinkedStatus" runat="server" /></span></div>
                <asp:Panel ID="pnlLinkedDetails" runat="server" Visible="false">
                    <div class="detail-row"><span class="k">Username / Email</span><span class="v"><asp:Label ID="lblLinkedUsername" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Account Status</span><span class="v"><asp:Label ID="lblLinkedAccountStatus" runat="server" /></span></div>
                </asp:Panel>
            </div>

            <asp:Panel ID="pnlNotImplemented" runat="server" CssClass="alert alert-warning">
                <i data-lucide="info" class="w-4 h-4 mt-0.5"></i>
                <span>
                    Creating a new login account, linking an existing Parent-role user, and password reset are not implemented yet on this page.
                    Building these safely requires confirming the exact <b>Users</b> table columns, the <b>Parent</b> role's exact stored value, and the
                    project's existing password-hashing method (from <code>Login.aspx.cs</code>) — none of which have been verified. Guessing at any of
                    these would risk creating a second, incompatible authentication path or an insecure password store.
                    Share those details and this page can be completed to actually create/link accounts.
                </span>
            </asp:Panel>

            <a href="~/Modules/Parents/Parents.aspx" runat="server" class="btn btn-secondary mt-4"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Parents</a>
        </asp:Panel>
    </div>
</asp:Content>
