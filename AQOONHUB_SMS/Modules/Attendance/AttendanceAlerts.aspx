<%@ Page Title="Attendance Alerts | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AttendanceAlerts.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.AttendanceAlerts" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .al-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .al-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; } @media (min-width:900px){ .al-sum { grid-template-columns:repeat(5,1fr); } }
        .al-card { padding:.85rem 1rem; } .al-card .lbl { font-size:.62rem; font-weight:700; text-transform:uppercase; color:#64748B; } .al-card .val { font-size:1.4rem; font-weight:800; }
        .tbl { width:100%; border-collapse:collapse; } .tbl th { padding:.55rem .7rem; background:#f8fafc; text-align:left; font-size:.6rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; } .tbl td { padding:.55rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.8rem; }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="al-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Attendance/Attendance.aspx" runat="server" class="hover:text-brand-600">Attendance</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Alerts</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Attendance Alerts</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Rule-based alerts derived from submitted attendance data.</p>
            </div>
            <asp:Button ID="btnGenerate" runat="server" Text="Regenerate Alerts" CssClass="btn btn-primary" OnClick="btnGenerate_Click" CausesValidation="false" Visible="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <div class="al-sum mb-4">
            <div class="card al-card"><p class="lbl">Total</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div>
            <div class="card al-card"><p class="lbl">New</p><p class="val text-red-700"><asp:Literal ID="litNew" runat="server" Text="0" /></p></div>
            <div class="card al-card"><p class="lbl">Reviewed</p><p class="val text-amber-700"><asp:Literal ID="litReviewed" runat="server" Text="0" /></p></div>
            <div class="card al-card"><p class="lbl">Resolved</p><p class="val text-emerald-700"><asp:Literal ID="litResolved" runat="server" Text="0" /></p></div>
            <div class="card al-card"><p class="lbl">Critical Active</p><p class="val text-red-700"><asp:Literal ID="litCritical" runat="server" Text="0" /></p></div>
        </div>

        <div class="card p-4 mb-4">
            <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Type</label>
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="input">
                        <asp:ListItem Text="All Types" Value="" /><asp:ListItem Text="Consecutive Absence" Value="ConsecutiveAbsence" /><asp:ListItem Text="Low Attendance" Value="LowAttendance" />
                        <asp:ListItem Text="Frequent Late" Value="FrequentLate" /><asp:ListItem Text="Unsubmitted Session" Value="UnsubmittedSession" />
                    </asp:DropDownList></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="All" Value="" /><asp:ListItem Text="New" Value="New" /><asp:ListItem Text="Reviewed" Value="Reviewed" /><asp:ListItem Text="Resolved" Value="Resolved" /><asp:ListItem Text="Dismissed" Value="Dismissed" />
                    </asp:DropDownList></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Severity</label>
                    <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="input">
                        <asp:ListItem Text="All" Value="" /><asp:ListItem Text="Critical" Value="Critical" /><asp:ListItem Text="Warning" Value="Warning" /><asp:ListItem Text="Info" Value="Info" />
                    </asp:DropDownList></div>
                <div class="flex items-end gap-2"><asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" /></div>
            </div>
        </div>

        <div class="card overflow-hidden mb-4">
            <div class="overflow-x-auto">
                <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl" DataKeyNames="AttendanceAlertID" OnRowCommand="gv_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Severity"><ItemTemplate><span class="badge" style='<%# SevStyle(Convert.ToString(Eval("Severity"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Severity"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="AlertType" HeaderText="Type" />
                        <asp:TemplateField HeaderText="Student / Scope"><ItemTemplate>
                            <div class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentName"))) %><%# Server.HtmlEncode(Convert.ToString(Eval("ClassName"))) %></div>
                            <div class="text-xs text-gray-500"><%# Server.HtmlEncode(Convert.ToString(Eval("Description"))) %></div>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Status"><ItemTemplate><span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("Status"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Status"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="LastDetectedAt" HeaderText="Detected" DataFormatString="{0:dd MMM yyyy}" />
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <asp:LinkButton ID="lb1" runat="server" CommandName="Review" CommandArgument='<%# Eval("AttendanceAlertID") %>' CssClass="text-brand-600 text-xs font-semibold mr-2" Visible='<%# CanManage %>'>Review</asp:LinkButton>
                            <asp:LinkButton ID="lb2" runat="server" CommandName="Resolve" CommandArgument='<%# Eval("AttendanceAlertID") %>' CssClass="text-emerald-700 text-xs font-semibold mr-2" Visible='<%# CanManage %>'>Resolve</asp:LinkButton>
                            <asp:LinkButton ID="lb3" runat="server" CommandName="Dismiss" CommandArgument='<%# Eval("AttendanceAlertID") %>' CssClass="text-gray-500 text-xs font-semibold" Visible='<%# CanManage %>'>Dismiss</asp:LinkButton>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No alerts. Click “Regenerate Alerts” to compute from current attendance.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <asp:Panel ID="pnlResolve" runat="server" Visible="false" CssClass="card p-4">
            <h2 class="text-sm font-bold mb-2">Resolve Alert #<asp:Literal ID="litResolveId" runat="server" /></h2>
            <label class="block text-xs font-bold text-slate-700 mb-1.5">Resolution Notes</label>
            <asp:TextBox ID="txtNotes" runat="server" CssClass="input w-full" TextMode="MultiLine" Rows="3" />
            <div class="flex gap-2 mt-3">
                <asp:Button ID="btnResolveConfirm" runat="server" Text="Confirm Resolve" CssClass="btn btn-primary" OnClick="btnResolveConfirm_Click" CausesValidation="false" />
                <asp:Button ID="btnResolveCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnResolveCancel_Click" CausesValidation="false" />
            </div>
            <asp:HiddenField ID="hfResolveId" runat="server" />
        </asp:Panel>
    </div>
</asp:Content>
