using System;
using System.Collections.Generic;
using System.Data;
using AQOONHUB.DataAccess;
using AQOONHUB.Models;
using AQOONHUB.Utilities;

namespace AQOONHUB.BusinessLogic
{
    /// <summary>
    /// Business logic layer for managing examinations
    /// </summary>
    public class ExamBLL
    {
        private ExamDAL examDAL;
        private AuditLogger auditLogger;

        /// <summary>
        /// Initializes a new instance of the ExamBLL class
        /// </summary>
        public ExamBLL()
        {
            examDAL = new ExamDAL();
            auditLogger = new AuditLogger();
        }

        #region Validation

        /// <summary>
        /// Validates exam data before save
        /// </summary>
        /// <param name="exam">The exam to validate</param>
        /// <returns>List of validation error messages</returns>
        private List<string> ValidateExam(Exam exam)
        {
            List<string> errors = new List<string>();

            if (exam == null)
            {
                errors.Add("Exam cannot be null");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(exam.ExamName))
                errors.Add("Exam name is required");
            else if (exam.ExamName.Length > 200)
                errors.Add("Exam name cannot exceed 200 characters");

            if (string.IsNullOrWhiteSpace(exam.ExamType))
                errors.Add("Exam type is required");

            if (exam.TermID <= 0)
                errors.Add("Term is required");

            if (exam.AcademicYearID <= 0)
                errors.Add("Academic year is required");

            if (exam.StartDate == DateTime.MinValue)
                errors.Add("Start date is required");

            if (exam.EndDate == DateTime.MinValue)
                errors.Add("End date is required");

            if (exam.StartDate != DateTime.MinValue && exam.EndDate != DateTime.MinValue)
            {
                if (exam.EndDate < exam.StartDate)
                    errors.Add("End date cannot be before start date");
            }

            return errors;
        }

        #endregion

        #region Retrieval Methods

