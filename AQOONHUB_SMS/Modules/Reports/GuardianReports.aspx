<%@ Page Title="Guardian & Parent Reports | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="GuardianReports.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Reports.GuardianReports" %>
<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .cat-wrap { padding:1.25rem; max-width:1600px; margin:0 auto; }
        .cards { display:grid; grid-template-columns:repeat(1,1fr); gap:.85rem; }
        @media (min-width:640px){ .cards { grid-template-columns:repeat(2,1fr); } }
        @media (min-width:1024px){ .cards { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1400px){ .cards { grid-template-columns:repeat(4,1fr); } }
        .rc { display:block; padding:.9rem 1rem; border:1px solid #e5e7eb; border-radius:12px; background:#fff; text-decoration:none; color:#0f172a; }
        .rc:hover { border-color:#2563EB; box-shadow:0 1px 3px rgba(0,0,0,.06); }
        .rc-t { font-size:.85rem; font-weight:700; } .rc-d { font-size:.72rem; color:#64748B; margin-top:.2rem; min-height:2rem; }
        .rc-a { font-size:.72rem; font-weight:700; color:#2563EB; margin-top:.5rem; } .rc-off { opacity:.6; } .rc-off-a { color:#94a3b8; } .rc-lock { font-size:.7rem; }
    </style>
</asp:Content>
<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="cat-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Reports/Reports.aspx" runat="server" class="hover:text-brand-600">Reports</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Guardian & Parent Reports</span>
        </nav>
        <div class="mb-4"><h1 class="text-xl md:text-2xl font-bold tracking-tight">Guardian & Parent Reports</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Guardian directory and parent-student links.</p></div>
        <asp:Literal ID="litDataSource" runat="server" />
        <div class="cards"><asp:Literal ID="litCards" runat="server" /></div>
    </div>
</asp:Content>
