namespace AQOONHUB_SMS.Modules.Students
{
    public partial class EditStudent
    {
        protected global::System.Web.UI.WebControls.Panel pnlSuccess;
        protected global::System.Web.UI.WebControls.Label lblSuccess;
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Label lblError;
        protected global::System.Web.UI.WebControls.ValidationSummary valSummary;

        protected global::System.Web.UI.WebControls.Panel pnlNotFound;
        protected global::System.Web.UI.WebControls.Panel pnlFormBody;

        protected global::System.Web.UI.WebControls.Label lblStudentCode;
        protected global::System.Web.UI.WebControls.Label lblAdmissionNo;

        protected global::System.Web.UI.WebControls.TextBox txtFirstName;
        protected global::System.Web.UI.WebControls.TextBox txtLastName;

        protected global::System.Web.UI.WebControls.DropDownList ddlGender;
        protected global::System.Web.UI.WebControls.DropDownList ddlStatus;

        protected global::System.Web.UI.WebControls.TextBox txtDateOfBirth;
        protected global::System.Web.UI.WebControls.CustomValidator cvDateOfBirth;

        protected global::System.Web.UI.WebControls.TextBox txtEnrollmentDate;

        protected global::System.Web.UI.WebControls.DropDownList ddlAcademicYear;
        protected global::System.Web.UI.WebControls.DropDownList ddlClass;
        protected global::System.Web.UI.WebControls.DropDownList ddlSection;
        protected global::System.Web.UI.WebControls.DropDownList ddlShift;
        protected global::System.Web.UI.WebControls.Panel pnlShiftWarn;

        // Placement change confirmation
        protected global::System.Web.UI.WebControls.Panel pnlPlacementConfirm;
        protected global::System.Web.UI.WebControls.Panel pnlPcError;
        protected global::System.Web.UI.WebControls.Label lblPcError;
        protected global::System.Web.UI.WebControls.Label lblPcCode;
        protected global::System.Web.UI.WebControls.Label lblPcName;
        protected global::System.Web.UI.WebControls.Label lblPcCurYear;
        protected global::System.Web.UI.WebControls.Label lblPcCurClass;
        protected global::System.Web.UI.WebControls.Label lblPcCurShift;
        protected global::System.Web.UI.WebControls.Label lblPcCurSection;
        protected global::System.Web.UI.WebControls.Label lblPcNewYear;
        protected global::System.Web.UI.WebControls.Label lblPcNewClass;
        protected global::System.Web.UI.WebControls.Label lblPcNewShift;
        protected global::System.Web.UI.WebControls.Label lblPcNewSection;
        protected global::System.Web.UI.WebControls.DropDownList ddlReason;
        protected global::System.Web.UI.WebControls.TextBox txtEffectiveDate;
        protected global::System.Web.UI.WebControls.TextBox txtReasonOther;
        protected global::System.Web.UI.WebControls.HiddenField hfConfirmToken;
        protected global::System.Web.UI.WebControls.LinkButton btnCancelPlacement;
        protected global::System.Web.UI.WebControls.LinkButton btnConfirmPlacement;

        protected global::System.Web.UI.WebControls.Panel pnlGuardianField;
        protected global::System.Web.UI.WebControls.DropDownList ddlGuardian;
        protected global::System.Web.UI.WebControls.Panel pnlNoGuardians;

        protected global::System.Web.UI.WebControls.TextBox txtAddress;
        protected global::System.Web.UI.WebControls.TextBox txtMedicalNotes;

        protected global::System.Web.UI.WebControls.Image imgCurrentPhoto;
        protected global::System.Web.UI.WebControls.Panel pnlCurrentPhotoFallback;
        protected global::System.Web.UI.WebControls.FileUpload fuPhoto;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator revPhoto;
        protected global::System.Web.UI.WebControls.CustomValidator cvPhoto;

        protected global::System.Web.UI.WebControls.LinkButton btnCancel;
        protected global::System.Web.UI.WebControls.LinkButton btnSave;
    }
}
