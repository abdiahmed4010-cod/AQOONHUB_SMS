namespace AQOONHUB_SMS.Modules.Reports
{ public sealed partial class ReportsRepository { public System.Data.DataTable PreviewBuilder(BuilderValidation validation){return ReportBuilderMetadata.Execute(this,validation,ReportBuilderMetadata.PreviewLimit);} } }
