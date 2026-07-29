using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Academic
{
    public partial class Timetable : System.Web.UI.Page
    {
        private readonly AcademicsRepository _repo = new AcademicsRepository();
        private static readonly string[] Days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday" };

        private bool ListMode
        {
            get { object o = ViewState["list"]; return o != null && (bool)o; }
            set { ViewState["list"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Authorize()) return;
            if (!IsPostBack)
            {
                FillYears(ddlYear, false);
                int y = _repo.GetActiveAcademicYearId();
                if (y > 0 && ddlYear.Items.FindByValue(y.ToString()) != null) ddlYear.SelectedValue = y.ToString();
                ReloadFilterDependents();
                RenderView();
            }
        }

        private bool Authorize()
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx", true);
                return false;
            }
            if (!_repo.CanManage(Convert.ToString(Session["Role"])))
            {
                Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true);
                return false;
            }
            return true;
        }

        // ---- markup helpers ----
        protected string DayName(int d) { return d >= 0 && d < Days.Length ? Days[d] : "—"; }
        protected string Time(object v)
        {
            if (v == null || v == DBNull.Value) return "—";
            TimeSpan ts = (TimeSpan)v;
            return DateTime.Today.Add(ts).ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }

        // ---- filter dependents ----
        private void ReloadFilterDependents()
        {
            int year; int.TryParse(ddlYear.SelectedValue, out year);
            ddlTerm.Items.Clear();
            ddlTerm.Items.Add(new ListItem("All Terms", ""));
            if (year > 0)
                foreach (DataRow r in _repo.GetTermsLookup(year).Rows)
                    ddlTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));

            FillList(ddlClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "— Select class —");
            ReloadFilterSections();
        }

        private void ReloadFilterSections()
        {
            int classId; int.TryParse(ddlClass.SelectedValue, out classId);
            ddlSection.Items.Clear();
            ddlSection.Items.Add(new ListItem("— Select section —", "0"));
            if (classId > 0)
                foreach (DataRow r in _repo.GetSectionsLookup(classId).Rows)
                    ddlSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
        }

        protected void ddlYear_Changed(object sender, EventArgs e) { ReloadFilterDependents(); RenderView(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { ReloadFilterSections(); RenderView(); }
        protected void btnView_Click(object sender, EventArgs e) { RenderView(); }
        protected void btnWeekly_Click(object sender, EventArgs e) { ListMode = false; RenderView(); }
        protected void btnList_Click(object sender, EventArgs e) { ListMode = true; RenderView(); }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            int y = _repo.GetActiveAcademicYearId();
            if (y > 0 && ddlYear.Items.FindByValue(y.ToString()) != null) ddlYear.SelectedValue = y.ToString();
            ReloadFilterDependents();
            ListMode = false;
            RenderView();
        }

        private int Nz(DropDownList ddl) { int v; return int.TryParse(ddl.SelectedValue, out v) ? v : 0; }

        // ---- render ----
        private void RenderView()
        {
            btnWeekly.CssClass = "view-btn" + (ListMode ? "" : " active");
            btnList.CssClass = "view-btn" + (ListMode ? " active" : "");
            btnWeekly.Style["border-radius"] = "8px 0 0 8px";
            btnList.Style["border-radius"] = "0 8px 8px 0";

            int section = Nz(ddlSection), year = Nz(ddlYear);
            int? term = Nz(ddlTerm) > 0 ? Nz(ddlTerm) : (int?)null;

            litContext.Text = BuildContext();

            if (section <= 0)
            {
                pnlWeekly.Visible = !ListMode;
                pnlList.Visible = ListMode;
                litWeekly.Text = "<div class='py-12 text-center text-sm text-gray-500'>Select a class and section, then click “View Timetable”.</div>";
                gvList.DataSource = null; gvList.DataBind();
                return;
            }

            DataTable dt = _repo.GetTimetable(section, year > 0 ? year : (int?)null, term);

            if (ListMode)
            {
                pnlWeekly.Visible = false; pnlList.Visible = true;
                gvList.DataSource = dt; gvList.DataBind();
            }
            else
            {
                pnlWeekly.Visible = true; pnlList.Visible = false;
                litWeekly.Text = BuildWeekly(dt);
            }
        }

        private string BuildContext()
        {
            string cls = ddlClass.SelectedIndex > 0 ? ddlClass.SelectedItem.Text : "—";
            string sec = ddlSection.SelectedIndex > 0 ? ddlSection.SelectedItem.Text : "—";
            string term = ddlTerm.SelectedIndex > 0 ? ddlTerm.SelectedItem.Text : "All Terms";
            string yr = ddlYear.SelectedIndex >= 0 && ddlYear.SelectedValue != "" ? ddlYear.SelectedItem.Text : "—";
            return "Academic Year: " + Server.HtmlEncode(yr) + " · Term: " + Server.HtmlEncode(term) +
                   " · Class: " + Server.HtmlEncode(cls) + " · Section: " + Server.HtmlEncode(sec);
        }

        private static readonly string[] Palette = {
            "background:#EFF6FF;color:#1D4ED8", "background:#ECFDF5;color:#047857",
            "background:#FEF3C7;color:#B45309", "background:#FCE7F3;color:#BE185D",
            "background:#EDE9FE;color:#6D28D9", "background:#E0F2FE;color:#0369A1" };

        private string BuildWeekly(DataTable dt)
        {
            // distinct time slots ordered by start
            var slots = new System.Collections.Generic.List<Tuple<TimeSpan, TimeSpan>>();
            foreach (DataRow r in dt.Rows)
            {
                TimeSpan s = (TimeSpan)r["StartTime"], en = (TimeSpan)r["EndTime"];
                bool found = false;
                foreach (var sl in slots) if (sl.Item1 == s && sl.Item2 == en) { found = true; break; }
                if (!found) slots.Add(Tuple.Create(s, en));
            }
            slots.Sort((a, b) => a.Item1.CompareTo(b.Item1));

            if (slots.Count == 0)
                return "<div class='py-12 text-center text-sm text-gray-500'>No lessons scheduled for this section yet. Click “Add Timetable Entry”.</div>";

            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='tt-grid'><thead><tr><th class='tt-timecol'>Time / Period</th>");
            for (int d = 0; d < Days.Length; d++) sb.Append("<th>").Append(Days[d]).Append("</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var sl in slots)
            {
                sb.Append("<tr><td class='tt-timecol'>")
                  .Append(Time(sl.Item1)).Append("<br>").Append(Time(sl.Item2)).Append("</td>");
                for (int d = 0; d < Days.Length; d++)
                {
                    sb.Append("<td>");
                    foreach (DataRow r in dt.Rows)
                    {
                        if (Convert.ToInt32(r["DayOfWeek"]) == d &&
                            (TimeSpan)r["StartTime"] == sl.Item1 && (TimeSpan)r["EndTime"] == sl.Item2)
                        {
                            string color = Palette[Math.Abs(Convert.ToInt32(r["SubjectID"])) % Palette.Length];
                            sb.Append("<div class='lesson' style='").Append(color).Append("'>")
                              .Append("<div class='s'>").Append(Server.HtmlEncode(Convert.ToString(r["SubjectName"]))).Append("</div>")
                              .Append("<div class='t'>").Append(Server.HtmlEncode(Convert.ToString(r["TeacherName"]))).Append("</div>")
                              .Append("<div class='r'>").Append(r["RoomNumber"] == DBNull.Value ? "" : Server.HtmlEncode(Convert.ToString(r["RoomNumber"]))).Append("</div>")
                              .Append("</div>");
                        }
                    }
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        // ---- drawer ----
        protected void btnAddEntry_Click(object sender, EventArgs e)
        {
            hfId.Value = "0";
            litTitle.Text = "Add Timetable Entry";
            FillYears(dYear, false);
            int y = _repo.GetActiveAcademicYearId();
            if (y > 0 && dYear.Items.FindByValue(y.ToString()) != null) dYear.SelectedValue = y.ToString();
            ReloadDrawerYearDependents();
            FillList(dClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "— Select class —");
            ClearDrawerSectionDependents();
            dPeriod.Text = "1"; dStart.Text = ""; dEnd.Text = ""; dRoom.Text = "";
            dTeacher.Text = ""; hfStaffId.Value = "0"; dDay.SelectedValue = "0";
            pnlDrawer.Visible = true;
        }

        protected void btnCancel_Click(object sender, EventArgs e) { pnlDrawer.Visible = false; }

        private void ReloadDrawerYearDependents()
        {
            int year; int.TryParse(dYear.SelectedValue, out year);
            dTerm.Items.Clear();
            dTerm.Items.Add(new ListItem("— No term —", "0"));
            if (year > 0)
                foreach (DataRow r in _repo.GetTermsLookup(year).Rows)
                    dTerm.Items.Add(new ListItem(Convert.ToString(r["TermName"]), Convert.ToString(r["TermID"])));
        }

        private void ClearDrawerSectionDependents()
        {
            dSection.Items.Clear(); dSection.Items.Add(new ListItem("— Select section —", "0"));
            dSubject.Items.Clear(); dSubject.Items.Add(new ListItem("— Select subject —", "0"));
        }

        protected void dYear_Changed(object sender, EventArgs e) { ReloadDrawerYearDependents(); pnlDrawer.Visible = true; }

        protected void dClass_Changed(object sender, EventArgs e)
        {
            int classId; int.TryParse(dClass.SelectedValue, out classId);
            dSection.Items.Clear(); dSection.Items.Add(new ListItem("— Select section —", "0"));
            dSubject.Items.Clear(); dSubject.Items.Add(new ListItem("— Select subject —", "0"));
            dTeacher.Text = ""; hfStaffId.Value = "0";
            if (classId > 0)
                foreach (DataRow r in _repo.GetSectionsLookup(classId).Rows)
                    dSection.Items.Add(new ListItem(Convert.ToString(r["SectionName"]), Convert.ToString(r["SectionID"])));
            pnlDrawer.Visible = true;
        }

        protected void dSection_Changed(object sender, EventArgs e)
        {
            LoadDrawerSubjects();
            pnlDrawer.Visible = true;
        }

        private void LoadDrawerSubjects()
        {
            int section, year;
            int.TryParse(dSection.SelectedValue, out section);
            int.TryParse(dYear.SelectedValue, out year);
            dSubject.Items.Clear(); dSubject.Items.Add(new ListItem("— Select subject —", "0"));
            dTeacher.Text = ""; hfStaffId.Value = "0";
            if (section > 0 && year > 0)
                foreach (DataRow r in _repo.GetSectionAssignedSubjects(section, year).Rows)
                {
                    // value encodes subjectId:staffId so the teacher auto-fills
                    ListItem li = new ListItem(Convert.ToString(r["SubjectName"]),
                        Convert.ToString(r["SubjectID"]) + ":" + Convert.ToString(r["StaffID"]));
                    li.Attributes["data-teacher"] = Convert.ToString(r["TeacherName"]);
                    dSubject.Items.Add(li);
                }
        }

        protected void dSubject_Changed(object sender, EventArgs e)
        {
            SetTeacherFromSubject();
            pnlDrawer.Visible = true;
        }

        private void SetTeacherFromSubject()
        {
            string val = dSubject.SelectedValue;
            if (val.Contains(":"))
            {
                string[] parts = val.Split(':');
                hfStaffId.Value = parts[1];
                int sid; int.TryParse(parts[1], out sid);
                dTeacher.Text = sid > 0 ? _repo.GetStaffName(sid) : "";
            }
            else { hfStaffId.Value = "0"; dTeacher.Text = ""; }
        }

        protected void gvList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id) || id <= 0) return;

            if (e.CommandName == "EditRow")
            {
                DataRow r = _repo.GetTimetableEntry(id);
                if (r == null) return;
                hfId.Value = id.ToString();
                litTitle.Text = "Edit Timetable Entry";
                FillYears(dYear, false);
                int year = Convert.ToInt32(r["AcademicYearID"]);
                if (dYear.Items.FindByValue(year.ToString()) != null) dYear.SelectedValue = year.ToString();
                ReloadDrawerYearDependents();
                if (r["TermID"] != DBNull.Value && dTerm.Items.FindByValue(Convert.ToString(r["TermID"])) != null) dTerm.SelectedValue = Convert.ToString(r["TermID"]);
                FillList(dClass, _repo.GetClassesLookup(), "ClassName", "ClassID", "— Select class —");
                int classId = Convert.ToInt32(r["ClassID"]);
                if (dClass.Items.FindByValue(classId.ToString()) != null) dClass.SelectedValue = classId.ToString();
                dSection.Items.Clear(); dSection.Items.Add(new ListItem("— Select section —", "0"));
                foreach (DataRow s in _repo.GetSectionsLookup(classId).Rows)
                    dSection.Items.Add(new ListItem(Convert.ToString(s["SectionName"]), Convert.ToString(s["SectionID"])));
                if (dSection.Items.FindByValue(Convert.ToString(r["SectionID"])) != null) dSection.SelectedValue = Convert.ToString(r["SectionID"]);
                LoadDrawerSubjects();
                string key = Convert.ToString(r["SubjectID"]) + ":" + Convert.ToString(r["StaffID"]);
                if (dSubject.Items.FindByValue(key) != null) dSubject.SelectedValue = key;
                SetTeacherFromSubject();
                dDay.SelectedValue = Convert.ToString(r["DayOfWeek"]);
                dPeriod.Text = Convert.ToString(r["PeriodNo"]);
                dStart.Text = ((TimeSpan)r["StartTime"]).ToString(@"hh\:mm");
                dEnd.Text = ((TimeSpan)r["EndTime"]).ToString(@"hh\:mm");
                dRoom.Text = r["RoomNumber"] == DBNull.Value ? "" : Convert.ToString(r["RoomNumber"]);
                pnlDrawer.Visible = true;
            }
            else if (e.CommandName == "DeleteRow")
            {
                try { _repo.DeleteTimetableEntry(id); Show(true, "Timetable entry deleted."); RenderView(); }
                catch (Exception ex) { Show(false, ex.Message); }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = int.Parse(hfId.Value);
            int section, year, staff, period, day;
            int.TryParse(dSection.SelectedValue, out section);
            int.TryParse(dYear.SelectedValue, out year);
            int.TryParse(hfStaffId.Value, out staff);
            int.TryParse(dPeriod.Text, out period);
            int.TryParse(dDay.SelectedValue, out day);
            int term; int.TryParse(dTerm.SelectedValue, out term);

            int subject = 0;
            if (dSubject.SelectedValue.Contains(":")) int.TryParse(dSubject.SelectedValue.Split(':')[0], out subject);

            TimeSpan start, end;
            if (year <= 0) { Reopen("Please select an academic year."); return; }
            if (section <= 0) { Reopen("Please select a section."); return; }
            if (subject <= 0 || staff <= 0) { Reopen("Please select a subject with an assigned teacher."); return; }
            if (!TimeSpan.TryParse(dStart.Text, out start)) { Reopen("Please provide a valid start time."); return; }
            if (!TimeSpan.TryParse(dEnd.Text, out end)) { Reopen("Please provide a valid end time."); return; }
            if (string.IsNullOrWhiteSpace(dRoom.Text)) { Reopen("Room is required."); return; }

            try
            {
                _repo.SaveTimetableEntry(id, section, subject, staff, day, period, start, end, dRoom.Text.Trim(),
                    year, term > 0 ? term : (int?)null);
                Show(true, "Timetable entry saved.");
                pnlDrawer.Visible = false;
                // reflect the saved entry in the main view
                if (ddlSection.Items.FindByValue(section.ToString()) != null) ddlSection.SelectedValue = section.ToString();
                RenderView();
            }
            catch (Exception ex) { Reopen(ex.Message); }
        }

        private void Reopen(string text) { Show(false, text); pnlDrawer.Visible = true; }

        // ---- helpers ----
        private void FillYears(DropDownList ddl, bool allOption)
        {
            ddl.Items.Clear();
            if (allOption) ddl.Items.Add(new ListItem("All Years", ""));
            foreach (DataRow r in _repo.GetAcademicYearsLookup().Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r["YearName"]), Convert.ToString(r["AcademicYearID"])));
        }

        private void FillList(DropDownList ddl, DataTable dt, string text, string val, string first)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(first, "0"));
            foreach (DataRow r in dt.Rows)
                ddl.Items.Add(new ListItem(Convert.ToString(r[text]), Convert.ToString(r[val])));
        }

        private void Show(bool ok, string text)
        {
            msg.Visible = true;
            msg.CssClass = "rounded-lg p-3 mb-4 text-sm no-print " + (ok
                ? "bg-emerald-50 text-emerald-800 border border-emerald-200"
                : "bg-amber-50 text-amber-800 border border-amber-200");
            msgText.Text = HttpUtility.HtmlEncode(text);
        }
    }
}
