using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace AQOONHUB_SMS.Modules.Reports
{
    public partial class PerformanceAnalytics : System.Web.UI.Page
    {
        private readonly ReportsRepository _repo = new ReportsRepository();
        private string Role { get { return Convert.ToString(Session["Role"]); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/Modules/Authentication/Login.aspx", true); return; }
            if (!ReportAuthorization.CanViewCategory(Role, ReportAuthorization.Performance)) { Response.Redirect("~/Modules/Dashboard/Dashboard.aspx", true); return; }
            if (!IsPostBack) { BindYears(); BindTerms(); BindClasses(); BindSections(); BindExams(); LoadAnalytics(); }
        }

        private int? Sel(DropDownList d) { int n; return int.TryParse(d.SelectedValue, out n) && n > 0 ? n : (int?)null; }
        private void Fill(DropDownList d, DataTable t, string text, string value, string all)
        {
            d.Items.Clear(); d.Items.Add(new ListItem(all, "0"));
            foreach (DataRow r in t.Rows) d.Items.Add(new ListItem(Convert.ToString(r[text]), Convert.ToString(r[value])));
        }
        private void BindYears() { Fill(ddlYear, _repo.GetAcademicYears(), "YearName", "AcademicYearID", "All years"); }
        private void BindTerms() { Fill(ddlTerm, _repo.GetTerms(Sel(ddlYear)), "TermName", "TermID", "All terms"); }
        private void BindClasses() { Fill(ddlClass, _repo.GetClasses(Sel(ddlYear)), "ClassName", "ClassID", "All classes"); }
        private void BindSections() { Fill(ddlSection, _repo.GetSections(Sel(ddlClass)), "SectionName", "SectionID", "All sections"); }
        private void BindExams() { Fill(ddlExam, _repo.GetExamsLookup(Sel(ddlYear)), "ExamName", "ExamID", "All examinations"); }
        protected void ddlYear_Changed(object sender, EventArgs e) { BindTerms(); BindClasses(); BindSections(); BindExams(); }
        protected void ddlClass_Changed(object sender, EventArgs e) { BindSections(); }
        protected void btnApply_Click(object sender, EventArgs e) { LoadAnalytics(); }
        protected void btnReset_Click(object sender, EventArgs e) { Response.Redirect("~/Modules/Reports/PerformanceAnalytics.aspx"); }

        private ReportFilter CurrentFilter()
        {
            return new ReportFilter { YearID=Sel(ddlYear), TermID=Sel(ddlTerm), ExamID=Sel(ddlExam), ClassID=Sel(ddlClass), SectionID=Sel(ddlSection) };
        }

        private void LoadAnalytics()
        {
            pnlError.Visible = false;
            try
            {
                ReportFilter f = CurrentFilter();
                DataTable summary = _repo.GetPerformanceSummary(f); DataRow s = summary.Rows[0];
                bool has = s["AveragePerformance"] != DBNull.Value;
                litAverage.Text = has ? Convert.ToDecimal(s["AveragePerformance"]).ToString("0.00") + "%" : "No data available";
                litPass.Text = s["PassRate"] == DBNull.Value ? "No data available" : Convert.ToDecimal(s["PassRate"]).ToString("0.00") + "%";
                litFail.Text = s["FailureRate"] == DBNull.Value ? "No data available" : Convert.ToDecimal(s["FailureRate"]).ToString("0.00") + "%";
                litStudents.Text = has ? Convert.ToString(s["TotalStudents"]) : "No data available";
                litRisk.Text = has ? Convert.ToString(s["AtRiskStudents"]) : "No data available";

                DataTable trend=_repo.GetStudentPerformanceTrend(f), classes=_repo.GetClassPerformanceComparison(f,false), subjects=_repo.GetSubjectPerformanceComparison(f), passFail=_repo.GetPassFailDistribution(f), enrollment=_repo.GetEnrollmentGrowth(f), years=_repo.GetAcademicYearComparison(f), relationship=_repo.GetAttendanceExamRelationship(f);
                var defs = new DataTable(); defs.Columns.Add("Title"); defs.Columns.Add("CanvasId"); defs.Columns.Add("AriaLabel"); defs.Columns.Add("EmptyMessage"); defs.Columns.Add("HasData", typeof(bool));
                var payload = new List<object>();
                AddChart(defs,payload,"Student Performance Trend","cTrend","Line chart of published examination averages","Performance analytics require published examination data.","line",trend);
                AddChart(defs,payload,"Class Performance Comparison","cClasses","Bar chart comparing historical class averages","No published class comparison data is available.","bar",classes);
                AddChart(defs,payload,"Subject Performance Comparison","cSubjects","Bar chart comparing submitted published subject results","No published subject comparison data is available.","bar",subjects);
                AddChart(defs,payload,"Pass vs Fail Distribution","cPassFail","Doughnut chart of passed and failed published summaries","No published pass or fail results are available.","doughnut",passFail);
                AddChart(defs,payload,"Enrollment Growth","cEnrollment","Line chart of real student enrollment dates","No enrollment dates are available for this scope.","line",enrollment);
                AddChart(defs,payload,"Academic Year Comparison","cYears","Bar chart comparing published academic-year averages","No published academic-year comparison data is available.","bar",years);
                AddScatter(defs,payload,relationship);
                rptCharts.DataSource=defs; rptCharts.DataBind();
                litChartData.Text="<script>window.AQHAnalytics="+new JavaScriptSerializer().Serialize(payload)+";</script>";
                gvRisk.DataSource=_repo.GetAtRiskStudents(f); gvRisk.DataBind();
            }
            catch
            {
                pnlError.Visible=true; litError.Text="Performance analytics are temporarily unavailable. No unverified values have been displayed.";
                litAverage.Text=litPass.Text=litFail.Text=litStudents.Text=litRisk.Text="Unavailable";
            }
        }

        private static void AddChart(DataTable defs,List<object> payload,string title,string id,string aria,string empty,string type,DataTable t)
        {
            bool has=t!=null&&t.Rows.Count>0; defs.Rows.Add(title,id,aria,empty,has); if(!has)return;
            var labels=new List<string>();var values=new List<decimal>();foreach(DataRow r in t.Rows){labels.Add(Convert.ToString(r["Label"]));values.Add(Convert.ToDecimal(r["Value"]));}
            payload.Add(new { id=id,type=type,data=new { labels=labels,datasets=new[]{new { label=title,data=values.ToArray(),borderColor="#2563EB",backgroundColor=type=="doughnut"?new[]{"#16A34A","#DC2626","#7C3AED"}:new[]{"rgba(37,99,235,.35)"},tension=.3,fill=type=="line" } } } });
        }
        private static void AddScatter(DataTable defs,List<object> payload,DataTable t)
        {
            const string id="cRelationship", title="Observed attendance and examination relationship";bool has=t!=null&&t.Rows.Count>0;defs.Rows.Add(title,id,"Scatter chart of official attendance rate and published examination average","Attendance comparison is unavailable because no compatible official attendance and published examination data exists.",has);if(!has)return;
            var points=new List<object>();foreach(DataRow r in t.Rows)points.Add(new{x=Convert.ToDecimal(r["Attendance"]),y=Convert.ToDecimal(r["Performance"])});
            payload.Add(new{id=id,type="scatter",data=new{datasets=new[]{new{label=title,data=points.ToArray(),backgroundColor="#7C3AED"}}}});
        }
    }
}
