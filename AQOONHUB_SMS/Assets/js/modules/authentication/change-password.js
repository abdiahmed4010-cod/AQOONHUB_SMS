/* ============================================================
   AQOONHUB SMS — Change Password page behaviour (Vanilla JS)
   - Password visibility toggles (values never exposed to server)
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
        var cfg = window.AqoonCp || {};
        var map = { current: cfg.currentId, "new": cfg.newId, confirm: cfg.confirmId };

        var toggles = document.querySelectorAll(".toggle-pw[data-target]");
        for (var i = 0; i < toggles.length; i++) {
            (function (btn) {
                var input = document.getElementById(map[btn.getAttribute("data-target")]);
                if (!input) { return; }
                btn.addEventListener("click", function () {
                    var show = input.getAttribute("type") === "password";
                    input.setAttribute("type", show ? "text" : "password");
                    btn.classList.toggle("is-visible", show);
                    btn.setAttribute("aria-pressed", show ? "true" : "false");
                    btn.setAttribute("aria-label", (show ? "Hide " : "Show ") + (btn.getAttribute("data-target") === "current" ? "current" : btn.getAttribute("data-target")) + " password");
                    input.focus();
                });
            })(toggles[i]);
        }

        var btn = document.getElementById(cfg.submitId);
        var wrap = document.getElementById("signinWrap");
        window.aqoonCpSubmit = function () {
            if (!btn) { return true; }
            if (btn.dataset.submitting === "1") { return false; }
            btn.dataset.submitting = "1";
            if (wrap) { wrap.classList.add("is-loading"); }
            btn.setAttribute("aria-busy", "true");
            btn.value = "Updating...";
            return true;
        };

        window.addEventListener("pageshow", function () {
            if (btn) { btn.dataset.submitting = ""; btn.removeAttribute("aria-busy"); btn.value = btn.getAttribute("data-label") || btn.value; }
            if (wrap) { wrap.classList.remove("is-loading"); }
        });

        var years = document.querySelectorAll(".foot-year");
        var y = String(new Date().getFullYear());
        for (var k = 0; k < years.length; k++) { years[k].textContent = y; }
    });
})();
