using AQOONHUB_SMS.App_Code.Models;
using AQOONHUB_SMS.App_Code.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace AQOONHUB_SMS.App_Code.DataAccess
{
    public class StudentDAL
    {
        private DatabaseHelper db;

        public StudentDAL()
        {
            db = new DatabaseHelper();
        }

        public List<Student> GetAllStudents(string status, string search)
        {
            List<Student> students = new List<Student>();

            string query = @"
                SELECT s.*, g.FullName as GuardianName, c.ClassName, sec.SectionName,
                       ISNULL(i.TotalAmount, 0) as BilledAmount,
                       ISNULL(i.PaidAmount, 0) as PaidAmount,
                       ISNULL(i.TotalAmount, 0) - ISNULL(i.PaidAmount, 0) as Balance
                FROM Students s
                INNER JOIN Guardians g ON s.GuardianID = g.GuardianID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                LEFT JOIN (
                    SELECT StudentID, SUM(TotalAmount) as TotalAmount, SUM(PaidAmount) as PaidAmount
                    FROM Invoices WHERE Status != 'Void'
                    GROUP BY StudentID
                ) i ON s.StudentID = i.StudentID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(status))
            {
                query += " AND s.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query += @" AND (s.FirstName LIKE @Search OR s.LastName LIKE @Search 
                          OR s.StudentCode LIKE @Search OR s.AdmissionNo LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", "%" + search + "%"));
            }

            query += " ORDER BY s.CreatedAt DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                students.Add(MapToStudent(row));
            }

            return students;
        }

        public List<Student> GetAllStudents()
        {
            return GetAllStudents(null, null);
        }

        public Student GetStudentById(int studentId)
        {
            string query = @"
                SELECT s.*, g.FullName as GuardianName, c.ClassName, sec.SectionName
                FROM Students s
                INNER JOIN Guardians g ON s.GuardianID = g.GuardianID
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE s.StudentID = @StudentID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StudentID", studentId)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
                return MapToStudent(dt.Rows[0]);

            return null;
        }

        public int AddStudent(Student student)
        {
            string query = @"
                INSERT INTO Students (StudentCode, AdmissionNo, FirstName, LastName, 
                    Gender, DateOfBirth, GuardianID, SectionID, AcademicYearID,
                    Status, MedicalNotes, Address)
                VALUES (@StudentCode, @AdmissionNo, @FirstName, @LastName,
                    @Gender, @DateOfBirth, @GuardianID, @SectionID, @AcademicYearID,
                    @Status, @MedicalNotes, @Address);
                SELECT SCOPE_IDENTITY();";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StudentCode", student.StudentCode),
                new SqlParameter("@AdmissionNo", student.AdmissionNo),
                new SqlParameter("@FirstName", student.FirstName),
                new SqlParameter("@LastName", student.LastName),
                new SqlParameter("@Gender", student.Gender),
                new SqlParameter("@DateOfBirth", student.DateOfBirth),
                new SqlParameter("@GuardianID", student.GuardianID),
                new SqlParameter("@SectionID", student.SectionID),
                new SqlParameter("@AcademicYearID", student.AcademicYearID),
                new SqlParameter("@Status", student.Status),
                new SqlParameter("@MedicalNotes", string.IsNullOrEmpty(student.MedicalNotes) ? (object)DBNull.Value : student.MedicalNotes),
                new SqlParameter("@Address", string.IsNullOrEmpty(student.Address) ? (object)DBNull.Value : student.Address)
            };

            return Convert.ToInt32(db.ExecuteScalar(query, parameters));
        }

        public string GenerateStudentCode()
        {
            string query = @"
                SELECT TOP 1 StudentCode 
                FROM Students 
                WHERE StudentCode LIKE 'AQH-' + CAST(YEAR(GETDATE()) AS VARCHAR) + '-%'
                ORDER BY StudentCode DESC";

            DataTable dt = db.ExecuteQuery(query);

            int nextNumber = 1;
            if (dt.Rows.Count > 0)
            {
                string lastCode = dt.Rows[0]["StudentCode"].ToString();
                string lastNum = lastCode.Substring(lastCode.LastIndexOf('-') + 1);
                nextNumber = int.Parse(lastNum) + 1;
            }

            return "AQH-" + DateTime.Now.Year.ToString() + "-" + nextNumber.ToString("D4");
        }

        public bool UpdateStudent(Student student)
        {
            string query = @"
                UPDATE Students SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Gender = @Gender,
                    DateOfBirth = @DateOfBirth,
                    GuardianID = @GuardianID,
                    SectionID = @SectionID,
                    Status = @Status,
                    MedicalNotes = @MedicalNotes,
                    Address = @Address,
                    UpdatedAt = GETDATE()
                WHERE StudentID = @StudentID";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StudentID", student.StudentID),
                new SqlParameter("@FirstName", student.FirstName),
                new SqlParameter("@LastName", student.LastName),
                new SqlParameter("@Gender", student.Gender),
                new SqlParameter("@DateOfBirth", student.DateOfBirth),
                new SqlParameter("@GuardianID", student.GuardianID),
                new SqlParameter("@SectionID", student.SectionID),
                new SqlParameter("@Status", student.Status),
                new SqlParameter("@MedicalNotes", string.IsNullOrEmpty(student.MedicalNotes) ? (object)DBNull.Value : student.MedicalNotes),
                new SqlParameter("@Address", string.IsNullOrEmpty(student.Address) ? (object)DBNull.Value : student.Address)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public bool SoftDeleteStudent(int studentId)
        {
            string query = "UPDATE Students SET Status = 'Deleted', UpdatedAt = GETDATE() WHERE StudentID = @StudentID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@StudentID", studentId)
            };

            return db.ExecuteNonQuery(query, parameters) > 0;
        }

        public DataTable GetEnrollmentByClass()
        {
            string query = @"
                SELECT c.ClassName, COUNT(*) as EnrollmentCount
                FROM Students s
                INNER JOIN Sections sec ON s.SectionID = sec.SectionID
                INNER JOIN Classes c ON sec.ClassID = c.ClassID
                WHERE s.Status = 'Active'
                GROUP BY c.ClassName
                ORDER BY c.ClassName";

            return db.ExecuteQuery(query);
        }

        private Student MapToStudent(DataRow row)
        {
            Student student = new Student();
            student.StudentID = Convert.ToInt32(row["StudentID"]);
            student.StudentCode = row["StudentCode"].ToString();
            student.AdmissionNo = row["AdmissionNo"].ToString();
            student.FirstName = row["FirstName"].ToString();
            student.LastName = row["LastName"].ToString();
            student.Gender = row["Gender"].ToString();
            student.DateOfBirth = Convert.ToDateTime(row["DateOfBirth"]);
            student.GuardianID = Convert.ToInt32(row["GuardianID"]);
            student.GuardianName = row["GuardianName"].ToString();
            student.SectionID = Convert.ToInt32(row["SectionID"]);
            student.ClassName = row["ClassName"].ToString();
            student.SectionName = row["SectionName"].ToString();
            student.Status = row["Status"].ToString();
            student.PhotoPath = row["PhotoPath"] == DBNull.Value ? null : row["PhotoPath"].ToString();
            student.MedicalNotes = row["MedicalNotes"] == DBNull.Value ? null : row["MedicalNotes"].ToString();
            student.Address = row["Address"] == DBNull.Value ? null : row["Address"].ToString();
            student.EnrollmentDate = Convert.ToDateTime(row["EnrollmentDate"]);
            student.BilledAmount = row["BilledAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["BilledAmount"]);
            student.PaidAmount = row["PaidAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["PaidAmount"]);
            return student;
        }
    }
}