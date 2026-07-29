using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AQOONHUB_SMS.Modules.Payroll
{
    public sealed class PayrollRepository
    {
        private readonly string _connectionString;

        public PayrollRepository()
        {
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings["AQOONHUB_DB"];

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new ConfigurationErrorsException(
                    "Connection string 'AQOONHUB_DB' was not found.");
            }

            _connectionString = settings.ConnectionString;
        }

        public DataTable GetPayrollPeriods()
        {
            const string sql = @"
SELECT
    PayrollPeriodID,
    PeriodName,
    StartDate,
    EndDate,
    PaymentDate,
    Status,
    CreatedBy,
    UpdatedBy,
    CreatedAt,
    UpdatedAt
FROM dbo.PayrollPeriods
ORDER BY StartDate DESC, PayrollPeriodID DESC;";

            return ExecuteDataTable(sql, null);
        }

        public PayrollPeriodData GetPayrollPeriod(int payrollPeriodId)
        {
            const string sql = @"
SELECT
    PayrollPeriodID,
    PeriodName,
    StartDate,
    EndDate,
    PaymentDate,
    Status,
    CreatedBy,
    UpdatedBy,
    CreatedAt,
    UpdatedAt
FROM dbo.PayrollPeriods
WHERE PayrollPeriodID = @PayrollPeriodID;";

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@PayrollPeriodID", SqlDbType.Int).Value =
                    payrollPeriodId;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return new PayrollPeriodData
                    {
                        PayrollPeriodID = Convert.ToInt32(reader["PayrollPeriodID"]),
                        PeriodName = Convert.ToString(reader["PeriodName"]),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"]),
                        PaymentDate = reader["PaymentDate"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(reader["PaymentDate"]),
                        Status = Convert.ToString(reader["Status"]),
                        CreatedBy = reader["CreatedBy"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(reader["CreatedBy"]),
                        UpdatedBy = reader["UpdatedBy"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(reader["UpdatedBy"]),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        UpdatedAt = reader["UpdatedAt"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(reader["UpdatedAt"])
                    };
                }
            }
        }

        public int SavePayrollPeriod(
            PayrollPeriodInput input,
            int? currentUserId = null)
        {
            ValidatePayrollPeriodInput(input);

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        int payrollPeriodId;

                        if (input.PayrollPeriodID.HasValue &&
                            input.PayrollPeriodID.Value > 0)
                        {
                            const string updateSql = @"
DECLARE @CurrentStatus NVARCHAR(20);

SELECT @CurrentStatus = Status
FROM dbo.PayrollPeriods WITH (UPDLOCK, HOLDLOCK)
WHERE PayrollPeriodID = @PayrollPeriodID;

IF @CurrentStatus IS NULL
    THROW 51000, 'The payroll period was not found.', 1;

IF @CurrentStatus <> N'Draft'
    THROW 51000, 'Only a Draft payroll period can be edited.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.PayrollPeriods WITH (UPDLOCK, HOLDLOCK)
    WHERE PayrollPeriodID <> @PayrollPeriodID
      AND StartDate <= @EndDate
      AND EndDate >= @StartDate
      AND Status <> N'Cancelled'
)
    THROW 51000, 'The payroll period overlaps another active payroll period.', 1;

UPDATE dbo.PayrollPeriods
SET
    PeriodName = @PeriodName,
    StartDate = @StartDate,
    EndDate = @EndDate,
    PaymentDate = @PaymentDate,
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE PayrollPeriodID = @PayrollPeriodID;

SELECT @PayrollPeriodID;";

                            using (SqlCommand command = new SqlCommand(
                                updateSql, connection, transaction))
                            {
                                AddPayrollPeriodParameters(
                                    command, input, currentUserId);
                                payrollPeriodId = Convert.ToInt32(
                                    command.ExecuteScalar());
                            }
                        }
                        else
                        {
                            const string insertSql = @"
IF EXISTS
(
    SELECT 1
    FROM dbo.PayrollPeriods WITH (UPDLOCK, HOLDLOCK)
    WHERE StartDate <= @EndDate
      AND EndDate >= @StartDate
      AND Status <> N'Cancelled'
)
    THROW 51000, 'The payroll period overlaps another active payroll period.', 1;

INSERT INTO dbo.PayrollPeriods
(
    PeriodName,
    StartDate,
    EndDate,
    PaymentDate,
    Status,
    CreatedBy,
    UpdatedBy,
    CreatedAt,
    UpdatedAt
)
OUTPUT INSERTED.PayrollPeriodID
VALUES
(
    @PeriodName,
    @StartDate,
    @EndDate,
    @PaymentDate,
    N'Draft',
    @CurrentUserID,
    NULL,
    SYSUTCDATETIME(),
    NULL
);";

                            using (SqlCommand command = new SqlCommand(
                                insertSql, connection, transaction))
                            {
                                AddPayrollPeriodParameters(
                                    command, input, currentUserId);
                                payrollPeriodId = Convert.ToInt32(
                                    command.ExecuteScalar());
                            }
                        }

                        transaction.Commit();
                        return payrollPeriodId;
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public void SetPayrollPeriodStatus(
            int payrollPeriodId,
            string newStatus,
            int? currentUserId = null)
        {
            string normalizedStatus = NormalizePeriodStatus(newStatus);

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        const string selectSql = @"
SELECT Status
FROM dbo.PayrollPeriods WITH (UPDLOCK, HOLDLOCK)
WHERE PayrollPeriodID = @PayrollPeriodID;";

                        string currentStatus;

                        using (SqlCommand command = new SqlCommand(
                            selectSql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollPeriodID", SqlDbType.Int).Value =
                                payrollPeriodId;

                            object value = command.ExecuteScalar();

                            if (value == null || value == DBNull.Value)
                            {
                                throw new InvalidOperationException(
                                    "The payroll period was not found.");
                            }

                            currentStatus = Convert.ToString(value);
                        }

                        if (!IsValidPeriodTransition(currentStatus, normalizedStatus))
                        {
                            throw new InvalidOperationException(
                                "The requested payroll period status transition is not allowed.");
                        }

                        if (normalizedStatus == "Completed")
                        {
                            const string completionSql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.PayrollRecords WITH (UPDLOCK, HOLDLOCK)
    WHERE PayrollPeriodID = @PayrollPeriodID
)
    THROW 51000, 'A payroll period with no payroll records cannot be completed.', 1;

IF EXISTS
(
    SELECT 1
    FROM dbo.PayrollRecords WITH (UPDLOCK, HOLDLOCK)
    WHERE PayrollPeriodID = @PayrollPeriodID
      AND PaymentStatus IN (N'Pending', N'Failed')
)
    THROW 51000, 'The payroll period cannot be completed while Pending or Failed records remain.', 1;";

                            using (SqlCommand command = new SqlCommand(
                                completionSql, connection, transaction))
                            {
                                command.Parameters.Add(
                                    "@PayrollPeriodID", SqlDbType.Int).Value =
                                    payrollPeriodId;
                                command.ExecuteNonQuery();
                            }
                        }

                        const string updateSql = @"
UPDATE dbo.PayrollPeriods
SET
    Status = @Status,
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE PayrollPeriodID = @PayrollPeriodID;";

                        using (SqlCommand command = new SqlCommand(
                            updateSql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@Status", SqlDbType.NVarChar, 20).Value =
                                normalizedStatus;
                            command.Parameters.Add(
                                "@CurrentUserID", SqlDbType.Int).Value =
                                ToDatabaseValue(currentUserId);
                            command.Parameters.Add(
                                "@PayrollPeriodID", SqlDbType.Int).Value =
                                payrollPeriodId;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public DataTable GetEligibleStaff()
        {
            const string sql = @"
SELECT
    StaffID,
    EmployeeID,
    Department,
    Position,
    HireDate,
    Salary,
    Status
FROM dbo.Staff
WHERE Status = N'Active'
ORDER BY EmployeeID, StaffID;";

            return ExecuteDataTable(sql, null);
        }

        public DataTable GetSalaryStructures(
            int? staffId,
            string search,
            string status)
        {
            ValidateOptionalText(search, 150, "search");
            ValidateOptionalText(status, 20, "status");

            const string sql = @"
SELECT
    ss.SalaryStructureID,
    ss.StaffID,
    s.EmployeeID,
    s.Department,
    s.Position,
    ss.BasicSalary,
    ss.HousingAllowance,
    ss.TransportAllowance,
    ss.OtherAllowance,
    ss.TaxDeduction,
    ss.OtherDeduction,
    ss.EffectiveFrom,
    ss.EffectiveTo,
    ss.Status,
    ss.CreatedBy,
    ss.UpdatedBy,
    ss.CreatedAt,
    ss.UpdatedAt
FROM dbo.StaffSalaryStructures AS ss
INNER JOIN dbo.Staff AS s
    ON s.StaffID = ss.StaffID
WHERE (@StaffID IS NULL OR ss.StaffID = @StaffID)
  AND (@Status = N'' OR ss.Status = @Status)
  AND
  (
      @Search = N''
      OR s.EmployeeID LIKE N'%' + @Search + N'%'
      OR s.Department LIKE N'%' + @Search + N'%'
      OR s.Position LIKE N'%' + @Search + N'%'
  )
ORDER BY ss.EffectiveFrom DESC, ss.SalaryStructureID DESC;";

            return ExecuteDataTable(
                sql,
                new[]
                {
                    CreateNullableIntParameter("@StaffID", staffId),
                    CreateNVarCharParameter("@Search", 150, search),
                    CreateNVarCharParameter("@Status", 20, status)
                });
        }

        public DataRow GetSalaryStructure(int salaryStructureId)
        {
            const string sql = @"
SELECT
    ss.SalaryStructureID,
    ss.StaffID,
    s.EmployeeID,
    s.Department,
    s.Position,
    ss.BasicSalary,
    ss.HousingAllowance,
    ss.TransportAllowance,
    ss.OtherAllowance,
    ss.TaxDeduction,
    ss.OtherDeduction,
    ss.EffectiveFrom,
    ss.EffectiveTo,
    ss.Status,
    ss.CreatedBy,
    ss.UpdatedBy,
    ss.CreatedAt,
    ss.UpdatedAt
FROM dbo.StaffSalaryStructures AS ss
INNER JOIN dbo.Staff AS s
    ON s.StaffID = ss.StaffID
WHERE ss.SalaryStructureID = @SalaryStructureID;";

            DataTable table = ExecuteDataTable(
                sql,
                new[]
                {
                    new SqlParameter("@SalaryStructureID", SqlDbType.Int)
                    {
                        Value = salaryStructureId
                    }
                });

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public int SaveSalaryStructure(
            SalaryStructureInput input,
            int? currentUserId = null)
        {
            ValidateSalaryStructureInput(input);

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        const string staffSql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Staff WITH (UPDLOCK, HOLDLOCK)
    WHERE StaffID = @StaffID
      AND Status = N'Active'
)
    THROW 51000, 'The selected active staff member was not found.', 1;";

                        using (SqlCommand command = new SqlCommand(
                            staffSql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@StaffID", SqlDbType.Int).Value = input.StaffID;
                            command.ExecuteNonQuery();
                        }

                        const string overlapSql = @"
IF EXISTS
(
    SELECT 1
    FROM dbo.StaffSalaryStructures WITH (UPDLOCK, HOLDLOCK)
    WHERE StaffID = @StaffID
      AND SalaryStructureID <> @SalaryStructureID
      AND Status = N'Active'
      AND @Status = N'Active'
      AND EffectiveFrom <= ISNULL(@EffectiveTo, CONVERT(DATE, '99991231'))
      AND ISNULL(EffectiveTo, CONVERT(DATE, '99991231')) >= @EffectiveFrom
)
    THROW 51000, 'The salary structure overlaps another active salary structure for this staff member.', 1;";

                        using (SqlCommand command = new SqlCommand(
                            overlapSql, connection, transaction))
                        {
                            AddSalaryStructureParameters(
                                command, input, currentUserId);
                            command.ExecuteNonQuery();
                        }

                        int salaryStructureId;

                        if (input.SalaryStructureID.HasValue &&
                            input.SalaryStructureID.Value > 0)
                        {
                            const string updateSql = @"
UPDATE dbo.StaffSalaryStructures
SET
    StaffID = @StaffID,
    BasicSalary = @BasicSalary,
    HousingAllowance = @HousingAllowance,
    TransportAllowance = @TransportAllowance,
    OtherAllowance = @OtherAllowance,
    TaxDeduction = @TaxDeduction,
    OtherDeduction = @OtherDeduction,
    EffectiveFrom = @EffectiveFrom,
    EffectiveTo = @EffectiveTo,
    Status = @Status,
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE SalaryStructureID = @SalaryStructureID;

IF @@ROWCOUNT = 0
    THROW 51000, 'The salary structure was not found.', 1;

SELECT @SalaryStructureID;";

                            using (SqlCommand command = new SqlCommand(
                                updateSql, connection, transaction))
                            {
                                AddSalaryStructureParameters(
                                    command, input, currentUserId);
                                salaryStructureId = Convert.ToInt32(
                                    command.ExecuteScalar());
                            }
                        }
                        else
                        {
                            const string insertSql = @"
INSERT INTO dbo.StaffSalaryStructures
(
    StaffID,
    BasicSalary,
    HousingAllowance,
    TransportAllowance,
    OtherAllowance,
    TaxDeduction,
    OtherDeduction,
    EffectiveFrom,
    EffectiveTo,
    Status,
    CreatedBy,
    UpdatedBy,
    CreatedAt,
    UpdatedAt
)
OUTPUT INSERTED.SalaryStructureID
VALUES
(
    @StaffID,
    @BasicSalary,
    @HousingAllowance,
    @TransportAllowance,
    @OtherAllowance,
    @TaxDeduction,
    @OtherDeduction,
    @EffectiveFrom,
    @EffectiveTo,
    @Status,
    @CurrentUserID,
    NULL,
    SYSUTCDATETIME(),
    NULL
);";

                            using (SqlCommand command = new SqlCommand(
                                insertSql, connection, transaction))
                            {
                                AddSalaryStructureParameters(
                                    command, input, currentUserId);
                                salaryStructureId = Convert.ToInt32(
                                    command.ExecuteScalar());
                            }
                        }

                        transaction.Commit();
                        return salaryStructureId;
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public PayrollListResult GetPayrollRecords(
            PayrollListFilter filter,
            int pageNumber,
            int pageSize,
            string sortColumn,
            string sortDirection)
        {
            if (filter == null)
            {
                filter = new PayrollListFilter();
            }

            ValidateOptionalText(filter.Department, 100, "Department");
            ValidateOptionalText(filter.PaymentStatus, 20, "PaymentStatus");
            ValidateOptionalText(filter.Search, 150, "Search");

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 200);

            string orderBy = GetPayrollSortExpression(sortColumn);
            string direction = string.Equals(
                sortDirection, "ASC", StringComparison.OrdinalIgnoreCase)
                ? "ASC"
                : "DESC";

            string fromAndWhere = @"
FROM dbo.PayrollRecords AS pr
INNER JOIN dbo.PayrollPeriods AS pp
    ON pp.PayrollPeriodID = pr.PayrollPeriodID
INNER JOIN dbo.Staff AS s
    ON s.StaffID = pr.StaffID
WHERE (@PayrollPeriodID IS NULL
       OR pr.PayrollPeriodID = @PayrollPeriodID)
  AND (@Department = N''
       OR s.Department = @Department)
  AND (@PaymentStatus = N''
       OR pr.PaymentStatus = @PaymentStatus)
  AND
  (
      @Search = N''
      OR s.EmployeeID LIKE N'%' + @Search + N'%'
      OR s.Department LIKE N'%' + @Search + N'%'
      OR s.Position LIKE N'%' + @Search + N'%'
  )";

            string countSql = "SELECT COUNT_BIG(1) " + fromAndWhere + ";";
            string dataSql = @"
SELECT
    pr.PayrollRecordID,
    pr.PayrollPeriodID,
    pp.PeriodName,
    pp.StartDate,
    pp.EndDate,
    pp.PaymentDate,
    pr.StaffID,
    s.EmployeeID,
    s.Department,
    s.Position,
    pr.BasicSalary,
    pr.HousingAllowance,
    pr.TransportAllowance,
    pr.OtherAllowance,
    pr.TaxDeduction,
    pr.OtherDeduction,
    pr.TotalAllowances,
    pr.BonusAmount,
    pr.TotalDeductions,
    pr.GrossSalary,
    pr.NetSalary,
    pr.PaymentStatus,
    pr.PaymentMethod,
    pr.PaymentReference,
    pr.PaidDate,
    pr.CreatedBy,
    pr.UpdatedBy,
    pr.CreatedAt,
    pr.UpdatedAt
" + fromAndWhere + @"
ORDER BY " + orderBy + " " + direction + @",
         pr.PayrollRecordID DESC
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;";

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();
                long totalRecords;

                using (SqlCommand command = new SqlCommand(countSql, connection))
                {
                    AddPayrollListFilterParameters(command, filter);
                    totalRecords = Convert.ToInt64(command.ExecuteScalar());
                }

                DataTable records = new DataTable();

                using (SqlCommand command = new SqlCommand(dataSql, connection))
                {
                    AddPayrollListFilterParameters(command, filter);
                    command.Parameters.Add("@Offset", SqlDbType.Int).Value =
                        (pageNumber - 1) * pageSize;
                    command.Parameters.Add("@PageSize", SqlDbType.Int).Value =
                        pageSize;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(records);
                    }
                }

                return new PayrollListResult
                {
                    Records = records,
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        public DataTable GetPayrollSummary(
            int? payrollPeriodId,
            string department)
        {
            ValidateOptionalText(department, 100, "department");

            const string sql = @"
SELECT
    COUNT_BIG(1) AS RecordCount,
    ISNULL(SUM(pr.GrossSalary), 0) AS GrossSalary,
    ISNULL(SUM(pr.TotalDeductions), 0) AS TotalDeductions,
    ISNULL(SUM(pr.NetSalary), 0) AS NetSalary,
    ISNULL
    (
        SUM
        (
            CASE
                WHEN pr.PaymentStatus = N'Paid'
                THEN pr.NetSalary
                ELSE 0
            END
        ),
        0
    ) AS Paid,
    ISNULL
    (
        SUM
        (
            CASE
                WHEN pr.PaymentStatus = N'Pending'
                THEN pr.NetSalary
                ELSE 0
            END
        ),
        0
    ) AS Pending
FROM dbo.PayrollRecords AS pr
INNER JOIN dbo.Staff AS s
    ON s.StaffID = pr.StaffID
WHERE (@PayrollPeriodID IS NULL
       OR pr.PayrollPeriodID = @PayrollPeriodID)
  AND (@Department = N''
       OR s.Department = @Department);";

            return ExecuteDataTable(
                sql,
                new[]
                {
                    CreateNullableIntParameter(
                        "@PayrollPeriodID", payrollPeriodId),
                    CreateNVarCharParameter(
                        "@Department", 100, department)
                });
        }

        /// <summary>
        /// Detailed payroll totals for reporting (single summary row). Housing and
        /// Transport allowances are excluded from all reported figures.
        /// </summary>
        public DataTable GetPayrollReport(int? payrollPeriodId, string department, string status)
        {
            ValidateOptionalText(department, 100, "department");
            ValidateOptionalText(status, 30, "status");

            const string sql = @"
SELECT
    COUNT_BIG(1) AS RecordCount,
    ISNULL(SUM(pr.BasicSalary), 0) AS TotalBasic,
    ISNULL(SUM(pr.OtherAllowance), 0) AS TotalOtherAllowance,
    ISNULL(SUM(pr.BonusAmount), 0) AS TotalBonus,
    ISNULL(SUM(pr.GrossSalary), 0) AS TotalGross,
    ISNULL(SUM(pr.TaxDeduction), 0) AS TotalTax,
    ISNULL(SUM(pr.OtherDeduction), 0) AS TotalOtherDeduction,
    ISNULL(SUM(pr.TotalDeductions), 0) AS TotalDeductions,
    ISNULL(SUM(pr.NetSalary), 0) AS TotalNet,
    ISNULL(SUM(CASE WHEN pr.PaymentStatus = N'Paid' THEN pr.NetSalary ELSE 0 END), 0) AS PaidAmount,
    ISNULL(SUM(CASE WHEN pr.PaymentStatus = N'Pending' THEN pr.NetSalary ELSE 0 END), 0) AS PendingAmount,
    ISNULL(SUM(CASE WHEN pr.PaymentStatus = N'Failed' THEN 1 ELSE 0 END), 0) AS FailedCount
FROM dbo.PayrollRecords AS pr
INNER JOIN dbo.Staff AS s ON s.StaffID = pr.StaffID
WHERE (@PayrollPeriodID IS NULL OR pr.PayrollPeriodID = @PayrollPeriodID)
  AND (@Department = N'' OR s.Department = @Department)
  AND (@Status = N'' OR pr.PaymentStatus = @Status);";

            return ExecuteDataTable(sql, new[]
            {
                CreateNullableIntParameter("@PayrollPeriodID", payrollPeriodId),
                CreateNVarCharParameter("@Department", 100, department),
                CreateNVarCharParameter("@Status", 30, status)
            });
        }

        /// <summary>Per-department payroll totals for reporting.</summary>
        public DataTable GetPayrollByDepartment(int? payrollPeriodId, string status)
        {
            ValidateOptionalText(status, 30, "status");

            const string sql = @"
SELECT
    ISNULL(NULLIF(s.Department, N''), N'(Unspecified)') AS Department,
    COUNT_BIG(1) AS Records,
    ISNULL(SUM(pr.GrossSalary), 0) AS Gross,
    ISNULL(SUM(pr.TotalDeductions), 0) AS Deductions,
    ISNULL(SUM(pr.NetSalary), 0) AS Net,
    ISNULL(SUM(CASE WHEN pr.PaymentStatus = N'Paid' THEN pr.NetSalary ELSE 0 END), 0) AS Paid
FROM dbo.PayrollRecords AS pr
INNER JOIN dbo.Staff AS s ON s.StaffID = pr.StaffID
WHERE (@PayrollPeriodID IS NULL OR pr.PayrollPeriodID = @PayrollPeriodID)
  AND (@Status = N'' OR pr.PaymentStatus = @Status)
GROUP BY ISNULL(NULLIF(s.Department, N''), N'(Unspecified)')
ORDER BY Department;";

            return ExecuteDataTable(sql, new[]
            {
                CreateNullableIntParameter("@PayrollPeriodID", payrollPeriodId),
                CreateNVarCharParameter("@Status", 30, status)
            });
        }

        public int GeneratePayroll(
            int payrollPeriodId,
            int? currentUserId = null)
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        DateTime endDate;
                        string periodStatus;

                        const string periodSql = @"
SELECT EndDate, Status
FROM dbo.PayrollPeriods WITH (UPDLOCK, HOLDLOCK)
WHERE PayrollPeriodID = @PayrollPeriodID;";

                        using (SqlCommand command = new SqlCommand(
                            periodSql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollPeriodID", SqlDbType.Int).Value =
                                payrollPeriodId;

                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new InvalidOperationException(
                                        "The payroll period was not found.");
                                }

                                endDate = Convert.ToDateTime(reader["EndDate"]);
                                periodStatus = Convert.ToString(reader["Status"]);
                            }
                        }

                        if (periodStatus.Equals(
                                "Completed",
                                StringComparison.OrdinalIgnoreCase) ||
                            periodStatus.Equals(
                                "Cancelled",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(
                                "Payroll cannot be generated for a Completed or Cancelled period.");
                        }

                        const string missingSql = @"
SELECT COUNT_BIG(1)
FROM dbo.Staff AS s WITH (UPDLOCK, HOLDLOCK)
WHERE s.Status = N'Active'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.PayrollRecords AS pr WITH (UPDLOCK, HOLDLOCK)
      WHERE pr.PayrollPeriodID = @PayrollPeriodID
        AND pr.StaffID = s.StaffID
  )
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.StaffSalaryStructures AS ss WITH (UPDLOCK, HOLDLOCK)
      WHERE ss.StaffID = s.StaffID
        AND ss.Status = N'Active'
        AND ss.EffectiveFrom <= @PeriodEndDate
        AND (ss.EffectiveTo IS NULL
             OR ss.EffectiveTo >= @PeriodEndDate)
  );";

                        long missingCount;

                        using (SqlCommand command = new SqlCommand(
                            missingSql, connection, transaction))
                        {
                            AddGenerationParameters(
                                command,
                                payrollPeriodId,
                                endDate,
                                currentUserId);

                            missingCount = Convert.ToInt64(
                                command.ExecuteScalar());
                        }

                        if (missingCount > 0)
                        {
                            throw new InvalidOperationException(
                                missingCount +
                                " active staff member(s) do not have an applicable active salary structure. Payroll generation was cancelled.");
                        }

                        const string negativeSalarySql = @"
SELECT COUNT_BIG(1)
FROM dbo.Staff AS s WITH (UPDLOCK, HOLDLOCK)
CROSS APPLY
(
    SELECT TOP (1)
        ss.BasicSalary,
        ss.HousingAllowance,
        ss.TransportAllowance,
        ss.OtherAllowance,
        ss.TaxDeduction,
        ss.OtherDeduction
    FROM dbo.StaffSalaryStructures AS ss WITH (UPDLOCK, HOLDLOCK)
    WHERE ss.StaffID = s.StaffID
      AND ss.Status = N'Active'
      AND ss.EffectiveFrom <= @PeriodEndDate
      AND (ss.EffectiveTo IS NULL
           OR ss.EffectiveTo >= @PeriodEndDate)
    ORDER BY ss.EffectiveFrom DESC, ss.SalaryStructureID DESC
) AS selectedStructure
WHERE s.Status = N'Active'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.PayrollRecords AS pr WITH (UPDLOCK, HOLDLOCK)
      WHERE pr.PayrollPeriodID = @PayrollPeriodID
        AND pr.StaffID = s.StaffID
  )
  AND
  (
      selectedStructure.BasicSalary
      + selectedStructure.HousingAllowance
      + selectedStructure.TransportAllowance
      + selectedStructure.OtherAllowance
      - selectedStructure.TaxDeduction
      - selectedStructure.OtherDeduction
  ) < 0;";

                        long negativeSalaryCount;

                        using (SqlCommand command = new SqlCommand(
                            negativeSalarySql, connection, transaction))
                        {
                            AddGenerationParameters(
                                command,
                                payrollPeriodId,
                                endDate,
                                currentUserId);

                            negativeSalaryCount = Convert.ToInt64(
                                command.ExecuteScalar());
                        }

                        if (negativeSalaryCount > 0)
                        {
                            throw new InvalidOperationException(
                                "Payroll generation would create a negative net salary for " +
                                negativeSalaryCount + " staff member(s).");
                        }

                        const string insertSql = @"
;WITH ApplicableStructures AS
(
    SELECT
        s.StaffID,
        ss.BasicSalary,
        ss.HousingAllowance,
        ss.TransportAllowance,
        ss.OtherAllowance,
        ss.TaxDeduction,
        ss.OtherDeduction,
        ROW_NUMBER() OVER
        (
            PARTITION BY s.StaffID
            ORDER BY ss.EffectiveFrom DESC,
                     ss.SalaryStructureID DESC
        ) AS RowNumber
    FROM dbo.Staff AS s WITH (UPDLOCK, HOLDLOCK)
    INNER JOIN dbo.StaffSalaryStructures AS ss WITH (UPDLOCK, HOLDLOCK)
        ON ss.StaffID = s.StaffID
       AND ss.Status = N'Active'
       AND ss.EffectiveFrom <= @PeriodEndDate
       AND (ss.EffectiveTo IS NULL
            OR ss.EffectiveTo >= @PeriodEndDate)
    WHERE s.Status = N'Active'
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.PayrollRecords AS existingRecord
               WITH (UPDLOCK, HOLDLOCK)
          WHERE existingRecord.PayrollPeriodID = @PayrollPeriodID
            AND existingRecord.StaffID = s.StaffID
      )
)
INSERT INTO dbo.PayrollRecords
(
    PayrollPeriodID,
    StaffID,
    BasicSalary,
    HousingAllowance,
    TransportAllowance,
    OtherAllowance,
    TaxDeduction,
    OtherDeduction,
    TotalAllowances,
    BonusAmount,
    TotalDeductions,
    GrossSalary,
    NetSalary,
    PaymentStatus,
    PaymentMethod,
    PaymentReference,
    PaidDate,
    Notes,
    CreatedBy,
    UpdatedBy,
    CreatedAt,
    UpdatedAt
)
SELECT
    @PayrollPeriodID,
    StaffID,
    BasicSalary,
    HousingAllowance,
    TransportAllowance,
    OtherAllowance,
    TaxDeduction,
    OtherDeduction,
    HousingAllowance + TransportAllowance + OtherAllowance,
    CONVERT(DECIMAL(18,2), 0),
    TaxDeduction + OtherDeduction,
    BasicSalary + HousingAllowance + TransportAllowance + OtherAllowance,
    BasicSalary + HousingAllowance + TransportAllowance + OtherAllowance
        - TaxDeduction - OtherDeduction,
    N'Pending',
    NULL,
    NULL,
    NULL,
    NULL,
    @CurrentUserID,
    NULL,
    SYSUTCDATETIME(),
    NULL
FROM ApplicableStructures
WHERE RowNumber = 1;

SELECT @@ROWCOUNT;";

                        int insertedCount;

                        using (SqlCommand command = new SqlCommand(
                            insertSql, connection, transaction))
                        {
                            AddGenerationParameters(
                                command,
                                payrollPeriodId,
                                endDate,
                                currentUserId);

                            insertedCount = Convert.ToInt32(
                                command.ExecuteScalar());
                        }

                        const string updatePeriodSql = @"
UPDATE dbo.PayrollPeriods
SET
    Status = N'Processing',
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE PayrollPeriodID = @PayrollPeriodID
  AND Status = N'Draft';";

                        using (SqlCommand command = new SqlCommand(
                            updatePeriodSql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollPeriodID", SqlDbType.Int).Value =
                                payrollPeriodId;
                            command.Parameters.Add(
                                "@CurrentUserID", SqlDbType.Int).Value =
                                ToDatabaseValue(currentUserId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return insertedCount;
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Creates payroll records for an explicit set of active staff, snapshotting
        /// BasicSalary directly from Staff.Salary and applying the manual monthly
        /// components entered in the Create Pay Run wizard. Housing/Transport
        /// allowances are always stored as 0. Runs in one serializable transaction,
        /// skips staff already having a record for the period (duplicate prevention),
        /// and rejects negative net salary.
        /// </summary>
        public int GeneratePayRun(
            int payrollPeriodId,
            IList<PayRunStaffComponent> staff,
            string defaultPaymentMethod,
            int? currentUserId = null)
        {
            if (staff == null || staff.Count == 0)
            {
                throw new InvalidOperationException(
                    "Select at least one employee for the pay run.");
            }

            string paymentMethod = string.IsNullOrWhiteSpace(defaultPaymentMethod)
                ? "Bank Transfer"
                : defaultPaymentMethod.Trim();

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        // Lock the period and validate its status.
                        string periodStatus;
                        using (SqlCommand command = new SqlCommand(
                            "SELECT Status FROM dbo.PayrollPeriods WITH (UPDLOCK, HOLDLOCK) WHERE PayrollPeriodID = @PayrollPeriodID;",
                            connection, transaction))
                        {
                            command.Parameters.Add("@PayrollPeriodID", SqlDbType.Int).Value = payrollPeriodId;
                            object result = command.ExecuteScalar();
                            if (result == null)
                            {
                                throw new InvalidOperationException("The payroll period was not found.");
                            }
                            periodStatus = Convert.ToString(result);
                        }

                        if (periodStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                            periodStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(
                                "Payroll cannot be generated for a Completed or Cancelled period.");
                        }

                        int insertedCount = 0;

                        foreach (PayRunStaffComponent component in staff)
                        {
                            // Load and lock the staff member; snapshot Basic Salary from Staff.Salary.
                            decimal basicSalary;
                            string status;
                            using (SqlCommand command = new SqlCommand(
                                "SELECT Salary, Status FROM dbo.Staff WITH (UPDLOCK, HOLDLOCK) WHERE StaffID = @StaffID;",
                                connection, transaction))
                            {
                                command.Parameters.Add("@StaffID", SqlDbType.Int).Value = component.StaffID;
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (!reader.Read())
                                    {
                                        continue; // staff no longer exists — skip
                                    }
                                    basicSalary = reader["Salary"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["Salary"]);
                                    status = Convert.ToString(reader["Status"]);
                                }
                            }

                            if (!string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                            {
                                continue; // only active staff are paid
                            }

                            // Skip if a record already exists for this staff + period (duplicate prevention).
                            using (SqlCommand command = new SqlCommand(
                                "SELECT COUNT_BIG(1) FROM dbo.PayrollRecords WITH (UPDLOCK, HOLDLOCK) WHERE PayrollPeriodID = @PayrollPeriodID AND StaffID = @StaffID;",
                                connection, transaction))
                            {
                                command.Parameters.Add("@PayrollPeriodID", SqlDbType.Int).Value = payrollPeriodId;
                                command.Parameters.Add("@StaffID", SqlDbType.Int).Value = component.StaffID;
                                if (Convert.ToInt64(command.ExecuteScalar()) > 0)
                                {
                                    continue;
                                }
                            }

                            decimal otherAllowance = Math.Max(0m, component.OtherAllowance);
                            decimal bonus = Math.Max(0m, component.Bonus);
                            decimal taxDeduction = Math.Max(0m, component.TaxDeduction);
                            decimal otherDeduction = Math.Max(0m, component.OtherDeduction);
                            if (basicSalary < 0m) basicSalary = 0m;

                            decimal totalAllowances = otherAllowance;
                            decimal grossSalary = basicSalary + otherAllowance + bonus;
                            decimal totalDeductions = taxDeduction + otherDeduction;
                            decimal netSalary = grossSalary - totalDeductions;

                            if (netSalary < 0m)
                            {
                                throw new InvalidOperationException(
                                    "Deductions exceed gross salary for one or more employees. Net salary cannot be negative.");
                            }

                            const string insertSql = @"
INSERT INTO dbo.PayrollRecords
(
    PayrollPeriodID, StaffID, BasicSalary, HousingAllowance, TransportAllowance,
    OtherAllowance, TaxDeduction, OtherDeduction, TotalAllowances, BonusAmount,
    TotalDeductions, GrossSalary, NetSalary, PaymentStatus, PaymentMethod,
    PaymentReference, PaidDate, Notes, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt
)
VALUES
(
    @PayrollPeriodID, @StaffID, @BasicSalary, 0, 0,
    @OtherAllowance, @TaxDeduction, @OtherDeduction, @TotalAllowances, @BonusAmount,
    @TotalDeductions, @GrossSalary, @NetSalary, N'Pending', @PaymentMethod,
    NULL, NULL, NULL, @CurrentUserID, NULL, SYSUTCDATETIME(), NULL
);";

                            using (SqlCommand command = new SqlCommand(insertSql, connection, transaction))
                            {
                                command.Parameters.Add("@PayrollPeriodID", SqlDbType.Int).Value = payrollPeriodId;
                                command.Parameters.Add("@StaffID", SqlDbType.Int).Value = component.StaffID;
                                command.Parameters.Add("@BasicSalary", SqlDbType.Decimal).Value = basicSalary;
                                command.Parameters.Add("@OtherAllowance", SqlDbType.Decimal).Value = otherAllowance;
                                command.Parameters.Add("@TaxDeduction", SqlDbType.Decimal).Value = taxDeduction;
                                command.Parameters.Add("@OtherDeduction", SqlDbType.Decimal).Value = otherDeduction;
                                command.Parameters.Add("@TotalAllowances", SqlDbType.Decimal).Value = totalAllowances;
                                command.Parameters.Add("@BonusAmount", SqlDbType.Decimal).Value = bonus;
                                command.Parameters.Add("@TotalDeductions", SqlDbType.Decimal).Value = totalDeductions;
                                command.Parameters.Add("@GrossSalary", SqlDbType.Decimal).Value = grossSalary;
                                command.Parameters.Add("@NetSalary", SqlDbType.Decimal).Value = netSalary;
                                command.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = paymentMethod;
                                command.Parameters.Add("@CurrentUserID", SqlDbType.Int).Value = ToDatabaseValue(currentUserId);
                                command.ExecuteNonQuery();
                                insertedCount++;
                            }
                        }

                        // Move the period into Processing when it is still a Draft.
                        using (SqlCommand command = new SqlCommand(
                            "UPDATE dbo.PayrollPeriods SET Status = N'Processing', UpdatedBy = @CurrentUserID, UpdatedAt = SYSUTCDATETIME() WHERE PayrollPeriodID = @PayrollPeriodID AND Status = N'Draft';",
                            connection, transaction))
                        {
                            command.Parameters.Add("@PayrollPeriodID", SqlDbType.Int).Value = payrollPeriodId;
                            command.Parameters.Add("@CurrentUserID", SqlDbType.Int).Value = ToDatabaseValue(currentUserId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return insertedCount;
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public DataRow GetPayrollRecord(int payrollRecordId)
        {
            const string sql = @"
SELECT
    pr.PayrollRecordID,
    pr.PayrollPeriodID,
    pp.PeriodName,
    pp.StartDate,
    pp.EndDate,
    pp.PaymentDate,
    pr.StaffID,
    s.EmployeeID,
    s.Department,
    s.Position,
    pr.BasicSalary,
    pr.HousingAllowance,
    pr.TransportAllowance,
    pr.OtherAllowance,
    pr.TaxDeduction,
    pr.OtherDeduction,
    pr.TotalAllowances,
    pr.BonusAmount,
    pr.TotalDeductions,
    pr.GrossSalary,
    pr.NetSalary,
    pr.PaymentStatus,
    pr.PaymentMethod,
    pr.PaymentReference,
    pr.PaidDate,
    pr.Notes,
    pr.CreatedBy,
    pr.UpdatedBy,
    pr.CreatedAt,
    pr.UpdatedAt
FROM dbo.PayrollRecords AS pr
INNER JOIN dbo.PayrollPeriods AS pp
    ON pp.PayrollPeriodID = pr.PayrollPeriodID
INNER JOIN dbo.Staff AS s
    ON s.StaffID = pr.StaffID
WHERE pr.PayrollRecordID = @PayrollRecordID;";

            DataTable table = ExecuteDataTable(
                sql,
                new[]
                {
                    new SqlParameter("@PayrollRecordID", SqlDbType.Int)
                    {
                        Value = payrollRecordId
                    }
                });

            return table.Rows.Count == 0 ? null : table.Rows[0];
        }

        public DataTable GetPayrollAdjustments(int payrollRecordId)
        {
            const string sql = @"
SELECT
    PayrollAdjustmentID,
    PayrollRecordID,
    AdjustmentType,
    AdjustmentName,
    Amount,
    Notes,
    CreatedBy,
    CreatedAt
FROM dbo.PayrollAdjustments
WHERE PayrollRecordID = @PayrollRecordID
ORDER BY CreatedAt, PayrollAdjustmentID;";

            return ExecuteDataTable(
                sql,
                new[]
                {
                    new SqlParameter("@PayrollRecordID", SqlDbType.Int)
                    {
                        Value = payrollRecordId
                    }
                });
        }

        public int AddPayrollAdjustment(
            PayrollAdjustmentInput input,
            int? currentUserId = null)
        {
            ValidateAdjustmentInput(input);

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        EnsurePayrollRecordCanBeChanged(
                            connection, transaction, input.PayrollRecordID);

                        const string insertSql = @"
INSERT INTO dbo.PayrollAdjustments
(
    PayrollRecordID,
    AdjustmentType,
    AdjustmentName,
    Amount,
    Notes,
    CreatedBy,
    CreatedAt
)
OUTPUT INSERTED.PayrollAdjustmentID
VALUES
(
    @PayrollRecordID,
    @AdjustmentType,
    @AdjustmentName,
    @Amount,
    @Notes,
    @CurrentUserID,
    SYSUTCDATETIME()
);";

                        int adjustmentId;

                        using (SqlCommand command = new SqlCommand(
                            insertSql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollRecordID", SqlDbType.Int).Value =
                                input.PayrollRecordID;
                            command.Parameters.Add(
                                "@AdjustmentType",
                                SqlDbType.NVarChar,
                                20).Value =
                                NormalizeAdjustmentType(input.AdjustmentType);
                            command.Parameters.Add(
                                "@AdjustmentName",
                                SqlDbType.NVarChar,
                                100).Value = input.AdjustmentName.Trim();
                            AddMoneyParameter(command, "@Amount", input.Amount);
                            command.Parameters.Add(
                                "@Notes", SqlDbType.NVarChar, 500).Value =
                                string.IsNullOrWhiteSpace(input.Notes)
                                    ? (object)DBNull.Value
                                    : input.Notes.Trim();
                            command.Parameters.Add(
                                "@CurrentUserID", SqlDbType.Int).Value =
                                ToDatabaseValue(currentUserId);

                            adjustmentId = Convert.ToInt32(
                                command.ExecuteScalar());
                        }

                        RecalculatePayrollRecord(
                            connection,
                            transaction,
                            input.PayrollRecordID,
                            currentUserId);

                        transaction.Commit();
                        return adjustmentId;
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public void DeletePayrollAdjustment(
            int payrollAdjustmentId,
            int payrollRecordId,
            int? currentUserId = null)
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        EnsurePayrollRecordCanBeChanged(
                            connection, transaction, payrollRecordId);

                        const string deleteSql = @"
DELETE FROM dbo.PayrollAdjustments
WHERE PayrollAdjustmentID = @PayrollAdjustmentID
  AND PayrollRecordID = @PayrollRecordID;

IF @@ROWCOUNT = 0
    THROW 51000, 'The payroll adjustment was not found.', 1;";

                        using (SqlCommand command = new SqlCommand(
                            deleteSql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollAdjustmentID", SqlDbType.Int).Value =
                                payrollAdjustmentId;
                            command.Parameters.Add(
                                "@PayrollRecordID", SqlDbType.Int).Value =
                                payrollRecordId;
                            command.ExecuteNonQuery();
                        }

                        RecalculatePayrollRecord(
                            connection,
                            transaction,
                            payrollRecordId,
                            currentUserId);

                        transaction.Commit();
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public void UpdatePayrollNotes(
            int payrollRecordId,
            string notes,
            int? currentUserId = null)
        {
            ValidateOptionalText(notes, 500, "notes");

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        EnsurePayrollRecordCanBeChanged(
                            connection, transaction, payrollRecordId);

                        const string sql = @"
UPDATE dbo.PayrollRecords
SET
    Notes = @Notes,
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE PayrollRecordID = @PayrollRecordID;

IF @@ROWCOUNT = 0
    THROW 51000, 'The payroll record was not found.', 1;";

                        using (SqlCommand command = new SqlCommand(
                            sql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollRecordID", SqlDbType.Int).Value =
                                payrollRecordId;
                            command.Parameters.Add(
                                "@Notes", SqlDbType.NVarChar, 500).Value =
                                string.IsNullOrWhiteSpace(notes)
                                    ? (object)DBNull.Value
                                    : notes.Trim();
                            command.Parameters.Add(
                                "@CurrentUserID", SqlDbType.Int).Value =
                                ToDatabaseValue(currentUserId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public void MarkPayrollPaid(
            int payrollRecordId,
            string paymentMethod,
            string paymentReference,
            DateTime paidDate,
            int? currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException(
                    "Payment method is required.", "paymentMethod");
            }

            ValidateRequiredText(
                paymentMethod, 30, "Payment method", "paymentMethod");
            ValidateOptionalText(
                paymentReference, 100, "paymentReference");

            if (paidDate == DateTime.MinValue)
            {
                throw new ArgumentException(
                    "A valid paid date is required.", "paidDate");
            }

            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        const string sql = @"
DECLARE @PaymentStatus NVARCHAR(20);
DECLARE @PeriodStatus NVARCHAR(20);

SELECT
    @PaymentStatus = pr.PaymentStatus,
    @PeriodStatus = pp.Status
FROM dbo.PayrollRecords AS pr WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dbo.PayrollPeriods AS pp WITH (UPDLOCK, HOLDLOCK)
    ON pp.PayrollPeriodID = pr.PayrollPeriodID
WHERE pr.PayrollRecordID = @PayrollRecordID;

IF @PaymentStatus IS NULL
    THROW 51000, 'The payroll record was not found.', 1;

IF @PaymentStatus NOT IN (N'Pending', N'Failed')
    THROW 51000, 'Only Pending or Failed payroll records can be marked as Paid.', 1;

IF @PeriodStatus <> N'Processing'
    THROW 51000, 'Payroll can be marked as Paid only while its period is Processing.', 1;

UPDATE dbo.PayrollRecords
SET
    PaymentStatus = N'Paid',
    PaymentMethod = @PaymentMethod,
    PaymentReference = @PaymentReference,
    PaidDate = @PaidDate,
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE PayrollRecordID = @PayrollRecordID;";

                        using (SqlCommand command = new SqlCommand(
                            sql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollRecordID", SqlDbType.Int).Value =
                                payrollRecordId;
                            command.Parameters.Add(
                                "@PaymentMethod",
                                SqlDbType.NVarChar,
                                30).Value = paymentMethod.Trim();
                            command.Parameters.Add(
                                "@PaymentReference",
                                SqlDbType.NVarChar,
                                100).Value =
                                string.IsNullOrWhiteSpace(paymentReference)
                                    ? (object)DBNull.Value
                                    : paymentReference.Trim();
                            command.Parameters.Add(
                                "@PaidDate", SqlDbType.DateTime2).Value =
                                paidDate;
                            command.Parameters.Add(
                                "@CurrentUserID", SqlDbType.Int).Value =
                                ToDatabaseValue(currentUserId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public void SetPayrollPaymentFailed(
            int payrollRecordId,
            int? currentUserId = null)
        {
            using (SqlConnection connection = CreateConnection())
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        const string sql = @"
DECLARE @PaymentStatus NVARCHAR(20);
DECLARE @PeriodStatus NVARCHAR(20);

SELECT
    @PaymentStatus = pr.PaymentStatus,
    @PeriodStatus = pp.Status
FROM dbo.PayrollRecords AS pr WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dbo.PayrollPeriods AS pp WITH (UPDLOCK, HOLDLOCK)
    ON pp.PayrollPeriodID = pr.PayrollPeriodID
WHERE pr.PayrollRecordID = @PayrollRecordID;

IF @PaymentStatus IS NULL
    THROW 51000, 'The payroll record was not found.', 1;

IF @PaymentStatus <> N'Pending'
    THROW 51000, 'Only a Pending payroll record can be marked as Failed.', 1;

IF @PeriodStatus <> N'Processing'
    THROW 51000, 'Payroll can be marked as Failed only while its period is Processing.', 1;

UPDATE dbo.PayrollRecords
SET
    PaymentStatus = N'Failed',
    PaymentMethod = NULL,
    PaymentReference = NULL,
    PaidDate = NULL,
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE PayrollRecordID = @PayrollRecordID;";

                        using (SqlCommand command = new SqlCommand(
                            sql, connection, transaction))
                        {
                            command.Parameters.Add(
                                "@PayrollRecordID", SqlDbType.Int).Value =
                                payrollRecordId;
                            command.Parameters.Add(
                                "@CurrentUserID", SqlDbType.Int).Value =
                                ToDatabaseValue(currentUserId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        RollbackSafely(transaction);
                        throw;
                    }
                }
            }
        }

        public string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return string.Empty;
            }

            return role.Trim()
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private DataTable ExecuteDataTable(
            string commandText,
            SqlParameter[] parameters)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = CreateConnection())
            using (SqlCommand command = new SqlCommand(
                commandText, connection))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        private static void AddPayrollPeriodParameters(
            SqlCommand command,
            PayrollPeriodInput input,
            int? currentUserId)
        {
            command.Parameters.Add(
                "@PayrollPeriodID", SqlDbType.Int).Value =
                input.PayrollPeriodID.HasValue
                    ? (object)input.PayrollPeriodID.Value
                    : 0;
            command.Parameters.Add(
                "@PeriodName", SqlDbType.NVarChar, 100).Value =
                input.PeriodName.Trim();
            command.Parameters.Add(
                "@StartDate", SqlDbType.Date).Value =
                input.StartDate.Date;
            command.Parameters.Add(
                "@EndDate", SqlDbType.Date).Value =
                input.EndDate.Date;
            command.Parameters.Add(
                "@PaymentDate", SqlDbType.Date).Value =
                input.PaymentDate.HasValue
                    ? (object)input.PaymentDate.Value.Date
                    : DBNull.Value;
            command.Parameters.Add(
                "@CurrentUserID", SqlDbType.Int).Value =
                ToDatabaseValue(currentUserId);
        }

        private static void AddSalaryStructureParameters(
            SqlCommand command,
            SalaryStructureInput input,
            int? currentUserId)
        {
            command.Parameters.Add(
                "@SalaryStructureID", SqlDbType.Int).Value =
                input.SalaryStructureID.HasValue
                    ? (object)input.SalaryStructureID.Value
                    : 0;
            command.Parameters.Add(
                "@StaffID", SqlDbType.Int).Value = input.StaffID;
            AddMoneyParameter(
                command, "@BasicSalary", input.BasicSalary);
            AddMoneyParameter(
                command, "@HousingAllowance", input.HousingAllowance);
            AddMoneyParameter(
                command, "@TransportAllowance", input.TransportAllowance);
            AddMoneyParameter(
                command, "@OtherAllowance", input.OtherAllowance);
            AddMoneyParameter(
                command, "@TaxDeduction", input.TaxDeduction);
            AddMoneyParameter(
                command, "@OtherDeduction", input.OtherDeduction);
            command.Parameters.Add(
                "@EffectiveFrom", SqlDbType.Date).Value =
                input.EffectiveFrom.Date;
            command.Parameters.Add(
                "@EffectiveTo", SqlDbType.Date).Value =
                input.EffectiveTo.HasValue
                    ? (object)input.EffectiveTo.Value.Date
                    : DBNull.Value;
            command.Parameters.Add(
                "@Status", SqlDbType.NVarChar, 20).Value =
                NormalizeSalaryStructureStatus(input.Status);
            command.Parameters.Add(
                "@CurrentUserID", SqlDbType.Int).Value =
                ToDatabaseValue(currentUserId);
        }

        private static void AddPayrollListFilterParameters(
            SqlCommand command,
            PayrollListFilter filter)
        {
            command.Parameters.Add(
                CreateNullableIntParameter(
                    "@PayrollPeriodID", filter.PayrollPeriodId));
            command.Parameters.Add(
                CreateNVarCharParameter(
                    "@Department", 100, filter.Department));
            command.Parameters.Add(
                CreateNVarCharParameter(
                    "@PaymentStatus", 20, filter.PaymentStatus));
            command.Parameters.Add(
                CreateNVarCharParameter(
                    "@Search", 150, filter.Search));
        }

        private static void AddGenerationParameters(
            SqlCommand command,
            int payrollPeriodId,
            DateTime periodEndDate,
            int? currentUserId)
        {
            command.Parameters.Add(
                "@PayrollPeriodID", SqlDbType.Int).Value =
                payrollPeriodId;
            command.Parameters.Add(
                "@PeriodEndDate", SqlDbType.Date).Value =
                periodEndDate.Date;
            command.Parameters.Add(
                "@CurrentUserID", SqlDbType.Int).Value =
                ToDatabaseValue(currentUserId);
        }

        private static SqlParameter CreateNullableIntParameter(
            string name,
            int? value)
        {
            return new SqlParameter(name, SqlDbType.Int)
            {
                Value = ToDatabaseValue(value)
            };
        }

        private static SqlParameter CreateNVarCharParameter(
            string name,
            int size,
            string value)
        {
            return new SqlParameter(name, SqlDbType.NVarChar, size)
            {
                Value = string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : value.Trim()
            };
        }

        private static void AddMoneyParameter(
            SqlCommand command,
            string name,
            decimal value)
        {
            SqlParameter parameter = command.Parameters.Add(
                name, SqlDbType.Decimal);
            parameter.Precision = 18;
            parameter.Scale = 2;
            parameter.Value = value;
        }

        private static object ToDatabaseValue(int? value)
        {
            return value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static string GetPayrollSortExpression(string sortColumn)
        {
            switch ((sortColumn ?? string.Empty).Trim())
            {
                case "PeriodName":
                    return "pp.PeriodName";
                case "EmployeeID":
                    return "s.EmployeeID";
                case "Department":
                    return "s.Department";
                case "Position":
                    return "s.Position";
                case "BasicSalary":
                    return "pr.BasicSalary";
                case "GrossSalary":
                    return "pr.GrossSalary";
                case "TotalDeductions":
                    return "pr.TotalDeductions";
                case "NetSalary":
                    return "pr.NetSalary";
                case "PaymentStatus":
                    return "pr.PaymentStatus";
                case "PaidDate":
                    return "pr.PaidDate";
                default:
                    return "pp.StartDate";
            }
        }

        private static void EnsurePayrollRecordCanBeChanged(
            SqlConnection connection,
            SqlTransaction transaction,
            int payrollRecordId)
        {
            const string sql = @"
DECLARE @PaymentStatus NVARCHAR(20);
DECLARE @PeriodStatus NVARCHAR(20);

SELECT
    @PaymentStatus = pr.PaymentStatus,
    @PeriodStatus = pp.Status
FROM dbo.PayrollRecords AS pr WITH (UPDLOCK, HOLDLOCK)
INNER JOIN dbo.PayrollPeriods AS pp WITH (UPDLOCK, HOLDLOCK)
    ON pp.PayrollPeriodID = pr.PayrollPeriodID
WHERE pr.PayrollRecordID = @PayrollRecordID;

IF @PaymentStatus IS NULL
    THROW 51000, 'The payroll record was not found.', 1;

IF @PaymentStatus = N'Paid'
    THROW 51000, 'Paid payroll records cannot be changed.', 1;

IF @PeriodStatus IN (N'Completed', N'Cancelled')
    THROW 51000, 'Payroll records in Completed or Cancelled periods cannot be changed.', 1;";

            using (SqlCommand command = new SqlCommand(
                sql, connection, transaction))
            {
                command.Parameters.Add(
                    "@PayrollRecordID", SqlDbType.Int).Value =
                    payrollRecordId;
                command.ExecuteNonQuery();
            }
        }

        private static void RecalculatePayrollRecord(
            SqlConnection connection,
            SqlTransaction transaction,
            int payrollRecordId,
            int? currentUserId)
        {
            const string sql = @"
DECLARE @AdjustmentAllowances DECIMAL(18,2);
DECLARE @AdjustmentDeductions DECIMAL(18,2);
DECLARE @AdjustmentBonuses DECIMAL(18,2);

SELECT
    @AdjustmentAllowances = ISNULL(SUM
    (
        CASE WHEN AdjustmentType = N'Allowance'
             THEN Amount ELSE 0 END
    ), 0),
    @AdjustmentDeductions = ISNULL(SUM
    (
        CASE WHEN AdjustmentType = N'Deduction'
             THEN Amount ELSE 0 END
    ), 0),
    @AdjustmentBonuses = ISNULL(SUM
    (
        CASE WHEN AdjustmentType = N'Bonus'
             THEN Amount ELSE 0 END
    ), 0)
FROM dbo.PayrollAdjustments WITH (UPDLOCK, HOLDLOCK)
WHERE PayrollRecordID = @PayrollRecordID;

DECLARE @BasicSalary DECIMAL(18,2);
DECLARE @HousingAllowance DECIMAL(18,2);
DECLARE @TransportAllowance DECIMAL(18,2);
DECLARE @OtherAllowance DECIMAL(18,2);
DECLARE @TaxDeduction DECIMAL(18,2);
DECLARE @OtherDeduction DECIMAL(18,2);

SELECT
    @BasicSalary = BasicSalary,
    @HousingAllowance = HousingAllowance,
    @TransportAllowance = TransportAllowance,
    @OtherAllowance = OtherAllowance,
    @TaxDeduction = TaxDeduction,
    @OtherDeduction = OtherDeduction
FROM dbo.PayrollRecords WITH (UPDLOCK, HOLDLOCK)
WHERE PayrollRecordID = @PayrollRecordID;

IF @BasicSalary IS NULL
    THROW 51000, 'The payroll record was not found.', 1;

DECLARE @TotalAllowances DECIMAL(18,2) =
    @HousingAllowance +
    @TransportAllowance +
    @OtherAllowance +
    @AdjustmentAllowances;

DECLARE @BonusAmount DECIMAL(18,2) =
    @AdjustmentBonuses;

DECLARE @TotalDeductions DECIMAL(18,2) =
    @TaxDeduction +
    @OtherDeduction +
    @AdjustmentDeductions;

DECLARE @GrossSalary DECIMAL(18,2) =
    @BasicSalary +
    @TotalAllowances +
    @BonusAmount;

DECLARE @NetSalary DECIMAL(18,2) =
    @GrossSalary -
    @TotalDeductions;

IF @NetSalary < 0
    THROW 51000, 'The adjustment would create a negative net salary.', 1;

UPDATE dbo.PayrollRecords
SET
    TotalAllowances = @TotalAllowances,
    BonusAmount = @BonusAmount,
    TotalDeductions = @TotalDeductions,
    GrossSalary = @GrossSalary,
    NetSalary = @NetSalary,
    UpdatedBy = @CurrentUserID,
    UpdatedAt = SYSUTCDATETIME()
WHERE PayrollRecordID = @PayrollRecordID;";

            using (SqlCommand command = new SqlCommand(
                sql, connection, transaction))
            {
                command.Parameters.Add(
                    "@PayrollRecordID", SqlDbType.Int).Value =
                    payrollRecordId;
                command.Parameters.Add(
                    "@CurrentUserID", SqlDbType.Int).Value =
                    ToDatabaseValue(currentUserId);
                command.ExecuteNonQuery();
            }
        }

        private static void ValidatePayrollPeriodInput(
            PayrollPeriodInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            ValidateRequiredText(
                input.PeriodName, 100, "Period name", "input");

            if (input.StartDate == DateTime.MinValue ||
                input.EndDate == DateTime.MinValue)
            {
                throw new ArgumentException(
                    "Valid start and end dates are required.", "input");
            }

            if (input.EndDate.Date < input.StartDate.Date)
            {
                throw new ArgumentException(
                    "End date cannot be earlier than start date.", "input");
            }

            if (input.PaymentDate.HasValue &&
                input.PaymentDate.Value.Date < input.StartDate.Date)
            {
                throw new ArgumentException(
                    "Payment date cannot be earlier than start date.", "input");
            }
        }

        private static void ValidateSalaryStructureInput(
            SalaryStructureInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.StaffID <= 0)
            {
                throw new ArgumentException(
                    "A valid staff member is required.", "input");
            }

            if (input.BasicSalary < 0 ||
                input.HousingAllowance < 0 ||
                input.TransportAllowance < 0 ||
                input.OtherAllowance < 0 ||
                input.TaxDeduction < 0 ||
                input.OtherDeduction < 0)
            {
                throw new ArgumentException(
                    "Salary amounts cannot be negative.", "input");
            }

            decimal grossSalary =
                input.BasicSalary +
                input.HousingAllowance +
                input.TransportAllowance +
                input.OtherAllowance;

            decimal deductions =
                input.TaxDeduction +
                input.OtherDeduction;

            if (grossSalary - deductions < 0)
            {
                throw new ArgumentException(
                    "The salary structure would create a negative net salary.",
                    "input");
            }

            if (input.EffectiveFrom == DateTime.MinValue)
            {
                throw new ArgumentException(
                    "A valid effective-from date is required.", "input");
            }

            if (input.EffectiveTo.HasValue &&
                input.EffectiveTo.Value.Date < input.EffectiveFrom.Date)
            {
                throw new ArgumentException(
                    "Effective-to date cannot be earlier than effective-from date.",
                    "input");
            }

            NormalizeSalaryStructureStatus(input.Status);
        }

        private static void ValidateAdjustmentInput(
            PayrollAdjustmentInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.PayrollRecordID <= 0)
            {
                throw new ArgumentException(
                    "A valid payroll record is required.", "input");
            }

            NormalizeAdjustmentType(input.AdjustmentType);
            ValidateRequiredText(
                input.AdjustmentName,
                100,
                "Adjustment name",
                "input");
            ValidateOptionalText(input.Notes, 500, "Notes");

            if (input.Amount <= 0)
            {
                throw new ArgumentException(
                    "Adjustment amount must be greater than zero.", "input");
            }
        }

        private static void ValidateRequiredText(
            string value,
            int maximumLength,
            string displayName,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    displayName + " is required.", parameterName);
            }

            if (value.Trim().Length > maximumLength)
            {
                throw new ArgumentException(
                    displayName + " cannot exceed " +
                    maximumLength + " characters.",
                    parameterName);
            }
        }

        private static void ValidateOptionalText(
            string value,
            int maximumLength,
            string parameterName)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                value.Trim().Length > maximumLength)
            {
                throw new ArgumentException(
                    parameterName + " cannot exceed " +
                    maximumLength + " characters.",
                    parameterName);
            }
        }

        private static string NormalizeAdjustmentType(string value)
        {
            string normalized = (value ?? string.Empty).Trim();

            if (normalized.Equals(
                "Allowance", StringComparison.OrdinalIgnoreCase))
            {
                return "Allowance";
            }

            if (normalized.Equals(
                "Deduction", StringComparison.OrdinalIgnoreCase))
            {
                return "Deduction";
            }

            if (normalized.Equals(
                "Bonus", StringComparison.OrdinalIgnoreCase))
            {
                return "Bonus";
            }

            throw new ArgumentException(
                "Adjustment type must be Allowance, Deduction, or Bonus.",
                "value");
        }

        private static string NormalizeSalaryStructureStatus(string value)
        {
            string normalized = (value ?? string.Empty).Trim();

            if (normalized.Equals(
                "Active", StringComparison.OrdinalIgnoreCase))
            {
                return "Active";
            }

            if (normalized.Equals(
                "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return "Inactive";
            }

            throw new ArgumentException(
                "Salary structure status must be Active or Inactive.",
                "value");
        }

        private static string NormalizePeriodStatus(string value)
        {
            string normalized = (value ?? string.Empty).Trim();

            if (normalized.Equals(
                "Draft", StringComparison.OrdinalIgnoreCase))
            {
                return "Draft";
            }

            if (normalized.Equals(
                "Processing", StringComparison.OrdinalIgnoreCase))
            {
                return "Processing";
            }

            if (normalized.Equals(
                "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return "Completed";
            }

            if (normalized.Equals(
                "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return "Cancelled";
            }

            throw new ArgumentException(
                "Invalid payroll period status.", "value");
        }

        private static bool IsValidPeriodTransition(
            string currentStatus,
            string newStatus)
        {
            return
                (string.Equals(
                     currentStatus,
                     "Draft",
                     StringComparison.OrdinalIgnoreCase) &&
                 (string.Equals(
                      newStatus,
                      "Processing",
                      StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(
                      newStatus,
                      "Cancelled",
                      StringComparison.OrdinalIgnoreCase))) ||
                (string.Equals(
                     currentStatus,
                     "Processing",
                     StringComparison.OrdinalIgnoreCase) &&
                 (string.Equals(
                      newStatus,
                      "Completed",
                      StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(
                      newStatus,
                      "Cancelled",
                      StringComparison.OrdinalIgnoreCase)));
        }

        private static void RollbackSafely(SqlTransaction transaction)
        {
            if (transaction == null)
            {
                return;
            }

            try
            {
                transaction.Rollback();
            }
            catch (InvalidOperationException)
            {
            }
            catch (SqlException)
            {
            }
        }
    }

    public sealed class PayrollPeriodInput
    {
        public int? PayrollPeriodID { get; set; }

        public string PeriodName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? PaymentDate { get; set; }
    }

    public sealed class SalaryStructureInput
    {
        public int? SalaryStructureID { get; set; }

        public int StaffID { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal HousingAllowance { get; set; }

        public decimal TransportAllowance { get; set; }

        public decimal OtherAllowance { get; set; }

        public decimal TaxDeduction { get; set; }

        public decimal OtherDeduction { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string Status { get; set; }
    }

    public sealed class PayrollAdjustmentInput
    {
        public int PayrollRecordID { get; set; }

        public string AdjustmentType { get; set; }

        public string AdjustmentName { get; set; }

        public decimal Amount { get; set; }

        public string Notes { get; set; }
    }

    /// <summary>
    /// Per-employee monthly payroll components entered in the Create Pay Run wizard.
    /// Basic Salary is not included here — it is snapshotted from Staff.Salary.
    /// </summary>
    public sealed class PayRunStaffComponent
    {
        public int StaffID { get; set; }
        public decimal OtherAllowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal TaxDeduction { get; set; }
        public decimal OtherDeduction { get; set; }
    }

    public sealed class PayrollListFilter
    {
        public int? PayrollPeriodId { get; set; }

        public string Department { get; set; }

        public string PaymentStatus { get; set; }

        public string Search { get; set; }
    }

    public sealed class PayrollListResult
    {
        public DataTable Records { get; set; }

        public long TotalRecords { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize <= 0 || TotalRecords <= 0)
                {
                    return 0;
                }

                return Convert.ToInt32(
                    Math.Ceiling(TotalRecords / (decimal)PageSize));
            }
        }
    }

    public sealed class PayrollPeriodData
    {
        public int PayrollPeriodID { get; set; }

        public string PeriodName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string Status { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Centralized payroll money formatting. Always renders United States dollars
    /// using the en-US culture so the display never depends on the Windows server
    /// culture. Stored decimal values are never converted — only the display text.
    /// </summary>
    public static class PayrollFormat
    {
        private static readonly System.Globalization.CultureInfo Usd =
            System.Globalization.CultureInfo.GetCultureInfo("en-US");

        // Culture-independent numeric format for GridView DataFormatString ("$1,000.00").
        public const string MoneyFormat = "{0:\\$#,##0.00}";

        public static string Money(decimal amount)
        {
            return amount.ToString("C2", Usd);
        }

        public static string Money(object value)
        {
            decimal amount = value == null || value == System.DBNull.Value
                ? 0m
                : System.Convert.ToDecimal(value);
            return Money(amount);
        }
    }
}