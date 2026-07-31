using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Attendance
{
    public partial class MarkAttendance : System.Web.UI.Page
    {
        private readonly AttendanceRepository _repo = new AttendanceRepository();

        private string Role { get { return Convert.ToString(Session["Role"]); } }
        private int UserId { get { int v; return int.TryParse(Convert.ToString(Session["UserID"]), out v) ? v : 0; } }

        // Prefilled statuses/times/remarks for the currently loaded session (used by Chk/TimeVal binders).
        private Dictionary<int, DataRow> _existing = new Dictionary<int, DataRow>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return false; }
            // Only markers reach this page; registrar/parent/student are redirected. (Fine-grained
            // scope authorization is enforced per action in the repository.)
            if (!_repo.CanMarkAttendance(Role)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return false; }
            return true;
        }

        // ---------- scope accessors ----------
        private int YearV() { int v; return int.TryParse(ddlYear.SelectedValue, out v) ? v : 0; }
        private int TermV() { int v; return int.TryParse(ddlTerm.SelectedValue, out v) ? v : 0; }
        private int ClassV() { int v; return int.TryParse(ddlClass.SelectedValue, out v) ? v : 0; }
        private int SectionV() { int v; return int.TryParse(ddlSection.SelectedValue, out v) ? v : 0; }
        private int SubjectV() { int v; return int.TryParse(ddlSubject.SelectedValue, out v) ? v : 0; }
        private string SessionType { get { return ddlSessionType.SelectedValue; } }
        private DateTime DateV()
        {
            DateTime d;
            return DateTime.TryParse(txtDate.Text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : DateTime.Today;
        }

        private AttendanceRepository.AttendanceScope Scope()
        {
            bool subject = string.Equals(SessionType, "Subject", StringComparison.OrdinalIgnoreCase);
            return new AttendanceRepository.AttendanceScope
            {
                AcademicYearID = YearV(),
                TermID = TermV() > 0 ? TermV() : (int?)null,
                AttendanceDate = DateV(),
                ClassID = ClassV(),
                SectionID = SectionV(),
                SubjectID = subject && SubjectV() > 0 ? SubjectV() : (int?)null,
                SessionType = SessionType
            };
        }

        // ---------- binders ----------
        protected string Chk(object studentId, string status)
        {
            int sid = Convert.ToInt32(studentId);
            if (_existing.ContainsKey(sid) && string.Equals(Convert.ToString(_existing[sid]["AttendanceStatus"]), status, StringComparison.OrdinalIgnoreCase))
                return "checked=\"checked\"";
            return "";
        }

        protected string TimeVal(object checkIn)
        {
            if (checkIn == null || checkIn == DBNull.Value) return "";
            TimeSpan ts = (TimeSpan)checkIn;
            return ts.ToString(@"hh\:mm");
        }

        private void BindYears()
        {
            ddlYear.Items.Clear();
            foreach (DataRow r in _repo.GetAcademicYears().Rows)
                ddlYear.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
            int a = _repo.GetActiveAcademicYearId();
            if (a > 0 && ddlYear.Items.FindByValue(a.ToString()) != null) ddlYear.SelectedValue = a.ToString();
        }
        private void BindTerms()
        {
            ddlTerm.Items.Clear(); ddlTerm.Items.Add(new ListItem("— None —", "0"));
            foreach (DataRow r in _repo.GetTerms(YearV()).Rows)
                ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));
        }
        private void BindClasses()
        {
            ddlClass.Items.Clear(); ddlClass.Items.Add(new ListItem("— Select class —", "0"));
            foreach (DataRow r in _repo.GetClasses(YearV()).Rows)
                ddlClass.Items.Add(new ListItem(Convert.ToString(r["ClassName"]), Convert.ToString(r["ClassID"])));
        }
        private void BindSections()
        {
            ddlSection.Items.Clear(); ddlSection.Items.Add(new ListItem("— Select section —", "0"));
            if (ClassV() > 0)
                foreach (DataRow r in _repo.GetSectionsForClass(ClassV()).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
        }
        private void BindSubjects()
        {
            ddlSubject.Items.Clear(); ddlSubject.Items.Add(new ListItem("— Select subject —", "0"));
            bool subject = string.Equals(SessionType, "Subject", StringComparison.OrdinalIgnoreCase);
            ddlSubject.Enabled = subject;
            if (subject && ClassV() > 0)
                foreach (DataRow r in _repo.GetSubjectsForClass(ClassV(), YearV()).Rows)
                    ddlSubject.Items.Add(new ListItem(Convert.ToString(r["SubjectName"]), Convert.ToString(r["SubjectID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); BindSubjects(); HideGrid(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); BindSubjects(); HideGrid(); }
        protected void ddlSessionType_Changed(object sender, EventArgs e) { BindSubjects(); HideGrid(); }
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlSessionType.SelectedValue = "Daily"; txtSearch.Text = "";
            BindYears(); BindTerms(); BindClasses(); BindSections(); BindSubjects(); HideGrid();
        }

        private void HideGrid() { pnlGrid.Visible = false; pnlSession.Visible = false; }

        // ---------- load ----------
        protected void btnLoad_Click(object sender, EventArgs e)
        {
            var sc = Scope();
            string err = _repo.ValidateAttendanceScope(sc);
            if (err != null) { HideGrid(); Show(false, err); return; }
            if (SectionV() <= 0) { HideGrid(); Show(false, "Select a section."); return; }
            if (!_repo.UserCanMarkAttendance(UserId, Role, sc))
            {
                HideGrid();
                // Managers/teachers only; teacher lacking assignment is rejected here (and again on save).
                Show(false, "You are not authorized to mark attendance for this class/section/subject.");
                return;
            }
            LoadGrid();
        }

        private void LoadGrid()
        {
            var sc = Scope();
            _existing.Clear();

            DataRow session = _repo.GetAttendanceSessionByScope(sc);
            bool submitted = false;
            if (session != null)
            {
                int sessionId = Convert.ToInt32(session["AttendanceSessionID"]);
                string status = Convert.ToString(session["Status"]);
                submitted = !status.Equals("Draft", StringComparison.OrdinalIgnoreCase);
                foreach (DataRow r in _repo.GetAttendanceRecords(sessionId).Rows)
                    _existing[Convert.ToInt32(r["StudentID"])] = r;

                pnlSession.Visible = true;
                lblStatus.Text = status;
                lblStatus.Attributes["style"] = StatusStyle(status);
                string info = "Session #" + sessionId;
                if (session["SubmittedAt"] != DBNull.Value) info += " · submitted " + Convert.ToDateTime(session["SubmittedAt"]).ToString("dd MMM yyyy HH:mm");
                if (session["ReopenedAt"] != DBNull.Value) info += " · reopened " + Convert.ToDateTime(session["ReopenedAt"]).ToString("dd MMM yyyy HH:mm");
                litSessionInfo.Text = HttpUtility.HtmlEncode(info);
            }
            else
            {
                pnlSession.Visible = false;
            }

            DataTable students = _repo.GetEligibleStudents(YearV(), SectionV());
            // optional client-side style search filter
            string q = (txtSearch.Text ?? "").Trim();
            if (q.Length > 0)
            {
                DataView dv = students.DefaultView;
                dv.RowFilter = "FullName LIKE '%" + q.Replace("'", "''") + "%' OR StudentCode LIKE '%" + q.Replace("'", "''") + "%'";
                students = dv.ToTable();
            }

            gvStudents.DataSource = students;
            gvStudents.DataBind();

            pnlGrid.Visible = true;
            bool readOnly = submitted;
            btnDraft.Visible = !readOnly;
            btnSubmit.Visible = !readOnly;
            btnReopen.Visible = readOnly && _repo.CanManageAttendance(Role);
            gvStudents.Enabled = !readOnly;
        }

        protected string StatusStyle(string s)
        {
            switch ((s ?? "").ToLowerInvariant())
            {
                case "draft": return "background:#FEF3C7;color:#B45309";
                case "submitted": return "background:#DCFCE7;color:#15803D";
                case "locked": return "background:#E0E7FF;color:#3730A3";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        // ---------- payload ----------
        private List<AttendanceRepository.MarkRow> ReadPayload()
        {
            var rows = new List<AttendanceRepository.MarkRow>();
            string json = hfPayload.Value;
            if (string.IsNullOrWhiteSpace(json)) return rows;
            var ser = new JavaScriptSerializer();
            var arr = ser.Deserialize<List<PostRow>>(json);
            foreach (PostRow p in arr)
            {
                TimeSpan ci;
                TimeSpan? checkIn = TimeSpan.TryParse(p.ci, CultureInfo.InvariantCulture, out ci) ? ci : (TimeSpan?)null;
                rows.Add(new AttendanceRepository.MarkRow
                {
                    StudentID = p.s,
                    Status = (p.st ?? "").Trim(),
                    CheckInTime = checkIn,
                    Remarks = p.rm
                });
            }
            return rows;
        }

        private class PostRow { public int s { get; set; } public string st { get; set; } public string ci { get; set; } public string rm { get; set; } }

        protected void btnDraft_Click(object sender, EventArgs e)
        {
            try
            {
                _repo.SaveAttendanceDraft(Scope(), ReadPayload(), UserId, Role);
                Show(true, "Draft saved. Unmarked students remain Not Marked.");
                LoadGrid();
            }
            catch (Exception ex) { Show(false, ex.Message); SafeReload(); }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                _repo.SubmitAttendance(Scope(), ReadPayload(), UserId, Role);
                Show(true, "Attendance submitted and locked.");
                LoadGrid();
            }
            catch (Exception ex) { Show(false, ex.Message); SafeReload(); }
        }

        protected void btnReopen_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow session = _repo.GetAttendanceSessionByScope(Scope());
                if (session == null) { Show(false, "No submitted session to reopen."); return; }
                _repo.ReopenAttendance(Convert.ToInt32(session["AttendanceSessionID"]), hfReopenReason.Value, UserId, Role);
                Show(true, "Attendance reopened as Draft. Records preserved.");
                LoadGrid();
            }
            catch (Exception ex) { Show(false, ex.Message); SafeReload(); }
        }

        private void SafeReload()
        {
            try { if (SectionV() > 0 && ClassV() > 0 && YearV() > 0) LoadGrid(); } catch { HideGrid(); }
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm " + (ok
                ? "bg-emerald-50 text-emerald-800 border border-emerald-200"
                : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
