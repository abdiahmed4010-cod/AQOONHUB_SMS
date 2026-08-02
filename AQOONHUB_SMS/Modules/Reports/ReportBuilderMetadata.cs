using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;

namespace AQOONHUB_SMS.Modules.Reports
{
    public sealed class BuilderField { public string Key, Label, Column, Type; public bool Sensitive; public string[] Operators; }
    public sealed class BuilderSource
    {
        public string Key, Name, Category, Handler, Orientation; public int ExportLimit;
        public string[] Roles; public BuilderField[] Fields;
    }
    public sealed class BuilderFilter { public string field { get; set; } public string @operator { get; set; } public string value { get; set; } public string value2 { get; set; } }
    public sealed class BuilderSort { public string field { get; set; } public string direction { get; set; } }
    public sealed class BuilderConfig
    {
        public int version { get; set; } public string sourceKey { get; set; } public string[] columns { get; set; }
        public BuilderFilter[] filters { get; set; } public string[] groupBy { get; set; } public BuilderSort sort { get; set; }
        public string[] summaries { get; set; } public string chart { get; set; } public string orientation { get; set; }
    }
    public sealed class BuilderValidation { public bool Valid; public string Message; public BuilderSource Source; public BuilderConfig Config; }

    public static class ReportBuilderMetadata
    {
        public const int PreviewLimit = 100, MaxColumns = 20, MaxFilters = 10, MaxGroups = 3;
        static ReportBuilderMetadata()
        {
            // Map logical fields to the exact presentation columns returned by existing fixed handlers.
            Sources["students"].Fields[0].Column="Code"; Sources["students"].Fields[1].Column="Name";
            Sources["examinations"].Handler="exam-student-results"; Sources["examinations"].Fields[2].Column="Obtained"; Sources["examinations"].Fields[4].Column="Result Status";
            Sources["attendance"].Handler="att-individual"; Sources["attendance"].Fields[0].Column="Date"; Sources["attendance"].Fields[1].Column="Class"; Sources["attendance"].Fields[2].Column="Section"; Sources["attendance"].Fields[3].Column="Status";
            Sources["finance"].Handler="fin-collected";
            Sources["payroll"].Handler="pay-monthly"; Sources["payroll"].Fields[0].Column="Employee"; Sources["payroll"].Fields[1].Column="Gross"; Sources["payroll"].Fields[2].Column="Deductions"; Sources["payroll"].Fields[3].Column="Net";
            Sources["staff"].Fields[0].Column="Name";
            Sources["staff"].Roles=new[]{"superadmin","admin"};
            Sources["enrollment"].Fields[0].Column="Name";
            Sources["analytics"].Fields[0].Column="Label"; Sources["analytics"].Fields[1].Column="Students"; Sources["analytics"].Fields[2].Column="Value"; Sources["analytics"].Fields[3].Column="Value";
        }
        private static readonly string[] TextOps = { "equals", "not-equals", "contains", "starts-with", "ends-with", "is-empty", "is-not-empty" };
        private static readonly string[] NumberOps = { "equals", "not-equals", "greater-than", "less-than", "greater-or-equal", "less-or-equal", "between", "is-empty", "is-not-empty" };
        private static readonly Dictionary<string, BuilderSource> Sources = Build();
        private static BuilderField F(string key,string label,string column,string type,bool sensitive=false) { return new BuilderField { Key=key,Label=label,Column=column,Type=type,Sensitive=sensitive,Operators=type=="number"||type=="date"?NumberOps:TextOps }; }
        private static Dictionary<string, BuilderSource> Build()
        {
            var d=new Dictionary<string,BuilderSource>(StringComparer.OrdinalIgnoreCase);
            Action<BuilderSource> add=s=>d[s.Key]=s;
            add(new BuilderSource{Key="students",Name="Students",Category=ReportAuthorization.Student,Handler="students-all",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","academic","registrar","teacher"},Fields=new[]{F("studentCode","Student Code","Student Code","text"),F("fullName","Full Name","Student Name","text"),F("className","Class","Class","text"),F("sectionName","Section","Section","text"),F("gender","Gender","Gender","text"),F("status","Status","Status","text")}});
            add(new BuilderSource{Key="academics",Name="Academics",Category=ReportAuthorization.Academic,Handler="classes",Orientation="Portrait",ExportLimit=10000,Roles=new[]{"superadmin","admin","academic","registrar","teacher"},Fields=new[]{F("className","Class","Class","text"),F("capacity","Capacity","Capacity","number"),F("status","Status","Status","text")}});
            add(new BuilderSource{Key="examinations",Name="Examinations",Category=ReportAuthorization.Examination,Handler="exam-results",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","academic","registrar","teacher"},Fields=new[]{F("student","Student","Student","text"),F("subject","Subject","Subject","text"),F("marks","Marks","Marks","number"),F("grade","Grade","Grade","text"),F("result","Result","Result","text")}});
            add(new BuilderSource{Key="attendance",Name="Attendance",Category=ReportAuthorization.Attendance,Handler="att-student-summary",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","academic","registrar","teacher"},Fields=new[]{F("student","Student","Student","text"),F("present","Present","Present","number"),F("absent","Absent","Absent","number"),F("percentage","Attendance %","Attendance %","number")}});
            add(new BuilderSource{Key="finance",Name="Finance",Category=ReportAuthorization.Finance,Handler="fee-collection",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","accountant"},Fields=new[]{F("student","Student","Student","text"),F("amount","Amount","Amount","number",true),F("date","Date","Date","date"),F("status","Status","Status","text")}});
            add(new BuilderSource{Key="payroll",Name="Payroll",Category=ReportAuthorization.Payroll,Handler="payroll-summary",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","accountant"},Fields=new[]{F("employee","Employee","Employee","text"),F("gross","Gross Pay","Gross Pay","number",true),F("deductions","Deductions","Deductions","number",true),F("net","Net Pay","Net Pay","number",true)}});
            add(new BuilderSource{Key="staff",Name="Staff",Category=ReportAuthorization.TeacherStaff,Handler="staff-all",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","academic","registrar"},Fields=new[]{F("name","Name","Name","text"),F("department","Department","Department","text"),F("role","Role","Role","text"),F("status","Status","Status","text")}});
            add(new BuilderSource{Key="enrollment",Name="Enrollment",Category=ReportAuthorization.Enrollment,Handler="students-active",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","academic","registrar"},Fields=new[]{F("student","Student","Student Name","text"),F("class","Class","Class","text"),F("section","Section","Section","text"),F("status","Status","Status","text")}});
            add(new BuilderSource{Key="guardians",Name="Guardians",Category=ReportAuthorization.Guardian,Handler="guardian-list",Orientation="Landscape",ExportLimit=10000,Roles=new[]{"superadmin","admin","academic","registrar"},Fields=new[]{F("name","Guardian","Guardian","text"),F("phone","Phone","Phone","text"),F("email","Email","Email","text")}});
            add(new BuilderSource{Key="security",Name="Security",Category=ReportAuthorization.Security,Handler="security-users",Orientation="Landscape",ExportLimit=5000,Roles=new[]{"superadmin","admin"},Fields=new[]{F("name","Name","Name","text"),F("email","Email","Email","text"),F("role","Role","Role","text"),F("status","Status","Status","text")}});
            add(new BuilderSource{Key="analytics",Name="Analytics",Category=ReportAuthorization.Performance,Handler="analytics-classes",Orientation="Landscape",ExportLimit=5000,Roles=new[]{"superadmin","admin","academic","registrar"},Fields=new[]{F("class","Class","Class","text"),F("students","Students","Students","number"),F("average","Average","Average","number"),F("passRate","Pass Rate","Pass Rate","number")}});
            return d;
        }
        public static IEnumerable<BuilderSource> ForRole(string role) { string r=ReportAuthorization.NormalizeRole(role); return Sources.Values.Where(s=>s.Roles.Contains(r)&&ReportAuthorization.CanViewCategory(role,s.Category)); }
        public static BuilderSource Resolve(string key) { BuilderSource s; return !string.IsNullOrWhiteSpace(key)&&Sources.TryGetValue(key,out s)?s:null; }
        public static BuilderValidation Validate(string json,string role)
        {
            try {
                var c=new JavaScriptSerializer{MaxJsonLength=65536}.Deserialize<BuilderConfig>(json??""); var s=c==null?null:Resolve(c.sourceKey);
                if(c==null||c.version!=1) return Bad("Unsupported configuration version.");
                if(s==null||!ForRole(role).Any(x=>x.Key.Equals(s.Key,StringComparison.OrdinalIgnoreCase))) return Bad("Unsupported or unauthorized data source.");
                c.columns=c.columns??new string[0]; c.filters=c.filters??new BuilderFilter[0]; c.groupBy=c.groupBy??new string[0]; c.summaries=c.summaries??new string[0];
                if(c.columns.Length<1||c.columns.Length>MaxColumns||c.columns.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=c.columns.Length) return Bad("Select 1 to 20 unique columns.");
                if(c.filters.Length>MaxFilters||c.groupBy.Length>MaxGroups) return Bad("Configuration limits exceeded.");
                foreach(string k in c.columns.Concat(c.groupBy)) if(!s.Fields.Any(f=>f.Key.Equals(k,StringComparison.OrdinalIgnoreCase))) return Bad("Unknown field rejected.");
                foreach(var f in c.filters){var m=s.Fields.FirstOrDefault(x=>x.Key.Equals(f.field??"",StringComparison.OrdinalIgnoreCase));if(m==null||!m.Operators.Contains(f.@operator??""))return Bad("Invalid field/operator combination.");}
                if(c.sort!=null&&(!s.Fields.Any(f=>f.Key.Equals(c.sort.field??"",StringComparison.OrdinalIgnoreCase))||(c.sort.direction!="asc"&&c.sort.direction!="desc")))return Bad("Invalid sort configuration.");
                if(c.summaries.Any(x=>!new[]{"count","sum","average","minimum","maximum","percentage"}.Contains((x??"").ToLowerInvariant())))return Bad("Invalid summary function.");
                if(!string.IsNullOrEmpty(c.chart)&&!new[]{"bar","line","doughnut"}.Contains(c.chart))return Bad("Invalid chart type.");
                if(!new[]{"Portrait","Landscape"}.Contains(c.orientation))return Bad("Invalid page orientation.");
                return new BuilderValidation{Valid=true,Source=s,Config=c};
            } catch { return Bad("Configuration could not be validated."); }
        }
        private static BuilderValidation Bad(string m){return new BuilderValidation{Valid=false,Message=m};}
        public static string Serialize(BuilderConfig c){return new JavaScriptSerializer{MaxJsonLength=65536}.Serialize(c);}
        public static DataTable Execute(ReportsRepository repo,BuilderValidation v,int limit)
        {
            var raw=repo.GetReportData(v.Source.Handler,new ReportFilter(),false)??new DataTable(); var result=new DataTable();
            var selected=v.Config.columns.Select(k=>v.Source.Fields.First(f=>f.Key.Equals(k,StringComparison.OrdinalIgnoreCase))).Where(f=>raw.Columns.Contains(f.Column)).ToArray();
            foreach(var f in selected)result.Columns.Add(f.Label,typeof(string)); IEnumerable<DataRow> rows=raw.AsEnumerable();
            foreach(var filter in v.Config.filters){var f=v.Source.Fields.First(x=>x.Key.Equals(filter.field,StringComparison.OrdinalIgnoreCase));if(!raw.Columns.Contains(f.Column))continue;rows=rows.Where(r=>Match(Convert.ToString(r[f.Column]),filter));}
            if(v.Config.sort!=null){var f=v.Source.Fields.First(x=>x.Key.Equals(v.Config.sort.field,StringComparison.OrdinalIgnoreCase));if(raw.Columns.Contains(f.Column))rows=v.Config.sort.direction=="desc"?rows.OrderByDescending(r=>Convert.ToString(r[f.Column])):rows.OrderBy(r=>Convert.ToString(r[f.Column]));}
            foreach(var row in rows.Take(Math.Max(1,Math.Min(limit,v.Source.ExportLimit)))){var n=result.NewRow();for(int i=0;i<selected.Length;i++)n[i]=Convert.ToString(row[selected[i].Column]);result.Rows.Add(n);} return result;
        }
        private static bool Match(string actual,BuilderFilter f){actual=actual??"";string val=f.value??"";switch(f.@operator){case"equals":return actual.Equals(val,StringComparison.OrdinalIgnoreCase);case"not-equals":return!actual.Equals(val,StringComparison.OrdinalIgnoreCase);case"contains":return actual.IndexOf(val,StringComparison.OrdinalIgnoreCase)>=0;case"starts-with":return actual.StartsWith(val,StringComparison.OrdinalIgnoreCase);case"ends-with":return actual.EndsWith(val,StringComparison.OrdinalIgnoreCase);case"is-empty":return string.IsNullOrWhiteSpace(actual);case"is-not-empty":return!string.IsNullOrWhiteSpace(actual);default:decimal a,b,c;if(!decimal.TryParse(actual,out a)||!decimal.TryParse(val,out b))return false;if(f.@operator=="greater-than")return a>b;if(f.@operator=="less-than")return a<b;if(f.@operator=="greater-or-equal")return a>=b;if(f.@operator=="less-or-equal")return a<=b;if(f.@operator=="between"&&decimal.TryParse(f.value2,out c))return a>=b&&a<=c;return a==b;}}
    }
}
