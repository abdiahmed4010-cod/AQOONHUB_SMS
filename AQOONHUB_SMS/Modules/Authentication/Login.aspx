<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs"
    Inherits="AQOONHUB_SMS.Modules.Authentication.Login" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sign In — AQOONHUB SMS</title>
    <link rel="icon" type="image/png" href="<%= ResolveUrl("~/Assets/images/logo.png") %>" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="<%= ResolveUrl("~/Assets/css/login-page.css") %>" />
</head>
<body>
    <form id="form1" runat="server" class="login-shell">

        <%-- ============================================================
             LEFT — BRANDING PANEL
             ============================================================ --%>
        <aside class="brand-panel" aria-hidden="true">
            <div class="brand-top">
                <img class="brand-logo" src="<%= ResolveUrl("~/Assets/images/logo.png") %>" width="52" height="52" alt="AQOONHUB School Management System logo" />
                <div class="brand-word">
                    <div class="name">AQOON<em>HUB</em></div>
                    <span class="sub">SCHOOL MANAGEMENT SYSTEM</span>
                </div>
            </div>

            <div class="brand-mid">
                <h2 class="brand-heading">Smart School.<br /><span class="grad">Stronger Future.</span></h2>
                <p class="brand-lead">
                    AQOONHUB SMS empowers schools to manage students, staff, academics,
                    communication and more in one secure platform.
                </p>

                <div class="feature-grid">
                    <div class="feature">
                        <span class="ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg></span>
                        <span class="lbl">Student<br />Management</span>
                    </div>
                    <div class="feature">
                        <span class="ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c3 3 9 3 12 0v-5"/></svg></span>
                        <span class="lbl">Academic<br />Excellence</span>
                    </div>
                    <div class="feature">
                        <span class="ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 3v18h18"/><path d="M18 17V9"/><path d="M13 17V5"/><path d="M8 17v-3"/></svg></span>
                        <span class="lbl">Reports &amp;<br />Analytics</span>
                    </div>
                    <div class="feature">
                        <span class="ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg></span>
                        <span class="lbl">Communication<br />Hub</span>
                    </div>
                    <div class="feature">
                        <span class="ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/><path d="m9 12 2 2 4-4"/></svg></span>
                        <span class="lbl">Secure &amp;<br />Reliable</span>
                    </div>
                    <div class="feature">
                        <span class="ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="7" y="2" width="10" height="20" rx="2"/><path d="M11 18h2"/></svg></span>
                        <span class="lbl">Anywhere<br />Access</span>
                    </div>
                </div>
            </div>

            <p class="brand-foot">&copy; <span class="foot-year">2026</span> AQOONHUB SMS. All rights reserved.</p>
        </aside>

        <%-- ============================================================
             RIGHT — LOGIN AREA
             ============================================================ --%>
        <main class="auth-panel">
            <div class="auth-inner">

                <div class="mobile-brand">
                    <img src="<%= ResolveUrl("~/Assets/images/logo.png") %>" width="60" height="60" alt="AQOONHUB School Management System logo" />
                    <div class="name">AQOON<em>HUB</em></div>
                    <div class="sub">SCHOOL MANAGEMENT SYSTEM</div>
                </div>

                <section class="auth-card" role="region" aria-labelledby="loginHeading">
                    <div class="card-head">
                        <div class="head-badge" aria-hidden="true">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                        </div>
                        <h1 id="loginHeading">Welcome Back!</h1>
                        <p>Sign in to continue to your account</p>
                    </div>

                    <div class="divider">Sign in with your credentials</div>

                    <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert" role="alert" aria-live="assertive">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                        <asp:Label ID="lblErrorMessage" runat="server" CssClass="msg"></asp:Label>
                    </asp:Panel>

                    <div class="field">
                        <label for="<%= txtEmail.ClientID %>">Email address <span class="req">*</span></label>
                        <div class="control">
                            <svg class="lead-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                            <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="Enter your email" autocomplete="username email" />
                        </div>
                    </div>

                    <div class="field">
                        <label for="<%= txtPassword.ClientID %>">Password <span class="req">*</span></label>
                        <div class="control has-toggle">
                            <svg class="lead-ico" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Enter your password" autocomplete="current-password" />
                            <button type="button" id="btnTogglePassword" class="toggle-pw" aria-label="Show password" aria-pressed="false">
                                <svg class="eye-on" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
                                <svg class="eye-off" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c6.5 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.53 13.53 0 0 0 2 12s3.5 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><path d="M14.12 14.12A3 3 0 1 1 9.88 9.88"/><line x1="2" y1="2" x2="22" y2="22"/></svg>
                            </button>
                        </div>
                    </div>

                    <div class="options">
                        <label class="remember" for="<%= chkRememberMe.ClientID %>">
                            <asp:CheckBox ID="chkRememberMe" runat="server" />
                            <span>Remember me</span>
                        </label>
                    </div>

                    <div class="signin-wrap" id="signinWrap">
                        <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn-signin"
                            data-label="Sign In" OnClick="btnLogin_Click" OnClientClick="return aqoonLoginSubmit();" />
                        <span class="spinner" aria-hidden="true"></span>
                    </div>

                    <p class="help-text">Need help? Contact your system administrator.</p>
                </section>

                <div class="security-row">
                    <div class="security-item"><span class="s-ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/><path d="m9 12 2 2 4-4"/></svg></span><span>Secure<br />Connection</span></div>
                    <div class="security-item"><span class="s-ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 11l-3 3-1.5-1.5"/></svg></span><span>Role Based<br />Access</span></div>
                    <div class="security-item"><span class="s-ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg></span><span>Data<br />Protection</span></div>
                    <div class="security-item"><span class="s-ico"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/><path d="M9 15l2 2 4-4"/></svg></span><span>Audit<br />Trail</span></div>
                </div>

                <p class="auth-footer">&copy; <span class="foot-year">2026</span> AQOONHUB SMS. All rights reserved.</p>

            </div>
        </main>
    </form>

    <script>
        window.AqoonLogin = {
            emailId: '<%= txtEmail.ClientID %>',
            passwordId: '<%= txtPassword.ClientID %>',
            loginId: '<%= btnLogin.ClientID %>'
        };
    </script>
    <script src="<%= ResolveUrl("~/Assets/js/modules/authentication/login-page.js") %>"></script>
</body>
</html>
