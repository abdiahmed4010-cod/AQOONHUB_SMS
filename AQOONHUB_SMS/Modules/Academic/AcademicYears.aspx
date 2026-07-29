<%@ Page Title="Academic Years | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="AcademicYears.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Academic.AcademicYears" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ay-wrap { padding:1.25rem; max-width:1400px; margin:0 auto; }
        .ay-table { width:100%; border-collapse:collapse; }
        .ay-table th { padding:.7rem 1rem; background:#f8fafc; text-align:left; font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .ay-table td { padding:.7rem 1rem; border-bottom:1px solid #f1f5f9; font-size:.85rem; white-space:nowrap; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:30px; height:30px; border-radius:8px; color:#64748B; }
        .ico-btn:hover { background:#EFF6FF; color:#2563EB; }
        @media (max-width:768px){ .ay-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ay-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Academic/Academics.aspx" runat="server" class="hover:text-brand-600">Academics</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Academic Years</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Academic Years</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage academic years, their duration and status.</p>
            </div>
            <asp:Button ID="btnNew" runat="server" Text="+ Add Academic Year" CssClass="btn btn-primary" OnClick="btnNew_Click" CausesValidation="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm">
            <asp:Literal ID="msgText" runat="server" />
        </asp:Panel>

        <!-- Add / Edit form -->
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="card p-5 mb-5">
            <h2 class="text-base font-bold mb-4"><asp:Literal ID="litFormTitle" runat="server" Text="Add Academic Year" /></h2>
            <asp:HiddenField ID="hfId" runat="server" Value="0" />
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year <span class="text-red-500">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="input" placeholder="e.g. 2026-2027" />
                </div>
                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5">Description</label>
                    <asp:TextBox ID="txtDesc" runat="server" CssClass="input" placeholder="e.g. Current Academic Year" />
                </div>
                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5">Start Date <span class="text-red-500">*</span></label>
                    <asp:TextBox ID="txtStart" runat="server" CssClass="input" TextMode="Date" />
                </div>
                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5">End Date <span class="text-red-500">*</span></label>
                    <asp:TextBox ID="txtEnd" runat="server" CssClass="input" TextMode="Date" />
                </div>
                <div>
                    <label class="block text-xs font-bold text-slate-700 mb-1.5">Status <span class="text-red-500">*</span></label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="Draft" Value="Draft" />
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Completed" Value="Completed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList>
                </div>
            </div>
            <div class="flex justify-end gap-2 mt-5">
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                <asp:Button ID="btnSave" runat="server" Text="Save Academic Year" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            </div>
        </asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Search</label>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search academic year..." /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="All Status" Value="" />
                        <asp:ListItem Text="Draft" Value="Draft" />
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Completed" Value="Completed" />
                        <asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList></div>
                <div class="flex items-end"><asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary w-full justify-center" OnClick="btnFilter_Click" CausesValidation="false" /></div>
            </div>
        </div>

        <!-- Table -->
        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="ay-table" DataKeyNames="AcademicYearID"
                    OnRowCommand="gv_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Academic Year"><ItemTemplate><span class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("YearName"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="StartDate" HeaderText="Start Date" DataFormatString="{0:dd MMM yyyy}" />
                        <asp:BoundField DataField="EndDate" HeaderText="End Date" DataFormatString="{0:dd MMM yyyy}" />
                        <asp:TemplateField HeaderText="Status"><ItemTemplate>
                            <span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("Status"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Status"))) %></span>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="flex items-center gap-1">
                                    <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditRow" CommandArgument='<%# Eval("AcademicYearID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                    <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="Activate" CommandArgument='<%# Eval("AcademicYearID") %>' ToolTip="Set Active"
                                        Visible='<%# !string.Equals(Convert.ToString(Eval("Status")),"Active",StringComparison.OrdinalIgnoreCase) %>'><i data-lucide="check-circle" class="w-4 h-4"></i></asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No academic years found.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
