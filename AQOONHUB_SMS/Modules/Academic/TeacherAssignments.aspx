<%@ Page Title="Teacher Assignments | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="TeacherAssignments.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Academic.TeacherAssignments" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ta-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .ta-table { width:100%; border-collapse:collapse; }
        .ta-table th { padding:.65rem 1rem; background:#f8fafc; text-align:left; font-size:.66rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .ta-table td { padding:.65rem 1rem; border-bottom:1px solid #f1f5f9; font-size:.84rem; white-space:nowrap; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:30px; height:30px; border-radius:8px; color:#64748B; }
        .ico-btn:hover { background:#EFF6FF; color:#2563EB; }
        .ico-btn.danger:hover { background:#FEF2F2; color:#DC2626; }
        .avatar-i { width:30px; height:30px; border-radius:50%; background:#E0E7FF; color:#4338CA; display:inline-flex; align-items:center; justify-content:center; font-size:.7rem; font-weight:800; }
        .drawer-back { position:fixed; inset:0; background:rgba(15,23,42,.45); z-index:60; }
        .drawer { position:fixed; top:0; right:0; height:100%; width:100%; max-width:420px; background:#fff; z-index:61; box-shadow:-8px 0 24px rgba(0,0,0,.12); overflow-y:auto; }
        .drawer-head { padding:1.1rem 1.25rem; border-bottom:1px solid #E5E7EB; display:flex; justify-content:space-between; align-items:center; }
        .drawer-body { padding:1.25rem; }
        @media (max-width:768px){ .ta-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ta-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Academic/Academics.aspx" runat="server" class="hover:text-brand-600">Academics</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Teacher Assignments</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Teacher Assignments</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage subject assignments for teachers across classes and sections.</p>
            </div>
            <asp:Button ID="btnAssign" runat="server" Text="+ Assign Teacher" CssClass="btn btn-primary" OnClick="btnAssign_Click" CausesValidation="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-5 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlFYear" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlFClass" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject</label><asp:DropDownList ID="ddlFSubject" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Teacher</label><asp:DropDownList ID="ddlFTeacher" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Search</label><asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Teacher or subject..." /></div>
            </div>
            <div class="mt-3 flex justify-end gap-2">
                <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
            </div>
        </div>

        <!-- Table -->
        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="ta-table" OnRowCommand="gv_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Teacher"><ItemTemplate>
                            <div class="flex items-center gap-2"><span class="avatar-i"><%# Initials(Convert.ToString(Eval("TeacherName"))) %></span><span class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("TeacherName"))) %></span></div>
                        </ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="SubjectName" HeaderText="Subject" />
                        <asp:BoundField DataField="ClassName" HeaderText="Class" />
                        <asp:BoundField DataField="SectionName" HeaderText="Section" />
                        <asp:BoundField DataField="WeeklyPeriods" HeaderText="Weekly Periods" />
                        <asp:TemplateField HeaderText="Status"><ItemTemplate>
                            <span class="badge" style='<%# Convert.ToBoolean(Eval("IsActive")) ? "background:#DCFCE7;color:#15803D" : "background:#FEF3C7;color:#B45309" %>'><%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %></span>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <div class="flex items-center gap-1">
                                <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditRow" CommandArgument='<%# Eval("CSTID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                <asp:LinkButton runat="server" CssClass="ico-btn danger" CommandName="RemoveRow" CommandArgument='<%# Eval("CSTID") %>' ToolTip="Remove teacher"
                                    OnClientClick="return confirm('Remove this teacher from the assignment? The class-subject link is kept.');"><i data-lucide="user-minus" class="w-4 h-4"></i></asp:LinkButton>
                            </div>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No teacher assignments yet. Click “Assign Teacher”.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <!-- ===== ASSIGN DRAWER ===== -->
        <asp:Panel ID="pnlDrawer" runat="server" Visible="false">
            <div class="drawer-back"></div>
            <div class="drawer">
                <div class="drawer-head">
                    <h3 class="font-bold text-base"><asp:Literal ID="litTitle" runat="server" Text="Assign Teacher" /></h3>
                    <asp:LinkButton ID="btnClose" runat="server" CssClass="ico-btn" OnClick="btnCancel_Click" CausesValidation="false"><i data-lucide="x" class="w-5 h-5"></i></asp:LinkButton>
                </div>
                <div class="drawer-body">
                    <asp:HiddenField ID="hfId" runat="server" Value="0" />
                    <div class="space-y-4">
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlSubject" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Teacher <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlTeacher" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Weekly Periods <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtPeriods" runat="server" CssClass="input" TextMode="Number" Text="4" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                                <asp:ListItem Text="Active" Value="1" />
                                <asp:ListItem Text="Inactive" Value="0" />
                            </asp:DropDownList></div>
                    </div>
                    <div class="flex justify-end gap-2 mt-6">
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                        <asp:Button ID="btnSave" runat="server" Text="Save Assignment" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
