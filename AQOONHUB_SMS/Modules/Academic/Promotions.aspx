<%@ Page Title="Promotions | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Promotions.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Academic.Promotions" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .pr-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .pr-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:.85rem; }
        @media (min-width:768px){ .pr-sum { grid-template-columns:repeat(4,1fr); } }
        @media (min-width:1200px){ .pr-sum { grid-template-columns:repeat(7,1fr); } }
        .pr-sum .card { padding:.9rem 1rem; }
        .pr-sum .lbl { font-size:.66rem; font-weight:700; text-transform:uppercase; letter-spacing:.03em; color:#64748B; }
        .pr-sum .val { font-size:1.4rem; font-weight:800; line-height:1.1; }
        .pr-table { width:100%; border-collapse:collapse; }
        .pr-table th { padding:.65rem 1rem; background:#f8fafc; text-align:left; font-size:.64rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .pr-table td { padding:.65rem 1rem; border-bottom:1px solid #f1f5f9; font-size:.83rem; white-space:nowrap; }
        .rv-table { width:100%; border-collapse:collapse; }
        .rv-table th { padding:.55rem .7rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; }
        .rv-table td { padding:.5rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; }
        .wiz-back { position:fixed; inset:0; background:rgba(15,23,42,.45); z-index:60; }
        .wiz { position:fixed; top:0; right:0; height:100%; width:100%; max-width:760px; background:#fff; z-index:61; box-shadow:-8px 0 24px rgba(0,0,0,.12); overflow-y:auto; }
        .wiz-head { padding:1.1rem 1.25rem; border-bottom:1px solid #E5E7EB; display:flex; justify-content:space-between; align-items:center; }
        .wiz-body { padding:1.25rem; }
        .steps { display:flex; gap:.5rem; margin-bottom:1.25rem; flex-wrap:wrap; }
        .step { display:flex; align-items:center; gap:.4rem; font-size:.75rem; font-weight:600; color:#94A3B8; }
        .step .n { width:22px; height:22px; border-radius:50%; background:#E2E8F0; color:#64748B; display:flex; align-items:center; justify-content:center; font-size:.7rem; font-weight:800; }
        .step.active { color:#2563EB; } .step.active .n { background:#2563EB; color:#fff; }
        .step.done .n { background:#16A34A; color:#fff; }
        @media (max-width:768px){ .pr-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="pr-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Academic/Academics.aspx" runat="server" class="hover:text-brand-600">Academics</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Promotions</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Promotions</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage student promotions between academic years.</p>
            </div>
            <asp:Button ID="btnPromote" runat="server" Text="+ Promote Students" CssClass="btn btn-primary" OnClick="btnPromote_Click" CausesValidation="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-5 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">From Academic Year <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlFrom" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">To Academic Year <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlTo" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Search</label><asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Name or code..." /></div>
            </div>
            <div class="mt-3 flex justify-end gap-2">
                <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
            </div>
        </div>

        <!-- Summary cards -->
        <div class="pr-sum mb-5">
            <div class="card"><p class="lbl">Total Students</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div>
            <div class="card"><p class="lbl">Eligible</p><p class="val text-blue-700"><asp:Literal ID="litEligible" runat="server" Text="0" /></p></div>
            <div class="card"><p class="lbl">Promoted</p><p class="val text-emerald-700"><asp:Literal ID="litPromoted" runat="server" Text="0" /></p></div>
            <div class="card"><p class="lbl">Pending</p><p class="val text-amber-700"><asp:Literal ID="litPending" runat="server" Text="0" /></p></div>
            <div class="card"><p class="lbl">Repeated</p><p class="val text-orange-700"><asp:Literal ID="litRepeated" runat="server" Text="0" /></p></div>
            <div class="card"><p class="lbl">Graduated</p><p class="val text-indigo-700"><asp:Literal ID="litGraduated" runat="server" Text="0" /></p></div>
            <div class="card"><p class="lbl">Not Eligible</p><p class="val text-rose-700"><asp:Literal ID="litNotEligible" runat="server" Text="0" /></p></div>
        </div>

        <!-- Promotion list -->
        <div class="card overflow-hidden">
            <div class="card-head"><h2 class="text-sm font-bold">Promotion List</h2></div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="pr-table">
                    <Columns>
                        <asp:TemplateField HeaderText="Student"><ItemTemplate>
                            <div class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("FullName"))) %></div>
                            <div class="text-xs text-gray-500"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentCode"))) %></div>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Current Class / Section"><ItemTemplate>
                            <%# Server.HtmlEncode(Convert.ToString(Eval("CurrentClass"))) %> / <%# Server.HtmlEncode(Convert.ToString(Eval("CurrentSection"))) %>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Promotion Status"><ItemTemplate>
                            <span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("PromotionStatus"))) %>'><%# PromoLabel(Eval("PromotionStatus")) %></span>
                        </ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Action Date"><ItemTemplate>
                            <%# Eval("ActionDate")==System.DBNull.Value ? "—" : Convert.ToDateTime(Eval("ActionDate")).ToString("dd MMM yyyy") %>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No students found for the selected From Academic Year.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <!-- ===== WIZARD ===== -->
        <asp:Panel ID="pnlWizard" runat="server" Visible="false">
            <div class="wiz-back"></div>
            <div class="wiz">
                <div class="wiz-head">
                    <h3 class="font-bold text-base">Promotion Wizard</h3>
                    <asp:LinkButton ID="btnCloseWiz" runat="server" CssClass="text-gray-500" OnClick="btnCloseWiz_Click" CausesValidation="false"><i data-lucide="x" class="w-5 h-5"></i></asp:LinkButton>
                </div>
                <div class="wiz-body">
                    <div class="steps">
                        <div id="s1" runat="server" class="step"><span class="n">1</span> Select Options</div>
                        <div id="s2" runat="server" class="step"><span class="n">2</span> Review Students</div>
                        <div id="s3" runat="server" class="step"><span class="n">3</span> Confirm</div>
                        <div id="s4" runat="server" class="step"><span class="n">4</span> Complete</div>
                    </div>

                    <asp:Panel ID="pnlWizMsg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm bg-amber-50 text-amber-800 border border-amber-200"><asp:Literal ID="wizMsgText" runat="server" /></asp:Panel>

                    <!-- STEP 1 -->
                    <asp:Panel ID="pnlStep1" runat="server">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">From Academic Year <span class="text-red-500">*</span></label><asp:DropDownList ID="wFrom" runat="server" CssClass="input" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">To Academic Year <span class="text-red-500">*</span></label><asp:DropDownList ID="wTo" runat="server" CssClass="input" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Current Class <span class="text-red-500">*</span></label><asp:DropDownList ID="wCurClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="wCurClass_Changed" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Current Section</label><asp:DropDownList ID="wCurSection" runat="server" CssClass="input" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Target Class <span class="text-red-500">*</span></label><asp:DropDownList ID="wTgtClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="wTgtClass_Changed" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Target Section <span class="text-red-500">*</span></label><asp:DropDownList ID="wTgtSection" runat="server" CssClass="input" /></div>
                            <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Default Decision</label>
                                <asp:DropDownList ID="wDefault" runat="server" CssClass="input">
                                    <asp:ListItem Text="Promoted" Value="Promoted" />
                                    <asp:ListItem Text="Repeated" Value="Repeated" />
                                    <asp:ListItem Text="Graduated" Value="Graduated" />
                                    <asp:ListItem Text="Transferred" Value="Transferred" />
                                    <asp:ListItem Text="Withdrawn" Value="Withdrawn" />
                                    <asp:ListItem Text="Not Eligible" Value="NotEligible" />
                                </asp:DropDownList></div>
                        </div>
                        <div class="flex justify-end mt-6"><asp:Button ID="btnStep1Next" runat="server" Text="Next: Review Students" CssClass="btn btn-primary" OnClick="btnStep1Next_Click" /></div>
                    </asp:Panel>

                    <!-- STEP 2 -->
                    <asp:Panel ID="pnlStep2" runat="server" Visible="false">
                        <div class="flex items-center justify-between mb-2">
                            <p class="text-sm text-gray-600">Select students and set each decision.</p>
                            <label class="text-xs font-semibold flex items-center gap-1"><asp:CheckBox ID="chkAll" runat="server" AutoPostBack="true" OnCheckedChanged="chkAll_Changed" /> Select all</label>
                        </div>
                        <div class="overflow-x-auto border rounded-lg">
                            <asp:GridView ID="gvReview" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="rv-table" DataKeyNames="StudentID">
                                <Columns>
                                    <asp:TemplateField><ItemTemplate><asp:CheckBox ID="chkSel" runat="server" /></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Student"><ItemTemplate>
                                        <div class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("FullName"))) %></div>
                                        <div class="text-xs text-gray-500"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentCode"))) %></div>
                                    </ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Current"><ItemTemplate><%# Server.HtmlEncode(Convert.ToString(Eval("CurrentClass"))) %> / <%# Server.HtmlEncode(Convert.ToString(Eval("CurrentSection"))) %></ItemTemplate></asp:TemplateField>
                                    <asp:TemplateField HeaderText="Decision"><ItemTemplate>
                                        <asp:DropDownList ID="ddlDecision" runat="server" CssClass="input" style="min-width:130px">
                                            <asp:ListItem Text="Promoted" Value="Promoted" />
                                            <asp:ListItem Text="Repeated" Value="Repeated" />
                                            <asp:ListItem Text="Graduated" Value="Graduated" />
                                            <asp:ListItem Text="Transferred" Value="Transferred" />
                                            <asp:ListItem Text="Withdrawn" Value="Withdrawn" />
                                            <asp:ListItem Text="Not Eligible" Value="NotEligible" />
                                        </asp:DropDownList>
                                    </ItemTemplate></asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No eligible students (all may already be promoted to the target year).</div></EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                        <div class="flex justify-between mt-6">
                            <asp:Button ID="btnStep2Back" runat="server" Text="Back" CssClass="btn btn-secondary" OnClick="btnStep2Back_Click" CausesValidation="false" />
                            <asp:Button ID="btnStep2Next" runat="server" Text="Next: Confirm" CssClass="btn btn-primary" OnClick="btnStep2Next_Click" />
                        </div>
                    </asp:Panel>

                    <!-- STEP 3 -->
                    <asp:Panel ID="pnlStep3" runat="server" Visible="false">
                        <div class="card p-4 mb-4">
                            <h4 class="font-bold text-sm mb-3">Confirm Promotion</h4>
                            <asp:Literal ID="litConfirm" runat="server" />
                        </div>
                        <p class="text-sm text-amber-700 mb-4">This action preserves each student's previous academic record. Continue?</p>
                        <div class="flex justify-between mt-4">
                            <asp:Button ID="btnStep3Back" runat="server" Text="Back" CssClass="btn btn-secondary" OnClick="btnStep3Back_Click" CausesValidation="false" />
                            <asp:Button ID="btnConfirm" runat="server" Text="Confirm & Promote" CssClass="btn btn-primary" OnClick="btnConfirm_Click" />
                        </div>
                    </asp:Panel>

                    <!-- STEP 4 -->
                    <asp:Panel ID="pnlStep4" runat="server" Visible="false">
                        <div class="text-center py-6">
                            <div class="mx-auto w-14 h-14 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center mb-3"><i data-lucide="check" class="w-7 h-7"></i></div>
                            <h4 class="font-bold text-lg mb-2">Promotion Complete</h4>
                            <asp:Literal ID="litComplete" runat="server" />
                        </div>
                        <div class="flex justify-center gap-2 mt-4">
                            <asp:Button ID="btnViewList" runat="server" Text="View Promotion List" CssClass="btn btn-primary" OnClick="btnViewList_Click" CausesValidation="false" />
                            <a href="~/Modules/Academic/Academics.aspx" runat="server" class="btn btn-secondary">Back to Academics</a>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
