<%@ Page Title="Add Student | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AddStudent.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Students.AddStudent" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Only page-specific additions — btn, input, card, badge, avatar already
           come from MainMaster.master's shared stylesheet. */
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
        .guardian-empty { background:#FFFBEB; border:1px solid #FDE68A; color:#92400E; font-size:.78rem; border-radius:.6rem; padding:.7rem .9rem; margin-top:.5rem; }
        .dark .guardian-empty { background:#3F2D0A; border-color:#78350F; color:#FCD34D; }
        .form-actions { display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; padding-top:1rem; border-top:1px solid #E5E7EB; margin-top:.5rem; }
        .dark .form-actions { border-color:#334155; }
        @media (max-width:768px){ .form-wrap{padding:.875rem;} .form-actions{justify-content:stretch;} .form-actions .btn{flex:1;justify-content:center;} }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-wrap">

        <!-- Header -->
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Students/Students.aspx" runat="server" class="hover:text-brand-600">Student Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Add Student</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Add Student</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Register a new student in AQOONHUB</p>
            </div>
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

        <asp:Panel ID="pnlFormBody" runat="server">
        <div class="card p-6">

            <!-- SECTION 1 — Student Identification -->
            <div class="form-section">
                <h2><i data-lucide="id-card" class="w-4 h-4 text-brand-600"></i> Student Identification</h2>
                <p class="sub">Student Code and Admission Number are generated automatically.</p>
                <div class="form-grid two-col">
                    <div class="field">
                        <label>Student Code</label>
                        <asp:Label ID="lblStudentCode" runat="server" CssClass="readonly-pill" />
                        <asp:HiddenField ID="hdnStudentCode" runat="server" />
                    </div>
                    <div class="field">
                        <label>Admission Number</label>
                        <asp:Label ID="lblAdmissionNo" runat="server" CssClass="readonly-pill" />
                        <asp:HiddenField ID="hdnAdmissionNo" runat="server" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtFirstName" Text="First Name *" />
                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="input" MaxLength="50" placeholder="e.g. Ayan" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirstName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="First name is required." Text="First name is required." />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtLastName" Text="Last Name *" />
                        <asp:TextBox ID="txtLastName" runat="server" CssClass="input" MaxLength="50" placeholder="e.g. Abdirahman" />
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
                            <asp:ListItem Text="Active" Value="Active" Selected="True" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
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

            <!-- SECTION 2 — Academic Information -->
            <div class="form-section">
                <h2><i data-lucide="school" class="w-4 h-4 text-brand-600"></i> Academic Information</h2>
                <p class="sub">Sections load automatically once a Class is selected.</p>
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
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlShift" Text="Shift *" />
                        <asp:DropDownList ID="ddlShift" runat="server" CssClass="input">
                            <asp:ListItem Text="Select Shift" Value="" />
                            <asp:ListItem Text="Morning" Value="Morning" />
                            <asp:ListItem Text="Afternoon" Value="Afternoon" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlShift" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a shift." Text="Please select a shift." InitialValue="" />
                    </div>
                </div>
            </div>

            <!-- SECTION 3 — Guardian Information -->
            <div class="form-section">
                <h2><i data-lucide="users" class="w-4 h-4 text-brand-600"></i> Guardian Information</h2>
                <p class="sub">Select the guardian responsible for this student.</p>
                <asp:Panel ID="pnlGuardianField" runat="server">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlGuardian" Text="Guardian" />
                        <asp:DropDownList ID="ddlGuardian" runat="server" CssClass="input" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlNoGuardians" runat="server" Visible="false">
                    <div class="guardian-empty">
                        <i data-lucide="info" class="w-3.5 h-3.5" style="display:inline;vertical-align:-2px;"></i>
                        No guardian records were found. A guardian must exist in the system before one can be linked here.
                    </div>
                </asp:Panel>
            </div>

            <!-- SECTION 4 — Personal & Health Information -->
            <div class="form-section">
                <h2><i data-lucide="heart-pulse" class="w-4 h-4 text-brand-600"></i> Personal &amp; Health Information</h2>
                <div class="form-grid two-col">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtAddress" Text="Address" />
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" MaxLength="200" placeholder="District, city…" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtMedicalNotes" Text="Medical Notes" />
                        <asp:TextBox ID="txtMedicalNotes" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" MaxLength="500" placeholder="Allergies, conditions, medications… (optional)" />
                    </div>
                </div>
            </div>

            <!-- SECTION 5 — Student Photo -->
            <div class="form-section">
                <h2><i data-lucide="camera" class="w-4 h-4 text-brand-600"></i> Student Photo</h2>
                <p class="sub">JPG, PNG or WEBP — max 2 MB. Optional.</p>
                <div class="photo-uploader">
                    <img id="imgPreview" class="photo-preview" alt="Student photo preview" src="" style="display:none;" />
                    <div id="imgPreviewFallback" class="photo-preview">
                        <i data-lucide="user" class="w-8 h-8"></i>
                    </div>
                    <div>
                        <asp:FileUpload ID="fuPhoto" runat="server" CssClass="input" onchange="AQPreviewPhoto(this)" />
                        <asp:RegularExpressionValidator ID="revPhoto" runat="server" ControlToValidate="fuPhoto"
                            ValidationExpression="^([Cc]:\\fakepath\\)?.*\.([Jj][Pp][Gg]|[Jj][Pp][Ee][Gg]|[Pp][Nn][Gg]|[Ww][Ee][Bb][Pp])$"
                            CssClass="field-error" Display="Dynamic" ValidationGroup="Save"
                            ErrorMessage="Only .jpg, .jpeg, .png or .webp files are allowed." Text="Only .jpg, .jpeg, .png or .webp files are allowed." />
                        <asp:CustomValidator ID="cvPhoto" runat="server" ControlToValidate="fuPhoto" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" OnServerValidate="cvPhoto_ServerValidate" ErrorMessage="Photo must be 2 MB or smaller and a valid image file." Text="Photo must be 2 MB or smaller and a valid image file." />
                    </div>
                </div>
            </div>

            <!-- Actions -->
            <div class="form-actions">
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click">
                    Cancel
                </asp:LinkButton>
                <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnReset_Click">
                    <i data-lucide="rotate-ccw" class="w-4 h-4"></i> Reset Form
                </asp:LinkButton>
                <asp:LinkButton ID="btnSaveAndAddAnother" runat="server" CssClass="btn btn-secondary" ValidationGroup="Save" OnClick="btnSaveAndAddAnother_Click">
                    <i data-lucide="repeat" class="w-4 h-4"></i> Save and Add Another
                </asp:LinkButton>
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Save" OnClick="btnSave_Click">
                    <i data-lucide="check" class="w-4 h-4"></i> Save Student
                </asp:LinkButton>
            </div>
        </div>
        </asp:Panel>
    </div>

    <script>
        function AQPreviewPhoto(input) {
            var img = document.getElementById('imgPreview');
            var fallback = document.getElementById('imgPreviewFallback');
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    img.src = e.target.result;
                    img.style.display = 'block';
                    fallback.style.display = 'none';
                };
                reader.readAsDataURL(input.files[0]);
            } else {
                img.style.display = 'none';
                fallback.style.display = 'flex';
            }
        }
    </script>
</asp:Content>
