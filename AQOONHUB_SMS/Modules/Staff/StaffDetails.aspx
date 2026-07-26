<%@ Page Title="Staff Details | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="StaffDetails.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Staff.StaffDetails" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .details-wrap { padding: 1.25rem; max-width: 1000px; margin: 0 auto; }
        .profile-card { display:flex; flex-wrap:wrap; align-items:center; gap:1.25rem; }
        .profile-photo-fallback { width:64px; height:64px; border-radius:.9rem; background:#7C3AED; color:#fff; display:flex; align-items:center; justify-content:center; font-weight:800; font-size:1.2rem; flex-shrink:0; }
        .detail-row { display:flex; justify-content:space-between; gap:1rem; padding:.5rem 0; border-bottom:1px solid #F1F5F9; font-size:.8rem; }
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
            <a href="~/Modules/Staff/Staff.aspx" runat="server" class="hover:text-brand-600">Staff &amp; HR</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Details</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Staff Details</h1>

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
                <p class="font-bold">Staff member not found.</p>
                <a href="~/Modules/Staff/Staff.aspx" runat="server" class="btn btn-secondary mt-3">Back to Staff</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server">
            <div class="card p-6 mb-5 profile-card">
                <div class="profile-photo-fallback"><asp:Label ID="lblInitials" runat="server" /></div>
                <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2 flex-wrap">
                        <h2 class="text-lg font-extrabold"><asp:Label ID="lblFullName" runat="server" /></h2>
                        <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" />
                    </div>
                    <p class="text-xs text-gray-500 dark:text-slate-400 mt-1">
                        <asp:Label ID="lblEmployeeId" runat="server" /> &middot; <asp:Label ID="lblPosition" runat="server" /> &middot; <asp:Label ID="lblDepartment" runat="server" />
                    </p>
                </div>
                <div class="flex gap-2 flex-wrap">
                    <asp:HyperLink ID="lnkEdit" runat="server" CssClass="btn btn-primary"><i data-lucide="pencil" class="w-4 h-4"></i> Edit</asp:HyperLink>
                    <asp:LinkButton ID="btnToggleLeave" runat="server" CssClass="btn btn-secondary" OnClick="btnToggleLeave_Click">
                        <i data-lucide="calendar-clock" class="w-4 h-4"></i> <asp:Label ID="lblToggleLeaveText" runat="server" />
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnDeactivate" runat="server" CssClass="btn btn-secondary" OnClick="btnDeactivate_Click"
                        OnClientClick="return confirm('Mark this staff member as Inactive?');">
                        <i data-lucide="power" class="w-4 h-4"></i> Deactivate
                    </asp:LinkButton>
                </div>
            </div>

            <div class="grid lg:grid-cols-2 gap-4">
                <div class="card p-6">
                    <h3 class="font-bold mb-3 text-sm">Contact Information</h3>
                    <div class="detail-row"><span class="k">Full Name</span><span class="v"><asp:Label ID="lblDetailName" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Email</span><span class="v"><asp:Label ID="lblDetailEmail" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Phone</span><span class="v"><asp:Label ID="lblDetailPhone" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Role</span><span class="v"><asp:Label ID="lblDetailRole" runat="server" /></span></div>
                </div>
                <div class="card p-6">
                    <h3 class="font-bold mb-3 text-sm">Employment Information</h3>
                    <div class="detail-row"><span class="k">Employee ID</span><span class="v"><asp:Label ID="lblDetailEmployeeId" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Department</span><span class="v"><asp:Label ID="lblDetailDepartment" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Position</span><span class="v"><asp:Label ID="lblDetailPosition" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Hire Date</span><span class="v"><asp:Label ID="lblDetailHireDate" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Salary</span><span class="v"><asp:Label ID="lblDetailSalary" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Leave Balance</span><span class="v"><asp:Label ID="lblDetailLeaveBalance" runat="server" /></span></div>
                </div>
            </div>

            <a href="~/Modules/Staff/Staff.aspx" runat="server" class="btn btn-secondary mt-5"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Staff</a>
        </asp:Panel>
    </div>
</asp:Content>
