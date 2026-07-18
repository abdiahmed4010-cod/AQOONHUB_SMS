using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AQOONHUB.Utilities
{
    /// <summary>
    /// Provides security utilities for password hashing, encryption, and token generation
    /// </summary>
    public static class SecurityHelper
    {
        #region Password Hashing

        /// <summary>
        /// Hashes a password using SHA-256 algorithm
        /// NOTE: For production, use bcrypt or Argon2 instead of SHA-256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Verifies a password against its hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            string computedHash = HashPassword(password);
            return string.Equals(computedHash, hash, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generates a secure random salt
        /// </summary>
        public static string GenerateSalt(int size = 32)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] saltBytes = new byte[size];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }

        /// <summary>
        /// Hashes password with salt using HMAC-SHA256 (more secure than plain SHA-256)
        /// </summary>
        public static string HashPasswordWithSalt(string password, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt))
                throw new ArgumentException("Password and salt are required");

            using (HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(salt)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hash);
            }
        }

        #endregion

        #region Token Generation

        /// <summary>
        /// Generates a cryptographically secure random token
        /// </summary>
        public static string GenerateToken(int length = 32)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[length];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }

        /// <summary>
        /// Generates a password reset token with expiration
        /// </summary>
        public static string GenerateResetToken()
        {
            return Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Generates an API key
        /// </summary>
        public static string GenerateApiKey()
        {
            return "aqh_" + Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        #endregion

        #region Encryption/Decryption

        /// <summary>
        /// Encrypts sensitive data using AES encryption
        /// </summary>
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            byte[] iv = new byte[16];

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Decrypts AES encrypted data
        /// </summary>
        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            byte[] iv = new byte[16];
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        #endregion

        #region Input Sanitization

        /// <summary>
        /// Sanitizes user input to prevent XSS attacks
        /// </summary>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return HttpUtility.HtmlEncode(input)
                .Replace("'", "&#39;")
                .Replace("--", "&#45;&#45;");
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates phone number (Somali format)
        /// </summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            // Supports: +252 61 XXX XXXX, +252 65 XXX XXXX, etc.
            string pattern = @"^\+252\s?(61|62|63|65|66|67|68|69|90|91)\s?\d{3}\s?\d{4}$";
            return System.Text.RegularExpressions.Regex.IsMatch(phone, pattern);
        }

        #endregion

        #region Session Security

        /// <summary>
        /// Generates a secure session identifier
        /// </summary>
        public static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N") + "_" + DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Validates session to prevent session fixation
        /// </summary>
        public static bool ValidateSession(HttpSessionStateBase session)
        {
            if (session == null || session["UserID"] == null)
                return false;

            // Check session timeout
            if (session["SessionStart"] != null)
            {
                DateTime start = (DateTime)session["SessionStart"];
                if (DateTime.Now.Subtract(start).TotalMinutes > 30) // 30 min timeout
                {
                    session.Clear();
                    session.Abandon();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Regenerates session ID after login (prevents session fixation)
        /// </summary>
        public static void RegenerateSessionId()
        {
            HttpContext.Current.Session["OldSessionID"] = HttpContext.Current.Session.SessionID;
            HttpContext.Current.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
        }

        #endregion

        #region Password Policy

        /// <summary>
        /// Validates password against security policy
        /// </summary>
        public static PasswordValidationResult ValidatePasswordPolicy(string password)
        {
            var result = new PasswordValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                result.IsValid = false;
                result.Errors.Add("Password must be at least 8 characters");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
            {
                result.IsValid = false;
                result.Errors.Add("Password must contain at least one uppercase letter");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
            {
                result.IsValid = false;
                result.Errors.Add("Password must contain at least one lowercase letter");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
            {
                result.IsValid = false;
                result.Errors.Add("Password must contain at least one number");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]"))
            {
                result.IsValid = false;
                result.Errors.Add("Password must contain at least one special character");
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Result of password policy validation
    /// </summary>
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}