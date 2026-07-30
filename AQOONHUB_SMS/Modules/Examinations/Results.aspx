<%@ Page Title="Results & Report Cards | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Results.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.Results" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .rs-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .rs-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:768px){ .rs-sum { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1200px){ .rs-sum { grid-template-columns:repeat(6,1fr); } }
        .rs-card { padding:.9rem 1rem; } .rs-card .lbl { font-size:.66rem; font-weight:700; text-transform:uppercase; color:#64748B; letter-spacing:.03em; } .rs-card .val { font-size:1.4rem; font-weight:800; line-height:1.05; }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.6rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; white-space:nowrap; }
        @media (max-width:768px){ .rs-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rs-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Results</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Results &amp; Report Cards</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Calculate, review and publish student examination results.</p>
            </div>
            <div class="flex flex-wrap gap-2">
                <asp:Button ID="btnCalculate" runat="server" Text="Calculate Results" CssClass="btn btn-secondary" OnClick="btnCalculate_Click" CausesValidation="false" />
                <asp:Button ID="btnExport" runat="server" Text="Export CSV" CssClass="btn btn-secondary" OnClick="btnExport_Click" CausesValidation="false" />
                <asp:Button ID="btnPublish" runat="server" Text="Publish Results" CssClass="btn btn-primary" OnClick="btnPublish_Click" CausesValidation="false" OnClientClick="return confirm('Publish results for this examination?');" />
                <asp:Button ID="btnUnpublish" runat="server" Text="Unpublish" CssClass="btn btn-secondary" OnClick="btnUnpublish_Click" CausesValidation="false" Visible="false" OnClientClick="return confirm('Unpublish results? They will be hidden from students/parents.');" />
            </div>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Summary -->
        <div class="rs-sum mb-4">
            <div class="card rs-card"><p class="lbl">Students Evaluated</p><p class="val"><asp:Literal ID="litEvaluated" runat="server" Text="0" /></p></div>
            <div class="card rs-card"><p class="lbl">Results Ready</p><p class="val text-blue-700"><asp:Literal ID="litReady" runat="server" Text="0" /></p></div>
            <div class="card rs-card"><p class="lbl">Published</p><p class="val text-teal-700"><asp:Literal ID="litPublished" runat="server" Text="No" /></p></div>
            <div class="card rs-card"><p class="lbl">Pass Rate</p><p class="val text-emerald-700"><asp:Literal ID="litPassRate" runat="server" Text="0%" /></p></div>
            <div class="card rs-card"><p class="lbl">Top Average</p><p class="val text-indigo-700"><asp:Literal ID="litTop" runat="server" Text="—" /></p></div>
            <div class="card rs-card"><p class="lbl">Incomplete</p><p class="val text-amber-700"><asp:Literal ID="litIncomplete" runat="server" Text="0" /></p></div>
        </div>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-2 md:grid-cols-6 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlTerm_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Examination</label><asp:DropDownList ID="ddlExam" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlExam_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="All" Value="" /><asp:ListItem Text="Passed" Value="Passed" /><asp:ListItem Text="Failed" Value="Failed" />
                        <asp:ListItem Text="Incomplete" Value="Incomplete" /><asp:ListItem Text="Absent" Value="Absent" /><asp:ListItem Text="Withheld" Value="Withheld" />
                    </asp:DropDownList></div>
                <div class="flex items-end gap-2">
                    <asp:Button ID="btnFilter" runat="server" Text="View" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="card overflow-hidden">
            <div class="card-head"><h2 class="text-sm font-bold">Results Overview</h2></div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                    <Columns>
                        <asp:TemplateField HeaderText="Student"><ItemTemplate>
                            <div class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("FullName"))) %></div>
                            <div class="text-xs text-gray-500"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentCode"))) %></div>
                        </ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="ClassName" HeaderText="Class" />
                        <asp:BoundField DataField="SectionName" HeaderText="Section" />
                        <asp:TemplateField HeaderText="Total"><ItemTemplate><%# Eval("TotalObtained") %> / <%# Eval("TotalMaximum") %></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Average"><ItemTemplate><%# Convert.ToDecimal(Eval("Average")).ToString("0.00") %>%</ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Grade"><ItemTemplate><span class="badge" style="background:#EEF2FF;color:#3730A3"><%# string.IsNullOrEmpty(Convert.ToString(Eval("Grade"))) ? "—" : Server.HtmlEncode(Convert.ToString(Eval("Grade"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Rank"><ItemTemplate><%# Convert.ToInt32(Eval("Rank"))>0 ? Convert.ToString(Eval("Rank")) : "—" %></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Result"><ItemTemplate><span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("ResultStatus"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("ResultStatus"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <a runat="server" href='<%# "~/Modules/Examinations/ReportCard.aspx?exam=" + CurrentExamId + "&student=" + Eval("StudentID") %>' class="text-brand-600 font-semibold text-xs">View / Report Card</a>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">Select an examination and click “Calculate Results”.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
