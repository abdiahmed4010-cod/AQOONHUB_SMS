<%@ Page Title="Transcript Center | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="Transcripts.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.Transcripts" %>

<asp:Content ID="cHead" ContentPlaceHolderID="head" runat="server">
    <style>
        .tc-wrap { padding: 1.25rem; max-width: 1200px; margin: 0 auto; }
        .tc-filter label { display:block; font-size:.72rem; font-weight:700; color:#374151; margin-bottom:.3rem; }
        .dark .tc-filter label { color:#CBD5E1; }
        .tc-grid { display:grid; grid-template-columns:repeat(2,1fr); gap:.8rem; }
        @media (min-width:768px){ .tc-grid { grid-template-columns:repeat(4,1fr); } }
        .tc-actions { display:flex; gap:.5rem; flex-wrap:wrap; }

        /* ---- Transcript document (premium) ---- */
        .tr-doc { --acc:#1E3A8A; --acc2:#2563EB; background:#fff; color:#0F172A; max-width:820px; margin:1.25rem auto; border:1px solid #E5E7EB; border-radius:.6rem; box-shadow:0 6px 24px -12px rgba(15,23,42,.25); padding:30px 34px; }
        .tr-doc.theme-purple { --acc:#5B21B6; --acc2:#7C3AED; }

        .tr-top { display:flex; align-items:flex-start; gap:16px; flex-wrap:wrap; }
        .tr-top img { width:66px; height:66px; object-fit:contain; }
        .tr-brand .nm { font-size:1.45rem; font-weight:800; color:var(--acc); line-height:1; letter-spacing:-.01em; }
        .tr-brand .nm em { font-style:normal; color:var(--acc2); }
        .tr-brand .tag { font-size:.6rem; font-weight:700; letter-spacing:.14em; color:#64748B; margin-top:4px; text-transform:uppercase; }
        .tr-contacts { font-size:.72rem; color:#475569; display:grid; gap:3px; }
        .tr-contacts div { display:flex; align-items:center; gap:6px; }
        .tr-contacts svg { width:13px; height:13px; color:var(--acc2); flex-shrink:0; }
        .tr-refbox { margin-left:auto; border:1px solid #E2E8F0; border-radius:.5rem; padding:8px 12px; min-width:150px; font-size:.68rem; color:#475569; }
        .tr-refbox .rl { text-align:center; }
        .tr-pill { display:inline-block; font-weight:800; color:var(--acc); border:1px solid var(--acc2); background:#EEF2FF; border-radius:.4rem; padding:2px 8px; margin-top:2px; }
        .theme-purple .tr-pill { background:#F3E9FF; }
        .tr-refbox .gen { margin-top:6px; text-align:center; }
        .tr-badge { display:inline-block; font-size:.62rem; font-weight:800; letter-spacing:.06em; padding:.15rem .55rem; border-radius:999px; }
        .tr-badge.official { background:#DCFCE7; color:#15803D; }
        .tr-badge.draft { background:#FEF3C7; color:#B45309; }
        .tr-pageno { text-align:right; font-size:.66rem; color:#64748B; margin-top:2px; }

        .tr-doctitle { text-align:center; font-size:1.1rem; font-weight:800; letter-spacing:.14em; color:var(--acc2); margin:16px 0 6px; }

        .tr-bar { display:flex; align-items:center; gap:8px; background:var(--acc); color:#fff; border-radius:.45rem; padding:6px 12px; margin:16px 0 8px; }
        .tr-bar svg { width:15px; height:15px; }
        .tr-bar span { font-size:.7rem; font-weight:800; letter-spacing:.09em; text-transform:uppercase; }

        .tr-info { display:grid; grid-template-columns:repeat(2,1fr); gap:5px 28px; font-size:.8rem; }
        @media (min-width:640px){ .tr-info { grid-template-columns:repeat(3,1fr); } }
        .tr-info .k { color:#64748B; }
        .tr-info .v { font-weight:700; color:#0F172A; }

        table.tr-table { width:100%; border-collapse:collapse; font-size:.76rem; margin-top:2px; }
        table.tr-table th { background:var(--acc); color:#fff; text-align:left; font-weight:700; padding:6px 8px; border:1px solid var(--acc); }
        table.tr-table td { padding:5px 8px; border:1px solid #E2E8F0; }
        table.tr-table tbody tr:nth-child(even) td, table.tr-table tbody tr:nth-child(even) td { background:#F8FAFC; }
        table.tr-table td.num, table.tr-table th.num { text-align:center; }
        .tr-gr { display:inline-block; min-width:26px; text-align:center; font-weight:800; border-radius:999px; padding:1px 7px; font-size:.7rem; background:#E7EFFE; color:var(--acc); }

        .tr-summary { display:grid; grid-template-columns:repeat(2,1fr); gap:8px; font-size:.8rem; margin-top:10px; }
        @media (min-width:640px){ .tr-summary { grid-template-columns:repeat(4,1fr); } }
        .tr-summary .box { background:#F8FAFC; border:1px solid #E5E7EB; border-radius:.5rem; padding:9px 11px; text-align:center; }
        .tr-summary .box .lbl { font-size:.62rem; color:#64748B; text-transform:uppercase; letter-spacing:.04em; }
        .tr-summary .box .val { font-size:1.05rem; font-weight:800; color:var(--acc); margin-top:2px; }
        .tr-summary .box .val.ok { color:#15803D; }
        .tr-summary .box .val.bad { color:#DC2626; }
        .tr-subrow { display:grid; grid-template-columns:repeat(3,1fr); gap:8px; margin-top:8px; font-size:.8rem; }
        .tr-subrow .b { border:1px solid #E5E7EB; border-radius:.5rem; padding:7px 11px; }
        .tr-subrow .b .k { color:#64748B; font-size:.68rem; }
        .tr-subrow .b .v { font-weight:700; }

        .tr-year-title { font-size:.9rem; font-weight:800; color:#fff; margin:18px 0 8px; padding:6px 12px; background:var(--acc); border-radius:.4rem; }
        .tr-yeargrid { display:grid; grid-template-columns:1fr; gap:14px; }
        @media (min-width:680px){ .tr-yeargrid { grid-template-columns:1fr 1fr; } }
        .tr-term-title { font-size:.72rem; font-weight:800; letter-spacing:.06em; color:var(--acc2); text-transform:uppercase; margin:8px 0 3px; }
        .tr-yearsum { background:#F1F5F9; border:1px solid #E2E8F0; border-radius:.5rem; padding:8px 12px; font-size:.76rem; margin-top:8px; display:flex; flex-wrap:wrap; gap:6px 22px; }
        .tr-yearsum b { color:var(--acc); }

        .tr-cumbox { border:1px solid #E2E8F0; border-radius:.5rem; padding:12px 14px; font-size:.8rem; display:grid; grid-template-columns:1fr; gap:4px 24px; }
        @media (min-width:560px){ .tr-cumbox { grid-template-columns:1fr 1fr; } }
        .tr-cumbox .k { color:#64748B; }
        .tr-cumbox .v { font-weight:800; color:var(--acc); }

        .tr-scale { display:flex; flex-wrap:wrap; gap:8px; }
        .tr-scale .chip { border-radius:.45rem; padding:5px 12px; text-align:center; font-size:.7rem; border:1px solid transparent; }
        .tr-scale .chip .g { font-weight:800; }
        .tr-scale .chip .r { font-size:.62rem; opacity:.85; }
        .sc-a { background:#DCFCE7; color:#15803D; }
        .sc-b { background:#DBEAFE; color:#1D4ED8; }
        .sc-c { background:#FEF3C7; color:#B45309; }
        .sc-d { background:#FFEDD5; color:#C2410C; }
        .sc-f { background:#FEE2E2; color:#B91C1C; }

        .tr-cert { font-size:.76rem; color:#334155; margin-top:6px; line-height:1.55; }
        .tr-sign { display:grid; grid-template-columns:repeat(4,1fr); gap:18px; margin-top:30px; font-size:.72rem; text-align:center; }
        .tr-sign.three { grid-template-columns:repeat(3,1fr); }
        .tr-sign .sig { height:34px; display:flex; align-items:flex-end; justify-content:center; }
        .tr-sign .sig svg { width:70px; height:30px; color:#334155; opacity:.7; }
        .tr-sign .line { border-top:1px solid #475569; margin-top:4px; padding-top:5px; }
        .tr-sign .role { color:#64748B; font-size:.66rem; }
        .tr-sign .nm { font-weight:700; }
        .tr-stamp { margin:0 auto; width:82px; height:82px; border:2px dashed var(--acc2); border-radius:50%; display:flex; align-items:center; justify-content:center; color:var(--acc2); font-size:.58rem; text-align:center; }

        .tr-footer { display:flex; align-items:center; justify-content:center; gap:8px; background:var(--acc); color:#fff; border-radius:.45rem; padding:8px 12px; margin-top:22px; font-size:.72rem; }
        .tr-footer svg { width:14px; height:14px; }
        .tr-verify { text-align:center; font-size:.7rem; color:#475569; margin-top:8px; }
        .tr-verify b { color:var(--acc); }
        .tr-empty { text-align:center; padding:40px 20px; color:#64748B; }

        /* Document stays white/formal in dark app theme */
        .dark .tr-doc { background:#fff; color:#0F172A; }

        /* ---- Print: show only the transcript ---- */
        @media print {
            @page { size: A4 portrait; margin: 12mm; }
            body * { visibility: hidden !important; }
            #printArea, #printArea * { visibility: visible !important; }
            #printArea { position:absolute; left:0; top:0; width:100%; }
            .tr-doc { box-shadow:none; border:none; margin:0; max-width:none; padding:0; border-radius:0; }
            .tr-page { page-break-after: always; }
            .tr-page:last-child { page-break-after: auto; }
            table.tr-table { page-break-inside:auto; }
            table.tr-table thead { display:table-header-group; }
            table.tr-table tr { page-break-inside:avoid; }
            .tr-sign { page-break-inside:avoid; }
        }
    </style>
</asp:Content>

<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
    <div class="tc-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5 no-print">
            <a href="~/Modules/Dashboard/Dashboard.aspx" runat="server" class="hover:text-brand-600">Dashboard</a><span>/</span>
            <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a><span>/</span>
            <span class="font-semibold text-gray-700 dark:text-slate-200">Transcript Center</span>
        </nav>
        <div class="mb-5 no-print">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Transcript Center</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Generate, review, print, and save official student academic transcripts.</p>
        </div>

        <!-- Filters -->
        <div class="card p-5 no-print">
            <div class="tc-filter tc-grid">
                <div>
                    <label>Transcript Type</label>
                    <asp:DropDownList ID="ddlType" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlType_Changed">
                        <asp:ListItem Text="Term Transcript" Value="Term" />
                        <asp:ListItem Text="Full Academic Transcript" Value="Full" />
                    </asp:DropDownList>
                </div>
                <div>
                    <label>Academic Year <asp:Label ID="lblYearOpt" runat="server" CssClass="text-gray-400 font-normal" /></label>
                    <asp:DropDownList ID="ddlYear" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlYear_Changed" />
                </div>
                <div id="wrapTerm" runat="server">
                    <label>Term / Semester <asp:Label ID="lblTermOpt" runat="server" CssClass="text-gray-400 font-normal" /></label>
                    <asp:DropDownList ID="ddlTerm" runat="server" CssClass="input" />
                </div>
                <div>
                    <label>Class</label>
                    <asp:DropDownList ID="ddlClass" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlClass_Changed" />
                </div>
                <div>
                    <label>Section</label>
                    <asp:DropDownList ID="ddlSection" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ddlSection_Changed" />
                </div>
                <div>
                    <label>Student</label>
                    <asp:DropDownList ID="ddlStudent" runat="server" CssClass="input" />
                </div>
                <div class="md:col-span-2">
                    <label>Search by name, Student Code or Admission No.</label>
                    <div class="flex gap-2">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="e.g. AQH-2026-0001 or student name" />
                        <asp:Button ID="btnFind" runat="server" Text="Find" CssClass="btn btn-secondary" OnClick="btnFind_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>
            <div class="tc-actions mt-4">
                <asp:Button ID="btnLoad" runat="server" Text="Load Transcript" CssClass="btn btn-primary" OnClick="btnLoad_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                <button type="button" class="btn btn-secondary" onclick="window.print();return false;"><i data-lucide="printer" class="w-4 h-4"></i> Print / Save as PDF</button>
            </div>
        </div>

        <!-- Messages -->
        <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="mt-4 no-print" role="status" aria-live="polite">
            <div class="card p-5 flex items-start gap-3">
                <i data-lucide="info" class="w-5 h-5 text-brand-600 mt-0.5"></i>
                <asp:Label ID="lblMsg" runat="server" CssClass="text-sm text-gray-600 dark:text-slate-300" />
            </div>
        </asp:Panel>

        <!-- Transcript document -->
        <div id="printArea">
            <asp:Literal ID="litDoc" runat="server" />
        </div>
    </div>
</asp:Content>
