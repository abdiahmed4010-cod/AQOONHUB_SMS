<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AQOONHUB_SMS._Default" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>AQOONHUB School Management System — Smart School. Stronger Future.</title>
    <meta name="description" content="AQOONHUB School Management System empowers schools to manage students, staff, academics, communication, finance and more in one secure platform." />
    <link rel="icon" type="image/png" href="<%= ResolveUrl("~/Assets/images/logo.png") %>" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
    <script src="https://cdn.tailwindcss.com?plugins=forms"></script>
    <script>
        tailwind.config = { theme: { extend: { colors: { royal: '#075BF5', navy: '#061B47', purple: '#7C2AE8' }, fontFamily: { sans: ['Inter', 'ui-sans-serif', 'system-ui'] } } } };
    </script>
    <link rel="stylesheet" href="<%= ResolveUrl("~/Assets/css/landing.css") %>" />
</head>
<body>
    <% string loginUrl = ResolveUrl("~/Modules/Authentication/Login.aspx");
       string logo = ResolveUrl("~/Assets/images/logo.png");
       string heroImg = ResolveUrl("~/Assets/images/campus.png");
       string aboutImg = ResolveUrl("~/Assets/images/about-dashboard.png"); %>

    <a href="#hero" class="sr-only focus:not-sr-only focus:absolute focus:z-[70] focus:top-2 focus:left-2 focus:bg-royal focus:text-white focus:px-4 focus:py-2 focus:rounded-lg">Skip to content</a>

    <!-- ============================ NAV ============================ -->
    <header id="siteNav" class="nav">
        <div class="container-x nav-inner">
            <a href="#hero" class="brand" aria-label="AQOONHUB home">
                <img src="<%= logo %>" width="40" height="40" alt="AQOONHUB logo" />
                <span class="name">AQOON<em>HUB</em><span class="sub">SCHOOL MANAGEMENT SYSTEM</span></span>
            </a>
            <nav class="nav-links" aria-label="Primary">
                <a class="nav-link active" data-section="hero" href="#hero">Home</a>
                <a class="nav-link" data-section="features" href="#features">Features</a>
                <a class="nav-link" data-section="modules" href="#modules">Modules</a>
                <a class="nav-link" data-section="security" href="#security">Security</a>
                <a class="nav-link" data-section="about" href="#about">About Us</a>
                <a class="nav-link" data-section="contact" href="#contact">Contact</a>
            </nav>
            <div class="flex items-center gap-2">
                <a href="<%= loginUrl %>" class="btn btn-grad hidden sm:inline-flex"><i data-lucide="lock" aria-hidden="true"></i> Sign In to Your Account</a>
                <button id="menuOpen" class="hamburger" aria-label="Open menu" aria-controls="mobileMenu" aria-expanded="false"><i data-lucide="menu" aria-hidden="true"></i></button>
            </div>
        </div>
    </header>

    <!-- Mobile menu -->
    <div id="mobileMenu" class="mobile-menu" role="dialog" aria-modal="true" aria-label="Menu">
        <div class="mobile-panel">
            <div class="flex items-center justify-between mb-3">
                <span class="brand"><img src="<%= logo %>" width="34" height="34" alt="" /><span class="name">AQOON<em>HUB</em></span></span>
                <button data-close class="hamburger" aria-label="Close menu"><i data-lucide="x" aria-hidden="true"></i></button>
            </div>
            <a class="m-link" href="#hero">Home</a>
            <a class="m-link" href="#features">Features</a>
            <a class="m-link" href="#modules">Modules</a>
            <a class="m-link" href="#security">Security</a>
            <a class="m-link" href="#about">About Us</a>
            <a class="m-link" href="#contact">Contact</a>
            <a href="<%= loginUrl %>" class="btn btn-grad mt-3"><i data-lucide="lock" aria-hidden="true"></i> Sign In to Your Account</a>
        </div>
    </div>

    <!-- ============================ HERO ============================ -->
    <section id="hero" class="hero">
        <span class="blob" style="width:340px;height:340px;background:#075BF5;top:-120px;right:-80px;"></span>
        <span class="blob" style="width:300px;height:300px;background:#7C2AE8;bottom:-120px;left:-90px;"></span>
        <div class="container-x py-14 md:py-20 grid lg:grid-cols-2 gap-10 items-center relative">
            <div class="reveal">
                <span class="badge-grad"><i data-lucide="sparkles" style="width:14px;height:14px" aria-hidden="true"></i> Welcome to AQOONHUB SMS</span>
                <h1 class="hero-title">Smart School.<br /><span class="text-royal">Stronger</span> <span class="text-purple">Future.</span></h1>
                <p class="text-muted mt-5 text-base leading-relaxed max-w-xl">AQOONHUB School Management System empowers schools to manage students, staff, academics, communication, finance and more in one secure platform.</p>
                <div class="grid grid-cols-2 gap-3 mt-6 max-w-md">
                    <span class="check-row"><i data-lucide="check-circle-2" aria-hidden="true"></i> Role-Based Access</span>
                    <span class="check-row"><i data-lucide="check-circle-2" aria-hidden="true"></i> Real-Time Insights</span>
                    <span class="check-row"><i data-lucide="check-circle-2" aria-hidden="true"></i> Secure &amp; Reliable</span>
                    <span class="check-row"><i data-lucide="check-circle-2" aria-hidden="true"></i> Cloud Ready</span>
                </div>
                <div class="flex flex-wrap gap-3 mt-8">
                    <a href="<%= loginUrl %>" class="btn btn-grad"><i data-lucide="log-in" aria-hidden="true"></i> Sign In to Your Account</a>
                    <a href="#features" class="btn btn-white"><i data-lucide="play-circle" aria-hidden="true"></i> See How It Works</a>
                </div>
            </div>
            <div class="reveal hero-img-wrap">
                <img src="<%= heroImg %>" alt="Modern AQOONHUB school campus building" width="720" height="495" />
            </div>
        </div>
    </section>

    <!-- ==================== QUICK MODULE CARDS ==================== -->
    <section class="container-x -mt-8 md:-mt-12 relative z-10 pb-4">
        <div class="grid sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
            <% string[,] quick = {
                {"users-round","ic-blue","Student Management","Manage student records, admissions, attendance, and academic history."},
                {"graduation-cap","ic-purple","Academics","Classes, subjects, exams, timetables, and results management."},
                {"calendar-check","ic-green","Attendance","Track attendance in real-time and generate detailed attendance reports."},
                {"wallet","ic-orange","Finance &amp; Fees","Fee structures, payments, invoices, receipts and financial reports."},
                {"message-square","ic-blue","Communication","Announcements, messages, SMS &amp; email campaigns and delivery tracking."},
                {"bar-chart-3","ic-purple","Reports &amp; Analytics","Powerful reports and analytics to make better data-driven decisions."} };
               for (int i = 0; i < quick.GetLength(0); i++) { %>
            <div class="card card-hover p-5 reveal flex flex-col" tabindex="0">
                <span class="icon-circle <%= quick[i,1] %>"><i data-lucide="<%= quick[i,0] %>" aria-hidden="true"></i></span>
                <h3 class="font-bold text-navy mt-3 text-[.98rem]"><%= quick[i,2] %></h3>
                <p class="text-muted text-[.82rem] mt-1 leading-relaxed flex-1"><%= quick[i,3] %></p>
                <a href="#modules" class="learn">Learn more <i data-lucide="arrow-right" aria-hidden="true"></i></a>
            </div>
            <% } %>
        </div>
    </section>

    <!-- ======================= STATS STRIP ======================= -->
    <section class="container-x py-8">
        <div class="card p-6 md:p-7">
            <div class="grid grid-cols-2 md:grid-cols-4 gap-6 items-center">
                <% string[,] stats = { {"users","10K+","Students Managed"},{"building-2","250+","Schools Trust Us"},{"user-cog","2K+","Teachers &amp; Staff"},{"shield-check","99.9%","System Uptime"} };
                   for (int i = 0; i < stats.GetLength(0); i++) { %>
                <div class="flex items-center gap-3 <%= i>0 ? "md:pl-6 md:border-l md:border-[color:var(--border)]" : "" %>">
                    <span class="stat-ico"><i data-lucide="<%= stats[i,0] %>" aria-hidden="true"></i></span>
                    <div><div class="stat-value"><%= stats[i,1] %></div><div class="stat-label"><%= stats[i,2] %></div></div>
                </div>
                <% } %>
            </div>
        </div>
    </section>

    <!-- ====================== CORE FEATURES ====================== -->
    <section id="features" class="py-16 md:py-20">
        <div class="container-x text-center">
            <span class="section-label">Core Features</span>
            <h2 class="section-title">Built for Modern School <span class="grad-text">Operations</span></h2>
            <div class="title-underline"></div>
            <p class="section-lead">AQOONHUB SMS brings all essential school processes together in one intelligent platform to simplify management, save time, and improve productivity.</p>
        </div>
        <div class="container-x mt-10 grid md:grid-cols-2 lg:grid-cols-3 gap-5">
            <% string[,] feats = {
                {"folder-open","ic-blue","Student Records","Store and manage student profiles, academic history, documents, and health records securely."},
                {"calendar-check","ic-green","Attendance Tracking","Track daily attendance in real time, generate reports, and reduce absenteeism effectively."},
                {"file-badge","ic-purple","Exam Management","Create exams, assign schedules, evaluate results, and publish grades with ease."},
                {"wallet","ic-orange","Fee Management","Manage fee structures, collections, invoices, and payment history transparently."},
                {"message-square","ic-blue","Parent Communication","Send announcements, notifications, and updates to parents via SMS, email, or in-app messages."},
                {"bar-chart-3","ic-purple","Reports &amp; Analytics","Generate insightful reports and analytics to make data-driven decisions for your school."} };
               for (int i = 0; i < feats.GetLength(0); i++) { %>
            <div class="card card-hover p-6 reveal flex flex-col" tabindex="0">
                <span class="icon-circle ic-lg <%= feats[i,1] %>"><i data-lucide="<%= feats[i,0] %>" aria-hidden="true"></i></span>
                <h3 class="font-bold text-navy mt-4 text-lg"><%= feats[i,2] %></h3>
                <p class="text-muted text-[.88rem] mt-2 leading-relaxed flex-1"><%= feats[i,3] %></p>
                <a href="#modules" class="learn">Learn more <i data-lucide="arrow-right" aria-hidden="true"></i></a>
            </div>
            <% } %>
        </div>
    </section>

    <!-- ========================= MODULES ========================= -->
    <section id="modules" class="py-16 md:py-20 bg-lb">
        <div class="container-x text-center">
            <span class="section-label">Powerful Modules</span>
            <h2 class="section-title">All Modules in One <span class="grad-text">Unified Platform</span></h2>
            <div class="title-underline"></div>
            <p class="section-lead">AQOONHUB brings every aspect of school management together in a unified platform. Each module works seamlessly with others to simplify operations and drive excellence.</p>
        </div>
        <div class="container-x mt-10 grid sm:grid-cols-2 lg:grid-cols-4 gap-5">
            <% string[,] mods = {
                {"users-round","ic-blue","Student Management","Manage student profiles, admissions, records, attendance, behavior, and academic history."},
                {"graduation-cap","ic-purple","Academics","Manage classes, subjects, timetables, curriculum, assignments, and resources."},
                {"calendar-check","ic-green","Attendance","Track attendance in real-time and generate detailed attendance reports."},
                {"clipboard-list","ic-orange","Examinations","Create exams, manage schedules, evaluate results, and publish grades with accuracy."},
                {"wallet","ic-orange","Finance &amp; Payroll","Manage fees, invoices, payments, expenses, payroll, and financial reports with transparency."},
                {"message-square","ic-blue","Communication","Send announcements, SMS, emails, and in-app messages to students, parents, and staff."},
                {"bar-chart-3","ic-purple","Reports &amp; Analytics","Generate insightful reports and analytics to make data-driven decisions for your school."},
                {"settings","ic-blue","Users &amp; Settings","Manage users, roles, permissions, and system settings to keep your platform secure and organized."} };
               for (int i = 0; i < mods.GetLength(0); i++) { %>
            <div class="card card-hover p-6 reveal flex flex-col" tabindex="0">
                <span class="icon-circle ic-lg <%= mods[i,1] %>"><i data-lucide="<%= mods[i,0] %>" aria-hidden="true"></i></span>
                <h3 class="font-bold text-navy mt-4"><%= mods[i,2] %></h3>
                <p class="text-muted text-[.84rem] mt-2 leading-relaxed flex-1"><%= mods[i,3] %></p>
                <a href="<%= loginUrl %>" class="learn">Learn more <i data-lucide="arrow-right" aria-hidden="true"></i></a>
            </div>
            <% } %>
        </div>
    </section>

    <!-- ========================== ABOUT ========================== -->
    <section id="about" class="py-16 md:py-20">
        <div class="container-x">
            <div class="grid lg:grid-cols-2 gap-10 items-center">
                <div class="reveal">
                    <img src="<%= aboutImg %>" alt="AQOONHUB dashboard on a modern office desk" width="640" height="460" class="rounded-2xl shadow-lg w-full object-cover" style="aspect-ratio:4/3;object-position:center;" />
                </div>
                <div class="reveal">
                    <span class="section-label">About Us</span>
                    <h2 class="section-title text-left">Built to Empower<br />Modern <span class="grad-text">Education</span></h2>
                    <p class="text-muted mt-4 leading-relaxed">AQOONHUB School Management System is a comprehensive platform designed to simplify school operations and drive better outcomes. From student management and academics to communication, finance, and reporting, we help schools work smarter and focus on what truly matters: education.</p>
                    <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6">
                        <% string[,] astats = { {"10K+","Students Managed"},{"250+","Schools"},{"2K+","Teachers &amp; Staff"},{"99.9%","Uptime"} };
                           for (int i=0;i<astats.GetLength(0);i++){ %>
                        <div class="text-center sm:text-left"><div class="stat-value"><%= astats[i,0] %></div><div class="stat-label"><%= astats[i,1] %></div></div>
                        <% } %>
                    </div>
                </div>
            </div>
            <div class="grid md:grid-cols-3 gap-5 mt-10">
                <% string[,] vals = {
                    {"target","ic-blue","Our Mission","Empower schools with smart technology to simplify management and enhance student success."},
                    {"eye","ic-purple","Our Vision","To be the most trusted and innovative school management platform for schools worldwide."},
                    {"shield-check","ic-green","Our Reliability","We are committed to security, performance, and continuous improvement you can rely on."} };
                   for (int i=0;i<vals.GetLength(0);i++){ %>
                <div class="value-card reveal">
                    <span class="icon-circle <%= vals[i,1] %>"><i data-lucide="<%= vals[i,0] %>" aria-hidden="true"></i></span>
                    <h3 class="font-bold text-navy mt-3"><%= vals[i,2] %></h3>
                    <p class="text-muted text-[.86rem] mt-1.5 leading-relaxed"><%= vals[i,3] %></p>
                </div>
                <% } %>
            </div>
        </div>
    </section>

    <!-- ========================= SECURITY ========================= -->
    <section id="security" class="py-16 md:py-20 bg-lb">
        <div class="container-x text-center">
            <span class="section-label">Security</span>
            <h2 class="section-title">Enterprise-Grade Protection for <span class="grad-text">Every School</span></h2>
            <div class="title-underline"></div>
            <p class="section-lead">AQOONHUB SMS is built with security at its core. From secure access controls and role-based permissions to audit logging, the platform is designed to keep your data protected.</p>
        </div>
        <div class="container-x mt-10 grid md:grid-cols-2 lg:grid-cols-3 gap-5">
            <% string[,] sec = {
                {"users-round","ic-blue","Role-Based Access","Granular role permissions ensure each user only accesses the data and actions their role allows."},
                {"shield","ic-green","Data Protection","Sensitive data is safeguarded with the platform's access controls and protection measures."},
                {"file-clock","ic-purple","Audit Trails","Comprehensive audit logs track key actions, supporting transparency and compliance."},
                {"lock","ic-orange","Secure Authentication","Password hashing and forced password-change support help keep unauthorized access out."},
                {"cloud","ic-blue","Backup &amp; Reliability","The platform is designed for dependable operation and safe, recoverable data practices."},
                {"activity","ic-purple","Activity Monitoring","Login activity monitoring helps administrators review access and spot suspicious activity."} };
               for (int i=0;i<sec.GetLength(0);i++){ %>
            <div class="card card-hover p-6 reveal flex flex-col" tabindex="0">
                <span class="icon-circle ic-lg <%= sec[i,1] %>"><i data-lucide="<%= sec[i,0] %>" aria-hidden="true"></i></span>
                <h3 class="font-bold text-navy mt-4"><%= sec[i,2] %></h3>
                <p class="text-muted text-[.86rem] mt-2 leading-relaxed flex-1"><%= sec[i,3] %></p>
            </div>
            <% } %>
        </div>
        <div class="container-x mt-8">
            <div class="sec-strip p-6 grid sm:grid-cols-3 gap-6">
                <% string[,] strip = {
                    {"lock","Encrypted Transport","Sign-in and data exchange are designed to run over encrypted HTTPS connections."},
                    {"user-check","Protected Access","Secure authentication and role-based controls protect your environment."},
                    {"server","Reliable Infrastructure","Designed for deployment on secure, high-availability infrastructure."} };
                   for (int i=0;i<strip.GetLength(0);i++){ %>
                <div class="flex items-start gap-3">
                    <span class="icon-circle ic-blue"><i data-lucide="<%= strip[i,0] %>" aria-hidden="true"></i></span>
                    <div><h4 class="font-bold text-navy text-[.95rem]"><%= strip[i,1] %></h4><p class="text-muted text-[.82rem] mt-1 leading-relaxed"><%= strip[i,2] %></p></div>
                </div>
                <% } %>
            </div>
            <p class="text-center text-[.75rem] text-muted mt-3">Security features reflect the platform&rsquo;s design goals; specific standards and certifications are provided per deployment.</p>
        </div>
    </section>

    <!-- ========================= CONTACT ========================= -->
    <section id="contact" class="py-16 md:py-20">
        <div class="container-x text-center">
            <span class="section-label">Contact</span>
            <h2 class="section-title">Let&rsquo;s Talk About <span class="grad-text">Your School</span></h2>
            <div class="title-underline"></div>
            <p class="section-lead">Have questions or are ready to get started? Our team is here to help you transform your school management experience.</p>
        </div>
        <div class="container-x mt-10 grid lg:grid-cols-2 gap-6 items-start">
            <!-- Form -->
            <div class="card p-6 md:p-7 reveal">
                <h3 class="font-bold text-navy text-lg flex items-center gap-2"><i data-lucide="mail" class="text-royal" style="width:20px;height:20px" aria-hidden="true"></i> Send Us a Message</h3>
                <p class="text-muted text-[.85rem] mt-1">Fill out the form below and we&rsquo;ll get back to you shortly.</p>
                <form id="contactForm" class="mt-5 grid sm:grid-cols-2 gap-4" novalidate>
                    <div><label class="field-label" for="fullName">Full Name</label><input class="field-input" id="fullName" name="fullName" type="text" placeholder="Enter your full name" autocomplete="name" /><span class="field-err" id="fullNameErr" aria-live="polite"></span></div>
                    <div><label class="field-label" for="emailAddr">Email Address</label><input class="field-input" id="emailAddr" name="emailAddr" type="email" placeholder="Enter your email address" autocomplete="email" /><span class="field-err" id="emailAddrErr" aria-live="polite"></span></div>
                    <div><label class="field-label" for="schoolName">School Name</label><input class="field-input" id="schoolName" name="schoolName" type="text" placeholder="Enter your school name" autocomplete="organization" /></div>
                    <div><label class="field-label" for="phone">Phone Number</label><input class="field-input" id="phone" name="phone" type="tel" placeholder="Enter your phone number" autocomplete="tel" /></div>
                    <div class="sm:col-span-2"><label class="field-label" for="subject">Subject</label>
                        <select class="field-input" id="subject" name="subject">
                            <option value="">Select a subject</option>
                            <option>General Inquiry</option><option>Product Demo</option><option>Technical Support</option>
                            <option>Pricing Information</option><option>Partnership</option><option>Other</option>
                        </select><span class="field-err" id="subjectErr" aria-live="polite"></span></div>
                    <div class="sm:col-span-2"><label class="field-label" for="message">Message</label><textarea class="field-input" id="message" name="message" placeholder="Tell us how we can help your school..."></textarea><span class="field-err" id="messageErr" aria-live="polite"></span></div>
                    <div class="sm:col-span-2"><button type="submit" class="btn btn-grad w-full"><i data-lucide="send" aria-hidden="true"></i> Send Message</button></div>
                </form>
                <div id="formAlert" class="hidden"></div>
            </div>
            <!-- Info -->
            <div class="reveal">
                <div class="card p-6 md:p-7" style="background:linear-gradient(150deg,#075BF5,#7C2AE8);color:#fff;border:none;">
                    <h3 class="font-bold text-lg">Get in Touch</h3>
                    <p class="text-white/80 text-[.85rem] mt-1">We&rsquo;d love to hear from you.</p>
                    <div class="mt-5 space-y-4">
                        <div class="flex items-start gap-3"><span class="contact-ico"><i data-lucide="map-pin" aria-hidden="true"></i></span><div><div class="font-semibold text-[.9rem]">Address</div><div class="text-white/80 text-[.85rem]">Mogadishu, Somalia &middot; Karaan District, Hodan Area</div></div></div>
                        <div class="flex items-start gap-3"><span class="contact-ico"><i data-lucide="mail" aria-hidden="true"></i></span><div><div class="font-semibold text-[.9rem]">Email</div><a href="mailto:info@aqoonhub.com" class="text-white/80 text-[.85rem] hover:text-white">info@aqoonhub.com</a></div></div>
                        <div class="flex items-start gap-3"><span class="contact-ico"><i data-lucide="phone" aria-hidden="true"></i></span><div><div class="font-semibold text-[.9rem]">Phone</div><a href="tel:+252612345678" class="text-white/80 text-[.85rem] hover:text-white">+252 61 2345678</a></div></div>
                        <div class="flex items-start gap-3"><span class="contact-ico"><i data-lucide="clock" aria-hidden="true"></i></span><div><div class="font-semibold text-[.9rem]">Support Hours</div><div class="text-white/80 text-[.85rem]">Monday&ndash;Friday, 8:00 AM&ndash;6:00 PM (EAT)</div></div></div>
                    </div>
                    <div class="mt-6"><div class="font-semibold text-[.9rem] mb-2">Follow Us</div>
                        <div class="flex gap-2">
                            <a href="#" class="social-btn" aria-label="Facebook"><i data-lucide="facebook" aria-hidden="true"></i></a>
                            <a href="#" class="social-btn" aria-label="X (Twitter)"><i data-lucide="twitter" aria-hidden="true"></i></a>
                            <a href="#" class="social-btn" aria-label="LinkedIn"><i data-lucide="linkedin" aria-hidden="true"></i></a>
                            <a href="#" class="social-btn" aria-label="YouTube"><i data-lucide="youtube" aria-hidden="true"></i></a>
                        </div>
                    </div>
                </div>
                <div class="map-area mt-4" role="img" aria-label="Map showing Mogadishu, Somalia">
                    <div class="map-grid"></div>
                    <div class="map-pin"><i data-lucide="map-pin" aria-hidden="true"></i></div>
                </div>
            </div>
        </div>
    </section>

    <!-- ========================== FOOTER ========================== -->
    <footer class="footer pt-14 pb-6">
        <div class="container-x grid sm:grid-cols-2 lg:grid-cols-5 gap-8">
            <div class="lg:col-span-1">
                <span class="brand"><img src="<%= logo %>" width="40" height="40" alt="AQOONHUB logo" style="background:#fff;padding:2px;" /><span class="name" style="color:#fff">AQOON<em style="color:#8FB4FF">HUB</em><span class="sub" style="color:#93A4C7">SCHOOL MANAGEMENT SYSTEM</span></span></span>
                <p class="text-[.85rem] mt-4 leading-relaxed" style="color:#A9B7D4">Empowering schools with smart technology for a better tomorrow.</p>
                <div class="flex gap-2 mt-4">
                    <a href="#" class="footer-social" aria-label="Facebook"><i data-lucide="facebook" aria-hidden="true"></i></a>
                    <a href="#" class="footer-social" aria-label="X (Twitter)"><i data-lucide="twitter" aria-hidden="true"></i></a>
                    <a href="#" class="footer-social" aria-label="LinkedIn"><i data-lucide="linkedin" aria-hidden="true"></i></a>
                    <a href="#" class="footer-social" aria-label="YouTube"><i data-lucide="youtube" aria-hidden="true"></i></a>
                </div>
            </div>
            <div><div class="footer-title">Quick Links</div><ul class="space-y-2 text-[.87rem]"><li><a href="#hero">Home</a></li><li><a href="#features">Features</a></li><li><a href="#modules">Modules</a></li><li><a href="#security">Security</a></li><li><a href="#about">About Us</a></li><li><a href="#contact">Contact</a></li></ul></div>
            <div><div class="footer-title">Support</div><ul class="space-y-2 text-[.87rem]"><li><a href="#contact">Help Center</a></li><li><a href="#contact">Documentation</a></li><li><a href="#contact">Privacy Policy</a></li><li><a href="#contact">Terms of Service</a></li></ul></div>
            <div><div class="footer-title">Contact Us</div><ul class="space-y-2 text-[.87rem]">
                <li class="flex items-center gap-2"><i data-lucide="phone" style="width:15px;height:15px" aria-hidden="true"></i> +252 61 2345678</li>
                <li class="flex items-center gap-2"><i data-lucide="mail" style="width:15px;height:15px" aria-hidden="true"></i> info@aqoonhub.com</li>
                <li class="flex items-center gap-2"><i data-lucide="map-pin" style="width:15px;height:15px" aria-hidden="true"></i> Mogadishu, Somalia</li>
            </ul></div>
            <div><div class="footer-title">Follow Us</div><ul class="space-y-2 text-[.87rem]"><li><a href="#">Facebook</a></li><li><a href="#">X (Twitter)</a></li><li><a href="#">LinkedIn</a></li><li><a href="#">YouTube</a></li></ul></div>
        </div>
        <div class="container-x mt-10 pt-6 border-t border-white/10 text-center text-[.82rem]" style="color:#93A4C7">&copy; <span id="copyYear">2026</span> AQOONHUB SMS. All rights reserved.</div>
    </footer>

    <button id="toTop" class="to-top" aria-label="Scroll to top"><i data-lucide="arrow-up" aria-hidden="true"></i></button>

    <script src="https://unpkg.com/lucide@0.400.0/dist/umd/lucide.min.js"></script>
    <script src="<%= ResolveUrl("~/Assets/js/landing.js") %>"></script>
</body>
</html>
