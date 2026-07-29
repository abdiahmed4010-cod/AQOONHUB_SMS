<%@ Page Title="Fee Categories | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="FeeCategories.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.FeeCategories" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .fp { padding: 1.25rem; max-width: 1440px; margin: 0 auto; }
        .fee-cols { display:grid; grid-template-columns:1fr; gap:1.25rem; }
        @media (min-width:1024px){ .fee-cols { grid-template-columns:minmax(0,1fr) minmax(0,1.6fr); align-items:start; } }
        .card-head { display:flex; align-items:center; gap:.75rem; padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .dark .card-head { border-color:#334155; }
        .card-head h2 { font-size:.95rem; font-weight:800; }
        .card-head .sub { font-size:.72rem; color:#6B7280; margin-top:.1rem; }
        .dark .card-head .sub { color:#94A3B8; }
        .fld label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .dark .fld label { color:#CBD5E1; }
        .fg2 { display:grid; grid-template-columns:1fr 1fr; gap:1rem; }
        .filter-bar { display:flex; flex-wrap:wrap; align-items:center; gap:.6rem; }
        .filter-bar .grow { flex:1; min-width:180px; position:relative; }
        .filter-bar .grow svg { position:absolute; left:.75rem; top:50%; transform:translateY(-50%); color:#9CA3AF; width:1rem; height:1rem; }
        .filter-bar .grow input { padding-left:2.25rem; }
        .act-ic { display:inline-flex; align-items:center; justify-content:center; width:1.9rem; height:1.9rem; border-radius:.5rem; transition:all .15s; }
        .act-ic.edit { color:#F59E0B; background:#FFFBEB; } .act-ic.edit:hover { background:#FEF3C7; }
        .act-ic.del { color:#EF4444; background:#FEF2F2; } .act-ic.del:hover { background:#FEE2E2; }
        .dark .act-ic.edit { background:#2A2410; } .dark .act-ic.del { background:#2A1414; }
        @media (max-width:800px){ .fp { padding:.875rem; } .fg2 { grid-template-columns:1fr; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="fp">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span>Finance</span>
            <span>/</span><a href="~/Modules/Finance/FeeManagement.aspx" runat="server" class="hover:text-brand-600">Fee Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Fee Categories</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Fee Categories</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Create and manage all fee categories used in the school.</p>
            </div>
            <a href="~/Modules/Finance/FeeManagement.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back to Fee Management</a>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 bg-blue-50 text-blue-800 border border-blue-200 text-sm dark:bg-blue-500/10 dark:text-blue-300 dark:border-blue-500/30">
            <i data-lucide="info" class="w-4 h-4 inline-block mr-1"></i><asp:Literal ID="msgText" runat="server" />
        </asp:Panel>

        <div class="fee-cols">

            <!-- Add / Edit form -->
            <section class="card overflow-hidden">
                <div class="card-head">
                    <span class="w-9 h-9 rounded-xl flex items-center justify-center" style="background:#EFF6FF;color:#2563EB"><i data-lucide="tag" class="w-4 h-4"></i></span>
                    <div><h2>Add / Edit Fee Category</h2><p class="sub">Create or update a fee category.</p></div>
                </div>
                <div class="p-5">
                    <asp:HiddenField ID="categoryId" runat="server" />
                    <div class="fg2">
                        <div class="fld"><label>Category Name *</label><asp:TextBox ID="name" runat="server" CssClass="input" placeholder="e.g. Tuition Fee" /></div>
                        <div class="fld"><label>Category Code *</label><asp:TextBox ID="code" runat="server" CssClass="input" placeholder="e.g. TUI" /></div>
                    </div>
                    <div class="fld mt-4"><label>Description</label><asp:TextBox ID="description" runat="server" TextMode="MultiLine" Rows="3" CssClass="input" placeholder="Enter description (optional)" /></div>
                    <div class="fg2 mt-4">
                        <div class="fld"><label>Default Billing Term *</label>
                            <asp:DropDownList ID="term" runat="server" CssClass="input">
                                <asp:ListItem>Monthly</asp:ListItem>
                                <asp:ListItem>Per Term</asp:ListItem>
                                <asp:ListItem>Annual</asp:ListItem>
                                <asp:ListItem>One Time</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="fld"><label>Status</label>
                            <label class="inline-flex items-center gap-2 mt-1.5 cursor-pointer">
                                <asp:CheckBox ID="active" runat="server" Checked="true" />
                                <span class="text-sm font-semibold">Active</span>
                            </label>
                        </div>
                    </div>
                    <div class="flex justify-end gap-2 mt-6 pt-4 border-t border-gray-100 dark:border-slate-700">
                        <asp:Button ID="clear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="clear_Click" CausesValidation="false" />
                        <asp:Button ID="save" runat="server" Text="Save Category" CssClass="btn btn-primary" OnClick="save_Click" />
                    </div>
                </div>
            </section>

            <!-- List -->
            <section class="card overflow-hidden">
                <div class="card-head justify-between">
                    <div><h2>Fee Categories List</h2><p class="sub">All fee categories in the school.</p></div>
                </div>
                <div class="p-3.5 filter-bar border-b border-gray-100 dark:border-slate-700">
                    <div class="grow">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" /></svg>
                        <asp:TextBox ID="txtCatSearch" runat="server" CssClass="input" placeholder="Search by category name or code…" />
                    </div>
                    <asp:Button ID="btnCatFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnCatFilter_Click" CausesValidation="false" />
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full">
                        <thead>
                            <tr>
                                <th class="th">#</th><th class="th">Category Name</th><th class="th">Code</th>
                                <th class="th">Billing Term</th><th class="th">Description</th><th class="th">Status</th><th class="th">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="list" runat="server" OnItemCommand="list_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="td"><%# Container.ItemIndex + 1 %></td>
                                        <td class="td font-semibold"><%#: Eval("CategoryName") %></td>
                                        <td class="td"><span class="font-mono text-xs bg-gray-100 dark:bg-slate-700 rounded px-1.5 py-0.5"><%#: Eval("CategoryCode") %></span></td>
                                        <td class="td"><span class="badge" style='<%# TermStyle(Eval("DefaultBillingTerm")) %>'><%#: Eval("DefaultBillingTerm") %></span></td>
                                        <td class="td text-gray-500 dark:text-slate-400"><%#: Eval("Description") %></td>
                                        <td class="td"><span class="badge" style='<%# StatusStyle(Eval("IsActive")) %>'><%#: Eval("StatusText") %></span></td>
                                        <td class="td whitespace-nowrap">
                                            <div class="flex items-center gap-1.5">
                                                <asp:LinkButton runat="server" CssClass="act-ic edit" ToolTip="Edit" CommandName="editRow" CommandArgument='<%# Eval("FeeCategoryID") %>'><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                                <asp:LinkButton runat="server" CssClass="act-ic del" ToolTip="Delete" CommandName="deleteRow" CommandArgument='<%# Eval("FeeCategoryID") %>' OnClientClick="return confirm('Delete this unused category?');"><i data-lucide="trash-2" class="w-4 h-4"></i></asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <div class="px-4 py-3 text-xs text-gray-500 dark:text-slate-400 border-t border-gray-100 dark:border-slate-700">
                    <asp:Literal ID="litCount" runat="server" />
                </div>
            </section>
        </div>
    </div>
</asp:Content>
