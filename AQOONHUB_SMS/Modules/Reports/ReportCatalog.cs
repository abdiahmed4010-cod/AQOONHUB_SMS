using System;
using System.Collections.Generic;

namespace AQOONHUB_SMS.Modules.Reports
{
    /// <summary>Immutable metadata for one whitelisted report.</summary>
    public sealed class ReportDefinition
    {
        public string Key;
        public string Title;
        public string Category;
        public string Description;
        public string Handler;          // whitelisted repository handler id (never SQL, never from user)
        public string Orientation;      // "Portrait" | "Landscape"
        public string[] Filters;        // filter ids the viewer may render (whitelist)
        public bool Sensitive;          // medical / salary / private -> extra permission
        public bool SupportsCharts;
        public bool Available;          // false => viewer shows an honest "unavailable" state
        public string ExportName;
        public string RequiredPermission;
        public string HistoricalSource;

        public ReportDefinition(string key, string title, string category, string handler, string description,
            string orientation, string[] filters, bool sensitive, bool available)
        {
            Key = key; Title = title; Category = category; Handler = handler; Description = description;
            Orientation = orientation; Filters = filters ?? new string[0]; Sensitive = sensitive;
            Available = available; SupportsCharts = false; ExportName = ReportUi.Slug(key);
            RequiredPermission = category; HistoricalSource = null;
        }
    }

    /// <summary>
    /// Strict whitelist mapping report key -> metadata. ReportViewer/category pages resolve keys ONLY
    /// through this catalog. No SQL, table names, columns, WHERE/ORDER BY or user SQL ever come from
    /// the QueryString. The Handler id maps to a hardcoded parameterized query in ReportsRepository.
    /// </summary>
    public static class ReportCatalog
    {
        private static readonly Dictionary<string, ReportDefinition> _byKey = Build();

        public static ReportDefinition Resolve(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            ReportDefinition d;
            return _byKey.TryGetValue(key.Trim().ToLowerInvariant(), out d) ? d : null;
        }

        public static IEnumerable<ReportDefinition> ForCategory(string category)
        {
            foreach (var d in _byKey.Values) if (d.Category == category) yield return d;
        }

        public static int Count { get { return _byKey.Count; } }

        // common filter sets
        private static readonly string[] F_Student = { "year", "class", "section", "gender", "status", "search" };
        private static readonly string[] F_YearClassSec = { "year", "class", "section" };
        private static readonly string[] F_Academic = { "year", "term", "class", "section", "subject", "teacher" };
        private static readonly string[] F_Staff = { "department", "role", "status", "search" };
        private static readonly string[] F_Enroll = { "year", "class", "section", "gender", "from", "to" };
        private static readonly string[] F_None = new string[0];

