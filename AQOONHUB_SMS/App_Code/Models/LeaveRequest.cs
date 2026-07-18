using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a staff leave request
    /// </summary>
    public class LeaveRequest
    {
        public int LeaveID { get; set; }
        public int StaffID { get; set; }
        public string StaffName { get; set; }
        public string EmployeeID { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public int? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "Approved": return "success";
                    case "Pending": return "warning";
                    case "Rejected": return "danger";
                    default: return "secondary";
                }
            }
        }

        public bool IsPending
        {
            get { return Status == "Pending"; }
        }

        public string DurationDisplay
        {
            get { return string.Format("{0:MMM dd} → {1:MMM dd} ({2} days)", StartDate, EndDate, Days); }
        }
    }
}