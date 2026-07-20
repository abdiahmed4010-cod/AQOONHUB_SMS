using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a student admission application
    /// </summary>
    public class Admission
    {
        public int AdmissionID { get; set; }
        public string ApplicationNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get { return string.Format("{0} {1}", FirstName, LastName); }
        }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int ApplyingForClassID { get; set; }
        public string ApplyingForClassName { get; set; }
        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianEmail { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
        public int? ReviewedBy { get; set; }
        public string ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string Notes { get; set; }

        // Computed properties
        public int Age
        {
            get
            {
                int age = DateTime.Now.Year - DateOfBirth.Year;
                if (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear) age--;
                return age;
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "Approved": return "success";
                    case "Pending": return "warning";
                    case "Under Review": return "info";
                    case "Rejected": return "danger";
                    default: return "secondary";
                }
            }
        }

        public int DaysSinceApplication
        {
            get { return (DateTime.Now - ApplicationDate).Days; }
        }
    }
}