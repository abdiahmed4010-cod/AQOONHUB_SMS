using System;
using System.Collections.Generic;

namespace AQOONHUB.Models
{
    public class Guardian
    {
        private int _guardianID;
        private int? _userID;
        private string _fullName;
        private string _relationship;
        private string _phone;
        private string _email;
        private string _address;
        private bool _isActive;
        private List<string> _children;

        public Guardian()
        {
            _children = new List<string>();
        }

        public int GuardianID
        {
            get { return _guardianID; }
            set { _guardianID = value; }
        }

        public int? UserID
        {
            get { return _userID; }
            set { _userID = value; }
        }

        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

        public string Relationship
        {
            get { return _relationship; }
            set { _relationship = value; }
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public List<string> Children
        {
            get { return _children; }
            set { _children = value; }
        }
    }
}