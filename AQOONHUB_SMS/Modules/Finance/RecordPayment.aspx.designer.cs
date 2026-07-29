namespace AQOONHUB_SMS.Modules.Finance
{
    public partial class RecordPayment
    {
        protected global::System.Web.UI.WebControls.Panel msg;
        protected global::System.Web.UI.WebControls.Literal msgText;

        protected global::System.Web.UI.WebControls.Panel pnlSuccess;
        protected global::System.Web.UI.WebControls.Literal litRcpNumber;
        protected global::System.Web.UI.WebControls.Literal litRcpAmount;
        protected global::System.Web.UI.WebControls.Literal litRcpBalance;
        protected global::System.Web.UI.WebControls.Literal litRcpStatus;
        protected global::System.Web.UI.WebControls.HyperLink lnkReceipt;

        protected global::System.Web.UI.WebControls.Panel pnlForm;
        protected global::System.Web.UI.WebControls.HiddenField hidInvoiceId;
        protected global::System.Web.UI.WebControls.HiddenField hidBalance;

        protected global::System.Web.UI.WebControls.Panel pnlSelect;
        protected global::System.Web.UI.WebControls.DropDownList student;
        protected global::System.Web.UI.WebControls.DropDownList invoice;

        protected global::System.Web.UI.WebControls.Panel pnlInfo;
        protected global::System.Web.UI.WebControls.Literal litStudent;
        protected global::System.Web.UI.WebControls.Literal litStudentCode;
        protected global::System.Web.UI.WebControls.Literal litClass;
        protected global::System.Web.UI.WebControls.Literal litInvoiceNo;
        protected global::System.Web.UI.WebControls.Literal litInvAmount;
        protected global::System.Web.UI.WebControls.Literal litPaid;
        protected global::System.Web.UI.WebControls.Literal litPrevBalance;
        protected global::System.Web.UI.WebControls.Literal litDueDate;
        protected global::System.Web.UI.WebControls.Literal litStatus;

        protected global::System.Web.UI.WebControls.Panel pnlPay;
        protected global::System.Web.UI.WebControls.TextBox amount;
        protected global::System.Web.UI.WebControls.DropDownList method;
        protected global::System.Web.UI.WebControls.TextBox date;
        protected global::System.Web.UI.WebControls.TextBox reference;
        protected global::System.Web.UI.WebControls.TextBox notes;
        protected global::System.Web.UI.WebControls.Button save;
    }
}
