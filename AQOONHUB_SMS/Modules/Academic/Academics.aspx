<%@ Page Title="Academics | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Academics.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Academic.Academics" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .aca-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .sum-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .sum-grid { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .sum-grid { grid-template-columns:repeat(5,1fr); } }
        .sum-card { display:flex; align-items:center; gap:.85rem; padding:1.05rem 1.15rem; }
        .sum-ico { width:44px; height:44px; border-radius:12px; display:flex; align-items:center; justify-content:center; flex:none; }
        .sum-card .lbl { font-size:.72rem; font-weight:600; color:#64748B; }
        .sum-card .val { font-size:1.35rem; font-weight:800; line-height:1.1; }
        .aca-tabs { display:flex; flex-wrap:wrap; gap:.25rem; border-bottom:1px solid #E5E7EB; margin:1.25rem 0; }
        .aca-tab { padding:.65rem .95rem; font-size:.83rem; font-weight:600; color:#64748B; border-bottom:2px solid transparent; display:inline-flex; align-items:center; gap:.4rem; text-decoration:none; }
        .aca-tab:hover { color:#2563EB; }
        .aca-tab.active { color:#2563EB; border-bottom-color:#2563EB; }
        .ov-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:1100px){ .ov-grid { grid-template-columns:1.3fr 1fr 1fr; } }
        .bar-row { display:grid; grid-template-columns:70px 1fr 42px; align-items:center; gap:.6rem; margin-bottom:.55rem; }
        .bar-track { background:#EEF2FF; border-radius:6px; height:16px; overflow:hidden; }
        .bar-fill { background:#2563EB; height:100%; border-radius:6px; }
        .ev-item { display:flex; gap:.75rem; padding:.6rem 0; border-bottom:1px solid #F1F5F9; }
        .ev-date { width:44px; text-align:center; flex:none; }
        .ev-day { font-size:1.05rem; font-weight:800; line-height:1; }
        .ev-mon { font-size:.62rem; font-weight:700; color:#64748B; text-transform:uppercase; }
        .qa-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:.6rem; }
        .qa { display:flex; align-items:center; gap:.5rem; padding:.7rem .8rem; border:1px solid #E5E7EB; border-radius:10px; font-size:.82rem; font-weight:600; color:#334155; text-decoration:none; }
        .qa:hover { border-color:#2563EB; color:#2563EB; }
        @media (max-width:768px){ .aca-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="aca-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Home</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Academics</span>
            <span>/</span><span>Overview</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Overview</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage academic years, classes, sections, subjects, teacher assignments and timetables.</p>
            </div>
            <a href="~/Modules/Academic/AcademicYears.aspx" runat="server" class="btn btn-primary"><i data-lucide="plus" class="w-4 h-4"></i> Add Academic Year</a>
        </div>

        <!-- Summary cards -->
        <div class="sum-grid">
            <div class="card sum-card"><div class="sum-ico" style="background:#EDE9FE;color:#7C3AED"><i data-lucide="calendar-check" class="w-5 h-5"></i></div>
                <div><p class="lbl">Active Academic Year</p><p class="val"><asp:Literal ID="litActiveYear" runat="server" Text="—" /></p></div></div>
            <div class="card sum-card"><div class="sum-ico" style="background:#DBEAFE;color:#2563EB"><i data-lucide="school" class="w-5 h-5"></i></div>
                <div><p class="lbl">Total Classes</p><p class="val"><asp:Literal ID="litClasses" runat="server" Text="0" /></p></div></div>
            <div class="card sum-card"><div class="sum-ico" style="background:#DCFCE7;color:#16A34A"><i data-lucide="users" class="w-5 h-5"></i></div>
                <div><p class="lbl">Total Sections</p><p class="val"><asp:Literal ID="litSections" runat="server" Text="0" /></p></div></div>
            <div class="card sum-card"><div class="sum-ico" style="background:#FEF3C7;color:#D97706"><i data-lucide="book-open" class="w-5 h-5"></i></div>
                <div><p class="lbl">Total Subjects</p><p class="val"><asp:Literal ID="litSubjects" runat="server" Text="0" /></p></div></div>
            <div class="card sum-card"><div class="sum-ico" style="background:#EDE9FE;color:#7C3AED"><i data-lucide="user-check" class="w-5 h-5"></i></div>
                <div><p class="lbl">Active Teachers</p><p class="val"><asp:Literal ID="litTeachers" runat="server" Text="0" /></p></div></div>
        </div>

        <!-- Tabs -->
        <div class="aca-tabs">
            <a class="aca-tab active" href="~/Modules/Academic/Academics.aspx" runat="server"><i data-lucide="layout-dashboard" class="w-4 h-4"></i> Overview</a>
            <a class="aca-tab" href="~/Modules/Academic/AcademicYears.aspx" runat="server"><i data-lucide="calendar-range" class="w-4 h-4"></i> Academic Years</a>
            <a class="aca-tab" href="~/Modules/Academic/ClassesSections.aspx" runat="server"><i data-lucide="school" class="w-4 h-4"></i> Classes &amp; Sections</a>
            <a class="aca-tab" href="~/Modules/Academic/Subjects.aspx" runat="server"><i data-lucide="book-open" class="w-4 h-4"></i> Subjects</a>
            <a class="aca-tab" href="~/Modules/Academic/TeacherAssignments.aspx" runat="server"><i data-lucide="user-cog" class="w-4 h-4"></i> Teacher Assignments</a>
            <a class="aca-tab" href="~/Modules/Academic/Timetable.aspx" runat="server"><i data-lucide="calendar-clock" class="w-4 h-4"></i> Timetable</a>
            <a class="aca-tab" href="~/Modules/Academic/Promotions.aspx" runat="server"><i data-lucide="trending-up" class="w-4 h-4"></i> Promotions</a>
        </div>

        <div class="ov-grid">
            <!-- Distribution -->
            <div class="card p-4">
                <h2 class="text-sm font-bold mb-3 flex items-center gap-2"><i data-lucide="bar-chart-3" class="w-4 h-4 text-blue-600"></i> Student Distribution by Class</h2>
                <asp:Literal ID="litDistribution" runat="server" />
                <asp:Panel ID="pnlNoDist" runat="server" Visible="false"><p class="text-sm text-gray-400 py-6 text-center">No student data yet.</p></asp:Panel>
            </div>

            <!-- Upcoming events -->
            <div class="card p-4">
                <h2 class="text-sm font-bold mb-2 flex items-center gap-2"><i data-lucide="calendar-days" class="w-4 h-4 text-blue-600"></i> Upcoming Academic Events</h2>
                <asp:Repeater ID="rptEvents" runat="server">
                    <ItemTemplate>
                        <div class="ev-item">
                            <div class="ev-date"><div class="ev-day"><%# Eval("EventDate", "{0:dd}") %></div><div class="ev-mon"><%# Eval("EventDate", "{0:MMM}") %></div></div>
                            <div class="flex-1">
                                <p class="text-sm font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("Title"))) %></p>
                                <p class="text-xs text-gray-500"><%# Eval("EventDate", "{0:dddd, dd MMMM yyyy}") %></p>
                            </div>
                            <span class="badge" style="background:#DBEAFE;color:#2563EB;align-self:center">Upcoming</span>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlNoEvents" runat="server" Visible="false"><p class="text-sm text-gray-400 py-6 text-center">No upcoming events.</p></asp:Panel>
            </div>

            <!-- Quick actions -->
            <div class="card p-4">
                <h2 class="text-sm font-bold mb-3 flex items-center gap-2"><i data-lucide="zap" class="w-4 h-4 text-blue-600"></i> Quick Actions</h2>
                <div class="qa-grid">
                    <a class="qa" href="~/Modules/Academic/ClassesSections.aspx" runat="server"><i data-lucide="school" class="w-4 h-4"></i> Add Class</a>
                    <a class="qa" href="~/Modules/Academic/ClassesSections.aspx" runat="server"><i data-lucide="layers" class="w-4 h-4"></i> Add Section</a>
                    <a class="qa" href="~/Modules/Academic/Subjects.aspx" runat="server"><i data-lucide="book-open" class="w-4 h-4"></i> Add Subject</a>
                    <a class="qa" href="~/Modules/Academic/TeacherAssignments.aspx" runat="server"><i data-lucide="user-cog" class="w-4 h-4"></i> Assign Teacher</a>
                    <a class="qa" href="~/Modules/Academic/Timetable.aspx" runat="server"><i data-lucide="calendar-clock" class="w-4 h-4"></i> Create Timetable</a>
                    <a class="qa" href="~/Modules/Academic/Promotions.aspx" runat="server"><i data-lucide="trending-up" class="w-4 h-4"></i> Promote Students</a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
