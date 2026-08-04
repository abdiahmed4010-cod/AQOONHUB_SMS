<%@ Page Title="Edit Student | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="EditStudent.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Students.EditStudent" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 1100px; margin: 0 auto; }
        .form-section { margin-bottom: 1.25rem; }
        .form-section h2 { font-size: .9rem; font-weight: 800; margin: 0 0 .2rem; display:flex; align-items:center; gap:.5rem; }
        .form-section p.sub { font-size:.75rem; color:#6B7280; margin:0 0 1rem; }
        .dark .form-section p.sub { color:#94A3B8; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field label .req { color:#EF4444; margin-left:.15rem; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .readonly-pill { display:inline-flex; align-items:center; gap:.4rem; background:#EFF6FF; color:#1D4ED8; font-weight:700; font-size:.85rem; padding:.55rem .8rem; border-radius:.6rem; border:1px solid #DBEAFE; }
        .dark .readonly-pill { background:#1E293B; color:#93C5FD; border-color:#334155; }
        .photo-uploader { display:flex; align-items:center; gap:1.25rem; flex-wrap:wrap; }
        .photo-preview { width:96px; height:96px; border-radius:.9rem; object-fit:cover; border:1px solid #E5E7EB; background:#F1F5F9; display:flex; align-items:center; justify-content:center; color:#9CA3AF; flex-shrink:0; }
        .dark .photo-preview { border-color:#334155; background:#1E293B; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .form-actions { display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; padding-top:1rem; border-top:1px solid #E5E7EB; margin-top:.5rem; }
        .dark .form-actions { border-color:#334155; }
        @media (max-width:768px){ .form-wrap{padding:.875rem;} .form-actions{justify-content:stretch;} .form-actions .btn{flex:1;justify-content:center;} }

        /* Placement confirmation modal */
        .pc-backdrop { position:fixed; inset:0; background:rgba(15,23,42,.55); z-index:80; display:flex; align-items:center; justify-content:center; padding:1rem; }
        .pc-modal { background:#fff; border-radius:1rem; width:100%; max-width:620px; max-height:90vh; overflow-y:auto; padding:1.5rem; box-shadow:0 20px 50px -12px rgba(15,23,42,.4); }
        .dark .pc-modal { background:#1E293B; color:#E2E8F0; }
        .pc-head { display:flex; align-items:center; gap:.5rem; }
        .pc-head h3 { font-size:1.05rem; font-weight:800; margin:0; }
        .pc-sub { font-size:.78rem; color:#6B7280; margin:.35rem 0 1rem; }
        .dark .pc-sub { color:#94A3B8; }
        .pc-student { display:flex; align-items:center; gap:.6rem; flex-wrap:wrap; margin-bottom:.9rem; }
        .pc-compare { display:grid; grid-template-columns:1fr auto 1fr; gap:.75rem; align-items:center; }
        @media (max-width:560px){ .pc-compare { grid-template-columns:1fr; } .pc-arrow { transform:rotate(90deg); justify-self:center; } }
        .pc-col { border:1px solid #E5E7EB; border-radius:.7rem; padding:.75rem .9rem; }
        .dark .pc-col { border-color:#334155; }
        .pc-col-new { border-color:#2563EB; background:#EFF6FF; }
        .dark .pc-col-new { background:#1E293B; border-color:#3B82F6; }
        .pc-col h4 { font-size:.68rem; font-weight:800; text-transform:uppercase; letter-spacing:.04em; color:#64748B; margin:0 0 .5rem; }
        .pc-col dl { margin:0; display:grid; grid-template-columns:auto 1fr; gap:.25rem .6rem; }
        .pc-col dt { font-size:.72rem; color:#6B7280; }
        .dark .pc-col dt { color:#94A3B8; }
        .pc-col dd { font-size:.82rem; font-weight:600; margin:0; text-align:right; }
        .pc-arrow { color:#94A3B8; }
        .pc-note { font-size:.76rem; color:#475569; background:#F8FAFC; border-radius:.6rem; padding:.6rem .8rem; margin:1rem 0 .5rem; }
        .dark .pc-note { color:#CBD5E1; background:#0F172A; }
        .pc-actions { display:flex; gap:.6rem; justify-content:flex-end; margin-top:1rem; }
        @media (max-width:560px){ .pc-actions { flex-direction:column-reverse; } .pc-actions .btn { width:100%; justify-content:center; } }
        @media (prefers-reduced-motion: reduce) { * { scroll-behavior:auto; } }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Students/Students.aspx" runat="server" class="hover:text-brand-600">Student Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Edit Student</span>
        </nav>
        <div class="mb-6">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Edit Student</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Student Code and Admission Number cannot be changed.</p>
        </div>

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
                <p class="font-bold">Student not found.</p>
                <a href="~/Modules/Students/Students.aspx" runat="server" class="btn btn-secondary mt-3">Back to Students</a>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlFormBody" runat="server">
        <div class="card p-6">

            <div class="form-section">
                <h2><i data-lucide="id-card" class="w-4 h-4 text-brand-600"></i> Student Identification</h2>
                <div class="form-grid two-col">
                    <div class="field">
                        <label>Student Code</label>
                        <asp:Label ID="lblStudentCode" runat="server" CssClass="readonly-pill" />
                    </div>
                    <div class="field">
                        <label>Admission Number</label>
                        <asp:Label ID="lblAdmissionNo" runat="server" CssClass="readonly-pill" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtFirstName" Text="First Name *" />
                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="input" MaxLength="50" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirstName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="First name is required." Text="First name is required." />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtLastName" Text="Last Name *" />
                        <asp:TextBox ID="txtLastName" runat="server" CssClass="input" MaxLength="50" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLastName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Last name is required." Text="Last name is required." />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlGender" Text="Gender *" />
                        <asp:DropDownList ID="ddlGender" runat="server" CssClass="input">
                            <asp:ListItem Text="Select Gender" Value="" />
                            <asp:ListItem Text="Male" Value="Male" />
                            <asp:ListItem Text="Female" Value="Female" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlGender" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a gender." Text="Please select a gender." InitialValue="" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlStatus" Text="Status *" />
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                            <asp:ListItem Text="Graduated" Value="Graduated" />
                            <asp:ListItem Text="Transferred" Value="Transferred" />
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtDateOfBirth" Text="Date of Birth *" />
                        <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDateOfBirth" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Date of birth is required." Text="Date of birth is required." />
                        <asp:CustomValidator ID="cvDateOfBirth" runat="server" ControlToValidate="txtDateOfBirth" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" OnServerValidate="cvDateOfBirth_ServerValidate" ErrorMessage="Date of birth must be in the past and give a reasonable student age (3–25 years)." Text="Date of birth must be in the past and give a reasonable student age (3–25 years)." />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtEnrollmentDate" Text="Enrollment Date *" />
                        <asp:TextBox ID="txtEnrollmentDate" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEnrollmentDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Enrollment date is required." Text="Enrollment date is required." />
                    </div>
                </div>
            </div>

            <div class="form-section">
                <h2><i data-lucide="school" class="w-4 h-4 text-brand-600"></i> Academic Placement</h2>
                <p class="sub"><i data-lucide="info" class="w-3.5 h-3.5 inline-block align-[-2px]"></i> Changes to Academic Year, Class, Shift, or Section are recorded in the student's placement history.</p>
                <div class="form-grid two-col">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlAcademicYear" Text="Academic Year *" />
                        <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlAcademicYear" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select an academic year." Text="Please select an academic year." InitialValue="0" />
                    </div>
                    <div class="field"></div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlClass" Text="Class *" />
                        <asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_SelectedIndexChanged" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlClass" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a class." Text="Please select a class." InitialValue="0" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlSection" Text="Section *" />
                        <asp:DropDownList ID="ddlSection" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlSection" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a section." Text="Please select a section." InitialValue="0" />
                        <p class="text-[11px] text-gray-500 dark:text-slate-400 mt-1">Sections are filtered by shift. Sections marked <em>“Shift Not Assigned”</em> cannot receive a moved student until configured in <a class="text-brand-600 hover:underline" href="<%= ResolveUrl("~/Modules/Academic/ClassesSections.aspx") %>">Classes &amp; Sections</a>.</p>
                        <asp:Panel ID="pnlShiftWarn" runat="server" Visible="false" CssClass="mt-2 p-2.5 rounded-lg text-xs" role="alert" aria-live="polite" style="background:#FEF3C7;color:#92400E;border:1px solid #FDE68A;">
                            This section contains mixed-shift students. Resolve the existing student placements before assigning a section shift. The student's shift will not be changed automatically.
                        </asp:Panel>
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlShift" Text="Shift *" />
                        <asp:DropDownList ID="ddlShift" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlShift_Changed">
                            <asp:ListItem Text="Select Shift" Value="" />
                            <asp:ListItem Text="Morning" Value="Morning" />
                            <asp:ListItem Text="Afternoon" Value="Afternoon" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlShift" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a shift." Text="Please select a shift." InitialValue="" />
                    </div>
                </div>
            </div>

            <div class="form-section">
                <h2><i data-lucide="users" class="w-4 h-4 text-brand-600"></i> Guardian Information</h2>
                <asp:Panel ID="pnlGuardianField" runat="server">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlGuardian" Text="Guardian" />
                        <asp:DropDownList ID="ddlGuardian" runat="server" CssClass="input" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlNoGuardians" runat="server" Visible="false">
                    <div class="guardian-empty">No guardian records were found.</div>
                </asp:Panel>
            </div>

            <div class="form-section">
                <h2><i data-lucide="heart-pulse" class="w-4 h-4 text-brand-600"></i> Personal &amp; Health Information</h2>
                <div class="form-grid two-col">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtAddress" Text="Address" />
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" MaxLength="200" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtMedicalNotes" Text="Medical Notes" />
                        <asp:TextBox ID="txtMedicalNotes" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" MaxLength="500" />
                    </div>
                </div>
            </div>

            <div class="form-section">
                <h2><i data-lucide="camera" class="w-4 h-4 text-brand-600"></i> Student Photo</h2>
                <p class="sub">Leave empty to keep the current photo. JPG, PNG or WEBP — max 2 MB.</p>
                <div class="photo-uploader">
                    <asp:Image ID="imgCurrentPhoto" runat="server" CssClass="photo-preview" />
                    <asp:Panel ID="pnlCurrentPhotoFallback" runat="server" CssClass="photo-preview">
                        <i data-lucide="user" class="w-8 h-8"></i>
                    </asp:Panel>
                    <div>
                        <asp:FileUpload ID="fuPhoto" runat="server" CssClass="input" />
                        <asp:RegularExpressionValidator ID="revPhoto" runat="server" ControlToValidate="fuPhoto"
                            ValidationExpression="^([Cc]:\\fakepath\\)?.*\.([Jj][Pp][Gg]|[Jj][Pp][Ee][Gg]|[Pp][Nn][Gg]|[Ww][Ee][Bb][Pp])$"
                            CssClass="field-error" Display="Dynamic" ValidationGroup="Save"
                            ErrorMessage="Only .jpg, .jpeg, .png or .webp files are allowed." Text="Only .jpg, .jpeg, .png or .webp files are allowed." />
                        <asp:CustomValidator ID="cvPhoto" runat="server" ControlToValidate="fuPhoto" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" OnServerValidate="cvPhoto_ServerValidate" ErrorMessage="Photo must be 2 MB or smaller and a valid image file." Text="Photo must be 2 MB or smaller and a valid image file." />
                    </div>
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

        <%-- ===================== Placement change confirmation ===================== --%>
        <asp:Panel ID="pnlPlacementConfirm" runat="server" Visible="false" CssClass="pc-backdrop">
            <div class="pc-modal" role="dialog" aria-modal="true" aria-labelledby="pcTitle" aria-describedby="pcDesc" id="pcModal">
                <div class="pc-head">
                    <i data-lucide="git-branch" class="w-5 h-5 text-brand-600"></i>
                    <h3 id="pcTitle">Confirm Placement Change</h3>
                </div>
                <p id="pcDesc" class="pc-sub">Review the current and new academic placement, provide a reason and effective date, then confirm.</p>

                <asp:Panel ID="pnlPcError" runat="server" Visible="false" CssClass="alert alert-danger" role="alert" aria-live="assertive">
                    <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
                    <asp:Label ID="lblPcError" runat="server" />
                </asp:Panel>

                <div class="pc-student">
                    <span class="readonly-pill"><asp:Label ID="lblPcCode" runat="server" /></span>
                    <span class="font-bold"><asp:Label ID="lblPcName" runat="server" /></span>
                </div>

                <div class="pc-compare">
                    <div class="pc-col">
                        <h4>Current placement</h4>
                        <dl>
                            <dt>Academic Year</dt><dd><asp:Label ID="lblPcCurYear" runat="server" /></dd>
                            <dt>Class</dt><dd><asp:Label ID="lblPcCurClass" runat="server" /></dd>
                            <dt>Shift</dt><dd><asp:Label ID="lblPcCurShift" runat="server" /></dd>
                            <dt>Section</dt><dd><asp:Label ID="lblPcCurSection" runat="server" /></dd>
                        </dl>
                    </div>
                    <div class="pc-arrow" aria-hidden="true"><i data-lucide="arrow-right" class="w-5 h-5"></i></div>
                    <div class="pc-col pc-col-new">
                        <h4>New placement</h4>
                        <dl>
                            <dt>Academic Year</dt><dd><asp:Label ID="lblPcNewYear" runat="server" /></dd>
                            <dt>Class</dt><dd><asp:Label ID="lblPcNewClass" runat="server" /></dd>
                            <dt>Shift</dt><dd><asp:Label ID="lblPcNewShift" runat="server" /></dd>
                            <dt>Section</dt><dd><asp:Label ID="lblPcNewSection" runat="server" /></dd>
                        </dl>
                    </div>
                </div>

                <p class="pc-note"><i data-lucide="shield-check" class="w-4 h-4 inline-block align-[-3px] text-emerald-600"></i>
                    This change will create a placement history record. Existing attendance, examination, finance, and report history will not be rewritten.</p>

                <div class="form-grid two-col" style="margin-top:.5rem;">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlReason" Text="Placement Change Reason *" />
                        <asp:DropDownList ID="ddlReason" runat="server" CssClass="input">
                            <asp:ListItem Text="Select a reason" Value="" />
                            <asp:ListItem Text="Class Transfer" Value="Class Transfer" />
                            <asp:ListItem Text="Section Transfer" Value="Section Transfer" />
                            <asp:ListItem Text="Shift Change" Value="Shift Change" />
                            <asp:ListItem Text="Academic Promotion" Value="Academic Promotion" />
                            <asp:ListItem Text="Placement Correction" Value="Placement Correction" />
                            <asp:ListItem Text="Administrative Adjustment" Value="Administrative Adjustment" />
                            <asp:ListItem Text="Other" Value="Other" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlReason" CssClass="field-error" Display="Dynamic" ValidationGroup="Confirm" InitialValue="" ErrorMessage="Please select a reason." Text="Please select a reason." />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtEffectiveDate" Text="Effective Date *" />
                        <asp:TextBox ID="txtEffectiveDate" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEffectiveDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Confirm" ErrorMessage="Please provide an effective date." Text="Please provide an effective date." />
                    </div>
                </div>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtReasonOther" Text="Additional explanation (required when reason is Other)" />
                    <asp:TextBox ID="txtReasonOther" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" MaxLength="300" />
                </div>

                <asp:HiddenField ID="hfConfirmToken" runat="server" />
                <div class="pc-actions">
                    <asp:LinkButton ID="btnCancelPlacement" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancelPlacement_Click">Cancel</asp:LinkButton>
                    <asp:LinkButton ID="btnConfirmPlacement" runat="server" CssClass="btn btn-primary" ValidationGroup="Confirm" OnClick="btnConfirmPlacement_Click" OnClientClick="return aqoonPcConfirm(this);">
                        <i data-lucide="check" class="w-4 h-4"></i> Confirm Placement Change
                    </asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </div>

    <script>
        // Double-submit guard for the confirm button (server also enforces via token + concurrency).
        window.aqoonPcConfirm = function (btn) {
            if (typeof Page_ClientValidate === 'function' && !Page_ClientValidate('Confirm')) return false;
            if (btn.getAttribute('data-busy') === '1') return false;
            btn.setAttribute('data-busy', '1');
            btn.classList.add('opacity-60', 'pointer-events-none');
            return true;
        };
        // Focus management + Escape close for the placement modal.
        (function () {
            var modal = document.getElementById('pcModal');
            if (!modal) return;
            var focusable = modal.querySelector('select, input, textarea, a, button');
            if (focusable) { try { focusable.focus(); } catch (e) {} }
            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') { var c = document.getElementById('<%= btnCancelPlacement.ClientID %>'); if (c) c.click(); }
            });
        })();
    </script>
</asp:Content>
