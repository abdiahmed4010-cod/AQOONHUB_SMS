<%@ Page Title="New Admission | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AddAdmission.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Admission.AddAdmission" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-wrap { padding: 1.25rem; max-width: 1100px; margin: 0 auto; }

        /* Form card header banner */
        .form-banner { display:flex; align-items:center; gap:.9rem; padding:1.25rem 1.5rem; border-bottom:1px solid #E5E7EB; }
        .dark .form-banner { border-color:#334155; }
        .form-banner .fic { width:2.75rem; height:2.75rem; border-radius:.8rem; display:flex; align-items:center; justify-content:center; background:#EFF6FF; color:#2563EB; flex-shrink:0; }
        .dark .form-banner .fic { background:#1E293B; color:#93C5FD; }
        .form-banner h2 { font-size:1rem; font-weight:800; letter-spacing:-.01em; }
        .form-banner p { font-size:.75rem; color:#6B7280; margin-top:.1rem; }
        .dark .form-banner p { color:#94A3B8; }

        .form-body { padding:1.5rem; }
        .form-section { margin-bottom:1.5rem; }
        .form-section:last-of-type { margin-bottom:0; }
        .form-section > h2 { font-size:.8rem; font-weight:800; margin:0 0 .9rem; display:flex; align-items:center; gap:.5rem; text-transform:uppercase; letter-spacing:.04em; color:#374151; padding-bottom:.55rem; border-bottom:1px dashed #E5E7EB; }
        .dark .form-section > h2 { color:#CBD5E1; border-color:#334155; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1.1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .readonly-pill { display:inline-flex; align-items:center; gap:.4rem; background:#EFF6FF; color:#1D4ED8; font-weight:700; font-size:.85rem; padding:.55rem .8rem; border-radius:.6rem; border:1px solid #DBEAFE; }
        .dark .readonly-pill { background:#1E293B; color:#93C5FD; border-color:#334155; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .guardian-toggle { display:inline-flex; padding:.25rem; background:#F1F5F9; border-radius:.7rem; margin-bottom:1rem; }
        .dark .guardian-toggle { background:#0F172A; }
        .guardian-toggle label { font-size:.78rem; font-weight:700; padding:.4rem .9rem; border-radius:.5rem; cursor:pointer; color:#64748B; }
        .form-actions { display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; padding:1.25rem 1.5rem; border-top:1px solid #E5E7EB; background:#F8FAFC; }
        .dark .form-actions { border-color:#334155; background:#0F172A; }
        @media (max-width:768px){ .form-wrap{padding:.875rem;} .form-body{padding:1.1rem;} .form-actions{justify-content:stretch;} .form-actions .btn{flex:1;justify-content:center;} }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Admission/Admissions.aspx" runat="server" class="hover:text-brand-600">Admissions</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">New Application</span>
        </nav>
        <div class="mb-6">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight"><asp:Literal ID="litPageTitle" runat="server" Text="New Admission Application" /></h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litPageSubtitle" runat="server" Text="Application Number is generated automatically." /></p>
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
        <div class="card overflow-hidden">

            <div class="form-banner">
                <span class="fic"><i data-lucide="clipboard-list" class="w-6 h-6"></i></span>
                <div>
                    <h2>Student Application</h2>
                    <p>Fill in the applicant, class and guardian details below.</p>
                </div>
            </div>

            <div class="form-body">

            <div class="form-section">
                <h2><i data-lucide="id-card" class="w-4 h-4 text-brand-600"></i> Applicant Identification</h2>
                <div class="form-grid two-col">
                    <div class="field">
                        <label>Application Number</label>
                        <asp:Label ID="lblApplicationNo" runat="server" CssClass="readonly-pill" />
                        <asp:HiddenField ID="hdnApplicationNo" runat="server" />
                    </div>
                    <div class="field"></div>
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
                        <asp:Label runat="server" AssociatedControlID="txtDateOfBirth" Text="Date of Birth *" />
                        <asp:TextBox ID="txtDateOfBirth" runat="server" CssClass="input" TextMode="Date" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDateOfBirth" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Date of birth is required." Text="Date of birth is required." />
                        <asp:CustomValidator ID="cvDateOfBirth" runat="server" ControlToValidate="txtDateOfBirth" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" OnServerValidate="cvDateOfBirth_ServerValidate" ErrorMessage="Date of birth must be in the past and give a reasonable age (3–25 years)." Text="Date of birth must be in the past and give a reasonable age (3–25 years)." />
                    </div>
                </div>
            </div>

            <asp:Panel ID="pnlStatus" runat="server" CssClass="form-section" Visible="false">
                <h2><i data-lucide="flag" class="w-4 h-4 text-brand-600"></i> Application Status</h2>
                <div class="form-grid two-col">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlStatus" Text="Status *" />
                        <div class="flex gap-2">
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input" />
                            <asp:LinkButton ID="btnUpdateStatus" runat="server" CssClass="btn btn-secondary whitespace-nowrap" CausesValidation="false" OnClick="btnUpdateStatus_Click">
                                <i data-lucide="refresh-cw" class="w-4 h-4"></i> Update Status
                            </asp:LinkButton>
                        </div>
                        <p class="text-[11px] text-gray-400 mt-1">Change only the status (e.g. Rejected → Under Review / Enrolled) without editing other fields.</p>
                    </div>
                </div>
            </asp:Panel>

            <div class="form-section">
                <h2><i data-lucide="school" class="w-4 h-4 text-brand-600"></i> Applying For</h2>
                <div class="form-grid two-col">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlClass" Text="Class *" />
                        <asp:DropDownList ID="ddlClass" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlClass" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a class." Text="Please select a class." InitialValue="0" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlAcademicYear" Text="Academic Year" />
                        <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="input" />
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
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtPreviousSchool" Text="Previous School" />
                        <asp:TextBox ID="txtPreviousSchool" runat="server" CssClass="input" MaxLength="150" placeholder="Enter previous school name" />
                    </div>
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="txtLastGradeCompleted" Text="Last Grade Completed" />
                        <asp:TextBox ID="txtLastGradeCompleted" runat="server" CssClass="input" MaxLength="50" placeholder="Enter last grade" />
                    </div>
                </div>
            </div>

            <div class="form-section">
                <h2><i data-lucide="users" class="w-4 h-4 text-brand-600"></i> Guardian Information</h2>
                <div class="field guardian-toggle">
                    <asp:RadioButtonList ID="rblGuardianMode" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow"
                        AutoPostBack="true" OnSelectedIndexChanged="rblGuardianMode_SelectedIndexChanged" CssClass="flex gap-4">
                        <asp:ListItem Text="Select Existing Guardian" Value="Existing" Selected="True" />
                        <asp:ListItem Text="Create New Guardian" Value="New" />
                    </asp:RadioButtonList>
                </div>

                <asp:Panel ID="pnlExistingGuardian" runat="server">
                    <div class="field">
                        <asp:Label runat="server" AssociatedControlID="ddlExistingGuardian" Text="Guardian *" />
                        <asp:DropDownList ID="ddlExistingGuardian" runat="server" CssClass="input" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlExistingGuardian" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a guardian." Text="Please select a guardian." InitialValue="0" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlNewGuardian" runat="server" Visible="false">
                    <div class="form-grid two-col">
                        <div class="field">
                            <asp:Label runat="server" AssociatedControlID="txtGuardianName" Text="Guardian Name *" />
                            <asp:TextBox ID="txtGuardianName" runat="server" CssClass="input" MaxLength="100" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtGuardianName" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Guardian name is required." Text="Guardian name is required." />
                        </div>
                        <div class="field">
                            <asp:Label runat="server" AssociatedControlID="ddlGuardianRelationship" Text="Relationship *" />
                            <asp:DropDownList ID="ddlGuardianRelationship" runat="server" CssClass="input">
                                <asp:ListItem Text="Select Relationship" Value="" />
                                <asp:ListItem Text="Mother" Value="Mother" />
                                <asp:ListItem Text="Father" Value="Father" />
                                <asp:ListItem Text="Guardian" Value="Guardian" />
                                <asp:ListItem Text="Other" Value="Other" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlGuardianRelationship" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Please select a relationship." Text="Please select a relationship." InitialValue="" />
                        </div>
                        <div class="field">
                            <asp:Label runat="server" AssociatedControlID="txtGuardianPhone" Text="Guardian Phone *" />
                            <asp:TextBox ID="txtGuardianPhone" runat="server" CssClass="input" MaxLength="30" />
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtGuardianPhone" CssClass="field-error" Display="Dynamic" ValidationGroup="Save" ErrorMessage="Guardian phone is required." Text="Guardian phone is required." />
                        </div>
                        <div class="field">
                            <asp:Label runat="server" AssociatedControlID="txtGuardianEmail" Text="Guardian Email" />
                            <asp:TextBox ID="txtGuardianEmail" runat="server" CssClass="input" MaxLength="100" TextMode="Email" />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtGuardianEmail" CssClass="field-error" Display="Dynamic" ValidationGroup="Save"
                                ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ErrorMessage="Please enter a valid email address." Text="Please enter a valid email address." />
                        </div>
                    </div>
                    <asp:Panel ID="pnlGuardianDuplicateWarning" runat="server" CssClass="alert alert-warning" Visible="false" Style="background:#FFFBEB;color:#92400E;border:1px solid #FDE68A;">
                        <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
                        <asp:Label ID="lblGuardianDuplicateWarning" runat="server" />
                    </asp:Panel>
                </asp:Panel>
            </div>

            <div class="form-section">
                <h2><i data-lucide="file-text" class="w-4 h-4 text-brand-600"></i> Notes</h2>
                <div class="field">
                    <asp:Label runat="server" AssociatedControlID="txtNotes" Text="Notes" />
                    <asp:TextBox ID="txtNotes" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" MaxLength="500" />
                </div>
            </div>

            </div>

            <div class="form-actions">
                <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnCancel_Click">Cancel</asp:LinkButton>
                <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnReset_Click">
                    <i data-lucide="rotate-ccw" class="w-4 h-4"></i> Reset Form
                </asp:LinkButton>
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" ValidationGroup="Save" OnClick="btnSave_Click">
                    <i data-lucide="check" class="w-4 h-4"></i> Submit Application
                </asp:LinkButton>
            </div>
        </div>
        </asp:Panel>
    </div>
</asp:Content>
