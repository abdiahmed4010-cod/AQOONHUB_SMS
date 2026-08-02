using System;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Reports
{
    /// <summary>Whitelisted report filters (parsed/validated by the page; never raw SQL).</summary>
    public sealed class ReportFilter
    {
        public int? YearID, TermID, ClassID, SectionID, SubjectID, StaffID, StudentID, GuardianID, ExamID, PeriodID;
        public string Gender, Status, Search, Department, Role, Method, PublicationStatus;
        public DateTime? From, To;
        public int Page = 1, PageSize = 50;

        public static readonly string[] Genders = { "Male", "Female" };
        public string SafeGender { get { return (Gender == "Male" || Gender == "Female") ? Gender : null; } }
    }

    public sealed partial class ReportsRepository
    {
        /// <summary>Central, whitelisted report dispatch. Handler ids come only from ReportCatalog.
        /// Every query is hardcoded and parameterized. Sensitive columns require allowSensitive.</summary>
        public DataTable GetReportData(string handler, ReportFilter f, bool allowSensitive)
        {
            if (f == null) f = new ReportFilter();
            switch (handler)
            {
                // ---------------- STUDENTS ----------------
                case "students-all": return Students(f, null);
                case "students-active": return Students(f, "Active");
                case "students-inactive": return Students(f, "!Active");
                case "students-withdrawn": return Students(f, "Withdrawn");
                case "students-by-class": return CountBy(f, "c.ClassName", "Class");
                case "students-by-section": return CountBy(f, "c.ClassName + ' / ' + sec.SectionName", "Section");
                case "students-by-gender": return CountBy(f, "ISNULL(st.Gender,'Unknown')", "Gender");
                case "students-by-year": return ExecuteDataTable(
                    "SELECT y.YearName AS [Academic Year], COUNT(st.StudentID) AS Students FROM AcademicYears y LEFT JOIN Students st ON st.AcademicYearID=y.AcademicYearID GROUP BY y.YearName, y.StartDate ORDER BY y.StartDate DESC", null);
                case "student-id-list": return ExecuteDataTable(
                    "SELECT st.StudentCode AS [Student Code], st.AdmissionNo AS [Admission No], st.FullName AS [Name], ISNULL(c.ClassName,'') AS Class, ISNULL(sec.SectionName,'') AS Section " + StudentJoins() + StudentWhere(f, null) + " ORDER BY st.FullName", StudentParams(f));
                case "student-contact": return ExecuteDataTable(
                    "SELECT st.StudentCode AS [Code], st.FullName AS [Name], ISNULL(st.Address,'') AS Address, ISNULL(g.Phone,'') AS [Guardian Phone], ISNULL(g.Email,'') AS [Guardian Email] " + StudentJoins() + " LEFT JOIN Guardians g ON g.GuardianID=st.GuardianID " + StudentWhere(f, null) + " ORDER BY st.FullName", StudentParams(f));
                case "student-guardian-info": return ExecuteDataTable(
                    "SELECT st.StudentCode AS [Code], st.FullName AS [Student], ISNULL(g.FullName,'') AS [Guardian], ISNULL(g.Relationship,'') AS [Relationship], ISNULL(g.Phone,'') AS [Phone] " + StudentJoins() + " LEFT JOIN Guardians g ON g.GuardianID=st.GuardianID " + StudentWhere(f, null) + " ORDER BY st.FullName", StudentParams(f));
                case "student-medical":
                    if (!allowSensitive) return Denied();
                    return ExecuteDataTable("SELECT st.StudentCode AS [Code], st.FullName AS [Student], ISNULL(c.ClassName,'') AS Class, ISNULL(st.MedicalNotes,'') AS [Medical Notes] " + StudentJoins() + StudentWhere(f, null) + " AND ISNULL(st.MedicalNotes,'')<>'' ORDER BY st.FullName", StudentParams(f));
                case "student-documents":
                    if (!allowSensitive) return Denied();
                    return ExecuteDataTable("SELECT st.FullName AS [Student], d.DocumentName AS [Document], d.DocumentType AS [Type], d.UploadedAt AS [Uploaded] FROM Documents d JOIN Students st ON d.EntityType='Student' AND d.EntityID=st.StudentID ORDER BY d.UploadedAt DESC", null);
                case "student-profile": return StudentProfile(f);
                case "students-transferred": return ExecuteDataTable(
                    "SELECT st.StudentCode AS [Code], st.FullName AS [Student], t.TransferType AS [Type], t.DestinationSchool AS [Destination], t.TransferDate AS [Date], t.TransferStatus AS [Status] FROM StudentTransfers t JOIN Students st ON st.StudentID=t.StudentID ORDER BY t.TransferDate DESC", null);
                case "students-promoted": return ExecuteDataTable(
                    "SELECT st.StudentCode AS [Code], st.FullName AS [Student], y1.YearName AS [From Year], y2.YearName AS [To Year], p.Status, p.ActionDate AS [Date] FROM StudentPromotions p JOIN Students st ON st.StudentID=p.StudentID LEFT JOIN AcademicYears y1 ON y1.AcademicYearID=p.FromAcademicYearID LEFT JOIN AcademicYears y2 ON y2.AcademicYearID=p.ToAcademicYearID ORDER BY p.ActionDate DESC", null);
                case "new-admissions": return ExecuteDataTable(
                    "SELECT st.AdmissionNo AS [Admission No], st.FullName AS [Student], ISNULL(c.ClassName,'') AS Class, st.EnrollmentDate AS [Enrolled], st.Status " + StudentJoins() + StudentWhere(f, "Active") + DateRange(f, "st.EnrollmentDate") + " ORDER BY st.EnrollmentDate DESC", StudentParams(f));
                case "admission-numbers": return ExecuteDataTable(
                    "SELECT st.AdmissionNo AS [Admission No], st.StudentCode AS [Code], st.FullName AS [Student], ISNULL(c.ClassName,'') AS Class " + StudentJoins() + StudentWhere(f, null) + " ORDER BY st.AdmissionNo", StudentParams(f));

                // ---------------- ACADEMIC ----------------
                case "academic-years": return ExecuteDataTable("SELECT YearName AS [Academic Year], StartDate AS [Start], EndDate AS [End], Status FROM AcademicYears ORDER BY StartDate DESC", null);
                case "terms": return ExecuteDataTable("SELECT t.TermName AS [Term], y.YearName AS [Academic Year], t.StartDate AS [Start], t.EndDate AS [End], t.Status FROM Terms t JOIN AcademicYears y ON y.AcademicYearID=t.AcademicYearID WHERE (@y IS NULL OR t.AcademicYearID=@y) ORDER BY t.StartDate", YP(f));
                case "classes": return ExecuteDataTable("SELECT c.ClassName AS [Class], ISNULL(c.ClassCode,'') AS [Code], ISNULL(c.Level,'') AS [Level], ISNULL(c.Capacity,0) AS [Capacity], ISNULL(c.Status,'Active') AS [Status] FROM Classes c WHERE (@y IS NULL OR c.AcademicYearID=@y) ORDER BY c.ClassName", YP(f));
                case "sections": return ExecuteDataTable("SELECT c.ClassName AS [Class], sec.SectionName AS [Section], ISNULL(sec.Capacity,0) AS [Capacity], ISNULL(sec.RoomNumber,'') AS [Room], ISNULL(sec.Status,'Active') AS [Status] FROM Sections sec JOIN Classes c ON c.ClassID=sec.ClassID WHERE (@y IS NULL OR sec.AcademicYearID=@y) AND (@c IS NULL OR sec.ClassID=@c) ORDER BY c.ClassName, sec.SectionName", YCP(f));
                case "subjects": return ExecuteDataTable("SELECT SubjectName AS [Subject], SubjectCode AS [Code], ISNULL(SubjectType,'') AS [Type], ISNULL(MaxMarks,0) AS [Max Marks], ISNULL(PassMarks,0) AS [Pass Marks] FROM Subjects WHERE ISNULL(IsActive,1)=1 ORDER BY SubjectName", null);
                case "subjects-by-class": return ExecuteDataTable(
                    "SELECT DISTINCT c.ClassName AS [Class], sub.SubjectName AS [Subject], sub.SubjectCode AS [Code] FROM ClassSubjectTeachers cst JOIN Sections sec ON sec.SectionID=cst.SectionID JOIN Classes c ON c.ClassID=sec.ClassID JOIN Subjects sub ON sub.SubjectID=cst.SubjectID WHERE (@y IS NULL OR cst.AcademicYearID=@y) AND (@c IS NULL OR c.ClassID=@c) ORDER BY c.ClassName, sub.SubjectName", YCP(f));
                case "class-subject-assignments": return ExecuteDataTable(
                    "SELECT c.ClassName AS [Class], sec.SectionName AS [Section], sub.SubjectName AS [Subject], ISNULL(u.FullName,'—') AS [Teacher], ISNULL(cst.WeeklyPeriods,0) AS [Periods] FROM ClassSubjectTeachers cst JOIN Sections sec ON sec.SectionID=cst.SectionID JOIN Classes c ON c.ClassID=sec.ClassID JOIN Subjects sub ON sub.SubjectID=cst.SubjectID LEFT JOIN Staff sf ON sf.StaffID=cst.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID WHERE ISNULL(cst.IsActive,1)=1 AND (@y IS NULL OR cst.AcademicYearID=@y) AND (@c IS NULL OR c.ClassID=@c) AND (@sec IS NULL OR sec.SectionID=@sec) AND (@subj IS NULL OR sub.SubjectID=@subj) ORDER BY c.ClassName, sec.SectionName, sub.SubjectName", AcademicParams(f));
                case "teacher-subject-assignments": return ExecuteDataTable(
                    "SELECT ISNULL(u.FullName,sf.EmployeeID) AS [Teacher], sub.SubjectName AS [Subject], c.ClassName AS [Class], sec.SectionName AS [Section] FROM ClassSubjectTeachers cst JOIN Sections sec ON sec.SectionID=cst.SectionID JOIN Classes c ON c.ClassID=sec.ClassID JOIN Subjects sub ON sub.SubjectID=cst.SubjectID JOIN Staff sf ON sf.StaffID=cst.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID WHERE ISNULL(cst.IsActive,1)=1 AND (@y IS NULL OR cst.AcademicYearID=@y) AND (@staff IS NULL OR cst.StaffID=@staff) ORDER BY [Teacher], sub.SubjectName", AcademicParams(f));
                case "teacher-class-assignments": return ExecuteDataTable(
                    "SELECT DISTINCT ISNULL(u.FullName,sf.EmployeeID) AS [Teacher], c.ClassName AS [Class], sec.SectionName AS [Section] FROM ClassSubjectTeachers cst JOIN Sections sec ON sec.SectionID=cst.SectionID JOIN Classes c ON c.ClassID=sec.ClassID JOIN Staff sf ON sf.StaffID=cst.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID WHERE ISNULL(cst.IsActive,1)=1 AND (@y IS NULL OR cst.AcademicYearID=@y) AND (@staff IS NULL OR cst.StaffID=@staff) ORDER BY [Teacher], c.ClassName", AcademicParams(f));
                case "class-timetable": return Timetable(f, "class");
                case "teacher-timetable": return Timetable(f, "teacher");
                case "subject-timetable": return Timetable(f, "subject");
                case "class-teachers": return ExecuteDataTable(
                    "SELECT c.ClassName AS [Class], sec.SectionName AS [Section], ISNULL(u.FullName,ISNULL(sf.EmployeeID,'— none —')) AS [Class Teacher] FROM Sections sec JOIN Classes c ON c.ClassID=sec.ClassID LEFT JOIN Staff sf ON sf.StaffID=sec.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID WHERE (@y IS NULL OR sec.AcademicYearID=@y) AND (@c IS NULL OR c.ClassID=@c) ORDER BY c.ClassName, sec.SectionName", YCP(f));
                case "class-capacity": return ExecuteDataTable(
                    "SELECT c.ClassName AS [Class], sec.SectionName AS [Section], ISNULL(sec.Capacity,0) AS [Capacity], COUNT(st.StudentID) AS [Enrolled], (ISNULL(sec.Capacity,0)-COUNT(st.StudentID)) AS [Available], CASE WHEN ISNULL(sec.Capacity,0)=0 THEN 'No Capacity Set' WHEN COUNT(st.StudentID)>sec.Capacity THEN 'Over Capacity' WHEN COUNT(st.StudentID)=sec.Capacity THEN 'Full' WHEN COUNT(st.StudentID)>=sec.Capacity*0.9 THEN 'Near Capacity' ELSE 'Available' END AS [Status] FROM Sections sec JOIN Classes c ON c.ClassID=sec.ClassID LEFT JOIN Students st ON st.SectionID=sec.SectionID AND ISNULL(st.Status,'Active')='Active' WHERE (@y IS NULL OR sec.AcademicYearID=@y) AND (@c IS NULL OR c.ClassID=@c) GROUP BY c.ClassName, sec.SectionName, sec.Capacity ORDER BY c.ClassName, sec.SectionName", YCP(f));
                case "promotion-report":
                case "promotion-history": return ExecuteDataTable(
                    "SELECT st.FullName AS [Student], y1.YearName AS [From Year], y2.YearName AS [To Year], p.Status, p.ActionDate AS [Date] FROM StudentPromotions p JOIN Students st ON st.StudentID=p.StudentID LEFT JOIN AcademicYears y1 ON y1.AcademicYearID=p.FromAcademicYearID LEFT JOIN AcademicYears y2 ON y2.AcademicYearID=p.ToAcademicYearID WHERE (@y IS NULL OR p.ToAcademicYearID=@y OR p.FromAcademicYearID=@y) ORDER BY p.ActionDate DESC", YP(f));
                case "repeated-students": return ExecuteDataTable(
                    "SELECT st.FullName AS [Student], y1.YearName AS [From Year], y2.YearName AS [To Year], p.ActionDate AS [Date] FROM StudentPromotions p JOIN Students st ON st.StudentID=p.StudentID LEFT JOIN AcademicYears y1 ON y1.AcademicYearID=p.FromAcademicYearID LEFT JOIN AcademicYears y2 ON y2.AcademicYearID=p.ToAcademicYearID WHERE p.Status LIKE '%Repeat%' ORDER BY p.ActionDate DESC", null);

                // ---------------- STAFF ----------------
                case "staff-all": return Staff(f, null, null);
                case "staff-teachers": return Staff(f, "teacher", null);
                case "staff-nonteaching": return Staff(f, "nonteacher", null);
                case "staff-active": return Staff(f, null, "Active");
                case "staff-inactive": return Staff(f, null, "!Active");
                case "staff-by-department": return ExecuteDataTable("SELECT ISNULL(Department,'Unassigned') AS [Department], COUNT(*) AS [Staff] FROM Staff GROUP BY ISNULL(Department,'Unassigned') ORDER BY [Staff] DESC", null);
                case "staff-by-role": return ExecuteDataTable("SELECT ISNULL(u.Role,'—') AS [Role], COUNT(*) AS [Staff] FROM Staff sf LEFT JOIN Users u ON u.UserID=sf.UserID GROUP BY ISNULL(u.Role,'—') ORDER BY [Staff] DESC", null);
                case "staff-new": return ExecuteDataTable("SELECT sf.EmployeeID AS [Employee], ISNULL(u.FullName,'') AS [Name], ISNULL(sf.Department,'') AS [Department], ISNULL(sf.Position,'') AS [Position], sf.HireDate AS [Hired] FROM Staff sf LEFT JOIN Users u ON u.UserID=sf.UserID WHERE (@from IS NULL OR sf.HireDate>=@from) AND (@to IS NULL OR sf.HireDate<=@to) ORDER BY sf.HireDate DESC", DateParams(f));
                case "staff-contact": return ExecuteDataTable("SELECT sf.EmployeeID AS [Employee], ISNULL(u.FullName,'') AS [Name], ISNULL(u.Email,'') AS [Email], ISNULL(u.Phone,'') AS [Phone], ISNULL(sf.Department,'') AS [Department] FROM Staff sf LEFT JOIN Users u ON u.UserID=sf.UserID ORDER BY [Name]", null);
                case "teacher-workload": return TeacherWorkload(f);
                case "invigilator-assignments": return InvigilatorAssignments();
                case "staff-salary":
                    if (!allowSensitive) return Denied();
                    return ExecuteDataTable("SELECT sf.EmployeeID AS [Employee], ISNULL(u.FullName,'') AS [Name], ISNULL(sf.Department,'') AS [Department], ISNULL(sf.Position,'') AS [Position], ISNULL(sf.Salary,0) AS [Salary] FROM Staff sf LEFT JOIN Users u ON u.UserID=sf.UserID WHERE (@dept IS NULL OR sf.Department=@dept) ORDER BY [Name]", new[] { P("@dept", (object)NullIfEmpty(f.Department) ?? DBNull.Value) });

                // ---------------- ENROLLMENT ----------------
                case "admissions-by-date": return ExecuteDataTable("SELECT CAST(ApplicationDate AS date) AS [Date], COUNT(*) AS [Admissions] FROM Admissions WHERE (@from IS NULL OR ApplicationDate>=@from) AND (@to IS NULL OR ApplicationDate<=@to) GROUP BY CAST(ApplicationDate AS date) ORDER BY [Date] DESC", DateParams(f));
                case "admissions-by-year": return ExecuteDataTable("SELECT ISNULL(y.YearName,'—') AS [Academic Year], COUNT(*) AS [Admissions] FROM Admissions a LEFT JOIN AcademicYears y ON y.AcademicYearID=a.AcademicYearID GROUP BY y.YearName, y.StartDate ORDER BY y.StartDate DESC", null);
                case "admissions-by-class": return ExecuteDataTable("SELECT ISNULL(c.ClassName,'—') AS [Applying For], COUNT(*) AS [Admissions] FROM Admissions a LEFT JOIN Classes c ON c.ClassID=a.ApplyingForClassID WHERE (@y IS NULL OR a.AcademicYearID=@y) GROUP BY c.ClassName ORDER BY [Admissions] DESC", YP(f));
                case "admissions-by-gender": return ExecuteDataTable("SELECT ISNULL(Gender,'Unknown') AS [Gender], COUNT(*) AS [Admissions] FROM Admissions WHERE (@y IS NULL OR AcademicYearID=@y) GROUP BY Gender", YP(f));
                case "enrollment-trend": return ExecuteDataTable("SELECT DATEFROMPARTS(YEAR(EnrollmentDate),MONTH(EnrollmentDate),1) AS Bucket, COUNT(*) AS [Enrolled] FROM Students WHERE EnrollmentDate IS NOT NULL AND (@y IS NULL OR AcademicYearID=@y) GROUP BY DATEFROMPARTS(YEAR(EnrollmentDate),MONTH(EnrollmentDate),1) ORDER BY Bucket", YP(f));
                case "transfers-out": return ExecuteDataTable("SELECT st.FullName AS [Student], t.TransferType AS [Type], ISNULL(t.DestinationSchool,'') AS [Destination], t.TransferDate AS [Date], t.TransferStatus AS [Status] FROM StudentTransfers t JOIN Students st ON st.StudentID=t.StudentID WHERE t.TransferType<>'Return' ORDER BY t.TransferDate DESC", null);
                case "transfers-in": return ExecuteDataTable("SELECT st.FullName AS [Student], t.TransferDate AS [Date], ISNULL(t.ReturnReason,'') AS [Reason], t.TransferStatus AS [Status] FROM StudentTransfers t JOIN Students st ON st.StudentID=t.StudentID WHERE t.ReturnedDate IS NOT NULL ORDER BY t.ReturnedDate DESC", null);

                // ---------------- GUARDIAN ----------------
                case "guardian-list": return ExecuteDataTable("SELECT g.FullName AS [Guardian], ISNULL(g.Relationship,'') AS [Relationship], ISNULL(g.Phone,'') AS [Phone], ISNULL(g.Email,'') AS [Email], CASE WHEN ISNULL(g.IsActive,1)=1 THEN 'Active' ELSE 'Inactive' END AS [Status] FROM Guardians g WHERE (@s='' OR g.FullName LIKE '%'+@s+'%') ORDER BY g.FullName", SearchP(f));
                case "guardians-by-student": return ExecuteDataTable("SELECT st.FullName AS [Student], g.FullName AS [Guardian], ISNULL(g.Relationship,'') AS [Relationship], CASE WHEN sg.IsPrimary=1 THEN 'Primary' ELSE 'Secondary' END AS [Link], ISNULL(g.Phone,'') AS [Phone] FROM StudentGuardians sg JOIN Students st ON st.StudentID=sg.StudentID JOIN Guardians g ON g.GuardianID=sg.GuardianID ORDER BY st.FullName", null);
                case "students-by-guardian": return ExecuteDataTable("SELECT g.FullName AS [Guardian], st.FullName AS [Student], st.StudentCode AS [Code], CASE WHEN sg.IsPrimary=1 THEN 'Primary' ELSE 'Secondary' END AS [Link] FROM StudentGuardians sg JOIN Guardians g ON g.GuardianID=sg.GuardianID JOIN Students st ON st.StudentID=sg.StudentID WHERE (@s='' OR g.FullName LIKE '%'+@s+'%') ORDER BY g.FullName, st.FullName", SearchP(f));
                case "guardian-multi-child": return ExecuteDataTable("SELECT g.FullName AS [Guardian], COUNT(sg.StudentID) AS [Children], ISNULL(g.Phone,'') AS [Phone] FROM Guardians g JOIN StudentGuardians sg ON sg.GuardianID=g.GuardianID GROUP BY g.FullName, g.Phone HAVING COUNT(sg.StudentID)>1 ORDER BY [Children] DESC", null);
                case "guardian-contact": return ExecuteDataTable("SELECT g.FullName AS [Guardian], ISNULL(g.Phone,'') AS [Phone], ISNULL(g.AlternatePhone,'') AS [Alt Phone], ISNULL(g.Email,'') AS [Email], ISNULL(g.Address,'') AS [Address] FROM Guardians g WHERE (@s='' OR g.FullName LIKE '%'+@s+'%') ORDER BY g.FullName", SearchP(f));
                case "guardian-missing-contact": return ExecuteDataTable("SELECT g.FullName AS [Guardian], ISNULL(g.Relationship,'') AS [Relationship] FROM Guardians g WHERE ISNULL(g.Phone,'')='' AND ISNULL(g.Email,'')='' ORDER BY g.FullName", null);
                case "parent-accounts": return ExecuteDataTable("SELECT g.FullName AS [Guardian], u.Email AS [Login Email], CASE WHEN ISNULL(u.IsActive,0)=1 THEN 'Active' ELSE 'Inactive' END AS [Account], ISNULL(u.LastLogin,NULL) AS [Last Login] FROM Guardians g JOIN Users u ON u.UserID=g.UserID ORDER BY g.FullName", null);
                case "parent-accounts-active": return ExecuteDataTable("SELECT g.FullName AS [Guardian], u.Email AS [Login Email], u.LastLogin AS [Last Login] FROM Guardians g JOIN Users u ON u.UserID=g.UserID WHERE ISNULL(u.IsActive,0)=1 ORDER BY g.FullName", null);
                case "parent-student-links": return ExecuteDataTable("SELECT g.FullName AS [Guardian], st.FullName AS [Student], CASE WHEN sg.IsPrimary=1 THEN 'Primary' ELSE 'Secondary' END AS [Link] FROM StudentGuardians sg JOIN Guardians g ON g.GuardianID=sg.GuardianID JOIN Students st ON st.StudentID=sg.StudentID ORDER BY g.FullName", null);
                case "unlinked-students": return ExecuteDataTable("SELECT st.StudentCode AS [Code], st.FullName AS [Student], ISNULL(c.ClassName,'') AS Class " + StudentJoins() + " WHERE NOT EXISTS (SELECT 1 FROM StudentGuardians sg WHERE sg.StudentID=st.StudentID) AND (@y IS NULL OR st.AcademicYearID=@y) ORDER BY st.FullName", YP(f));
                case "unlinked-guardians": return ExecuteDataTable("SELECT g.FullName AS [Guardian], ISNULL(g.Phone,'') AS [Phone] FROM Guardians g WHERE NOT EXISTS (SELECT 1 FROM StudentGuardians sg WHERE sg.GuardianID=g.GuardianID) ORDER BY g.FullName", null);

                case "unavailable": return null;
                default: return Stage3(handler, f, allowSensitive);
            }
        }

        // ---- shared builders ----
        private static string StudentJoins()
        { return " FROM Students st LEFT JOIN Sections sec ON sec.SectionID=st.SectionID LEFT JOIN Classes c ON c.ClassID=sec.ClassID"; }

        private static string StudentWhere(ReportFilter f, string statusMode)
        {
            string w = " WHERE 1=1";
            if (f.YearID.HasValue) w += " AND st.AcademicYearID=@y";
            if (f.ClassID.HasValue) w += " AND sec.ClassID=@c";
            if (f.SectionID.HasValue) w += " AND st.SectionID=@sec";
            if (f.SafeGender != null) w += " AND st.Gender=@g";
            if (!string.IsNullOrEmpty(f.Search)) w += " AND (st.FullName LIKE '%'+@s+'%' OR st.StudentCode LIKE '%'+@s+'%')";
            if (statusMode == "Active") w += " AND ISNULL(st.Status,'Active')='Active'";
            else if (statusMode == "!Active") w += " AND ISNULL(st.Status,'Active')<>'Active'";
            else if (statusMode == "Withdrawn") w += " AND ISNULL(st.Status,'')='Withdrawn'";
            else if (!string.IsNullOrEmpty(f.Status) && statusMode == null) w += " AND ISNULL(st.Status,'Active')=@st";
            return w;
        }

        private SqlParameter[] StudentParams(ReportFilter f)
        {
            return new[]
            {
                P("@y",(object)f.YearID??DBNull.Value), P("@c",(object)f.ClassID??DBNull.Value), P("@sec",(object)f.SectionID??DBNull.Value),
                P("@g",(object)f.SafeGender??DBNull.Value), P("@s",f.Search??""), P("@st",(object)NullIfEmpty(f.Status)??DBNull.Value)
            };
        }

        private DataTable Students(ReportFilter f, string statusMode)
        {
            string sql = "SELECT st.StudentCode AS [Code], st.AdmissionNo AS [Admission No], st.FullName AS [Name], ISNULL(st.Gender,'') AS [Gender], ISNULL(c.ClassName,'') AS [Class], ISNULL(sec.SectionName,'') AS [Section], ISNULL(st.Status,'Active') AS [Status]"
                + StudentJoins() + StudentWhere(f, statusMode) + " ORDER BY st.FullName";
            return ExecuteDataTable(sql, StudentParams(f));
        }

        private DataTable CountBy(ReportFilter f, string groupExpr, string label)
        {
            string sql = "SELECT " + groupExpr + " AS [" + label + "], COUNT(*) AS [Students]" + StudentJoins()
                + StudentWhere(f, "Active") + " GROUP BY " + groupExpr + " ORDER BY [Students] DESC";
            return ExecuteDataTable(sql, StudentParams(f));
        }

        private DataTable StudentProfile(ReportFilter f)
        {
            if (!f.StudentID.HasValue) return null;
            const string sql = @"
SELECT st.StudentCode AS [Student Code], st.AdmissionNo AS [Admission No], st.FullName AS [Full Name],
       ISNULL(st.Gender,'') AS [Gender], st.DateOfBirth AS [Date of Birth], ISNULL(y.YearName,'') AS [Academic Year],
       ISNULL(c.ClassName,'') AS [Class], ISNULL(sec.SectionName,'') AS [Section], st.EnrollmentDate AS [Enrollment Date],
       ISNULL(st.Status,'Active') AS [Status], ISNULL(g.FullName,'') AS [Guardian], ISNULL(g.Phone,'') AS [Guardian Phone]
FROM Students st
LEFT JOIN Sections sec ON sec.SectionID=st.SectionID LEFT JOIN Classes c ON c.ClassID=sec.ClassID
LEFT JOIN AcademicYears y ON y.AcademicYearID=st.AcademicYearID LEFT JOIN Guardians g ON g.GuardianID=st.GuardianID
WHERE st.StudentID=@id";
            DataTable row = ExecuteDataTable(sql, new[] { P("@id", f.StudentID.Value) });
            // pivot single row into label/value pairs for a clean profile view
            DataTable outT = new DataTable(); outT.Columns.Add("Field"); outT.Columns.Add("Value");
            if (row.Rows.Count > 0)
                foreach (DataColumn col in row.Columns)
                    outT.Rows.Add(col.ColumnName, row.Rows[0][col] == DBNull.Value ? "" : row.Rows[0][col].ToString());
            return outT;
        }

        private DataTable Staff(ReportFilter f, string teachMode, string statusMode)
        {
            string teach = "";
            if (teachMode == "teacher") teach = " AND (EXISTS (SELECT 1 FROM ClassSubjectTeachers cst WHERE cst.StaffID=sf.StaffID) OR EXISTS (SELECT 1 FROM Sections s2 WHERE s2.StaffID=sf.StaffID))";
            else if (teachMode == "nonteacher") teach = " AND NOT EXISTS (SELECT 1 FROM ClassSubjectTeachers cst WHERE cst.StaffID=sf.StaffID) AND NOT EXISTS (SELECT 1 FROM Sections s2 WHERE s2.StaffID=sf.StaffID)";
            string st = "";
            if (statusMode == "Active") st = " AND ISNULL(sf.Status,'Active')='Active'";
            else if (statusMode == "!Active") st = " AND ISNULL(sf.Status,'Active')<>'Active'";
            string sql = "SELECT sf.EmployeeID AS [Employee], ISNULL(u.FullName,'') AS [Name], ISNULL(sf.Department,'') AS [Department], ISNULL(sf.Position,'') AS [Position], ISNULL(u.Role,'') AS [Role], ISNULL(sf.Status,'Active') AS [Status] "
                + "FROM Staff sf LEFT JOIN Users u ON u.UserID=sf.UserID WHERE 1=1" + teach + st
                + (string.IsNullOrEmpty(f.Department) ? "" : " AND sf.Department=@dept")
                + (string.IsNullOrEmpty(f.Search) ? "" : " AND (u.FullName LIKE '%'+@s+'%' OR sf.EmployeeID LIKE '%'+@s+'%')")
                + " ORDER BY [Name]";
            return ExecuteDataTable(sql, new[] { P("@dept", (object)NullIfEmpty(f.Department) ?? DBNull.Value), P("@s", f.Search ?? "") });
        }

        private DataTable TeacherWorkload(ReportFilter f)
        {
            const string sql = @"
SELECT ISNULL(u.FullName,sf.EmployeeID) AS [Teacher],
  (SELECT COUNT(DISTINCT sec.ClassID) FROM ClassSubjectTeachers cst JOIN Sections sec ON sec.SectionID=cst.SectionID WHERE cst.StaffID=sf.StaffID AND ISNULL(cst.IsActive,1)=1) AS [Classes],
  (SELECT COUNT(DISTINCT cst.SectionID) FROM ClassSubjectTeachers cst WHERE cst.StaffID=sf.StaffID AND ISNULL(cst.IsActive,1)=1) AS [Sections],
  (SELECT COUNT(DISTINCT cst.SubjectID) FROM ClassSubjectTeachers cst WHERE cst.StaffID=sf.StaffID AND ISNULL(cst.IsActive,1)=1) AS [Subjects],
  ISNULL((SELECT SUM(ISNULL(cst.WeeklyPeriods,0)) FROM ClassSubjectTeachers cst WHERE cst.StaffID=sf.StaffID AND ISNULL(cst.IsActive,1)=1),0) AS [Weekly Periods],
  (SELECT COUNT(*) FROM Sections s2 WHERE s2.StaffID=sf.StaffID) AS [Class-Teacher Of]
FROM Staff sf LEFT JOIN Users u ON u.UserID=sf.UserID
WHERE (@staff IS NULL OR sf.StaffID=@staff)
  AND (EXISTS (SELECT 1 FROM ClassSubjectTeachers cst WHERE cst.StaffID=sf.StaffID) OR EXISTS (SELECT 1 FROM Sections s2 WHERE s2.StaffID=sf.StaffID))
ORDER BY [Teacher]";
            return ExecuteDataTable(sql, new[] { P("@staff", (object)f.StaffID ?? DBNull.Value) });
        }

        private DataTable InvigilatorAssignments()
        {
            if (ExecuteScalar("SELECT OBJECT_ID('dbo.ExamSchedules','U')", null) == DBNull.Value) return null;
            return ExecuteDataTable("SELECT ISNULL(u.FullName,'—') AS [Invigilator], e.ExamName AS [Exam], sub.SubjectName AS [Subject], es.ExamDate AS [Date] FROM ExamSchedules es JOIN Exams e ON e.ExamID=es.ExamID LEFT JOIN Subjects sub ON sub.SubjectID=es.SubjectID LEFT JOIN Staff sf ON sf.StaffID=es.InvigilatorStaffID LEFT JOIN Users u ON u.UserID=sf.UserID WHERE es.InvigilatorStaffID IS NOT NULL ORDER BY es.ExamDate DESC", null);
        }

        private DataTable Timetable(ReportFilter f, string mode)
        {
            string w = " WHERE ISNULL(tt.IsActive,1)=1";
            if (f.YearID.HasValue) w += " AND tt.AcademicYearID=@y";
            if (f.TermID.HasValue) w += " AND tt.TermID=@tm";
            if (mode == "class" && f.SectionID.HasValue) w += " AND tt.SectionID=@sec";
            if (mode == "teacher" && f.StaffID.HasValue) w += " AND tt.StaffID=@staff";
            if (mode == "subject" && f.SubjectID.HasValue) w += " AND tt.SubjectID=@subj";
            string sql = "SELECT tt.DayOfWeek AS [Day], tt.PeriodNo AS [Period], tt.StartTime AS [Start], tt.EndTime AS [End], c.ClassName AS [Class], sec.SectionName AS [Section], sub.SubjectName AS [Subject], ISNULL(u.FullName,'') AS [Teacher], ISNULL(tt.RoomNumber,'') AS [Room] "
                + "FROM Timetable tt JOIN Sections sec ON sec.SectionID=tt.SectionID JOIN Classes c ON c.ClassID=sec.ClassID JOIN Subjects sub ON sub.SubjectID=tt.SubjectID LEFT JOIN Staff sf ON sf.StaffID=tt.StaffID LEFT JOIN Users u ON u.UserID=sf.UserID"
                + w + " ORDER BY tt.DayOfWeek, tt.PeriodNo";
            return ExecuteDataTable(sql, new[] { P("@y", (object)f.YearID ?? DBNull.Value), P("@tm", (object)f.TermID ?? DBNull.Value), P("@sec", (object)f.SectionID ?? DBNull.Value), P("@staff", (object)f.StaffID ?? DBNull.Value), P("@subj", (object)f.SubjectID ?? DBNull.Value) });
        }

        // ---- param helpers ----
        private static string DateRange(ReportFilter f, string col)
        {
            string w = "";
            if (f.From.HasValue) w += " AND " + col + ">=@from";
            if (f.To.HasValue) w += " AND " + col + "<=@to";
            return w;
        }
        private SqlParameter[] YP(ReportFilter f) { return new[] { P("@y", (object)f.YearID ?? DBNull.Value) }; }
        private SqlParameter[] YCP(ReportFilter f) { return new[] { P("@y", (object)f.YearID ?? DBNull.Value), P("@c", (object)f.ClassID ?? DBNull.Value) }; }
        private SqlParameter[] SearchP(ReportFilter f) { return new[] { P("@s", f.Search ?? "" ) }; }
        private SqlParameter[] DateParams(ReportFilter f) { return new[] { P("@from", (object)f.From ?? DBNull.Value), P("@to", (object)f.To ?? DBNull.Value) }; }
        private SqlParameter[] AcademicParams(ReportFilter f)
        {
            return new[]
            {
                P("@y",(object)f.YearID??DBNull.Value), P("@c",(object)f.ClassID??DBNull.Value), P("@sec",(object)f.SectionID??DBNull.Value),
                P("@subj",(object)f.SubjectID??DBNull.Value), P("@staff",(object)f.StaffID??DBNull.Value)
            };
        }
        private static object NullIfEmpty(string s) { return string.IsNullOrWhiteSpace(s) ? null : (object)s; }
        private static DataTable Denied() { DataTable t = new DataTable(); t.Columns.Add("Notice"); t.Rows.Add("You are not authorized to view this report."); return t; }

        // ---- filter option lookups (for the viewer's dropdowns) ----
        public DataTable GetClasses(int? yearId) { return ExecuteDataTable("SELECT ClassID, ClassName FROM Classes WHERE (@y IS NULL OR AcademicYearID=@y) AND ISNULL(Status,'Active')='Active' ORDER BY ClassName", new[] { P("@y", (object)yearId ?? DBNull.Value) }); }
        public DataTable GetSections(int? classId) { return ExecuteDataTable("SELECT SectionID, SectionName FROM Sections WHERE (@c IS NULL OR ClassID=@c) AND ISNULL(Status,'Active')='Active' ORDER BY SectionName", new[] { P("@c", (object)classId ?? DBNull.Value) }); }
        public DataTable GetTerms(int? yearId) { return ExecuteDataTable("SELECT TermID, TermName FROM Terms WHERE (@y IS NULL OR AcademicYearID=@y) ORDER BY StartDate", new[] { P("@y", (object)yearId ?? DBNull.Value) }); }
        public DataTable GetSubjects() { return ExecuteDataTable("SELECT SubjectID, SubjectName FROM Subjects WHERE ISNULL(IsActive,1)=1 ORDER BY SubjectName", null); }
        public DataTable GetTeachers() { return ExecuteDataTable("SELECT sf.StaffID, ISNULL(u.FullName, sf.EmployeeID) AS Name FROM Staff sf LEFT JOIN Users u ON u.UserID=sf.UserID ORDER BY Name", null); }
        public DataTable GetDepartments() { return ExecuteDataTable("SELECT DISTINCT Department FROM Staff WHERE ISNULL(Department,'')<>'' ORDER BY Department", null); }
        public DataTable GetStudents(int? sectionId) { return ExecuteDataTable("SELECT StudentID, FullName + ' (' + StudentCode + ')' AS Name FROM Students WHERE (@sec IS NULL OR SectionID=@sec) ORDER BY FullName", new[] { P("@sec", (object)sectionId ?? DBNull.Value) }); }
        public DataTable GetExamsLookup(int? yearId) { return ExecuteDataTable("SELECT ExamID, ExamName FROM Exams WHERE (@y IS NULL OR AcademicYearID=@y) ORDER BY StartDate DESC", new[] { P("@y", (object)yearId ?? DBNull.Value) }); }
        public DataTable GetPayrollPeriods() { return ExecuteDataTable("SELECT PayrollPeriodID, PeriodName FROM PayrollPeriods ORDER BY StartDate DESC", null); }
    }
}
