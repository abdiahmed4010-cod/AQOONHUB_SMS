<%@ Page Title="Review Application | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AdmissionReview.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Admission.AdmissionReview" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .review-wrap { padding: 1.25rem; max-width: 1000px; margin: 0 auto; }

        .review-hero { display:flex; align-items:center; gap:1rem; padding:1.5rem; border-bottom:1px solid #E5E7EB; }
        .dark .review-hero { border-color:#334155; }
        .review-hero .av { width:3.25rem; height:3.25rem; border-radius:.9rem; display:flex; align-items:center; justify-content:center; background:#EFF6FF; color:#2563EB; flex-shrink:0; }
        .dark .review-hero .av { background:#1E293B; color:#93C5FD; }

        .detail-block { padding:1.25rem 1.5rem; }
        .detail-block h3 { font-size:.72rem; font-weight:800; text-transform:uppercase; letter-spacing:.05em; color:#6B7280; margin-bottom:.5rem; }
        .dark .detail-block h3 { color:#94A3B8; }
        .detail-row { display:flex; justify-content:space-between; gap:1rem; padding:.6rem 0; border-bottom:1px solid #F1F5F9; font-size:.82rem; }
        .dark .detail-row { border-color:#263449; }
        .detail-row .k { color:#6B7280; font-weight:600; }
        .dark .detail-row .k { color:#94A3B8; }
        .detail-row .v { font-weight:700; text-align:right; }

        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .card-head { padding:1rem 1.5rem; border-bottom:1px solid #E5E7EB; }
        .dark .card-head { border-color:#334155; }
        .card-head h3 { font-size:.95rem; font-weight:800; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .alert-info { background:#EFF6FF; color:#1D4ED8; border:1px solid #DBEAFE; }
        .form-actions { display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; padding:1.25rem 1.5rem; border-top:1px solid #E5E7EB; background:#F8FAFC; }
        .dark .form-actions { border-color:#334155; background:#0F172A; }
        @media (max-width:768px){ .review-wrap{padding:.875rem;} }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="review-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Admission/Admissions.aspx" runat="server" class="hover:text-brand-600">Admissions</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Review</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Review Application</h1>

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
                <p class="font-bold">Application not found.</p>
                <a href="~/Modules/Admission/Admissions.aspx" runat="server" class="btn btn-secondary mt-3">Back to Admissions</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server">
            <div class="card overflow-hidden mb-5">
                <div class="review-hero">
                    <span class="av"><i data-lucide="user-round" class="w-7 h-7"></i></span>
                    <div class="flex items-center gap-3 flex-wrap">
                        <h2 class="text-lg font-extrabold"><asp:Label ID="lblFullName" runat="server" /></h2>
                        <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" />
                    </div>
                </div>

                <div class="grid md:grid-cols-2">
                    <div class="detail-block md:border-r border-gray-100 dark:border-slate-700">
                        <h3>Applicant</h3>
                        <div class="detail-row"><span class="k">Application No.</span><span class="v"><asp:Label ID="lblApplicationNo" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Gender</span><span class="v"><asp:Label ID="lblGender" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Date of Birth</span><span class="v"><asp:Label ID="lblDob" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Applying For</span><span class="v"><asp:Label ID="lblClass" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Shift</span><span class="v"><asp:Label ID="lblShift" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Application Date</span><span class="v"><asp:Label ID="lblAppDate" runat="server" /></span></div>
                    </div>
                    <div class="detail-block">
                        <h3>Guardian &amp; Review</h3>
                        <div class="detail-row"><span class="k">Guardian Name</span><span class="v"><asp:Label ID="lblGuardianName" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Guardian Phone</span><span class="v"><asp:Label ID="lblGuardianPhone" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Guardian Email</span><span class="v"><asp:Label ID="lblGuardianEmail" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Notes</span><span class="v"><asp:Label ID="lblNotes" runat="server" /></span></div>
                        <div class="detail-row"><span class="k">Reviewed By / At</span><span class="v"><asp:Label ID="lblReviewed" runat="server" /></span></div>
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlNoGuardianWarning" runat="server" CssClass="alert alert-danger" Visible="false">
                <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
                <span>
                    This application is not linked to a valid Guardian record. Select or create a Guardian before enrollment.
                    <div class="flex gap-2 mt-3 flex-wrap">
                        <asp:DropDownList ID="ddlLinkExistingGuardian" runat="server" CssClass="input !w-auto" />
                        <asp:LinkButton ID="btnLinkExistingGuardian" runat="server" CssClass="btn btn-secondary !py-1.5 !text-xs" CausesValidation="false" OnClick="btnLinkExistingGuardian_Click">Link Selected Guardian</asp:LinkButton>
                        <asp:LinkButton ID="btnCreateGuardianFromApp" runat="server" CssClass="btn btn-secondary !py-1.5 !text-xs" CausesValidation="false" OnClick="btnCreateGuardianFromApp_Click"
                            OnClientClick="return confirm('Create a new Guardian from this application\'s name/phone/email?');">Create Guardian From Application</asp:LinkButton>
                    </div>
                </span>
            </asp:Panel>

            <asp:Panel ID="pnlActionForm" runat="server">
                <div class="card overflow-hidden mb-5">
                    <div class="card-head">
                        <h3>Approve &amp; Enroll</h3>
                    </div>
                    <div class="p-6">
                        <div class="form-grid two-col">
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="ddlAcademicYear" Text="Academic Year *" />
                                <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlAcademicYear" CssClass="field-error" Display="Dynamic" ValidationGroup="Approve" ErrorMessage="Please select an academic year." Text="Please select an academic year." InitialValue="0" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="ddlSection" Text="Section *" />
                                <asp:DropDownList ID="ddlSection" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlSection" CssClass="field-error" Display="Dynamic" ValidationGroup="Approve" ErrorMessage="Please select a section." Text="Please select a section." InitialValue="0" />
                            </div>
                        </div>
                    </div>
                    <div class="form-actions">
                        <asp:LinkButton ID="btnUnderReview" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnUnderReview_Click">
                            <i data-lucide="eye" class="w-4 h-4"></i> Mark Under Review
                        </asp:LinkButton>
                        <asp:LinkButton ID="btnReject" runat="server" CssClass="btn btn-secondary !text-red-500" CausesValidation="false" OnClick="btnReject_Click"
                            OnClientClick="return confirm('Reject this application?');">
                            <i data-lucide="x" class="w-4 h-4"></i> Reject
                        </asp:LinkButton>
                        <asp:LinkButton ID="btnApprove" runat="server" CssClass="btn btn-primary" ValidationGroup="Approve" OnClick="btnApprove_Click"
                            OnClientClick="return confirm('Approve this application and enroll as a student?');">
                            <i data-lucide="check" class="w-4 h-4"></i> Approve &amp; Enroll
                        </asp:LinkButton>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlAlreadyFinalized" runat="server" CssClass="alert alert-info" Visible="false">
                <i data-lucide="info" class="w-4 h-4 mt-0.5"></i>
                <asp:Label ID="lblFinalizedText" runat="server" />
            </asp:Panel>

            <a href="~/Modules/Admission/Admissions.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Admissions</a>
        </asp:Panel>
    </div>
</asp:Content>
