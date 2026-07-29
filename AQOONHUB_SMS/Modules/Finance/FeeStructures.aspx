<%@ Page Title="Fee Structure | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="FeeStructures.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.FeeStructures" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .students-wrap { padding: 1.25rem; max-width: 1440px; margin: 0 auto; }
        .filter-bar { display:flex; flex-wrap:wrap; align-items:center; gap:.625rem; }
        .filter-bar .grow { flex:1; min-width:200px; position:relative; }
        .filter-bar .grow svg { position:absolute; left:.75rem; top:50%; transform:translateY(-50%); color:#9CA3AF; width:1rem; height:1rem; }
        .filter-bar .grow input { padding-left:2.25rem; }
        @media (max-width: 768px) { .students-wrap { padding: .875rem; } }
    </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="students-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Fee Structure</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Fee Structure</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Define fee amounts by class, category, and academic year.</p>
            </div>
            <asp:HyperLink ID="lnkAddFee" runat="server" CssClass="btn btn-primary" NavigateUrl="~/Modules/Finance/AddFeeStructure.aspx">
                <i data-lucide="plus" class="w-4 h-4"></i> Add Fee Structure
            </asp:HyperLink>
        </div>

        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false" Style="background:#ECFDF5;color:#166534;border:1px solid #BBF7D0;border-radius:.7rem;padding:.85rem 1rem;font-size:.82rem;margin-bottom:1rem;">
            <asp:Label ID="lblSuccess" runat="server" />
        </asp:Panel>

        <div class="card p-3.5 mb-4 filter-bar">
            <div class="grow">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Search by fee name…" />
            </div>
            <asp:DropDownList ID="ddlAcademicYear" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Academic Years" Value="0" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlClass" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All Classes" Value="0" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input !w-auto">
                <asp:ListItem Text="All" Value="" />
                <asp:ListItem Text="Active" Value="1" />
                <asp:ListItem Text="Inactive" Value="0" />
            </asp:DropDownList>
            <asp:LinkButton ID="btnSearch" runat="server" CssClass="btn btn-primary" OnClick="btnSearch_Click">Search</asp:LinkButton>
            <asp:LinkButton ID="btnReset" runat="server" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false">Reset</asp:LinkButton>
        </div>

        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <asp:GridView ID="gvFees" runat="server" AutoGenerateColumns="false" GridLines="None" ShowHeader="true"
                    CssClass="w-full" DataKeyNames="ClassFeeStructureID" OnRowCommand="gvFees_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="ClassName" HeaderText="Class" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="SectionName" HeaderText="Section" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="CategoryName" HeaderText="Fee Category" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="YearName" HeaderText="Academic Year" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="BillingTerm" HeaderText="Billing Term" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Amount ($)">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>$<%# Convert.ToDecimal(Eval("Amount")).ToString("N2") %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <span class="badge" style='<%# Convert.ToString(Eval("StatusText")) == "Active" ? "background:#DCFCE7;color:#15803D" : "background:#F1F5F9;color:#64748B" %>'><%#: Eval("StatusText") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex gap-1">
                                    <asp:HyperLink runat="server" CssClass="btn-ghost btn !p-1.5" ToolTip="Edit" NavigateUrl='<%# "~/Modules/Finance/EditFeeStructure.aspx?id=" + Eval("ClassFeeStructureID") %>'>
                                        <i data-lucide="pencil" class="w-4 h-4"></i>
                                    </asp:HyperLink>
                                    <asp:LinkButton runat="server" CssClass="btn-ghost btn !p-1.5" ToolTip='<%# Convert.ToString(Eval("StatusText")) == "Active" ? "Deactivate" : "Activate" %>'
                                        CommandName="ToggleActive" CommandArgument='<%# Eval("ClassFeeStructureID") + "|" + (Convert.ToString(Eval("StatusText")) == "Active") %>'>
                                        <i data-lucide="power" class="w-4 h-4"></i>
                                    </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="flex flex-col items-center justify-center py-16 text-center">
                            <span class="w-14 h-14 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-300 flex items-center justify-center mb-4">
                                <i data-lucide="wallet" class="w-7 h-7"></i>
                            </span>
                            <h3 class="font-bold">No fee structures found</h3>
                            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1 mb-4 max-w-sm">Try adjusting your search or filters, or add a new fee structure.</p>
                            <a href="~/Modules/Finance/AddFeeStructure.aspx" runat="server" class="btn btn-primary"><i data-lucide="plus" class="w-4 h-4"></i> Add Fee Structure</a>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
