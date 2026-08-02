using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Linq;

namespace AQOONHUB_SMS.Modules.Administration
{
    public sealed class PagedData { public DataTable Rows { get; set; } public int Total { get; set; } }
    public sealed class SystemRepository
    {
        private readonly string cs;
        public SystemRepository()
        {
            var item = ConfigurationManager.ConnectionStrings["AQOONHUB_DB"];
            if (item == null || string.IsNullOrWhiteSpace(item.ConnectionString)) throw new ConfigurationErrorsException("Database configuration is unavailable.");
            cs = item.ConnectionString;
        }
        private DataTable Query(string sql, params SqlParameter[] args)
        {
            using (var cn = new SqlConnection(cs)) using (var cmd = new SqlCommand(sql, cn)) using (var da = new SqlDataAdapter(cmd))
            { if (args != null) cmd.Parameters.AddRange(args); var dt = new DataTable(); da.Fill(dt); return dt; }
        }
        private int Execute(string sql, params SqlParameter[] args)
        {
            using (var cn = new SqlConnection(cs)) using (var cmd = new SqlCommand(sql, cn))
            { if (args != null) cmd.Parameters.AddRange(args); cn.Open(); return cmd.ExecuteNonQuery(); }
        }
        public DataTable Roles() { return Query(@"SELECT r.RoleID,r.RoleName,ISNULL(r.Description,'') Description,r.IsActive,COUNT(DISTINCT u.UserID) UserCount,COUNT(DISTINCT rp.PermissionID) PermissionCount FROM Roles r LEFT JOIN Users u ON REPLACE(REPLACE(LOWER(u.Role),' ',''),'-','')=REPLACE(REPLACE(LOWER(r.RoleName),' ',''),'-','') LEFT JOIN RolePermissions rp ON rp.RoleID=r.RoleID GROUP BY r.RoleID,r.RoleName,r.Description,r.IsActive ORDER BY r.RoleName"); }
        public DataTable Permissions(int roleId) { return Query(@"SELECT p.PermissionID,p.Module,p.PermissionName,p.Description,CAST(CASE WHEN rp.RolePermissionID IS NULL THEN 0 ELSE 1 END AS bit) IsGranted FROM Permissions p LEFT JOIN RolePermissions rp ON rp.PermissionID=p.PermissionID AND rp.RoleID=@RoleID WHERE p.IsActive=1 ORDER BY p.Module,p.PermissionName", new SqlParameter("@RoleID", roleId)); }
        public DataTable Users(string search, string role, string status)
        {
            return Query(@"SELECT UserID,FullName,Email,Phone,Role,ISNULL(IsActive,0) IsActive,LastLogin,CreatedAt FROM Users WHERE (@Search='' OR FullName LIKE '%'+@Search+'%' OR Email LIKE '%'+@Search+'%') AND (@Role='' OR Role=@Role) AND (@Status='' OR ISNULL(IsActive,0)=CASE WHEN @Status='Active' THEN 1 ELSE 0 END) ORDER BY FullName", new SqlParameter("@Search", search ?? ""),new SqlParameter("@Role",role ?? ""),new SqlParameter("@Status",status ?? ""));
        }
        public DataRow User(int id) { var t=Query("SELECT u.UserID,u.FullName,u.Email,u.Phone,COALESCE((SELECT TOP 1 r.RoleName FROM Roles r WHERE REPLACE(REPLACE(REPLACE(LOWER(r.RoleName),' ',''),'-',''),'_','')=REPLACE(REPLACE(REPLACE(LOWER(u.Role),' ',''),'-',''),'_','')),u.Role) Role,ISNULL(u.IsActive,0) IsActive,u.CreatedAt,u.LastLogin FROM Users u WHERE u.UserID=@ID",new SqlParameter("@ID",id)); return t.Rows.Count==0?null:t.Rows[0]; }
        public void SaveUser(int? id,string name,string email,string phone,string role,bool active,string passwordHash)
        {
            if(id.HasValue) Execute(@"UPDATE Users SET FullName=@Name,Email=@Email,Phone=NULLIF(@Phone,''),Role=@Role,IsActive=@Active,UpdatedAt=GETDATE() WHERE UserID=@ID",new SqlParameter("@Name",name),new SqlParameter("@Email",email),new SqlParameter("@Phone",phone??""),new SqlParameter("@Role",role),new SqlParameter("@Active",active),new SqlParameter("@ID",id.Value));
            else Execute(@"INSERT Users(FullName,Email,PasswordHash,Phone,Role,IsActive,CreatedAt,UpdatedAt) VALUES(@Name,@Email,@Hash,NULLIF(@Phone,''),@Role,@Active,GETDATE(),GETDATE())",new SqlParameter("@Name",name),new SqlParameter("@Email",email),new SqlParameter("@Hash",passwordHash),new SqlParameter("@Phone",phone??""),new SqlParameter("@Role",role),new SqlParameter("@Active",active));
        }
        public bool EmailExists(string email,int? exceptId) { return Convert.ToInt32(Query("SELECT COUNT(*) N FROM Users WHERE LOWER(Email)=LOWER(@Email) AND (@ID IS NULL OR UserID<>@ID)",new SqlParameter("@Email",email),new SqlParameter("@ID",(object)exceptId??DBNull.Value)).Rows[0][0])>0; }
        public void ToggleUser(int id,int actor) { Execute("UPDATE Users SET IsActive=CASE WHEN ISNULL(IsActive,0)=1 THEN 0 ELSE 1 END,UpdatedAt=GETDATE() WHERE UserID=@ID AND UserID<>@Actor",new SqlParameter("@ID",id),new SqlParameter("@Actor",actor)); }
        public void AssignRole(int userId,int roleId,int actor)
        {
            Execute(@"DECLARE @Role nvarchar(50)=(SELECT RoleName FROM Roles WHERE RoleID=@RoleID AND IsActive=1); IF @Role IS NULL THROW 50001,'Invalid role',1; UPDATE Users SET Role=@Role,UpdatedAt=GETDATE() WHERE UserID=@UserID; IF NOT EXISTS(SELECT 1 FROM UserRoles WHERE UserID=@UserID AND RoleID=@RoleID) INSERT UserRoles(UserID,RoleID,AssignedBy,AssignedAt) VALUES(@UserID,@RoleID,@Actor,GETDATE());",new SqlParameter("@UserID",userId),new SqlParameter("@RoleID",roleId),new SqlParameter("@Actor",actor));
        }
        public void SetPermission(int roleId,int permissionId,bool granted)
        {
            if(granted) Execute("IF NOT EXISTS(SELECT 1 FROM RolePermissions WHERE RoleID=@R AND PermissionID=@P) INSERT RolePermissions(RoleID,PermissionID,AssignedAt) VALUES(@R,@P,GETDATE())",new SqlParameter("@R",roleId),new SqlParameter("@P",permissionId));
            else Execute("DELETE RolePermissions WHERE RoleID=@R AND PermissionID=@P",new SqlParameter("@R",roleId),new SqlParameter("@P",permissionId));
        }
        public DataTable Audit(DateTime? from,DateTime? to,string user,string module,string action)
        { return Query(@"SELECT al.AuditID,al.ActionTime,ISNULL(u.FullName,'System') UserName,al.Module,al.Action,al.Detail,ISNULL(al.IPAddress,'Unavailable') IPAddress,CASE WHEN al.Action LIKE '%FAIL%' OR al.Action='DELETE' THEN 'Failed' WHEN al.Action LIKE '%WARN%' THEN 'Warning' ELSE 'Success' END Status FROM AuditLog al LEFT JOIN Users u ON u.UserID=al.UserID WHERE (@From IS NULL OR al.ActionTime>=@From) AND (@To IS NULL OR al.ActionTime<DATEADD(day,1,@To)) AND (@User='' OR u.FullName LIKE '%'+@User+'%' OR u.Email LIKE '%'+@User+'%') AND (@Module='' OR al.Module=@Module) AND (@Action='' OR al.Action=@Action) ORDER BY al.ActionTime DESC",new SqlParameter("@From",(object)from??DBNull.Value),new SqlParameter("@To",(object)to??DBNull.Value),new SqlParameter("@User",user??""),new SqlParameter("@Module",module??""),new SqlParameter("@Action",action??"")); }
        public DataTable Login(DateTime? from,DateTime? to,string role,string status,string search)
        { return Query(@"SELECT la.LoginID,la.LoginTime,ISNULL(u.FullName,COALESCE(NULLIF(la.Email,''),'Unknown account')) UserName,ISNULL(u.Email,la.Email) Email,ISNULL(u.Role,'Unknown') Role,COALESCE(NULLIF(la.DeviceInfo,''),NULLIF(la.Device,''),'Unavailable') DeviceInfo,ISNULL(la.IPAddress,'Unavailable') IPAddress,la.Status,ISNULL(la.FailureReason,'') FailureReason FROM LoginActivity la LEFT JOIN Users u ON u.UserID=la.UserID WHERE (@From IS NULL OR la.LoginTime>=@From) AND (@To IS NULL OR la.LoginTime<DATEADD(day,1,@To)) AND (@Role='' OR u.Role=@Role) AND (@Status='' OR la.Status=@Status) AND (@Search='' OR u.FullName LIKE '%'+@Search+'%' OR u.Email LIKE '%'+@Search+'%' OR la.Email LIKE '%'+@Search+'%') ORDER BY la.LoginTime DESC",new SqlParameter("@From",(object)from??DBNull.Value),new SqlParameter("@To",(object)to??DBNull.Value),new SqlParameter("@Role",role??""),new SqlParameter("@Status",status??""),new SqlParameter("@Search",search??"")); }
        public DataTable LoginTrend(DateTime from,DateTime to) { return Query(@"WITH d AS(SELECT CAST(@From AS date) D UNION ALL SELECT DATEADD(day,1,D) FROM d WHERE D<CAST(@To AS date)) SELECT d.D, SUM(CASE WHEN la.Status='Success' THEN 1 ELSE 0 END) SuccessCount,SUM(CASE WHEN la.Status<>'Success' THEN 1 ELSE 0 END) FailedCount FROM d LEFT JOIN LoginActivity la ON CAST(la.LoginTime AS date)=d.D GROUP BY d.D ORDER BY d.D OPTION(MAXRECURSION 370)",new SqlParameter("@From",from),new SqlParameter("@To",to)); }
        public DataRow SchoolSettings() { var t=Query("SELECT TOP 1 * FROM SchoolSettings ORDER BY SettingID"); return t.Rows.Count==0?null:t.Rows[0]; }
        public void SaveSchool(string name,string address,string phone,string email,string currency,string timezone,string language)
        { Execute(@"UPDATE SchoolSettings SET SchoolName=@Name,Address=NULLIF(@Address,''),Phone=NULLIF(@Phone,''),Email=NULLIF(@Email,''),Currency=@Currency,TimeZone=@Timezone,Language=@Language,UpdatedAt=GETDATE() WHERE SettingID=(SELECT TOP 1 SettingID FROM SchoolSettings ORDER BY SettingID)",new SqlParameter("@Name",name),new SqlParameter("@Address",address??""),new SqlParameter("@Phone",phone??""),new SqlParameter("@Email",email??""),new SqlParameter("@Currency",currency),new SqlParameter("@Timezone",timezone),new SqlParameter("@Language",language)); }
        public void Log(int? userId,string action,string module,string detail,string ip) { Execute("INSERT AuditLog(UserID,Action,Module,Detail,IPAddress,ActionTime) VALUES(@U,@A,@M,@D,@IP,GETDATE())",new SqlParameter("@U",(object)userId??DBNull.Value),new SqlParameter("@A",action),new SqlParameter("@M",module),new SqlParameter("@D",detail),new SqlParameter("@IP",ip??"Unavailable")); }

