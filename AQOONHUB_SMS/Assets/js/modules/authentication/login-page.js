/* ============================================================
   AQOONHUB SMS — Login page behaviour (Vanilla JS)
   - Password visibility toggle (value never exposed to server)
   - Submit loading state (does not block ASP.NET postback)
   - Dynamic footer year
   ============================================================ */
(function () {
    "use strict";

    function ready(fn) {
        if (document.readyState !== "loading") { fn(); }
        else { document.addEventListener("DOMContentLoaded", fn); }
    }

    ready(function () {
        var cfg = window.AqoonLogin || {};

        /* ---- Password visibility toggle ---- */
        var toggle = document.getElementById("btnTogglePassword");
        var pwd = cfg.passwordId ? document.getElementById(cfg.passwordId) : null;

        if (toggle && pwd) {
            toggle.addEventListener("click", function () {
                var show = pwd.getAttribute("type") === "password";
                pwd.setAttribute("type", show ? "text" : "password");
                toggle.classList.toggle("is-visible", show);
                toggle.setAttribute("aria-pressed", show ? "true" : "false");
                toggle.setAttribute("aria-label", show ? "Hide password" : "Show password");
                pwd.focus();
            });
        }

        /* ---- Submit loading state ----
           Runs on the button's client click. Returns true so the
           ASP.NET postback proceeds normally; only shows a spinner
           and guards against duplicate submits. */
        var btn = document.getElementById(cfg.loginId);
        var wrap = document.getElementById("signinWrap");
        window.aqoonLoginSubmit = function () {
            if (!btn) { return true; }
            var email = cfg.emailId ? document.getElementById(cfg.emailId) : null;
            var hasEmail = email && email.value.trim().length > 0;
            var hasPwd = pwd && pwd.value.trim().length > 0;

            // Let the server render accessible validation errors when empty.
            if (!hasEmail || !hasPwd) { return true; }

            if (btn.dataset.submitting === "1") { return false; }
            btn.dataset.submitting = "1";
            if (wrap) { wrap.classList.add("is-loading"); }
            btn.setAttribute("aria-busy", "true");
            btn.value = "Signing In...";
            return true;
        };

        // Safety: if the page is restored from bfcache, clear loading state.
        window.addEventListener("pageshow", function () {
            if (btn) {
                btn.dataset.submitting = "";
                btn.setAttribute("value", btn.getAttribute("data-label") || btn.value);
                btn.removeAttribute("aria-busy");
            }
            if (wrap) { wrap.classList.remove("is-loading"); }
        });

        /* ---- Dynamic footer year ---- */
        var y = String(new Date().getFullYear());
        var years = document.querySelectorAll(".foot-year");
        for (var i = 0; i < years.length; i++) { years[i].textContent = y; }
    });
})();
