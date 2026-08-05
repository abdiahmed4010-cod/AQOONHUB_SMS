/* ============================================================================
   AQOONHUB SMS — Public Landing Page interactions (Vanilla JS)
   Mobile menu, smooth scroll, sticky header, active-section highlight,
   scroll-to-top, reveal-on-scroll, contact-form validation, dynamic year.
   ============================================================================ */
(function () {
    "use strict";

    function ready(fn) {
        if (document.readyState !== "loading") { fn(); }
        else { document.addEventListener("DOMContentLoaded", fn); }
    }

    ready(function () {
        var nav = document.getElementById("siteNav");
        var menu = document.getElementById("mobileMenu");
        var openBtn = document.getElementById("menuOpen");

        /* ---- Sticky header shadow ---- */
        function onScroll() {
            if (nav) { nav.classList.toggle("scrolled", window.scrollY > 8); }
            var top = document.getElementById("toTop");
            if (top) { top.classList.toggle("show", window.scrollY > 500); }
        }
        window.addEventListener("scroll", onScroll, { passive: true });
        onScroll();

        /* ---- Mobile menu ---- */
        function setMenu(open) {
            if (!menu) { return; }
            menu.classList.toggle("open", open);
            if (openBtn) { openBtn.setAttribute("aria-expanded", open ? "true" : "false"); }
            document.documentElement.style.overflow = open ? "hidden" : "";
            if (open) {
                var first = menu.querySelector("a, button");
                if (first) { try { first.focus(); } catch (e) {} }
            }
        }
        if (openBtn) { openBtn.addEventListener("click", function () { setMenu(true); }); }
        if (menu) {
            menu.addEventListener("click", function (e) {
                // Close when clicking the backdrop, the close button, or a link.
                if (e.target === menu || e.target.closest("[data-close]") || e.target.closest("a.m-link")) {
                    setMenu(false);
                }
            });
        }
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && menu && menu.classList.contains("open")) { setMenu(false); }
        });

        /* ---- Smooth scroll for in-page anchors (respects reduced motion) ---- */
        var reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        document.querySelectorAll('a[href^="#"]').forEach(function (a) {
            a.addEventListener("click", function (e) {
                var id = a.getAttribute("href");
                if (id.length < 2) { return; }
                var target = document.querySelector(id);
                if (!target) { return; }
                e.preventDefault();
                target.scrollIntoView({ behavior: reduce ? "auto" : "smooth", block: "start" });
                history.replaceState(null, "", id);
            });
        });

        /* ---- Active section highlight ---- */
        var navLinks = Array.prototype.slice.call(document.querySelectorAll(".nav-link[data-section]"));
        var sections = navLinks
            .map(function (l) { return document.getElementById(l.getAttribute("data-section")); })
            .filter(Boolean);
        if ("IntersectionObserver" in window && sections.length) {
            var io = new IntersectionObserver(function (entries) {
                entries.forEach(function (en) {
                    if (en.isIntersecting) {
                        var id = en.target.id;
                        navLinks.forEach(function (l) { l.classList.toggle("active", l.getAttribute("data-section") === id); });
                    }
                });
            }, { rootMargin: "-45% 0px -50% 0px", threshold: 0 });
            sections.forEach(function (s) { io.observe(s); });
        }

        /* ---- Reveal on scroll ---- */
        var reveals = document.querySelectorAll(".reveal");
        if ("IntersectionObserver" in window && reveals.length && !reduce) {
            var ro = new IntersectionObserver(function (entries, obs) {
                entries.forEach(function (en) {
                    if (en.isIntersecting) { en.target.classList.add("in"); obs.unobserve(en.target); }
                });
            }, { threshold: 0.12 });
            reveals.forEach(function (r) { ro.observe(r); });
        } else {
            reveals.forEach(function (r) { r.classList.add("in"); });
        }

        /* ---- Scroll to top ---- */
        var toTop = document.getElementById("toTop");
        if (toTop) {
            toTop.addEventListener("click", function () {
                window.scrollTo({ top: 0, behavior: reduce ? "auto" : "smooth" });
            });
        }

        /* ---- Dynamic year ---- */
        var y = document.getElementById("copyYear");
        if (y) { y.textContent = String(new Date().getFullYear()); }

        /* ---- Contact form validation (frontend demo only) ---- */
        var form = document.getElementById("contactForm");
        if (form) {
            var alertBox = document.getElementById("formAlert");

            function setError(input, msg) {
                var err = document.getElementById(input.id + "Err");
                var bad = !!msg;
                input.classList.toggle("invalid", bad);
                input.setAttribute("aria-invalid", bad ? "true" : "false");
                if (err) { err.textContent = msg || ""; err.classList.toggle("show", bad); }
                return !bad;
            }

            function validEmail(v) { return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v); }

            form.addEventListener("submit", function (e) {
                e.preventDefault();
                var ok = true;
                var name = form.fullName, email = form.emailAddr, subject = form.subject, message = form.message;

                ok = setError(name, name.value.trim() ? "" : "Please enter your full name.") && ok;
                ok = setError(email, !email.value.trim() ? "Please enter your email address." : (validEmail(email.value.trim()) ? "" : "Please enter a valid email address.")) && ok;
                ok = setError(subject, subject.value ? "" : "Please choose a subject.") && ok;
                ok = setError(message, message.value.trim() ? "" : "Please enter a message.") && ok;

                if (!ok) {
                    if (alertBox) { alertBox.className = "hidden"; }
                    var firstBad = form.querySelector(".invalid");
                    if (firstBad) { firstBad.focus(); }
                    return;
                }

                // No backend endpoint — honest frontend-only demo confirmation.
                if (alertBox) {
                    alertBox.textContent = "Thanks, " + name.value.trim() + "! This is a demo form — your message was not sent. Please email info@aqoonhub.com or sign in to contact us.";
                    alertBox.className = "mt-4 p-3 rounded-lg text-sm";
                    alertBox.style.background = "#DEF7EE";
                    alertBox.style.color = "#065F46";
                    alertBox.style.border = "1px solid #A7F3D0";
                    alertBox.setAttribute("role", "status");
                }
                form.reset();
            });
        }

        /* ---- Lucide icons ---- */
        if (window.lucide && typeof window.lucide.createIcons === "function") {
            try { window.lucide.createIcons(); } catch (e) {}
        }
    });
})();
