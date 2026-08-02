<%@ Page Title="Attendance Analytics | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AttendanceAnalytics.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.AttendanceAnalytics" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .an-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .an-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .an-sum { grid-template-columns:repeat(4,1fr); } }
        @media (min-width:1200px){ .an-sum { grid-template-columns:repeat(8,1fr); } }
        .an-card { padding:.85rem 1rem; } .an-card .lbl { font-size:.62rem; font-weight:700; text-transform:uppercase; color:#64748B; } .an-card .val { font-size:1.3rem; font-weight:800; }
        .an-grid { display:grid; grid-template-columns:1fr; gap:1rem; } @media (min-width:1000px){ .an-grid { grid-template-columns:1fr 1fr; } }
        .tbl { width:100%; border-collapse:collapse; } .tbl th { padding:.5rem .7rem; background:#f8fafc; text-align:left; font-size:.6rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; } .tbl td { padding:.5rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.8rem; white-space:nowrap; }
        .chartbox { position:relative; height:260px; }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="an-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Attendance/Attendance.aspx" runat="server" class="hover:text-brand-600">Attendance</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Analytics</span>
        </nav>
        <div class="mb-4">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Attendance Analytics</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Analyse attendance trends, identify risks and compare class performance.</p>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <div class="card p-4 mb-4">
            <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-8 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">From</label><asp:TextBox ID="txtFrom" runat="server" CssClass="input" TextMode="Date" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">To</label><asp:TextBox ID="txtTo" runat="server" CssClass="input" TextMode="Date" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Session Type</label>
                    <asp:DropDownList ID="ddlSessionType" runat="server" CssClass="input">
                        <asp:ListItem Text="All" Value="" /><asp:ListItem Text="Daily" Value="Daily" /><asp:ListItem Text="Morning" Value="Morning" /><asp:ListItem Text="Afternoon" Value="Afternoon" /><asp:ListItem Text="Subject" Value="Subject" />
                    </asp:DropDownList></div>
                <div class="flex items-end gap-2">
                    <asp:Button ID="btnView" runat="server" Text="Apply" CssClass="btn btn-primary" OnClick="btnView_Click" CausesValidation="false" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="an-sum mb-4">
            <div class="card an-card"><p class="lbl">Attendance Rate</p><p class="val text-indigo-700"><asp:Literal ID="litRate" runat="server" Text="0%" /></p></div>
            <div class="card an-card"><p class="lbl">Total Sessions</p><p class="val"><asp:Literal ID="litSessions" runat="server" Text="0" /></p></div>
            <div class="card an-card"><p class="lbl">Students</p><p class="val"><asp:Literal ID="litStudents" runat="server" Text="0" /></p></div>
            <div class="card an-card"><p class="lbl">Present</p><p class="val text-emerald-700"><asp:Literal ID="litP" runat="server" Text="0" /></p></div>
            <div class="card an-card"><p class="lbl">Absent</p><p class="val text-red-700"><asp:Literal ID="litA" runat="server" Text="0" /></p></div>
            <div class="card an-card"><p class="lbl">Late</p><p class="val text-amber-700"><asp:Literal ID="litL" runat="server" Text="0" /></p></div>
            <div class="card an-card"><p class="lbl">Excused</p><p class="val text-violet-700"><asp:Literal ID="litE" runat="server" Text="0" /></p></div>
            <div class="card an-card"><p class="lbl">At-Risk</p><p class="val text-red-700"><asp:Literal ID="litRisk" runat="server" Text="0" /></p></div>
        </div>

        <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="card p-10 text-center text-gray-500">
            No submitted attendance found for the selected filters.
        </asp:Panel>

        <asp:Panel ID="pnlData" runat="server" Visible="false">
            <div class="an-grid mb-4">
                <div class="card p-4"><h2 class="text-sm font-bold mb-2">Status Breakdown</h2><div class="chartbox"><canvas id="cBreak"></canvas></div></div>
                <div class="card p-4"><h2 class="text-sm font-bold mb-2">Weekly Attendance Trend</h2><div class="chartbox"><canvas id="cTrend"></canvas></div></div>
                <div class="card p-4"><h2 class="text-sm font-bold mb-2">Attendance by Class</h2><div class="chartbox"><canvas id="cClass"></canvas></div></div>
                <div class="card p-4"><h2 class="text-sm font-bold mb-2">Monthly Trend</h2><div class="chartbox"><canvas id="cMonth"></canvas></div></div>
            </div>

            <div class="an-grid">
                <div class="card overflow-hidden"><div class="card-head"><h2 class="text-sm font-bold">Top Attendance</h2></div><div class="overflow-x-auto">
                    <asp:GridView ID="gvTop" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns><asp:BoundField DataField="FullName" HeaderText="Student" /><asp:BoundField DataField="TotalSessions" HeaderText="Sessions" />
                            <asp:TemplateField HeaderText="%"><ItemTemplate><%# Convert.ToDecimal(Eval("Percentage")).ToString("0.0") %>%</ItemTemplate></asp:TemplateField></Columns>
                        <EmptyDataTemplate><div class="py-6 text-center text-sm text-gray-500">No data.</div></EmptyDataTemplate></asp:GridView></div></div>
                <div class="card overflow-hidden"><div class="card-head"><h2 class="text-sm font-bold">Most Absent</h2></div><div class="overflow-x-auto">
                    <asp:GridView ID="gvAbsent" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns><asp:BoundField DataField="FullName" HeaderText="Student" /><asp:BoundField DataField="Absent" HeaderText="Absent" />
                            <asp:TemplateField HeaderText="%"><ItemTemplate><%# Convert.ToDecimal(Eval("Percentage")).ToString("0.0") %>%</ItemTemplate></asp:TemplateField></Columns>
                        <EmptyDataTemplate><div class="py-6 text-center text-sm text-gray-500">No data.</div></EmptyDataTemplate></asp:GridView></div></div>
                <div class="card overflow-hidden"><div class="card-head"><h2 class="text-sm font-bold">Frequent Late</h2></div><div class="overflow-x-auto">
                    <asp:GridView ID="gvLate" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns><asp:BoundField DataField="FullName" HeaderText="Student" /><asp:BoundField DataField="Late" HeaderText="Late" /><asp:BoundField DataField="LateMinutes" HeaderText="Late Min" /></Columns>
                        <EmptyDataTemplate><div class="py-6 text-center text-sm text-gray-500">No data.</div></EmptyDataTemplate></asp:GridView></div></div>
                <div class="card overflow-hidden"><div class="card-head"><h2 class="text-sm font-bold">At-Risk Students</h2></div><div class="overflow-x-auto">
                    <asp:GridView ID="gvRisk" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns><asp:BoundField DataField="FullName" HeaderText="Student" /><asp:BoundField DataField="TotalSessions" HeaderText="Sessions" />
                            <asp:TemplateField HeaderText="%"><ItemTemplate><span class="badge" style="background:#FEE2E2;color:#DC2626"><%# Convert.ToDecimal(Eval("Percentage")).ToString("0.0") %>%</span></ItemTemplate></asp:TemplateField></Columns>
                        <EmptyDataTemplate><div class="py-6 text-center text-sm text-gray-500">No at-risk students.</div></EmptyDataTemplate></asp:GridView></div></div>
            </div>
        </asp:Panel>
    </div>

    <script src='<%= ResolveUrl("~/Assets/js/plugins/chart.js") %>'></script>
    <asp:Literal ID="litChartData" runat="server" />
    <script>
        (function () {
            if (typeof Chart === 'undefined' || !window.AN) return;
            var d = window.AN;
            function mk(id, cfg) { var el = document.getElementById(id); if (el) new Chart(el, cfg); }
            mk('cBreak', { type: 'doughnut', data: { labels: d.breakLabels, datasets: [{ data: d.breakData, backgroundColor: ['#16A34A', '#DC2626', '#D97706', '#7C3AED'] }] }, options: { responsive: true, maintainAspectRatio: false } });
            mk('cTrend', { type: 'line', data: { labels: d.trendLabels, datasets: [{ label: 'Rate %', data: d.trendData, borderColor: '#2563EB', backgroundColor: 'rgba(37,99,235,.1)', fill: true, tension: .3 }] }, options: { responsive: true, maintainAspectRatio: false, scales: { y: { min: 0, max: 100 } } } });
            mk('cClass', { type: 'bar', data: { labels: d.classLabels, datasets: [{ label: 'Rate %', data: d.classData, backgroundColor: '#0D9488' }] }, options: { responsive: true, maintainAspectRatio: false, scales: { y: { min: 0, max: 100 } } } });
            mk('cMonth', { type: 'line', data: { labels: d.monthLabels, datasets: [{ label: 'Rate %', data: d.monthData, borderColor: '#7C3AED', backgroundColor: 'rgba(124,58,237,.1)', fill: true, tension: .3 }] }, options: { responsive: true, maintainAspectRatio: false, scales: { y: { min: 0, max: 100 } } } });
        })();
    </script>
</asp:Content>
