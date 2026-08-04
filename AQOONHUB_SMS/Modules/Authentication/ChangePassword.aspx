<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs"
    Inherits="AQOONHUB_SMS.Modules.Authentication.ChangePassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Change Password — AQOONHUB SMS</title>
    <link rel="icon" type="image/png" href="<%= ResolveUrl("~/Assets/images/logo.png") %>" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="<%= ResolveUrl("~/Assets/css/login-page.css") %>" />
</head>
<body>
    <form id="form1" runat="server" class="login-shell">
        <main class="auth-panel">
            <div class="auth-inner">

                <div class="mobile-brand" style="display:block">
                    <img src="<%= ResolveUrl("~/Assets/images/logo.png") %>" width="56" height="56" alt="AQOONHUB School Management System logo" />
                    <div class="name">AQOON<em>HUB</em></div>
                    <div class="sub">SCHOOL MANAGEMENT SYSTEM</div>
                </div>

                <section class="auth-card" role="region" aria-labelledby="cpHeading">
                    <div class="card-head">
                        <div class="head-badge" aria-hidden="true">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                        </div>
                        <h1 id="cpHeading">Change Your Password</h1>
                        <p>A password update is required before you continue.</p>
                    </div>

                    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert" role="alert" aria-live="assertive">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                        <asp:Label ID="lblError" runat="server" CssClass="msg" />
                    </asp:Panel>

                    <div class="field">
                        <label for="<%= txtCurrent.ClientID %>">Current password <span class="req">*</span></label>
                        <div class="control has-toggle">
                            <svg class="lead-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                            <asp:TextBox ID="txtCurrent" runat="server" TextMode="Password" placeholder="Enter your current password" autocomplete="current-password" />
                            <button type="button" class="toggle-pw" data-target="current" aria-label="Show current password" aria-pressed="false">
                                <svg class="eye-on" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
                                <svg class="eye-off" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c6.5 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.53 13.53 0 0 0 2 12s3.5 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><path d="M14.12 14.12A3 3 0 1 1 9.88 9.88"/><line x1="2" y1="2" x2="22" y2="22"/></svg>
                            </button>
                        </div>
                    </div>

                    <div class="field">
                        <label for="<%= txtNew.ClientID %>">New password <span class="req">*</span></label>
                        <div class="control has-toggle">
                            <svg class="lead-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                            <asp:TextBox ID="txtNew" runat="server" TextMode="Password" placeholder="Create a new password" autocomplete="new-password" />
                            <button type="button" class="toggle-pw" data-target="new" aria-label="Show new password" aria-pressed="false">
                                <svg class="eye-on" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
                                <svg class="eye-off" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c6.5 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.53 13.53 0 0 0 2 12s3.5 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><path d="M14.12 14.12A3 3 0 1 1 9.88 9.88"/><line x1="2" y1="2" x2="22" y2="22"/></svg>
                            </button>
                        </div>
                    </div>

                    <div class="field">
                        <label for="<%= txtConfirm.ClientID %>">Confirm new password <span class="req">*</span></label>
                        <div class="control has-toggle">
                            <svg class="lead-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                            <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" placeholder="Re-enter the new password" autocomplete="new-password" />
                            <button type="button" class="toggle-pw" data-target="confirm" aria-label="Show confirmation password" aria-pressed="false">
                                <svg class="eye-on" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
                                <svg class="eye-off" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c6.5 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.53 13.53 0 0 0 2 12s3.5 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><path d="M14.12 14.12A3 3 0 1 1 9.88 9.88"/><line x1="2" y1="2" x2="22" y2="22"/></svg>
                            </button>
                        </div>
                    </div>

                    <ul class="help-text" style="text-align:left;margin:0 0 16px;padding-left:1.1rem;list-style:disc;">
                        <li>At least 8 characters</li>
                        <li>Contains letters and numbers</li>
                        <li>Different from your current password</li>
                    </ul>

                    <div class="signin-wrap" id="signinWrap">
                        <asp:Button ID="btnSubmit" runat="server" Text="Update Password" CssClass="btn-signin"
                            data-label="Update Password" OnClick="btnSubmit_Click" OnClientClick="return aqoonCpSubmit();" />
                        <span class="spinner" aria-hidden="true"></span>
                    </div>

                    <p class="help-text">
                        <asp:LinkButton ID="lnkLogout" runat="server" CssClass="forgot-link" CausesValidation="false" OnClick="lnkLogout_Click">Sign out instead</asp:LinkButton>
                    </p>
                </section>

                <p class="auth-footer" style="display:block">&copy; <span class="foot-year">2026</span> AQOONHUB SMS. All rights reserved.</p>
            </div>
        </main>
    </form>

    <script>
        window.AqoonCp = { currentId: '<%= txtCurrent.ClientID %>', newId: '<%= txtNew.ClientID %>', confirmId: '<%= txtConfirm.ClientID %>', submitId: '<%= btnSubmit.ClientID %>' };
    </script>
    <script src="<%= ResolveUrl("~/Assets/js/modules/authentication/change-password.js") %>"></script>
</body>
</html>
