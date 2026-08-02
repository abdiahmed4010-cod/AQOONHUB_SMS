<%@ Page Title="Report Viewer | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ReportViewer.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Reports.ReportViewer" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .rv-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .tbl { width:100%; border-collapse:collapse; } .tbl th { padding:.55rem .75rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; } .tbl td { padding:.5rem .75rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; white-space:nowrap; }
        .rv-filters { display:grid; grid-template-columns:repeat(2,1fr); gap:.75rem; } @media (min-width:768px){ .rv-filters { grid-template-columns:repeat(4,1fr); } } @media (min-width:1200px){ .rv-filters { grid-template-columns:repeat(6,1fr); } }
        .empty { padding:2.5rem 1rem; text-align:center; color:#94a3b8; font-size:.85rem; }
        @media print {
            body * { visibility:hidden; }
            #rvPrint, #rvPrint * { visibility:visible; }
            #rvPrint { position:absolute; left:0; top:0; width:100%; }
            .no-print { display:none !important; }
        }
    </style>
    <asp:Literal ID="litPageCss" runat="server" />
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rv-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5 no-print">
            <a href="~/Modules/Reports/Reports.aspx" runat="server" class="hover:text-brand-600">Reports</a>
            <span>/</span><asp:HyperLink ID="lnkCategory" runat="server" CssClass="hover:text-brand-600" /><span>/</span>
            <span class="font-semibold text-gray-700 dark:text-slate-200"><asp:Literal ID="litCrumb" runat="server" /></span>
        </nav>

        <asp:Panel ID="pnlInvalid" runat="server" Visible="false" CssClass="card p-10 text-center text-gray-500">
            <p class="text-lg font-semibold mb-1">Report unavailable</p>
            <p class="text-sm"><asp:Literal ID="litInvalid" runat="server" /></p>
            <a href="~/Modules/Reports/Reports.aspx" runat="server" class="btn btn-secondary mt-4 inline-flex">Back to Reports</a>
        </asp:Panel>

        <asp:Panel ID="pnlReport" runat="server" Visible="false">
            <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
                <div>
                    <h1 class="text-xl md:text-2xl font-bold tracking-tight"><asp:Literal ID="litTitle" runat="server" /></h1>
                    <p class="text-sm text-gray-500 dark:text-slate-400 mt-1"><asp:Literal ID="litDesc" runat="server" /></p>
                </div>
                <div class="flex gap-2 no-print">
                    <asp:Button ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-secondary" OnClientClick="window.print();return false;" CausesValidation="false" />
                    <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn btn-secondary" OnClick="btnExport_Click" CausesValidation="false" />
                    <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-primary">Back to Category</asp:HyperLink>
                </div>
            </div>

            <asp:Panel ID="pnlFilters" runat="server" CssClass="card p-4 mb-4 no-print">
                <div class="rv-filters">
                    <asp:Panel ID="fYear" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></asp:Panel>
                    <asp:Panel ID="fTerm" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fClass" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></asp:Panel>
                    <asp:Panel ID="fSection" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSection_Changed" /></asp:Panel>
                    <asp:Panel ID="fSubject" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject</label><asp:DropDownList ID="ddlSubject" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fExam" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Examination</label><asp:DropDownList ID="ddlExam" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fPeriod" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Pay Period</label><asp:DropDownList ID="ddlPeriod" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fTeacher" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Teacher</label><asp:DropDownList ID="ddlTeacher" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fStudent" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Student</label><asp:DropDownList ID="ddlStudent" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fDept" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Department</label><asp:DropDownList ID="ddlDept" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fGender" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Gender</label>
                        <asp:DropDownList ID="ddlGender" runat="server" CssClass="input"><asp:ListItem Text="All" Value="" /><asp:ListItem Text="Male" Value="Male" /><asp:ListItem Text="Female" Value="Female" /></asp:DropDownList></asp:Panel>
                    <asp:Panel ID="fStatus" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label><asp:DropDownList ID="ddlStatus" runat="server" CssClass="input" /></asp:Panel>
                    <asp:Panel ID="fFrom" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">From</label><asp:TextBox ID="txtFrom" runat="server" CssClass="input" TextMode="Date" /></asp:Panel>
                    <asp:Panel ID="fTo" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">To</label><asp:TextBox ID="txtTo" runat="server" CssClass="input" TextMode="Date" /></asp:Panel>
                    <asp:Panel ID="fSearch" runat="server" Visible="false"><label class="block text-xs font-bold text-slate-700 mb-1.5">Search</label><asp:TextBox ID="txtSearch" runat="server" CssClass="input" /></asp:Panel>
                </div>
                <div class="flex items-end gap-2 mt-3">
                    <asp:Button ID="btnApply" runat="server" Text="Apply Filters" CssClass="btn btn-primary" OnClick="btnApply_Click" CausesValidation="false" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </asp:Panel>

            <div id="rvPrint">
                <div class="mb-2">
                    <div class="hidden print:block text-lg font-bold"><asp:Literal ID="litPrintTitle" runat="server" /></div>
                    <asp:Literal ID="litSummary" runat="server" />
                </div>
                <div class="card overflow-hidden">
                    <div class="overflow-x-auto">
                        <asp:GridView ID="gv" runat="server" AutoGenerateColumns="true" GridLines="None" CssClass="tbl"
                            AllowPaging="true" PageSize="50" OnPageIndexChanging="gv_PageIndexChanging" HeaderStyle-CssClass="th"
                            EnableViewState="true">
                            <EmptyDataTemplate><div class="empty">No records match the selected filters.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
