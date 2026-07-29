<%@ Page Title="Create Pay Run | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="CreatePayRun.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Payroll.CreatePayRun" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .cpr-wrap { padding:1.25rem; max-width:1400px; margin:0 auto; }
        .steps { display:flex; align-items:center; gap:.5rem; overflow-x:auto; }
        .steps .st { display:flex; align-items:center; gap:.6rem; padding:.4rem; flex-shrink:0; }
        .steps .st .n { width:1.9rem; height:1.9rem; border-radius:999px; display:flex; align-items:center; justify-content:center; font-weight:800; font-size:.78rem; background:#E2E8F0; color:#64748B; }
        .steps .st.active .n { background:#2563EB; color:#fff; }
        .steps .st.done .n { background:#16A34A; color:#fff; }
        .steps .st .t { font-size:.8rem; font-weight:700; color:#64748B; }
        .steps .st.active .t { color:#2563EB; }
        .steps .sep { flex:1; min-width:1rem; height:2px; background:#E2E8F0; }
        .fld label { display:block; font-size:.75rem; font-weight:700; margin-bottom:.35rem; color:#374151; }
        .fg3 { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:768px){ .fg3 { grid-template-columns:repeat(3,1fr); } }
        .wz-table { width:100%; border-collapse:collapse; font-size:.83rem; }
        .wz-table th { padding:.7rem 1rem; background:#f8fafc; text-align:left; font-size:.68rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .wz-table td { padding:.6rem 1rem; border-bottom:1px solid #f1f5f9; white-space:nowrap; }
        .wz-table input.cell { width:7rem; border:1px solid #E5E7EB; border-radius:.45rem; padding:.35rem .5rem; font-size:.82rem; text-align:right; }
        .sum-row { display:flex; justify-content:space-between; gap:1rem; font-size:.85rem; padding:.45rem 0; border-bottom:1px solid #F1F5F9; }
        .sum-row .k { color:#475569; } .sum-row .v { font-weight:800; }
        @media (max-width:768px){ .cpr-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="cpr-wrap">

        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="hover:text-brand-600">Payroll</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Create Pay Run</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-5">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Create New Pay Run</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Generate payroll for active staff. Basic Salary is copied automatically from each staff record.</p>
            </div>
            <a href="~/Modules/Payroll/Payroll.aspx" runat="server" class="btn btn-secondary">Cancel</a>
        </div>

        <!-- Step indicator -->
        <div class="card p-4 mb-5">
            <div class="steps">
                <asp:Panel ID="stp1" runat="server" CssClass="st"><span class="n">1</span><span class="t">Pay Run Details</span></asp:Panel>
                <span class="sep"></span>
                <asp:Panel ID="stp2" runat="server" CssClass="st"><span class="n">2</span><span class="t">Employees</span></asp:Panel>
                <span class="sep"></span>
                <asp:Panel ID="stp3" runat="server" CssClass="st"><span class="n">3</span><span class="t">Salary Components</span></asp:Panel>
                <span class="sep"></span>
                <asp:Panel ID="stp4" runat="server" CssClass="st"><span class="n">4</span><span class="t">Review &amp; Confirm</span></asp:Panel>
            </div>
        </div>

        <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="p-3 mb-4 rounded-lg bg-amber-50 text-amber-800 border border-amber-200 text-sm">
            <i data-lucide="alert-triangle" class="w-4 h-4 inline-block mr-1"></i><asp:Literal ID="lblMsg" runat="server" />
        </asp:Panel>

        <!-- STEP 1 -->
        <asp:Panel ID="pnlStep1" runat="server" CssClass="card p-6">
            <h2 class="font-extrabold mb-4">Pay Run Details</h2>
            <div class="fg3">
                <div class="fld"><label>Pay Run Name *</label><asp:TextBox ID="txtPayRunName" runat="server" CssClass="input" MaxLength="120" placeholder="e.g. July 2026 Salary" /></div>
                <div class="fld"><label>Pay Period *</label>
                    <asp:DropDownList ID="ddlPeriod" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlPeriod_Changed" />
                </div>
                <div class="fld"><label>Pay Date</label>
                    <div class="input bg-gray-50 dark:bg-slate-800"><asp:Literal ID="litPayDate" runat="server" Text="—" /></div>
                </div>
                <div class="fld"><label>Department</label><asp:DropDownList ID="ddlDept" runat="server" CssClass="input" /></div>
                <div class="fld"><label>Default Payment Method *</label>
                    <asp:DropDownList ID="ddlMethod" runat="server" CssClass="input">
                        <asp:ListItem Text="Bank Transfer" Value="Bank Transfer" />
                        <asp:ListItem Text="Mobile Money" Value="Mobile Money" />
                        <asp:ListItem Text="Cash" Value="Cash" />
                        <asp:ListItem Text="Cheque" Value="Cheque" />
                        <asp:ListItem Text="Other" Value="Other" />
                    </asp:DropDownList>
                </div>
            </div>
            <div class="flex justify-end mt-6 pt-4 border-t border-gray-100 dark:border-slate-700">
                <asp:Button ID="btnNext1" runat="server" Text="Next: Select Employees" CssClass="btn btn-primary" OnClick="btnNext1_Click" />
            </div>
        </asp:Panel>

        <!-- STEP 2 -->
        <asp:Panel ID="pnlStep2" runat="server" Visible="false" CssClass="card overflow-hidden">
            <div class="p-5 border-b border-gray-100 dark:border-slate-700 flex flex-wrap items-center justify-between gap-2">
                <h2 class="font-extrabold">Select Employees</h2>
                <div class="flex items-center gap-2">
                    <asp:TextBox ID="txtStaffSearch" runat="server" CssClass="input !w-auto" placeholder="Search ID, dept, position" />
                    <asp:Button ID="btnStaffFilter" runat="server" Text="Filter" CssClass="btn btn-secondary" OnClick="btnStaffFilter_Click" />
                    <button type="button" class="btn btn-secondary" onclick="cprToggleAll(true);return false;">Select All</button>
                    <button type="button" class="btn btn-secondary" onclick="cprToggleAll(false);return false;">Clear</button>
                </div>
            </div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvStaff" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="wz-table" DataKeyNames="StaffID">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate><input type="checkbox" onclick="cprToggleAll(this.checked)" /></HeaderTemplate>
                            <ItemTemplate><asp:CheckBox ID="chkSel" runat="server" CssClass="cpr-chk" /></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="EmployeeID" HeaderText="Employee ID" />
                        <asp:BoundField DataField="Department" HeaderText="Department" />
                        <asp:BoundField DataField="Position" HeaderText="Position" />
                        <asp:BoundField DataField="Salary" HeaderText="Basic Salary" DataFormatString="{0:$#,##0.00}" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                    </Columns>
                    <EmptyDataTemplate><div class="py-10 text-center text-sm text-gray-500">No active staff found.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
            <div class="flex items-center justify-between p-5 border-t border-gray-100 dark:border-slate-700">
                <asp:Button ID="btnBack2" runat="server" Text="Back" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnBack2_Click" />
                <asp:Button ID="btnNext2" runat="server" Text="Next: Salary Components" CssClass="btn btn-primary" OnClick="btnNext2_Click" />
            </div>
        </asp:Panel>

        <!-- STEP 3 -->
        <asp:Panel ID="pnlStep3" runat="server" Visible="false" CssClass="card overflow-hidden">
            <div class="p-5 border-b border-gray-100 dark:border-slate-700">
                <h2 class="font-extrabold">Salary Components</h2>
                <p class="text-xs text-gray-500 dark:text-slate-400 mt-1">Basic Salary is read-only (from Staff record). Tax Deduction — Manual Amount. Enter monthly amounts; totals preview live.</p>
            </div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvComponents" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="wz-table" DataKeyNames="StaffID" OnRowDataBound="gvComponents_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="EmployeeID" HeaderText="Employee ID" />
                        <asp:BoundField DataField="Department" HeaderText="Department" />
                        <asp:BoundField DataField="BasicSalary" HeaderText="Basic Salary" DataFormatString="{0:$#,##0.00}" />
                        <asp:TemplateField HeaderText="Other Allowance"><ItemTemplate><asp:TextBox ID="txtOther" runat="server" CssClass="cell cpr-calc" Text='<%# Eval("OtherAllowance","{0:0.00}") %>' /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Bonus"><ItemTemplate><asp:TextBox ID="txtBonus" runat="server" CssClass="cell cpr-calc" Text='<%# Eval("Bonus","{0:0.00}") %>' /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Tax (Manual)"><ItemTemplate><asp:TextBox ID="txtTax" runat="server" CssClass="cell cpr-calc" Text='<%# Eval("TaxDeduction","{0:0.00}") %>' /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Other Deduction"><ItemTemplate><asp:TextBox ID="txtOtherDed" runat="server" CssClass="cell cpr-calc" Text='<%# Eval("OtherDeduction","{0:0.00}") %>' /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Gross"><ItemTemplate><span class="cpr-gross font-semibold">0.00</span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Deductions"><ItemTemplate><span class="cpr-ded font-semibold text-rose-600">0.00</span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Net"><ItemTemplate><span class="cpr-net font-bold text-emerald-700">0.00</span></ItemTemplate></asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="flex items-center justify-between p-5 border-t border-gray-100 dark:border-slate-700">
                <asp:Button ID="btnBack3" runat="server" Text="Back" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnBack3_Click" />
                <asp:Button ID="btnNext3" runat="server" Text="Next: Review" CssClass="btn btn-primary" OnClick="btnNext3_Click" />
            </div>
        </asp:Panel>

        <!-- STEP 4 -->
        <asp:Panel ID="pnlStep4" runat="server" Visible="false">
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
                <div class="card p-5 lg:col-span-2 overflow-hidden">
                    <h2 class="font-extrabold mb-3">Review Employees</h2>
                    <div class="overflow-x-auto">
                        <asp:GridView ID="gvReview" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="wz-table">
                            <Columns>
                                <asp:BoundField DataField="EmployeeID" HeaderText="Employee ID" />
                                <asp:BoundField DataField="Department" HeaderText="Department" />
                                <asp:BoundField DataField="BasicSalary" HeaderText="Basic" DataFormatString="{0:$#,##0.00}" />
                                <asp:BoundField DataField="OtherAllowance" HeaderText="Other Allow." DataFormatString="{0:$#,##0.00}" />
                                <asp:BoundField DataField="Bonus" HeaderText="Bonus" DataFormatString="{0:$#,##0.00}" />
                                <asp:BoundField DataField="GrossSalary" HeaderText="Gross" DataFormatString="{0:$#,##0.00}" />
                                <asp:BoundField DataField="TotalDeductions" HeaderText="Deductions" DataFormatString="{0:$#,##0.00}" />
                                <asp:BoundField DataField="NetSalary" HeaderText="Net" DataFormatString="{0:$#,##0.00}" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
                <div class="card p-5">
                    <h2 class="font-extrabold mb-3">Pay Run Summary</h2>
                    <div class="sum-row"><span class="k">Pay Run Name</span><span class="v"><asp:Literal ID="litRName" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Pay Period</span><span class="v"><asp:Literal ID="litRPeriod" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Pay Date</span><span class="v"><asp:Literal ID="litRPayDate" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Employees</span><span class="v"><asp:Literal ID="litRCount" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Payment Method</span><span class="v"><asp:Literal ID="litRMethod" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Total Basic</span><span class="v"><asp:Literal ID="litRBasic" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Total Other Allowance</span><span class="v"><asp:Literal ID="litROther" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Total Bonus</span><span class="v"><asp:Literal ID="litRBonus" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Total Gross</span><span class="v text-blue-700"><asp:Literal ID="litRGross" runat="server" /></span></div>
                    <div class="sum-row"><span class="k">Total Deductions</span><span class="v text-rose-700"><asp:Literal ID="litRDeductions" runat="server" /></span></div>
                    <div class="sum-row"><span class="k font-bold">Total Net Pay</span><span class="v text-emerald-700 text-base"><asp:Literal ID="litRNet" runat="server" /></span></div>
                    <div class="flex items-center justify-between mt-5 pt-4 border-t border-gray-100 dark:border-slate-700">
                        <asp:Button ID="btnBack4" runat="server" Text="Back" CssClass="btn btn-secondary" CausesValidation="false" OnClick="btnBack4_Click" />
                        <asp:Button ID="btnCreate" runat="server" Text="Create Pay Run" CssClass="btn btn-primary" OnClick="btnCreate_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>

<asp:Content ID="s" ContentPlaceHolderID="scripts" runat="server">
    <script>
        function cprToggleAll(check) {
            document.querySelectorAll('.cpr-chk input[type=checkbox]').forEach(function (c) { c.checked = check; });
        }
        function cprNum(v) { var n = parseFloat(String(v || '').replace(/[^0-9.]/g, '')); return isNaN(n) ? 0 : n; }
        function cprRecalc() {
            var rows = document.querySelectorAll('#MainContent_gvComponents tbody tr');
            rows.forEach(function (tr) {
                var cells = tr.querySelectorAll('td');
                if (cells.length < 3) return;
                var basic = cprNum(cells[2].textContent);
                var other = cprNum((tr.querySelector('input[id*=txtOther]') || {}).value);
                var bonus = cprNum((tr.querySelector('input[id*=txtBonus]') || {}).value);
                var tax = cprNum((tr.querySelector('input[id*=txtTax]') || {}).value);
                var oded = cprNum((tr.querySelector('input[id*=txtOtherDed]') || {}).value);
                var gross = basic + other + bonus;
                var ded = tax + oded;
                var net = gross - ded; if (net < 0) net = 0;
                var f = function (n) { return n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }); };
                var g = tr.querySelector('.cpr-gross'); if (g) g.textContent = f(gross);
                var d = tr.querySelector('.cpr-ded'); if (d) d.textContent = f(ded);
                var nn = tr.querySelector('.cpr-net'); if (nn) nn.textContent = f(net);
            });
        }
        document.addEventListener('DOMContentLoaded', function () {
            cprRecalc();
            document.addEventListener('keyup', function (e) { if (e.target && e.target.classList && e.target.classList.contains('cpr-calc')) cprRecalc(); });
        });
    </script>
</asp:Content>
