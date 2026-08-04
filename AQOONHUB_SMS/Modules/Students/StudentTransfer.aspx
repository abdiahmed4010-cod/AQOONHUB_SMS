<%@ Page Title="Student Transfer | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="StudentTransfer.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Students.StudentTransfer" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .transfer-wrap { padding: 1.25rem; max-width: 1100px; margin: 0 auto; }
        .profile-card { display:flex; flex-wrap:wrap; align-items:center; gap:1.25rem; }
        .profile-photo-fallback { width:64px; height:64px; border-radius:.9rem; background:#7C3AED; color:#fff; display:flex; align-items:center; justify-content:center; font-weight:800; font-size:1.2rem; flex-shrink:0; }
        .form-section { margin-bottom: 1.25rem; }
        .form-section h2 { font-size:.9rem; font-weight:800; margin:0 0 .8rem; display:flex; align-items:center; gap:.5rem; }
        .form-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .form-grid.two-col { grid-template-columns:repeat(2,1fr); } }
        .field label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .field label { color:#CBD5E1; }
        .field-error { font-size:.72rem; color:#EF4444; margin-top:.3rem; display:block; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        .alert-info { background:#EFF6FF; color:#1D4ED8; border:1px solid #DBEAFE; }
        .form-actions { display:flex; gap:.6rem; flex-wrap:wrap; justify-content:flex-end; padding-top:1rem; border-top:1px solid #E5E7EB; margin-top:.5rem; }
        .dark .form-actions { border-color:#334155; }
        .detail-row { display:flex; justify-content:space-between; gap:1rem; padding:.5rem 0; border-bottom:1px solid #F1F5F9; font-size:.8rem; }
        .dark .detail-row { border-color:#263449; }
        .detail-row .k { color:#6B7280; font-weight:600; }
        .dark .detail-row .k { color:#94A3B8; }
        .detail-row .v { font-weight:700; text-align:right; }
        @media (max-width:768px){ .transfer-wrap{padding:.875rem;} }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="transfer-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span>
            <a href="~/Modules/Students/Students.aspx" runat="server" class="hover:text-brand-600">Student Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Transfer</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">
            <asp:Label ID="lblPageTitle" runat="server" Text="Student Transfer" />
        </h1>

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

            <!-- Student summary -->
            <div class="card p-6 mb-5 profile-card">
                <asp:Image ID="imgPhoto" runat="server" CssClass="profile-photo-fallback" Style="object-fit:cover;" />
                <asp:Panel ID="pnlPhotoFallback" runat="server" CssClass="profile-photo-fallback">
                    <asp:Label ID="lblInitials" runat="server" />
                </asp:Panel>
                <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2 flex-wrap">
                        <h2 class="text-lg font-extrabold"><asp:Label ID="lblFullName" runat="server" /></h2>
                        <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" />
                    </div>
                    <p class="text-xs text-gray-500 dark:text-slate-400 mt-1">
                        <asp:Label ID="lblStudentCode" runat="server" /> &middot; <asp:Label ID="lblAdmissionNo" runat="server" /> &middot;
                        <asp:Label ID="lblGender" runat="server" /> &middot; <asp:Label ID="lblClassSection" runat="server" />
                    </p>
                    <p class="text-xs text-gray-500 dark:text-slate-400 mt-1">
                        Guardian: <asp:Label ID="lblGuardian" runat="server" />
                    </p>
                </div>
                <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-secondary" NavigateUrl="~/Modules/Students/Students.aspx">
                    <i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Students
                </asp:HyperLink>
            </div>

            <!-- CASE 2: active transfer summary + Return form -->
            <asp:Panel ID="pnlActiveTransferInfo" runat="server" Visible="false">
                <div class="card p-6 mb-5">
                    <h3 class="font-bold mb-3 text-sm">Current Transfer</h3>
                    <div class="detail-row"><span class="k">Destination School</span><span class="v"><asp:Label ID="lblCurDestSchool" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Destination Location</span><span class="v"><asp:Label ID="lblCurDestLocation" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Transfer Date</span><span class="v"><asp:Label ID="lblCurTransferDate" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Reason</span><span class="v"><asp:Label ID="lblCurReason" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Certificate No.</span><span class="v"><asp:Label ID="lblCurCertNo" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Notes</span><span class="v"><asp:Label ID="lblCurNotes" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Processed By</span><span class="v"><asp:Label ID="lblCurProcessedBy" runat="server" /></span></div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlReturnForm" runat="server" Visible="false">
                <div class="card p-6 mb-5">
                    <div class="form-section">
                        <h2><i data-lucide="undo-2" class="w-4 h-4 text-brand-600"></i> Return to School</h2>
                        <div class="form-grid two-col">
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtReturnDate" Text="Return Date *" />
                                <asp:TextBox ID="txtReturnDate" runat="server" CssClass="input" TextMode="Date" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtReturnDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Return" ErrorMessage="Return date is required." Text="Return date is required." />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="ddlReturnAcademicYear" Text="Academic Year *" />
                                <asp:DropDownList ID="ddlReturnAcademicYear" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlReturnAcademicYear" CssClass="field-error" Display="Dynamic" ValidationGroup="Return" ErrorMessage="Please select an academic year." Text="Please select an academic year." InitialValue="0" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="ddlReturnClass" Text="Class *" />
                                <asp:DropDownList ID="ddlReturnClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlReturnClass_SelectedIndexChanged" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlReturnClass" CssClass="field-error" Display="Dynamic" ValidationGroup="Return" ErrorMessage="Please select a class." Text="Please select a class." InitialValue="0" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="ddlReturnShift" Text="Shift *" />
                                <asp:DropDownList ID="ddlReturnShift" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlReturnShift_Changed">
                                    <asp:ListItem Text="Select Shift" Value="" />
                                    <asp:ListItem Text="Morning" Value="Morning" />
                                    <asp:ListItem Text="Afternoon" Value="Afternoon" />
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlReturnShift" CssClass="field-error" Display="Dynamic" ValidationGroup="Return" ErrorMessage="Please select a shift." Text="Please select a shift." InitialValue="" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="ddlReturnSection" Text="Section *" />
                                <asp:DropDownList ID="ddlReturnSection" runat="server" CssClass="input" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlReturnSection" CssClass="field-error" Display="Dynamic" ValidationGroup="Return" ErrorMessage="Please select a section." Text="Please select a section." InitialValue="0" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtReturnReason" Text="Return Reason *" />
                                <asp:TextBox ID="txtReturnReason" runat="server" CssClass="input" MaxLength="300" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtReturnReason" CssClass="field-error" Display="Dynamic" ValidationGroup="Return" ErrorMessage="Return reason is required." Text="Return reason is required." />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtReturnNotes" Text="Return Notes" />
                                <asp:TextBox ID="txtReturnNotes" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" MaxLength="500" />
                            </div>
                        </div>
                    </div>
                    <div class="form-actions">
                        <asp:LinkButton ID="btnReturn" runat="server" CssClass="btn btn-primary" ValidationGroup="Return" OnClick="btnReturn_Click"
                            OnClientClick="return confirm('Return this student to school as Active?');">
                            <i data-lucide="check" class="w-4 h-4"></i> Return to School
                        </asp:LinkButton>
                    </div>
                </div>
            </asp:Panel>

            <!-- CASE 1: Transfer form -->
            <asp:Panel ID="pnlTransferForm" runat="server" Visible="false">
                <div class="card p-6 mb-5">
                    <div class="form-section">
                        <h2><i data-lucide="arrow-right-left" class="w-4 h-4 text-brand-600"></i> Transfer Student</h2>
                        <div class="form-grid two-col">
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="ddlTransferType" Text="Transfer Type" />
                                <asp:DropDownList ID="ddlTransferType" runat="server" CssClass="input">
                                    <asp:ListItem Text="External Transfer" Value="External Transfer" Selected="True" />
                                </asp:DropDownList>
                            </div>
                            <div class="field"></div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtDestSchool" Text="Destination School *" />
                                <asp:TextBox ID="txtDestSchool" runat="server" CssClass="input" MaxLength="150" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDestSchool" CssClass="field-error" Display="Dynamic" ValidationGroup="Transfer" ErrorMessage="Destination school is required." Text="Destination school is required." />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtDestLocation" Text="Destination Location *" />
                                <asp:TextBox ID="txtDestLocation" runat="server" CssClass="input" MaxLength="200" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDestLocation" CssClass="field-error" Display="Dynamic" ValidationGroup="Transfer" ErrorMessage="Destination location is required." Text="Destination location is required." />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtDestContact" Text="Contact Person" />
                                <asp:TextBox ID="txtDestContact" runat="server" CssClass="input" MaxLength="100" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtDestPhone" Text="Destination Phone" />
                                <asp:TextBox ID="txtDestPhone" runat="server" CssClass="input" MaxLength="30" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtTransferDate" Text="Transfer Date *" />
                                <asp:TextBox ID="txtTransferDate" runat="server" CssClass="input" TextMode="Date" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTransferDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Transfer" ErrorMessage="Transfer date is required." Text="Transfer date is required." />
                                <asp:CustomValidator ID="cvTransferDate" runat="server" ControlToValidate="txtTransferDate" CssClass="field-error" Display="Dynamic" ValidationGroup="Transfer" OnServerValidate="cvTransferDate_ServerValidate" ErrorMessage="Transfer date cannot be far in the future." Text="Transfer date cannot be far in the future." />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtCertNo" Text="Transfer Certificate Number" />
                                <asp:TextBox ID="txtCertNo" runat="server" CssClass="input" MaxLength="50" />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtTransferReason" Text="Transfer Reason *" />
                                <asp:TextBox ID="txtTransferReason" runat="server" CssClass="input" MaxLength="300" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTransferReason" CssClass="field-error" Display="Dynamic" ValidationGroup="Transfer" ErrorMessage="Transfer reason is required." Text="Transfer reason is required." />
                            </div>
                            <div class="field">
                                <asp:Label runat="server" AssociatedControlID="txtTransferNotes" Text="Notes" />
                                <asp:TextBox ID="txtTransferNotes" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" MaxLength="500" />
                            </div>
                        </div>
                    </div>
                    <div class="form-actions">
                        <asp:LinkButton ID="btnTransfer" runat="server" CssClass="btn btn-primary !bg-red-500 hover:!bg-red-600" ValidationGroup="Transfer" OnClick="btnTransfer_Click"
                            OnClientClick="return confirm('Transfer this student out of the school? Status will change to Transferred.');">
                            <i data-lucide="arrow-right-left" class="w-4 h-4"></i> Confirm Transfer
                        </asp:LinkButton>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlNoPermission" runat="server" CssClass="alert alert-info" Visible="false">
                <i data-lucide="info" class="w-4 h-4 mt-0.5"></i>
                You have read-only access to transfer history for this student.
            </asp:Panel>

            <!-- Transfer history -->
            <div class="card overflow-hidden">
                <div class="p-4 border-b border-gray-100 dark:border-slate-700">
                    <h3 class="font-bold text-sm">Transfer History</h3>
                </div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full">
                        <Columns>
                            <asp:BoundField DataField="TransferType" HeaderText="Type" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="DestinationSchool" HeaderText="Destination" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="TransferDate" HeaderText="Transfer Date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="TransferReason" HeaderText="Reason" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="TransferCertificateNo" HeaderText="Cert. No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Status">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate><span class="badge" style='<%# GetHistoryStatusStyle(Eval("TransferStatus")) %>'><%# Eval("TransferStatus") %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ReturnedDate" HeaderText="Returned Date" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="CreatedAt" HeaderText="Created" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="text-center py-8 text-sm text-gray-500 dark:text-slate-400">No transfer history for this student.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

        </asp:Panel>
    </div>
</asp:Content>
