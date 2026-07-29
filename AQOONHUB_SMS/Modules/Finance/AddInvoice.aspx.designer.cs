namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class AddInvoice
    {
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Label lblError;
        protected global::System.Web.UI.WebControls.ValidationSummary valSummary;

        protected global::System.Web.UI.WebControls.TextBox txtStudentSearch;
        protected global::System.Web.UI.WebControls.LinkButton btnFindStudent;
        protected global::System.Web.UI.WebControls.HiddenField hdnStudentId;
        protected global::System.Web.UI.WebControls.CustomValidator cvStudentSelected;
        protected global::System.Web.UI.WebControls.Label lblSelectedStudent;
        protected global::System.Web.UI.WebControls.GridView gvStudentResults;

        protected global::System.Web.UI.WebControls.Panel pnlInvoiceForm;
        protected global::System.Web.UI.WebControls.DropDownList ddlAcademicYear;
        protected global::System.Web.UI.WebControls.DropDownList ddlTerm;
        protected global::System.Web.UI.WebControls.TextBox txtDueDate;
        protected global::System.Web.UI.WebControls.CheckBoxList cblFees;
        protected global::System.Web.UI.WebControls.Label lblNoFees;
        protected global::System.Web.UI.WebControls.Label lblTotalPreview;

        protected global::System.Web.UI.WebControls.LinkButton btnCancel;
        protected global::System.Web.UI.WebControls.LinkButton btnGenerate;
    }
}