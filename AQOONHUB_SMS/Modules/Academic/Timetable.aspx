<%@ Page Title="Weekly Timetable | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Timetable.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Academic.Timetable" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .tt-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .tt-grid { width:100%; border-collapse:collapse; min-width:900px; }
        .tt-grid th, .tt-grid td { border:1px solid #E5E7EB; padding:.4rem; vertical-align:top; }
        .tt-grid th { background:#f8fafc; font-size:.72rem; font-weight:700; color:#334155; text-align:center; }
        .tt-timecol { background:#f8fafc; font-size:.72rem; font-weight:700; color:#475569; white-space:nowrap; text-align:center; width:120px; }
        .lesson { border-radius:8px; padding:.4rem .5rem; font-size:.74rem; line-height:1.25; }
        .lesson .s { font-weight:800; }
        .lesson .t { color:#475569; }
        .lesson .r { color:#64748B; font-size:.68rem; }
        .tt-list th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.64rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tt-list td { padding:.6rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; white-space:nowrap; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:30px; height:30px; border-radius:8px; color:#64748B; }
        .ico-btn:hover { background:#EFF6FF; color:#2563EB; }
        .ico-btn.danger:hover { background:#FEF2F2; color:#DC2626; }
        .view-btn { padding:.45rem .85rem; font-size:.8rem; font-weight:600; border:1px solid #E5E7EB; background:#fff; color:#475569; }
        .view-btn.active { background:#2563EB; color:#fff; border-color:#2563EB; }
        .drawer-back { position:fixed; inset:0; background:rgba(15,23,42,.45); z-index:60; }
        .drawer { position:fixed; top:0; right:0; height:100%; width:100%; max-width:440px; background:#fff; z-index:61; box-shadow:-8px 0 24px rgba(0,0,0,.12); overflow-y:auto; }
        .drawer-head { padding:1.1rem 1.25rem; border-bottom:1px solid #E5E7EB; display:flex; justify-content:space-between; align-items:center; }
        .drawer-body { padding:1.25rem; }
        @media print {
            body * { visibility:hidden; }
            #printArea, #printArea * { visibility:visible; }
            #printArea { position:absolute; left:0; top:0; width:100%; }
            .no-print { display:none !important; }
            @page { size:landscape; }
        }
        @media (max-width:768px){ .tt-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tt-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5 no-print">
            <a href="~/Modules/Academic/Academics.aspx" runat="server" class="hover:text-brand-600">Academics</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Timetable</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4 no-print">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Weekly Timetable</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">View and manage class schedules for the selected academic period.</p>
            </div>
            <div class="flex gap-2">
                <asp:Button ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-secondary" OnClientClick="window.print();return false;" CausesValidation="false" />
                <asp:Button ID="btnAddEntry" runat="server" Text="+ Add Timetable Entry" CssClass="btn btn-primary" OnClick="btnAddEntry_Click" CausesValidation="false" />
            </div>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm no-print"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4 no-print">
            <div class="grid grid-cols-1 md:grid-cols-4 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
            </div>
            <div class="mt-3 flex justify-between items-center">
                <div class="flex">
                    <asp:LinkButton ID="btnWeekly" runat="server" CssClass="view-btn active" style="border-radius:8px 0 0 8px" OnClick="btnWeekly_Click" CausesValidation="false">Weekly View</asp:LinkButton>
                    <asp:LinkButton ID="btnList" runat="server" CssClass="view-btn" style="border-radius:0 8px 8px 0" OnClick="btnList_Click" CausesValidation="false">List View</asp:LinkButton>
                </div>
                <div class="flex gap-2">
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                    <asp:Button ID="btnViewTimetable" runat="server" Text="View Timetable" CssClass="btn btn-primary" OnClick="btnView_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <!-- Print header + grid -->
        <div id="printArea">
            <div class="mb-3">
                <h2 class="text-lg font-bold">AQOONHUB SMS — Weekly Timetable</h2>
                <p class="text-sm text-gray-600"><asp:Literal ID="litContext" runat="server" /></p>
            </div>

            <asp:Panel ID="pnlWeekly" runat="server" CssClass="card overflow-hidden">
                <div class="overflow-x-auto">
                    <asp:Literal ID="litWeekly" runat="server" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlList" runat="server" Visible="false" CssClass="card overflow-hidden">
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvList" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tt-list" OnRowCommand="gvList_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="ClassName" HeaderText="Class" />
                            <asp:BoundField DataField="SectionName" HeaderText="Section" />
                            <asp:TemplateField HeaderText="Term"><ItemTemplate><%# Eval("TermName")==System.DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(Eval("TermName"))) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Day"><ItemTemplate><%# DayName(Convert.ToInt32(Eval("DayOfWeek"))) %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="PeriodNo" HeaderText="Period" />
                            <asp:TemplateField HeaderText="Start"><ItemTemplate><%# Time(Eval("StartTime")) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="End"><ItemTemplate><%# Time(Eval("EndTime")) %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="SubjectName" HeaderText="Subject" />
                            <asp:BoundField DataField="TeacherName" HeaderText="Teacher" />
                            <asp:TemplateField HeaderText="Room"><ItemTemplate><%# Eval("RoomNumber")==System.DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(Eval("RoomNumber"))) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                                <div class="flex items-center gap-1 no-print">
                                    <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditRow" CommandArgument='<%# Eval("TimetableID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                    <asp:LinkButton runat="server" CssClass="ico-btn danger" CommandName="DeleteRow" CommandArgument='<%# Eval("TimetableID") %>' ToolTip="Delete"
                                        OnClientClick="return confirm('Delete this timetable entry?');"><i data-lucide="trash-2" class="w-4 h-4"></i></asp:LinkButton>
                                </div>
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="py-10 text-center text-sm text-gray-500">No timetable entries for the selected filters.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </asp:Panel>
        </div>

        <!-- ===== ENTRY DRAWER ===== -->
        <asp:Panel ID="pnlDrawer" runat="server" Visible="false" CssClass="no-print">
            <div class="drawer-back"></div>
            <div class="drawer">
                <div class="drawer-head">
                    <h3 class="font-bold text-base"><asp:Literal ID="litTitle" runat="server" Text="Add Timetable Entry" /></h3>
                    <asp:LinkButton ID="btnClose" runat="server" CssClass="ico-btn" OnClick="btnCancel_Click" CausesValidation="false"><i data-lucide="x" class="w-5 h-5"></i></asp:LinkButton>
                </div>
                <div class="drawer-body">
                    <asp:HiddenField ID="hfId" runat="server" Value="0" />
                    <div class="space-y-4">
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="dYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="dYear_Changed" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label>
                            <asp:DropDownList ID="dTerm" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="dClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="dClass_Changed" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="dSection" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="dSection_Changed" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="dSubject" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="dSubject_Changed" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Teacher</label>
                            <asp:TextBox ID="dTeacher" runat="server" CssClass="input bg-gray-50" ReadOnly="true" />
                            <asp:HiddenField ID="hfStaffId" runat="server" Value="0" /></div>
                        <div class="grid grid-cols-2 gap-3">
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Day <span class="text-red-500">*</span></label>
                                <asp:DropDownList ID="dDay" runat="server" CssClass="input">
                                    <asp:ListItem Text="Sunday" Value="0" />
                                    <asp:ListItem Text="Monday" Value="1" />
                                    <asp:ListItem Text="Tuesday" Value="2" />
                                    <asp:ListItem Text="Wednesday" Value="3" />
                                    <asp:ListItem Text="Thursday" Value="4" />
                                </asp:DropDownList></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Period No <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="dPeriod" runat="server" CssClass="input" TextMode="Number" Text="1" /></div>
                        </div>
                        <div class="grid grid-cols-2 gap-3">
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Start Time <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="dStart" runat="server" CssClass="input" TextMode="Time" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">End Time <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="dEnd" runat="server" CssClass="input" TextMode="Time" /></div>
                        </div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Room <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="dRoom" runat="server" CssClass="input" placeholder="e.g. R-20" /></div>
                    </div>
                    <div class="flex justify-end gap-2 mt-6">
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                        <asp:Button ID="btnSave" runat="server" Text="Save Entry" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
