<%@ Page Title="Admissions | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Admissions.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Admission.Admissions" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .adm-wrap { padding: 1.25rem; max-width: 1500px; margin: 0 auto; }

        /* KPI tiles */
        .kpi-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:640px){ .kpi-grid { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .kpi-grid { grid-template-columns:repeat(5,1fr); } }
        .kpi { padding:1.15rem 1.15rem 1rem; }
        .kpi .ic { width:2.75rem; height:2.75rem; border-radius:.8rem; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
        .kpi .lbl { font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; color:#6B7280; }
        .dark .kpi .lbl { color:#94A3B8; }
        .kpi .val { font-size:1.7rem; font-weight:800; line-height:1.1; letter-spacing:-.02em; }
        .kpi .ctx { font-size:.7rem; color:#9CA3AF; margin-top:.15rem; }
        .dark .kpi .ctx { color:#64748B; }

        /* Layout */
        .adm-cols { display:grid; grid-template-columns:1fr; gap:1.25rem; }
        @media (min-width:1200px){ .adm-cols { grid-template-columns:minmax(0,1.9fr) minmax(0,1fr); align-items:start; } }
        .adm-secondary { display:grid; grid-template-columns:1fr; gap:1.25rem; }
        @media (min-width:900px){ .adm-secondary { grid-template-columns:1fr 1fr; } }

        /* Filter bar */
        .filter-bar { display:flex; flex-wrap:wrap; align-items:center; gap:.6rem; }
        .filter-bar .grow { flex:1; min-width:180px; position:relative; }
        .filter-bar .grow svg { position:absolute; left:.75rem; top:50%; transform:translateY(-50%); color:#9CA3AF; width:1rem; height:1rem; }
        .filter-bar .grow input { padding-left:2.25rem; }

        /* Card section headers */
        .card-head { display:flex; align-items:center; justify-content:space-between; gap:.75rem; padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .dark .card-head { border-color:#334155; }
        .card-head h2 { font-size:.95rem; font-weight:800; letter-spacing:-.01em; }
        .card-head .sub { font-size:.72rem; color:#6B7280; margin-top:.1rem; }
        .dark .card-head .sub { color:#94A3B8; }

        /* Alerts */
        .alert { border-radius:.7rem; padding:.85rem 1rem; font-size:.82rem; display:flex; gap:.6rem; align-items:flex-start; margin-bottom:1rem; }
        .alert-success { background:#ECFDF5; color:#166534; border:1px solid #BBF7D0; }
        .alert-danger { background:#FEF2F2; color:#991B1B; border:1px solid #FECACA; }

        /* Action icon buttons */
        .act-ic { display:inline-flex; align-items:center; justify-content:center; width:2rem; height:2rem; border-radius:.55rem; transition:all .15s; }
        .act-ic.view { color:#0EA5E9; background:#F0F9FF; } .act-ic.view:hover { background:#E0F2FE; }
        .act-ic.edit { color:#F59E0B; background:#FFFBEB; } .act-ic.edit:hover { background:#FEF3C7; }
        .act-ic.del  { color:#EF4444; background:#FEF2F2; } .act-ic.del:hover  { background:#FEE2E2; }
        .dark .act-ic.view { background:#0C2A3A; } .dark .act-ic.edit { background:#2A2410; } .dark .act-ic.del { background:#2A1414; }

        /* Inline form */
        .inline-field label { display:block; font-size:.72rem; font-weight:700; margin-bottom:.3rem; color:#374151; }
        .dark .inline-field label { color:#CBD5E1; }
        .inline-field .req { color:#EF4444; }
        .inline-grid { display:grid; grid-template-columns:1fr 1fr; gap:.85rem; }
        .inline-grid .full { grid-column:1 / -1; }
        .field-error { font-size:.7rem; color:#EF4444; margin-top:.25rem; display:block; }

        /* Donut */
        .donut { width:9rem; height:9rem; border-radius:50%; position:relative; flex-shrink:0; }
        .donut::after { content:''; position:absolute; inset:1.6rem; background:#fff; border-radius:50%; }
        .dark .donut::after { background:#1E293B; }
        .donut .center { position:absolute; inset:0; display:flex; flex-direction:column; align-items:center; justify-content:center; z-index:1; }
        .donut .center .t { font-size:1.45rem; font-weight:800; line-height:1; }
        .donut .center .s { font-size:.6rem; font-weight:700; text-transform:uppercase; letter-spacing:.05em; color:#9CA3AF; margin-top:.2rem; }
        .legend-row { display:flex; align-items:center; gap:.55rem; font-size:.78rem; padding:.28rem 0; }
        .legend-row .dot { width:.7rem; height:.7rem; border-radius:3px; flex-shrink:0; }
        .legend-row .lname { color:#4B5563; }
        .dark .legend-row .lname { color:#CBD5E1; }
        .legend-row .lval { margin-left:auto; font-weight:700; }

        /* Recent admissions */
        .recent-item { display:flex; align-items:center; gap:.75rem; padding:.7rem 0; border-bottom:1px solid #F1F5F9; }
        .dark .recent-item { border-color:#263449; }
        .recent-item:last-child { border-bottom:none; }

        /* Quick actions */
        .qa-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:.625rem; }
        @media (min-width:1200px){ .qa-grid { grid-template-columns:repeat(3,1fr); } }
        .qa-btn { display:flex; flex-direction:column; align-items:flex-start; gap:.5rem; padding:.85rem; border-radius:.7rem; border:1px solid #E5E7EB; background:#fff; font-size:.75rem; font-weight:700; color:#374151; transition:all .15s; text-decoration:none; }
        .qa-btn:hover { border-color:#2563EB; background:#F8FAFC; transform:translateY(-1px); }
        .dark .qa-btn { background:#0F172A; border-color:#334155; color:#E2E8F0; }
        .dark .qa-btn:hover { background:#273549; border-color:#2563EB; }
        .qa-btn .qic { width:2rem; height:2rem; border-radius:.55rem; display:flex; align-items:center; justify-content:center; }

        /* Pager */
        .adm-pager table { margin:0 auto; }
        .adm-pager td { padding:0 .15rem; }
        .adm-pager a, .adm-pager span { display:inline-flex; align-items:center; justify-content:center; min-width:2rem; height:2rem; padding:0 .5rem; border-radius:.5rem; font-size:.8rem; font-weight:600; border:1px solid #E5E7EB; color:#374151; text-decoration:none; }
        .dark .adm-pager a, .dark .adm-pager span { border-color:#334155; color:#CBD5E1; }
        .adm-pager a:hover { background:#F1F5F9; }
        .adm-pager span { background:#2563EB; color:#fff; border-color:#2563EB; }

        /* Process flow */
        .flow { display:flex; gap:.5rem; overflow-x:auto; padding:.25rem; }
        .flow .step { flex:1; min-width:120px; display:flex; flex-direction:column; align-items:center; text-align:center; gap:.5rem; padding:.5rem; }
        .flow .step .sic { width:2.6rem; height:2.6rem; border-radius:.8rem; display:flex; align-items:center; justify-content:center; background:#EFF6FF; color:#2563EB; }
        .dark .flow .step .sic { background:#1E293B; color:#93C5FD; }
        .flow .step .stt { font-size:.75rem; font-weight:700; }
        .flow .step .std { font-size:.66rem; color:#9CA3AF; line-height:1.25; }
        .flow .arrow { display:flex; align-items:center; color:#CBD5E1; flex-shrink:0; }
        .dark .flow .arrow { color:#475569; }
        @media (max-width:768px) { .adm-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="adm-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Admissions</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Admissions</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage student admissions and applications.</p>
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

        <!-- ===== KPI SUMMARY CARDS ===== -->
        <div class="kpi-grid mb-5">
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Total Applications</p><p class="val"><asp:Label ID="lblTotalCount" runat="server" Text="0" /></p></div>
                    <span class="ic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="files" class="w-5 h-5"></i></span>
                </div>
                <p class="ctx">All time</p>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">New Applications</p><p class="val"><asp:Label ID="lblNewCount" runat="server" Text="0" /></p></div>
                    <span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="user-plus" class="w-5 h-5"></i></span>
                </div>
                <p class="ctx">This month</p>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Under Review</p><p class="val"><asp:Label ID="lblUnderReviewCount" runat="server" Text="0" CssClass="src-review" /></p></div>
                    <span class="ic" style="background:#FFFBEB;color:#F59E0B"><i data-lucide="clock" class="w-5 h-5"></i></span>
                </div>
                <p class="ctx">In progress</p>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Admitted</p><p class="val"><asp:Label ID="lblApprovedCount" runat="server" Text="0" CssClass="src-admitted" /></p></div>
                    <span class="ic" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="check-circle-2" class="w-5 h-5"></i></span>
                </div>
                <p class="ctx">Enrolled students</p>
            </div>
            <div class="card kpi">
                <div class="flex items-start justify-between">
                    <div><p class="lbl">Rejected</p><p class="val"><asp:Label ID="lblRejectedCount" runat="server" Text="0" CssClass="src-rejected" /></p></div>
                    <span class="ic" style="background:#FEF2F2;color:#EF4444"><i data-lucide="x-circle" class="w-5 h-5"></i></span>
                </div>
                <p class="ctx">Not accepted</p>
            </div>
        </div>

        <!-- hidden counts for donut -->
        <asp:Label ID="lblPendingCount" runat="server" Text="0" CssClass="src-new" style="display:none" />
        <asp:Label ID="lblExpiredCount" runat="server" Text="0" CssClass="src-expired" style="display:none" />

        <!-- ===== APPLICATIONS LIST ===== -->
        <div class="mb-5">

            <!-- Applications List -->
            <div class="card overflow-hidden">
                <div class="card-head">
                    <div>
                        <h2>Applications List</h2>
                        <p class="sub">View all applications with status and quick actions.</p>
                    </div>
                </div>

                <div class="p-3.5 filter-bar border-b border-gray-100 dark:border-slate-700">
                    <div class="grow">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search by name, application no…" />
                    </div>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input !w-auto">
                        <asp:ListItem Text="All Status" Value="" />
                        <asp:ListItem Text="Pending" Value="Pending" />
                        <asp:ListItem Text="Under Review" Value="Under Review" />
                        <asp:ListItem Text="Enrolled" Value="Enrolled" />
                        <asp:ListItem Text="Rejected" Value="Rejected" />
                    </asp:DropDownList>
                    <asp:DropDownList ID="ddlClassFilter" runat="server" CssClass="input !w-auto" />
                    <asp:TextBox ID="txtFromDate" runat="server" CssClass="input !w-auto" TextMode="Date" ToolTip="From date" />
                    <asp:TextBox ID="txtToDate" runat="server" CssClass="input !w-auto" TextMode="Date" ToolTip="To date" />
                    <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-primary" OnClick="btnSearch_Click">Filter</asp:LinkButton>
                    <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false">Reset</asp:LinkButton>
                </div>

                <div class="overflow-x-auto">
                    <asp:GridView ID="gvAdmissions" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true"
                        CssClass="w-full" DataKeyNames="AdmissionID" AllowPaging="true" PageSize="10"
                        OnPageIndexChanging="gvAdmissions_PageIndexChanging" OnRowCommand="gvAdmissions_RowCommand">
                        <PagerStyle CssClass="adm-pager" HorizontalAlign="Center" />
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="«" LastPageText="»" PageButtonCount="5" />
                        <Columns>
                            <asp:TemplateField HeaderText="#">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ApplicationNo" HeaderText="Application No." HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Student Name">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate>
                                    <div class="flex items-center gap-3">
                                        <span class="avatar" style='<%# "width:32px;height:32px;font-size:12px;background:" + GetAvatarColor(Eval("FullName")) %>'><%# GetInitials(Eval("FullName")) %></span>
                                        <div>
                                            <p class="font-semibold"><%# Eval("FullName") %></p>
                                            <p class="text-[11px] text-gray-400"><%# Eval("GuardianName") %></p>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ClassName" HeaderText="Class Applied" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Shift">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate><span class="badge" style='<%# GetShiftStyle(Eval("Shift")) %>'><%# Eval("Shift") == null || Eval("Shift").ToString() == "" ? "—" : Eval("Shift") %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="ApplicationDate" HeaderText="Application Date" DataFormatString="{0:dd MMM yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                            <asp:TemplateField HeaderText="Status">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate><span class="badge" style='<%# GetStatusStyle(Eval("Status")) %>'><%# Eval("Status") %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Action">
                                <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                                <ItemTemplate>
                                    <div class="flex items-center gap-1.5">
                                        <asp:HyperLink runat="server" CssClass="act-ic view" ToolTip="View / Review"
                                            NavigateUrl='<%# "~/Modules/Admission/AdmissionReview.aspx?id=" + Eval("AdmissionID") %>'>
                                            <i data-lucide="eye" class="w-4 h-4"></i>
                                        </asp:HyperLink>
                                        <asp:HyperLink ID="lnkEdit" runat="server" CssClass="act-ic edit" ToolTip="Edit" Visible='<%# CanManage %>'
                                            NavigateUrl='<%# "~/Modules/Admission/AddAdmission.aspx?id=" + Eval("AdmissionID") %>'>
                                            <i data-lucide="pencil" class="w-4 h-4"></i>
                                        </asp:HyperLink>
                                        <asp:LinkButton ID="lnkDelete" runat="server" CssClass="act-ic del" ToolTip="Delete" Visible='<%# CanManage %>'
                                            CommandName="DeleteRow" CommandArgument='<%# Eval("AdmissionID") %>' CausesValidation="false"
                                            OnClientClick="return confirm('Delete this application? This cannot be undone.');">
                                            <i data-lucide="trash-2" class="w-4 h-4"></i>
                                        </asp:LinkButton>
                                    </div>
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
                <div class="px-4 py-3 text-xs text-gray-500 dark:text-slate-400 border-t border-gray-100 dark:border-slate-700">
                    <asp:Label ID="lblResultInfo" runat="server" />
                </div>
            </div>
        </div>

        <!-- ===== SECONDARY ROW: Donut + Recent Admissions ===== -->
        <div class="adm-secondary mb-5">

            <!-- Status Overview donut -->
            <div class="card overflow-hidden">
                <div class="card-head">
                    <div><h2>Application Status Overview</h2><p class="sub">Visual breakdown of applications by status.</p></div>
                    <span class="w-8 h-8 rounded-lg flex items-center justify-center" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="pie-chart" class="w-4 h-4"></i></span>
                </div>
                <div class="p-5 flex flex-col sm:flex-row items-center gap-6">
                    <div class="donut" id="admDonut">
                        <div class="center"><span class="t" id="donutTotal">0</span><span class="s">Total</span></div>
                    </div>
                    <div class="flex-1 w-full">
                        <div class="legend-row"><span class="dot" style="background:#0EA5E9"></span><span class="lname">New</span><span class="lval" id="leg-new">0</span></div>
                        <div class="legend-row"><span class="dot" style="background:#F59E0B"></span><span class="lname">Under Review</span><span class="lval" id="leg-review">0</span></div>
                        <div class="legend-row"><span class="dot" style="background:#22C55E"></span><span class="lname">Admitted</span><span class="lval" id="leg-admitted">0</span></div>
                        <div class="legend-row"><span class="dot" style="background:#EF4444"></span><span class="lname">Rejected</span><span class="lval" id="leg-rejected">0</span></div>
                        <div class="legend-row"><span class="dot" style="background:#94A3B8"></span><span class="lname">Expired</span><span class="lval" id="leg-expired">0</span></div>
                    </div>
                </div>
            </div>

            <!-- Recent Admissions -->
            <div class="card overflow-hidden">
                <div class="card-head">
                    <div><h2>Recent Admissions</h2><p class="sub">Recently admitted students for quick reference.</p></div>
                    <span class="w-8 h-8 rounded-lg flex items-center justify-center" style="background:#ECFDF5;color:#22C55E"><i data-lucide="user-check" class="w-4 h-4"></i></span>
                </div>
                <div class="p-5">
                    <asp:Repeater ID="rptRecent" runat="server">
                        <ItemTemplate>
                            <div class="recent-item">
                                <span class="avatar" style='<%# "width:36px;height:36px;font-size:13px;background:" + GetAvatarColor(Eval("FullName")) %>'><%# GetInitials(Eval("FullName")) %></span>
                                <div class="min-w-0 flex-1">
                                    <p class="font-semibold text-sm truncate"><%# Eval("FullName") %></p>
                                    <p class="text-[11px] text-gray-400"><%# Eval("ClassName") %> · <%# Eval("ApplicationNo") %></p>
                                </div>
                                <div class="text-right">
                                    <span class="badge" style="background:#DCFCE7;color:#15803D">Admitted</span>
                                    <p class="text-[10px] text-gray-400 mt-1"><%# Eval("EnrolledAt", "{0:dd MMM yyyy}") %></p>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Panel ID="pnlNoRecent" runat="server" Visible="false" CssClass="text-center text-sm text-gray-500 dark:text-slate-400 py-6">
                        No admitted students yet.
                    </asp:Panel>
                </div>
            </div>
        </div>

        <!-- ===== QUICK ACTIONS ===== -->
        <div class="card overflow-hidden mb-5">
            <div class="card-head">
                <div><h2>Quick Actions</h2><p class="sub">Common admission tasks for faster workflow enrollment.</p></div>
            </div>
            <div class="p-4 qa-grid">
                <a href="~/Modules/Admission/AddAdmission.aspx" runat="server" class="qa-btn">
                    <span class="qic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="user-plus" class="w-4 h-4"></i></span>Add New Application
                </a>
                <a href="~/Modules/Reports/Import.aspx" runat="server" class="qa-btn">
                    <span class="qic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="upload" class="w-4 h-4"></i></span>Bulk Import
                </a>
                <a href="~/Modules/Settings/Settings.aspx" runat="server" class="qa-btn">
                    <span class="qic" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="settings" class="w-4 h-4"></i></span>Application Settings
                </a>
                <a href="~/Modules/Reports/Print.aspx" runat="server" class="qa-btn">
                    <span class="qic" style="background:#FFFBEB;color:#F59E0B"><i data-lucide="printer" class="w-4 h-4"></i></span>Print Forms
                </a>
                <a href="~/Modules/Reports/Export.aspx" runat="server" class="qa-btn">
                    <span class="qic" style="background:#F0F9FF;color:#0EA5E9"><i data-lucide="download" class="w-4 h-4"></i></span>Export Report
                </a>
                <a href="~/Modules/Reports/Import.aspx" runat="server" class="qa-btn">
                    <span class="qic" style="background:#FEF2F2;color:#EF4444"><i data-lucide="file-up" class="w-4 h-4"></i></span>Upload Documents
                </a>
            </div>
        </div>

        <!-- ===== ADMISSION PROCESS FLOW ===== -->
        <div class="card overflow-hidden">
            <div class="card-head">
                <div><h2>Admission Process Flow</h2><p class="sub">Step-by-step admission process from application to enrollment.</p></div>
            </div>
            <div class="p-4">
                <div class="flow">
                    <div class="step"><span class="sic"><i data-lucide="file-plus" class="w-5 h-5"></i></span><span class="stt">1. Application</span><span class="std">Student/Guardian submits application</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="eye" class="w-5 h-5"></i></span><span class="stt">2. Review</span><span class="std">Admin reviews application</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="clipboard-check" class="w-5 h-5"></i></span><span class="stt">3. Assessment</span><span class="std">Evaluate based on criteria</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="gavel" class="w-5 h-5"></i></span><span class="stt">4. Decision</span><span class="std">Accept or reject application</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="badge-check" class="w-5 h-5"></i></span><span class="stt">5. Admission</span><span class="std">Generate admission number</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="graduation-cap" class="w-5 h-5"></i></span><span class="stt">6. Enrollment</span><span class="std">Student enrolled in class</span></div>
                    <div class="arrow"><i data-lucide="chevron-right" class="w-5 h-5"></i></div>
                    <div class="step"><span class="sic"><i data-lucide="database" class="w-5 h-5"></i></span><span class="stt">7. Records</span><span class="std">Add to student database</span></div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>

<asp:Content ID="ContentScripts" ContentPlaceHolderID="scripts" runat="server">
    <script>
        (function () {
            function num(sel) {
                var el = document.querySelector(sel);
                if (!el) return 0;
                var n = parseInt((el.textContent || '0').replace(/[^0-9]/g, ''), 10);
                return isNaN(n) ? 0 : n;
            }
            function setText(id, v) { var el = document.getElementById(id); if (el) el.textContent = v; }
            function renderDonut() {
                var vals = [num('.src-new'), num('.src-review'), num('.src-admitted'), num('.src-rejected'), num('.src-expired')];
                var colors = ['#0EA5E9', '#F59E0B', '#22C55E', '#EF4444', '#94A3B8'];
                setText('leg-new', vals[0]); setText('leg-review', vals[1]); setText('leg-admitted', vals[2]);
                setText('leg-rejected', vals[3]); setText('leg-expired', vals[4]);
                var total = vals.reduce(function (a, b) { return a + b; }, 0);
                var dt = document.getElementById('donutTotal');
                if (dt) dt.textContent = total;
                var donut = document.getElementById('admDonut');
                if (!donut) return;
                var bg;
                if (total === 0) { bg = 'conic-gradient(#E5E7EB 0 100%)'; }
                else {
                    var stops = [], acc = 0;
                    for (var i = 0; i < vals.length; i++) {
                        if (vals[i] <= 0) continue;
                        var s = (acc / total) * 100; acc += vals[i]; var e = (acc / total) * 100;
                        stops.push(colors[i] + ' ' + s + '% ' + e + '%');
                    }
                    bg = 'conic-gradient(' + stops.join(', ') + ')';
                }
                donut.style.background = bg;
            }
            document.addEventListener('DOMContentLoaded', renderDonut);
            if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(renderDonut);
            }
        })();
    </script>
</asp:Content>
