<%@ Page Title="Attendance Calendar | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AttendanceCalendar.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.AttendanceCalendar" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ca-wrap { padding:1.25rem; max-width:1400px; margin:0 auto; }
        .cal { display:grid; grid-template-columns:repeat(7,1fr); gap:6px; }
        .cal .hd { text-align:center; font-size:.66rem; font-weight:700; text-transform:uppercase; color:#64748B; padding:.35rem 0; }
        .cell { min-height:74px; border:1px solid #e5e7eb; border-radius:10px; padding:.4rem; font-size:.72rem; position:relative; background:#fff; }
        .cell .dn { font-weight:700; color:#334155; }
        .cell.empty { background:transparent; border:none; }
        .cell a { text-decoration:none; color:inherit; display:block; height:100%; }
        .dot { display:inline-block; width:9px; height:9px; border-radius:50%; margin-right:3px; }
        .legend { display:flex; flex-wrap:wrap; gap:.9rem; font-size:.72rem; color:#475569; }
        @media (max-width:640px){ .cell { min-height:58px; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ca-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Attendance/Attendance.aspx" runat="server" class="hover:text-brand-600">Attendance</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Calendar</span>
        </nav>
        <div class="mb-4">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Attendance Calendar</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Monthly attendance overview for a class/section or an individual student.</p>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <div class="card p-4 mb-4">
            <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Month</label><asp:TextBox ID="txtMonth" runat="server" CssClass="input" TextMode="Month" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSection_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Student (optional)</label><asp:DropDownList ID="ddlStudent" runat="server" CssClass="input" /></div>
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

        <div class="card p-4 mb-3">
            <div class="legend">
                <span><span class="dot" style="background:#16A34A"></span>Present / Good</span>
                <span><span class="dot" style="background:#DC2626"></span>Absent / Low</span>
                <span><span class="dot" style="background:#D97706"></span>Late</span>
                <span><span class="dot" style="background:#7C3AED"></span>Excused</span>
                <span><span class="dot" style="background:#3B82F6"></span>Mixed</span>
                <span><span class="dot" style="background:#e5e7eb"></span>No Session</span>
            </div>
        </div>

        <div class="card p-4">
            <div class="cal mb-2">
                <div class="hd">Mon</div><div class="hd">Tue</div><div class="hd">Wed</div><div class="hd">Thu</div><div class="hd">Fri</div><div class="hd">Sat</div><div class="hd">Sun</div>
            </div>
            <asp:Literal ID="litCalendar" runat="server" />
        </div>
    </div>
</asp:Content>
