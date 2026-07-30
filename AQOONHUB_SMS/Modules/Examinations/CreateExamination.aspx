<%@ Page Title="Create Examination | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="CreateExamination.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.CreateExamination" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ce-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .ce-grid { display:grid; grid-template-columns:1fr; gap:1.25rem; }
        @media (min-width:1100px){ .ce-grid { grid-template-columns:2fr 1fr; align-items:start; } }
        .fld label { display:block; font-size:.72rem; font-weight:700; color:#334155; margin-bottom:.35rem; }
        .req { color:#ef4444; }
        .ov-row { display:flex; justify-content:space-between; gap:1rem; padding:.5rem 0; border-bottom:1px solid #f1f5f9; font-size:.83rem; }
        .ov-row .k { color:#64748B; } .ov-row .v { font-weight:700; text-align:right; }
        .chip { display:inline-block; padding:.2rem .55rem; border-radius:999px; background:#EFF6FF; color:#1D4ED8; font-size:.72rem; font-weight:600; margin:.15rem .15rem 0 0; }
        .subj-list label { display:flex; align-items:center; gap:.4rem; font-size:.83rem; font-weight:500; padding:.25rem .1rem; }
        @media (max-width:768px){ .ce-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ce-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a>
            <span>/</span><a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200"><asp:Literal ID="litCrumb" runat="server" Text="Create Examination" /></span>
        </nav>
        <div class="mb-4">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight"><asp:Literal ID="litHeading" runat="server" Text="Create Examination" /></h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Configure exam details, select classes and subjects, and set evaluation parameters.</p>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <div class="ce-grid">
            <!-- LEFT: setup -->
            <div class="card p-5">
                <h2 class="text-base font-bold mb-4 flex items-center gap-2"><i data-lucide="clipboard-list" class="w-5 h-5 text-blue-600"></i> Exam Setup</h2>
                <asp:HiddenField ID="hfId" runat="server" Value="0" />
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div class="fld md:col-span-1"><label>Exam Name <span class="req">*</span></label><asp:TextBox ID="txtName" runat="server" CssClass="input" placeholder="e.g. Mid Term Examination" /></div>
                    <div class="fld"><label>Academic Year <span class="req">*</span></label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                    <div class="fld"><label>Term <span class="req">*</span></label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></div>
                    <div class="fld"><label>Exam Type <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlType" runat="server" CssClass="input">
                            <asp:ListItem Text="Quiz" Value="Quiz" /><asp:ListItem Text="Monthly Test" Value="Monthly Test" />
                            <asp:ListItem Text="Mid Term Examination" Value="Mid Term Examination" /><asp:ListItem Text="Final Examination" Value="Final Examination" />
                            <asp:ListItem Text="Practical" Value="Practical" /><asp:ListItem Text="Assignment" Value="Assignment" />
                            <asp:ListItem Text="Other" Value="Other" />
                        </asp:DropDownList></div>
                    <div class="fld"><label>Start Date <span class="req">*</span></label><asp:TextBox ID="txtStart" runat="server" CssClass="input" TextMode="Date" /></div>
                    <div class="fld"><label>End Date <span class="req">*</span></label><asp:TextBox ID="txtEnd" runat="server" CssClass="input" TextMode="Date" /></div>
                    <div class="fld"><label>Class <span class="req">*</span></label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                    <div class="fld"><label>Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                    <div class="fld"><label>Subject Scope <span class="req">*</span></label>
                        <asp:DropDownList ID="ddlScope" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlScope_Changed">
                            <asp:ListItem Text="All Subjects" Value="All" /><asp:ListItem Text="Selected Subjects" Value="Selected" />
                        </asp:DropDownList></div>
                </div>

                <asp:Panel ID="pnlSubjects" runat="server" CssClass="mt-4" Visible="false">
                    <label class="block text-xs font-bold text-slate-700 mb-1.5">Selected Subjects <span class="req">*</span></label>
                    <div class="subj-list border rounded-lg p-3 max-h-48 overflow-y-auto">
                        <asp:CheckBoxList ID="cblSubjects" runat="server" RepeatLayout="Flow" />
                        <asp:Panel ID="pnlNoSubjects" runat="server" Visible="false"><p class="text-sm text-gray-400">No subjects assigned to this class. Assign subjects under Academics → Subjects first.</p></asp:Panel>
                    </div>
                </asp:Panel>

                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
                    <div class="fld"><label>Passing Mark (%) <span class="req">*</span></label><asp:TextBox ID="txtPass" runat="server" CssClass="input" TextMode="Number" Text="40" /></div>
                    <div class="fld"><label>Total Marks <span class="req">*</span></label><asp:TextBox ID="txtTotal" runat="server" CssClass="input" TextMode="Number" Text="100" /></div>
                    <div class="fld"><label>Weight (%) <span class="req">*</span></label><asp:TextBox ID="txtWeight" runat="server" CssClass="input" TextMode="Number" Text="100" /></div>
                </div>

                <div class="flex flex-wrap justify-end gap-2 mt-6 pt-4 border-t border-gray-100">
                    <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="btn btn-secondary">Cancel</a>
                    <asp:Button ID="btnDraft" runat="server" Text="Save Draft" CssClass="btn btn-secondary" OnClick="btnDraft_Click" />
                    <asp:Button ID="btnCreate" runat="server" Text="+ Create Examination" CssClass="btn btn-primary" OnClick="btnCreate_Click" />
                </div>
            </div>

            <!-- RIGHT: overview -->
            <div class="card p-5">
                <h2 class="text-base font-bold mb-4 flex items-center gap-2"><i data-lucide="file-text" class="w-5 h-5 text-blue-600"></i> Examination Overview</h2>
                <div class="ov-row"><span class="k">Exam Name</span><span class="v"><asp:Literal ID="ovName" runat="server" Text="—" /></span></div>
                <div class="ov-row"><span class="k">Academic Year</span><span class="v"><asp:Literal ID="ovYear" runat="server" Text="—" /></span></div>
                <div class="ov-row"><span class="k">Term</span><span class="v"><asp:Literal ID="ovTerm" runat="server" Text="—" /></span></div>
                <div class="ov-row"><span class="k">Exam Type</span><span class="v"><asp:Literal ID="ovType" runat="server" Text="—" /></span></div>
                <div class="ov-row"><span class="k">Duration</span><span class="v"><asp:Literal ID="ovDuration" runat="server" Text="—" /></span></div>
                <div class="ov-row"><span class="k">Status</span><span class="v"><asp:Literal ID="ovStatus" runat="server" Text="Draft" /></span></div>
                <div class="mt-4">
                    <p class="text-xs font-bold text-slate-700 mb-1.5">Selected Class</p>
                    <div><asp:Literal ID="ovClass" runat="server" Text="<span class='chip'>—</span>" /></div>
                </div>
                <div class="mt-3">
                    <p class="text-xs font-bold text-slate-700 mb-1.5">Selected Subjects</p>
                    <div><asp:Literal ID="ovSubjects" runat="server" Text="<span class='chip'>—</span>" /></div>
                </div>
                <div class="mt-4 p-3 rounded-lg bg-amber-50 border border-amber-100 text-xs text-amber-800">
                    <b>Note:</b> Scheduling, rooms and invigilators are configured after the exam is created.
                </div>
            </div>
        </div>
    </div>
</asp:Content>