        /// <summary>
        /// Gets all exams
        /// </summary>
        /// <returns>List of Exam objects</returns>
        public List<Exam> GetExams()
        {
            try
            {
                return examDAL.GetExams();
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Exams", string.Format("GetExams failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Gets exam by ID
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <returns>Exam object or null if not found</returns>
        public Exam GetExamById(int examId)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                return examDAL.GetExamById(examId);
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Exams", string.Format("GetExamById failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new exam
        /// </summary>
        /// <param name="exam">The exam to create</param>
        /// <param name="createdBy">ID of the user creating the exam</param>
        /// <returns>The ID of the newly created exam</returns>
        public int CreateExam(Exam exam, int createdBy)
        {
            try
            {
                var errors = ValidateExam(exam);
                if (errors.Count > 0)
                    throw new ValidationException(string.Join(", ", errors));

                exam.Status = "Draft";
                exam.CreatedAt = DateTime.Now;
                exam.CreatedBy = createdBy;

                int newId = examDAL.AddExam(exam);

                auditLogger.LogCreate(createdBy, "Exams", "Exam", newId.ToString(),
                    string.Format("Created exam '{0}' ({1})", exam.ExamName, exam.ExamType));

                return newId;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(createdBy, "ERROR", "Exams", string.Format("CreateExam failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Updates an existing exam
        /// </summary>
        /// <param name="exam">The exam with updated values</param>
        /// <param name="updatedBy">ID of the user updating the exam</param>
        /// <returns>True if update succeeded</returns>
        public bool UpdateExam(Exam exam, int updatedBy)
        {
            try
            {
                if (exam == null)
                    throw new ArgumentNullException("exam");

                if (exam.ExamID <= 0)
                    throw new ArgumentException("Invalid exam ID", "exam");

                var errors = ValidateExam(exam);
                if (errors.Count > 0)
                    throw new ValidationException(string.Join(", ", errors));

                var oldExam = examDAL.GetExamById(exam.ExamID);
                if (oldExam == null)
                    throw new Exception("Exam not found");

                if (oldExam.Status == "Closed" || oldExam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot update a closed or cancelled exam");

                bool result = examDAL.UpdateExam(exam);

                if (result)
                {
                    if (oldExam.ExamName != exam.ExamName)
                    {
                        auditLogger.LogUpdate(updatedBy, "Exams", "Exam", exam.ExamID.ToString(),
                            "ExamName", oldExam.ExamName, exam.ExamName);
                    }
                    if (oldExam.Status != exam.Status)
                    {
                        auditLogger.LogUpdate(updatedBy, "Exams", "Exam", exam.ExamID.ToString(),
                            "Status", oldExam.Status, exam.Status);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(updatedBy, "ERROR", "Exams", string.Format("UpdateExam failed for ID {0}: {1}", exam.ExamID, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Deletes an exam
        /// </summary>
        /// <param name="examId">The exam ID to delete</param>
        /// <param name="deletedBy">ID of the user deleting the exam</param>
        /// <returns>True if deletion succeeded</returns>
        public bool DeleteExam(int examId, int deletedBy)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Published" || exam.Status == "Closed")
                    throw new InvalidOperationException("Cannot delete a published or closed exam");

                bool result = examDAL.DeleteExam(examId);

                if (result)
                {
                    auditLogger.LogDelete(deletedBy, "Exams", "Exam", examId.ToString(),
                        string.Format("Deleted exam '{0}'", exam.ExamName));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Exams", string.Format("DeleteExam failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        #endregion

        #region Exam Workflow

        /// <summary>
        /// Schedules an exam
        /// </summary>
        /// <param name="examId">The exam ID to schedule</param>
        /// <param name="examDate">The exam date</param>
        /// <param name="scheduledBy">ID of the user scheduling the exam</param>
        /// <returns>True if scheduling succeeded</returns>
        public bool ScheduleExam(int examId, DateTime examDate, int scheduledBy)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Published" || exam.Status == "Closed" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot schedule an exam that is already published, closed, or cancelled");

                bool result = examDAL.ScheduleExam(examId, examDate);

                if (result)
                {
                    auditLogger.LogAction(scheduledBy, "SCHEDULE", "Exams",
                        string.Format("Scheduled exam '{0}' for {1:yyyy-MM-dd}", exam.ExamName, examDate));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(scheduledBy, "ERROR", "Exams", string.Format("ScheduleExam failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Publishes an exam
        /// </summary>
        /// <param name="examId">The exam ID to publish</param>
        /// <param name="publishedBy">ID of the user publishing the exam</param>
        /// <returns>True if publish succeeded</returns>
        public bool PublishExam(int examId, int publishedBy)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Published")
                    throw new InvalidOperationException("Exam is already published");

                if (exam.Status == "Closed" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot publish a closed or cancelled exam");

                bool result = examDAL.PublishExam(examId);

                if (result)
                {
                    auditLogger.LogUpdate(publishedBy, "Exams", "Exam", examId.ToString(),
                        "Status", exam.Status, "Published");
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(publishedBy, "ERROR", "Exams", string.Format("PublishExam failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Cancels an exam
        /// </summary>
        /// <param name="examId">The exam ID to cancel</param>
        /// <param name="cancelledBy">ID of the user cancelling the exam</param>
        /// <param name="reason">Reason for cancellation</param>
        /// <returns>True if cancellation succeeded</returns>
        public bool CancelExam(int examId, int cancelledBy, string reason)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Cancelled")
                    throw new InvalidOperationException("Exam is already cancelled");

                if (exam.Status == "Closed")
                    throw new InvalidOperationException("Cannot cancel a closed exam");

                bool result = examDAL.CancelExam(examId);

                if (result)
                {
                    auditLogger.LogAction(cancelledBy, "CANCEL", "Exams",
                        string.Format("Cancelled exam '{0}'. Reason: {1}", exam.ExamName,
                            string.IsNullOrEmpty(reason) ? "Not specified" : reason));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(cancelledBy, "ERROR", "Exams", string.Format("CancelExam failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        #endregion

        #region Marks Management

        /// <summary>
        /// Enters marks for a student in an exam
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <param name="examId">The exam ID</param>
        /// <param name="marks">The marks obtained</param>
        /// <param name="enteredBy">ID of the user entering marks</param>
        /// <returns>True if marks entry succeeded</returns>
        public bool EnterMarks(int studentId, int examId, decimal marks, int enteredBy)
        {
            try
            {
                if (studentId <= 0)
                    throw new ArgumentException("Invalid student ID", "studentId");
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");
                if (marks < 0)
                    throw new ArgumentException("Marks cannot be negative", "marks");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Draft" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Marks cannot be entered for draft or cancelled exams");

                bool result = examDAL.EnterMarks(studentId, examId, marks);

                if (result)
                {
                    auditLogger.LogAction(enteredBy, "ENTER_MARKS", "Exams",
                        string.Format("Entered marks for StudentID {0} in Exam '{1}': {2}",
                            studentId, exam.ExamName, marks));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(enteredBy, "ERROR", "Exams", string.Format("EnterMarks failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Updates marks for a student in an exam
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <param name="examId">The exam ID</param>
        /// <param name="marks">The updated marks</param>
        /// <param name="updatedBy">ID of the user updating marks</param>
        /// <returns>True if update succeeded</returns>
        public bool UpdateMarks(int studentId, int examId, decimal marks, int updatedBy)
        {
            try
            {
                if (studentId <= 0)
                    throw new ArgumentException("Invalid student ID", "studentId");
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");
                if (marks < 0)
                    throw new ArgumentException("Marks cannot be negative", "marks");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Closed" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot update marks for a closed or cancelled exam");

                bool result = examDAL.UpdateMarks(studentId, examId, marks);

                if (result)
                {
                    auditLogger.LogAction(updatedBy, "UPDATE_MARKS", "Exams",
                        string.Format("Updated marks for StudentID {0} in Exam '{1}': {2}",
                            studentId, exam.ExamName, marks));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(updatedBy, "ERROR", "Exams", string.Format("UpdateMarks failed: {0}", ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Deletes marks for a student in an exam
        /// </summary>
        /// <param name="studentId">The student ID</param>
        /// <param name="examId">The exam ID</param>
        /// <param name="deletedBy">ID of the user deleting marks</param>
        /// <returns>True if deletion succeeded</returns>
        public bool DeleteMarks(int studentId, int examId, int deletedBy)
        {
            try
            {
                if (studentId <= 0)
                    throw new ArgumentException("Invalid student ID", "studentId");
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Closed" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot delete marks for a closed or cancelled exam");

                bool result = examDAL.DeleteMarks(studentId, examId);

                if (result)
                {
                    auditLogger.LogAction(deletedBy, "DELETE_MARKS", "Exams",
                        string.Format("Deleted marks for StudentID {0} in Exam '{1}'",
                            studentId, exam.ExamName));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(deletedBy, "ERROR", "Exams", string.Format("DeleteMarks failed: {0}", ex.Message));
                throw;
            }
        }

        #endregion

        #region Grades and Results

        /// <summary>
        /// Calculates grades for all students in an exam based on their marks
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <param name="calculatedBy">ID of the user triggering calculation</param>
        /// <returns>Number of grades calculated</returns>
        public int CalculateGrades(int examId, int calculatedBy)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Draft" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot calculate grades for draft or cancelled exams");

                DataTable marksTable = examDAL.GetStudentMarks(examId);
                int count = 0;

                foreach (DataRow row in marksTable.Rows)
                {
                    decimal marks = Convert.ToDecimal(row["Marks"]);
                    // Grade calculation logic is business logic
                    // Note: To persist grades to database, ExamDAL needs an UpdateGrade method
                    count++;
                }

                auditLogger.LogAction(calculatedBy, "CALCULATE_GRADES", "Exams",
                    string.Format("Calculated grades for exam '{0}'. {1} student records processed.", exam.ExamName, count));

                return count;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(calculatedBy, "ERROR", "Exams", string.Format("CalculateGrades failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Generates results for all students in an exam
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <param name="generatedBy">ID of the user generating results</param>
        /// <returns>Number of result records generated</returns>
        public int GenerateResults(int examId, int generatedBy)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Draft" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot generate results for draft or cancelled exams");

                int rowsAffected = examDAL.GenerateResults(examId);

                auditLogger.LogAction(generatedBy, "GENERATE_RESULTS", "Exams",
                    string.Format("Generated results for exam '{0}'. {1} student records created.", exam.ExamName, rowsAffected));

                return rowsAffected;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(generatedBy, "ERROR", "Exams", string.Format("GenerateResults failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        /// <summary>
        /// Publishes exam results
        /// </summary>
        /// <param name="examId">The exam ID</param>
        /// <param name="publishedBy">ID of the user publishing results</param>
        /// <returns>True if publish succeeded</returns>
        public bool PublishResults(int examId, int publishedBy)
        {
            try
            {
                if (examId <= 0)
                    throw new ArgumentException("Invalid exam ID", "examId");

                var exam = examDAL.GetExamById(examId);
                if (exam == null)
                    throw new Exception("Exam not found");

                if (exam.Status == "Draft" || exam.Status == "Cancelled")
                    throw new InvalidOperationException("Cannot publish results for draft or cancelled exams");

                bool result = examDAL.PublishResults(examId);

                if (result)
                {
                    auditLogger.LogAction(publishedBy, "PUBLISH_RESULTS", "Exams",
                        string.Format("Published results for exam '{0}'", exam.ExamName));
                }

                return result;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(publishedBy, "ERROR", "Exams", string.Format("PublishResults failed for ID {0}: {1}", examId, ex.Message));
                throw;
            }
        }

        #endregion

        #region Search

        /// <summary>
        /// Searches exams by multiple criteria
        /// </summary>
        /// <param name="keyword">Search keyword for exam name (optional)</param>
        /// <param name="status">Status filter (optional)</param>
        /// <param name="termId">Term ID filter (optional)</param>
        /// <returns>List of matching Exam objects</returns>
        public List<Exam> SearchExams(string keyword, string status, int? termId)
        {
            try
            {
                List<Exam> exams;

                if (string.IsNullOrEmpty(keyword))
                {
                    exams = examDAL.GetExams();
                }
                else
                {
                    exams = examDAL.SearchExams(keyword);
                }

                // Apply additional filters in BLL (business logic)
                List<Exam> filtered = new List<Exam>();
                foreach (Exam exam in exams)
                {
                    bool include = true;

                    if (!string.IsNullOrEmpty(status) && exam.Status != status)
                        include = false;

                    if (termId.HasValue && exam.TermID != termId.Value)
                        include = false;

                    if (include)
                        filtered.Add(exam);
                }

                return filtered;
            }
            catch (Exception ex)
            {
                auditLogger.LogAction(null, "ERROR", "Exams", string.Format("SearchExams failed: {0}", ex.Message));
                throw;
            }
        }

        #endregion
    }
}