namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class Invoices
    {
        protected global::System.Web.UI.WebControls.HyperLink lnkAddInvoice;

        protected global::System.Web.UI.WebControls.Label lblTotalInvoiced;
        protected global::System.Web.UI.WebControls.Label lblCollected;
        protected global::System.Web.UI.WebControls.Label lblOutstanding;
        protected global::System.Web.UI.WebControls.Label lblOverdueCount;

        protected global::System.Web.UI.WebControls.TextBox txtSearch;
        protected global::System.Web.UI.WebControls.DropDownList ddlAcademicYear;
        protected global::System.Web.UI.WebControls.DropDownList ddlTerm;
        protected global::System.Web.UI.WebControls.DropDownList ddlStatus;
        protected global::System.Web.UI.WebControls.LinkButton btnSearch;
        protected global::System.Web.UI.WebControls.LinkButton btnReset;

        protected global::System.Web.UI.WebControls.GridView gvInvoices;

        protected global::System.Web.UI.WebControls.Label lblResultsSummary;
        protected global::System.Web.UI.WebControls.LinkButton btnPrevPage;
        protected global::System.Web.UI.WebControls.Label lblPageIndicator;
        protected global::System.Web.UI.WebControls.LinkButton btnNextPage;
    }
}
