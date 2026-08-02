using System;
using System.Collections.Generic;

namespace AQOONHUB_SMS.Modules.Reports
{
    /// <summary>Central, server-side authorization for the Reports module.
    /// Maps normalized roles to the report categories they may view. Parents never
    /// have management-report access.</summary>
    public static class ReportAuthorization
    {
        // Canonical category keys (must match ReportCatalog.Category values).
        public const string Overview = "Overview";
        public const string Student = "Student";
        public const string Academic = "Academic";
        public const string Examination = "Examination";
        public const string Attendance = "Attendance";
        public const string Finance = "Finance";
        public const string Payroll = "Payroll";
        public const string TeacherStaff = "TeacherStaff";
        public const string Enrollment = "Enrollment";
        public const string Guardian = "Guardian";
        public const string Library = "Library";
        public const string Security = "Security";
        public const string Performance = "Performance";
        public const string CustomBuilder = "CustomBuilder";
        public const string Saved = "Saved";
        public const string Scheduled = "Scheduled";
        public const string ExportHistory = "ExportHistory";
        public const string AuditLog = "AuditLog";

        public static readonly string[] AllCategories =
        {
            Overview, Student, Academic, Examination, Attendance, Finance, Payroll, TeacherStaff,
            Enrollment, Guardian, Library, Security, Performance, CustomBuilder, Saved, Scheduled,
            ExportHistory, AuditLog
        };

        public static string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return string.Empty;
            return role.Trim().Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        public static bool IsParent(string role)
        {
            string r = NormalizeRole(role);
            return r == "parent" || r == "guardian";
        }

        public static bool IsManagement(string role)
        {
            string r = NormalizeRole(role);
            return r == "superadmin" || r == "admin" || r == "academic" || r == "attendanceofficer"
                || r == "registrar" || r == "accountant" || r == "hr" || r == "teacher" || r == "examofficer";
        }

        /// <summary>Category -> normalized roles allowed to view it.</summary>
        private static readonly Dictionary<string, HashSet<string>> Map = Build();

        private static Dictionary<string, HashSet<string>> Build()
        {
            Func<string[], HashSet<string>> set = a => new HashSet<string>(a, StringComparer.Ordinal);
            var m = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            string[] full = { "superadmin", "admin" };
            string[] academicRoles = { "superadmin", "admin", "academic", "examofficer", "attendanceofficer", "registrar", "teacher" };

            m[Overview]      = set(new[] { "superadmin", "admin", "academic", "examofficer", "attendanceofficer", "registrar", "accountant", "hr", "teacher" });
            m[Student]       = set(academicRoles);
            m[Academic]      = set(academicRoles);
            m[Examination]   = set(academicRoles);
            m[Attendance]    = set(academicRoles);
            m[Enrollment]    = set(new[] { "superadmin", "admin", "academic", "examofficer", "attendanceofficer", "registrar" });
            m[Guardian]      = set(new[] { "superadmin", "admin", "registrar" });
            m[Performance]   = set(new[] { "superadmin", "admin", "academic", "examofficer" });
            m[Finance]       = set(new[] { "superadmin", "admin", "accountant" });
            m[Payroll]       = set(new[] { "superadmin", "admin", "accountant" });
            m[TeacherStaff]  = set(new[] { "superadmin", "admin", "hr" });
            m[Library]       = set(new[] { "superadmin", "admin" });
            m[Security]      = set(new[] { "superadmin", "admin" });
            m[CustomBuilder] = set(new[] { "superadmin", "admin", "academic", "registrar", "accountant", "teacher" });
            m[Saved]         = set(new[] { "superadmin", "admin", "academic", "registrar", "accountant", "teacher" });
            m[Scheduled]     = set(new[] { "superadmin", "admin", "academic", "registrar", "accountant", "teacher" });
            m[ExportHistory] = set(new[] { "superadmin", "admin", "academic", "registrar", "accountant", "teacher" });
            m[AuditLog]      = set(new[] { "superadmin", "admin" });
            return m;
        }

