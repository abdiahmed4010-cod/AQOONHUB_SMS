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
                        <label for="<%= txtFirstName.ClientID %>">First Name <span class="req">*</span></label>
                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="input" MaxLength="50" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirstName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="First name is required." Text="First name is required." />
                    </div>
                    <div class="field">
                        <label for="<%= txtLastName.ClientID %>">Last Name <span class="req">*</span></label>
                        <asp:TextBox ID="txtLastName" runat="server" CssClass="input" MaxLength="50" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLastName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Last name is required." Text="Last name is required." />
                    </div>
                    <div class="field">
                        <label for="<%= ddlGender.ClientID %>">Gender <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlGender" runat="server" CssClass="input">
                            <asp:ListItem Text="Select Gender" Value="" />
                            <asp:ListItem Text="Male" Value="Male" />
                            <asp:ListItem Text="Female" Value="Female" />
                        </asp:DropDownList>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlGender" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a gender." Text="Please select a gender." InitialValue="" />
                    </div>
                    <div class="field">
                        <label for="<%= ddlStatus.ClientID %>">Status <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                            <asp:ListItem Text="Graduated" Value="Graduated" />
                            <asp:ListItem Text="Transferred" Value="Transferred" />
                        </asp:DropDownList>
                    </div>
                    <div class="field">
                        <label for="<%= txtDateOfBirth.ClientID %>">Date of Birth <span class="req">*</span></label>
                        <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDateOfBirth" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Date of birth is required." Text="Date of birth is required." />
                        <asp:CustomValidator ID="cvDateOfBirth" runat="server" ControlToValidate="txtDateOfBirth" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" OnServerValidate="cvDateOfBirth_ServerValidate" ErrorMessage="Date of birth must be in the past and give a reasonable student age (3–25 years)." Text="Date of birth must be in the past and give a reasonable student age (3–25 years)." />
                    </div>
                    <div class="field">
                        <label for="<%= txtEnrollmentDate.ClientID %>">Enrollment Date <span class="req">*</span></label>
                        <asp:TextBox ID="txtEnrollmentDate" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEnrollmentDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Enrollment date is required." Text="Enrollment date is required." />
                    </div>
                </div>
            </div>

            <div class="form-section">
                <h2><i data-lucide="school" class="w-4 h-4 text-brand-600"></i> Academic Information</h2>
                <div class="form-grid two-col">
                    <div class="field">
                        <label for="<%= ddlAcademicYear.ClientID %>">Academic Year <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlAcademicYear" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select an academic year." Text="Please select an academic year." InitialValue="0" />
                    </div>
                    <div class="field"></div>
                    <div class="field">
                        <label for="<%= ddlClass.ClientID %>">Class <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_SelectedIndexChanged" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlClass" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a class." Text="Please select a class." InitialValue="0" />
                    </div>
                    <div class="field">
                        <label for="<%= ddlSection.ClientID %>">Section <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlSection" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlSection" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a section." Text="Please select a section." InitialValue="0" />
                    </div>
                </div>
            </div>

            <div class="form-section">
                <h2><i data-lucide="users" class="w-4 h-4 text-brand-600"></i> Guardian Information</h2>
                <asp:Panel ID="pnlGuardianField" runat="server">
                    <div class="field">
                        <label for="<%= ddlGuardian.ClientID %>">Guardian</label>
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
                        <label for="<%= txtAddress.ClientID %>">Address</label>
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" MaxLength="200" />
                    </div>
                    <div class="field">
                        <label for="<%= txtMedicalNotes.ClientID %>">Medical Notes</label>
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
    </div>
</asp:Content>