        private PagedData Paged(string countSql,string rowsSql,int page,int size,params SqlParameter[] args)
        {
            page=Math.Max(1,page); size=new[]{10,25,50,100}.Contains(size)?size:10;
            var countArgs=args.Select(x=>new SqlParameter(x.ParameterName,x.Value)).ToArray();
            int total=Convert.ToInt32(Query(countSql,countArgs).Rows[0][0]); int pages=Math.Max(1,(int)Math.Ceiling(total/(double)size));page=Math.Min(page,pages);
            var list=args.Select(x=>new SqlParameter(x.ParameterName,x.Value)).ToList();list.Add(new SqlParameter("@Offset",(page-1)*size));list.Add(new SqlParameter("@Size",size));
            return new PagedData{Rows=Query(rowsSql,list.ToArray()),Total=total};
        }
        public PagedData UsersPage(string search,string role,string status,int page,int size)
        {
            const string where=" FROM Users WHERE (@Search='' OR FullName LIKE '%'+@Search+'%' OR Email LIKE '%'+@Search+'%') AND (@Role='' OR Role=@Role) AND (@Status='' OR ISNULL(IsActive,0)=CASE WHEN @Status='Active' THEN 1 ELSE 0 END)";
            var a=new[]{new SqlParameter("@Search",search??""),new SqlParameter("@Role",role??""),new SqlParameter("@Status",status??"")};
            return Paged("SELECT COUNT(*)"+where,"SELECT UserID,FullName,Email,Phone,Role,ISNULL(IsActive,0) IsActive,LastLogin,CreatedAt"+where+" ORDER BY FullName,UserID OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY",page,size,a);
        }
        public int UserCount(string status){return Convert.ToInt32(Query("SELECT COUNT(*) FROM Users WHERE (@S='' OR ISNULL(IsActive,0)=CASE WHEN @S='Active' THEN 1 ELSE 0 END)",new SqlParameter("@S",status??"")).Rows[0][0]);}
        public void UpdateUserSafe(int id,string name,string email,string phone,string role,bool active,int actor)
        {
            using(var cn=new SqlConnection(cs)){cn.Open();using(var tx=cn.BeginTransaction()){try{var cmd=new SqlCommand(@"IF NOT EXISTS(SELECT 1 FROM Users WHERE UserID=@ID) THROW 50001,'User not found.',1; IF NOT EXISTS(SELECT 1 FROM Roles WHERE RoleName=@Role AND IsActive=1) THROW 50002,'Role is invalid.',1; IF EXISTS(SELECT 1 FROM Users WHERE LOWER(Email)=LOWER(@Email) AND UserID<>@ID) THROW 50003,'Email is already in use.',1; IF EXISTS(SELECT 1 FROM Users WHERE UserID=@ID AND REPLACE(LOWER(Role),' ','')='superadmin' AND @Active=0) AND (SELECT COUNT(*) FROM Users WHERE REPLACE(LOWER(Role),' ','')='superadmin' AND ISNULL(IsActive,0)=1)<=1 THROW 50004,'The last active Super Admin cannot be deactivated.',1; UPDATE Users SET FullName=@Name,Email=@Email,Phone=NULLIF(@Phone,''),Role=@Role,IsActive=@Active,UpdatedAt=GETDATE() WHERE UserID=@ID;",cn,tx);cmd.Parameters.AddRange(new[]{new SqlParameter("@ID",id),new SqlParameter("@Name",name),new SqlParameter("@Email",email),new SqlParameter("@Phone",phone??""),new SqlParameter("@Role",role),new SqlParameter("@Active",active)});cmd.ExecuteNonQuery();tx.Commit();}catch{tx.Rollback();throw;}}}
        }
        public void DeactivateUser(int id,int actor){if(id==actor)throw new InvalidOperationException("You cannot deactivate your own account.");var u=User(id);if(u==null)throw new InvalidOperationException("User not found.");UpdateUserSafe(id,u["FullName"].ToString(),u["Email"].ToString(),u["Phone"].ToString(),u["Role"].ToString(),false,actor);}
        public void SavePermissions(int roleId,IEnumerable<int> permissionIds)
        {
            var ids=(permissionIds??Enumerable.Empty<int>()).Distinct().ToArray();using(var cn=new SqlConnection(cs)){cn.Open();using(var tx=cn.BeginTransaction()){try{var check=new SqlCommand("SELECT RoleName FROM Roles WHERE RoleID=@R AND IsActive=1",cn,tx);check.Parameters.AddWithValue("@R",roleId);string role=Convert.ToString(check.ExecuteScalar());if(string.IsNullOrEmpty(role))throw new InvalidOperationException("Role not found.");if(NormalizeRole(role)=="superadmin")throw new InvalidOperationException("Super Admin permissions are protected.");if(ids.Length>0){var valid=new SqlCommand("SELECT COUNT(*) FROM Permissions WHERE IsActive=1 AND PermissionID IN ("+string.Join(",",ids)+")",cn,tx);if(Convert.ToInt32(valid.ExecuteScalar())!=ids.Length)throw new InvalidOperationException("One or more permissions are invalid.");}var del=new SqlCommand("DELETE RolePermissions WHERE RoleID=@R",cn,tx);del.Parameters.AddWithValue("@R",roleId);del.ExecuteNonQuery();foreach(int id in ids){var ins=new SqlCommand("INSERT RolePermissions(RoleID,PermissionID,AssignedAt) VALUES(@R,@P,GETDATE())",cn,tx);ins.Parameters.AddWithValue("@R",roleId);ins.Parameters.AddWithValue("@P",id);ins.ExecuteNonQuery();}tx.Commit();}catch{tx.Rollback();throw;}}}
        }
        private string NormalizeRole(string r){return(r??"").Replace(" ","").Replace("-","").Replace("_","").ToLowerInvariant();}
        public PagedData AuditPage(DateTime? from,DateTime? to,string user,string module,string action,int page,int size)
        {const string w=" FROM AuditLog al LEFT JOIN Users u ON u.UserID=al.UserID WHERE (@From IS NULL OR al.ActionTime>=@From) AND (@To IS NULL OR al.ActionTime<DATEADD(day,1,@To)) AND (@User='' OR u.FullName LIKE '%'+@User+'%' OR u.Email LIKE '%'+@User+'%') AND (@Module='' OR al.Module=@Module) AND (@Action='' OR al.Action=@Action)";var a=new[]{new SqlParameter("@From",(object)from??DBNull.Value),new SqlParameter("@To",(object)to??DBNull.Value),new SqlParameter("@User",user??""),new SqlParameter("@Module",module??""),new SqlParameter("@Action",action??"")};return Paged("SELECT COUNT(*)"+w,"SELECT al.AuditID,al.ActionTime,ISNULL(u.FullName,'System') UserName,al.Module,al.Action,al.Detail,ISNULL(al.IPAddress,'Unavailable') IPAddress,CASE WHEN al.Action LIKE '%FAIL%' OR al.Action='DELETE' THEN 'Failed' WHEN al.Action LIKE '%WARN%' THEN 'Warning' ELSE 'Success' END Status"+w+" ORDER BY al.ActionTime DESC,al.AuditID DESC OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY",page,size,a);}
        public DataRow AuditDetail(long id){var t=Query("SELECT al.AuditID,al.ActionTime,ISNULL(u.FullName,'System') UserName,al.Module,al.Action,al.Detail,ISNULL(al.IPAddress,'Unavailable') IPAddress,CASE WHEN al.Action LIKE '%FAIL%' OR al.Action='DELETE' THEN 'Failed' WHEN al.Action LIKE '%WARN%' THEN 'Warning' ELSE 'Success' END Status FROM AuditLog al LEFT JOIN Users u ON u.UserID=al.UserID WHERE al.AuditID=@ID",new SqlParameter("@ID",id));return t.Rows.Count==0?null:t.Rows[0];}
        public PagedData LoginPage(DateTime? from,DateTime? to,string role,string status,string search,int page,int size)
        {const string w=" FROM LoginActivity la LEFT JOIN Users u ON u.UserID=la.UserID WHERE (@From IS NULL OR la.LoginTime>=@From) AND (@To IS NULL OR la.LoginTime<DATEADD(day,1,@To)) AND (@Role='' OR u.Role=@Role) AND (@Status='' OR la.Status=@Status) AND (@Search='' OR u.FullName LIKE '%'+@Search+'%' OR u.Email LIKE '%'+@Search+'%' OR la.Email LIKE '%'+@Search+'%')";var a=new[]{new SqlParameter("@From",(object)from??DBNull.Value),new SqlParameter("@To",(object)to??DBNull.Value),new SqlParameter("@Role",role??""),new SqlParameter("@Status",status??""),new SqlParameter("@Search",search??"")};return Paged("SELECT COUNT(*)"+w,"SELECT la.LoginID,la.LoginTime,ISNULL(u.FullName,COALESCE(NULLIF(la.Email,''),'Unknown account')) UserName,ISNULL(u.Email,la.Email) Email,ISNULL(u.Role,'Unknown') Role,COALESCE(NULLIF(la.DeviceInfo,''),NULLIF(la.Device,''),'Unavailable') DeviceInfo,ISNULL(la.IPAddress,'Unavailable') IPAddress,la.Status,ISNULL(la.FailureReason,'') FailureReason"+w+" ORDER BY la.LoginTime DESC,la.LoginID DESC OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY",page,size,a);}
        public DataRow LoginDetail(long id){var t=Query("SELECT la.LoginID,la.LoginTime,ISNULL(u.FullName,COALESCE(NULLIF(la.Email,''),'Unknown account')) UserName,ISNULL(u.Email,la.Email) Email,ISNULL(u.Role,'Unknown') Role,COALESCE(NULLIF(la.DeviceInfo,''),NULLIF(la.Device,''),'Unavailable') DeviceInfo,ISNULL(la.IPAddress,'Unavailable') IPAddress,la.Status,ISNULL(la.FailureReason,'') FailureReason FROM LoginActivity la LEFT JOIN Users u ON u.UserID=la.UserID WHERE la.LoginID=@ID",new SqlParameter("@ID",id));return t.Rows.Count==0?null:t.Rows[0];}
    }
}
