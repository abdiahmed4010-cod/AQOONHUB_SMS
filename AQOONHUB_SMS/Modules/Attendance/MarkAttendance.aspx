<%@ Page Title="Mark Attendance | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="MarkAttendance.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Attendance.MarkAttendance" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ma-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.55rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; white-space:nowrap; }
        .seg { display:inline-flex; border:1px solid #e2e8f0; border-radius:8px; overflow:hidden; }
        .seg label { padding:.28rem .55rem; font-size:.72rem; font-weight:700; cursor:pointer; color:#64748B; background:#fff; }
        .seg input { position:absolute; opacity:0; width:0; height:0; }
        .seg input:checked + label.p { background:#DCFCE7; color:#15803D; }
        .seg input:checked + label.l { background:#FEF3C7; color:#B45309; }
        .seg input:checked + label.a { background:#FEE2E2; color:#DC2626; }
        .seg input:checked + label.e { background:#EDE9FE; color:#6D28D9; }
        @media (max-width:768px){ .ma-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ma-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Attendance/Attendance.aspx" runat="server" class="hover:text-brand-600">Attendance</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Mark Attendance</span>
        </nav>
        <div class="mb-4">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Mark Attendance</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Record daily attendance for students in the selected class and section.</p>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Date</label><asp:TextBox ID="txtDate" runat="server" CssClass="input" TextMode="Date" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class</label><asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Session Type</label>
                    <asp:DropDownList ID="ddlSessionType" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSessionType_Changed">
                        <asp:ListItem Text="Daily" Value="Daily" /><asp:ListItem Text="Morning" Value="Morning" />
                        <asp:ListItem Text="Afternoon" Value="Afternoon" /><asp:ListItem Text="Subject" Value="Subject" />
                    </asp:DropDownList></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Subject</label><asp:DropDownList ID="ddlSubject" runat="server" CssClass="input" Enabled="false" /></div>
            </div>
            <div class="flex flex-wrap items-end gap-2 mt-3">
                <div class="flex-1 min-w-[180px]"><label class="block text-xs font-bold text-slate-700 mb-1.5">Search Student</label><asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Name or code" /></div>
                <asp:Button ID="btnLoad" runat="server" Text="Load Students" CssClass="btn btn-primary" OnClick="btnLoad_Click" CausesValidation="false" />
                <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
            </div>
        </div>

        <asp:Panel ID="pnlSession" runat="server" Visible="false" CssClass="flex flex-wrap items-center gap-3 mb-3 text-sm">
            <asp:Label ID="lblStatus" runat="server" CssClass="badge" Text="Draft" />
            <span class="text-gray-500"><asp:Literal ID="litSessionInfo" runat="server" /></span>
        </asp:Panel>

        <asp:Panel ID="pnlGrid" runat="server" Visible="false">
            <!-- Bulk actions -->
            <div class="card p-3 mb-3 flex flex-wrap gap-2 items-center">
                <span class="text-xs font-bold text-slate-600 mr-1">Bulk:</span>
                <button type="button" class="btn btn-secondary" onclick="maBulk('Present')">Mark All Present</button>
                <button type="button" class="btn btn-secondary" onclick="maBulkSel('Present')">Selected Present</button>
                <button type="button" class="btn btn-secondary" onclick="maBulkSel('Late')">Selected Late</button>
                <button type="button" class="btn btn-secondary" onclick="maBulkSel('Absent')">Selected Absent</button>
                <button type="button" class="btn btn-secondary" onclick="maBulkSel('Excused')">Selected Excused</button>
                <button type="button" class="btn btn-secondary" onclick="maBulkSel('')">Clear Selected</button>
            </div>

            <div class="card overflow-hidden mb-3">
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvStudents" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl" DataKeyNames="StudentID">
                        <Columns>
                            <asp:TemplateField><HeaderTemplate><input type="checkbox" onclick="maToggleAll(this)" /></HeaderTemplate>
                                <ItemTemplate><input type="checkbox" class="ma-sel" /></ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Student"><ItemTemplate>
                                <input type="hidden" class="ma-sid" value='<%# Eval("StudentID") %>' />
                                <div class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("FullName"))) %></div>
                                <div class="text-xs text-gray-500"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentCode"))) %> · <%# Server.HtmlEncode(Convert.ToString(Eval("AdmissionNo"))) %></div>
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Status"><ItemTemplate>
                                <div class="seg" data-sid='<%# Eval("StudentID") %>'>
                                    <input type="radio" id='p<%# Eval("StudentID") %>' name='st<%# Eval("StudentID") %>' value="Present" <%# Chk(Eval("StudentID"),"Present") %> /><label class="p" for='p<%# Eval("StudentID") %>'>P</label>
                                    <input type="radio" id='l<%# Eval("StudentID") %>' name='st<%# Eval("StudentID") %>' value="Late" <%# Chk(Eval("StudentID"),"Late") %> /><label class="l" for='l<%# Eval("StudentID") %>'>L</label>
                                    <input type="radio" id='a<%# Eval("StudentID") %>' name='st<%# Eval("StudentID") %>' value="Absent" <%# Chk(Eval("StudentID"),"Absent") %> /><label class="a" for='a<%# Eval("StudentID") %>'>A</label>
                                    <input type="radio" id='e<%# Eval("StudentID") %>' name='st<%# Eval("StudentID") %>' value="Excused" <%# Chk(Eval("StudentID"),"Excused") %> /><label class="e" for='e<%# Eval("StudentID") %>'>E</label>
                                </div>
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Check-in"><ItemTemplate>
                                <input type="time" class="input ma-time" style="width:120px" data-sid='<%# Eval("StudentID") %>' value='<%# TimeVal(Eval("CheckInTime")) %>' />
                            </ItemTemplate></asp:TemplateField>
                            <asp:TemplateField HeaderText="Remarks"><ItemTemplate>
                                <input type="text" class="input ma-rem" style="width:180px" maxlength="300" data-sid='<%# Eval("StudentID") %>' value='<%# Server.HtmlEncode(Convert.ToString(Eval("Remarks"))) %>' />
                            </ItemTemplate></asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">No active students in this scope.</div></EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <!-- Hidden payload posted to server (JS serialises the grid; server revalidates everything) -->
            <asp:HiddenField ID="hfPayload" runat="server" />

            <div class="flex flex-wrap gap-2">
                <asp:Button ID="btnDraft" runat="server" Text="Save Draft" CssClass="btn btn-secondary" OnClientClick="return maCollect();" OnClick="btnDraft_Click" CausesValidation="false" />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit Attendance" CssClass="btn btn-primary" OnClientClick="return maCollect();" OnClick="btnSubmit_Click" CausesValidation="false" />
                <asp:Button ID="btnReopen" runat="server" Text="Reopen" CssClass="btn btn-secondary" Visible="false" OnClick="btnReopen_Click" CausesValidation="false" OnClientClick="return maReopen();" />
                <asp:HiddenField ID="hfReopenReason" runat="server" />
            </div>
        </asp:Panel>
    </div>

    <script>
        function maToggleAll(cb){ document.querySelectorAll('.ma-sel').forEach(function(x){ x.checked = cb.checked; }); }
        function maSet(sid, status){
            var g = document.querySelector('.seg[data-sid="'+sid+'"]'); if(!g) return;
            g.querySelectorAll('input[type=radio]').forEach(function(r){ r.checked = (r.value===status); });
        }
        function maBulk(status){ document.querySelectorAll('.seg').forEach(function(g){ maSet(g.getAttribute('data-sid'), status); }); }
        function maBulkSel(status){
            document.querySelectorAll('#<%= gvStudents.ClientID %> tr').forEach(function(tr){
                var sel = tr.querySelector('.ma-sel'); var sidEl = tr.querySelector('.ma-sid');
                if(sel && sel.checked && sidEl){ maSet(sidEl.value, status); }
            });
        }
        function maCollect(){
            var rows = [];
            document.querySelectorAll('.seg').forEach(function(g){
                var sid = g.getAttribute('data-sid');
                var picked = g.querySelector('input[type=radio]:checked');
                var status = picked ? picked.value : '';
                var time = document.querySelector('.ma-time[data-sid="'+sid+'"]');
                var rem = document.querySelector('.ma-rem[data-sid="'+sid+'"]');
                rows.push({ s: parseInt(sid,10), st: status, ci: time?time.value:'', rm: rem?rem.value:'' });
            });
            document.getElementById('<%= hfPayload.ClientID %>').value = JSON.stringify(rows);
            return true;
        }
        function maReopen(){
            var r = prompt('Enter a reason to reopen this submitted attendance:');
            if(r===null || r.trim()===''){ return false; }
            document.getElementById('<%= hfReopenReason.ClientID %>').value = r.trim();
            return true;
        }
    </script>
</asp:Content>
