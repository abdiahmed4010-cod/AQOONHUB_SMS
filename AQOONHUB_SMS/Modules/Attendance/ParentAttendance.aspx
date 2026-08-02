<%@ Page Title="My Children's Attendance | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ParentAttendance.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.ParentAttendance" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .pa-wrap { padding:1.25rem; max-width:1200px; margin:0 auto; }
        .pa-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; } @media (min-width:768px){ .pa-sum { grid-template-columns:repeat(6,1fr); } }
        .pa-card { padding:.85rem 1rem; } .pa-card .lbl { font-size:.62rem; font-weight:700; text-transform:uppercase; color:#64748B; } .pa-card .val { font-size:1.35rem; font-weight:800; }
        .tbl { width:100%; border-collapse:collapse; } .tbl th { padding:.55rem .7rem; background:#f8fafc; text-align:left; font-size:.6rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; } .tbl td { padding:.55rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.8rem; white-space:nowrap; }
        .cal { display:grid; grid-template-columns:repeat(7,1fr); gap:5px; } .cal .hd { text-align:center; font-size:.62rem; font-weight:700; color:#64748B; } .cell { min-height:52px; border:1px solid #e5e7eb; border-radius:8px; padding:.3rem; font-size:.68rem; } .cell.empty { border:none; } .dot { display:inline-block; width:8px; height:8px; border-radius:50%; }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="pa-wrap">
        <div class="mb-4">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">My Children's Attendance</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">View published attendance for your linked children.</p>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <asp:Panel ID="pnlNoChildren" runat="server" Visible="false" CssClass="card p-10 text-center text-gray-500">
            No linked children were found for your account. Please contact the school office.
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server" Visible="false">
            <div class="card p-4 mb-4 flex flex-wrap items-end gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Child</label><asp:DropDownList ID="ddlChild" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlChild_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Month</label><asp:TextBox ID="txtMonth" runat="server" CssClass="input" TextMode="Month" AutoPostBack="true" OnTextChanged="txtMonth_Changed" /></div>
                <div class="text-sm text-gray-600"><asp:Literal ID="litChildInfo" runat="server" /></div>
            </div>

            <div class="pa-sum mb-4">
                <div class="card pa-card"><p class="lbl">Attendance %</p><p class="val text-indigo-700"><asp:Literal ID="litPct" runat="server" Text="0%" /></p></div>
                <div class="card pa-card"><p class="lbl">Sessions</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div>
                <div class="card pa-card"><p class="lbl">Present</p><p class="val text-emerald-700"><asp:Literal ID="litP" runat="server" Text="0" /></p></div>
                <div class="card pa-card"><p class="lbl">Absent</p><p class="val text-red-700"><asp:Literal ID="litA" runat="server" Text="0" /></p></div>
                <div class="card pa-card"><p class="lbl">Late</p><p class="val text-amber-700"><asp:Literal ID="litL" runat="server" Text="0" /></p></div>
                <div class="card pa-card"><p class="lbl">Excused</p><p class="val text-violet-700"><asp:Literal ID="litE" runat="server" Text="0" /></p></div>
            </div>

            <asp:Panel ID="pnlAlerts" runat="server" Visible="false" CssClass="card p-4 mb-4">
                <h2 class="text-sm font-bold mb-2">Attendance Notices</h2>
                <asp:Repeater ID="rptAlerts" runat="server"><ItemTemplate>
                    <div class="flex items-center gap-2 py-1.5 border-b border-gray-50 text-sm">
                        <span class="badge" style='<%# Convert.ToString(Eval("Severity"))=="Critical" ? "background:#FEE2E2;color:#DC2626" : "background:#FEF3C7;color:#B45309" %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Severity"))) %></span>
                        <span><%# Server.HtmlEncode(Convert.ToString(Eval("Description"))) %></span>
                    </div>
                </ItemTemplate></asp:Repeater>
            </asp:Panel>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
                <div class="card overflow-hidden"><div class="card-head"><h2 class="text-sm font-bold">Recent Attendance</h2></div><div class="overflow-x-auto">
                    <asp:GridView ID="gvRecent" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="AttendanceDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy}" />
                            <asp:BoundField DataField="SessionType" HeaderText="Type" />
                            <asp:TemplateField HeaderText="Status"><ItemTemplate><span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("AttendanceStatus"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("AttendanceStatus"))) %></span></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Check-in"><ItemTemplate><%# FormatTime(Eval("CheckInTime")) %></ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No attendance records yet.</div></EmptyDataTemplate>
                    </asp:GridView></div></div>

                <div class="card p-4"><h2 class="text-sm font-bold mb-2">Attendance Calendar</h2>
                    <div class="cal mb-1"><div class="hd">Mon</div><div class="hd">Tue</div><div class="hd">Wed</div><div class="hd">Thu</div><div class="hd">Fri</div><div class="hd">Sat</div><div class="hd">Sun</div></div>
                    <asp:Literal ID="litCalendar" runat="server" />
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
