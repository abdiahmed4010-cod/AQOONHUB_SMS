<%@ Page Title="Attendance Settings | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AttendanceSettings.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.AttendanceSettings" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .st-wrap { padding:1.25rem; max-width:900px; margin:0 auto; }
        .st-row { display:flex; align-items:center; justify-content:space-between; gap:1rem; padding:.85rem 0; border-bottom:1px solid #f1f5f9; }
        .st-row:last-child { border-bottom:none; }
        .st-row .k { font-size:.86rem; font-weight:600; color:#334155; }
        .st-row .h { font-size:.72rem; color:#94a3b8; margin-top:.15rem; }
        .st-in { width:150px; }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="st-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Attendance/Attendance.aspx" runat="server" class="hover:text-brand-600">Attendance</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Settings</span>
        </nav>
        <div class="mb-4">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Attendance Settings</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Configure marking rules, the attendance-rate policy and alert thresholds.</p>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <asp:Panel ID="pnlReadOnly" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm bg-amber-50 text-amber-800 border border-amber-200">
            You have view-only access. Only management roles can change attendance settings.
        </asp:Panel>

        <div class="card p-5 mb-4">
            <h2 class="text-sm font-bold mb-2">Marking Rules</h2>
            <div class="st-row"><div><div class="k">Allow Teachers to Mark Attendance</div><div class="h">Teachers may mark their assigned class/section only.</div></div><asp:CheckBox ID="chkAllowTeachers" runat="server" /></div>
            <div class="st-row"><div><div class="k">Allow Editing After Submission</div><div class="h">Managers can reopen; teachers cannot casually edit.</div></div><asp:CheckBox ID="chkAllowEdit" runat="server" /></div>
            <div class="st-row"><div><div class="k">Edit Window (Hours)</div><div class="h">How long an edit is permitted after marking.</div></div><asp:TextBox ID="txtEditWindow" runat="server" CssClass="input st-in" TextMode="Number" /></div>
            <div class="st-row"><div><div class="k">Attendance Start Time</div></div><asp:TextBox ID="txtStart" runat="server" CssClass="input st-in" TextMode="Time" /></div>
            <div class="st-row"><div><div class="k">Attendance End Time</div></div><asp:TextBox ID="txtEnd" runat="server" CssClass="input st-in" TextMode="Time" /></div>
            <div class="st-row"><div><div class="k">Late After (Minutes)</div><div class="h">Arrivals past start-time + this many minutes count as Late.</div></div><asp:TextBox ID="txtLateAfter" runat="server" CssClass="input st-in" TextMode="Number" /></div>
            <div class="st-row"><div><div class="k">Excused Requires Remarks</div></div><asp:CheckBox ID="chkExcusedRemarks" runat="server" /></div>
            <div class="st-row"><div><div class="k">Allow Future-Date Attendance</div><div class="h">Off by default; marking future dates is normally rejected.</div></div><asp:CheckBox ID="chkFuture" runat="server" /></div>
        </div>

        <div class="card p-5 mb-4">
            <h2 class="text-sm font-bold mb-2">Attendance-Rate Policy</h2>
            <div class="st-row"><div><div class="k">Include Late as Attended</div><div class="h">Late still counts toward attendance but is tracked separately.</div></div><asp:CheckBox ID="chkIncludeLate" runat="server" /></div>
            <div class="st-row"><div><div class="k">Exclude Excused from Attendance Rate</div><div class="h">Excused records removed from the rate denominator.</div></div><asp:CheckBox ID="chkExcludeExcused" runat="server" /></div>
        </div>

        <div class="card p-5 mb-4">
            <h2 class="text-sm font-bold mb-2">Alerts &amp; Notifications</h2>
            <div class="st-row"><div><div class="k">Consecutive Absence Alert (days)</div></div><asp:TextBox ID="txtConsecutive" runat="server" CssClass="input st-in" TextMode="Number" /></div>
            <div class="st-row"><div><div class="k">Low Attendance Threshold (%)</div></div><asp:TextBox ID="txtLowThreshold" runat="server" CssClass="input st-in" TextMode="Number" /></div>
            <div class="rounded-lg p-3 my-3 text-xs bg-slate-50 text-slate-600 border border-slate-200">
                No email/SMS provider is configured in this build. These preferences are stored but notifications are <b>not</b> sent until a provider is integrated.
            </div>
            <div class="st-row"><div><div class="k">Enable Parent Notifications</div></div><asp:CheckBox ID="chkParent" runat="server" /></div>
            <div class="st-row"><div><div class="k">Enable Email Notifications</div></div><asp:CheckBox ID="chkEmail" runat="server" /></div>
            <div class="st-row"><div><div class="k">Enable SMS Notifications</div></div><asp:CheckBox ID="chkSms" runat="server" /></div>
        </div>

        <div class="flex gap-2">
            <asp:Button ID="btnSave" runat="server" Text="Save Settings" CssClass="btn btn-primary" OnClick="btnSave_Click" CausesValidation="false" />
            <asp:HyperLink ID="lnkBack" runat="server" NavigateUrl="~/Modules/Attendance/Attendance.aspx" CssClass="btn btn-secondary">Cancel</asp:HyperLink>
        </div>
    </div>
</asp:Content>
