using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    /// <summary>
    /// Represents a school announcement/notice
    /// </summary>
    public class Announcement
    {
        public int AnnouncementID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Audience { get; set; }
        public int? TargetClassID { get; set; }
        public string TargetClassName { get; set; }
        public bool IsPinned { get; set; }
        public int AuthorID { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties
        public string TimeAgo
        {
            get
            {
                TimeSpan diff = DateTime.Now - CreatedAt;
                if (diff.TotalMinutes < 60) return string.Format("{0}m ago", (int)diff.TotalMinutes);
                if (diff.TotalHours < 24) return string.Format("{0}h ago", (int)diff.TotalHours);
                if (diff.TotalDays < 7) return string.Format("{0}d ago", (int)diff.TotalDays);
                return CreatedAt.ToString("MMM dd");
            }
        }

        public string AudienceBadgeClass
        {
            get
            {
                switch (Audience)
                {
                    case "School-wide": return "primary";
                    case "Parents": return "success";
                    case "Staff": return "info";
                    default: return "secondary";
                }
            }
        }
    }
}