<%@ Page Title="Classes & Sections | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ClassesSections.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Academic.ClassesSections" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .cs-wrap { padding:1.25rem; max-width:1500px; margin:0 auto; }
        .cls-cards { display:grid; grid-template-columns:repeat(2,1fr); gap:.85rem; }
        @media (min-width:640px){ .cls-cards { grid-template-columns:repeat(3,1fr); } }
        @media (min-width:1024px){ .cls-cards { grid-template-columns:repeat(6,1fr); } }
        .cls-card { padding:.9rem; text-align:left; border:1px solid #E5E7EB; border-radius:14px; background:#fff; display:block; text-decoration:none; color:inherit; }
        .cls-card:hover { border-color:#93C5FD; }
        .cls-card.sel { border-color:#2563EB; box-shadow:0 0 0 2px #2563EB22; }
        .cls-ico { width:40px; height:40px; border-radius:11px; display:flex; align-items:center; justify-content:center; margin-bottom:.5rem; }
        .cs-table { width:100%; border-collapse:collapse; }
        .cs-table th { padding:.65rem 1rem; background:#f8fafc; text-align:left; font-size:.66rem; font-weight:700; text-transform:uppercase; letter-spacing:.04em; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .cs-table td { padding:.65rem 1rem; border-bottom:1px solid #f1f5f9; font-size:.84rem; white-space:nowrap; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:30px; height:30px; border-radius:8px; color:#64748B; }
        .ico-btn:hover { background:#EFF6FF; color:#2563EB; }
        .drawer-back { position:fixed; inset:0; background:rgba(15,23,42,.45); z-index:60; }
        .drawer { position:fixed; top:0; right:0; height:100%; width:100%; max-width:420px; background:#fff; z-index:61; box-shadow:-8px 0 24px rgba(0,0,0,.12); overflow-y:auto; }
        .drawer-head { padding:1.1rem 1.25rem; border-bottom:1px solid #E5E7EB; display:flex; justify-content:space-between; align-items:center; }
        .drawer-body { padding:1.25rem; }
        .mini-grid { display:grid; grid-template-columns:repeat(4,1fr); gap:.85rem; }
        @media (max-width:768px){ .cs-wrap { padding:.875rem; } .mini-grid { grid-template-columns:repeat(2,1fr); } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="cs-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Academic/Academics.aspx" runat="server" class="hover:text-brand-600">Academics</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Classes &amp; Sections</span>
        </nav>
        <div class="flex flex-wrap items-center justify-between gap-3 mb-4">
            <div>
                <h1 class="text-xl md:text-2xl font-bold tracking-tight">Classes &amp; Sections</h1>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage classes, sections, capacity, class teachers and student distribution.</p>
            </div>
            <asp:Button ID="btnAddClass" runat="server" Text="+ Add Class" CssClass="btn btn-primary" OnClick="btnAddClass_Click" CausesValidation="false" />
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <!-- Filters -->
        <div class="card p-4 mb-4">
            <div class="grid grid-cols-1 md:grid-cols-4 gap-3">
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year</label>
                    <asp:DropDownList ID="ddlYearFilter" runat="server" CssClass="input" /></div>
                <div class="md:col-span-2"><label class="block text-xs font-bold text-slate-700 mb-1.5">Search Class</label>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="input" placeholder="Class name or code..." /></div>
                <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status</label>
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="input">
                        <asp:ListItem Text="All Status" Value="" />
                        <asp:ListItem Text="Active" Value="Active" />
                        <asp:ListItem Text="Inactive" Value="Inactive" />
                        <asp:ListItem Text="Archived" Value="Archived" />
                    </asp:DropDownList></div>
            </div>
            <div class="mt-3 text-right"><asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary" OnClick="btnFilter_Click" CausesValidation="false" /></div>
        </div>

        <!-- Class cards -->
        <asp:Repeater ID="rptClasses" runat="server" OnItemCommand="rptClasses_ItemCommand">
            <HeaderTemplate><div class="cls-cards mb-5"></HeaderTemplate>
            <ItemTemplate>
                <asp:LinkButton runat="server" CommandName="Select" CommandArgument='<%# Eval("ClassID") %>'
                    CssClass='<%# "cls-card" + (IsSelected(Eval("ClassID")) ? " sel" : "") %>'>
                    <span class="cls-ico" style="background:#EDE9FE;color:#7C3AED"><i data-lucide="graduation-cap" class="w-5 h-5"></i></span>
                    <span class="block font-bold text-sm"><%# Server.HtmlEncode(Convert.ToString(Eval("ClassName"))) %></span>
                    <span class="block text-[11px] text-gray-500 mb-1"><%# Server.HtmlEncode(Convert.ToString(Eval("ClassCode"))) %> · <%# Server.HtmlEncode(Convert.ToString(Eval("Level"))) %></span>
                    <span class="block text-xs text-gray-600"><%# Eval("SectionCount") %> Sections</span>
                    <span class="block text-xs text-gray-600"><%# Eval("StudentCount") %> Students</span>
                    <span class="block text-[11px] text-gray-400">Capacity: <%# Eval("Capacity") %></span>
                </asp:LinkButton>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>
        <asp:Panel ID="pnlNoClasses" runat="server" Visible="false" CssClass="card p-10 text-center text-sm text-gray-500 mb-5">No classes found. Click “Add Class” to create one.</asp:Panel>

        <!-- Sections of selected class -->
        <asp:Panel ID="pnlSections" runat="server" Visible="false" CssClass="card overflow-hidden mb-5">
            <div class="card-head justify-between flex items-center">
                <h2 class="text-sm font-bold">Sections in <asp:Literal ID="litSelClass" runat="server" /></h2>
                <asp:Button ID="btnAddSection" runat="server" Text="+ Add Section" CssClass="btn btn-primary" OnClick="btnAddSection_Click" CausesValidation="false" />
            </div>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvSections" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="cs-table" OnRowCommand="gvSections_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="Section Name"><ItemTemplate><span class="font-semibold"><%# Server.HtmlEncode(Convert.ToString(Eval("SectionName"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Class Teacher"><ItemTemplate><%# Eval("TeacherName") == null || Eval("TeacherName") == System.DBNull.Value ? "<span class='text-gray-400'>—</span>" : Server.HtmlEncode(Convert.ToString(Eval("TeacherName"))) %></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Room"><ItemTemplate><%# Eval("RoomNumber") == System.DBNull.Value ? "—" : Server.HtmlEncode(Convert.ToString(Eval("RoomNumber"))) %></ItemTemplate></asp:TemplateField>
                        <asp:BoundField DataField="StudentCount" HeaderText="Students" />
                        <asp:BoundField DataField="Capacity" HeaderText="Capacity" />
                        <asp:TemplateField HeaderText="Status"><ItemTemplate><span class="badge" style='<%# StatusStyle(Convert.ToString(Eval("Status"))) %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Status"))) %></span></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                            <div class="flex items-center gap-1">
                                <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditSection" CommandArgument='<%# Eval("SectionID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="ArchiveSection" CommandArgument='<%# Eval("SectionID") %>' ToolTip="Archive"><i data-lucide="archive" class="w-4 h-4"></i></asp:LinkButton>
                            </div>
                        </ItemTemplate></asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><div class="py-10 text-center text-sm text-gray-500">No sections in this class yet.</div></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </asp:Panel>

        <!-- Summary tiles -->
        <div class="mini-grid">
            <div class="card sum-card p-4"><div><p class="text-xs text-gray-500 font-semibold">Total Classes</p><p class="text-2xl font-extrabold"><asp:Literal ID="litTotClasses" runat="server" Text="0" /></p></div></div>
            <div class="card sum-card p-4"><div><p class="text-xs text-gray-500 font-semibold">Total Sections</p><p class="text-2xl font-extrabold"><asp:Literal ID="litTotSections" runat="server" Text="0" /></p></div></div>
            <div class="card sum-card p-4"><div><p class="text-xs text-gray-500 font-semibold">Total Students</p><p class="text-2xl font-extrabold"><asp:Literal ID="litTotStudents" runat="server" Text="0" /></p></div></div>
            <div class="card sum-card p-4"><div><p class="text-xs text-gray-500 font-semibold">Avg Occupancy</p><p class="text-2xl font-extrabold"><asp:Literal ID="litOccupancy" runat="server" Text="0%" /></p></div></div>
        </div>

        <!-- ===== CLASS DRAWER ===== -->
        <asp:Panel ID="pnlClassDrawer" runat="server" Visible="false">
            <div class="drawer-back"></div>
            <div class="drawer">
                <div class="drawer-head">
                    <h3 class="font-bold text-base"><asp:Literal ID="litClassTitle" runat="server" Text="Add Class" /></h3>
                    <asp:LinkButton ID="btnCloseClass" runat="server" CssClass="ico-btn" OnClick="btnCancelClass_Click" CausesValidation="false"><i data-lucide="x" class="w-5 h-5"></i></asp:LinkButton>
                </div>
                <div class="drawer-body">
                    <asp:HiddenField ID="hfClassId" runat="server" Value="0" />
                    <div class="space-y-4">
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtClassName" runat="server" CssClass="input" placeholder="e.g. Form 1" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class Code <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtClassCode" runat="server" CssClass="input" placeholder="e.g. F1" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Level <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlLevel" runat="server" CssClass="input">
                                <asp:ListItem Text="Primary" Value="Primary" />
                                <asp:ListItem Text="Secondary" Value="Secondary" />
                            </asp:DropDownList></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Capacity <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtClassCapacity" runat="server" CssClass="input" TextMode="Number" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlClassYear" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlClassStatus" runat="server" CssClass="input">
                                <asp:ListItem Text="Active" Value="Active" />
                                <asp:ListItem Text="Inactive" Value="Inactive" />
                                <asp:ListItem Text="Archived" Value="Archived" />
                            </asp:DropDownList></div>
                    </div>
                    <div class="flex justify-end gap-2 mt-6">
                        <asp:Button ID="btnCancelClass" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancelClass_Click" CausesValidation="false" />
                        <asp:Button ID="btnSaveClass" runat="server" Text="Save Class" CssClass="btn btn-primary" OnClick="btnSaveClass_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- ===== SECTION DRAWER ===== -->
        <asp:Panel ID="pnlSectionDrawer" runat="server" Visible="false">
            <div class="drawer-back"></div>
            <div class="drawer">
                <div class="drawer-head">
                    <h3 class="font-bold text-base"><asp:Literal ID="litSectionTitle" runat="server" Text="Add New Section" /></h3>
                    <asp:LinkButton ID="btnCloseSection" runat="server" CssClass="ico-btn" OnClick="btnCancelSection_Click" CausesValidation="false"><i data-lucide="x" class="w-5 h-5"></i></asp:LinkButton>
                </div>
                <div class="drawer-body">
                    <asp:HiddenField ID="hfSectionId" runat="server" Value="0" />
                    <div class="space-y-4">
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlSectionClass" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Section Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtSectionName" runat="server" CssClass="input" placeholder="e.g. Form 1A" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Class Teacher</label>
                            <asp:DropDownList ID="ddlSectionTeacher" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Room</label>
                            <asp:TextBox ID="txtSectionRoom" runat="server" CssClass="input" placeholder="e.g. R-12" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Capacity <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtSectionCapacity" runat="server" CssClass="input" TextMode="Number" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Academic Year <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlSectionYear" runat="server" CssClass="input" /></div>
                        <div><label class="block text-xs font-bold text-slate-700 mb-1.5">Status <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlSectionStatus" runat="server" CssClass="input">
                                <asp:ListItem Text="Active" Value="Active" />
                                <asp:ListItem Text="Inactive" Value="Inactive" />
                                <asp:ListItem Text="Archived" Value="Archived" />
                            </asp:DropDownList></div>
                    </div>
                    <div class="flex justify-end gap-2 mt-6">
                        <asp:Button ID="btnCancelSection" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancelSection_Click" CausesValidation="false" />
                        <asp:Button ID="btnSaveSection" runat="server" Text="Save Section" CssClass="btn btn-primary" OnClick="btnSaveSection_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
