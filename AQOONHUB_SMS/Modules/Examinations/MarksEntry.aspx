<%@ Page Title="Marks Entry | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="MarksEntry.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.MarksEntry" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .me-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .me-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:1000px){ .me-sum { grid-template-columns:repeat(4,1fr); } }
        .me-card { display:flex; align-items:center; gap:.85rem; padding:1rem 1.1rem; }
        .me-ico { width:42px; height:42px; border-radius:11px; display:flex; align-items:center; justify-content:center; flex:none; }
        .me-card .lbl { font-size:.7rem; font-weight:600; color:#64748B; } .me-card .val { font-size:1.35rem; font-weight:800; }
        .me-grid { display:grid; grid-template-columns:1fr; gap:1rem; margin-top:1rem; }
        @media (min-width:1200px){ .me-grid { grid-template-columns:1fr 300px; align-items:start; } }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.55rem .7rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.5rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; white-space:nowrap; }
        .tbl input.score { width:80px; } .tbl input.rem { width:140px; }
        .gs-tbl { width:100%; border-collapse:collapse; } .gs-tbl td { padding:.3rem .5rem; font-size:.78rem; border-bottom:1px solid #f1f5f9; }
        @media (max-width:768px){ .me-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="me-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Marks Entry</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Marks Entry</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Enter and manage student marks for the selected examination and subject.</p>
            </div>
            <div class="flex gap-2">
                <asp:Button ID="btnExport" runat="server" Text="Export Sheet" CssClass="btn btn-secondary" OnClick="btnExport_Click" CausesValidation="false" />
                <asp:Button ID="btnSaveDraft" runat="server" Text="Save Progress" CssClass="btn btn-secondary" OnClick="btnSaveDraft_Click" />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit Marks" CssClass="btn btn-primary" OnClick="btnSubmit_Click" OnClientClick="return confirm('Submit marks? They will be locked.');" />
                <asp:Button ID="btnReopen" runat="server" Text="Reopen" CssClass="btn btn-secondary" OnClick="btnReopen_Click" CausesValidation="false" Visible="false" OnClientClick="return confirm('Reopen these submitted marks for editing?');" />
            </div>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Summary -->
        <div class="me-sum">
            <div class="card me-card"><div class="me-ico" style="background:#DCFCE7;color:#16A34A"><i data-lucide="users" class="w-5 h-5"></i></div><div><p class="lbl">Total Students</p><p class="val"><asp:Literal ID="litTotal" runat="server" Text="0" /></p></div></div>
            <div class="card me-card"><div class="me-ico" style="background:#DBEAFE;color:#2563EB"><i data-lucide="edit-3" class="w-5 h-5"></i></div><div><p class="lbl">Entered</p><p class="val"><asp:Literal ID="litEntered" runat="server" Text="0" /></p></div></div>
            <div class="card me-card"><div class="me-ico" style="background:#FEF3C7;color:#D97706"><i data-lucide="clock" class="w-5 h-5"></i></div><div><p class="lbl">Remaining</p><p class="val"><asp:Literal ID="litRemaining" runat="server" Text="0" /></p></div></div>
            <div class="card me-card"><div class="me-ico" style="background:#CCFBF1;color:#0D9488"><i data-lucide="bar-chart-3" class="w-5 h-5"></i></div><div><p class="lbl">Completion</p><p class="val"><asp:Literal ID="litCompletion" runat="server" Text="0%" /></p></div></div>
        </div>

        <!-- Filters -->
        <div class="card p-4 mt-4">
            <div class="grid grid-cols-2 md:grid-cols-6 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlTerm_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Examination</label><asp:DropDownList ID="ddlExam" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlExam_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSection_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject</label><asp:DropDownList ID="ddlSubject" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSubject_Changed" /></div>
            </div>
            <div class="mt-3 flex justify-end"><asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" /></div>
        </div>

        <div class="me-grid">
            <!-- Marks table -->
            <div class="card overflow-hidden">
                <div class="card-head justify-between flex items-center">
                    <h2 class="text-sm font-bold">Students</h2>
                    <span class="text-xs text-gray-500">Auto-save: Off · <asp:Literal ID="litLockNote" runat="server" /></span>
                </div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvMarks" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl" DataKeyNames="StudentID">
                        <Columns>
                            <asp:TemplateField HeaderText="#"><ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="StudentCode" HeaderText="Student ID" />
                            <asp:BoundField DataField="FullName" HeaderText="Student Name" />
                            <asp:BoundField DataField="AdmissionNo" HeaderText="Admission No" />
                            <asp:TemplateField HeaderText="Score"><ItemTemplate>
                                <asp:TextBox ID="txtScore" runat="server" CssClass="input score" TextMode="Number" step="0.01"
                                    Text='<%# Eval("Marks")==System.DBNull.Value ? "" : Convert.ToString(Eval("Marks")) %>' Enabled='<%# CanEdit %>' />
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Total"><ItemTemplate><asp:Literal ID="litRowTotal" runat="server" Text='<%# TotalMarks %>' /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Grade"><ItemTemplate><span class="badge" style="background:#EEF2FF;color:#3730A3"><%# Eval("Grade")==System.DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(Eval("Grade"))) %></span></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Attendance"><ItemTemplate>
                                <asp:DropDownList ID="ddlAtt" runat="server" CssClass="input" Enabled='<%# CanEdit %>'>
                                    <asp:ListItem Text="Present" Value="Present" /><asp:ListItem Text="Absent" Value="Absent" />
                                    <asp:ListItem Text="Excused" Value="Excused" /><asp:ListItem Text="Withheld" Value="Withheld" />
                                </asp:DropDownList>
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Remarks"><ItemTemplate>
                                <asp:TextBox ID="txtRemarks" runat="server" CssClass="input rem" Text='<%# Eval("Remarks")==System.DBNull.Value ? "" : Convert.ToString(Eval("Remarks")) %>' Enabled='<%# CanEdit %>' />
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Status"><ItemTemplate>
                                <span class="badge" style='<%# string.Equals(Convert.ToString(Eval("MarkStatus")),"Submitted",StringComparison.OrdinalIgnoreCase) ? "background:#DCFCE7;color:#15803D" : (string.IsNullOrEmpty(Convert.ToString(Eval("MarkStatus"))) ? "background:#F1F5F9;color:#64748B" : "background:#FEF3C7;color:#B45309") %>'><%# string.IsNullOrEmpty(Convert.ToString(Eval("MarkStatus"))) ? "Not Entered" : Server.HtmlEncode(Convert.ToString(Eval("MarkStatus"))) %></span>
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">Select a scheduled examination, class, section and subject to load students.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- Grading scale + summary -->
            <div class="space-y-4">
                <div class="card p-4">
                    <h2 class="text-sm font-bold mb-2">Grading Scale</h2>
                    <asp:GridView ID="gvScale" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="gs-tbl">
                        <Columns>
                            <asp:BoundField DataField="GradeLetter" HeaderText="Grade" />
                            <asp:TemplateField HeaderText="Range"><ItemTemplate><%# Eval("MinMarks") %>–<%# Eval("MaxMarks") %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="Description" HeaderText="Desc" />
                        </Columns>
                        <EmptyDataTemplate><div class="py-4 text-center text-xs text-gray-400">No grading scale for the year.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <div class="card p-4">
                    <h2 class="text-sm font-bold mb-2">Submission Summary</h2>
                    <div class="text-sm space-y-1">
                        <div class="flex justify-between"><span class="text-gray-500">Total Students</span><b><asp:Literal ID="litSumTotal" runat="server" Text="0" /></b></div>
                        <div class="flex justify-between"><span class="text-gray-500">Entered</span><b><asp:Literal ID="litSumEntered" runat="server" Text="0" /></b></div>
                        <div class="flex justify-between"><span class="text-gray-500">Submitted</span><b><asp:Literal ID="litSumSubmitted" runat="server" Text="0" /></b></div>
                        <div class="flex justify-between"><span class="text-gray-500">Ready to Submit</span><b><asp:Literal ID="litReady" runat="server" Text="No" /></b></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
