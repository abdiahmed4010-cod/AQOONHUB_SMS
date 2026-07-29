<%@ Page Title="Payroll Periods"
    Language="C#"
    MasterPageFile="~/MasterPages/MainMaster.master"
    AutoEventWireup="true"
    CodeBehind="PayrollPeriods.aspx.cs"
    Inherits="AQOONHUB_SMS.Modules.Payroll.PayrollPeriods" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
    <style>
        .period-table {
            width: 100%;
            border-collapse: collapse;
        }

        .period-table th {
            padding: .875rem 1rem;
            border-bottom: 1px solid #e2e8f0;
            background: #f8fafc;
            color: #475569;
            font-size: .75rem;
            font-weight: 700;
            letter-spacing: .05em;
            text-align: left;
            text-transform: uppercase;
            white-space: nowrap;
        }

        .period-table td {
            padding: 1rem;
            border-bottom: 1px solid #f1f5f9;
            color: #334155;
            font-size: .875rem;
            vertical-align: middle;
            white-space: nowrap;
        }

        .period-table tr:hover td {
            background: #f8fafc;
        }

        .period-table .pager-row td {
            padding: 1rem;
            background: #fff;
        }

        .period-table .pager-row table {
            margin-left: auto;
            margin-right: auto;
        }

        .period-table .pager-row a,
        .period-table .pager-row span {
            display: inline-flex;
            min-width: 2.25rem;
            height: 2.25rem;
            margin: 0 .125rem;
            align-items: center;
            justify-content: center;
            border: 1px solid #cbd5e1;
            border-radius: .5rem;
            color: #475569;
            font-size: .875rem;
            font-weight: 600;
        }

        .period-table .pager-row span {
            border-color: #2563eb;
            background: #2563eb;
            color: #fff;
        }

        .period-table .pager-row a:hover {
            background: #f8fafc;
        }

        .validation-error {
            display: block;
            margin-top: .375rem;
            color: #dc2626;
            font-size: .75rem;
            font-weight: 500;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen bg-slate-50 p-4 sm:p-6 lg:p-8">
        <div class="mx-auto max-w-7xl space-y-6">
            <section class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
                <div class="flex flex-col gap-5 p-6 lg:flex-row lg:items-center lg:justify-between">
                    <div>
                        <div class="mb-2 flex items-center gap-2 text-sm font-semibold text-blue-600">
                            <i data-lucide="calendar-range" class="h-4 w-4"></i>
                            <span>Payroll Configuration</span>
                        </div>
                        <h1 class="text-2xl font-bold tracking-tight text-slate-900 sm:text-3xl">
                            Payroll Periods
                        </h1>
                        <p class="mt-2 max-w-2xl text-sm leading-6 text-slate-600">
                            Create and manage payroll date ranges, payment dates, and processing status.
                        </p>
                    </div>

                    <div class="flex flex-wrap gap-3">
                        <a href="Payroll.aspx"
                           class="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50">
                            <i data-lucide="arrow-left" class="h-4 w-4"></i>
                            Back to Payroll
                        </a>

                        <asp:LinkButton ID="btnNewPeriod"
                            runat="server"
                            CausesValidation="false"
                            OnClick="btnNewPeriod_Click"
                            CssClass="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2">
                            <i data-lucide="plus" class="h-4 w-4"></i>
                            New Payroll Period
                        </asp:LinkButton>
                    </div>
                </div>
            </section>

            <asp:Panel ID="pnlMessage"
                runat="server"
                Visible="false"
                role="alert"
                CssClass="rounded-xl border px-4 py-3 shadow-sm">
                <div class="flex items-start gap-3">
                    <i data-lucide="info" class="mt-0.5 h-5 w-5 shrink-0"></i>
                    <asp:Label ID="lblMessage"
                        runat="server"
                        CssClass="text-sm font-medium" />
                </div>
            </asp:Panel>

            <section class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <div class="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
                    <div class="flex items-start justify-between">
                        <div>
                            <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">
                                Total Periods
                            </p>
                            <asp:Label ID="lblTotalPeriods"
                                runat="server"
                                Text="0"
                                CssClass="mt-2 block text-2xl font-bold text-slate-900" />
                        </div>
                        <span class="rounded-lg bg-slate-100 p-2 text-slate-600">
                            <i data-lucide="calendar-days" class="h-5 w-5"></i>
                        </span>
                    </div>
                </div>

                <div class="rounded-xl border border-blue-200 bg-white p-5 shadow-sm">
                    <div class="flex items-start justify-between">
                        <div>
                            <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">
                                Draft
                            </p>
                            <asp:Label ID="lblDraftCount"
                                runat="server"
                                Text="0"
                                CssClass="mt-2 block text-2xl font-bold text-blue-700" />
                        </div>
                        <span class="rounded-lg bg-blue-50 p-2 text-blue-600">
                            <i data-lucide="file-pen-line" class="h-5 w-5"></i>
                        </span>
                    </div>
                </div>

                <div class="rounded-xl border border-amber-200 bg-white p-5 shadow-sm">
                    <div class="flex items-start justify-between">
                        <div>
                            <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">
                                Processing
                            </p>
                            <asp:Label ID="lblProcessingCount"
                                runat="server"
                                Text="0"
                                CssClass="mt-2 block text-2xl font-bold text-amber-700" />
                        </div>
                        <span class="rounded-lg bg-amber-50 p-2 text-amber-600">
                            <i data-lucide="loader-circle" class="h-5 w-5"></i>
                        </span>
                    </div>
                </div>

                <div class="rounded-xl border border-emerald-200 bg-white p-5 shadow-sm">
                    <div class="flex items-start justify-between">
                        <div>
                            <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">
                                Completed
                            </p>
                            <asp:Label ID="lblCompletedCount"
                                runat="server"
                                Text="0"
                                CssClass="mt-2 block text-2xl font-bold text-emerald-700" />
                        </div>
                        <span class="rounded-lg bg-emerald-50 p-2 text-emerald-600">
                            <i data-lucide="circle-check-big" class="h-5 w-5"></i>
                        </span>
                    </div>
                </div>
            </section>

            <section class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="mb-4 flex items-center gap-2">
                    <i data-lucide="list-filter" class="h-5 w-5 text-slate-500"></i>
                    <h2 class="text-base font-bold text-slate-900">Filter Periods</h2>
                </div>

                <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                    <div>
                        <label for="<%= ddlStatusFilter.ClientID %>"
                            class="mb-1.5 block text-sm font-semibold text-slate-700">
                            Status
                        </label>
                        <asp:DropDownList ID="ddlStatusFilter"
                            runat="server"
                            CssClass="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm text-slate-700 outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100">
                            <asp:ListItem Text="All Statuses" Value="" />
                            <asp:ListItem Text="Draft" Value="Draft" />
                            <asp:ListItem Text="Processing" Value="Processing" />
                            <asp:ListItem Text="Completed" Value="Completed" />
                            <asp:ListItem Text="Cancelled" Value="Cancelled" />
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label for="<%= txtSearch.ClientID %>"
                            class="mb-1.5 block text-sm font-semibold text-slate-700">
                            Search
                        </label>
                        <asp:TextBox ID="txtSearch"
                            runat="server"
                            MaxLength="100"
                            placeholder="Search period name or dates"
                            CssClass="w-full rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm text-slate-700 outline-none transition placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-100" />
                    </div>
                </div>

                <div class="mt-4 flex flex-wrap justify-end gap-3">
                    <asp:LinkButton ID="btnReset"
                        runat="server"
                        CausesValidation="false"
                        OnClick="btnReset_Click"
                        CssClass="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 transition hover:bg-slate-50">
                        <i data-lucide="rotate-ccw" class="h-4 w-4"></i>
                        Reset
                    </asp:LinkButton>

                    <asp:LinkButton ID="btnSearch"
                        runat="server"
                        CausesValidation="false"
                        OnClick="btnSearch_Click"
                        CssClass="inline-flex items-center gap-2 rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-slate-800">
                        <i data-lucide="search" class="h-4 w-4"></i>
                        Search
                    </asp:LinkButton>
                </div>
            </section>

            <section class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
                <div class="border-b border-slate-200 px-5 py-4">
                    <h2 class="text-base font-bold text-slate-900">Payroll Periods</h2>
                    <p class="mt-1 text-sm text-slate-500">
                        Review period dates and manage valid payroll status transitions.
                    </p>
                </div>

                <div class="overflow-x-auto">
                    <asp:GridView ID="gvPayrollPeriods"
                        runat="server"
                        AutoGenerateColumns="False"
                        GridLines="None"
                        DataKeyNames="PayrollPeriodID"
                        AllowSorting="True"
                        AllowPaging="True"
                        PageSize="15"
                        CssClass="period-table"
                        PagerStyle-CssClass="pager-row"
                        OnRowCommand="gvPayrollPeriods_RowCommand"
                        OnPageIndexChanging="gvPayrollPeriods_PageIndexChanging"
                        OnSorting="gvPayrollPeriods_Sorting">
                        <Columns>
                            <asp:BoundField DataField="PeriodName"
                                HeaderText="Period Name"
                                SortExpression="PeriodName" />

                            <asp:BoundField DataField="StartDate"
                                HeaderText="Start Date"
                                SortExpression="StartDate"
                                DataFormatString="{0:dd MMM yyyy}"
                                HtmlEncode="false" />

                            <asp:BoundField DataField="EndDate"
                                HeaderText="End Date"
                                SortExpression="EndDate"
                                DataFormatString="{0:dd MMM yyyy}"
                                HtmlEncode="false" />

                            <asp:BoundField DataField="PaymentDate"
                                HeaderText="Payment Date"
                                SortExpression="PaymentDate"
                                DataFormatString="{0:dd MMM yyyy}"
                                HtmlEncode="false"
                                NullDisplayText="—" />

                            <asp:TemplateField HeaderText="Status"
                                SortExpression="Status">
                                <ItemTemplate>
                                    <span class='<%# GetPeriodStatusCss(Convert.ToString(Eval("Status"))) %>'>
                                        <%#: Convert.ToString(Eval("Status")) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="CreatedAt"
                                HeaderText="Created"
                                SortExpression="CreatedAt"
                                DataFormatString="{0:dd MMM yyyy HH:mm}"
                                HtmlEncode="false" />

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="flex flex-wrap items-center gap-2">
                                        <asp:LinkButton ID="btnEditPeriod"
                                            runat="server"
                                            Text="Edit"
                                            CausesValidation="false"
                                            CommandName="EditPeriod"
                                            CommandArgument='<%# Eval("PayrollPeriodID") %>'
                                            Visible='<%# CanShowPeriodAction(Convert.ToString(Eval("Status")), "EditPeriod") %>'
                                            CssClass="inline-flex items-center rounded-lg border border-blue-200 bg-blue-50 px-2.5 py-1.5 text-xs font-semibold text-blue-700 transition hover:bg-blue-100" />

                                        <asp:LinkButton ID="btnStartProcessing"
                                            runat="server"
                                            Text="Start Processing"
                                            CausesValidation="false"
                                            CommandName="StartProcessing"
                                            CommandArgument='<%# Eval("PayrollPeriodID") %>'
                                            Visible='<%# CanShowPeriodAction(Convert.ToString(Eval("Status")), "StartProcessing") %>'
                                            CssClass="inline-flex items-center rounded-lg border border-amber-200 bg-amber-50 px-2.5 py-1.5 text-xs font-semibold text-amber-700 transition hover:bg-amber-100" />

                                        <asp:LinkButton ID="btnCompletePeriod"
                                            runat="server"
                                            Text="Complete"
                                            CausesValidation="false"
                                            CommandName="CompletePeriod"
                                            CommandArgument='<%# Eval("PayrollPeriodID") %>'
                                            Visible='<%# CanShowPeriodAction(Convert.ToString(Eval("Status")), "CompletePeriod") %>'
                                            CssClass="inline-flex items-center rounded-lg border border-emerald-200 bg-emerald-50 px-2.5 py-1.5 text-xs font-semibold text-emerald-700 transition hover:bg-emerald-100" />

                                        <asp:LinkButton ID="btnCancelPeriod"
                                            runat="server"
                                            Text="Cancel"
                                            CausesValidation="false"
                                            CommandName="CancelPeriod"
                                            CommandArgument='<%# Eval("PayrollPeriodID") %>'
                                            Visible='<%# CanShowPeriodAction(Convert.ToString(Eval("Status")), "CancelPeriod") %>'
                                            CssClass="inline-flex items-center rounded-lg border border-rose-200 bg-rose-50 px-2.5 py-1.5 text-xs font-semibold text-rose-700 transition hover:bg-rose-100" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <EmptyDataTemplate>
                            <div class="px-6 py-16 text-center">
                                <div class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 text-slate-500">
                                    <i data-lucide="calendar-x" class="h-6 w-6"></i>
                                </div>
                                <p class="mt-4 text-sm font-semibold text-slate-700">
                                    No payroll periods found.
                                </p>
                                <p class="mt-1 text-sm text-slate-500">
                                    Change the filters or create a new payroll period.
                                </p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </section>
        </div>

        <asp:Panel ID="pnlPeriodModal"
            runat="server"
            Visible="false"
            CssClass="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-slate-950/60 p-4">
            <div class="my-8 w-full max-w-2xl overflow-hidden rounded-2xl bg-white shadow-2xl">
                <div class="flex items-center justify-between border-b border-slate-200 px-6 py-4">
                    <div class="flex items-center gap-3">
                        <span class="rounded-lg bg-blue-100 p-2 text-blue-700">
                            <i data-lucide="calendar-plus" class="h-5 w-5"></i>
                        </span>
                        <asp:Label ID="lblPeriodModalTitle"
                            runat="server"
                            Text="New Payroll Period"
                            CssClass="text-lg font-bold text-slate-900" />
                    </div>

                    <asp:LinkButton ID="btnClosePeriodModalTop"
                        runat="server"
                        CausesValidation="false"
                        OnClick="btnClosePeriodModal_Click"
                        aria-label="Close"
                        CssClass="rounded-lg p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-700">
                        <i data-lucide="x" class="h-5 w-5"></i>
                    </asp:LinkButton>
                </div>

                <div class="space-y-5 p-6">
                    <asp:HiddenField ID="hfPayrollPeriodID" runat="server" />

                    <asp:ValidationSummary ID="vsPayrollPeriod"
                        runat="server"
                        ValidationGroup="PayrollPeriodForm"
                        HeaderText="Correct the following:"
                        CssClass="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700" />

                    <div>
                        <label for="<%= txtPeriodName.ClientID %>"
                            class="mb-1.5 block text-sm font-semibold text-slate-700">
                            Period Name <span class="text-rose-600">*</span>
                        </label>
                        <asp:TextBox ID="txtPeriodName"
                            runat="server"
                            MaxLength="100"
                            CssClass="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100" />
                        <asp:RequiredFieldValidator ID="rfvPeriodName"
                            runat="server"
                            ControlToValidate="txtPeriodName"
                            ValidationGroup="PayrollPeriodForm"
                            ErrorMessage="Period name is required."
                            Text="Period name is required."
                            Display="Dynamic"
                            CssClass="validation-error" />
                    </div>

                    <div class="grid grid-cols-1 gap-5 md:grid-cols-2">
                        <div>
                            <label for="<%= txtStartDate.ClientID %>"
                                class="mb-1.5 block text-sm font-semibold text-slate-700">
                                Start Date <span class="text-rose-600">*</span>
                            </label>
                            <asp:TextBox ID="txtStartDate"
                                runat="server"
                                TextMode="Date"
                                CssClass="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100" />
                            <asp:RequiredFieldValidator ID="rfvStartDate"
                                runat="server"
                                ControlToValidate="txtStartDate"
                                ValidationGroup="PayrollPeriodForm"
                                ErrorMessage="Start date is required."
                                Text="Start date is required."
                                Display="Dynamic"
                                CssClass="validation-error" />
                        </div>

                        <div>
                            <label for="<%= txtEndDate.ClientID %>"
                                class="mb-1.5 block text-sm font-semibold text-slate-700">
                                End Date <span class="text-rose-600">*</span>
                            </label>
                            <asp:TextBox ID="txtEndDate"
                                runat="server"
                                TextMode="Date"
                                CssClass="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100" />
                            <asp:RequiredFieldValidator ID="rfvEndDate"
                                runat="server"
                                ControlToValidate="txtEndDate"
                                ValidationGroup="PayrollPeriodForm"
                                ErrorMessage="End date is required."
                                Text="End date is required."
                                Display="Dynamic"
                                CssClass="validation-error" />
                            <asp:CompareValidator ID="cvEndDate"
                                runat="server"
                                ControlToValidate="txtEndDate"
                                ControlToCompare="txtStartDate"
                                Type="Date"
                                Operator="GreaterThanEqual"
                                ValidationGroup="PayrollPeriodForm"
                                ErrorMessage="End date cannot be earlier than start date."
                                Text="End date cannot be earlier than start date."
                                Display="Dynamic"
                                CssClass="validation-error" />
                        </div>
                    </div>

                    <div>
                        <label for="<%= txtPaymentDate.ClientID %>"
                            class="mb-1.5 block text-sm font-semibold text-slate-700">
                            Payment Date
                        </label>
                        <asp:TextBox ID="txtPaymentDate"
                            runat="server"
                            TextMode="Date"
                            CssClass="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm outline-none transition focus:border-blue-500 focus:ring-2 focus:ring-blue-100" />
                        <asp:CompareValidator ID="cvPaymentDate"
                            runat="server"
                            ControlToValidate="txtPaymentDate"
                            ControlToCompare="txtStartDate"
                            Type="Date"
                            Operator="GreaterThanEqual"
                            ValidationGroup="PayrollPeriodForm"
                            ErrorMessage="Payment date cannot be earlier than start date."
                            Text="Payment date cannot be earlier than start date."
                            Display="Dynamic"
                            CssClass="validation-error" />
                        <p class="mt-1.5 text-xs text-slate-500">
                            Payment date is optional and can be set later while the period is still Draft.
                        </p>
                    </div>
                </div>

                <div class="flex justify-end gap-3 border-t border-slate-200 bg-slate-50 px-6 py-4">
                    <asp:Button ID="btnClosePeriodModal"
                        runat="server"
                        Text="Cancel"
                        CausesValidation="false"
                        OnClick="btnClosePeriodModal_Click"
                        CssClass="cursor-pointer rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 transition hover:bg-slate-100" />

                    <asp:Button ID="btnSavePeriod"
                        runat="server"
                        Text="Save Period"
                        ValidationGroup="PayrollPeriodForm"
                        OnClick="btnSavePeriod_Click"
                        CssClass="cursor-pointer rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlStatusModal"
            runat="server"
            Visible="false"
            CssClass="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 p-4">
            <div class="w-full max-w-lg overflow-hidden rounded-2xl bg-white shadow-2xl">
                <div class="flex items-center justify-between border-b border-slate-200 px-6 py-4">
                    <div class="flex items-center gap-3">
                        <span class="rounded-lg bg-amber-100 p-2 text-amber-700">
                            <i data-lucide="triangle-alert" class="h-5 w-5"></i>
                        </span>
                        <asp:Label ID="lblStatusModalTitle"
                            runat="server"
                            CssClass="text-lg font-bold text-slate-900" />
                    </div>

                    <asp:LinkButton ID="btnCancelStatusTop"
                        runat="server"
                        CausesValidation="false"
                        OnClick="btnCancelStatus_Click"
                        aria-label="Close"
                        CssClass="rounded-lg p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-700">
                        <i data-lucide="x" class="h-5 w-5"></i>
                    </asp:LinkButton>
                </div>

                <div class="p-6">
                    <asp:HiddenField ID="hfStatusPayrollPeriodID" runat="server" />
                    <asp:HiddenField ID="hfRequestedStatus" runat="server" />

                    <asp:Label ID="lblStatusModalMessage"
                        runat="server"
                        CssClass="block text-sm leading-6 text-slate-600" />
                </div>

                <div class="flex justify-end gap-3 border-t border-slate-200 bg-slate-50 px-6 py-4">
                    <asp:Button ID="btnCancelStatus"
                        runat="server"
                        Text="Cancel"
                        CausesValidation="false"
                        OnClick="btnCancelStatus_Click"
                        CssClass="cursor-pointer rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 transition hover:bg-slate-100" />

                    <asp:Button ID="btnConfirmStatus"
                        runat="server"
                        Text="Confirm"
                        CausesValidation="false"
                        OnClick="btnConfirmStatus_Click"
                        CssClass="cursor-pointer rounded-lg bg-amber-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-amber-700 focus:outline-none focus:ring-2 focus:ring-amber-500 focus:ring-offset-2" />
                </div>
            </div>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function initializePayrollPeriodIcons() {
            if (window.lucide) {
                window.lucide.createIcons();
            }
        }

        document.addEventListener(
            "DOMContentLoaded",
            initializePayrollPeriodIcons);

        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(
                initializePayrollPeriodIcons);
        }
    </script>
</asp:Content>