        private static Dictionary<string, ReportDefinition> Build()
        {
            var d = new Dictionary<string, ReportDefinition>(StringComparer.Ordinal);
            Action<string, string, string, string, string, string, string[], bool, bool> add =
                (key, title, cat, handler, desc, orient, filters, sensitive, available) =>
                    d[key] = new ReportDefinition(key, title, cat, handler, desc, orient, filters, sensitive, available);

            string S = ReportAuthorization.Student, A = ReportAuthorization.Academic, T = ReportAuthorization.TeacherStaff,
                   E = ReportAuthorization.Enrollment, G = ReportAuthorization.Guardian, L = ReportAuthorization.Library;

            // ---------- STUDENT ----------
            add("all-students", "All Students", S, "students-all", "Every student with class, section and status.", "Landscape", F_Student, false, true);
            add("student-profile", "Student Profile", S, "student-profile", "Detailed profile for one student.", "Portrait", new[] { "student" }, false, true);
            add("students-by-class", "Students by Class", S, "students-by-class", "Student counts grouped by class.", "Portrait", F_YearClassSec, false, true);
            add("students-by-section", "Students by Section", S, "students-by-section", "Student counts grouped by section.", "Portrait", F_YearClassSec, false, true);
            add("students-by-gender", "Students by Gender", S, "students-by-gender", "Gender distribution.", "Portrait", new[] { "year", "class", "section" }, false, true);
            add("active-students", "Active Students", S, "students-active", "Currently active students.", "Landscape", F_Student, false, true);
            add("inactive-students", "Inactive Students", S, "students-inactive", "Non-active students.", "Landscape", F_Student, false, true);
            add("new-admissions", "New Admissions", S, "new-admissions", "Recently admitted students.", "Landscape", F_Enroll, false, true);
            add("withdrawn-students", "Withdrawn Students", S, "students-withdrawn", "Students marked withdrawn.", "Landscape", F_YearClassSec, false, true);
            add("transferred-students", "Transferred Students", S, "students-transferred", "Students with transfer records.", "Landscape", F_YearClassSec, false, true);
            add("promoted-students", "Promoted Students", S, "students-promoted", "Students with promotion records.", "Landscape", new[] { "year" }, false, true);
            add("students-by-academic-year", "Students by Academic Year", S, "students-by-year", "Enrollment by academic year.", "Portrait", F_None, false, true);
            add("guardian-information", "Guardian Information", S, "student-guardian-info", "Student primary guardian contacts.", "Landscape", F_YearClassSec, false, true);
            add("student-contact-report", "Student Contact Report", S, "student-contact", "Student contact details.", "Landscape", F_YearClassSec, false, true);
            add("student-medical-information", "Student Medical Information", S, "student-medical", "Confidential medical notes.", "Landscape", F_YearClassSec, true, true);
            add("student-id-list", "Student ID List", S, "student-id-list", "Student codes and admission numbers.", "Portrait", F_YearClassSec, false, true);
            add("student-documents-report", "Student Documents", S, "student-documents", "Uploaded student documents.", "Landscape", new[] { "year", "class", "section" }, true, true);

            // ---------- ACADEMIC ----------
            add("academic-years", "Academic Years", A, "academic-years", "All academic years.", "Portrait", F_None, false, true);
            add("terms", "Terms", A, "terms", "Terms per academic year.", "Portrait", new[] { "year" }, false, true);
            add("classes", "Classes", A, "classes", "All classes with capacity.", "Portrait", new[] { "year" }, false, true);
            add("sections", "Sections", A, "sections", "All sections.", "Portrait", new[] { "year", "class" }, false, true);
            add("subjects", "Subjects", A, "subjects", "All subjects.", "Portrait", F_None, false, true);
            add("subjects-by-class", "Subjects by Class", A, "subjects-by-class", "Subjects assigned per class.", "Landscape", new[] { "year", "class" }, false, true);
            add("class-subject-assignments", "Class-Subject Assignments", A, "class-subject-assignments", "Subjects assigned to sections.", "Landscape", F_Academic, false, true);
            add("teacher-subject-assignments", "Teacher-Subject Assignments", A, "teacher-subject-assignments", "Subjects assigned to teachers.", "Landscape", F_Academic, false, true);
            add("teacher-class-assignments", "Teacher-Class Assignments", A, "teacher-class-assignments", "Classes assigned to teachers.", "Landscape", F_Academic, false, true);
            add("class-timetable", "Class Timetable", A, "class-timetable", "Timetable for a section.", "Landscape", new[] { "year", "class", "section", "term" }, false, true);
            add("teacher-timetable", "Teacher Timetable", A, "teacher-timetable", "Timetable for a teacher.", "Landscape", new[] { "year", "teacher", "term" }, false, true);
            add("subject-timetable", "Subject Timetable", A, "subject-timetable", "Timetable for a subject.", "Landscape", new[] { "year", "subject", "term" }, false, true);
            add("student-promotion-report", "Student Promotion Report", A, "promotion-report", "Promotion records.", "Landscape", new[] { "year" }, false, true);
            add("promotion-history", "Promotion History", A, "promotion-history", "All promotion history.", "Landscape", new[] { "year" }, false, true);
            add("repeated-students", "Repeated Students", A, "repeated-students", "Students repeating a class.", "Landscape", new[] { "year" }, false, true);
            add("class-capacity-report", "Class Capacity Report", A, "class-capacity", "Capacity vs enrolled.", "Landscape", new[] { "year", "class" }, false, true);

            // ---------- TEACHER & STAFF ----------
            add("all-staff", "All Staff", T, "staff-all", "Every staff member.", "Landscape", F_Staff, false, true);
            add("teacher-list", "Teacher List", T, "staff-teachers", "Teaching staff.", "Landscape", F_Staff, false, true);
            add("non-teaching-staff", "Non-Teaching Staff", T, "staff-nonteaching", "Non-teaching staff.", "Landscape", F_Staff, false, true);
            add("staff-by-department", "Staff by Department", T, "staff-by-department", "Staff grouped by department.", "Portrait", F_None, false, true);
            add("staff-by-role", "Staff by Role", T, "staff-by-role", "Staff grouped by role.", "Portrait", F_None, false, true);
            add("active-staff", "Active Staff", T, "staff-active", "Active staff.", "Landscape", F_Staff, false, true);
            add("inactive-staff", "Inactive Staff", T, "staff-inactive", "Inactive staff.", "Landscape", F_Staff, false, true);
            add("newly-employed-staff", "Newly Employed Staff", T, "staff-new", "Recently hired staff.", "Landscape", new[] { "from", "to" }, false, true);
            add("staff-contact-report", "Staff Contact Report", T, "staff-contact", "Staff contact details.", "Landscape", F_Staff, false, true);
            add("teacher-workload", "Teacher Workload", T, "teacher-workload", "Classes, sections, subjects and periods per teacher.", "Landscape", new[] { "year", "teacher" }, false, true);
            add("teacher-subject-assignments2", "Teacher Subject Assignments", T, "teacher-subject-assignments", "Subjects per teacher.", "Landscape", F_Academic, false, true);
            add("teacher-class-assignments2", "Teacher Class Assignments", T, "teacher-class-assignments", "Classes per teacher.", "Landscape", F_Academic, false, true);
            add("class-teachers", "Class Teachers", T, "class-teachers", "Assigned class teachers per section.", "Landscape", new[] { "year", "class" }, false, true);
            add("attendance-responsibility", "Attendance Responsibility", T, "class-teachers", "Class teachers responsible for attendance.", "Landscape", new[] { "year", "class" }, false, true);
            add("invigilator-assignments", "Invigilator Assignments", T, "invigilator-assignments", "Exam invigilator assignments.", "Landscape", F_None, false, true);
            add("staff-salary-summary", "Staff Salary Summary", T, "staff-salary", "Salary per staff member.", "Landscape", new[] { "department", "role" }, true, true);
            add("staff-profile-report", "Staff Profile Report", T, "staff-all", "Detailed staff profiles.", "Landscape", F_Staff, false, true);

            // ---------- ENROLLMENT ----------
            add("admissions-by-date", "Admissions by Date", E, "admissions-by-date", "Admissions grouped by date.", "Portrait", new[] { "from", "to" }, false, true);
            add("admissions-by-academic-year", "Admissions by Academic Year", E, "admissions-by-year", "Admissions per year.", "Portrait", F_None, false, true);
            add("admissions-by-class", "Admissions by Class", E, "admissions-by-class", "Admissions per applying class.", "Portrait", new[] { "year" }, false, true);
            add("admissions-by-section", "Admissions by Section", E, "students-by-section", "Enrolled students per section.", "Portrait", F_YearClassSec, false, true);
            add("admissions-by-gender", "Admissions by Gender", E, "admissions-by-gender", "Admissions by gender.", "Portrait", new[] { "year" }, false, true);
            add("enrollment-trend", "Enrollment Trend", E, "enrollment-trend", "Monthly enrollment counts.", "Portrait", new[] { "year" }, false, true);
            add("current-enrollment", "Current Enrollment", E, "students-active", "Currently enrolled active students.", "Landscape", F_YearClassSec, false, true);
            add("class-enrollment", "Class Enrollment", E, "students-by-class", "Enrollment per class.", "Portrait", new[] { "year" }, false, true);
            add("section-enrollment", "Section Enrollment", E, "students-by-section", "Enrollment per section.", "Portrait", new[] { "year", "class" }, false, true);
            add("enrollment-capacity", "Enrollment Capacity", E, "class-capacity", "Capacity vs enrolled.", "Landscape", new[] { "year", "class" }, false, true);
            add("transferred-in-students", "Transferred-In Students", E, "transfers-in", "Students returned from transfer.", "Landscape", new[] { "year" }, false, true);
            add("transferred-out-students", "Transferred-Out Students", E, "transfers-out", "Students transferred out.", "Landscape", new[] { "year" }, false, true);
            add("withdrawn-students2", "Withdrawn Students", E, "students-withdrawn", "Withdrawn students.", "Landscape", F_YearClassSec, false, true);
            add("re-enrolled-students", "Re-Enrolled Students", E, "transfers-in", "Students re-enrolled after transfer.", "Landscape", new[] { "year" }, false, true);
            add("promotion-enrollment", "Promotion Enrollment", E, "promotion-report", "Promotion-based enrollment.", "Landscape", new[] { "year" }, false, true);
            add("admission-number-report", "Admission Number Report", E, "admission-numbers", "Admission numbers per student.", "Portrait", F_YearClassSec, false, true);
            add("enrollment-comparison-by-year", "Enrollment Comparison by Year", E, "students-by-year", "Enrollment across years.", "Portrait", F_None, false, true);

            // ---------- GUARDIAN ----------
            add("guardian-list", "Guardian List", G, "guardian-list", "All guardians.", "Landscape", new[] { "search" }, false, true);
            add("guardians-by-student", "Guardians by Student", G, "guardians-by-student", "Guardians linked to each student.", "Landscape", F_YearClassSec, false, true);
            add("students-by-guardian", "Students by Guardian", G, "students-by-guardian", "Students linked to each guardian.", "Landscape", new[] { "search" }, false, true);
            add("parents-with-multiple-children", "Parents with Multiple Children", G, "guardian-multi-child", "Guardians linked to 2+ students.", "Landscape", F_None, false, true);
            add("guardian-contact-information", "Guardian Contact Information", G, "guardian-contact", "Guardian phone/email.", "Landscape", new[] { "search" }, false, true);
            add("missing-guardian-contacts", "Missing Guardian Contacts", G, "guardian-missing-contact", "Guardians without phone/email.", "Landscape", F_None, false, true);
            add("parent-user-accounts", "Parent User Accounts", G, "parent-accounts", "Guardian login accounts.", "Landscape", F_None, false, true);
            add("active-parent-accounts", "Active Parent Accounts", G, "parent-accounts-active", "Active guardian login accounts.", "Landscape", F_None, false, true);
            add("parent-student-links", "Parent-Student Links", G, "parent-student-links", "Guardian-student link records.", "Landscape", F_None, false, true);
            add("unlinked-students", "Unlinked Students", G, "unlinked-students", "Students without a guardian link.", "Landscape", F_YearClassSec, false, true);
            add("unlinked-guardians", "Unlinked Guardians", G, "unlinked-guardians", "Guardians without a student link.", "Landscape", F_None, false, true);
            add("parent-attendance-access", "Parent Attendance Access", G, "parent-accounts", "Parents with portal access.", "Landscape", F_None, false, true);
            add("parent-report-card-access", "Parent Report Card Access", G, "parent-accounts", "Parents with report-card access.", "Landscape", F_None, false, true);

            string EX = ReportAuthorization.Examination, AT = ReportAuthorization.Attendance,
                   FI = ReportAuthorization.Finance, PY = ReportAuthorization.Payroll;
            string[] F_Exam = { "year", "term", "exam", "class", "section", "subject", "student" };
            string[] F_ExamScope = { "year", "term", "exam", "class", "section" };
            string[] F_Att = { "year", "class", "section", "from", "to", "student" };
            string[] F_Fin = { "year", "class", "section", "student", "from", "to" };
            string[] F_Pay = { "period", "department", "teacher" };

            // ---------- EXAMINATION ----------
            add("examination-list", "Examination List", EX, "exam-list", "All examinations with year, term and status.", "Portrait", new[] { "year", "term", "exam" }, false, true);
            add("examination-schedule", "Examination Schedule", EX, "exam-schedule", "Exam dates, times, rooms and invigilators.", "Landscape", F_ExamScope, false, true);
            add("examination-timetable", "Examination Timetable", EX, "exam-schedule", "Exam timetable by date/subject.", "Landscape", F_ExamScope, false, true);
            add("examination-rooms", "Examination Rooms", EX, "exam-rooms", "Exam rooms and capacity.", "Portrait", null, false, true);
            add("invigilator-assignments-exam", "Invigilator Assignments", EX, "exam-invigilators", "Invigilators assigned to exam sessions.", "Landscape", new[] { "year", "exam" }, false, true);
            add("marks-entry-status", "Marks Entry Status", EX, "exam-marks-status", "Eligible vs entered/submitted marks per subject.", "Landscape", new[] { "exam" }, false, true);
            add("missing-marks", "Missing Marks", EX, "exam-missing-marks", "Students with no marks entered.", "Landscape", new[] { "exam" }, false, true);
            add("submitted-marks", "Submitted Marks", EX, "exam-submitted-marks", "Submitted marks.", "Landscape", new[] { "exam" }, false, true);
            add("locked-marks", "Locked Marks", EX, "exam-locked-marks", "Locked marks.", "Landscape", new[] { "exam" }, false, true);
            add("student-results", "Student Results", EX, "exam-student-results", "Per-student subject marks and grades.", "Landscape", F_Exam, false, true);
            add("class-results", "Class Results", EX, "exam-class-results", "Class results from immutable published snapshot.", "Landscape", F_ExamScope, false, true);
            add("subject-results", "Subject Results", EX, "exam-subject-results", "Per-subject averages, pass/fail.", "Landscape", new[] { "exam" }, false, true);
            add("grade-distribution", "Grade Distribution", EX, "exam-grade-distribution", "Grade counts from snapshot (not re-graded).", "Portrait", new[] { "exam" }, false, true);
            add("pass-fail-report", "Pass and Fail", EX, "exam-pass-fail", "Passed/failed counts.", "Portrait", new[] { "exam" }, false, true);
            add("top-performing-students", "Top Performing Students", EX, "exam-top-performers", "Highest snapshot averages.", "Landscape", new[] { "exam" }, false, true);
            add("lowest-performing-students", "Lowest Performing Students", EX, "exam-lowest-performers", "Lowest snapshot averages.", "Landscape", new[] { "exam" }, false, true);
            add("class-ranking", "Class Ranking", EX, "exam-class-ranking", "Rank within class/section (snapshot).", "Landscape", new[] { "exam", "section" }, false, true);
            add("subject-ranking", "Subject Ranking", EX, "exam-subject-ranking", "Subjects by average mark.", "Portrait", new[] { "exam" }, false, true);
            add("overall-examination-analysis", "Overall Examination Analysis", EX, "exam-overall-analysis", "Per-exam summary indicators.", "Landscape", new[] { "year", "exam" }, false, true);
            add("published-results", "Published Results", EX, "exam-published-results", "Published snapshot results.", "Landscape", new[] { "exam" }, false, true);
            add("unpublished-results", "Unpublished Results", EX, "exam-unpublished-results", "Unpublished snapshot results.", "Landscape", new[] { "exam" }, false, true);
            add("report-cards", "Report Cards", EX, "exam-report-cards", "Published report-card summary rows.", "Landscape", new[] { "exam" }, false, true);
            add("examination-result-history", "Examination Result History", EX, "exam-result-history", "All snapshot results across exams.", "Landscape", new[] { "exam" }, false, true);

            // ---------- ATTENDANCE ----------
            add("attendance-by-date", "Attendance by Date", AT, "att-by-date", "Submitted/locked attendance for a date range.", "Landscape", F_Att, false, true);
            add("individual-student-attendance", "Individual Student Attendance", AT, "att-individual", "Historical attendance for one student.", "Landscape", new[] { "year", "class", "section", "student" }, false, true);
            add("class-attendance", "Class Attendance", AT, "att-class", "Per-student attendance rate and risk.", "Landscape", F_Att, false, true);
            add("section-attendance", "Section Attendance", AT, "att-section", "Per-student attendance for a section.", "Landscape", F_Att, false, true);
            add("subject-attendance", "Subject Attendance", AT, "att-subject", "Subject-session attendance.", "Landscape", F_Att, false, true);
            add("present-students", "Present Students", AT, "att-present", "Present records.", "Landscape", F_Att, false, true);
            add("absent-students", "Absent Students", AT, "att-absent", "Absent records.", "Landscape", F_Att, false, true);
            add("late-students", "Late Students", AT, "att-late", "Late records.", "Landscape", F_Att, false, true);
            add("excused-students", "Excused Students", AT, "att-excused", "Excused records.", "Landscape", F_Att, false, true);
            add("low-attendance-students", "Low Attendance Students", AT, "att-low-attendance", "Students below the attendance threshold.", "Landscape", F_Att, false, true);
            add("consecutive-absences", "Consecutive Absences", AT, "att-consecutive", "Consecutive-absence alerts.", "Landscape", null, false, true);
            add("frequent-late-arrivals", "Frequent Late Arrivals", AT, "att-frequent-late", "Frequent-late alerts.", "Landscape", null, false, true);
            add("daily-attendance-summary", "Daily Attendance Summary", AT, "att-daily-summary", "Daily counts.", "Portrait", F_Att, false, true);
            add("weekly-attendance-summary", "Weekly Attendance Summary", AT, "att-weekly-summary", "Weekly counts.", "Portrait", F_Att, false, true);
            add("monthly-attendance-summary", "Monthly Attendance Summary", AT, "att-monthly-summary", "Monthly counts.", "Portrait", F_Att, false, true);
            add("attendance-trend", "Attendance Trend", AT, "att-trend", "Daily attendance trend.", "Portrait", F_Att, false, true);
            add("attendance-calendar-report", "Attendance Calendar", AT, "att-calendar", "Per-day attendance counts.", "Landscape", F_Att, false, true);
            add("unsubmitted-attendance", "Unsubmitted Attendance", AT, "att-unsubmitted", "Draft sessions not yet submitted.", "Landscape", new[] { "year", "class", "section" }, false, true);
            add("attendance-alerts-report", "Attendance Alerts", AT, "att-alerts", "Attendance alerts (no internal notes).", "Landscape", null, false, true);
            add("attendance-import-history", "Attendance Import History", AT, "att-import-history", "Import batch metadata (no file content).", "Landscape", null, false, true);

            // ---------- FINANCE ----------
            add("fee-categories", "Fee Categories", FI, "fin-fee-categories", "Fee categories.", "Portrait", null, false, true);
            add("fee-structure", "Fee Structure", FI, "fin-fee-structure", "Fee structures by class.", "Landscape", new[] { "year" }, false, true);
            add("student-fee-statement", "Student Fee Statement", FI, "fin-student-statement", "Invoices, paid and balance for one student.", "Landscape", new[] { "year", "class", "section", "student" }, false, true);
            add("class-fee-report", "Class Fee Report", FI, "fin-class-fee", "Class fee structures.", "Landscape", new[] { "year", "class" }, false, true);
            add("section-fee-report", "Section Fee Report", FI, "fin-section-fee", "Section fee structures.", "Landscape", new[] { "year" }, false, true);
            add("collected-fees", "Collected Fees", FI, "fin-collected", "Payments received.", "Landscape", new[] { "from", "to" }, false, true);
            add("outstanding-fees", "Outstanding Fees", FI, "fin-outstanding", "Invoices with a balance.", "Landscape", new[] { "year" }, false, true);
            add("overdue-fees", "Overdue Fees", FI, "fin-overdue", "Balances past the invoice due date.", "Landscape", new[] { "year" }, false, true);
            add("partial-payments", "Partial Payments", FI, "fin-partial", "Partially paid invoices.", "Landscape", new[] { "year" }, false, true);
            add("fully-paid-students", "Fully Paid Students", FI, "fin-fully-paid", "Fully paid invoices.", "Landscape", new[] { "year" }, false, true);
            add("unpaid-students", "Unpaid Students", FI, "fin-unpaid", "Unpaid invoices.", "Landscape", new[] { "year" }, false, true);
            add("payment-history", "Payment History", FI, "fin-payment-history", "Payment history for a student.", "Landscape", new[] { "year", "class", "section", "student" }, false, true);
            add("daily-collection", "Daily Collection", FI, "fin-daily-collection", "Collection per day.", "Portrait", new[] { "from", "to" }, false, true);
            add("weekly-collection", "Weekly Collection", FI, "fin-weekly-collection", "Collection per week.", "Portrait", new[] { "from", "to" }, false, true);
            add("monthly-collection", "Monthly Collection", FI, "fin-monthly-collection", "Collection per month.", "Portrait", new[] { "from", "to" }, false, true);
            add("academic-year-collection", "Academic Year Collection", FI, "fin-year-collection", "Collection per academic year.", "Portrait", null, false, true);
            add("discounts-report", "Discounts", FI, "fin-discounts", "Invoices with a discount.", "Landscape", null, false, true);
            add("scholarships-report", "Scholarships", FI, "unavailable", "No scholarships data source in this build.", "Portrait", null, false, false);
            add("waivers-report", "Waivers", FI, "unavailable", "No waivers data source in this build.", "Portrait", null, false, false);
            add("refunds-report", "Refunds", FI, "unavailable", "No refunds data source in this build.", "Portrait", null, false, false);
            add("payment-methods-report", "Payment Methods", FI, "fin-payment-methods", "Collection by payment method.", "Portrait", null, false, true);
            add("cashier-collection-report", "Cashier Collection", FI, "fin-cashier", "Collection by cashier.", "Portrait", null, false, true);
            add("income-summary", "Income Summary", FI, "fin-income-summary", "Invoiced/collected/outstanding totals.", "Portrait", null, false, true);
            add("finance-summary", "Finance Summary", FI, "fin-summary", "Overall finance summary.", "Portrait", null, false, true);

            // ---------- PAYROLL (category restricted to management/accountant) ----------
            add("employee-salary-structure", "Employee Salary Structure", PY, "pay-salary-structure", "Current salary structures.", "Landscape", null, false, true);
            add("monthly-payroll", "Monthly Payroll", PY, "pay-monthly", "Payroll snapshot per period.", "Landscape", F_Pay, false, true);
            add("payroll-by-department", "Payroll by Department", PY, "pay-by-department", "Payroll totals by department.", "Portrait", new[] { "period" }, false, true);
            add("payroll-by-employee", "Payroll by Employee", PY, "pay-by-employee", "Payroll per employee.", "Landscape", F_Pay, false, true);
            add("basic-salary-report", "Basic Salary", PY, "pay-basic", "Basic salary per employee (snapshot).", "Portrait", new[] { "period" }, false, true);
            add("allowances-report", "Allowances", PY, "pay-allowances", "Allowances per employee (snapshot).", "Portrait", new[] { "period" }, false, true);
            add("deductions-report", "Deductions", PY, "pay-deductions", "Deductions per employee (snapshot).", "Portrait", new[] { "period" }, false, true);
            add("net-salary-report", "Net Salary", PY, "pay-net", "Net salary per employee (snapshot).", "Portrait", new[] { "period" }, false, true);
            add("paid-salaries", "Paid Salaries", PY, "pay-paid", "Paid payroll records.", "Landscape", new[] { "period" }, false, true);
            add("unpaid-salaries", "Unpaid Salaries", PY, "pay-unpaid", "Unpaid payroll records.", "Landscape", new[] { "period" }, false, true);
            add("payroll-payment-history", "Payroll Payment History", PY, "pay-history", "Payroll payment history.", "Landscape", null, false, true);
            add("payslips-report", "Payslips", PY, "pay-payslips", "Payslip rows (snapshot).", "Landscape", F_Pay, false, true);
            add("pay-run-report", "Pay Run Report", PY, "pay-run", "Pay-run totals per period.", "Landscape", null, false, true);
            add("cancelled-payments", "Cancelled Payments", PY, "pay-cancelled", "Cancelled payroll records.", "Landscape", new[] { "period" }, false, true);
            add("payroll-summary", "Payroll Summary", PY, "pay-summary", "Payroll totals summary.", "Portrait", new[] { "period" }, false, true);
            add("annual-payroll-cost", "Annual Payroll Cost", PY, "pay-annual-cost", "Net payroll cost per year.", "Portrait", null, false, true);
            add("salary-change-history", "Salary Change History", PY, "pay-salary-change", "Salary structure history.", "Landscape", null, false, true);

            // ---------- USER & SECURITY (explicit safe columns; unavailable means no authoritative source) ----------
            string SE = ReportAuthorization.Security, PA = ReportAuthorization.Performance;
            add("user-accounts", "User Accounts", SE, "security-users", "Safe account details without credentials or tokens.", "Landscape", new[] { "role", "search" }, true, true);
            add("users-by-role", "Users by Role", SE, "security-users-role", "Account totals using the authoritative Users.Role field.", "Portrait", new[] { "role" }, true, true);
            add("active-users", "Active Users", SE, "security-users-active", "Accounts marked active.", "Landscape", null, true, true);
            add("inactive-users", "Inactive Users", SE, "security-users-inactive", "Accounts marked inactive.", "Landscape", null, true, true);
            add("locked-accounts", "Locked Accounts", SE, "security-unavailable", "Locked-account reporting is unavailable because account lock status is not stored.", "Portrait", null, true, false);
            add("login-history", "Login History", SE, "security-login-history", "Recorded sign-in activity.", "Landscape", new[] { "from", "to" }, true, true);
            add("failed-login-attempts", "Failed Login Attempts", SE, "security-failed-logins", "Recorded login rows whose status is Failed.", "Landscape", new[] { "from", "to" }, true, true);
            add("password-change-audit", "Password Change Audit", SE, "security-unavailable", "Password-change reporting is unavailable because no authoritative audit source is stored.", "Landscape", null, true, false);
            add("role-permission-report", "Role Permission Report", SE, "security-role-permissions", "Current role-permission mappings; empty when none are assigned.", "Landscape", null, true, true);
            add("user-activity", "User Activity", SE, "security-user-activity", "Recorded login activity only.", "Landscape", new[] { "from", "to" }, true, true);
            add("audit-log", "Audit Log", SE, "security-audit-log", "Recorded system audit activity.", "Landscape", new[] { "from", "to" }, true, true);
            add("record-change-history", "Record Creation/Update History", SE, "security-unavailable", "Unavailable because no consistent authoritative record-history source is stored.", "Landscape", null, true, false);
            add("sensitive-actions", "Sensitive Actions", SE, "security-sensitive-actions", "Recorded security, role, user, delete and suspension actions.", "Landscape", new[] { "from", "to" }, true, true);

            // ---------- PERFORMANCE ANALYTICS (published historical snapshots only) ----------
            add("student-performance-trend", "Student Performance Trend", PA, "analytics-trend", "Published examination performance over time.", "Landscape", F_ExamScope, false, true);
            add("class-performance-comparison", "Class Performance Comparison", PA, "analytics-classes", "Historical class averages from published snapshots.", "Landscape", F_ExamScope, false, true);
            add("subject-performance-comparison", "Subject Performance Comparison", PA, "analytics-subjects", "Published submitted subject-result averages.", "Landscape", F_Exam, false, true);
            add("performance-pass-fail", "Pass and Fail Distribution", PA, "analytics-pass-fail", "Published snapshot result-status distribution.", "Portrait", F_ExamScope, false, true);
            add("enrollment-growth", "Enrollment Growth", PA, "analytics-enrollment", "Real student enrollment dates grouped by month.", "Landscape", new[] { "year", "from", "to" }, false, true);
            add("academic-year-comparison", "Academic Year Comparison", PA, "analytics-years", "Published snapshot averages by academic year.", "Landscape", null, false, true);
            add("attendance-examination-relationship", "Attendance vs Examination", PA, "analytics-attendance-exam", "Observed attendance and examination relationship; no causal claim.", "Landscape", F_ExamScope, false, true);
            add("top-performing-classes", "Top Performing Classes", PA, "analytics-top-classes", "Highest historical class averages.", "Landscape", F_ExamScope, false, true);
            add("low-performing-classes", "Low-Performing Classes", PA, "analytics-low-classes", "Lowest historical class averages.", "Landscape", F_ExamScope, false, true);
            add("at-risk-students", "At-Risk Students", PA, "analytics-at-risk", "Published failures and official attendance below the configured threshold.", "Landscape", F_ExamScope, true, true);

            foreach (ReportDefinition item in d.Values)
            {
                if (item.Category == PA) { item.SupportsCharts = item.Handler.StartsWith("analytics-"); item.HistoricalSource = item.Handler == "analytics-enrollment" ? "Students.EnrollmentDate" : "StudentExamSummaries (published snapshot)"; }
                if (item.Category == SE) item.RequiredPermission = "Security management";
            }

            // ---------- LIBRARY (no data source in this build -> unavailable, honest) ----------
            foreach (var lk in new[]{ "book-inventory","books-by-category","available-books","borrowed-books","overdue-books","lost-books",
                "damaged-books","student-borrowing-history","staff-borrowing-history","most-borrowed-books","library-fines","monthly-library-activity" })
                add(lk, TitleCase(lk), L, "unavailable", "Requires a Library data source (not present in this build).", "Portrait", F_None, false, false);

            return d;
        }

        private static string TitleCase(string key)
        {
            var parts = key.Split('-');
            for (int i = 0; i < parts.Length; i++)
                if (parts[i].Length > 0) parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
            return string.Join(" ", parts);
        }
    }
}
