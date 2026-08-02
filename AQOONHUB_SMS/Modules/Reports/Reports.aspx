<%@ Page Title="Reports | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Reports.Reports" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .rp-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .rp-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .rp-sum { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .rp-sum { grid-template-columns:repeat(6,1fr); } }
        .rp-card { padding:.9rem 1rem; } .rp-card .lbl { font-size:.64rem; font-weight:700; text-transform:uppercase; color:#64748B; letter-spacing:.03em; } .rp-card .val { font-size:1.5rem; font-weight:800; line-height:1.05; }
        .rp-grid { display:grid; grid-template-columns:1fr; gap:1rem; } @media (min-width:1100px){ .rp-grid { grid-template-columns:2fr 1fr; } }
        .rp-cats { display:grid; grid-template-columns:repeat(2,1fr); gap:.75rem; } @media (min-width:768px){ .rp-cats { grid-template-columns:repeat(3,1fr); } } @media (min-width:1200px){ .rp-cats { grid-template-columns:repeat(4,1fr); } }
        .cat { display:flex; align-items:center; gap:.6rem; padding:.75rem .85rem; border:1px solid #e5e7eb; border-radius:10px; text-decoration:none; color:#0f172a; background:#fff; }
        .cat:hover { border-color:#2563EB; background:#f8fafc; } .cat .ic { width:34px; height:34px; border-radius:9px; display:flex; align-items:center; justify-content:center; background:#EEF2FF; color:#4338CA; flex:none; }
        .cat.disabled { opacity:.55; cursor:not-allowed; } .cat .nm { font-size:.82rem; font-weight:600; }
        .tbl { width:100%; border-collapse:collapse; } .tbl th { padding:.55rem .7rem; background:#f8fafc; text-align:left; font-size:.6rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; } .tbl td { padding:.55rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.8rem; white-space:nowrap; }
        .chartbox { position:relative; height:230px; }
        .empty { padding:2.25rem 1rem; text-align:center; color:#94a3b8; font-size:.82rem; }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rp-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Reports</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Reports</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Generate, analyse, print and export school reports.</p>
            </div>
            <div class="flex gap-2">
                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-secondary" OnClick="btnRefresh_Click" CausesValidation="false" />
                <asp:HyperLink ID="lnkCustom" runat="server" NavigateUrl="~/Modules/Reports/CustomReportBuilder.aspx" CssClass="btn btn-primary" Visible="false"><i data-lucide="plus" class="w-4 h-4"></i> Create Custom Report</asp:HyperLink>
            </div>
        </div>

        <!-- Summary cards -->
        <div class="rp-sum mb-4">
            <div class="card rp-card"><p class="lbl">Total Generated</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div>
            <div class="card rp-card"><p class="lbl">Generated Today</p><p class="val text-blue-700"><asp:Literal ID="litToday" runat="server" Text="0" /></p></div>
            <div class="card rp-card"><p class="lbl">Saved Reports</p><p class="val text-indigo-700"><asp:Literal ID="litSaved" runat="server" Text="0" /></p></div>
            <div class="card rp-card"><p class="lbl">Scheduled</p><p class="val text-teal-700"><asp:Literal ID="litScheduled" runat="server" Text="0" /></p></div>
            <div class="card rp-card"><p class="lbl">Recent Exports</p><p class="val text-emerald-700"><asp:Literal ID="litExports" runat="server" Text="0" /></p></div>
            <div class="card rp-card"><p class="lbl">Report Categories</p><p class="val text-violet-700"><asp:Literal ID="litCategories" runat="server" Text="0" /></p></div>
        </div>

        <div class="rp-grid mb-4">
            <!-- Monthly generation chart -->
            <div class="card p-4">
                <div class="card-head border-0 p-0 mb-2"><h2 class="text-sm font-bold">Monthly Report Generation</h2></div>
                <asp:Panel ID="pnlChart" runat="server" Visible="false"><div class="chartbox"><canvas id="cMonthly"></canvas></div></asp:Panel>
                <asp:Panel ID="pnlChartEmpty" runat="server"><div class="empty">No report generation history yet. Charts appear once reports are generated.</div></asp:Panel>
            </div>
            <!-- Most used -->
            <div class="card overflow-hidden">
                <div class="card-head"><h2 class="text-sm font-bold">Most Used Reports</h2></div>
                <asp:Repeater ID="rptMostUsed" runat="server"><ItemTemplate>
                    <div class="flex items-center justify-between px-4 py-2 border-b border-gray-50 text-sm">
                        <span><%# AQOONHUB_SMS.Modules.Reports.ReportUi.Enc(Eval("ReportName")) %></span><b class="text-gray-500"><%# Eval("Uses") %></b>
                    </div>
                </ItemTemplate></asp:Repeater>
                <asp:Panel ID="pnlMostUsedEmpty" runat="server"><div class="empty">No usage recorded yet.</div></asp:Panel>
            </div>
        </div>

        <!-- Report Categories -->
        <div class="card p-4 mb-4">
            <div class="card-head border-0 p-0 mb-3"><h2 class="text-sm font-bold">Report Categories</h2></div>
            <div class="rp-cats"><asp:Literal ID="litCategoryCards" runat="server" /></div>
        </div>

        <div class="rp-grid mb-4">
            <!-- Recent activity -->
            <div class="card overflow-hidden">
                <div class="card-head"><h2 class="text-sm font-bold">Recent Report Activity</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvActivity" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="CreatedAt" HeaderText="When" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                            <asp:BoundField DataField="UserName" HeaderText="User" />
                            <asp:BoundField DataField="Action" HeaderText="Action" />
                            <asp:BoundField DataField="ReportName" HeaderText="Report" />
                        </Columns>
                        <EmptyDataTemplate><div class="empty">No report activity yet.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
            <!-- Data source status -->
            <div class="card overflow-hidden">
                <div class="card-head"><h2 class="text-sm font-bold">Data Source Status</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvSources" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="Source" HeaderText="Source" />
                            <asp:TemplateField HeaderText="Status"><ItemTemplate>
                                <span class="badge" style='<%# Convert.ToBoolean(Eval("Available")) ? "background:#DCFCE7;color:#15803D" : "background:#F1F5F9;color:#64748B" %>'><%# Convert.ToBoolean(Eval("Available")) ? "Available" : "Unavailable" %></span>
                            </ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="Rows" HeaderText="Records" DataFormatString="{0:#,##0}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <div class="rp-grid">
            <!-- Recent exports -->
            <div class="card overflow-hidden">
                <div class="card-head"><h2 class="text-sm font-bold">Recent Exports</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvExports" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="GeneratedAt" HeaderText="When" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                            <asp:BoundField DataField="ReportName" HeaderText="Report" />
                            <asp:BoundField DataField="ExportFormat" HeaderText="Format" />
                            <asp:BoundField DataField="GeneratedByName" HeaderText="By" />
                        </Columns>
                        <EmptyDataTemplate><div class="empty">No exports yet.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
            <!-- Scheduled preview -->
            <div class="card overflow-hidden">
                <div class="card-head"><h2 class="text-sm font-bold">Scheduled Reports</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvScheduled" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="ReportName" HeaderText="Report" />
                            <asp:BoundField DataField="Frequency" HeaderText="Frequency" />
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                        </Columns>
                        <EmptyDataTemplate><div class="empty">No scheduled reports.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <script src='<%= ResolveUrl("~/Assets/js/plugins/chart.js?v=4.4.7") %>'></script>
    <asp:Literal ID="litChartData" runat="server" />
    <script>
        (function () {
            if (typeof Chart === 'undefined' || !window.RP) return;
            var el = document.getElementById('cMonthly');
            if (el) new Chart(el, { type: 'line', data: { labels: window.RP.labels, datasets: [{ label: 'Reports', data: window.RP.data, borderColor: '#2563EB', backgroundColor: 'rgba(37,99,235,.1)', fill: true, tension: .3 }] }, options: { responsive: true, maintainAspectRatio: false } });
        })();
    </script>
</asp:Content>
