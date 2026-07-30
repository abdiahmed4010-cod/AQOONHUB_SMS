<%@ Page Title="Examination Details | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ExaminationDetails.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.ExaminationDetails" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .ed-wrap { padding:1.25rem; max-width:1300px; margin:0 auto; }
        .ed-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:1000px){ .ed-grid { grid-template-columns:2fr 1fr; align-items:start; } }
        .kv { display:flex; justify-content:space-between; padding:.5rem 0; border-bottom:1px solid #f1f5f9; font-size:.85rem; }
        .kv .k { color:#64748B; } .kv .v { font-weight:700; text-align:right; }
        .chip { display:inline-block; padding:.2rem .55rem; border-radius:999px; background:#EFF6FF; color:#1D4ED8; font-size:.72rem; font-weight:600; margin:.15rem .15rem 0 0; }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.64rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; }
        .tbl td { padding:.6rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.83rem; }
        @media (max-width:768px){ .ed-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ed-wrap">
        <asp:Panel ID="pnlNotFound" runat="server" Visible="false" CssClass="card p-10 text-center text-gray-500">
            Examination not found. <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="text-brand-600">Back to list</a>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server" Visible="false">
            <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
                <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
                <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Details</span>
            </nav>
            <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
                <div class="flex items-center gap-3">
                    <h1 class="text-xl md:text-2xl font-bold tracking-tight"><asp:Literal ID="litName" runat="server" /></h1>
                    <asp:Label ID="lblStatus" runat="server" CssClass="badge" />
                </div>
                <div class="flex flex-wrap gap-2">
                    <asp:HyperLink ID="lnkEdit" runat="server" CssClass="btn btn-secondary"><i data-lucide="pencil" class="w-4 h-4"></i> Edit</asp:HyperLink>
                    <asp:HyperLink ID="lnkSchedule" runat="server" CssClass="btn btn-secondary" Visible="false"><i data-lucide="calendar-clock" class="w-4 h-4"></i> Manage Schedule</asp:HyperLink>
                    <asp:HyperLink ID="lnkMarks" runat="server" CssClass="btn btn-secondary" Visible="false"><i data-lucide="pencil-ruler" class="w-4 h-4"></i> Enter Marks</asp:HyperLink>
                    <asp:HyperLink ID="lnkResults" runat="server" CssClass="btn btn-secondary" Visible="false"><i data-lucide="bar-chart-3" class="w-4 h-4"></i> View Results</asp:HyperLink>
                    <asp:Button ID="btnActivate" runat="server" Text="Activate" CssClass="btn btn-primary" OnClick="btnActivate_Click" Visible="false" />
                    <asp:Button ID="btnCancel" runat="server" Text="Cancel Exam" CssClass="btn btn-secondary" OnClick="btnCancel_Click" Visible="false"
                        OnClientClick="return confirm('Cancel this examination? It becomes read-only.');" />
                    <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-secondary" OnClick="btnDelete_Click" Visible="false"
                        OnClientClick="return confirm('Delete this draft examination and its scope rows?');" />
                </div>
            </div>

            <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

            <div class="ed-grid">
                <div class="space-y-4">
                    <div class="card p-5">
                        <h2 class="text-sm font-bold mb-3">Examination</h2>
                        <div class="kv"><span class="k">Exam Type</span><span class="v"><asp:Literal ID="litType" runat="server" /></span></div>
                        <div class="kv"><span class="k">Academic Year</span><span class="v"><asp:Literal ID="litYear" runat="server" /></span></div>
                        <div class="kv"><span class="k">Term</span><span class="v"><asp:Literal ID="litTerm" runat="server" /></span></div>
                        <div class="kv"><span class="k">Duration</span><span class="v"><asp:Literal ID="litDuration" runat="server" /></span></div>
                        <div class="kv"><span class="k">Total Marks</span><span class="v"><asp:Literal ID="litTotal" runat="server" /></span></div>
                        <div class="kv"><span class="k">Passing Mark</span><span class="v"><asp:Literal ID="litPass" runat="server" />%</span></div>
                        <div class="kv"><span class="k">Weight</span><span class="v"><asp:Literal ID="litWeight" runat="server" />%</span></div>
                        <div class="kv"><span class="k">Created By</span><span class="v"><asp:Literal ID="litCreatedBy" runat="server" /></span></div>
                    </div>

                    <div class="card overflow-hidden">
                        <div class="card-head"><h2 class="text-sm font-bold">Subjects</h2></div>
                        <div class="overflow-x-auto">
                            <asp:GridView ID="gvSubjects" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                                <Columns>
                                    <asp:BoundField DataField="SubjectName" HeaderText="Subject" />
                                    <asp:BoundField DataField="TotalMarks" HeaderText="Total Marks" />
                                    <asp:BoundField DataField="PassingMark" HeaderText="Passing Mark" />
                                </Columns>
                                <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No subjects.</div></EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </div>
                </div>

                <div class="space-y-4">
                    <div class="card p-5">
                        <h2 class="text-sm font-bold mb-2">Scope</h2>
                        <p class="text-xs font-bold text-slate-600 mb-1">Classes / Sections</p>
                        <div><asp:Literal ID="litScope" runat="server" /></div>
                    </div>
                    <div class="card p-5">
                        <h2 class="text-sm font-bold mb-3">Progress</h2>
                        <div class="kv"><span class="k">Schedule</span><span class="v"><asp:Literal ID="litSchedule" runat="server" Text="Not Scheduled" /></span></div>
                        <div class="kv"><span class="k">Marks Entry</span><span class="v"><asp:Literal ID="litMarks" runat="server" Text="Not Started" /></span></div>
                        <div class="kv"><span class="k">Publication</span><span class="v"><asp:Literal ID="litPublication" runat="server" Text="Not Published" /></span></div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
