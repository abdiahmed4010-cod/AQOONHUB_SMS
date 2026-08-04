<%@ Page Title="Student Details | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="StudentDetails.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Students.StudentDetails" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .details-wrap { padding: 1.25rem; max-width: 1000px; margin: 0 auto; }
        .profile-card { display:flex; flex-wrap:wrap; align-items:center; gap:1.25rem; }
        .profile-photo { width:72px; height:72px; border-radius:.9rem; object-fit:cover; border:1px solid #E5E7EB; }
        .profile-photo-fallback { width:72px; height:72px; border-radius:.9rem; display:flex; align-items:center; justify-content:center; color:#fff; font-weight:800; font-size:1.4rem; flex-shrink:0; }
        .stat-mini { text-align:center; }
        .stat-mini .v { font-size:1.05rem; font-weight:800; }
        .stat-mini .l { font-size:.65rem; font-weight:700; text-transform:uppercase; color:#6B7280; }
        .dark .stat-mini .l { color:#94A3B8; }
        .detail-row { display:flex; justify-content:space-between; gap:1rem; padding:.65rem 0; border-bottom:1px solid #F1F5F9; font-size:.82rem; }
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
            <a href="~/Modules/Students/Students.aspx" runat="server" class="hover:text-brand-600">Student Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Student Details</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Student Details</h1>

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
                <p class="font-bold">Student not found.</p>
                <a href="~/Modules/Students/Students.aspx" runat="server" class="btn btn-secondary mt-3">Back to Students</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server">
            <div class="card p-6 mb-5 profile-card">
                <asp:Image ID="imgPhoto" runat="server" CssClass="profile-photo" />
                <asp:Panel ID="pnlPhotoFallback" runat="server" CssClass="profile-photo-fallback">
                    <asp:Label ID="lblInitials" runat="server" />
                </asp:Panel>
                <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2 flex-wrap">
                        <h2 class="text-lg font-extrabold"><asp:Label ID="lblFullName" runat="server" /></h2>
                        <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" />
                    </div>
                    <p class="text-xs text-gray-500 dark:text-slate-400 mt-1">
                        <asp:Label ID="lblStudentCode" runat="server" /> &middot; <asp:Label ID="lblAdmissionNo" runat="server" /> &middot; <asp:Label ID="lblClassSection" runat="server" />
                    </p>
                </div>
                <div class="flex gap-6">
                    <div class="stat-mini"><div class="v"><asp:Label ID="lblGenderStat" runat="server" /></div><div class="l">Gender</div></div>
                    <div class="stat-mini"><div class="v"><asp:Label ID="lblAgeStat" runat="server" /></div><div class="l">Age</div></div>
                    <div class="stat-mini"><div class="v"><asp:Label ID="lblStatusStat" runat="server" /></div><div class="l">Status</div></div>
                </div>
            </div>

            <asp:Panel ID="pnlTransferSummary" runat="server" CssClass="alert alert-info" Visible="false" Style="background:#EFF6FF;color:#1D4ED8;border:1px solid #DBEAFE;">
                <i data-lucide="arrow-right-left" class="w-4 h-4 mt-0.5"></i>
                <asp:Label ID="lblTransferSummaryText" runat="server" />
            </asp:Panel>

            <div class="flex gap-2 flex-wrap mb-5">
                <asp:HyperLink ID="lnkEdit" runat="server" CssClass="btn btn-primary">
                    <i data-lucide="pencil" class="w-4 h-4"></i> Edit
                </asp:HyperLink>
                <asp:LinkButton ID="btnToggleActive" runat="server" CssClass="btn btn-secondary" OnClick="btnToggleActive_Click">
                    <i data-lucide="power" class="w-4 h-4"></i> <asp:Label ID="lblToggleActiveText" runat="server" Text="Deactivate" />
                </asp:LinkButton>
                <asp:LinkButton ID="btnGraduate" runat="server" CssClass="btn btn-secondary" OnClick="btnGraduate_Click" OnClientClick="return confirm('Mark this student as Graduated?');">
                    <i data-lucide="graduation-cap" class="w-4 h-4"></i> Graduate
                </asp:LinkButton>
                <asp:HyperLink ID="lnkTransfer" runat="server" CssClass="btn btn-secondary">
                    <i data-lucide="arrow-right-left" class="w-4 h-4"></i> <asp:Label ID="lblTransferLinkText" runat="server" Text="Transfer Student" />
                </asp:HyperLink>
                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-secondary !text-red-500" OnClick="btnDelete_Click" OnClientClick="return confirm('Delete this student? This moves the record to Trash — a Super Admin can restore it later.');">
                    <i data-lucide="trash-2" class="w-4 h-4"></i> Delete
                </asp:LinkButton>
                <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-secondary ml-auto" NavigateUrl="~/Modules/Students/Students.aspx">
                    <i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Students
                </asp:HyperLink>
            </div>

            <div class="grid lg:grid-cols-2 gap-4">
                <div class="card p-6">
                    <h3 class="font-bold mb-3 text-sm">Personal Information</h3>
                    <div class="detail-row"><span class="k">Full Name</span><span class="v"><asp:Label ID="lblDetailName" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Gender</span><span class="v"><asp:Label ID="lblDetailGender" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Date of Birth</span><span class="v"><asp:Label ID="lblDetailDob" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Address</span><span class="v"><asp:Label ID="lblDetailAddress" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Medical Notes</span><span class="v"><asp:Label ID="lblDetailMedical" runat="server" /></span></div>
                </div>
                <div class="card p-6">
                    <h3 class="font-bold mb-3 text-sm">Academic &amp; Guardian</h3>
                    <div class="detail-row"><span class="k">Student Code</span><span class="v"><asp:Label ID="lblDetailStudentCode" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Admission Number</span><span class="v"><asp:Label ID="lblDetailAdmissionNo" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Class / Section</span><span class="v"><asp:Label ID="lblDetailClassSection" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Academic Year</span><span class="v"><asp:Label ID="lblDetailAcademicYear" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Guardian</span><span class="v"><asp:Label ID="lblDetailGuardian" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Enrollment Date</span><span class="v"><asp:Label ID="lblDetailEnrolled" runat="server" /></span></div>
                </div>

                <div class="card p-6 md:col-span-2">
                    <h3 class="font-bold mb-3 text-sm">Parent Login Account</h3>
                    <div class="detail-row"><span class="k">Guardian</span><span class="v"><asp:Label ID="lblPAName" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Email</span><span class="v"><asp:Label ID="lblPAEmail" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Phone</span><span class="v"><asp:Label ID="lblPAPhone" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Account Status</span><span class="v"><asp:Label ID="lblPABadge" runat="server" CssClass="badge" /></span></div>
                    <div class="detail-row"><span class="k">Linked User</span><span class="v"><asp:Label ID="lblPALinkedEmail" runat="server" /></span></div>

                    <asp:Panel ID="pnlPAError" runat="server" Visible="false" role="alert" aria-live="assertive"
                        CssClass="mt-3 p-3 rounded-lg text-xs" style="background:#FEF2F2;color:#B91C1C;border:1px solid #FECACA;">
                        <asp:Label ID="lblPAError" runat="server" />
                    </asp:Panel>

                    <asp:Panel ID="pnlPASuccess" runat="server" Visible="false" role="status" aria-live="polite"
                        CssClass="mt-3 p-3 rounded-lg text-xs" style="background:#ECFDF5;color:#065F46;border:1px solid #A7F3D0;">
                        <p class="font-semibold mb-2"><asp:Label ID="lblPASuccessMsg" runat="server" /></p>
                        <div class="flex items-center gap-2" id="pnlTempWrap" runat="server" visible="false">
                            <span class="k">Temporary password</span>
                            <code style="font-size:.85rem;background:#fff;border:1px solid #A7F3D0;border-radius:.4rem;padding:.25rem .6rem;letter-spacing:.02em;"><asp:Label ID="lblPATempPassword" runat="server" /></code>
                        </div>
                    </asp:Panel>

                    <asp:Button ID="btnCreateParentAccount" runat="server" Visible="false" CssClass="btn btn-primary mt-3"
                        Text="Create Parent Account" OnClick="btnCreateParentAccount_Click"
                        OnClientClick="return confirm('Create a Parent login account for this guardian? A one-time temporary password will be shown once.');" />
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
