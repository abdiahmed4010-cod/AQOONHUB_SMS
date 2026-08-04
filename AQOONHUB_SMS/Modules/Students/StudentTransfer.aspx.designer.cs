namespace AQOONHUB_SMS.Modules.Students
{
    public partial class StudentTransfer
    {
        protected global::System.Web.UI.WebControls.Label lblPageTitle;

        protected global::System.Web.UI.WebControls.Panel pnlSuccess;
        protected global::System.Web.UI.WebControls.Label lblSuccess;
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Label lblError;

        protected global::System.Web.UI.WebControls.Panel pnlNotFound;
        protected global::System.Web.UI.WebControls.Panel pnlBody;

        protected global::System.Web.UI.WebControls.Image imgPhoto;
        protected global::System.Web.UI.WebControls.Panel pnlPhotoFallback;
        protected global::System.Web.UI.WebControls.Label lblInitials;

        protected global::System.Web.UI.WebControls.Label lblFullName;
        protected global::System.Web.UI.WebControls.Label lblStatusBadge;
        protected global::System.Web.UI.WebControls.Label lblStudentCode;
        protected global::System.Web.UI.WebControls.Label lblAdmissionNo;
        protected global::System.Web.UI.WebControls.Label lblGender;
        protected global::System.Web.UI.WebControls.Label lblClassSection;
        protected global::System.Web.UI.WebControls.Label lblGuardian;
        protected global::System.Web.UI.WebControls.HyperLink lnkBack;

        protected global::System.Web.UI.WebControls.Panel pnlActiveTransferInfo;
        protected global::System.Web.UI.WebControls.Label lblCurDestSchool;
        protected global::System.Web.UI.WebControls.Label lblCurDestLocation;
        protected global::System.Web.UI.WebControls.Label lblCurTransferDate;
        protected global::System.Web.UI.WebControls.Label lblCurReason;
        protected global::System.Web.UI.WebControls.Label lblCurCertNo;
        protected global::System.Web.UI.WebControls.Label lblCurNotes;
        protected global::System.Web.UI.WebControls.Label lblCurProcessedBy;

        protected global::System.Web.UI.WebControls.Panel pnlReturnForm;
        protected global::System.Web.UI.WebControls.TextBox txtReturnDate;
        protected global::System.Web.UI.WebControls.DropDownList ddlReturnAcademicYear;
        protected global::System.Web.UI.WebControls.DropDownList ddlReturnClass;
        protected global::System.Web.UI.WebControls.DropDownList ddlReturnShift;
        protected global::System.Web.UI.WebControls.DropDownList ddlReturnSection;
        protected global::System.Web.UI.WebControls.TextBox txtReturnReason;
        protected global::System.Web.UI.WebControls.TextBox txtReturnNotes;
        protected global::System.Web.UI.WebControls.LinkButton btnReturn;

        protected global::System.Web.UI.WebControls.Panel pnlTransferForm;
        protected global::System.Web.UI.WebControls.DropDownList ddlTransferType;
        protected global::System.Web.UI.WebControls.TextBox txtDestSchool;
        protected global::System.Web.UI.WebControls.TextBox txtDestLocation;
        protected global::System.Web.UI.WebControls.TextBox txtDestContact;
        protected global::System.Web.UI.WebControls.TextBox txtDestPhone;
        protected global::System.Web.UI.WebControls.TextBox txtTransferDate;
        protected global::System.Web.UI.WebControls.CustomValidator cvTransferDate;
        protected global::System.Web.UI.WebControls.TextBox txtCertNo;
        protected global::System.Web.UI.WebControls.TextBox txtTransferReason;
        protected global::System.Web.UI.WebControls.TextBox txtTransferNotes;
        protected global::System.Web.UI.WebControls.LinkButton btnTransfer;

        protected global::System.Web.UI.WebControls.Panel pnlNoPermission;

        protected global::System.Web.UI.WebControls.GridView gvHistory;
    }
}
