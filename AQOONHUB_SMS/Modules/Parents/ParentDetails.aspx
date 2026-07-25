<%@ Page Title="Parent Details | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ParentDetails.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Parents.ParentDetails" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .details-wrap { padding: 1.25rem; max-width: 1100px; margin: 0 auto; }
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
            <a href="~/Modules/Parents/Parents.aspx" runat="server" class="hover:text-brand-600">Parents</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Details</span>
        </nav>
        <h1 class="text-xl md:text-2xl font-bold tracking-tight mb-6">Parent Details</h1>

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
            <div class="card p-6 mb-5 profile-card">
                <div class="profile-photo-fallback"><asp:Label ID="lblInitials" runat="server" /></div>
                <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2 flex-wrap">
                        <h2 class="text-lg font-extrabold"><asp:Label ID="lblFullName" runat="server" /></h2>
                        <asp:Label ID="lblStatusBadge" runat="server" CssClass="badge" />
                    </div>
                    <p class="text-xs text-gray-500 dark:text-slate-400 mt-1">
                        <asp:Label ID="lblRelationship" runat="server" /> &middot; <asp:Label ID="lblPhone" runat="server" />
                    </p>
                </div>
                <div class="flex gap-2 flex-wrap">
                    <asp:HyperLink ID="lnkEdit" runat="server" CssClass="btn btn-primary"><i data-lucide="pencil" class="w-4 h-4"></i> Edit</asp:HyperLink>
                    <asp:HyperLink ID="lnkAssignStudent" runat="server" CssClass="btn btn-secondary"><i data-lucide="link" class="w-4 h-4"></i> Assign Student</asp:HyperLink>
                    <asp:HyperLink ID="lnkManageLogin" runat="server" CssClass="btn btn-secondary"><i data-lucide="key-round" class="w-4 h-4"></i> Manage Login</asp:HyperLink>
                    <asp:LinkButton ID="btnToggleActive" runat="server" CssClass="btn btn-secondary" OnClick="btnToggleActive_Click">
                        <i data-lucide="power" class="w-4 h-4"></i> <asp:Label ID="lblToggleActiveText" runat="server" />
                    </asp:LinkButton>
                </div>
            </div>

            <div class="grid lg:grid-cols-2 gap-4 mb-5">
                <div class="card p-6">
                    <h3 class="font-bold mb-3 text-sm">Contact Information</h3>
                    <div class="detail-row"><span class="k">Phone</span><span class="v"><asp:Label ID="lblDetailPhone" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Alternate Phone</span><span class="v"><asp:Label ID="lblDetailAltPhone" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Email</span><span class="v"><asp:Label ID="lblDetailEmail" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Address</span><span class="v"><asp:Label ID="lblDetailAddress" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Emergency Contact</span><span class="v"><asp:Label ID="lblDetailEmergency" runat="server" /></span></div>
                </div>
                <div class="card p-6">
                    <h3 class="font-bold mb-3 text-sm">Other Information</h3>
                    <div class="detail-row"><span class="k">Occupation</span><span class="v"><asp:Label ID="lblDetailOccupation" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">National ID</span><span class="v"><asp:Label ID="lblDetailNationalId" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Login Account</span><span class="v"><asp:Label ID="lblDetailLoginAccount" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Created</span><span class="v"><asp:Label ID="lblDetailCreated" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Last Updated</span><span class="v"><asp:Label ID="lblDetailUpdated" runat="server" /></span></div>
                </div>
            </div>

            <div class="card overflow-hidden mb-5">
                <div class="p-4 border-b border-gray-100 dark:border-slate-700"><h3 class="font-bold text-sm">Linked Students</h3></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvStudents" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full">
                        <Columns>
                            <asp:BoundField DataField="StudentCode" HeaderText="Student Code" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="AdmissionNo" HeaderText="Admission No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="FullName" HeaderText="Name" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="ClassName" HeaderText="Class" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="SectionName" HeaderText="Section" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Status">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate><span class="badge" style='<%# GetStudentStatusStyle(Eval("Status")) %>'><%# Eval("Status") %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate>
                                    <asp:HyperLink runat="server" CssClass="btn btn-secondary !py-1 !px-3 !text-xs" NavigateUrl='<%# "~/Modules/Students/StudentDetails.aspx?id=" + Eval("StudentID") %>'>View</asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="text-center py-8 text-sm text-gray-500 dark:text-slate-400">No linked students.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <div class="card overflow-hidden mb-5">
                <div class="p-4 border-b border-gray-100 dark:border-slate-700"><h3 class="font-bold text-sm">Linked Admission Applications</h3></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvAdmissions" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true" CssClass="w-full">
                        <Columns>
                            <asp:BoundField DataField="ApplicationNo" HeaderText="Application No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="FullName" HeaderText="Applicant" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:BoundField DataField="ClassName" HeaderText="Applied For" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Status">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate><span class="badge" style='<%# GetAdmissionStatusStyle(Eval("Status")) %>'><%# Eval("Status") %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate>
                                    <asp:HyperLink runat="server" CssClass="btn btn-secondary !py-1 !px-3 !text-xs" NavigateUrl='<%# "~/Modules/Admission/AdmissionReview.aspx?id=" + Eval("AdmissionID") %>'>View</asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="text-center py-8 text-sm text-gray-500 dark:text-slate-400">No linked applications.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <a href="~/Modules/Parents/Parents.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Parents</a>
        </asp:Panel>
    </div>
</asp:Content>
