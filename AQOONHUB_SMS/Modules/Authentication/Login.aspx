<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" 
    Inherits="AQOONHUB_SMS.Modules.Authentication.Login" 
    MasterPageFile="" %>

<!DOCTYPE html>
<html lang="en" class="">
<!-- rest of your Login.aspx stays exactly the same -->
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sign In — AQOONHUB SMS</title>
    <link rel="icon" type="image/png" href="../../assets/logo.png" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
    <script src="https://cdn.tailwindcss.com"></script>
    <script>
        tailwind.config = {
            darkMode: 'class',
            theme: {
                extend: {
                    colors: {
                        brand: {
                            50: '#EFF6FF', 100: '#DBEAFE', 200: '#BFDBFE', 300: '#93C5FD',
                            400: '#60A5FA', 500: '#3B82F6', 600: '#2563EB', 700: '#1D4ED8',
                            800: '#1E3A8A', 900: '#1E3A8A'
                        },
                        sidebar: '#1E3A8A',
                        surface: '#F8FAFC'
                    },
                    fontFamily: {
                        sans: ['Inter', 'ui-sans-serif', 'system-ui']
                    },
                    boxShadow: {
                        card: '0 1px 2px 0 rgb(0 0 0 / 0.04), 0 1px 3px 0 rgb(0 0 0 / 0.06)',
                        pop: '0 12px 32px -8px rgb(15 23 42 / 0.18)'
                    }
                }
            }
        }
    </script>
    <style>
        html { -webkit-font-smoothing: antialiased; }
        body { font-family: 'Inter', sans-serif; }
        .input {
            width: 100%;
            border: 1px solid #E5E7EB;
            border-radius: .6rem;
            padding: .55rem .8rem;
            font-size: .85rem;
            background: #fff;
            color: #111827;
            outline: none;
            transition: border .15s, box-shadow .15s;
        }
        .input:focus {
            border-color: #2563EB;
            box-shadow: 0 0 0 3px rgba(37,99,235,.12);
        }
        .dark .input {
            background: #0F172A;
            border-color: #334155;
            color: #E2E8F0;
        }
        .btn-primary {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: .45rem;
            font-weight: 600;
            font-size: .82rem;
            border-radius: .6rem;
            padding: .55rem 1rem;
            transition: all .15s;
            cursor: pointer;
            border: 1px solid transparent;
            background: #2563EB;
            color: #fff;
        }
        .btn-primary:hover { background: #1D4ED8; }
        .btn-primary:disabled {
            opacity: .6;
            cursor: not-allowed;
        }
        .fadein {
            animation: fadein .25s ease;
        }
        @keyframes fadein {
            from { opacity: 0; transform: translateY(4px); }
            to { opacity: 1; transform: none; }
        }
        .spinner {
            width: 16px;
            height: 16px;
            border: 2px solid rgba(255,255,255,.3);
            border-top-color: #fff;
            border-radius: 50%;
            animation: spin .6s linear infinite;
        }
        @keyframes spin {
            to { transform: rotate(360deg); }
        }
        .error-shake {
            animation: shake .4s ease;
        }
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-6px); }
            75% { transform: translateX(6px); }
        }
    </style>
