<%@ Page Title="Dashboard | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Dashboard.Dashboard" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        /* ===== AQOONHUB Dashboard Styles ===== */
        :root {
            --brand-primary: #2563EB;
            --brand-secondary: #7C3AED;
            --success: #22C55E;
            --danger: #EF4444;
            --warning: #F59E0B;
            --info: #0EA5E9;
            --text-primary: #111827;
            --text-secondary: #6B7280;
            --bg-surface: #F8FAFC;
            --bg-card: #FFFFFF;
            --border-color: #E5E7EB;
            --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.04), 0 1px 3px 0 rgb(0 0 0 / 0.06);
            --shadow-md: 0 4px 6px -1px rgb(0 0 0 / 0.08), 0 2px 4px -2px rgb(0 0 0 / 0.06);
            --radius: 0.9rem;
            --radius-sm: 0.6rem;
        }

        .dashboard-container {
            padding: 1.5rem;
            max-width: 1440px;
            margin: 0 auto;
        }

        .dash-header {
            margin-bottom: 1.5rem;
        }
        .dash-header h1 {
            font-size: 1.5rem;
            font-weight: 800;
            color: var(--text-primary);
            letter-spacing: -0.025em;
            margin: 0;
        }
        .dash-header p {
            font-size: 0.875rem;
            color: var(--text-secondary);
            margin: 0.25rem 0 0 0;
        }
        .dash-header .breadcrumb {
            display: flex;
            align-items: center;
            gap: 0.375rem;
            font-size: 0.75rem;
            color: var(--text-secondary);
            margin-bottom: 0.5rem;
        }
        .dash-header .breadcrumb a {
            color: var(--text-secondary);
            text-decoration: none;
        }
        .dash-header .breadcrumb .current {
            font-weight: 600;
            color: var(--text-primary);
        }

        /* Stats Grid */
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(1, 1fr);
            gap: 1rem;
            margin-bottom: 1.5rem;
        }
        @media (min-width: 640px) {
            .stats-grid { grid-template-columns: repeat(2, 1fr); }
        }
        @media (min-width: 1024px) {
            .stats-grid { grid-template-columns: repeat(4, 1fr); }
        }

        .stat-card {
            background: var(--bg-card);
            border: 1px solid var(--border-color);
            border-radius: var(--radius);
            padding: 1.25rem;
            display: flex;
            align-items: flex-start;
            gap: 1rem;
            box-shadow: var(--shadow-sm);
            transition: all 0.2s ease;
        }
        .stat-card:hover {
            box-shadow: var(--shadow-md);
            transform: translateY(-2px);
        }

        .stat-icon {
            width: 3rem;
            height: 3rem;
            border-radius: var(--radius-sm);
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }
        .stat-icon svg {
            width: 1.25rem;
            height: 1.25rem;
        }

        .stat-icon.blue { background: #EFF6FF; color: #2563EB; }
        .stat-icon.green { background: #ECFDF5; color: #22C55E; }
        .stat-icon.purple { background: #F5F3FF; color: #7C3AED; }
        .stat-icon.orange { background: #FFFBEB; color: #F59E0B; }
        .stat-icon.red { background: #FEF2F2; color: #EF4444; }
        .stat-icon.cyan { background: #ECFEFF; color: #0EA5E9; }
        .stat-icon.indigo { background: #EEF2FF; color: #4F46E5; }

        .stat-content { min-width: 0; flex: 1; }
        .stat-label {
            font-size: 0.75rem;
            font-weight: 600;
            color: var(--text-secondary);
            text-transform: uppercase;
            letter-spacing: 0.05em;
            margin-bottom: 0.25rem;
        }
        .stat-value {
            font-size: 1.5rem;
            font-weight: 800;
            color: var(--text-primary);
            line-height: 1.2;
        }
        .stat-sub {
            font-size: 0.6875rem;
            color: var(--text-secondary);
            margin-top: 0.25rem;
        }

        /* Section Cards */
        .section-grid {
            display: grid;
            grid-template-columns: 1fr;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }
        @media (min-width: 1024px) {
            .section-grid.two-col { grid-template-columns: repeat(2, 1fr); }
            .section-grid.three-col { grid-template-columns: repeat(3, 1fr); }
        }

        .dash-card {
            background: var(--bg-card);
            border: 1px solid var(--border-color);
            border-radius: var(--radius);
            box-shadow: var(--shadow-sm);
            overflow: hidden;
        }
        .dash-card-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 1rem 1.25rem;
            border-bottom: 1px solid var(--border-color);
        }
        .dash-card-header h3 {
            font-size: 0.875rem;
            font-weight: 700;
            color: var(--text-primary);
            margin: 0;
        }
        .dash-card-header .card-action {
            font-size: 0.75rem;
            font-weight: 600;
            color: var(--brand-primary);
            text-decoration: none;
        }
        .dash-card-header .card-action:hover {
            opacity: 0.8;
        }
        .dash-card-body {
            padding: 1.25rem;
        }

        /* Mini Stats */
        .mini-stats {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 0.75rem;
        }
        @media (min-width: 640px) {
            .mini-stats { grid-template-columns: repeat(4, 1fr); }
        }
        .mini-stat {
            text-align: center;
            padding: 0.75rem;
            border-radius: var(--radius-sm);
            background: var(--bg-surface);
        }
        .mini-stat .value {
            font-size: 1.25rem;
            font-weight: 800;
            line-height: 1;
        }
        .mini-stat .label {
            font-size: 0.6875rem;
            font-weight: 600;
            color: var(--text-secondary);
            margin-top: 0.375rem;
            text-transform: uppercase;
        }

        /* Progress Bar */
        .progress-wrap { margin-top: 1rem; }
        .progress-label {
            display: flex;
            justify-content: space-between;
            font-size: 0.75rem;
            margin-bottom: 0.375rem;
        }
        .progress-label span:first-child {
            font-weight: 600;
            color: var(--text-primary);
        }
        .progress-label span:last-child {
            color: var(--text-secondary);
        }
        .progress-track {
            height: 0.5rem;
            background: #F3F4F6;
            border-radius: 999px;
            overflow: hidden;
        }
        .progress-fill {
            height: 100%;
            border-radius: 999px;
            transition: width 0.6s ease;
        }
        .progress-fill.green { background: var(--success); }
        .progress-fill.blue { background: var(--brand-primary); }
        .progress-fill.purple { background: var(--brand-secondary); }
        .progress-fill.orange { background: var(--warning); }
        .progress-fill.red { background: var(--danger); }

        /* Data Tables */
        .dash-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 0.8125rem;
        }
        .dash-table thead th {
            text-align: left;
            font-size: 0.6875rem;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.06em;
            color: var(--text-secondary);
            padding: 0.75rem 1rem;
            border-bottom: 1px solid var(--border-color);
            white-space: nowrap;
        }
        .dash-table tbody td {
            padding: 0.875rem 1rem;
            border-bottom: 1px solid #F1F5F9;
            color: var(--text-primary);
            vertical-align: middle;
        }
        .dash-table tbody tr:last-child td {
            border-bottom: none;
        }
        .dash-table tbody tr:hover {
            background: #F8FAFC;
        }

        /* Badges */
        .badge {
            display: inline-flex;
            align-items: center;
            gap: 0.25rem;
            font-size: 0.6875rem;
            font-weight: 600;
            padding: 0.2rem 0.6rem;
            border-radius: 999px;
            line-height: 1;
        }
        .badge-success { background: #DCFCE7; color: #15803D; }
        .badge-warning { background: #FEF3C7; color: #B45309; }
        .badge-danger { background: #FEE2E2; color: #B91C1C; }
        .badge-info { background: #E0F2FE; color: #0369A1; }
        .badge-primary { background: #EFF6FF; color: #1D4ED8; }
        .badge-secondary { background: #F1F5F9; color: #64748B; }

        /* Notifications */
        .notification-item {
            display: flex;
            align-items: flex-start;
            gap: 0.75rem;
            padding: 0.875rem;
            border-radius: var(--radius-sm);
            transition: background 0.15s;
        }
        .notification-item:hover {
            background: #F8FAFC;
        }
        .notification-item:not(:last-child) {
            border-bottom: 1px solid #F1F5F9;
        }
        .notif-icon {
            width: 2rem;
            height: 2rem;
            border-radius: var(--radius-sm);
            display: flex;
            align-items: center;
            justify-content: center;
            flex-shrink: 0;
        }
        .notif-icon svg {
            width: 0.875rem;
            height: 0.875rem;
        }
        .notif-content {
            flex: 1;
            min-width: 0;
        }
        .notif-title {
            font-size: 0.75rem;
            font-weight: 600;
            line-height: 1.4;
            color: var(--text-primary);
        }
        .notif-message {
            font-size: 0.6875rem;
            color: var(--text-secondary);
            margin-top: 0.125rem;
            line-height: 1.4;
        }
        .notif-meta {
            font-size: 0.625rem;
            color: #9CA3AF;
            margin-top: 0.25rem;
        }

        /* Empty State */
        .empty-state {
            text-align: center;
            padding: 2rem 1rem;
        }
        .empty-state svg {
            width: 2.5rem;
            height: 2.5rem;
            color: #D1D5DB;
            margin-bottom: 0.75rem;
        }
        .empty-state p {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--text-secondary);
        }

        /* Attendance Bar Cell */
        .att-bar-cell {
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }
        .att-bar-track {
            flex: 1;
            height: 0.5rem;
            background: #F3F4F6;
            border-radius: 999px;
            overflow: hidden;
        }
        .att-bar-fill {
            height: 100%;
            border-radius: 999px;
        }
        .att-bar-text {
            font-size: 0.6875rem;
            font-weight: 600;
            white-space: nowrap;
        }

        /* Responsive */
        @media (max-width: 768px) {
            .dashboard-container { padding: 1rem; }
            .dash-header h1 { font-size: 1.25rem; }
            .stat-value { font-size: 1.25rem; }
        }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="dashboard-container">

        <!-- Page Header -->
        <div class="dash-header">
            <nav class="breadcrumb">
                <a href="~/Default.aspx">Home</a>
                <span>/</span>
                <span class="current">Dashboard</span>
            </nav>
            <h1>Dashboard</h1>
            <p>Welcome back! Here is what is happening at AQOONHUB today.</p>
        </div>

        <!-- Top Statistics Cards -->
        <div class="stats-grid">
            <!-- Card 1: Total Students -->
            <div class="stat-card">
                <div class="stat-icon blue">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M4.26 10.147a60.438 60.438 0 0 0-.491 6.347A48.62 48.62 0 0 1 12 20.904a48.62 48.62 0 0 1 8.232-4.41 60.46 60.46 0 0 0-.491-6.347m-15.482 0a50.636 50.636 0 0 0-2.658-.813A59.906 59.906 0 0 1 12 3.493a59.903 59.903 0 0 1 10.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.717 50.717 0 0 1 12 13.489a50.702 50.702 0 0 1 7.74-3.342M6.75 15a.75.75 0 1 0 0-1.5.75.75 0 0 0 0 1.5Zm0 0v-3.675A55.378 55.378 0 0 1 12 8.443m-7.007 11.55A5.981 5.981 0 0 0 6.75 15.75v-1.5" />
                    </svg>
                </div>
                <div class="stat-content">
                    <div class="stat-label">Total Students</div>
                    <div class="stat-value"><asp:Label ID="lblTotalStudents" runat="server" Text="0" /></div>
                    <div class="stat-sub">Enrolled across all grades</div>
                </div>
            </div>

            <!-- Card 2: Active Students -->
            <div class="stat-card">
                <div class="stat-icon green">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75 11.25 15 15 9.75M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                    </svg>
                </div>
                <div class="stat-content">
                    <div class="stat-label">Active Students</div>
                    <div class="stat-value"><asp:Label ID="lblActiveStudents" runat="server" Text="0" /></div>
                    <div class="stat-sub">Currently attending</div>
                </div>
            </div>

            <!-- Card 3: Total Staff -->
            <div class="stat-card">
                <div class="stat-icon purple">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 14.15v4.25c0 1.094-.787 2.036-1.872 2.18-2.087.277-4.216.42-6.378.42s-4.291-.143-6.378-.42c-1.085-.144-1.872-1.086-1.872-2.18v-4.25m16.5 0a2.18 2.18 0 0 0 .75-1.661V8.706c0-1.081-.768-2.015-1.837-2.175a48.114 48.114 0 0 0-3.413-.387m4.5 4.006v5.325m-16.5 0a2.18 2.18 0 0 1 .75-1.661V8.706c0-1.081.768-2.015 1.837-2.175a48.111 48.111 0 0 1 3.413-.387m7.5 0V5.25a2.25 2.25 0 0 0-2.25-2.25h-1.5a2.25 2.25 0 0 0-2.25 2.25v.816m7.5 0a48.667 48.667 0 0 0-7.5 0" />
                    </svg>
                </div>
                <div class="stat-content">
                    <div class="stat-label">Total Staff</div>
                    <div class="stat-value"><asp:Label ID="lblTotalStaff" runat="server" Text="0" /></div>
                    <div class="stat-sub">Teaching and non-teaching</div>
                </div>
            </div>

            <!-- Card 4: Fee Collection -->
            <div class="stat-card">
                <div class="stat-icon orange">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 18.75a60.07 60.07 0 0 1 15.797 2.101c.727.198 1.453-.342 1.453-1.096V18.75M3.75 4.5v.75A.75.75 0 0 1 3 6h-.75m0 0v-.375c0-.621.504-1.125 1.125-1.125H20.25M2.25 6v9m18-10.5v.75c0 .414.336.75.75.75h.75m-1.5-1.5h.375c.621 0 1.125.504 1.125 1.125v9.75c0 .621-.504 1.125-1.125 1.125h-.375m1.5-1.5H21a.75.75 0 0 0-.75.75v.75m0 0H3.75m0 0h-.375a1.125 1.125 0 0 1-1.125-1.125V15m1.5 1.5v-.75A.75.75 0 0 0 3 15h-.75M15 10.5a3 3 0 1 1-6 0 3 3 0 0 1 6 0Zm3 0h.008v.008H18V10.5Zm-12 0h.008v.008H6V10.5Z" />
                    </svg>
                </div>
                <div class="stat-content">
                    <div class="stat-label">Fee Collection</div>
                    <div class="stat-value"><asp:Label ID="lblFeeCollection" runat="server" Text="$0.00" /></div>
                    <div class="stat-sub">Total collected this term</div>
                </div>
            </div>
        </div>

        <!-- Middle Section: Attendance + Finance + Academic -->
        <div class="section-grid three-col">

            <!-- Attendance Section -->
            <div class="dash-card">
                <div class="dash-card-header">
                    <h3>Today's Attendance</h3>
                    <asp:HyperLink ID="lnkAttendance" runat="server" NavigateUrl="~/Modules/Attendance/Attendance.aspx" CssClass="card-action">View All</asp:HyperLink>
                </div>
                <div class="dash-card-body">
                    <div class="mini-stats">
                        <div class="mini-stat">
                            <div class="value" style="color: var(--success);"><asp:Label ID="lblPresentToday" runat="server" Text="0" /></div>
                            <div class="label">Present</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--danger);"><asp:Label ID="lblAbsentToday" runat="server" Text="0" /></div>
                            <div class="label">Absent</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--warning);"><asp:Label ID="lblLateToday" runat="server" Text="0" /></div>
                            <div class="label">Late</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--brand-primary);"><asp:Label ID="lblAttendanceRate" runat="server" Text="0%" /></div>
                            <div class="label">Rate</div>
                        </div>
                    </div>
                    <div class="progress-wrap">
                        <div class="progress-label">
                            <span>Attendance Rate</span>
                            <span><asp:Label ID="lblAttendanceRateBar" runat="server" Text="0%" /></span>
                        </div>
                        <div class="progress-track">
                            <asp:Panel ID="attendanceProgress" runat="server" CssClass="progress-fill green" Style="width: 0%;"></asp:Panel>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Finance Section -->
            <div class="dash-card">
                <div class="dash-card-header">
                    <h3>Finance Overview</h3>
                    <asp:HyperLink ID="lnkFinance" runat="server" NavigateUrl="~/Modules/Finance/Finance.aspx" CssClass="card-action">Details</asp:HyperLink>
                </div>
                <div class="dash-card-body">
                    <div class="mini-stats" style="grid-template-columns: repeat(3, 1fr);">
                        <div class="mini-stat">
                            <div class="value" style="color: var(--brand-primary);"><asp:Label ID="lblTotalBilled" runat="server" Text="$0.00" /></div>
                            <div class="label">Billed</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--success);"><asp:Label ID="lblTotalCollected" runat="server" Text="$0.00" /></div>
                            <div class="label">Collected</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--danger);"><asp:Label ID="lblTotalOutstanding" runat="server" Text="$0.00" /></div>
                            <div class="label">Outstanding</div>
                        </div>
                    </div>
                    <div class="progress-wrap">
                        <div class="progress-label">
                            <span>Collection Rate</span>
                            <span><asp:Label ID="lblCollectionRate" runat="server" Text="0%" /></span>
                        </div>
                        <div class="progress-track">
                            <asp:Panel ID="collectionProgress" runat="server" CssClass="progress-fill blue" Style="width: 0%;"></asp:Panel>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Academic Section -->
            <div class="dash-card">
                <div class="dash-card-header">
                    <h3>Academic Overview</h3>
                    <asp:HyperLink ID="lnkAcademic" runat="server" NavigateUrl="~/Modules/Academic/Academic.aspx" CssClass="card-action">View All</asp:HyperLink>
                </div>
                <div class="dash-card-body">
                    <div class="mini-stats">
                        <div class="mini-stat">
                            <div class="value" style="color: var(--brand-secondary);"><asp:Label ID="lblUpcomingExams" runat="server" Text="0" /></div>
                            <div class="label">Upcoming Exams</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--info);"><asp:Label ID="lblActiveExams" runat="server" Text="0" /></div>
                            <div class="label">Active Exams</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--warning);"><asp:Label ID="lblPendingApplications" runat="server" Text="0" /></div>
                            <div class="label">Pending Apps</div>
                        </div>
                        <div class="mini-stat">
                            <div class="value" style="color: var(--success);"><asp:Label ID="lblCurrentTerm" runat="server" Text="-" /></div>
                            <div class="label">Current Term</div>
                        </div>
                    </div>
                    <div class="progress-wrap">
                        <div class="progress-label">
                            <span>Term Progress</span>
                            <span><asp:Label ID="lblTermProgress" runat="server" Text="0%" /></span>
                        </div>
                        <div class="progress-track">
                            <asp:Panel ID="termProgressBar" runat="server" CssClass="progress-fill purple" Style="width: 0%;"></asp:Panel>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Bottom Section: Tables + Notifications -->
        <div class="section-grid two-col">

            <!-- Left Column: Recent Activities + Attendance by Class -->
            <div style="display: flex; flex-direction: column; gap: 1rem;">

                <!-- Recent Activities -->
                <div class="dash-card">
                    <div class="dash-card-header">
                        <h3>Recent Activities</h3>
                        <asp:HyperLink ID="lnkAuditLog" runat="server" NavigateUrl="~/Modules/System/AuditLog.aspx" CssClass="card-action">View All</asp:HyperLink>
                    </div>
                    <div class="dash-card-body" style="padding: 0;">
                        <asp:GridView ID="gvRecentActivities" runat="server" AutoGenerateColumns="false" 
                            CssClass="dash-table" GridLines="None" ShowHeader="true">
                            <Columns>
                                <asp:TemplateField HeaderText="Type">
                                    <ItemTemplate>
                                        <asp:Label ID="lblActivityBadge" runat="server" 
                                            Text='<%# Eval("ActivityType") %>'
                                            CssClass='<%# GetActivityBadgeClass(Eval("ActivityType")) %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Description" HeaderText="Description" />
                                <asp:BoundField DataField="ActivityDate" HeaderText="Date" DataFormatString="{0:MMM dd, yyyy HH:mm}" />
                                <asp:BoundField DataField="PerformedBy" HeaderText="By" />
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="empty-state">
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z" />
                                    </svg>
                                    <p>No recent activities</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

                <!-- Attendance by Class -->
                <div class="dash-card">
                    <div class="dash-card-header">
                        <h3>Attendance by Class</h3>
                        <asp:HyperLink ID="lnkAttendanceByClass" runat="server" NavigateUrl="~/Modules/Attendance/Attendance.aspx" CssClass="card-action">View All</asp:HyperLink>
                    </div>
                    <div class="dash-card-body" style="padding: 0;">
                        <asp:GridView ID="gvAttendanceByClass" runat="server" AutoGenerateColumns="false" 
                            CssClass="dash-table" GridLines="None" ShowHeader="true"
                            OnRowDataBound="gvAttendanceByClass_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ClassName" HeaderText="Class" />
                                <asp:BoundField DataField="TotalStudents" HeaderText="Total" />
                                <asp:BoundField DataField="PresentCount" HeaderText="Present" />
                                <asp:BoundField DataField="AbsentCount" HeaderText="Absent" />
                                <asp:BoundField DataField="LateCount" HeaderText="Late" />
                                <asp:TemplateField HeaderText="Rate">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="hdnRate" runat="server" Value='<%# Eval("AttendanceRate") %>' />
                                        <div class="att-bar-cell">
                                            <div class="att-bar-track">
                                                <asp:Panel ID="pnlAttBar" runat="server" CssClass="att-bar-fill" Style="width: 0%;"></asp:Panel>
                                            </div>
                                            <span class="att-bar-text">
                                                <asp:Label ID="lblAttRate" runat="server" Text='<%# Eval("AttendanceRate") + "%" %>'></asp:Label>
                                            </span>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="empty-state">
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                        <path stroke-linecap="round" stroke-linejoin="round" d="M15 19.128a9.38 9.38 0 0 0 2.625.372 9.337 9.337 0 0 0 4.121-.952 4.125 4.125 0 0 0-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 0 1 8.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0 1 11.964-3.07M12 6.375a3.375 3.375 0 1 1-6.75 0 3.375 3.375 0 0 1 6.75 0Zm8.25 2.25a2.625 2.625 0 1 1-5.25 0 2.625 2.625 0 0 1 5.25 0Z" />
                                    </svg>
                                    <p>No attendance data available</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>

            <!-- Right Column: Notifications -->
            <div class="dash-card">
                <div class="dash-card-header">
                    <h3>Notifications</h3>
                    <asp:HyperLink ID="lnkNotifications" runat="server" NavigateUrl="~/Modules/Communication/Notifications.aspx" CssClass="card-action">View All</asp:HyperLink>
                </div>
                <div class="dash-card-body" style="padding: 0;">
                    <asp:ListView ID="lvNotifications" runat="server">
                        <LayoutTemplate>
                            <div>
                                <asp:PlaceHolder ID="itemPlaceholder" runat="server" />
                            </div>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <div class="notification-item">
                                <div class="notif-icon <%# GetNotificationIconClass(Eval("Priority")) %>">
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                        <%# GetNotificationIcon(Eval("Priority")) %>
                                    </svg>
                                </div>
                                <div class="notif-content">
                                    <div class="notif-title"><%# Eval("Title") %></div>
                                    <div class="notif-message"><%# Eval("Message") %></div>
                                    <div class="notif-meta">
                                        <span class="badge <%# GetNotificationBadgeClass(Eval("Priority")) %>"><%# Eval("Priority") %></span>
                                        <span><%# Eval("CreatedAt", "{0:MMM dd, yyyy HH:mm}") %></span>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                        <EmptyDataTemplate>
                            <div class="empty-state">
                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                                    <path stroke-linecap="round" stroke-linejoin="round" d="M14.857 17.082a23.848 23.848 0 0 0 5.454-1.31A8.967 8.967 0 0 1 18 9.75V9A6 6 0 0 0 6 9v.75a8.967 8.967 0 0 1-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 0 1-5.714 0m5.714 0a3 3 0 1 1-5.714 0" />
                                </svg>
                                <p>No new notifications</p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:ListView>
                </div>
            </div>
        </div>

    </div>
</asp:Content>