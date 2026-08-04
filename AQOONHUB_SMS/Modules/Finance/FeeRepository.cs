using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Finance
{
    public sealed class FeeRepository
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["AQOONHUB_DB"].ConnectionString;

        private DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private int Execute(string sql, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public DataTable GetAcademicYears()
        {
            return Query("SELECT AcademicYearID, YearName FROM AcademicYears ORDER BY IsCurrent DESC, StartDate DESC");
        }

        public DataTable GetClasses()
        {
            return Query("SELECT ClassID, ClassName FROM Classes WHERE Status = 'Active' ORDER BY ClassName");
        }

        public DataTable GetSections(int classId)
        {
            return Query("SELECT SectionID, SectionName FROM Sections WHERE ClassID=@ClassID AND Status='Active' ORDER BY SectionName",
                new SqlParameter("@ClassID", classId));
        }

        public DataTable GetStudents()
        {
            return Query(@"SELECT s.StudentID, s.StudentCode,
                    LTRIM(RTRIM(s.FirstName + ' ' + ISNULL(s.LastName,''))) AS StudentName,
                    s.SectionID, sec.SectionName, sec.ClassID, c.ClassName, s.AcademicYearID
                FROM Students s
                INNER JOIN Sections sec ON sec.SectionID=s.SectionID
                INNER JOIN Classes c ON c.ClassID=sec.ClassID
                WHERE s.Status='Active'
                ORDER BY s.FirstName,s.LastName");
        }

        public DataTable GetFeeCategories(bool activeOnly)
        {
            return Query(@"SELECT fc.*,
                    CASE WHEN fc.IsActive=1 THEN 'Active' ELSE 'Inactive' END AS StatusText
                FROM FeeCategories fc
                WHERE (@ActiveOnly=0 OR fc.IsActive=1)
                ORDER BY fc.CategoryName", new SqlParameter("@ActiveOnly", activeOnly));
        }

        public void SaveFeeCategory(int? id, string name, string code, string description, string term, bool active)
        {
            const string sql = @"
                IF @ID IS NULL
                    INSERT FeeCategories(CategoryName,CategoryCode,[Description],DefaultBillingTerm,IsActive)
                    VALUES(@Name,@Code,@Description,@Term,@Active)
                ELSE
                    UPDATE FeeCategories SET CategoryName=@Name,CategoryCode=@Code,[Description]=@Description,
                        DefaultBillingTerm=@Term,IsActive=@Active,UpdatedAt=SYSUTCDATETIME()
                    WHERE FeeCategoryID=@ID";
            Execute(sql,
                new SqlParameter("@ID", (object)id ?? DBNull.Value),
                new SqlParameter("@Name", name),
                new SqlParameter("@Code", code),
                new SqlParameter("@Description", (object)description ?? DBNull.Value),
                new SqlParameter("@Term", term),
                new SqlParameter("@Active", active));
        }

        public bool DeleteFeeCategory(int id)
        {
            return Execute(@"DELETE FeeCategories
                WHERE FeeCategoryID=@ID
                AND NOT EXISTS(SELECT 1 FROM ClassFeeStructures WHERE FeeCategoryID=@ID)
                AND NOT EXISTS(SELECT 1 FROM FeeInvoiceItems WHERE FeeCategoryID=@ID)",
                new SqlParameter("@ID", id)) > 0;
        }

        public DataTable GetFeeStructures()
        {
            return Query(@"SELECT fs.ClassFeeStructureID,ay.YearName,c.ClassName,
                    ISNULL(sec.SectionName,'All Sections') SectionName,fc.CategoryName,
                    fs.BillingTerm,fs.Amount,fs.DiscountType,fs.DiscountAmount,fs.[Description],
                    CASE WHEN fs.IsActive=1 THEN 'Active' ELSE 'Inactive' END StatusText
                FROM ClassFeeStructures fs
                INNER JOIN AcademicYears ay ON ay.AcademicYearID=fs.AcademicYearID
                INNER JOIN Classes c ON c.ClassID=fs.ClassID
                LEFT JOIN Sections sec ON sec.SectionID=fs.SectionID
                INNER JOIN FeeCategories fc ON fc.FeeCategoryID=fs.FeeCategoryID
                ORDER BY ay.StartDate DESC,c.ClassName,sec.SectionName,fc.CategoryName");
        }

        public void SaveFeeStructure(int academicYearId, int classId, int? sectionId, int categoryId,
            string term, decimal amount, string discountType, decimal discount, string description, bool active)
        {
            Execute(@"IF EXISTS(SELECT 1 FROM ClassFeeStructures
                        WHERE AcademicYearID=@Year AND ClassID=@Class
                        AND ((SectionID=@Section) OR (SectionID IS NULL AND @Section IS NULL))
                        AND FeeCategoryID=@Category AND IsActive=1)
                    THROW 50001, 'An active fee structure already exists for this selection.', 1;
                INSERT ClassFeeStructures(AcademicYearID,ClassID,SectionID,FeeCategoryID,BillingTerm,
                    Amount,DiscountType,DiscountAmount,[Description],IsActive)
                VALUES(@Year,@Class,@Section,@Category,@Term,@Amount,@DiscountType,@Discount,@Description,@Active)",
                new SqlParameter("@Year", academicYearId),
                new SqlParameter("@Class", classId),
                new SqlParameter("@Section", (object)sectionId ?? DBNull.Value),
                new SqlParameter("@Category", categoryId),
                new SqlParameter("@Term", term),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@DiscountType", discountType),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@Description", (object)description ?? DBNull.Value),
                new SqlParameter("@Active", active));
        }

        public DataTable GetApplicableStructures(int studentId)
        {
            return Query(@"SELECT fs.ClassFeeStructureID,fc.FeeCategoryID,fc.CategoryName,
                    COALESCE(fs.[Description],fc.[Description]) [Description],fs.Amount,
                    CASE fs.DiscountType WHEN 'Percentage' THEN ROUND(fs.Amount*fs.DiscountAmount/100,2)
                        WHEN 'Fixed Amount' THEN fs.DiscountAmount ELSE 0 END DiscountAmount
                FROM Students s
                INNER JOIN Sections sec ON sec.SectionID=s.SectionID
                INNER JOIN ClassFeeStructures fs ON fs.ClassID=sec.ClassID
                    AND fs.AcademicYearID=s.AcademicYearID
                    AND (fs.SectionID=s.SectionID OR fs.SectionID IS NULL)
                INNER JOIN FeeCategories fc ON fc.FeeCategoryID=fs.FeeCategoryID
                WHERE s.StudentID=@Student AND fs.IsActive=1 AND fc.IsActive=1
                    AND (fs.SectionID=s.SectionID OR NOT EXISTS(
                        SELECT 1 FROM ClassFeeStructures specific
                        WHERE specific.AcademicYearID=fs.AcademicYearID AND specific.ClassID=fs.ClassID
                        AND specific.SectionID=s.SectionID AND specific.FeeCategoryID=fs.FeeCategoryID
                        AND specific.IsActive=1))
                ORDER BY fc.CategoryName", new SqlParameter("@Student", studentId));
        }

        public int CreateInvoice(int studentId, DateTime invoiceDate, DateTime dueDate, string type,
            decimal invoiceDiscount, string remarks, string instructions, int createdBy, DataTable items)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int academicYearId;
                        using (var student = new SqlCommand("SELECT AcademicYearID FROM Students WHERE StudentID=@ID", connection, transaction))
                        {
                            student.Parameters.AddWithValue("@ID", studentId);
                            academicYearId = Convert.ToInt32(student.ExecuteScalar());
                        }
                        string number;
                        using (var numberCommand = new SqlCommand(@"SELECT 'INV-'+CONVERT(char(4),YEAR(@Date))+'-'+
                            RIGHT('000000'+CONVERT(varchar(6),ISNULL(MAX(TRY_CONVERT(int,RIGHT(InvoiceNumber,6))),0)+1),6)
                            FROM FeeInvoices WITH(UPDLOCK,HOLDLOCK) WHERE InvoiceNumber LIKE 'INV-'+CONVERT(char(4),YEAR(@Date))+'-%'", connection, transaction))
                        {
                            numberCommand.Parameters.AddWithValue("@Date", invoiceDate);
                            number = Convert.ToString(numberCommand.ExecuteScalar());
                        }
                        decimal subtotal = 0;
                        foreach (DataRow row in items.Rows) subtotal += Convert.ToDecimal(row["TotalAmount"]);
                        decimal total = Math.Max(0, subtotal - invoiceDiscount);
                        int invoiceId;
                        using (var command = new SqlCommand(@"INSERT FeeInvoices(InvoiceNumber,StudentID,AcademicYearID,InvoiceDate,DueDate,
                                InvoiceType,Subtotal,DiscountAmount,TotalAmount,PaidAmount,[Status],Remarks,PaymentInstructions,CreatedBy)
                            VALUES(@Number,@Student,@Year,@InvoiceDate,@DueDate,@Type,@Subtotal,@Discount,@Total,0,'Unpaid',@Remarks,@Instructions,@User);
                            SELECT CONVERT(int,SCOPE_IDENTITY());", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Number", number);
                            command.Parameters.AddWithValue("@Student", studentId);
                            command.Parameters.AddWithValue("@Year", academicYearId);
                            command.Parameters.AddWithValue("@InvoiceDate", invoiceDate.Date);
                            command.Parameters.AddWithValue("@DueDate", dueDate.Date);
                            command.Parameters.AddWithValue("@Type", type);
                            command.Parameters.AddWithValue("@Subtotal", subtotal);
                            command.Parameters.AddWithValue("@Discount", invoiceDiscount);
                            command.Parameters.AddWithValue("@Total", total);
                            command.Parameters.AddWithValue("@Remarks", (object)remarks ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Instructions", (object)instructions ?? DBNull.Value);
                            command.Parameters.AddWithValue("@User", createdBy);
                            invoiceId = Convert.ToInt32(command.ExecuteScalar());
                        }
                        foreach (DataRow row in items.Rows)
                        {
                            using (var command = new SqlCommand(@"INSERT FeeInvoiceItems(InvoiceID,FeeCategoryID,FeeCategoryName,[Description],Amount,DiscountAmount,TotalAmount)
                                VALUES(@Invoice,@Category,@Name,@Description,@Amount,@Discount,@Total)", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@Invoice", invoiceId);
                                command.Parameters.AddWithValue("@Category", row["FeeCategoryID"]);
                                command.Parameters.AddWithValue("@Name", row["CategoryName"]);
                                command.Parameters.AddWithValue("@Description", row["Description"]);
                                command.Parameters.AddWithValue("@Amount", row["Amount"]);
                                command.Parameters.AddWithValue("@Discount", row["DiscountAmount"]);
                                command.Parameters.AddWithValue("@Total", row["TotalAmount"]);
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        return invoiceId;
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public DataTable GetInvoices(string search, string status)
        {
            return Query(@"SELECT i.InvoiceID,i.InvoiceNumber,s.StudentCode,
                    LTRIM(RTRIM(s.FirstName+' '+ISNULL(s.LastName,''))) StudentName,
                    c.ClassName,fc.CategoryName FeeType,i.TotalAmount,
                    ISNULL((SELECT SUM(p.AmountPaid) FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID),0) PaidAmount,
                    i.TotalAmount-ISNULL((SELECT SUM(p.AmountPaid) FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID),0) Balance,
                    i.DueDate,
                    CASE WHEN i.Status='Cancelled' THEN 'Cancelled'
                         WHEN i.TotalAmount<=ISNULL((SELECT SUM(p.AmountPaid) FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID),0) THEN 'Paid'
                         WHEN i.DueDate<CAST(GETDATE() AS date) THEN 'Overdue'
                         WHEN ISNULL((SELECT SUM(p.AmountPaid) FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID),0)>0 THEN 'Partial'
                         ELSE 'Unpaid' END StatusText
                FROM FeeInvoices i
                INNER JOIN Students s ON s.StudentID=i.StudentID
                INNER JOIN Sections sec ON sec.SectionID=s.SectionID
                INNER JOIN Classes c ON c.ClassID=sec.ClassID
                OUTER APPLY(SELECT TOP 1 FeeCategoryName CategoryName FROM FeeInvoiceItems WHERE InvoiceID=i.InvoiceID ORDER BY InvoiceItemID) fc
                WHERE (@Search='' OR i.InvoiceNumber LIKE '%'+@Search+'%' OR s.StudentCode LIKE '%'+@Search+'%'
                    OR s.FirstName LIKE '%'+@Search+'%' OR s.LastName LIKE '%'+@Search+'%' OR fc.CategoryName LIKE '%'+@Search+'%')
                  AND (@Status='' OR @Status=CASE WHEN i.Status='Cancelled' THEN 'Cancelled'
                         WHEN i.TotalAmount<=ISNULL((SELECT SUM(p.AmountPaid) FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID),0) THEN 'Paid'
                         WHEN i.DueDate<CAST(GETDATE() AS date) THEN 'Overdue'
                         WHEN ISNULL((SELECT SUM(p.AmountPaid) FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID),0)>0 THEN 'Partial' ELSE 'Unpaid' END)
                ORDER BY i.CreatedAt DESC",
                new SqlParameter("@Search", search ?? ""),
                new SqlParameter("@Status", status ?? ""));
        }

        public DataTable GetOpenInvoices(int? studentId)
        {
            return Query(@"SELECT i.InvoiceID,i.InvoiceNumber,i.StudentID,i.TotalAmount,
                    ISNULL(SUM(p.AmountPaid),0) PaidAmount,i.TotalAmount-ISNULL(SUM(p.AmountPaid),0) Balance
                FROM FeeInvoices i LEFT JOIN FeePayments p ON p.InvoiceID=i.InvoiceID
                WHERE i.Status<>'Cancelled' AND (@Student IS NULL OR i.StudentID=@Student)
                GROUP BY i.InvoiceID,i.InvoiceNumber,i.StudentID,i.TotalAmount,i.InvoiceDate
                HAVING i.TotalAmount>ISNULL(SUM(p.AmountPaid),0)
                ORDER BY i.InvoiceDate DESC", new SqlParameter("@Student", (object)studentId ?? DBNull.Value));
        }

        public int RecordPayment(int invoiceId, decimal amount, string method, DateTime date,
            string reference, string notes, int receivedBy)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int studentId; decimal total; decimal paid; string status;
                        using (var command = new SqlCommand(@"SELECT i.StudentID,i.TotalAmount,i.Status,
                            ISNULL((SELECT SUM(AmountPaid) FROM FeePayments WHERE InvoiceID=i.InvoiceID),0)
                            FROM FeeInvoices i WITH(UPDLOCK,HOLDLOCK) WHERE i.InvoiceID=@ID", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@ID", invoiceId);
                            using (var reader = command.ExecuteReader())
                            {
                                if (!reader.Read()) throw new InvalidOperationException("Invoice not found.");
                                studentId = reader.GetInt32(0); total = reader.GetDecimal(1); status = reader.GetString(2); paid = reader.GetDecimal(3);
                            }
                        }
                        decimal balance = total - paid;
                        if (status == "Cancelled") throw new InvalidOperationException("Cancelled invoices cannot accept payments.");
                        if (amount <= 0 || amount > balance) throw new InvalidOperationException("Payment must be greater than zero and not exceed the balance.");
                        string receipt;
                        using (var command = new SqlCommand(@"SELECT 'RCP-'+CONVERT(char(4),YEAR(@Date))+'-'+
                            RIGHT('000000'+CONVERT(varchar(6),ISNULL(MAX(TRY_CONVERT(int,RIGHT(ReceiptNumber,6))),0)+1),6)
                            FROM FeePayments WITH(UPDLOCK,HOLDLOCK) WHERE ReceiptNumber LIKE 'RCP-'+CONVERT(char(4),YEAR(@Date))+'-%'", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Date", date);
                            receipt = Convert.ToString(command.ExecuteScalar());
                        }
                        int paymentId;
                        using (var command = new SqlCommand(@"INSERT FeePayments(InvoiceID,StudentID,ReceiptNumber,AmountPaid,PaymentMethod,
                                PaymentDate,ReferenceNumber,Notes,PreviousBalance,NewBalance,ReceivedBy)
                            VALUES(@Invoice,@Student,@Receipt,@Amount,@Method,@Date,@Reference,@Notes,@Previous,@New,@User);
                            SELECT CONVERT(int,SCOPE_IDENTITY());", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Invoice", invoiceId);
                            command.Parameters.AddWithValue("@Student", studentId);
                            command.Parameters.AddWithValue("@Receipt", receipt);
                            command.Parameters.AddWithValue("@Amount", amount);
                            command.Parameters.AddWithValue("@Method", method);
                            command.Parameters.AddWithValue("@Date", date.Date);
                            command.Parameters.AddWithValue("@Reference", (object)reference ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Notes", (object)notes ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Previous", balance);
                            command.Parameters.AddWithValue("@New", balance - amount);
                            command.Parameters.AddWithValue("@User", receivedBy);
                            paymentId = Convert.ToInt32(command.ExecuteScalar());
                        }
                        using (var command = new SqlCommand(@"UPDATE FeeInvoices SET PaidAmount=@Paid,
                            Status=CASE WHEN @Paid>=TotalAmount THEN 'Paid' ELSE 'Partial' END,UpdatedAt=SYSUTCDATETIME()
                            WHERE InvoiceID=@Invoice", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Paid", paid + amount);
                            command.Parameters.AddWithValue("@Invoice", invoiceId);
                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return paymentId;
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        public DataTable GetDashboardSummary()
        {
            // Paid amounts are precomputed per-invoice via OUTER APPLY so the outer
            // aggregates never wrap a subquery (which SQL Server disallows).
            return Query(@"SELECT COUNT(*) TotalInvoices,
                ISNULL(SUM(CASE WHEN i.InvoiceDate>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)
                    THEN mp.MonthPaid ELSE 0 END),0) CollectedThisMonth,
                ISNULL(SUM(i.TotalAmount-ap.AllPaid),0) Outstanding,
                ISNULL(SUM(CASE WHEN i.DueDate<CAST(GETDATE() AS date) AND i.Status<>'Cancelled'
                    AND i.TotalAmount>ap.AllPaid THEN 1 ELSE 0 END),0) Overdue,
                CAST(CASE WHEN COUNT(*)=0 THEN 0 ELSE 100.0*SUM(CASE WHEN i.TotalAmount<=ap.AllPaid
                    THEN 1 ELSE 0 END)/COUNT(*) END AS DECIMAL(5,2)) SuccessRate
                FROM FeeInvoices i
                OUTER APPLY (SELECT ISNULL(SUM(AmountPaid),0) AllPaid FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID) ap
                OUTER APPLY (SELECT ISNULL(SUM(AmountPaid),0) MonthPaid FROM FeePayments p WHERE p.InvoiceID=i.InvoiceID
                    AND p.PaymentDate>=DATEFROMPARTS(YEAR(GETDATE()),MONTH(GETDATE()),1)) mp");
        }

        public DataTable GetRecentPayments()
        {
            return Query(@"SELECT TOP 8 p.PaymentID,p.ReceiptNumber,p.PaymentDate,p.AmountPaid,p.PaymentMethod,
                s.FirstName+' '+ISNULL(s.LastName,'') StudentName,i.InvoiceNumber
                FROM FeePayments p INNER JOIN Students s ON s.StudentID=p.StudentID
                INNER JOIN FeeInvoices i ON i.InvoiceID=p.InvoiceID ORDER BY p.CreatedAt DESC");
        }

        public DataTable GetInvoice(int id)
        {
            return Query(@"SELECT i.*,s.StudentCode,s.FirstName+' '+ISNULL(s.LastName,'') StudentName,
                c.ClassName,sec.SectionName,ay.YearName,u.FullName PreparedBy,
                i.TotalAmount-ISNULL((SELECT SUM(AmountPaid) FROM FeePayments WHERE InvoiceID=i.InvoiceID),0) Balance
                FROM FeeInvoices i INNER JOIN Students s ON s.StudentID=i.StudentID
                INNER JOIN Sections sec ON sec.SectionID=s.SectionID INNER JOIN Classes c ON c.ClassID=sec.ClassID
                INNER JOIN AcademicYears ay ON ay.AcademicYearID=i.AcademicYearID
                INNER JOIN Users u ON u.UserID=i.CreatedBy WHERE i.InvoiceID=@ID",
                new SqlParameter("@ID", id));
        }

        public DataTable GetInvoiceItems(int id)
        {
            return Query("SELECT * FROM FeeInvoiceItems WHERE InvoiceID=@ID ORDER BY InvoiceItemID", new SqlParameter("@ID", id));
        }

        public DataTable GetPayment(int id)
        {
            return Query(@"SELECT p.*,i.InvoiceNumber,s.StudentCode,s.FirstName+' '+ISNULL(s.LastName,'') StudentName,u.FullName ReceivedByName
                FROM FeePayments p INNER JOIN FeeInvoices i ON i.InvoiceID=p.InvoiceID
                INNER JOIN Students s ON s.StudentID=p.StudentID INNER JOIN Users u ON u.UserID=p.ReceivedBy
                WHERE p.PaymentID=@ID", new SqlParameter("@ID", id));
        }

        // ============================================================
        // Finance Setup Queue — New Admissions Pending Finance Setup.
        // Reuses the canonical FeeInvoices model, the INV-{year} numbering,
        // and the same applicable-ClassFeeStructures resolution as
        // GetApplicableStructures / CreateInvoice. No new invoice architecture.
        // ============================================================

        // Per-active-student computed columns shared by summary + queue.
        private const string PendingBaseSql = @"
            FROM Students s
            INNER JOIN Sections sec ON sec.SectionID = s.SectionID
            INNER JOIN Classes c ON c.ClassID = sec.ClassID
            LEFT JOIN AcademicYears ay ON ay.AcademicYearID = s.AcademicYearID
            LEFT JOIN Guardians g ON g.GuardianID = s.GuardianID
            CROSS APPLY (SELECT
                CASE WHEN s.EnrollmentDate IS NULL THEN 1 ELSE 0 END AS MissingData,
                CASE WHEN EXISTS(SELECT 1 FROM FeeInvoices fi WHERE fi.StudentID=s.StudentID AND fi.AcademicYearID=s.AcademicYearID AND fi.Status<>'Cancelled') THEN 1 ELSE 0 END AS HasInvoice,
                (SELECT COUNT(*) FROM (
                    SELECT fs.FeeCategoryID
                    FROM ClassFeeStructures fs
                    INNER JOIN FeeCategories fc ON fc.FeeCategoryID=fs.FeeCategoryID AND fc.IsActive=1
                    WHERE fs.IsActive=1 AND fs.AcademicYearID=s.AcademicYearID AND fs.ClassID=sec.ClassID
                      AND (fs.SectionID=s.SectionID OR fs.SectionID IS NULL)
                      AND (fs.SectionID=s.SectionID OR NOT EXISTS(
                            SELECT 1 FROM ClassFeeStructures sp WHERE sp.AcademicYearID=fs.AcademicYearID AND sp.ClassID=fs.ClassID
                              AND sp.SectionID=s.SectionID AND sp.FeeCategoryID=fs.FeeCategoryID AND sp.IsActive=1))
                    GROUP BY fs.FeeCategoryID) z) AS AppCount,
                CASE WHEN EXISTS(
                    SELECT fs.FeeCategoryID
                    FROM ClassFeeStructures fs
                    WHERE fs.IsActive=1 AND fs.AcademicYearID=s.AcademicYearID AND fs.ClassID=sec.ClassID
                      AND (fs.SectionID=s.SectionID OR fs.SectionID IS NULL)
                      AND (fs.SectionID=s.SectionID OR NOT EXISTS(
                            SELECT 1 FROM ClassFeeStructures sp WHERE sp.AcademicYearID=fs.AcademicYearID AND sp.ClassID=fs.ClassID
                              AND sp.SectionID=s.SectionID AND sp.FeeCategoryID=fs.FeeCategoryID AND sp.IsActive=1))
                    GROUP BY fs.FeeCategoryID HAVING COUNT(*)>1) THEN 1 ELSE 0 END AS DupFlag
            ) calc
            WHERE s.Status='Active'";

        private const string StatusExpr = @"
            CASE WHEN calc.MissingData=1 THEN 'Missing Required Data'
                 WHEN calc.HasInvoice=1 THEN 'Invoice Already Exists'
                 WHEN calc.AppCount=0 THEN 'No Matching Fee Structure'
                 WHEN calc.DupFlag=1 THEN 'Multiple Fee Structures'
                 ELSE 'Ready' END";

        /// <summary>Student placement details for the confirmation modal.</summary>
        public DataTable GetStudentPlacement(int studentId)
        {
            return Query(@"SELECT s.StudentID, s.StudentCode,
                    LTRIM(RTRIM(s.FirstName+' '+ISNULL(s.LastName,''))) AS StudentName,
                    ISNULL(ay.YearName,'—') AS AcademicYear, c.ClassName, sec.SectionName, ISNULL(s.Shift,'—') AS Shift
                FROM Students s
                INNER JOIN Sections sec ON sec.SectionID=s.SectionID
                INNER JOIN Classes c ON c.ClassID=sec.ClassID
                LEFT JOIN AcademicYears ay ON ay.AcademicYearID=s.AcademicYearID
                WHERE s.StudentID=@id AND s.Status='Active'", new SqlParameter("@id", studentId));
        }

        /// <summary>Summary counts for the queue cards (all DB-derived).</summary>
        public DataTable PendingSetupSummary()
        {
            return Query(@"
                SELECT
                  (SELECT COUNT(*) " + PendingBaseSql + @" AND calc.HasInvoice=0) AS Pending,
                  (SELECT COUNT(*) " + PendingBaseSql + @" AND calc.HasInvoice=0 AND calc.MissingData=0 AND calc.AppCount>=1 AND calc.DupFlag=0) AS Ready,
                  (SELECT COUNT(*) " + PendingBaseSql + @" AND calc.HasInvoice=0 AND calc.MissingData=0 AND calc.AppCount=0) AS MissingStructure,
                  (SELECT COUNT(*) FROM FeeInvoices WHERE InvoiceType='Initial' AND CAST(CreatedAt AS date)=CAST(GETDATE() AS date)) AS CompletedToday");
        }

        /// <summary>Filtered, paginated queue. Default (empty status) hides already-invoiced students.</summary>
        public DataTable PendingSetupQueue(string search, int? yearId, int? classId, int? sectionId, string shift, string status, int page, int pageSize, out int total)
        {
            var filters = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@Search", (object)(search ?? "")),
                new SqlParameter("@Year", (object)yearId ?? DBNull.Value),
                new SqlParameter("@Class", (object)classId ?? DBNull.Value),
                new SqlParameter("@Section", (object)sectionId ?? DBNull.Value),
                new SqlParameter("@Shift", (object)(string.IsNullOrEmpty(shift) ? null : shift) ?? DBNull.Value),
                new SqlParameter("@Status", (object)(string.IsNullOrEmpty(status) ? null : status) ?? DBNull.Value)
            };

            string whereExtra = @"
                AND (@Search='' OR s.StudentCode LIKE '%'+@Search+'%' OR s.AdmissionNo LIKE '%'+@Search+'%'
                     OR LTRIM(RTRIM(s.FirstName+' '+ISNULL(s.LastName,''))) LIKE '%'+@Search+'%')
                AND (@Year IS NULL OR s.AcademicYearID=@Year)
                AND (@Class IS NULL OR sec.ClassID=@Class)
                AND (@Section IS NULL OR s.SectionID=@Section)
                AND (@Shift IS NULL OR s.Shift=@Shift)
                AND (( @Status IS NULL AND calc.HasInvoice=0 ) OR ( @Status IS NOT NULL AND (" + StatusExpr + @")=@Status ))";

            var countParams = filters.ToArray();
            total = Convert.ToInt32(Query("SELECT COUNT(*) " + PendingBaseSql + whereExtra, countParams).Rows[0][0]);

            var pageParams = new System.Collections.Generic.List<SqlParameter>(filters)
            {
                new SqlParameter("@Skip", (page - 1) * pageSize),
                new SqlParameter("@Take", pageSize)
            };

            string sql = @"
                SELECT s.StudentID, s.StudentCode, s.AdmissionNo,
                       LTRIM(RTRIM(s.FirstName+' '+ISNULL(s.LastName,''))) AS StudentName,
                       s.EnrollmentDate, ISNULL(ay.YearName,'—') AS AcademicYear, c.ClassName, sec.SectionName,
                       ISNULL(s.Shift,'—') AS Shift, ISNULL(g.FullName,'—') AS Guardian,
                       calc.AppCount, " + StatusExpr + @" AS FinanceStatus
                " + PendingBaseSql + whereExtra + @"
                ORDER BY s.EnrollmentDate DESC, s.StudentID DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";
            return Query(sql, pageParams.ToArray());
        }

        /// <summary>
        /// Creates the student's initial invoice in ONE transaction with every guard:
        /// locks the student, confirms Active, re-checks no year-invoice exists, loads the
        /// applicable ClassFeeStructures items, generates the canonical number, inserts header
        /// + items, audits. Returns false with a safe message on any guard failure.
        /// </summary>
        public bool CreateInitialInvoice(int studentId, DateTime invoiceDate, DateTime dueDate, int createdBy, string ip,
            out int invoiceId, out string invoiceNumber, out decimal total, out string message)
        {
            invoiceId = 0; invoiceNumber = null; total = 0; message = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int yearId, sectionId, classId;
                        using (var cmd = new SqlCommand("SELECT s.Status, s.AcademicYearID, s.SectionID, sec.ClassID FROM Students s WITH (UPDLOCK, HOLDLOCK) JOIN Sections sec ON sec.SectionID=s.SectionID WHERE s.StudentID=@id", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", studentId);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (!r.Read()) { tx.Rollback(); message = "Student not found."; return false; }
                                if (Convert.ToString(r["Status"]) != "Active") { tx.Rollback(); message = "The student is not active. Invoice not created."; return false; }
                                yearId = Convert.ToInt32(r["AcademicYearID"]);
                                sectionId = Convert.ToInt32(r["SectionID"]);
                                classId = Convert.ToInt32(r["ClassID"]);
                            }
                        }

                        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM FeeInvoices WITH (UPDLOCK, HOLDLOCK) WHERE StudentID=@s AND AcademicYearID=@y AND Status<>'Cancelled'", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@s", studentId);
                            cmd.Parameters.AddWithValue("@y", yearId);
                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) { tx.Rollback(); message = "An invoice already exists for this student and academic year."; return false; }
                        }

                        var items = new DataTable();
                        using (var cmd = new SqlCommand(@"
                            SELECT fs.FeeCategoryID, fc.CategoryName,
                                   COALESCE(fs.[Description],fc.[Description]) AS [Description],
                                   fs.Amount,
                                   CASE fs.DiscountType WHEN 'Percentage' THEN ROUND(fs.Amount*fs.DiscountAmount/100,2)
                                        WHEN 'Fixed Amount' THEN fs.DiscountAmount ELSE 0 END AS DiscountAmount
                            FROM ClassFeeStructures fs
                            INNER JOIN FeeCategories fc ON fc.FeeCategoryID=fs.FeeCategoryID AND fc.IsActive=1
                            WHERE fs.IsActive=1 AND fs.AcademicYearID=@y AND fs.ClassID=@c
                              AND (fs.SectionID=@sec OR fs.SectionID IS NULL)
                              AND (fs.SectionID=@sec OR NOT EXISTS(
                                    SELECT 1 FROM ClassFeeStructures sp WHERE sp.AcademicYearID=fs.AcademicYearID AND sp.ClassID=fs.ClassID
                                      AND sp.SectionID=@sec AND sp.FeeCategoryID=fs.FeeCategoryID AND sp.IsActive=1))
                            ORDER BY fc.CategoryName", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@y", yearId);
                            cmd.Parameters.AddWithValue("@c", classId);
                            cmd.Parameters.AddWithValue("@sec", sectionId);
                            using (var a = new SqlDataAdapter(cmd)) a.Fill(items);
                        }

                        if (items.Rows.Count == 0) { tx.Rollback(); message = "No matching fee structure exists for this student. Set one up in Fee Structures first."; return false; }

                        // True duplicate category (two rows resolved to the same level) — do not guess.
                        var seen = new System.Collections.Generic.HashSet<int>();
                        foreach (DataRow row in items.Rows)
                            if (!seen.Add(Convert.ToInt32(row["FeeCategoryID"])))
                            { tx.Rollback(); message = "Multiple conflicting fee structures match this student. Resolve them in Fee Structures before invoicing."; return false; }

                        decimal subtotal = 0;
                        foreach (DataRow row in items.Rows)
                        {
                            decimal amount = Convert.ToDecimal(row["Amount"]);
                            decimal disc = Convert.ToDecimal(row["DiscountAmount"]);
                            subtotal += Math.Max(0, amount - disc);
                        }
                        total = subtotal;

                        using (var cmd = new SqlCommand(@"SELECT 'INV-'+CONVERT(char(4),YEAR(@Date))+'-'+
                            RIGHT('000000'+CONVERT(varchar(6),ISNULL(MAX(TRY_CONVERT(int,RIGHT(InvoiceNumber,6))),0)+1),6)
                            FROM FeeInvoices WITH(UPDLOCK,HOLDLOCK) WHERE InvoiceNumber LIKE 'INV-'+CONVERT(char(4),YEAR(@Date))+'-%'", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Date", invoiceDate);
                            invoiceNumber = Convert.ToString(cmd.ExecuteScalar());
                        }

                        using (var cmd = new SqlCommand(@"INSERT FeeInvoices(InvoiceNumber,StudentID,AcademicYearID,InvoiceDate,DueDate,InvoiceType,Subtotal,DiscountAmount,TotalAmount,PaidAmount,[Status],CreatedBy)
                            VALUES(@Number,@Student,@Year,@InvoiceDate,@DueDate,'Initial',@Subtotal,0,@Total,0,'Unpaid',@User);
                            SELECT CONVERT(int,SCOPE_IDENTITY());", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@Number", invoiceNumber);
                            cmd.Parameters.AddWithValue("@Student", studentId);
                            cmd.Parameters.AddWithValue("@Year", yearId);
                            cmd.Parameters.AddWithValue("@InvoiceDate", invoiceDate.Date);
                            cmd.Parameters.AddWithValue("@DueDate", dueDate.Date);
                            cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                            cmd.Parameters.AddWithValue("@Total", total);
                            cmd.Parameters.AddWithValue("@User", createdBy);
                            invoiceId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (DataRow row in items.Rows)
                        {
                            decimal amount = Convert.ToDecimal(row["Amount"]);
                            decimal disc = Convert.ToDecimal(row["DiscountAmount"]);
                            using (var cmd = new SqlCommand(@"INSERT FeeInvoiceItems(InvoiceID,FeeCategoryID,FeeCategoryName,[Description],Amount,DiscountAmount,TotalAmount)
                                VALUES(@Invoice,@Category,@Name,@Description,@Amount,@Discount,@Total)", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@Invoice", invoiceId);
                                cmd.Parameters.AddWithValue("@Category", row["FeeCategoryID"]);
                                cmd.Parameters.AddWithValue("@Name", row["CategoryName"]);
                                cmd.Parameters.AddWithValue("@Description", row["Description"] == DBNull.Value ? (object)DBNull.Value : row["Description"]);
                                cmd.Parameters.AddWithValue("@Amount", amount);
                                cmd.Parameters.AddWithValue("@Discount", disc);
                                cmd.Parameters.AddWithValue("@Total", Math.Max(0, amount - disc));
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (var cmd = new SqlCommand("INSERT dbo.AuditLog(UserID,Action,Module,Detail,IPAddress,ActionTime) VALUES(@u,'FINANCE_INITIAL_INVOICE','Finance',@d,@ip,GETDATE())", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@u", createdBy);
                            cmd.Parameters.AddWithValue("@d", "Student #" + studentId + " Invoice #" + invoiceId + " (" + invoiceNumber + ") Year #" + yearId);
                            cmd.Parameters.AddWithValue("@ip", (object)(ip ?? "Unavailable"));
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        message = "Initial invoice " + invoiceNumber + " created.";
                        return true;
                    }
                    catch
                    {
                        try { tx.Rollback(); } catch { }
                        invoiceId = 0; invoiceNumber = null; total = 0;
                        message = "The invoice could not be created due to a system error. Please try again.";
                        return false;
                    }
                }
            }
        }
    }
}
