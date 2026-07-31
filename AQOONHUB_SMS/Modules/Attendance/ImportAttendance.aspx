<%@ Page Title="Import Attendance | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ImportAttendance.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.ImportAttendance" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .im-wrap { padding:1.25rem; max-width:1400px; margin:0 auto; }
        .im-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .im-sum { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .im-sum { grid-template-columns:repeat(6,1fr); } }
        .im-card { padding:.85rem 1rem; } .im-card .lbl { font-size:.64rem; font-weight:700; text-transform:uppercase; color:#64748B; } .im-card .val { font-size:1.35rem; font-weight:800; }
        .drop { border:2px dashed #cbd5e1; border-radius:12px; padding:1.25rem; text-align:center; background:#f8fafc; }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.55rem .7rem; background:#f8fafc; text-align:left; font-size:.6rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.5rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.8rem; white-space:nowrap; }
        @media (max-width:768px){ .im-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="im-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Attendance/Attendance.aspx" runat="server" class="hover:text-brand-600">Attendance</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Import Attendance</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Import Attendance</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Upload, validate and import attendance records using a CSV file.</p>
            </div>
            <asp:Button ID="btnTemplate" runat="server" Text="Download Template" CssClass="btn btn-secondary" OnClick="btnTemplate_Click" CausesValidation="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Import configuration -->
        <div class="card p-4 mb-4">
            <h2 class="text-sm font-bold mb-3">Import Configuration</h2>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Session Type</label>
                    <asp:DropDownList ID="ddlSessionType" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSessionType_Changed">
                        <asp:ListItem Text="Daily" Value="Daily" /><asp:ListItem Text="Morning" Value="Morning" />
                        <asp:ListItem Text="Afternoon" Value="Afternoon" /><asp:ListItem Text="Subject" Value="Subject" />
                    </asp:DropDownList></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject</label><asp:DropDownList ID="ddlSubject" runat="server" CssClass="input" Enabled="false" /></div>
                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5">Attendance Date</label>
                    <asp:DropDownList ID="ddlDateMode" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlDateMode_Changed">
                        <asp:ListItem Text="From CSV" Value="csv" /><asp:ListItem Text="Fixed date" Value="fixed" />
                    </asp:DropDownList></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Fixed Date</label><asp:TextBox ID="txtFixedDate" runat="server" CssClass="input" TextMode="Date" Enabled="false" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Import Mode</label>
                    <asp:DropDownList ID="ddlMode" runat="server" CssClass="input">
                        <asp:ListItem Text="Create new sessions" Value="create" /><asp:ListItem Text="Update existing Draft" Value="update" />
                    </asp:DropDownList></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Import As</label>
                    <asp:DropDownList ID="ddlImportAs" runat="server" CssClass="input">
                        <asp:ListItem Text="Draft" Value="draft" /><asp:ListItem Text="Submitted" Value="submitted" />
                    </asp:DropDownList></div>
            </div>
        </div>

        <!-- File upload -->
        <div class="card p-4 mb-4">
            <h2 class="text-sm font-bold mb-3">Upload CSV</h2>
            <div class="drop mb-3">
                <p class="text-sm text-gray-600 mb-2">Choose a .csv file (max 2 MB, 5,000 rows).</p>
                <asp:FileUpload ID="fu" runat="server" CssClass="text-sm" />
            </div>
            <div class="flex flex-wrap gap-2">
                <asp:Button ID="btnPreview" runat="server" Text="Validate &amp; Preview" CssClass="btn btn-primary" OnClick="btnPreview_Click" CausesValidation="false" />
                <asp:Button ID="btnReset" runat="server" Text="Choose Another File" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
            </div>
            <ol class="list-decimal ml-5 mt-3 text-xs text-gray-500 space-y-1">
                <li>Download the template and fill the attendance data.</li>
                <li>The file must be in .csv format (UTF-8).</li>
                <li>Status values must be: Present, Absent, Late, Excused.</li>
            </ol>
        </div>

        <!-- Preview -->
        <asp:Panel ID="pnlPreview" runat="server" Visible="false">
            <div class="im-sum mb-4">
                <div class="card im-card"><p class="lbl">Total Rows</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div>
                <div class="card im-card"><p class="lbl">Valid</p><p class="val text-emerald-700"><asp:Literal ID="litValid" runat="server" Text="0" /></p></div>
                <div class="card im-card"><p class="lbl">Warnings</p><p class="val text-amber-700"><asp:Literal ID="litWarn" runat="server" Text="0" /></p></div>
                <div class="card im-card"><p class="lbl">Errors</p><p class="val text-red-700"><asp:Literal ID="litErr" runat="server" Text="0" /></p></div>
                <div class="card im-card"><p class="lbl">Sessions to Create</p><p class="val text-blue-700"><asp:Literal ID="litCreate" runat="server" Text="0" /></p></div>
                <div class="card im-card"><p class="lbl">Sessions to Update</p><p class="val text-indigo-700"><asp:Literal ID="litUpdate" runat="server" Text="0" /></p></div>
            </div>

            <div class="card overflow-hidden mb-4">
                <div class="card-head flex items-center justify-between"><h2 class="text-sm font-bold">Validation Preview</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                        <Columns>
                            <asp:BoundField DataField="RowNumber" HeaderText="Row" />
                            <asp:BoundField DataField="AttendanceDate" HeaderText="Date" />
                            <asp:BoundField DataField="StudentCode" HeaderText="Code" />
                            <asp:TemplateField HeaderText="Student"><ItemTemplate><%# Server.HtmlEncode(Convert.ToString(Eval("StudentName"))) %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="Status" HeaderText="Status" />
                            <asp:BoundField DataField="CheckInTime" HeaderText="Check-in" />
                            <asp:TemplateField HeaderText="Remarks"><ItemTemplate><%# Server.HtmlEncode(Convert.ToString(Eval("Remarks"))) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Validation"><ItemTemplate><span class="badge" style='<%# VStyle(Convert.ToString(Eval("Validation"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Validation"))) %></span></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Message"><ItemTemplate><span class="text-xs text-gray-600"><%# Server.HtmlEncode(Convert.ToString(Eval("Message"))) %></span></ItemTemplate></asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <div class="flex flex-wrap gap-2">
                <asp:Button ID="btnImport" runat="server" Text="Import Validated Data" CssClass="btn btn-primary" OnClick="btnImport_Click" CausesValidation="false" OnClientClick="return confirm('Import the validated attendance rows?');" />
                <asp:Button ID="btnBack" runat="server" Text="Back / Choose Another File" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
            </div>
        </asp:Panel>

        <!-- History -->
        <div class="card overflow-hidden mt-6">
            <div class="card-head"><h2 class="text-sm font-bold">Recent Imports</h2></div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                    <Columns>
                        <asp:BoundField DataField="ImportedAt" HeaderText="When" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                        <asp:BoundField DataField="OriginalFileName" HeaderText="File" />
                        <asp:BoundField DataField="ClassName" HeaderText="Class" />
                        <asp:BoundField DataField="SectionName" HeaderText="Section" />
                        <asp:BoundField DataField="ImportStatus" HeaderText="Status" />
                        <asp:BoundField DataField="ImportedSessions" HeaderText="Sessions" />
                        <asp:BoundField DataField="ImportedRecords" HeaderText="Records" />
                        <asp:BoundField DataField="ImportedByName" HeaderText="By" />
                    </Columns>
                    <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No imports yet.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
