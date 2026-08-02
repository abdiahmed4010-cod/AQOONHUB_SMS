<%@ Page Title="Report Card | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ReportCard.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.ReportCard" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .rc-wrap { padding:1.25rem; max-width:850px; margin:0 auto; }
        .rc-card { background:#fff; border:1px solid #E5E7EB; border-radius:14px; padding:1.5rem; }
        .rc-head { text-align:center; border-bottom:2px solid #1e293b; padding-bottom:1rem; margin-bottom:1rem; }
        .rc-school { font-size:1.4rem; font-weight:800; color:#0f172a; }
        .rc-info { display:grid; grid-template-columns:repeat(2,1fr); gap:.4rem 1.5rem; margin:1rem 0; font-size:.85rem; }
        .rc-info .k { color:#64748B; } .rc-info b { color:#0f172a; }
        .rc-tbl { width:100%; border-collapse:collapse; margin-top:.5rem; }
        .rc-tbl th, .rc-tbl td { border:1px solid #cbd5e1; padding:.5rem .7rem; font-size:.83rem; text-align:left; }
        .rc-tbl th { background:#f1f5f9; font-weight:700; }
        .rc-totals { display:grid; grid-template-columns:repeat(4,1fr); gap:.75rem; margin-top:1rem; }
        .rc-totals .box { border:1px solid #E5E7EB; border-radius:10px; padding:.6rem; text-align:center; }
        .rc-totals .box .lbl { font-size:.66rem; color:#64748B; text-transform:uppercase; font-weight:700; } .rc-totals .box .val { font-size:1.1rem; font-weight:800; }
        .rc-sign { display:flex; justify-content:space-between; margin-top:2.5rem; }
        .rc-sign div { text-align:center; font-size:.8rem; color:#64748B; } .rc-sign .line { border-top:1px solid #94a3b8; width:160px; margin-bottom:.25rem; }
        @media print {
            body * { visibility:hidden; }
            #rcArea, #rcArea * { visibility:visible; }
            #rcArea { position:absolute; left:0; top:0; width:100%; }
            .no-print { display:none !important; }
            @page { size:A4 portrait; margin:12mm; }
            .rc-card { border:none; }
        }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rc-wrap">
        <asp:Panel ID="pnlDenied" runat="server" Visible="false" CssClass="card p-10 text-center text-gray-500">
            This report card is not available. <a href="~/Modules/Examinations/Results.aspx" runat="server" class="text-brand-600">Back to results</a>
        </asp:Panel>

        <asp:Panel ID="pnlBody" runat="server" Visible="false">
            <div class="flex items-center justify-between mb-4 no-print">
                <a href="~/Modules/Examinations/Results.aspx" runat="server" class="btn btn-secondary"><i data-lucide="arrow-left" class="w-4 h-4"></i> Back</a>
                <button type="button" class="btn btn-primary" onclick="window.print()"><i data-lucide="printer" class="w-4 h-4"></i> Print</button>
            </div>

            <div id="rcArea">
                <div class="rc-card">
                    <div class="rc-head">
                        <div class="rc-school">AQOONHUB SMS</div>
                        <div class="text-sm text-gray-600">Student Examination Report Card</div>
                        <div class="text-xs text-gray-500 mt-1"><asp:Literal ID="litExam" runat="server" /></div>
                    </div>

                    <div class="rc-info">
                        <div><span class="k">Student:</span> <b><asp:Literal ID="litName" runat="server" /></b></div>
                        <div><span class="k">Student Code:</span> <b><asp:Literal ID="litCode" runat="server" /></b></div>
                        <div><span class="k">Admission No:</span> <b><asp:Literal ID="litAdm" runat="server" /></b></div>
                        <div><span class="k">Class / Section:</span> <b><asp:Literal ID="litClass" runat="server" /></b></div>
                        <div><span class="k">Academic Year:</span> <b><asp:Literal ID="litYear" runat="server" /></b></div>
                        <div><span class="k">Term:</span> <b><asp:Literal ID="litTerm" runat="server" /></b></div>
                    </div>

                    <table class="rc-tbl">
                        <thead><tr><th scope="col">Subject</th><th scope="col">Max Marks</th><th scope="col">Obtained</th><th scope="col">%</th><th scope="col">Grade</th><th scope="col">Attendance</th><th scope="col">Remarks</th></tr></thead>
                        <asp:Repeater ID="rptSubjects" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Server.HtmlEncode(Convert.ToString(Eval("SubjectName"))) %></td>
                                    <td><%# Eval("TotalMarks") %></td>
                                    <td><%# Eval("Marks")==System.DBNull.Value ? "—" : Convert.ToString(Eval("Marks")) %></td>
                                    <td><%# Pct(Eval("Marks"), Eval("TotalMarks")) %></td>
                                    <td><%# Eval("Grade")==System.DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(Eval("Grade"))) %></td>
                                    <td><%# Server.HtmlEncode(Convert.ToString(Eval("AttendanceStatus"))) %></td>
                                    <td><%# Eval("Remarks")==System.DBNull.Value ? "" : Server.HtmlEncode(Convert.ToString(Eval("Remarks"))) %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </table>

                    <div class="rc-totals">
                        <div class="box"><div class="lbl">Total</div><div class="val"><asp:Literal ID="litObtained" runat="server" /> / <asp:Literal ID="litMax" runat="server" /></div></div>
                        <div class="box"><div class="lbl">Average</div><div class="val"><asp:Literal ID="litAvg" runat="server" /></div></div>
                        <div class="box"><div class="lbl">Grade</div><div class="val"><asp:Literal ID="litGrade" runat="server" /></div></div>
                        <div class="box"><div class="lbl">Rank</div><div class="val"><asp:Literal ID="litRank" runat="server" /></div></div>
                    </div>

                    <div class="mt-3 text-sm">Result: <span class="badge" style='<%# "" %>'><asp:Literal ID="litStatus" runat="server" /></span>
                        <span class="text-xs text-gray-500 ml-3"><asp:Literal ID="litPublished" runat="server" /></span></div>

                    <div class="rc-sign">
                        <div><div class="line"></div>Class Teacher</div>
                        <div><div class="line"></div>Examination Officer</div>
                        <div><div class="line"></div>Principal</div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
