<%@ Page Title="Attendance | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.Attendance" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .at-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .at-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .at-sum { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .at-sum { grid-template-columns:repeat(5,1fr); } }
        .at-card { display:flex; align-items:center; gap:.85rem; padding:1.05rem 1.15rem; }
        .at-ico { width:44px; height:44px; border-radius:12px; display:flex; align-items:center; justify-content:center; flex:none; }
        .at-card .lbl { font-size:.72rem; font-weight:600; color:#64748B; }
        .at-card .val { font-size:1.5rem; font-weight:800; line-height:1.05; }
        .at-tabs { display:flex; flex-wrap:wrap; gap:.25rem; border-bottom:1px solid #E5E7EB; margin:1.25rem 0; }
        .at-tab { padding:.6rem .9rem; font-size:.82rem; font-weight:600; color:#64748B; border-bottom:2px solid transparent; text-decoration:none; display:inline-flex; align-items:center; gap:.4rem; }
        .at-tab:hover { color:#2563EB; } .at-tab.active { color:#2563EB; border-bottom-color:#2563EB; }
        .at-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:1100px){ .at-grid { grid-template-columns:2fr 1fr; } }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.64rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.6rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.83rem; white-space:nowrap; }
        @media (max-width:768px){ .at-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="at-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Attendance</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Attendance Management</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Track daily attendance, monitor trends and manage attendance records.</p>
            </div>
            <div class="flex gap-2">
                <asp:HyperLink ID="lnkSettings" runat="server" NavigateUrl="~/Modules/Attendance/AttendanceSettings.aspx" CssClass="btn btn-secondary"><i data-lucide="settings" class="w-4 h-4"></i> Attendance Settings</asp:HyperLink>
            </div>
        </div>

        <!-- Summary cards -->
        <div class="at-sum">
            <div class="card at-card"><div class="at-ico" style="background:#DBEAFE;color:#2563EB"><i data-lucide="users" class="w-5 h-5"></i></div>
                <div><p class="lbl">Total Students</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div></div>
            <div class="card at-card"><div class="at-ico" style="background:#DCFCE7;color:#16A34A"><i data-lucide="check-circle" class="w-5 h-5"></i></div>
                <div><p class="lbl">Present Today</p><p class="val text-emerald-700"><asp:Literal ID="litPresent" runat="server" Text="0" /></p></div></div>
            <div class="card at-card"><div class="at-ico" style="background:#FEE2E2;color:#DC2626"><i data-lucide="x-circle" class="w-5 h-5"></i></div>
                <div><p class="lbl">Absent Today</p><p class="val text-red-700"><asp:Literal ID="litAbsent" runat="server" Text="0" /></p></div></div>
            <div class="card at-card"><div class="at-ico" style="background:#FEF3C7;color:#D97706"><i data-lucide="clock" class="w-5 h-5"></i></div>
                <div><p class="lbl">Late Today</p><p class="val text-amber-700"><asp:Literal ID="litLate" runat="server" Text="0" /></p></div></div>
            <div class="card at-card"><div class="at-ico" style="background:#EDE9FE;color:#7C3AED"><i data-lucide="percent" class="w-5 h-5"></i></div>
                <div><p class="lbl">Attendance Rate</p><p class="val text-indigo-700"><asp:Literal ID="litRate" runat="server" Text="0%" /></p></div></div>
        </div>

        <!-- Tabs (only implemented pages are linked) -->
        <div class="at-tabs">
            <a class="at-tab active" href="~/Modules/Attendance/Attendance.aspx" runat="server"><i data-lucide="layout-dashboard" class="w-4 h-4"></i> Overview</a>
            <a class="at-tab" href="~/Modules/Attendance/AttendanceSettings.aspx" runat="server"><i data-lucide="settings" class="w-4 h-4"></i> Settings</a>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-5 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Date</label><asp:TextBox ID="txtDate" runat="server" CssClass="input" TextMode="Date" /></div>
                <div class="flex items-end gap-2">
                    <asp:Button ID="btnFilter" runat="server" Text="View" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="at-grid">
            <!-- Today's attendance -->
            <div class="card overflow-hidden">
                <div class="card-head"><h2 class="text-sm font-bold">Today&#39;s Attendance</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvToday" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl" DataKeyNames="StudentID">
                        <Columns>
                            <asp:TemplateField HeaderText="Student"><ItemTemplate>
                                <div class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("FullName"))) %></div>
                                <div class="text-xs text-gray-500"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentCode"))) %></div>
                            </ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="ClassName" HeaderText="Class" />
                            <asp:BoundField DataField="SectionName" HeaderText="Section" />
                            <asp:TemplateField HeaderText="Status"><ItemTemplate>
                                <span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("AttendanceStatus"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("AttendanceStatus"))) %></span>
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Check-in"><ItemTemplate><%# FormatTime(Eval("CheckInTime")) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Remark"><ItemTemplate><%# Server.HtmlEncode(Convert.ToString(Eval("Remarks"))) %></ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No attendance marked for this date yet.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- Side: quick actions + breakdown + recent -->
            <div class="space-y-4">
                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Quick Actions</h2></div>
                    <div class="p-3 space-y-2 text-sm">
                        <a href="~/Modules/Attendance/MarkAttendance.aspx" runat="server" class="flex items-center gap-2 text-brand-600 hover:underline"><i data-lucide="clipboard-check" class="w-4 h-4"></i> Mark Attendance</a>
                        <a href="~/Modules/Attendance/AttendanceByDate.aspx" runat="server" class="flex items-center gap-2 text-brand-600 hover:underline"><i data-lucide="calendar-days" class="w-4 h-4"></i> Attendance by Date</a>
                        <a href="~/Modules/Attendance/StudentAttendanceReport.aspx" runat="server" class="flex items-center gap-2 text-brand-600 hover:underline"><i data-lucide="user" class="w-4 h-4"></i> Student Report</a>
                        <a href="~/Modules/Attendance/ClassAttendanceReport.aspx" runat="server" class="flex items-center gap-2 text-brand-600 hover:underline"><i data-lucide="users" class="w-4 h-4"></i> Class Report</a>
                        <a href="~/Modules/Attendance/AttendanceCalendar.aspx" runat="server" class="flex items-center gap-2 text-brand-600 hover:underline"><i data-lucide="calendar" class="w-4 h-4"></i> Attendance Calendar</a>
                        <a href="~/Modules/Attendance/AttendanceSettings.aspx" runat="server" class="flex items-center gap-2 text-brand-600 hover:underline"><i data-lucide="settings" class="w-4 h-4"></i> Attendance Settings</a>
                    </div>
                </div>

                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Today&#39;s Breakdown</h2></div>
                    <div class="p-4 text-sm space-y-2">
                        <div class="flex items-center justify-between"><span class="flex items-center gap-2"><span class="inline-block w-2.5 h-2.5 rounded-full" style="background:#16A34A"></span> Present</span><b><asp:Literal ID="litBP" runat="server" Text="0" /></b></div>
                        <div class="flex items-center justify-between"><span class="flex items-center gap-2"><span class="inline-block w-2.5 h-2.5 rounded-full" style="background:#DC2626"></span> Absent</span><b><asp:Literal ID="litBA" runat="server" Text="0" /></b></div>
                        <div class="flex items-center justify-between"><span class="flex items-center gap-2"><span class="inline-block w-2.5 h-2.5 rounded-full" style="background:#D97706"></span> Late</span><b><asp:Literal ID="litBL" runat="server" Text="0" /></b></div>
                        <div class="flex items-center justify-between"><span class="flex items-center gap-2"><span class="inline-block w-2.5 h-2.5 rounded-full" style="background:#7C3AED"></span> Excused</span><b><asp:Literal ID="litBE" runat="server" Text="0" /></b></div>
                    </div>
                </div>

                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Recent Attendance Activity</h2></div>
                    <div class="p-4">
                        <asp:Repeater ID="rptActivity" runat="server">
                            <ItemTemplate>
                                <div class="flex items-center justify-between py-1.5 border-b border-gray-50 text-sm">
                                    <span><b><%# Server.HtmlEncode(Convert.ToString(Eval("ClassName"))) %> / <%# Server.HtmlEncode(Convert.ToString(Eval("SectionName"))) %></b> · P<%# Eval("Present") %> A<%# Eval("Absent") %> L<%# Eval("Late") %></span>
                                    <span class="text-xs text-gray-400"><%# Eval("AttendanceDate","{0:dd MMM}") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlNoActivity" runat="server" Visible="false"><p class="text-sm text-gray-400 text-center py-6">No attendance activity yet.</p></asp:Panel>
                    </div>
                </div>

                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Attendance by Class</h2></div>
                    <div class="overflow-x-auto">
                        <asp:GridView ID="gvByClass" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                            <Columns>
                                <asp:BoundField DataField="ClassName" HeaderText="Class" />
                                <asp:BoundField DataField="Present" HeaderText="Present" />
                                <asp:BoundField DataField="Absent" HeaderText="Absent" />
                                <asp:BoundField DataField="Late" HeaderText="Late" />
                            </Columns>
                            <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No class attendance for this date.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
