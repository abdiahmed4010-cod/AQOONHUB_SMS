<%@ Page Title="Exam Schedule | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ExamSchedule.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.ExamSchedule" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .es-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .es-sum { display:grid; grid-template-columns:repeat(2,1fr); gap:1rem; }
        @media (min-width:1000px){ .es-sum { grid-template-columns:repeat(4,1fr); } }
        .es-card { display:flex; align-items:center; gap:.85rem; padding:1rem 1.1rem; }
        .es-ico { width:42px; height:42px; border-radius:11px; display:flex; align-items:center; justify-content:center; flex:none; }
        .es-card .lbl { font-size:.7rem; font-weight:600; color:#64748B; } .es-card .val { font-size:1.35rem; font-weight:800; }
        .es-grid { display:grid; grid-template-columns:1fr; gap:1rem; margin-top:1rem; }
        @media (min-width:1100px){ .es-grid { grid-template-columns:360px 1fr; align-items:start; } }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.55rem .7rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.55rem .7rem; border-bottom:1px solid #f1f5f9; font-size:.8rem; white-space:nowrap; }
        .fld { margin-bottom:.7rem; } .fld label { display:block; font-size:.7rem; font-weight:700; color:#334155; margin-bottom:.3rem; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:28px; height:28px; border-radius:7px; color:#64748B; }
        .ico-btn:hover { background:#EFF6FF; color:#2563EB; } .ico-btn.danger:hover { background:#FEF2F2; color:#DC2626; }
        @media (max-width:768px){ .es-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="es-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Exam Schedule</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Exam Schedule &amp; Timetable</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Create, manage and publish examination schedules.</p>
            </div>
            <asp:Button ID="btnPublish" runat="server" Text="Publish Schedule" CssClass="btn btn-primary" OnClick="btnPublish_Click" CausesValidation="false"
                OnClientClick="return confirm('Publish this schedule? It becomes read-only.');" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Summary -->
        <div class="es-sum">
            <div class="card es-card"><div class="es-ico" style="background:#DBEAFE;color:#2563EB"><i data-lucide="clipboard-list" class="w-5 h-5"></i></div><div><p class="lbl">Total Exams</p><p class="val"><asp:Literal ID="litTotalExams" runat="server" Text="0" /></p></div></div>
            <div class="card es-card"><div class="es-ico" style="background:#FEF3C7;color:#D97706"><i data-lucide="book-open" class="w-5 h-5"></i></div><div><p class="lbl">Scheduled Subjects</p><p class="val"><asp:Literal ID="litScheduled" runat="server" Text="0" /></p></div></div>
            <div class="card es-card"><div class="es-ico" style="background:#DCFCE7;color:#16A34A"><i data-lucide="user-check" class="w-5 h-5"></i></div><div><p class="lbl">Invigilators</p><p class="val"><asp:Literal ID="litInvigilators" runat="server" Text="0" /></p></div></div>
            <div class="card es-card"><div class="es-ico" style="background:#EDE9FE;color:#7C3AED"><i data-lucide="building" class="w-5 h-5"></i></div><div><p class="lbl">Rooms</p><p class="val"><asp:Literal ID="litRooms" runat="server" Text="0" /></p></div></div>
        </div>

        <!-- Filters -->
        <div class="card p-4 mt-4">
            <div class="grid grid-cols-1 md:grid-cols-5 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlFYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlFYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlFTerm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Examination</label><asp:DropDownList ID="ddlFExam" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlFStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="All Status" Value="" /><asp:ListItem Text="Draft" Value="Draft" /><asp:ListItem Text="Scheduled" Value="Scheduled" /><asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList></div>
                <div class="flex items-end gap-2">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="es-grid">
            <!-- LEFT: add/edit form -->
            <div class="card p-4">
                <h2 class="text-sm font-bold mb-3">Add / Edit Schedule</h2>
                <asp:HiddenField ID="hfId" runat="server" Value="0" />
                <div class="fld"><label>Examination <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlExam" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlExam_Changed" /></div>
                <div class="fld"><label>Class <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div class="fld"><label>Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSection_Changed" /></div>
                <div class="fld"><label>Subject <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlSubject" runat="server" CssClass="input" /></div>
                <div class="grid grid-cols-2 gap-2">
                    <div class="fld"><label>Date <span class="text-red-500">*</span></label><asp:TextBox ID="txtDate" runat="server" CssClass="input" TextMode="Date" /></div>
                    <div class="fld"><label>Room <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlRoom" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlRoom_Changed" /></div>
                    <div class="fld"><label>Start Time <span class="text-red-500">*</span></label><asp:TextBox ID="txtStart" runat="server" CssClass="input" TextMode="Time" /></div>
                    <div class="fld"><label>End Time <span class="text-red-500">*</span></label><asp:TextBox ID="txtEnd" runat="server" CssClass="input" TextMode="Time" /></div>
                </div>
                <div class="fld"><label>Invigilator <span class="text-red-500">*</span></label><asp:DropDownList ID="ddlInvigilator" runat="server" CssClass="input" /></div>
                <asp:Panel ID="pnlCapacity" runat="server" Visible="false" CssClass="rounded-lg p-2 mb-2 text-xs bg-amber-50 text-amber-800 border border-amber-100"><asp:Literal ID="litCapacity" runat="server" /></asp:Panel>
                <div class="fld"><label>Notes</label><asp:TextBox ID="txtNotes" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" /></div>
                <div class="fld"><label>Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="Scheduled" Value="Scheduled" /><asp:ListItem Text="Draft" Value="Draft" /><asp:ListItem Text="Cancelled" Value="Cancelled" />
                    </asp:DropDownList></div>
                <div class="flex justify-end gap-2 mt-2">
                    <asp:Button ID="btnClear" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnSave" runat="server" Text="Save Schedule" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>
            </div>

            <!-- RIGHT: schedule table -->
            <div class="card overflow-hidden">
                <div class="card-head justify-between flex items-center"><h2 class="text-sm font-bold">Scheduled Exams</h2></div>
                <div class="overflow-x-auto">
                    <asp:GridView ID="gv" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl" OnRowCommand="gv_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="ExamDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy}" />
                            <asp:BoundField DataField="ClassName" HeaderText="Class" />
                            <asp:TemplateField HeaderText="Section"><ItemTemplate><%# Eval("SectionName")==System.DBNull.Value ? "All" : Server.HtmlEncode(Convert.ToString(Eval("SectionName"))) %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="SubjectName" HeaderText="Subject" />
                            <asp:TemplateField HeaderText="Time"><ItemTemplate><%# Time(Eval("StartTime")) %> – <%# Time(Eval("EndTime")) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Room"><ItemTemplate><%# Eval("RoomName")==System.DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(Eval("RoomName"))) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Invigilator"><ItemTemplate><%# Eval("InvigilatorName")==System.DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(Eval("InvigilatorName"))) %></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Status"><ItemTemplate><span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("Status"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Status"))) %></span></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                                <div class="flex items-center gap-1">
                                    <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditRow" CommandArgument='<%# Eval("ExamScheduleID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                    <asp:LinkButton runat="server" CssClass="ico-btn danger" CommandName="DeleteRow" CommandArgument='<%# Eval("ExamScheduleID") %>' ToolTip="Remove" OnClientClick="return confirm('Remove this schedule entry?');"><i data-lucide="trash-2" class="w-4 h-4"></i></asp:LinkButton>
                                </div>
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No schedules. Select an examination and add subjects.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
