<%@ Page Title="Students | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Students.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Students.Students" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Only page-specific additions — everything else (btn, input, card, th, td,
           badge, avatar, tr.rowlink, tab-btn) already comes from MainMaster.master. */
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

        <!-- Header -->
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Students</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Students</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Register, manage and track all enrolled students.</p>
            </div>
            <div class="flex items-center gap-2 flex-wrap">
                <asp:HyperLink ID="lnkExport" runat="server" CssClass="btn btn-secondary" NavigateUrl="~/Modules/Reports/Export.aspx?module=students">
                    <i data-lucide="download" class="w-4 h-4"></i> Export
                </asp:HyperLink>
                <asp:HyperLink ID="lnkAddStudent" runat="server" CssClass="btn btn-primary" NavigateUrl="~/Modules/Students/AddStudent.aspx">
                    <i data-lucide="user-plus" class="w-4 h-4"></i> Add Student
                </asp:HyperLink>
            </div>
        </div>

        <!-- Summary Tiles -->
        <div class="grid grid-cols-2 md:grid-cols-5 gap-4 mb-5">
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#EFF6FF;color:#2563EB"><i data-lucide="graduation-cap" class="w-5 h-5"></i></span>
                <div><p class="lbl">Total</p><p class="val"><asp:Label ID="lblTotalStudents" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#ECFDF5;color:#22C55E"><i data-lucide="check-circle-2" class="w-5 h-5"></i></span>
                <div><p class="lbl">Active</p><p class="val"><asp:Label ID="lblActiveStudents" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#F1F5F9;color:#64748B"><i data-lucide="pause-circle" class="w-5 h-5"></i></span>
                <div><p class="lbl">Inactive</p><p class="val"><asp:Label ID="lblInactiveStudents" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#F5F3FF;color:#7C3AED"><i data-lucide="arrow-up-right" class="w-5 h-5"></i></span>
                <div><p class="lbl">Graduated</p><p class="val"><asp:Label ID="lblGraduatedStudents" runat="server" Text="0" /></p></div>
            </div>
            <div class="card p-5 stat-tile">
                <span class="ic" style="background:#ECFEFF;color:#0EA5E9"><i data-lucide="arrow-right-left" class="w-5 h-5"></i></span>
                <div><p class="lbl">Transferred</p><p class="val"><asp:Label ID="lblTransferredStudents" runat="server" Text="0" /></p></div>
            </div>
        </div>

        <!-- Filters -->
        <div class="card p-3.5 mb-4 filter-bar">
            <div class="grow">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search by name, ID, admission no., guardian name or phone…" />
            </div>
            <asp:DropDownList ID="ddlClass" runat="server" CssClass="input !w-auto" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_SelectedIndexChanged" />
            <asp:DropDownList ID="ddlSection" runat="server" CssClass="input !w-auto" />
            <asp:DropDownList ID="ddlGender" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Genders" Value="" />
                <asp:ListItem Text="Male" Value="Male" />
                <asp:ListItem Text="Female" Value="Female" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Statuses" Value="" />
                <asp:ListItem Text="Active" Value="Active" />
                <asp:ListItem Text="Inactive" Value="Inactive" />
                <asp:ListItem Text="Graduated" Value="Graduated" />
                <asp:ListItem Text="Transferred" Value="Transferred" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlShiftFilter" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Shifts" Value="" />
                <asp:ListItem Text="Morning" Value="Morning" />
                <asp:ListItem Text="Afternoon" Value="Afternoon" />
            </asp:DropDownList>
            <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-primary" OnClick="btnSearch_Click">Search</asp:LinkButton>
            <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false">Reset</asp:LinkButton>
        </div>

        <!-- Table -->
        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gvStudents" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true"
                    AllowSorting="true" CssClass="w-full" DataKeyNames="StudentID"
                    OnRowCommand="gvStudents_RowCommand" OnSorting="gvStudents_Sorting">
                    <HeaderStyle CssClass="" />
                    <Columns>
                        <asp:TemplateField HeaderText="Student" SortExpression="FirstName">
                            <HeaderStyle CssClass="th" />
                            <ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex items-center gap-3">
                                    <asp:Image ID="imgPhoto" runat="server" CssClass="avatar" Style="width:32px;height:32px;object-fit:cover;"
                                        Visible='<%# !string.IsNullOrEmpty(Eval("PhotoPath") as string) %>'
                                        ImageUrl='<%# ResolveUrl("~/" + Eval("PhotoPath")) %>' AlternateText='<%# Eval("FullName") + " photo" %>' />
                                    <span class="avatar" style='<%# "width:32px;height:32px;font-size:12px;background:" + GetAvatarColor(Eval("FullName")) %>'
                                        visible='<%# string.IsNullOrEmpty(Eval("PhotoPath") as string) %>'><%# GetInitials(Eval("FullName")) %></span>
                                    <div>
                                        <p class="font-semibold"><%# Eval("FullName") %></p>
                                        <p class="text-[11px] text-gray-400"><%# Eval("StudentCode") %></p>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="AdmissionNo" HeaderText="Admission No." SortExpression="AdmissionNo" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="ClassName" HeaderText="Class" SortExpression="ClassName" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="SectionName" HeaderText="Section" SortExpression="SectionName" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Shift" SortExpression="Shift">
                            <HeaderStyle CssClass="th" />
                            <ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <span class="badge" style='<%# GetShiftBadgeStyle(Eval("Shift")) %>'><%# Eval("Shift") == null || Eval("Shift").ToString() == "" ? "—" : Eval("Shift") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="DateOfBirth" HeaderText="Date of Birth" SortExpression="DateOfBirth" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="GuardianName" HeaderText="Guardian" SortExpression="GuardianName" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="EnrollmentDate" HeaderText="Enrolled" SortExpression="EnrollmentDate" DataFormatString="{0:MMM dd, yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Status" SortExpression="Status">
                            <HeaderStyle CssClass="th" />
                            <ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <span class="badge" style='<%# GetStatusBadgeStyle(Eval("Status")) %>'><%# Eval("Status") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <HeaderStyle CssClass="th" />
                            <ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex gap-1">
                                    <asp:HyperLink runat="server" CssClass="btn-ghost btn !p-1.5" NavigateUrl='<%# "~/Modules/Students/StudentDetails.aspx?id=" + Eval("StudentID") %>' ToolTip="View Details">
                                        <i data-lucide="eye" class="w-4 h-4"></i>
                                    </asp:HyperLink>
                                    <asp:HyperLink ID="lnkEditStudent" runat="server" CssClass="btn-ghost btn !p-1.5" Visible='<%# CanEdit %>' NavigateUrl='<%# "~/Modules/Students/EditStudent.aspx?id=" + Eval("StudentID") %>' ToolTip="Edit">
                                        <i data-lucide="pencil" class="w-4 h-4"></i>
                                    </asp:HyperLink>
                                    <asp:HyperLink runat="server" CssClass="btn-ghost btn !p-1.5" NavigateUrl='<%# "~/Modules/Students/StudentTransfer.aspx?id=" + Eval("StudentID") %>'
                                        ToolTip='<%# Eval("Status").ToString() == "Transferred" ? "Return to School" : "Transfer" %>'>
                                        <i data-lucide='<%# Eval("Status").ToString() == "Transferred" ? "undo-2" : "arrow-right-left" %>' class="w-4 h-4"></i>
                                    </asp:HyperLink>
                                    <asp:LinkButton runat="server" CssClass="btn-ghost btn !p-1.5 !text-red-500" CommandName="SoftDelete"
                                        CommandArgument='<%# Eval("StudentID") + "|" + Eval("FullName") %>'
                                        OnClientClick='<%# "return confirm(\x27Delete " + Eval("FullName") + "? This moves the record to Trash — a Super Admin can restore it later.\x27);" %>'
                                        ToolTip="Delete">
                                        <i data-lucide="trash-2" class="w-4 h-4"></i>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <RowStyle CssClass="rowlink" />
                    <EmptyDataTemplate>
                        <div class="flex flex-col items-center justify-center py-16 text-center">
                            <span class="w-14 h-14 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-300 flex items-center justify-center mb-4">
                                <i data-lucide="users" class="w-7 h-7"></i>
                            </span>
                            <h3 class="font-bold">No students found</h3>
                            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1 mb-4 max-w-sm">Try adjusting your search or filters, or add a new student.</p>
                            <a href="~/Modules/Students/AddStudent.aspx" runat="server" class="btn btn-primary"><i data-lucide="plus" class="w-4 h-4"></i> Add Student</a>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <!-- Pager -->
            <div class="flex items-center justify-between px-4 py-3 border-t border-gray-100 dark:border-slate-700 flex-wrap gap-2">
                <asp:Label runat="server" ID="lblResultsSummary" Text="Showing 0 of 0" CssClass="text-xs text-gray-500 dark:text-slate-400" />
                <div class="flex items-center gap-2">
                    <span class="text-xs text-gray-500 dark:text-slate-400">Page size:</span>
                    <asp:DropDownList ID="ddlPageSize" runat="server" CssClass="input !w-auto !py-1.5 text-xs" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
                        <asp:ListItem Text="10" Value="10" />
                        <asp:ListItem Text="25" Value="25" />
                        <asp:ListItem Text="50" Value="50" />
                        <asp:ListItem Text="100" Value="100" />
                    </asp:DropDownList>
                    <asp:LinkButton ID="btnPrevPage" runat="server" CssClass="btn btn-ghost !p-1.5" OnClick="btnPrevPage_Click"><i data-lucide="chevron-left" class="w-4 h-4"></i></asp:LinkButton>
                    <asp:Label runat="server" ID="lblPageIndicator" Text="Page 1 of 1" CssClass="text-xs" />
                    <asp:LinkButton ID="btnNextPage" runat="server" CssClass="btn btn-ghost !p-1.5" OnClick="btnNextPage_Click"><i data-lucide="chevron-right" class="w-4 h-4"></i></asp:LinkButton>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
