namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class AddPayment
    {
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Label lblError;
        protected global::System.Web.UI.WebControls.ValidationSummary valSummary;

        protected global::System.Web.UI.WebControls.Panel pnlNotFound;
        protected global::System.Web.UI.WebControls.Panel pnlFormBody;

        protected global::System.Web.UI.WebControls.Label lblInvoiceNo;
        protected global::System.Web.UI.WebControls.Label lblStudentInfo;
        protected global::System.Web.UI.WebControls.Label lblTotalAmount;
        protected global::System.Web.UI.WebControls.Label lblPaidSoFar;
        protected global::System.Web.UI.WebControls.Label lblBalanceDue;

        protected global::System.Web.UI.WebControls.Label lblReceiptNo;
        protected global::System.Web.UI.WebControls.TextBox txtAmount;
        protected global::System.Web.UI.WebControls.DropDownList ddlPaymentMethod;
        protected global::System.Web.UI.WebControls.TextBox txtPaymentDate;
        protected global::System.Web.UI.WebControls.TextBox txtNotes;

        protected global::System.Web.UI.WebControls.LinkButton btnCancel;
        protected global::System.Web.UI.WebControls.LinkButton btnSave;
    }
}
