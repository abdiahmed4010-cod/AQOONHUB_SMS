using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace AQOONHUB_SMS.Modules.Parents
{
    /// <summary>
    /// Safe, transactional provisioning of a Parent login account for a Student's Guardian.
    /// Canonical relationship stays Students.GuardianID -> Guardians.GuardianID -> Guardians.UserID -> Users.UserID.
    ///
    /// Passwords are hashed in the SAME format the existing Login page already verifies
    /// (PBKDF2-SHA256 "iterations:base64(salt):base64(hash)"), so no second auth system is
    /// introduced and the temporary password genuinely works at login. The plain temporary
    /// password is returned to the caller ONCE and is never stored or logged.
    /// </summary>
    public static class ParentAccountService
    {
        private const int Pbkdf2Iterations = 100000;
        private const int SaltBytes = 16;
        private const int HashBytes = 32;

        private static string Cs
        {
            get
            {
                var item = ConfigurationManager.ConnectionStrings["AQOONHUB_DB"];
                if (item == null || string.IsNullOrWhiteSpace(item.ConnectionString))
                    throw new ConfigurationErrorsException("Database configuration is unavailable.");
                return item.ConnectionString;
            }
        }

        public enum Outcome { CreatedNew, LinkedExisting, GuardianNotFound, AlreadyLinked, InvalidEmail, ConflictLinkedElsewhere, Error }

        public sealed class GuardianStatus
        {
            public int GuardianID;
            public string Name;
            public string Email;
            public string Phone;
            public bool IsActive;
            public int? UserID;
            public string LinkedUserEmail;
            public bool UserActive;

            /// <summary>"Linked Account" / "Inactive Account" / "No Account".</summary>
            public string AccountStatus
            {
                get
                {
                    if (!UserID.HasValue) return "No Account";
                    return UserActive ? "Linked Account" : "Inactive Account";
                }
            }

            public bool CanProvision
            {
                get { return !UserID.HasValue && !string.IsNullOrEmpty(Email) && Administration.SecurityHelper.IsValidEmail(Email.Trim()); }
            }
        }

        /// <summary>Read-only guardian + linked-account status. Never exposes any hash.</summary>
        public static GuardianStatus GetStatus(int guardianId)
        {
            using (var conn = new SqlConnection(Cs))
            using (var cmd = new SqlCommand(@"
                SELECT g.GuardianID, g.FullName, g.Email, g.Phone, ISNULL(g.IsActive,1) AS GActive,
                       g.UserID, u.Email AS UserEmail, ISNULL(u.IsActive,0) AS UActive
                FROM dbo.Guardians g
                LEFT JOIN dbo.Users u ON u.UserID = g.UserID
                WHERE g.GuardianID = @g", conn))
            {
                cmd.Parameters.AddWithValue("@g", guardianId);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    return new GuardianStatus
                    {
                        GuardianID = Convert.ToInt32(r["GuardianID"]),
                        Name = Convert.ToString(r["FullName"]),
                        Email = r["Email"] == DBNull.Value ? null : Convert.ToString(r["Email"]),
                        Phone = r["Phone"] == DBNull.Value ? null : Convert.ToString(r["Phone"]),
                        IsActive = Convert.ToBoolean(r["GActive"]),
                        UserID = r["UserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["UserID"]),
                        LinkedUserEmail = r["UserEmail"] == DBNull.Value ? null : Convert.ToString(r["UserEmail"]),
                        UserActive = Convert.ToBoolean(r["UActive"])
                    };
                }
            }
        }

        /// <summary>
        /// Provisions or links a Parent account for the guardian in ONE transaction.
        /// On CreatedNew, <paramref name="tempPassword"/> holds the one-time password.
        /// </summary>
        public static Outcome Provision(int guardianId, int? actorUserId, string ip, out string tempPassword, out string message)
        {
            tempPassword = null;
            message = null;

            using (var conn = new SqlConnection(Cs))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1-3. Lock the guardian row; confirm still unlinked.
                        string gName, gEmail;
                        using (var cmd = new SqlCommand(
                            "SELECT FullName, Email, UserID FROM dbo.Guardians WITH (UPDLOCK, HOLDLOCK) WHERE GuardianID=@g", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@g", guardianId);
                            using (var r = cmd.ExecuteReader())
                            {
                                if (!r.Read()) { tx.Rollback(); message = "Guardian not found."; return Outcome.GuardianNotFound; }
                                if (r["UserID"] != DBNull.Value) { tx.Rollback(); message = "This guardian already has a linked account."; return Outcome.AlreadyLinked; }
                                gName = Convert.ToString(r["FullName"]);
                                gEmail = r["Email"] == DBNull.Value ? null : Convert.ToString(r["Email"]);
                            }
                        }

                        // 4. Validate/normalize email.
                        if (string.IsNullOrWhiteSpace(gEmail) || !Administration.SecurityHelper.IsValidEmail(gEmail.Trim()))
                        { tx.Rollback(); message = "The guardian does not have a valid email address."; return Outcome.InvalidEmail; }
                        string email = gEmail.Trim();

                        int parentRoleId = 0;
                        using (var cmd = new SqlCommand("SELECT RoleID FROM dbo.Roles WHERE RoleName='Parent' AND IsActive=1", conn, tx))
                        { object o = cmd.ExecuteScalar(); if (o != null && o != DBNull.Value) parentRoleId = Convert.ToInt32(o); }

                        // 5. Existing user by normalized email?
                        int existingUserId = 0;
                        using (var cmd = new SqlCommand("SELECT TOP 1 UserID FROM dbo.Users WHERE LOWER(Email)=LOWER(@e)", conn, tx))
                        { cmd.Parameters.AddWithValue("@e", email); object o = cmd.ExecuteScalar(); if (o != null && o != DBNull.Value) existingUserId = Convert.ToInt32(o); }

                        int linkUserId;
                        Outcome outcome;

                        if (existingUserId > 0)
                        {
                            // 6. Confirm not already linked to a DIFFERENT guardian.
                            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Guardians WHERE UserID=@u AND GuardianID<>@g", conn, tx))
                            { cmd.Parameters.AddWithValue("@u", existingUserId); cmd.Parameters.AddWithValue("@g", guardianId); if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) { tx.Rollback(); message = "That email already belongs to a user linked to a different guardian."; return Outcome.ConflictLinkedElsewhere; } }

                            // Assign Parent role when missing (do not reset an existing password).
                            using (var cmd = new SqlCommand(
                                "UPDATE dbo.Users SET Role='Parent', UpdatedAt=GETDATE() WHERE UserID=@u AND REPLACE(REPLACE(LOWER(ISNULL(Role,'')),' ',''),'-','')<>'parent'", conn, tx))
                            { cmd.Parameters.AddWithValue("@u", existingUserId); cmd.ExecuteNonQuery(); }

                            linkUserId = existingUserId;
                            outcome = Outcome.LinkedExisting;
                            message = "An existing user with this email was linked as the parent account.";
                        }
                        else
                        {
                            // 7. New secure account. PBKDF2 hash matches Login's verifier.
                            tempPassword = GenerateTempPassword();
                            string hash = HashPassword(tempPassword);
                            using (var cmd = new SqlCommand(@"
                                INSERT INTO dbo.Users (FullName, Email, PasswordHash, Phone, Role, IsActive, MustChangePassword, CreatedAt, UpdatedAt)
                                OUTPUT INSERTED.UserID
                                VALUES (@name, @email, @hash, NULL, 'Parent', 1, 1, GETDATE(), GETDATE())", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@name", string.IsNullOrWhiteSpace(gName) ? "Parent" : gName);
                                cmd.Parameters.AddWithValue("@email", email);
                                cmd.Parameters.AddWithValue("@hash", hash);
                                linkUserId = Convert.ToInt32(cmd.ExecuteScalar());
                            }
                            outcome = Outcome.CreatedNew;
                            message = "Parent account created successfully.";
                        }

                        // Ensure UserRoles mapping (no duplicate).
                        if (parentRoleId > 0)
                        {
                            using (var cmd = new SqlCommand(
                                "IF NOT EXISTS(SELECT 1 FROM dbo.UserRoles WHERE UserID=@u AND RoleID=@r) INSERT dbo.UserRoles(UserID,RoleID,AssignedBy,AssignedAt) VALUES(@u,@r,@by,GETDATE())", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@u", linkUserId);
                                cmd.Parameters.AddWithValue("@r", parentRoleId);
                                cmd.Parameters.AddWithValue("@by", (object)actorUserId ?? DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Link guardian only after user + role succeed.
                        using (var cmd = new SqlCommand("UPDATE dbo.Guardians SET UserID=@u, UpdatedAt=SYSDATETIME() WHERE GuardianID=@g AND UserID IS NULL", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@u", linkUserId);
                            cmd.Parameters.AddWithValue("@g", guardianId);
                            if (cmd.ExecuteNonQuery() != 1) { tx.Rollback(); tempPassword = null; message = "The guardian was linked concurrently. No change made."; return Outcome.AlreadyLinked; }
                        }

                        // Safe audit (never the password).
                        using (var cmd = new SqlCommand(
                            "INSERT dbo.AuditLog(UserID,Action,Module,Detail,IPAddress,ActionTime) VALUES(@by,@action,'Parents',@detail,@ip,GETDATE())", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@by", (object)actorUserId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@action", outcome == Outcome.CreatedNew ? "PARENT_ACCOUNT_CREATED" : "PARENT_ACCOUNT_LINKED");
                            cmd.Parameters.AddWithValue("@detail", "Guardian #" + guardianId + " linked to User #" + linkUserId);
                            cmd.Parameters.AddWithValue("@ip", (object)(ip ?? "Unavailable"));
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return outcome;
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { }
                        tempPassword = null;
                        message = "The parent account could not be created due to a system error. Please try again.";
                        return Outcome.Error;
                    }
                }
            }
        }

        // -- Secure temporary password: cryptographically random, unpredictable, 14 chars. --
        private static string GenerateTempPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digit = "23456789";
            const string sym = "@#%+=?";
            string all = upper + lower + digit + sym;
            var chars = new char[14];
            // Guarantee one of each class, then fill the rest.
            chars[0] = upper[RandInt(upper.Length)];
            chars[1] = lower[RandInt(lower.Length)];
            chars[2] = digit[RandInt(digit.Length)];
            chars[3] = sym[RandInt(sym.Length)];
            for (int i = 4; i < chars.Length; i++) chars[i] = all[RandInt(all.Length)];
            // Fisher-Yates shuffle with secure randomness.
            for (int i = chars.Length - 1; i > 0; i--) { int j = RandInt(i + 1); var t = chars[i]; chars[i] = chars[j]; chars[j] = t; }
            return new string(chars);
        }

        private static int RandInt(int maxExclusive)
        {
            var buf = new byte[4];
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Rejection sampling to avoid modulo bias.
                int limit = int.MaxValue - (int.MaxValue % maxExclusive);
                int v;
                do { rng.GetBytes(buf); v = BitConverter.ToInt32(buf, 0) & int.MaxValue; } while (v >= limit);
                return v % maxExclusive;
            }
        }

        /// <summary>PBKDF2-SHA256 hash in the exact "iterations:base64(salt):base64(hash)" format Login verifies.</summary>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltBytes];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashBytes);
                return Pbkdf2Iterations.ToString() + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verifies a password against a stored hash using the SAME rules as Login.aspx.cs:
        /// a 3-part "iterations:salt:hash" value is verified as PBKDF2-SHA256; any other value
        /// is compared literally (legacy fallback). Centralised so Change Password reuses one
        /// format without altering Login's own verification path.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password)) return false;

            if (storedHash.Contains(":"))
            {
                string[] parts = storedHash.Split(':');
                if (parts.Length == 3)
                {
                    try
                    {
                        int iterations = int.Parse(parts[0]);
                        byte[] salt = Convert.FromBase64String(parts[1]);
                        byte[] stored = Convert.FromBase64String(parts[2]);
                        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                        {
                            byte[] computed = pbkdf2.GetBytes(stored.Length);
                            int diff = stored.Length ^ computed.Length;
                            for (int i = 0; i < stored.Length && i < computed.Length; i++) diff |= stored[i] ^ computed[i];
                            return diff == 0;
                        }
                    }
                    catch { return false; }
                }
            }
            return password == storedHash;
        }
    }
}
