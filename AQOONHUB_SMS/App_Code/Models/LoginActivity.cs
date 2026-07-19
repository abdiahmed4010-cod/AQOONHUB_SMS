using System;

namespace AQOONHUB.Models
{
    /// <summary>
    /// Represents a user login activity record
    /// </summary>
    public class LoginActivity
    {
        private int _loginActivityID;
        private int _userID;
        private DateTime _loginTime;
        private DateTime _logoutTime;
        private string _ipAddress;
        private string _device;
        private string _browser;
        private bool _isSuccessful;

        public int LoginActivityID
        {
            get { return _loginActivityID; }
            set { _loginActivityID = value; }
        }

        public int UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public DateTime LoginTime
        {
            get { return _loginTime; }
            set { _loginTime = value; }
        }

        public DateTime LogoutTime
        {
            get { return _logoutTime; }
            set { _logoutTime = value; }
        }

        public string IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public string Device
        {
            get { return _device; }
            set { _device = value; }
        }

        public string Browser
        {
            get { return _browser; }
            set { _browser = value; }
        }

        public bool IsSuccessful
        {
            get { return _isSuccessful; }
            set { _isSuccessful = value; }
        }

        public string StatusBadgeClass
        {
            get
            {
                if (_isSuccessful)
                    return "bg-success";
                else
                    return "bg-danger";
            }
        }

        public string StatusText
        {
            get
            {
                if (_isSuccessful)
                    return "Success";
                else
                    return "Failed";
            }
        }

        public string SessionDuration
        {
            get
            {
                if (_logoutTime == DateTime.MinValue)
                    return "Active";

                TimeSpan duration = _logoutTime.Subtract(_loginTime);
                if (duration.TotalHours >= 1)
                    return ((int)duration.TotalHours).ToString() + "h " + duration.Minutes.ToString() + "m";
                else
                    return duration.Minutes.ToString() + "m " + duration.Seconds.ToString() + "s";
            }
        }

        public bool IsActiveSession
        {
            get { return _logoutTime == DateTime.MinValue; }
        }
    }
}