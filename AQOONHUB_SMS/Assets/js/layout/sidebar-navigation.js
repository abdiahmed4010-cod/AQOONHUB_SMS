/* ============================================================
   AQOONHUB SMS — Sidebar navigation behaviour
   - Collapsible dropdown groups (click / Enter / Space, ARIA)
   - Active parent auto-expanded (server marks .open; this keeps ARIA in sync)
   - Mobile drawer: open/close, Escape, outside click, child-close, body lock
   Loaded once via MainMaster. No external libraries.
   ============================================================ */
(function () {
    "use strict";

    function ready(fn) {
        if (document.readyState !== "loading") { fn(); }
        else { document.addEventListener("DOMContentLoaded", fn); }
    }

    function isMobile() { return window.matchMedia("(max-width: 1023px)").matches; }

    ready(function () {
        var sidebar = document.getElementById("sidebar");
        if (!sidebar) { return; }

        var groups = Array.prototype.slice.call(sidebar.querySelectorAll(".nav-group"));

        /* ---- Sync initial ARIA + active-parent marker from server 'open' state ---- */
        groups.forEach(function (g) {
            var btn = g.querySelector(".nav-parent");
            var open = g.classList.contains("open");
            if (btn) { btn.setAttribute("aria-expanded", open ? "true" : "false"); }
            if (g.querySelector(".nav-item.active")) { g.classList.add("has-active"); }
        });

        function setGroup(g, open) {
            g.classList.toggle("open", open);
            var btn = g.querySelector(".nav-parent");
            if (btn) { btn.setAttribute("aria-expanded", open ? "true" : "false"); }
        }

        function toggleGroup(g) {
            var willOpen = !g.classList.contains("open");
            // On mobile keep only one open at a time.
            if (willOpen && isMobile()) {
                groups.forEach(function (other) { if (other !== g) { setGroup(other, false); } });
            }
            setGroup(g, willOpen);
        }

        groups.forEach(function (g) {
            var btn = g.querySelector(".nav-parent");
            if (!btn) { return; }
            btn.addEventListener("click", function (e) {
                e.preventDefault();
                toggleGroup(g);
            });
            btn.addEventListener("keydown", function (e) {
                if (e.key === "Enter" || e.key === " " || e.key === "Spacebar") {
                    e.preventDefault();
                    toggleGroup(g);
                }
            });
        });

        /* ---- Mobile drawer control (overrides the master's basic toggle) ---- */
        var backdrop = document.getElementById("sidebar-backdrop");
        var menuBtn = document.querySelector('button[aria-controls="sidebar"]');

        function drawerOpen() { return !sidebar.classList.contains("-translate-x-full"); }

        function setDrawer(open) {
            sidebar.classList.toggle("-translate-x-full", !open);
            if (backdrop) { backdrop.classList.toggle("hidden", !open); }
            if (menuBtn) { menuBtn.setAttribute("aria-expanded", open ? "true" : "false"); }
            // Lock body scroll only while the drawer overlays content (mobile).
            document.documentElement.classList.toggle("sidebar-open", open && isMobile());
        }

        // Expose a robust toggle used by the header button and backdrop.
        window.toggleSidebar = function (force) {
            var open = (force !== undefined) ? force : !drawerOpen();
            setDrawer(open);
        };

        if (backdrop) {
            backdrop.addEventListener("click", function () { setDrawer(false); });
        }

        // Close the drawer after choosing a route (mobile only).
        sidebar.querySelectorAll(".nav-submenu .nav-item, a.nav-item").forEach(function (link) {
            link.addEventListener("click", function () {
                if (isMobile()) { setDrawer(false); }
            });
        });

        // Escape closes the mobile drawer.
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && drawerOpen() && isMobile()) { setDrawer(false); }
        });

        // Reset body lock if viewport grows past mobile while drawer was open.
        window.addEventListener("resize", function () {
            if (!isMobile()) { document.documentElement.classList.remove("sidebar-open"); }
        });
    });
})();
