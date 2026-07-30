<%@ Page Title="Invigilators & Rooms | AQOONHUB SMS" Language="C#" MasterPageFile="~/MasterPages/MainMaster.master" AutoEventWireup="true" CodeBehind="ExamRooms.aspx.cs" Inherits="AQOONHUB_SMS.Modules.Examinations.ExamRooms" %>

<asp:Content ID="h" ContentPlaceHolderID="head" runat="server">
    <style>
        .rm-wrap { padding:1.25rem; max-width:1300px; margin:0 auto; }
        .rm-grid { display:grid; grid-template-columns:1fr; gap:1rem; }
        @media (min-width:1000px){ .rm-grid { grid-template-columns:1fr 2fr; align-items:start; } }
        .tbl { width:100%; border-collapse:collapse; }
        .tbl th { padding:.6rem .8rem; background:#f8fafc; text-align:left; font-size:.64rem; font-weight:700; text-transform:uppercase; color:#475569; border-bottom:1px solid #e2e8f0; white-space:nowrap; }
        .tbl td { padding:.6rem .8rem; border-bottom:1px solid #f1f5f9; font-size:.83rem; }
        .fld { margin-bottom:.75rem; } .fld label { display:block; font-size:.72rem; font-weight:700; color:#334155; margin-bottom:.3rem; }
        .ico-btn { display:inline-flex; align-items:center; justify-content:center; width:30px; height:30px; border-radius:8px; color:#64748B; } .ico-btn:hover { background:#EFF6FF; color:#2563EB; }
        @media (max-width:768px){ .rm-wrap { padding:.875rem; } }
    </style>
</asp:Content>

<asp:Content ID="b" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rm-wrap">
        <nav class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-slate-400 mb-1.5">
            <a href="~/Modules/Examinations/Examinations.aspx" runat="server" class="hover:text-brand-600">Examinations</a>
            <span>/</span><span class="font-semibold text-gray-700 dark:text-slate-200">Invigilators &amp; Rooms</span>
        </nav>
        <div class="mb-4">
            <h1 class="text-xl md:text-2xl font-bold tracking-tight">Invigilators &amp; Rooms</h1>
            <p class="text-sm text-gray-500 dark:text-slate-400 mt-1">Manage examination rooms and view available invigilators.</p>
        </div>

        <asp:Panel ID="msg" runat="server" Visible="false" CssClass="rounded-lg p-3 mb-4 text-sm"><asp:Literal ID="msgText" runat="server" /></asp:Panel>

        <div class="rm-grid">
            <!-- Room form -->
            <div class="card p-4">
                <h2 class="text-sm font-bold mb-3"><asp:Literal ID="litFormTitle" runat="server" Text="Add Room" /></h2>
                <asp:HiddenField ID="hfId" runat="server" Value="0" />
                <div class="fld"><label>Room Name <span class="text-red-500">*</span></label><asp:TextBox ID="txtName" runat="server" CssClass="input" placeholder="e.g. Hall C" /></div>
                <div class="fld"><label>Capacity <span class="text-red-500">*</span></label><asp:TextBox ID="txtCapacity" runat="server" CssClass="input" TextMode="Number" Text="40" /></div>
                <div class="fld"><label>Location</label><asp:TextBox ID="txtLocation" runat="server" CssClass="input" placeholder="e.g. Main Block" /></div>
                <div class="fld"><label>Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="input">
                        <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="Inactive" Value="Inactive" />
                    </asp:DropDownList></div>
                <div class="flex justify-end gap-2 mt-2">
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
                    <asp:Button ID="btnSave" runat="server" Text="Save Room" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>
            </div>

            <div class="space-y-4">
                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Exam Rooms</h2></div>
                    <div class="overflow-x-auto">
                        <asp:GridView ID="gvRooms" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl" OnRowCommand="gvRooms_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="RoomName" HeaderText="Room" />
                                <asp:BoundField DataField="Capacity" HeaderText="Capacity" />
                                <asp:BoundField DataField="Location" HeaderText="Location" />
                                <asp:BoundField DataField="Bookings" HeaderText="Bookings" />
                                <asp:TemplateField HeaderText="Status"><ItemTemplate>
                                    <span class="badge" style='<%# string.Equals(Convert.ToString(Eval("Status")),"Active",StringComparison.OrdinalIgnoreCase) ? "background:#DCFCE7;color:#15803D" : "background:#FEF3C7;color:#B45309" %>'><%# Server.HtmlEncode(Convert.ToString(Eval("Status"))) %></span>
                                </ItemTemplate></asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions"><ItemTemplate>
                                    <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="EditRow" CommandArgument='<%# Eval("ExamRoomID") %>' ToolTip="Edit"><i data-lucide="pencil" class="w-4 h-4"></i></asp:LinkButton>
                                    <asp:LinkButton runat="server" CssClass="ico-btn" CommandName="ToggleRow" CommandArgument='<%# Eval("ExamRoomID") %>' ToolTip="Activate / Deactivate"><i data-lucide="power" class="w-4 h-4"></i></asp:LinkButton>
                                </ItemTemplate></asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No rooms.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
                <div class="card overflow-hidden">
                    <div class="card-head"><h2 class="text-sm font-bold">Available Invigilators</h2></div>
                    <div class="overflow-x-auto">
                        <asp:GridView ID="gvInvig" runat="server" AutoGenerateColumns="false" GridLines="None" CssClass="tbl">
                            <Columns>
                                <asp:BoundField DataField="FullName" HeaderText="Staff" />
                                <asp:BoundField DataField="EmployeeID" HeaderText="Employee ID" />
                                <asp:BoundField DataField="Department" HeaderText="Department" />
                                <asp:BoundField DataField="Position" HeaderText="Position" />
                            </Columns>
                            <EmptyDataTemplate><div class="py-8 text-center text-sm text-gray-500">No active staff.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
