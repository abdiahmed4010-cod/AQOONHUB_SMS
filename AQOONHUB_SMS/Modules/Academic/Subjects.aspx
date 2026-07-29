<%@ Page Title="Subjects | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Subjects.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Academic.Subjects" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .su-wrap { padding:1.25rem; max-width:1400px; margin:0 auto; }
        .su-table { width:100%; border-collapse:collapse; }
        .su-table th { padding:.65rem 1rem; background:#f8fafc; text-align:left; font-size:.66rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .su-table td { padding:.65rem 1rem; border-bottom:1px solid #f1f5f9; font-size:.84rem; white-space:nowrap; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:30px; height:30px; border-radius:8px; color:#64748B; }
        .ico-btn:hover { background:#EFF6FF; color:#2563EB; }
        .drawer-back { position:fixed; inset:0; background:rgba(15,23,42,.45); z-index:60; }
        .drawer { position:fixed; top:0; right:0; height:100%; width:100%; max-width:440px; background:#fff; z-index:61; box-shadow:-8px 0 24px rgba(0,0,0,.12); overflow-y:auto; }
        .drawer-head { padding:1.1rem 1.25rem; border-bottom:1px solid #E5E7EB; display:flex; justify-content:space-between; align-items:center; }
        .drawer-body { padding:1.25rem; }
        @media (max-width:768px){ .su-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="su-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Academic/Academics.aspx" runat="server" class="hover:text-brand-600">Academics</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Subjects</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Subjects</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage academic subjects and their class assignments.</p>
            </div>
            <asp:Button ID="btnAdd" runat="server" Text="+ Add Subject" CssClass="btn btn-primary" OnClick="btnAdd_Click" CausesValidation="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-5 gap-3">
                <div class="md:col-span-2"><label class="block text-xs font-bold text-slate-700 mb-1.5">Search</label>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search subjects..." /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Type</label>
                    <asp:DropDownList ID="ddlTypeFilter" runat="server" CssClass="input">
                        <asp:ListItem Text="All Types" Value="" />
                        <asp:ListItem Text="Core" Value="Core" />
                        <asp:ListItem Text="Optional" Value="Optional" />
                        <asp:ListItem Text="Practical" Value="Practical" />
                    </asp:DropDownList></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label>
                    <asp:DropDownList ID="ddlClassFilter" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="input">
                        <asp:ListItem Text="All Status" Value="" />
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Inactive" Value="Inactive" />
                    </asp:DropDownList></div>
            </div>
            <div class="mt-3 flex justify-end gap-2">
                <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
            </div>
        </div>

        <!-- Table -->
        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="su-table" OnRowCommand="gv_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="SubjectCode" HeaderText="Code" />
                        <asp:TemplateField HeaderText="Subject Name"><ItemTemplate><span class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("SubjectName"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="SubjectType" HeaderText="Type" />
                        <asp:TemplateField HeaderText="Classes"><ItemTemplate><%# Eval("ClassCount") %> Classes</ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="WeeklyPeriods" HeaderText="Weekly Periods" />
                        <asp:BoundField DataField="MaxMarks" HeaderText="Max Marks" />
                        <asp:BoundField DataField="PassMarks" HeaderText="Pass Marks" />
                        <asp:TemplateField HeaderText="Status"><ItemTemplate>
                            <span class="badge" style='<%# Convert.ToBoolean(Eval("IsActive")) ? "background:#DCFCE7;color:#15803D" : "background:#FEF3C7;color:#B45309" %>'><%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %></span>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <div class="flex items-center gap-1">
                                <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditRow" CommandArgument='<%# Eval("SubjectID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="AssignRow" CommandArgument='<%# Eval("SubjectID") %>' ToolTip="Assign to class"><i data-lucide="link" class="w-4 h-4"></i></asp:LinkButton>
                                <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="ToggleActive" CommandArgument='<%# Eval("SubjectID") %>' ToolTip="Activate / Deactivate"><i data-lucide="power" class="w-4 h-4"></i></asp:LinkButton>
                            </div>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No subjects found.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <!-- ===== SUBJECT DRAWER ===== -->
        <asp:Panel ID="pnlDrawer" runat="server" Visible="false">
            <div class="drawer-back"></div>
            <div class="drawer">
                <div class="drawer-head">
                    <h3 class="font-bold text-base"><asp:Literal ID="litTitle" runat="server" Text="Add Subject" /></h3>
                    <asp:LinkButton ID="btnClose" runat="server" CssClass="ico-btn" OnClick="btnCancel_Click" CausesValidation="false"><i data-lucide="x" class="w-5 h-5"></i></asp:LinkButton>
                </div>
                <div class="drawer-body">
                    <asp:HiddenField ID="hfId" runat="server" Value="0" />
                    <div class="space-y-4">
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject Code <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtCode" runat="server" CssClass="input" placeholder="e.g. MAT-01" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtName" runat="server" CssClass="input" placeholder="e.g. Mathematics" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Type <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlType" runat="server" CssClass="input">
                                <asp:ListItem Text="Core" Value="Core" />
                                <asp:ListItem Text="Optional" Value="Optional" />
                                <asp:ListItem Text="Practical" Value="Practical" />
                            </asp:DropDownList></div>
                        <div class="grid grid-cols-2 gap-3">
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Max Marks <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtMax" runat="server" CssClass="input" TextMode="Number" Text="100" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Pass Marks <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtPass" runat="server" CssClass="input" TextMode="Number" Text="50" /></div>
                        </div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlActive" runat="server" CssClass="input">
                                <asp:ListItem Text="Active" Value="1" />
                                <asp:ListItem Text="Inactive" Value="0" />
                            </asp:DropDownList></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Description</label>
                            <asp:TextBox ID="txtDesc" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" placeholder="Optional" /></div>
                    </div>
                    <div class="flex justify-end gap-2 mt-6">
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                        <asp:Button ID="btnSave" runat="server" Text="Save Subject" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- ===== ASSIGN-TO-CLASS DRAWER ===== -->
        <asp:Panel ID="pnlAssign" runat="server" Visible="false">
            <div class="drawer-back"></div>
            <div class="drawer">
                <div class="drawer-head">
                    <h3 class="font-bold text-base">Assign Subject to Class</h3>
                    <asp:LinkButton ID="btnCloseAssign" runat="server" CssClass="ico-btn" OnClick="btnCancelAssign_Click" CausesValidation="false"><i data-lucide="x" class="w-5 h-5"></i></asp:LinkButton>
                </div>
                <div class="drawer-body">
                    <asp:HiddenField ID="hfAssignSubject" runat="server" Value="0" />
                    <p class="text-sm text-gray-600 mb-4">Assigning <span class="font-semibold"><asp:Literal ID="litAssignSubject" runat="server" /></span>. The teacher can be set later under Teacher Assignments.</p>
                    <div class="space-y-4">
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlAssignClass" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlAssignYear" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Weekly Periods <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtAssignPeriods" runat="server" CssClass="input" TextMode="Number" Text="4" /></div>
                    </div>
                    <div class="flex justify-end gap-2 mt-6">
                        <asp:Button ID="btnCancelAssign" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancelAssign_Click" CausesValidation="false" />
                        <asp:Button ID="btnSaveAssign" runat="server" Text="Assign" CssClass="btn btn-primary" OnClick="btnSaveAssign_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
