using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents school configuration settings
    /// </summary>
    public class SchoolSettings
    {
        public int SettingID { get; set; }
        public string SchoolName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
        public string Language { get; set; }
        public string LogoPath { get; set; }
        public int? CurrentAcademicYearID { get; set; }
        public string CurrentAcademicYearName { get; set; }
        public int? CurrentTermID { get; set; }
        public string CurrentTermName { get; set; }

        // Computed properties
        public string CurrencySymbol
        {
            get
            {
                switch (Currency)
                {
                    case "USD": return "$";
                    case "SOS": return "S";
                    case "KES": return "KSh";
                    default: return "$";
                }
            }
        }
    }
}