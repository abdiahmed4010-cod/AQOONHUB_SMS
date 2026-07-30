<%@ Page Title="Report Cards | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ReportCards.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.ReportCards" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .rc-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.62rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.6rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.82rem; white-space:nowrap; }
        @media (max-width:768px){ .rc-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rc-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Report Cards</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Report Cards</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Search and select a student to view or print their examination report card.</p>
            </div>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-2 md:grid-cols-6 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label><asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Term</label><asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlTerm_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Examination</label><asp:DropDownList ID="ddlExam" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlExam_Changed" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section</label><asp:DropDownList ID="ddlSection" runat="server" CssClass="input" /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Student</label><asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Name or code" /></div>
                <div class="flex items-end gap-2">
                    <asp:Button ID="btnFilter" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="btnReset_Click" CausesValidation="false" />
                </div>
            </div>
        </div>

        <div class="card overflow-hidden">
            <div class="card-head"><h2 class="text-sm font-bold">Students</h2></div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvCards" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                    <Columns>
                        <asp:TemplateField HeaderText="Student"><ItemTemplate>
                            <div class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("FullName"))) %></div>
                            <div class="text-xs text-gray-500"><%# Server.HtmlEncode(Convert.ToString(Eval("StudentCode"))) %></div>
                        </ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="ClassName" HeaderText="Class" />
                        <asp:BoundField DataField="SectionName" HeaderText="Section" />
                        <asp:TemplateField HeaderText="Average"><ItemTemplate><%# Convert.ToDecimal(Eval("Average")).ToString("0.00") %>%</ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Grade"><ItemTemplate><span class="badge" style="background:#EEF2FF;color:#3730A3"><%# string.IsNullOrEmpty(Convert.ToString(Eval("Grade"))) ? "—" : Server.HtmlEncode(Convert.ToString(Eval("Grade"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Rank"><ItemTemplate><%# Convert.ToInt32(Eval("Rank"))>0 ? Convert.ToString(Eval("Rank")) : "—" %></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Result"><ItemTemplate><span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("ResultStatus"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("ResultStatus"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Publication"><ItemTemplate><span class="badge" style='<%# PubStyle() %>'><%# PublicationLabel() %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <a runat="server" href='<%# "~/Modules/Examinations/ReportCard.aspx?exam=" + CurrentExamId + "&student=" + Eval("StudentID") %>' class="text-brand-600 font-semibold text-xs">View / Print</a>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-12 text-center text-sm text-gray-500">Select an examination to list students.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
