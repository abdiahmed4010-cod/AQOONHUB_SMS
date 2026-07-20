using System;

namespace AQOONHUB_SMS.App_Code.Models
{
    public class DashboardStats
    {
        private int _totalStudents;
        private int _activeStudents;
        private int _suspendedStudents;
        private int _newAdmissions;
        private int _totalStaff;
        private int _activeStaff;
        private int _onLeaveStaff;
        private decimal _totalBilled;
        private decimal _totalCollected;
        private decimal _totalOutstanding;
        private int _presentToday;
        private int _absentToday;
        private int _lateToday;
        private int _upcomingExams;
        private int _activeExams;
        private int _pendingApplications;

        public int TotalStudents
        {
            get { return _totalStudents; }
            set { _totalStudents = value; }
        }

        public int ActiveStudents
        {
            get { return _activeStudents; }
            set { _activeStudents = value; }
        }

        public int SuspendedStudents
        {
            get { return _suspendedStudents; }
            set { _suspendedStudents = value; }
        }

        public int NewAdmissions
        {
            get { return _newAdmissions; }
            set { _newAdmissions = value; }
        }

        public int TotalStaff
        {
            get { return _totalStaff; }
            set { _totalStaff = value; }
        }

        public int ActiveStaff
        {
            get { return _activeStaff; }
            set { _activeStaff = value; }
        }

        public int OnLeaveStaff
        {
            get { return _onLeaveStaff; }
            set { _onLeaveStaff = value; }
        }

        public decimal TotalBilled
        {
            get { return _totalBilled; }
            set { _totalBilled = value; }
        }

        public decimal TotalCollected
        {
            get { return _totalCollected; }
            set { _totalCollected = value; }
        }

        public decimal TotalOutstanding
        {
            get { return _totalOutstanding; }
            set { _totalOutstanding = value; }
        }

        public decimal CollectionRate
        {
            get
            {
                if (_totalBilled > 0)
                    return (_totalCollected / _totalBilled) * 100;
                else
                    return 0;
            }
        }

        public int PresentToday
        {
            get { return _presentToday; }
            set { _presentToday = value; }
        }

        public int AbsentToday
        {
            get { return _absentToday; }
            set { _absentToday = value; }
        }

        public int LateToday
        {
            get { return _lateToday; }
            set { _lateToday = value; }
        }

        public decimal TodayAttendanceRate
        {
            get
            {
                int total = _presentToday + _absentToday + _lateToday;
                if (total > 0)
                    return ((decimal)_presentToday / total) * 100;
                else
                    return 0;
            }
        }

        public int UpcomingExams
        {
            get { return _upcomingExams; }
            set { _upcomingExams = value; }
        }

        public int ActiveExams
        {
            get { return _activeExams; }
            set { _activeExams = value; }
        }

        public int PendingApplications
        {
            get { return _pendingApplications; }
            set { _pendingApplications = value; }
        }
    }
}