        /// <summary>Can this role view the given report category? Parents: never.</summary>
        public static bool CanViewCategory(string role, string category)
        {
            if (IsParent(role)) return false;
            string r = NormalizeRole(role);
            HashSet<string> allowed;
            return Map.TryGetValue(category, out allowed) && allowed.Contains(r);
        }

        /// <summary>May the user open the Reports module at all (has at least Overview)?</summary>
        public static bool CanAccessReports(string role)
        {
            return !IsParent(role) && CanViewCategory(role, Overview);
        }

        public static bool IsStage5Admin(string role)
        {
            string r = NormalizeRole(role); return r == "superadmin" || r == "admin";
        }

        public static bool CanUseStage5(string role, string feature) { return CanViewCategory(role, feature); }

        public static bool CanCreateVisibility(string role, string visibility, bool restricted)
        {
            if (visibility == "Private") return !IsParent(role);
            if (visibility == "Role-Based") return NormalizeRole(role) != "teacher" && !IsParent(role);
            if (visibility == "School-Wide") return IsStage5Admin(role) && !restricted;
            return false;
        }

        /// <summary>Categories visible to this role (for sidebar / overview grid), in canonical order.</summary>
        public static List<string> VisibleCategories(string role)
        {
            var list = new List<string>();
            foreach (string cat in AllCategories) if (CanViewCategory(role, cat)) list.Add(cat);
            return list;
        }

        /// <summary>May this role view sensitive medical/document reports? (management + registrar; never teacher.)</summary>
        public static bool CanViewMedical(string role)
        {
            string r = NormalizeRole(role);
            return r == "superadmin" || r == "admin" || r == "academic" || r == "registrar";
        }

        /// <summary>May this role view salary reports? (management + accountant only.)</summary>
        public static bool CanViewSalary(string role)
        {
            string r = NormalizeRole(role);
            return r == "superadmin" || r == "admin" || r == "accountant";
        }

        /// <summary>Server-side gate: can this role run this specific report (category + sensitivity)?</summary>
        /// <summary>Whole-school exam/attendance reports a Teacher must NOT run (ranking, analytics, cross-class).</summary>
        private static readonly HashSet<string> TeacherRestricted = new HashSet<string>(StringComparer.Ordinal)
        {
            "exam-class-ranking", "exam-subject-ranking", "exam-top-performers", "exam-lowest-performers",
            "exam-overall-analysis", "exam-grade-distribution", "exam-missing-marks", "exam-marks-status",
            "att-alerts", "att-import-history", "att-consecutive", "att-frequent-late", "att-low-attendance"
        };

        public static bool CanRunReport(string role, ReportDefinition def)
        {
            if (def == null) return false;
            string normalized = NormalizeRole(role);
            if (def.Category == Security)
            {
                if (normalized == "superadmin") return true;
                if (normalized != "admin") return false;
                return def.Handler == "security-users" || def.Handler == "security-users-role" ||
                    def.Handler == "security-users-active" || def.Handler == "security-users-inactive" ||
                    def.Handler == "security-login-history" || def.Handler == "security-failed-logins" ||
                    def.Handler == "security-user-activity";
            }
            // Salary is a cross-cutting finance/payroll permission: accountant may run it directly
            // even without broad Teacher/Staff category access.
            if (def.Handler == "staff-salary") return CanViewSalary(role);
            if (!CanViewCategory(role, def.Category)) return false;
            // Teachers are limited to assigned scope: block whole-school ranking/analytics/alert reports.
            if (NormalizeRole(role) == "teacher" && TeacherRestricted.Contains(def.Handler)) return false;
            if (def.Sensitive)
            {
                if (def.Handler == "student-medical" || def.Handler == "student-documents") return CanViewMedical(role);
                return IsManagement(role);
            }
            return true;
        }

        /// <summary>Whether sensitive columns may be included for this role+report.</summary>
        public static bool AllowSensitive(string role, ReportDefinition def)
        {
            if (def == null || !def.Sensitive) return true;
            if (def.Category == Security) return NormalizeRole(role) == "superadmin" || NormalizeRole(role) == "admin";
            if (def.Handler == "staff-salary") return CanViewSalary(role);
            return CanViewMedical(role);
        }