</head>
<body class="bg-surface dark:bg-slate-900 text-slate-900 dark:text-slate-100">
    <form id="form1" runat="server" class="min-h-screen flex">

        <!-- Left Panel -->
        <div class="hidden lg:flex w-[46%] bg-sidebar text-white flex-col justify-between p-12 relative overflow-hidden">
            <div class="absolute -top-24 -right-24 w-96 h-96 rounded-full bg-brand-600/30 blur-3xl"></div>
            <div class="absolute bottom-0 -left-24 w-96 h-96 rounded-full bg-violet-600/30 blur-3xl"></div>

            <div class="relative flex items-center gap-3 z-10">
                <img src="../../assets/logo.png" class="w-11 h-11 rounded-xl bg-white p-1" alt="AQOONHUB Logo" />
                <p class="text-xl font-extrabold tracking-tight">AQOON<span class="text-violet-300">HUB</span></p>
            </div>

            <div class="relative z-10">
                <h1 class="text-4xl font-extrabold leading-tight tracking-tight">Run your entire school<br />from one hub.</h1>
                <p class="text-indigo-200 mt-4 max-w-md text-sm leading-relaxed">
                    Admissions, academics, attendance, exams, finance, HR and reporting — 
                    a single enterprise-grade platform for primary &amp; secondary schools.
                </p>
                <div class="flex gap-6 mt-10">
                    <div>
                        <p class="text-2xl font-extrabold">1,240+</p>
                        <p class="text-xs text-indigo-300 mt-1">Students</p>
                    </div>
                    <div>
                        <p class="text-2xl font-extrabold">86</p>
                        <p class="text-xs text-indigo-300 mt-1">Staff</p>
                    </div>
                    <div>
                        <p class="text-2xl font-extrabold">99.9%</p>
                        <p class="text-xs text-indigo-300 mt-1">Uptime</p>
                    </div>
                </div>
            </div>

            <p class="relative z-10 text-xs text-indigo-300">
                &copy; 2026 AQOONHUB International School
            </p>
        </div>

        <!-- Right Panel -->
        <div class="flex-1 flex items-center justify-center p-6 bg-surface dark:bg-slate-900">
            <div class="w-full max-w-md fadein">

                <!-- Mobile Logo -->
                <div class="lg:hidden text-center mb-6">
                    <img src="../../assets/logo.png" class="w-14 h-14 rounded-2xl mx-auto mb-3" alt="AQOONHUB Logo" />
                    <p class="text-xl font-extrabold tracking-tight text-slate-900 dark:text-white">
                        AQOON<span class="text-brand-600">HUB</span>
                    </p>
                    <p class="text-xs text-gray-500 dark:text-slate-400 mt-1">School Management System</p>
                </div>

                <h2 class="text-2xl font-extrabold tracking-tight text-slate-900 dark:text-white">Welcome back</h2>
                <p class="text-sm text-gray-500 dark:text-slate-400 mt-1 mb-7">
                    Sign in to your AQOONHUB account to continue.
                </p>

                <!-- Error Message -->
                <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="mb-4 p-3.5 rounded-xl bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/30 flex items-start gap-2.5 error-shake">
                    <svg class="w-4 h-4 text-red-500 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                    <asp:Label ID="lblErrorMessage" runat="server" CssClass="text-xs text-red-700 dark:text-red-300 font-medium"></asp:Label>
                </asp:Panel>

                <!-- Login Form -->
                <div class="space-y-4">

                    <!-- Email / Username -->
                    <div>
                        <label class="text-xs font-semibold text-gray-600 dark:text-slate-300 mb-1.5 block">
                            Email address <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtEmail" runat="server" 
                            CssClass="input" 
                            TextMode="Email"
                            placeholder="you@aqoonhub.edu"
                            autocomplete="email" />
                    </div>

                    <!-- Password -->
                    <div>
                        <label class="text-xs font-semibold text-gray-600 dark:text-slate-300 mb-1.5 block">
                            Password <span class="text-red-500">*</span>
                        </label>
                        <div class="relative">
                            <asp:TextBox ID="txtPassword" runat="server" 
                                CssClass="input !pr-10" 
                                TextMode="Password"
                                placeholder="Enter your password"
                                autocomplete="current-password" />
                            <button type="button" 
                                id="btnTogglePassword"
                                class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-slate-300 transition-colors"
                                onclick="togglePasswordVisibility()"
                                title="Show / Hide password">
                                <svg id="eyeIcon" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                                        d="M15 12a3 3 0 11-6 0 3 3 0 016 0z M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                                </svg>
                                <svg id="eyeOffIcon" class="w-4 h-4 hidden" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                                        d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.542-7a10.05 10.05 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.542 7a10.058 10.058 0 01-3.7 4.79m0 0L21 21"></path>
                                </svg>
                            </button>
                        </div>
                    </div>

                    <!-- Remember Me & Forgot Password -->
                    <div class="flex items-center justify-between text-xs">
                        <label class="flex items-center gap-2 text-gray-600 dark:text-slate-300 cursor-pointer">
                            <asp:CheckBox ID="chkRememberMe" runat="server" CssClass="accent-brand-600 w-3.5 h-3.5" />
                            <span>Remember me</span>
                        </label>
                        <a href="ForgotPassword.aspx" class="font-semibold text-brand-600 hover:underline transition-colors">
                            Forgot password?
                        </a>
                    </div>

                    <!-- Login Button -->
                    <asp:Button ID="btnLogin" runat="server" 
                        Text="Sign in" 
                        CssClass="btn-primary w-full !py-2.5"
                        OnClick="btnLogin_Click"
                        OnClientClick="return showLoading();" />

                    <!-- Loading Button (Hidden by default) -->
                    <button type="button" 
                        id="btnLoading" 
                        disabled 
                        class="btn-primary w-full !py-2.5 hidden">
                        <span class="spinner"></span>
                        <span>Signing in...</span>
                    </button>

                </div>

                <!-- Security Note -->
                <p class="text-[11px] text-gray-400 dark:text-slate-500 mt-6 text-center flex items-center justify-center gap-1.5">
                    <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                            d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path>
                    </svg>
                    Protected by password policy, session timeout &amp; audit logging
                </p>

            </div>
        </div>

    </form>

    <script>
        // Toggle password visibility
        function togglePasswordVisibility() {
            var txtPassword = document.getElementById('<%= txtPassword.ClientID %>');
            var eyeIcon = document.getElementById('eyeIcon');
            var eyeOffIcon = document.getElementById('eyeOffIcon');

            if (txtPassword.type === 'password') {
                txtPassword.type = 'text';
                eyeIcon.classList.add('hidden');
                eyeOffIcon.classList.remove('hidden');
            } else {
                txtPassword.type = 'password';
                eyeIcon.classList.remove('hidden');
                eyeOffIcon.classList.add('hidden');
            }
        }

        // Show loading state
        function showLoading() {
            var btnLogin = document.getElementById('<%= btnLogin.ClientID %>');
            var btnLoading = document.getElementById('btnLoading');
            var txtEmail = document.getElementById('<%= txtEmail.ClientID %>');
            var txtPassword = document.getElementById('<%= txtPassword.ClientID %>');

            // Basic client-side validation
            if (!txtEmail.value.trim() || !txtPassword.value.trim()) {
                return true; // Let server handle validation
            }

            btnLogin.classList.add('hidden');
            btnLoading.classList.remove('hidden');
            return true;
        }

        // Dark mode detection
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            document.documentElement.classList.add('dark');
        }
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
            if (event.matches) {
                document.documentElement.classList.add('dark');
            } else {
                document.documentElement.classList.remove('dark');
            }
        });
    </script>
</body>
</html>