namespace AQOONHUB_SMS.Modules.Admission
{
    public partial class AddAdmission
    {
        protected global::System.Web.UI.WebControls.Panel pnlSuccess;
        protected global::System.Web.UI.WebControls.Label lblSuccess;
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Label lblError;
        protected global::System.Web.UI.WebControls.ValidationSummary valSummary;

        protected global::System.Web.UI.WebControls.Panel pnlFormBody;

        protected global::System.Web.UI.WebControls.Label lblApplicationNo;
        protected global::System.Web.UI.WebControls.HiddenField hdnApplicationNo;

        protected global::System.Web.UI.WebControls.TextBox txtFirstName;
        protected global::System.Web.UI.WebControls.TextBox txtLastName;
        protected global::System.Web.UI.WebControls.DropDownList ddlGender;
        protected global::System.Web.UI.WebControls.TextBox txtDateOfBirth;
        protected global::System.Web.UI.WebControls.CustomValidator cvDateOfBirth;

        protected global::System.Web.UI.WebControls.DropDownList ddlClass;

        protected global::System.Web.UI.WebControls.RadioButtonList rblGuardianMode;
        protected global::System.Web.UI.WebControls.Panel pnlExistingGuardian;
        protected global::System.Web.UI.WebControls.DropDownList ddlExistingGuardian;
        protected global::System.Web.UI.WebControls.Panel pnlNewGuardian;
        protected global::System.Web.UI.WebControls.TextBox txtGuardianName;
        protected global::System.Web.UI.WebControls.DropDownList ddlGuardianRelationship;
        protected global::System.Web.UI.WebControls.TextBox txtGuardianPhone;
        protected global::System.Web.UI.WebControls.TextBox txtGuardianEmail;
        protected global::System.Web.UI.WebControls.Panel pnlGuardianDuplicateWarning;
        protected global::System.Web.UI.WebControls.Label lblGuardianDuplicateWarning;

        protected global::System.Web.UI.WebControls.TextBox txtNotes;

        protected global::System.Web.UI.WebControls.LinkButton btnCancel;
        protected global::System.Web.UI.WebControls.LinkButton btnReset;
        protected global::System.Web.UI.WebControls.LinkButton btnSave;
    }
}
