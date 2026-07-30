<%@ Page Title="Grading Scales | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="GradingScales.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.GradingScales" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .gs-wrap { padding:1.25rem; max-width:1200px; margin:0 auto; }
        .gs-table { width:100%; border-collapse:collapse; }
        .gs-table th { padding:.7rem 1rem; background:#f8fafc; text-align:left; font-size:.66rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .gs-table td { padding:.7rem 1rem; border-bottom:1px solid #f1f5f9; font-size:.85rem; white-space:nowrap; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:30px; height:30px; border-radius:8px; color:#64748B; }
        .ico-btn:hover { background:#EFF6FF; color:#2563EB; }
        @media (max-width:768px){ .gs-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="gs-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Grading Scales</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Grading Scales</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Define grade letters, percentage ranges and pass rules per academic year.</p>
            </div>
            <asp:Button ID="btnNew" runat="server" Text="+ Add Grade" CssClass="btn btn-primary" OnClick="btnNew_Click" CausesValidation="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYearFilter" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYearFilter_Changed" /></div>
            </div>
        </div>

        <!-- Add/Edit form -->
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="card p-5 mb-5">
            <h2 class="text-base font-bold mb-4"><asp:Literal ID="litFormTitle" runat="server" Text="Add Grade" /></h2>
            <asp:HiddenField ID="hfId" runat="server" Value="0" />
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Grade Name <span class="text-red-500">*</span></label><asp:TextBox ID="txtLetter" runat="server" CssClass="input" placeholder="e.g. A+" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlYearForm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Grade Point (GPA)</label><asp:TextBox ID="txtGpa" runat="server" CssClass="input" TextMode="Number" Text="0" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Min % <span class="text-red-500">*</span></label><asp:TextBox ID="txtMin" runat="server" CssClass="input" TextMode="Number" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Max % <span class="text-red-500">*</span></label><asp:TextBox ID="txtMax" runat="server" CssClass="input" TextMode="Number" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Result</label>
                    <asp:DropDownList ID="ddlPass" runat="server" CssClass="input">
                        <asp:ListItem Text="Pass" Value="1" /><asp:ListItem Text="Fail" Value="0" />
                    </asp:DropDownList></div>
                <div class="md:col-span-2"><label class="block text-xs font-bold text-slate-700 mb-1.5">Description</label><asp:TextBox ID="txtDesc" runat="server" CssClass="input" placeholder="e.g. Outstanding" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="Inactive" Value="Inactive" />
                    </asp:DropDownList></div>
            </div>
            <div class="flex justify-end gap-2 mt-5">
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                <asp:Button ID="btnSave" runat="server" Text="Save Grade" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            </div>
        </asp:Panel>

        <!-- Table -->
        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="gs-table" OnRowCommand="gv_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Grade"><ItemTemplate><span class="font-bold"><%# Server.HtmlEncode(Convert.ToString(Eval("GradeLetter"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Range"><ItemTemplate><%# Eval("MinMarks") %> – <%# Eval("MaxMarks") %>%</ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="GPA" HeaderText="GPA" DataFormatString="{0:0.00}" />
                        <asp:BoundField DataField="Description" HeaderText="Description" />
                        <asp:TemplateField HeaderText="Result"><ItemTemplate>
                            <span class="badge" style='<%# Convert.ToBoolean(Eval("IsPass")) ? "background:#DCFCE7;color:#15803D" : "background:#FEE2E2;color:#DC2626" %>'><%# Convert.ToBoolean(Eval("IsPass")) ? "Pass" : "Fail" %></span>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Status"><ItemTemplate>
                            <span class="badge" style='<%# string.Equals(Convert.ToString(Eval("Status")),"Active",StringComparison.OrdinalIgnoreCase) ? "background:#DCFCE7;color:#15803D" : "background:#FEF3C7;color:#B45309" %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Status"))) %></span>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditRow" CommandArgument='<%# Eval("GradeID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No grades for the selected year.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
