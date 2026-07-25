<%@ Page Title="Parents | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Parents.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Parents.Parents" %>

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
        @media (max-width: 768px) { .students-wrap { padding: .875rem; } }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="students-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Parents</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Parents &amp; Guardians</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage guardian records and their linked students.</p>
            </div>
            <asp:HyperLink ID="lnkAddParent" runat="server" CssClass="btn btn-primary" NavigateUrl="~/Modules/Parents/AddParent.aspx">
                <i data-lucide="user-plus" class="w-4 h-4"></i> Add Parent
            </asp:HyperLink>
        </div>

        <div class="grid grid-cols-2 md:grid-cols-5 gap-4 mb-5">
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="users" class="w-5 h-5"></i></span>
                <div><p class="lbl">Total</p><p class="val"><asp:Label ID="lblTotalCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="check-circle-2" class="w-5 h-5"></i></span>
                <div><p class="lbl">Active</p><p class="val"><asp:Label ID="lblActiveCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#F1F5F9;color:#64748B"><i data-lucide="pause-circle" class="w-5 h-5"></i></span>
                <div><p class="lbl">Inactive</p><p class="val"><asp:Label ID="lblInactiveCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="link" class="w-5 h-5"></i></span>
                <div><p class="lbl">With Students</p><p class="val"><asp:Label ID="lblWithStudentsCount" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#FFFBEB;color:#F59E0B"><i data-lucide="link-2-off" class="w-5 h-5"></i></span>
                <div><p class="lbl">Without Students</p><p class="val"><asp:Label ID="lblWithoutStudentsCount" runat="server" Text="0" /></p></div>
            </div>
        </div>

        <div class="card p-3.5 mb-4 filter-bar">
            <div class="grow">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search by name, phone or email…" />
            </div>
            <asp:DropDownList ID="ddlRelationship" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Relationships" Value="" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Statuses" Value="" />
                <asp:ListItem Text="Active" Value="1" />
                <asp:ListItem Text="Inactive" Value="0" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlHasStudents" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="Has Linked Students" Value="1" />
                <asp:ListItem Text="No Linked Students" Value="0" />
            </asp:DropDownList>
            <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-primary" OnClick="btnSearch_Click">Search</asp:LinkButton>
            <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false">Reset</asp:LinkButton>
        </div>

        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gvParents" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true"
                    CssClass="w-full" DataKeyNames="GuardianID" OnRowCommand="gvParents_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Guardian">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex items-center gap-3">
                                    <span class="avatar" style='<%# "width:32px;height:32px;font-size:12px;background:" + GetAvatarColor(Eval("FullName")) %>'><%# GetInitials(Eval("FullName")) %></span>
                                    <p class="font-semibold"><%# Eval("FullName") %></p>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Relationship" HeaderText="Relationship" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="Phone" HeaderText="Phone" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="Email" HeaderText="Email" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="Occupation" HeaderText="Occupation" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Students">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate><span class="badge" style="background:#EFF6FF;color:#1D4ED8"><%# Eval("StudentCount") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <span class="badge" style='<%# Convert.ToBoolean(Eval("IsActive")) ? "background:#DCFCE7;color:#15803D" : "background:#F1F5F9;color:#64748B" %>'>
                                    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex gap-1">
                                    <asp:HyperLink runat="server" CssClass="btn-ghost btn !p-1.5" ToolTip="View" NavigateUrl='<%# "~/Modules/Parents/ParentDetails.aspx?id=" + Eval("GuardianID") %>'>
                                        <i data-lucide="eye" class="w-4 h-4"></i>
                                    </asp:HyperLink>
                                    <asp:HyperLink runat="server" CssClass="btn-ghost btn !p-1.5" ToolTip="Edit" NavigateUrl='<%# "~/Modules/Parents/EditParent.aspx?id=" + Eval("GuardianID") %>'>
                                        <i data-lucide="pencil" class="w-4 h-4"></i>
                                    </asp:HyperLink>
                                    <asp:LinkButton runat="server" CssClass="btn-ghost btn !p-1.5" ToolTip='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'
                                        CommandName="ToggleActive" CommandArgument='<%# Eval("GuardianID") + "|" + Eval("IsActive") %>'
                                        OnClientClick='<%# "return confirm(\x27" + (Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate") + " this guardian?\x27);" %>'>
                                        <i data-lucide="power" class="w-4 h-4"></i>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="flex flex-col items-center justify-center py-16 text-center">
                            <span class="w-14 h-14 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-300 flex items-center justify-center mb-4">
                                <i data-lucide="users" class="w-7 h-7"></i>
                            </span>
                            <h3 class="font-bold">No guardians found</h3>
                            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1 mb-4 max-w-sm">Try adjusting your search or filters, or add a new parent.</p>
                            <a href="~/Modules/Parents/AddParent.aspx" runat="server" class="btn btn-primary"><i data-lucide="plus" class="w-4 h-4"></i> Add Parent</a>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
            <div class="flex items-center justify-between px-4 py-3 border-t border-gray-100 dark:border-slate-700 flex-wrap gap-2">
                <asp:Label runat="server" ID="lblResultsSummary" Text="Showing 0 of 0" CssClass="text-xs text-gray-500 dark:text-slate-400" />
                <div class="flex items-center gap-2">
                    <span class="text-xs text-gray-500 dark:text-slate-400">Page size:</span>
                    <asp:DropDownList ID="ddlPageSize" runat="server" CssClass="input !w-auto !py-1.5 text-xs" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
                        <asp:ListItem Text="10" Value="10" />
                        <asp:ListItem Text="25" Value="25" />
                        <asp:ListItem Text="50" Value="50" />
                    </asp:DropDownList>
                    <asp:LinkButton ID="btnPrevPage" runat="server" CssClass="btn btn-ghost !p-1.5" OnClick="btnPrevPage_Click"><i data-lucide="chevron-left" class="w-4 h-4"></i></asp:LinkButton>
                    <asp:Label runat="server" ID="lblPageIndicator" Text="Page 1 of 1" CssClass="text-xs" />
                    <asp:LinkButton ID="btnNextPage" runat="server" CssClass="btn btn-ghost !p-1.5" OnClick="btnNextPage_Click"><i data-lucide="chevron-right" class="w-4 h-4"></i></asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
