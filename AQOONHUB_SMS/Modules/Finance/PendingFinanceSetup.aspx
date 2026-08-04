<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPages/MainMaster.master"
    CodeBehind="PendingFinanceSetup.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Finance.PendingFinanceSetup" Title="New Admissions Pending Finance Setup" %>

<asp:Content ID="cHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .fs-cards { display:grid; grid-template-columns:repeat(4,1fr); gap:1rem; }
        @media (max-width:900px){ .fs-cards{ grid-template-columns:repeat(2,1fr); } }
        @media (max-width:480px){ .fs-cards{ grid-template-columns:1fr; } }
        .fs-filters { display:grid; grid-template-columns:repeat(6,1fr); gap:.6rem; }
        @media (max-width:1024px){ .fs-filters{ grid-template-columns:repeat(3,1fr);} }
        @media (max-width:560px){ .fs-filters{ grid-template-columns:1fr 1fr;} }
        .fs-modal-backdrop{ position:fixed; inset:0; background:rgba(15,23,42,.5); z-index:80; display:flex; align-items:center; justify-content:center; padding:1rem; }
        .fs-modal{ background:#fff; border-radius:1rem; max-width:560px; width:100%; max-height:88vh; overflow-y:auto; }
        .dark .fs-modal{ background:#1E293B; }
    </style>
</asp:Content>

<asp:Content ID="cMain" ContentPlaceHolderID="MainContent" runat="server">
    <div class="p-4 md:p-6 max-w-7xl mx-auto">

        <div class="mb-5">
            <h1 class="text-xl md:text-2xl font-extrabold tracking-tight">New Admissions Pending Finance Setup</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Assign the correct fee structure and create the student's initial invoice.</p>
        </div>

        <asp:Panel ID="pnlMsgOk" runat="server" Visible="false" CssClass="card p-4 mb-4" role="status" aria-live="polite"
            style="border-left:4px solid #22C55E;">
            <asp:Literal ID="litMsgOk" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlMsgErr" runat="server" Visible="false" CssClass="card p-4 mb-4" role="alert" aria-live="assertive"
            style="border-left:4px solid #EF4444;">
            <asp:Label ID="lblMsgErr" runat="server" CssClass="text-sm" style="color:#B91C1C;" />
        </asp:Panel>

        <!-- Summary cards -->
        <div class="fs-cards mb-5">
            <div class="card p-4"><p class="text-xs text-gray-500 dark:text-slate-400">Pending Finance Setup</p><p class="text-2xl font-extrabold mt-1"><asp:Literal ID="litPending" runat="server" Text="0" /></p></div>
            <div class="card p-4"><p class="text-xs text-gray-500 dark:text-slate-400">Ready for Assignment</p><p class="text-2xl font-extrabold mt-1" style="color:#15803D;"><asp:Literal ID="litReady" runat="server" Text="0" /></p></div>
            <div class="card p-4"><p class="text-xs text-gray-500 dark:text-slate-400">Missing Fee Structure</p><p class="text-2xl font-extrabold mt-1" style="color:#B45309;"><asp:Literal ID="litMissing" runat="server" Text="0" /></p></div>
            <div class="card p-4"><p class="text-xs text-gray-500 dark:text-slate-400">Recently Completed (today)</p><p class="text-2xl font-extrabold mt-1" style="color:#2563EB;"><asp:Literal ID="litCompleted" runat="server" Text="0" /></p></div>
        </div>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="fs-filters">
                <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Code / Admission No / Name" aria-label="Search students" />
                <asp:DropDownList ID="ddlYear" runat="server" CssClass="input" aria-label="Academic year filter" />
                <asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" aria-label="Class filter" />
                <asp:DropDownList ID="ddlSection" runat="server" CssClass="input" aria-label="Section filter" />
                <asp:DropDownList ID="ddlShift" runat="server" CssClass="input" aria-label="Shift filter">
                    <asp:ListItem Value="">All Shifts</asp:ListItem>
                    <asp:ListItem Value="Morning">Morning</asp:ListItem>
                    <asp:ListItem Value="Afternoon">Afternoon</asp:ListItem>
                </asp:DropDownList>
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input" aria-label="Finance status filter">
                    <asp:ListItem Value="">Pending (default)</asp:ListItem>
                    <asp:ListItem Value="Ready">Ready</asp:ListItem>
                    <asp:ListItem Value="No Matching Fee Structure">No Matching Fee Structure</asp:ListItem>
                    <asp:ListItem Value="Multiple Fee Structures">Multiple Fee Structures</asp:ListItem>
                    <asp:ListItem Value="Invoice Already Exists">Invoice Already Exists</asp:ListItem>
                    <asp:ListItem Value="Missing Required Data">Missing Required Data</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="flex gap-2 mt-3">
                <asp:Button ID="btnApply" runat="server" CssClass="btn btn-primary" Text="Apply Filters" OnClick="btnApply_Click" />
                <asp:Button ID="btnClear" runat="server" CssClass="btn btn-secondary" Text="Clear Filters" OnClick="btnClear_Click" CausesValidation="false" />
            </div>
        </div>

        <!-- Queue table -->
        <div class="card overflow-hidden">
            <div class="overflow-x-auto">
                <table class="w-full" role="table">
                    <thead>
                        <tr>
                            <th class="th" scope="col">Student Code</th>
                            <th class="th" scope="col">Admission No</th>
                            <th class="th" scope="col">Student Name</th>
                            <th class="th" scope="col">Admission Date</th>
                            <th class="th" scope="col">Academic Year</th>
                            <th class="th" scope="col">Class</th>
                            <th class="th" scope="col">Section</th>
                            <th class="th" scope="col">Shift</th>
                            <th class="th" scope="col">Guardian</th>
                            <th class="th" scope="col">Fee Structures</th>
                            <th class="th" scope="col">Finance Status</th>
                            <th class="th" scope="col">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptQueue" runat="server" OnItemCommand="rptQueue_ItemCommand">
                            <ItemTemplate>
                                <tr class="border-t border-gray-100 dark:border-slate-700">
                                    <td class="td font-semibold"><%# Eval("StudentCode") %></td>
                                    <td class="td"><%# Eval("AdmissionNo") %></td>
                                    <td class="td"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentName"))) %></td>
                                    <td class="td"><%# Eval("EnrollmentDate", "{0:MMM dd, yyyy}") %></td>
                                    <td class="td"><%# Eval("AcademicYear") %></td>
                                    <td class="td"><%# Eval("ClassName") %></td>
                                    <td class="td"><%# Eval("SectionName") %></td>
                                    <td class="td"><%# Eval("Shift") %></td>
                                    <td class="td"><%# Server.HtmlEncode(Convert.ToString(Eval("Guardian"))) %></td>
                                    <td class="td"><%# Eval("AppCount") %></td>
                                    <td class="td"><span class="badge" style="<%# StatusStyle(Convert.ToString(Eval("FinanceStatus"))) %>"><%# Eval("FinanceStatus") %></span></td>
                                    <td class="td">
                                        <asp:LinkButton runat="server" CssClass="btn btn-primary !py-1 !text-xs" CommandName="prepare" CommandArgument='<%# Eval("StudentID") %>'
                                            Visible='<%# Convert.ToString(Eval("FinanceStatus"))=="Ready" %>'>Create Invoice</asp:LinkButton>
                                        <asp:HyperLink runat="server" CssClass="btn btn-secondary !py-1 !text-xs" NavigateUrl='<%# ResolveUrl("~/Modules/Finance/FeeStructures.aspx") %>'
                                            Visible='<%# Convert.ToString(Eval("FinanceStatus"))=="No Matching Fee Structure" %>'>Set Up Fee Structure</asp:HyperLink>
                                        <asp:Label runat="server" CssClass="text-xs text-gray-400"
                                            Visible='<%# Convert.ToString(Eval("FinanceStatus"))!="Ready" && Convert.ToString(Eval("FinanceStatus"))!="No Matching Fee Structure" %>'
                                            Text='<%# Convert.ToString(Eval("FinanceStatus"))=="Invoice Already Exists" ? "Completed" : (Convert.ToString(Eval("FinanceStatus"))=="Multiple Fee Structures" ? "Resolve in Fee Structures" : "Add enrollment date") %>'></asp:Label>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="p-8 text-center text-sm text-gray-500 dark:text-slate-400">
                <i data-lucide="inbox" class="w-8 h-8 mx-auto mb-2 opacity-60"></i>
                <p>No students match the current filters. New admissions without an initial invoice will appear here.</p>
            </asp:Panel>

            <div class="flex items-center justify-between p-3 border-t border-gray-100 dark:border-slate-700 text-xs">
                <asp:Label ID="lblPageInfo" runat="server" CssClass="text-gray-500 dark:text-slate-400" />
                <div class="flex gap-2">
                    <asp:Button ID="btnPrev" runat="server" CssClass="btn btn-secondary !py-1 !text-xs" Text="Previous" OnClick="btnPrev_Click" CausesValidation="false" />
                    <asp:Button ID="btnNext" runat="server" CssClass="btn btn-secondary !py-1 !text-xs" Text="Next" OnClick="btnNext_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <!-- Confirmation modal -->
        <asp:Panel ID="pnlConfirm" runat="server" Visible="false" CssClass="fs-modal-backdrop">
            <div class="fs-modal p-6" role="dialog" aria-modal="true" aria-labelledby="cpTitle">
                <h3 id="cpTitle" class="text-lg font-bold mb-1">Create Initial Invoice</h3>
                <p class="text-xs text-gray-500 dark:text-slate-400 mb-4">Review the details, then confirm. No invoice is created until you confirm.</p>

                <div class="text-sm space-y-1 mb-3">
                    <div class="detail-row"><span class="k">Student</span><span class="v"><asp:Label ID="lblCsName" runat="server" /> (<asp:Label ID="lblCsCode" runat="server" />)</span></div>
                    <div class="detail-row"><span class="k">Academic Year</span><span class="v"><asp:Label ID="lblCsYear" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Class / Section</span><span class="v"><asp:Label ID="lblCsClassSection" runat="server" /></span></div>
                    <div class="detail-row"><span class="k">Shift</span><span class="v"><asp:Label ID="lblCsShift" runat="server" /></span></div>
                </div>

                <table class="w-full text-sm mb-3">
                    <thead><tr><th class="th" scope="col">Fee Category</th><th class="th" scope="col">Amount</th><th class="th" scope="col">Discount</th><th class="th" scope="col">Total</th></tr></thead>
                    <tbody>
                        <asp:Repeater ID="rptPreview" runat="server">
                            <ItemTemplate>
                                <tr class="border-t border-gray-100 dark:border-slate-700">
                                    <td class="td"><%# Server.HtmlEncode(Convert.ToString(Eval("CategoryName"))) %></td>
                                    <td class="td"><%# Eval("Amount", "{0:N2}") %></td>
                                    <td class="td"><%# Eval("DiscountAmount", "{0:N2}") %></td>
                                    <td class="td font-semibold"><%# LineTotal(Container.DataItem) %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>

                <div class="flex items-center justify-between mb-4">
                    <label class="text-sm font-semibold" for="<%= txtDueDate.ClientID %>">Due date</label>
                    <asp:TextBox ID="txtDueDate" runat="server" TextMode="Date" CssClass="input !w-auto" />
                </div>
                <div class="flex items-center justify-between mb-5 text-base font-extrabold">
                    <span>Invoice Total</span><span><asp:Literal ID="litPreviewTotal" runat="server" Text="0.00" /></span>
                </div>

                <asp:HiddenField ID="hfStudentId" runat="server" />
                <div class="flex gap-2 justify-end">
                    <asp:Button ID="btnCancelConfirm" runat="server" CssClass="btn btn-secondary" Text="Cancel" OnClick="btnCancelConfirm_Click" CausesValidation="false" />
                    <asp:Button ID="btnConfirmCreate" runat="server" CssClass="btn btn-primary" Text="Confirm &amp; Create Invoice" OnClick="btnConfirmCreate_Click" />
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