        /// <summary>Human label for a category key.</summary>
        public static string Label(string category)
        {
            switch (category)
            {
                case Overview: return "Overview";
                case Student: return "Student Reports";
                case Academic: return "Academic Reports";
                case Examination: return "Examination Reports";
                case Attendance: return "Attendance Reports";
                case Finance: return "Finance Reports";
                case Payroll: return "Payroll Reports";
                case TeacherStaff: return "Teacher & Staff Reports";
                case Enrollment: return "Enrollment Reports";
                case Guardian: return "Guardian Reports";
                case Library: return "Library Reports";
                case Security: return "Security Reports";
                case Performance: return "Performance Analytics";
                case CustomBuilder: return "Custom Report Builder";
                case Saved: return "Saved Reports";
                case Scheduled: return "Scheduled Reports";
                case ExportHistory: return "Export History";
                case AuditLog: return "Report Audit Log";
                default: return category;
            }
        }

        /// <summary>Category page url (relative to app root). Only pages that exist should be linked by callers.</summary>
        public static string PageUrl(string category)
        {
            switch (category)
            {
                case Overview: return "~/Modules/Reports/Reports.aspx";
                case Student: return "~/Modules/Reports/StudentReports.aspx";
                case Academic: return "~/Modules/Reports/AcademicReports.aspx";
                case Examination: return "~/Modules/Reports/ExaminationReports.aspx";
                case Attendance: return "~/Modules/Reports/AttendanceReports.aspx";
                case Finance: return "~/Modules/Reports/FinanceReports.aspx";
                case Payroll: return "~/Modules/Reports/PayrollReports.aspx";
                case TeacherStaff: return "~/Modules/Reports/TeacherStaffReports.aspx";
                case Enrollment: return "~/Modules/Reports/EnrollmentReports.aspx";
                case Guardian: return "~/Modules/Reports/GuardianReports.aspx";
                case Library: return "~/Modules/Reports/LibraryReports.aspx";
                case Security: return "~/Modules/Reports/SecurityReports.aspx";
                case Performance: return "~/Modules/Reports/PerformanceAnalytics.aspx";
                case CustomBuilder: return "~/Modules/Reports/CustomReportBuilder.aspx";
                case Saved: return "~/Modules/Reports/SavedReports.aspx";
                case Scheduled: return "~/Modules/Reports/ScheduledReports.aspx";
                case ExportHistory: return "~/Modules/Reports/ExportHistory.aspx";
                case AuditLog: return "~/Modules/Reports/ReportAuditLog.aspx";
                default: return "~/Modules/Reports/Reports.aspx";
            }
        }

        public static string Icon(string category)
        {
            switch (category)
            {
                case Student: return "users";
                case Academic: return "book-open";
                case Examination: return "clipboard-list";
                case Attendance: return "user-check";
                case Finance: return "wallet";
                case Payroll: return "banknote";
                case TeacherStaff: return "briefcase";
                case Enrollment: return "user-plus";
                case Guardian: return "contact";
                case Library: return "library";
                case Security: return "shield";
                case Performance: return "trending-up";
                case CustomBuilder: return "sliders-horizontal";
                case Saved: return "bookmark";
                case Scheduled: return "clock";
                case ExportHistory: return "download";
                case AuditLog: return "scroll-text";
                default: return "bar-chart-3";
            }
        }

        /// <summary>Which category pages already exist (so navigation never dead-links).
        /// Stage 1: Overview. Stage 2: Student, Academic, TeacherStaff, Enrollment, Guardian, Library.</summary>
        public static bool PageExists(string category)
        {
            switch (category)
            {
                case Overview:
                case Student:
                case Academic:
                case TeacherStaff:
                case Enrollment:
                case Guardian:
                case Library:
                case Examination:
                case Attendance:
                case Finance:
                case Payroll:
                case Security:
                case Performance:
                case CustomBuilder:
                case Saved:
                case Scheduled:
                case ExportHistory:
                case AuditLog:
                    return true;
                default:
                    return false;
            }
        }
    }
}
