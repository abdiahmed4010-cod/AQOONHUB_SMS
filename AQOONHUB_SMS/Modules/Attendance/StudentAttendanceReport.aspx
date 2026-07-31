<%@ Page Title="Student Attendance Report | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="StudentAttendanceReport.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.StudentAttendanceReport" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .sr-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .sr-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .sr-sum { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .sr-sum { grid-template-columns:repeat(6,1fr); } }
        .sr-card { padding:.9rem 1rem; } .sr-card .lbl { font-size:.66rem; font-weight:700; text-transform:uppercase; color:#64748B; } .sr-card .val { font-size:1.4rem; font-weight:800; }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.55rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; white-space:nowrap; }
        @media print { body * { visibility:hidden; } #printArea, #printArea * { visibility:visible; } #printArea { position:absolute; left:0; top:0; width:100%; } .no-print { display:none !important; } @page { size:A4 portrait; margin:12mm; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="sr-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5 no-print">
            <a href="~/Modules/Attendance/Attendance.aspx" runat="server" class="hover:text-brand-600">Attendance</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Student Report</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Student Attendance Report</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Attendance history for a selected student across a date range.</p>
            </div>
            <div class="flex gap-2 no-print">
                <asp:Button ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-secondary" OnClientClick="window.print();return false;" CausesValidation="false" />
                <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn btn-secondary" OnClick="btnExport_Click" CausesValidation="false" />
            </div>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm no-print"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <div class="card p-4 mb-4 no-print">
            <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-8 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSection_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Student</label><asp:DropDownList ID="ddlStudent" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">From</label><asp:TextBox ID="txtFrom" runat="server" CssClass="input" TextMode="Date" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">To</label><asp:TextBox ID="txtTo" runat="server" CssClass="input" TextMode="Date" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Session Type</label>
                    <asp:DropDownList ID="ddlSessionType" runat="server" CssClass="input">
                        <asp:ListItem Text="All" Value="" /><asp:ListItem Text="Daily" Value="Daily" /><asp:ListItem Text="Morning" Value="Morning" />
                        <asp:ListItem Text="Afternoon" Value="Afternoon" /><asp:ListItem Text="Subject" Value="Subject" />
                    </asp:DropDownList></div>
            </div>
            <div class="flex items-end gap-2 mt-3">
                <asp:Button ID="btnView" runat="server" Text="View" CssClass="btn btn-primary" OnClick="btnView_Click" CausesValidation="false" />
                <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
            </div>
        </div>

        <div id="printArea">
            <div class="mb-3"><div class="hidden print:block text-lg font-bold">Student Attendance Report</div><asp:Literal ID="litHeader" runat="server" /></div>

            <div class="sr-sum mb-4">
                <div class="card sr-card"><p class="lbl">Total Sessions</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div>
                <div class="card sr-card"><p class="lbl">Present</p><p class="val text-emerald-700"><asp:Literal ID="litP" runat="server" Text="0" /></p></div>
                <div class="card sr-card"><p class="lbl">Absent</p><p class="val text-red-700"><asp:Literal ID="litA" runat="server" Text="0" /></p></div>
                <div class="card sr-card"><p class="lbl">Late</p><p class="val text-amber-700"><asp:Literal ID="litL" runat="server" Text="0" /></p></div>
                <div class="card sr-card"><p class="lbl">Excused</p><p class="val text-violet-700"><asp:Literal ID="litE" runat="server" Text="0" /></p></div>
                <div class="card sr-card"><p class="lbl">Attendance %</p><p class="val text-indigo-700"><asp:Literal ID="litPct" runat="server" Text="0%" /></p></div>
            </div>

            <div class="card overflow-hidden">
                <div class="overflow-x-auto">
                    <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="AttendanceDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy}" />
                            <asp:BoundField DataField="YearName" HeaderText="Year" />
                            <asp:BoundField DataField="ClassName" HeaderText="Class" />
                            <asp:BoundField DataField="SectionName" HeaderText="Section" />
                            <asp:BoundField DataField="SubjectName" HeaderText="Subject" />
                            <asp:BoundField DataField="SessionType" HeaderText="Type" />
                            <asp:TemplateField HeaderText="Status"><ItemTemplate><span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("AttendanceStatus"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("AttendanceStatus"))) %></span></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Check-in"><ItemTemplate><%# FormatTime(Eval("CheckInTime")) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Late Min"><ItemTemplate><%# Eval("LateMinutes") is System.DBNull ? "—" : Eval("LateMinutes") %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Remarks"><ItemTemplate><%# Server.HtmlEncode(Convert.ToString(Eval("Remarks"))) %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="MarkedByName" HeaderText="Marked By" />
                        </Columns>
                        <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">Select a student and date range, then click “View”.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
