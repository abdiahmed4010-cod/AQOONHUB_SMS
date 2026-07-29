using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class FeeCategories : System.Web.UI.Page
    {
        readonly FeeRepository repo = new FeeRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) Response.Redirect("~/Modules/Authentication/Login.aspx");
            if (!IsPostBack) Bind();
        }

        void Bind()
        {
            try
            {
                DataTable dt = repo.GetFeeCategories(false);
                string search = (txtCatSearch.Text ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(search))
                {
                    string safe = search.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]");
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = string.Format("CategoryName LIKE '%{0}%' OR CategoryCode LIKE '%{0}%'", safe);
                    dt = dv.ToTable();
                }
                list.DataSource = dt;
                list.DataBind();
                litCount.Text = HttpUtility.HtmlEncode(dt.Rows.Count == 1 ? "1 category" : dt.Rows.Count + " categories");
            }
            catch (Exception ex) { Show(ex.Message); }
        }

        protected void btnCatFilter_Click(object s, EventArgs e) { Bind(); }

        protected void save_Click(object s, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(code.Text)) { Show("Category name and code are required."); return; }
                int parsed;
                int? id = int.TryParse(categoryId.Value, out parsed) ? parsed : (int?)null;
                repo.SaveFeeCategory(id, name.Text.Trim(), code.Text.Trim().ToUpperInvariant(), description.Text.Trim(), term.SelectedValue, active.Checked);
                Clear();
                Bind();
                Show("Fee category saved.");
            }
            catch (Exception ex) { Show(ex.Message); }
        }

        protected void clear_Click(object s, EventArgs e) { Clear(); }

        void Clear() { categoryId.Value = ""; name.Text = code.Text = description.Text = ""; term.SelectedIndex = 0; active.Checked = true; }

        protected void list_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (e.CommandName == "deleteRow")
            {
                Show(repo.DeleteFeeCategory(id) ? "Category deleted." : "Category is in use and cannot be deleted.");
                Bind();
                return;
            }
            DataTable dt = repo.GetFeeCategories(false);
            DataRow[] rows = dt.Select("FeeCategoryID=" + id);
            if (rows.Length == 0) return;
            DataRow r = rows[0];
            categoryId.Value = id.ToString();
            name.Text = Convert.ToString(r["CategoryName"]);
            code.Text = Convert.ToString(r["CategoryCode"]);
            description.Text = Convert.ToString(r["Description"]);
            term.SelectedValue = Convert.ToString(r["DefaultBillingTerm"]);
            active.Checked = Convert.ToBoolean(r["IsActive"]);
        }

        // ---- markup helpers ----
        protected string TermStyle(object value)
        {
            switch (Convert.ToString(value))
            {
                case "Monthly": return "background:#EFF6FF;color:#1D4ED8";
                case "Per Term": return "background:#FCE7F3;color:#BE185D";
                case "Annual": return "background:#ECFDF5;color:#15803D";
                case "One Time": return "background:#F5F3FF;color:#7C3AED";
                default: return "background:#F1F5F9;color:#64748B";
            }
        }

        protected string StatusStyle(object isActive)
        {
            bool a = isActive != null && isActive != DBNull.Value && Convert.ToBoolean(isActive);
            return a ? "background:#DCFCE7;color:#15803D" : "background:#F1F5F9;color:#64748B";
        }

        void Show(string text) { msg.Visible = true; msgText.Text = HttpUtility.HtmlEncode(text); }
    }
}
