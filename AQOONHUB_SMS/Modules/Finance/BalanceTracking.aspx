<%@ Page Title="Balance Tracking | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="BalanceTracking.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.BalanceTracking" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .bt-wrap { padding:1.25rem; max-width:1440px; margin:0 auto; }
        .card-head { display:flex; align-items:center; justify-content:space-between; gap:.75rem; padding:1rem 1.25rem; border-bottom:1px solid #E5E7EB; }
        .dark .card-head { border-color:#334155; }
        .card-head h2 { font-size:.95rem; font-weight:800; }
        .card-head .sub { font-size:.72rem; color:#6B7280; margin-top:.1rem; }
        .dark .card-head .sub { color:#94A3B8; }
        @media (max-width:768px){ .bt-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="bt-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><span>Finance</span>
            <span>/</span><a href="~/Modules/Finance/FeeManagement.aspx" runat="server" class="hover:text-brand-600">Fee Management</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Balance Tracking</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-6">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Balance Tracking</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Outstanding, unpaid, partial and overdue student invoices.</p>
            </div>
            <a href="~/Modules/Finance/FeeManagement.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back</a>
        </div>

        <div class="card overflow-hidden">
            <div class="card-head">
                <div><h2>Outstanding Balances</h2><p class="sub">Invoices with a remaining balance</p></div>
            </div>
            <div id="balScroll" class="overflow-x-auto">
                <asp:GridView ID="grid" runat="server" AutoGenerateColumns="false" CssClass="w-full min-w-[960px]" GridLines="None">
                    <Columns>
                        <asp:TemplateField HeaderText="Student" HeaderStyle-CssClass="th" ItemStyle-CssClass="td">
                            <HeaderStyle Width="220px" /><ItemStyle Width="220px" />
                            <ItemTemplate>
                                <div class="min-w-[12rem]">
                                    <span class="font-semibold whitespace-nowrap"><%#: Eval("StudentName") %></span>
                                    <span class="block text-[11px] text-gray-400 whitespace-nowrap"><%#: Eval("StudentCode") %> &middot; <%#: Eval("ClassName") %></span>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:HyperLinkField DataTextField="InvoiceNumber" DataNavigateUrlFields="InvoiceID" DataNavigateUrlFormatString="ViewInvoice.aspx?id={0}" HeaderText="Invoice" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" ControlStyle-CssClass="text-brand-600 font-semibold hover:underline" />
                        <asp:BoundField DataField="TotalAmount" HeaderText="Invoice Amount" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="PaidAmount" HeaderText="Paid" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="Balance" HeaderText="Balance" DataFormatString="{0:N2}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:BoundField DataField="DueDate" HeaderText="Due Date" DataFormatString="{0:dd MMM yyyy}" HeaderStyle-CssClass="th" ItemStyle-CssClass="td" />
                        <asp:TemplateField HeaderText="Overdue">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate><%# DaysOverdue(Eval("DueDate")) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate><span class="badge" style='<%# StatusStyle(Eval("StatusText")) %>'><%#: Eval("StatusText") %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Action">
                            <HeaderStyle CssClass="th" /><ItemStyle CssClass="td" />
                            <ItemTemplate>
                                <div class="flex items-center gap-2 whitespace-nowrap">
                                    <a class="btn btn-secondary !py-1 !px-2.5 !text-xs" href='ViewInvoice.aspx?id=<%# Eval("InvoiceID") %>'>
                                        <i data-lucide="eye" class="w-3.5 h-3.5"></i> View
                                    </a>
                                    <a class="btn btn-primary !py-1 !px-2.5 !text-xs" href='RecordPayment.aspx?invoiceId=<%# Eval("InvoiceID") %>'>
                                        <i data-lucide="credit-card" class="w-3.5 h-3.5"></i> Collect
                                    </a>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="flex flex-col items-center justify-center py-16 text-center">
                            <span class="w-14 h-14 rounded-2xl bg-brand-50 dark:bg-slate-800 text-brand-600 dark:text-brand-300 flex items-center justify-center mb-4"><i data-lucide="scale" class="w-7 h-7"></i></span>
                            <h3 class="font-bold">No outstanding balances</h3>
                            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">All invoices are fully paid.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="s" ContentPlaceHolderID="scripts" runat="server">
    <script>
        // Ensure the balance table starts scrolled to the left so the Student column is visible on load.
        (function () {
            function resetScroll() { var el = document.getElementById('balScroll'); if (el) el.scrollLeft = 0; }
            document.addEventListener('DOMContentLoaded', resetScroll);
            window.addEventListener('load', resetScroll);
        })();
    </script>
</asp:Content>
