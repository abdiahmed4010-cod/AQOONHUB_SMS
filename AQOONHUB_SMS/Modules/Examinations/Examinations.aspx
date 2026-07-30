<%@ Page Title="Examinations | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Examinations.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.Examinations" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ex-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .ex-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .ex-sum { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .ex-sum { grid-template-columns:repeat(5,1fr); } }
        .ex-card { display:flex; align-items:center; gap:.85rem; padding:1.05rem 1.15rem; }
        .ex-ico { width:44px; height:44px; border-radius:12px; display:flex; align-items:center; justify-content:center; flex:none; }
        .ex-card .lbl { font-size:.72rem; font-weight:600; color:#64748B; }
        .ex-card .val { font-size:1.5rem; font-weight:800; line-height:1.05; }
        .ex-tabs { display:flex; flex-wrap:wrap; gap:.25rem; border-bottom:1px solid #E5E7EB; margin:1.25rem 0; }
        .ex-tab { padding:.6rem .9rem; font-size:.82rem; font-weight:600; color:#64748B; border-bottom:2px solid transparent; text-decoration:none; display:inline-flex; align-items:center; gap:.4rem; }
        .ex-tab:hover { color:#2563EB; } .ex-tab.active { color:#2563EB; border-bottom-color:#2563EB; }
        .ex-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:1100px){ .ex-grid { grid-template-columns:2fr 1fr; } }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.64rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.6rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.83rem; white-space:nowrap; }
        @media (max-width:768px){ .ex-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ex-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Examinations</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Examinations</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage exam schedules, mark entry, grading, invigilation and result publishing.</p>
            </div>
            <div class="flex gap-2">
                <asp:Button ID="btnPublish" runat="server" Text="Publish Results" CssClass="btn btn-secondary" Enabled="false" ToolTip="Available in the Results stage" />
                <a href="~/Modules/Examinations/CreateExamination.aspx" runat="server" class="btn btn-primary"><i data-lucide="plus" class="w-4 h-4"></i> Create Exam</a>
            </div>
        </div>

        <!-- Summary cards -->
        <div class="ex-sum">
            <div class="card ex-card"><div class="ex-ico" style="background:#DBEAFE;color:#2563EB"><i data-lucide="clipboard-list" class="w-5 h-5"></i></div>
                <div><p class="lbl">Total Exams</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div></div>
            <div class="card ex-card"><div class="ex-ico" style="background:#FEF3C7;color:#D97706"><i data-lucide="calendar-clock" class="w-5 h-5"></i></div>
                <div><p class="lbl">Upcoming Exams</p><p class="val"><asp:Literal ID="litUpcoming" runat="server" Text="0" /></p></div></div>
            <div class="card ex-card"><div class="ex-ico" style="background:#DCFCE7;color:#16A34A"><i data-lucide="check-circle" class="w-5 h-5"></i></div>
                <div><p class="lbl">Completed Exams</p><p class="val"><asp:Literal ID="litCompleted" runat="server" Text="0" /></p></div></div>
            <div class="card ex-card"><div class="ex-ico" style="background:#EDE9FE;color:#7C3AED"><i data-lucide="pencil" class="w-5 h-5"></i></div>
                <div><p class="lbl">Pending Mark Entry</p><p class="val"><asp:Literal ID="litPending" runat="server" Text="0" /></p></div></div>
            <div class="card ex-card"><div class="ex-ico" style="background:#CCFBF1;color:#0D9488"><i data-lucide="bar-chart-3" class="w-5 h-5"></i></div>
                <div><p class="lbl">Results Published</p><p class="val"><asp:Literal ID="litPublished" runat="server" Text="0" /></p></div></div>
        </div>

        <!-- Tabs -->
        <div class="ex-tabs">
            <a class="ex-tab active" href="~/Modules/Examinations/Examinations.aspx" runat="server"><i data-lucide="layout-dashboard" class="w-4 h-4"></i> Overview</a>
            <a class="ex-tab" href="~/Modules/Examinations/GradingScales.aspx" runat="server"><i data-lucide="award" class="w-4 h-4"></i> Grading Scales</a>
        </div>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-4 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="All Status" Value="" /><asp:ListItem Text="Draft" Value="Draft" />
                        <asp:ListItem Text="Scheduled" Value="Scheduled" /><asp:ListItem Text="Ongoing" Value="Ongoing" />
                        <asp:ListItem Text="Completed" Value="Completed" /><asp:ListItem Text="Published" Value="Published" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList></div>
                <div class="flex items-end gap-2">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="ex-grid">
            <!-- Exam list -->
            <div class="card overflow-hidden">
                <div class="card-head"><h2 class="text-sm font-bold">Examinations</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvExams" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="ExamName" HeaderText="Exam" />
                            <asp:BoundField DataField="ExamType" HeaderText="Type" />
                            <asp:BoundField DataField="TermName" HeaderText="Term" />
                            <asp:BoundField DataField="StartDate" HeaderText="Start" DataFormatString="{0:dd MMM yyyy}" />
                            <asp:BoundField DataField="SubjectCount" HeaderText="Subjects" />
                            <asp:TemplateField HeaderText="Status"><ItemTemplate>
                                <span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("Status"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Status"))) %></span>
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                                <a runat="server" href='<%# "~/Modules/Examinations/ExaminationDetails.aspx?id=" + Eval("ExamID") %>' class="text-brand-600 font-semibold text-xs">View</a>
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No examinations yet. Click “Create Exam” to add one.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- Side: rooms + upcoming -->
            <div class="space-y-4">
                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Exam Rooms</h2></div>
                    <div class="overflow-x-auto">
                        <asp:GridView ID="gvRooms" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                            <Columns>
                                <asp:BoundField DataField="RoomName" HeaderText="Room" />
                                <asp:BoundField DataField="Capacity" HeaderText="Capacity" />
                                <asp:BoundField DataField="Location" HeaderText="Location" />
                            </Columns>
                            <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No rooms.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Recent Activity</h2></div>
                    <div class="p-4">
                        <asp:Repeater ID="rptActivity" runat="server">
                            <ItemTemplate>
                                <div class="flex items-center justify-between py-1.5 border-b border-gray-50 text-sm">
                                    <span><%# Server.HtmlEncode(Convert.ToString(Eval("Activity"))) %>: <b><%# Server.HtmlEncode(Convert.ToString(Eval("ExamName"))) %></b></span>
                                    <span class="text-xs text-gray-400"><%# Eval("ActivityDate","{0:dd MMM}") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Panel ID="pnlNoActivity" runat="server" Visible="false"><p class="text-sm text-gray-400 text-center py-6">No exam activity yet.</p></asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
