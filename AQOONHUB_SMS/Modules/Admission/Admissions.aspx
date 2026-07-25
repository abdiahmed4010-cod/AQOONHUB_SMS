<%@ Page Title="Admissions | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Admissions.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Admission.Admissions" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .students-wrap { padding: 1.25rem; max-width: 1440px; margin: 0 auto; }
        .stat-tile { display: flex; align-items: center; gap: .875rem; }
        .stat-tile .ic { width: 2.5rem; height: 2.5rem; border-radius: .6rem; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
        .stat-tile .lbl { font-size:.7rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; color:#6B7280; }
        .dark .stat-tile .lbl { color:#94A3B8; }
        .stat-tile .val { font-size:1.3rem; font-weight:800; line-height:1.15; }
        .filter-bar { display:flex; flex-wrap:wrap; align-items:center; gap:.625rem; }
        .filter-bar .grow { flex:1; min-width:200px; position:relative; }
        .filter-bar .grow svg { position:absolute; left:.75rem; top:50%; transform:translateY(-50%); color:#9CA3AF; width:1rem; height:1rem; }
        .filter-bar .grow input { padding-left:2.25rem; }
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }
        @media (max-width: 768px) { .students-wrap { padding: .875rem; } }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="students-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Admissions</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Admissions</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Review applications and approve them into enrolled students.</p>
            </div>
            <asp:HyperLink ID="lnkAddAdmission" runat="server" CssClass="btn btn-primary" NavigateUrl="~/Modules/Admission/AddAdmission.aspx">
                <i data-lucide="clipboard-list" class="w-4 h-4"></i> New Application
            </asp:HyperLink>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
            <i data-lucide="check-circle-2" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblSuccess" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <i data-lucide="alert-triangle" class="w-4 h-4 mt-0.5"></i>
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>

        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-5">
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#FFFBEB;color:#F59E0B"><i data-lucide="clock" class="w-5 h-5"></i></span>
                <div><p class="lbl">Pending</p><p class="val"><asp:Label ID="lblPendingCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#EFF6FF;color:#0EA5E9"><i data-lucide="eye" class="w-5 h-5"></i></span>
                <div><p class="lbl">Under Review</p><p class="val"><asp:Label ID="lblUnderReviewCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="check-circle-2" class="w-5 h-5"></i></span>
                <div><p class="lbl">Enrolled</p><p class="val"><asp:Label ID="lblApprovedCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#FEF2F2;color:#EF4444"><i data-lucide="x-circle" class="w-5 h-5"></i></span>
                <div><p class="lbl">Rejected</p><p class="val"><asp:Label ID="lblRejectedCount" runat="server" Text="0" /></p></div>
            </div>
        </div>

        <div class="card p-3.5 mb-4 filter-bar">
            <div class="grow">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search by name or application number…" />
            </div>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Statuses" Value="" />
                <asp:ListItem Text="Pending" Value="Pending" />
                <asp:ListItem Text="Under Review" Value="Under Review" />
                <asp:ListItem Text="Enrolled" Value="Enrolled" />
                <asp:ListItem Text="Rejected" Value="Rejected" />
            </asp:DropDownList>
            <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-primary" OnClick="btnSearch_Click">Search</asp:LinkButton>
            <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false">Reset</asp:LinkButton>
        </div>

        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gvAdmissions" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true"
                    CssClass="w-full" DataKeyNames="AdmissionID">
                    <Columns>
                        <asp:TemplateField HeaderText="Applicant">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex items-center gap-3">
                                    <span class="avatar" style='<%# "width:32px;height:32px;font-size:12px;background:" + GetAvatarColor(Eval("FullName")) %>'><%# GetInitials(Eval("FullName")) %></span>
                                    <div>
                                        <p class="font-semibold"><%# Eval("FullName") %></p>
                                        <p class="text-[11px] text-gray-400"><%# Eval("ApplicationNo") %></p>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Gender" HeaderText="Gender" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="DateOfBirth" HeaderText="Date of Birth" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="ClassName" HeaderText="Applying For" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="GuardianName" HeaderText="Guardian" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="GuardianPhone" HeaderText="Guardian Phone" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="ApplicationDate" HeaderText="Applied" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Status">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate><span class="badge" style='<%# GetStatusStyle(Eval("Status")) %>'><%# Eval("Status") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <asp:HyperLink runat="server" CssClass="btn btn-secondary !py-1 !px-3 !text-xs"
                                    NavigateUrl='<%# "~/Modules/Admission/AdmissionReview.aspx?id=" + Eval("AdmissionID") %>'>
                                    Review
                                </asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="flex flex-col items-center justify-center py-16 text-center">
                            <span class="w-14 h-14 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-300 flex items-center justify-center mb-4">
                                <i data-lucide="clipboard-list" class="w-7 h-7"></i>
                            </span>
                            <h3 class="font-bold">No applications found</h3>
                            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1 mb-4 max-w-sm">Try adjusting your search or filters, or start a new application.</p>
                            <a href="~/Modules/Admission/AddAdmission.aspx" runat="server" class="btn btn-primary"><i data-lucide="plus" class="w-4 h-4"></i> New Application</a>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
