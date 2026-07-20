using System;
using System.Collections.Generic;
using System.Web;
using System.Net.Mail;
using System.Configuration;

namespace AQOONHUB_SMS.App_Code.Utilities
{
    /// <summary>
    /// Provides email sending functionality using SMTP for the AQOONHUB School Management System.
    /// </summary>
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromEmail;
        private readonly string _fromDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// Reads SMTP configuration from web.config appSettings.
        /// </summary>
        public EmailService()
        {
            _smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "localhost";

            int port;
            if (!int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out port))
            {
                port = 25;
            }
            _smtpPort = port;

            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"] ?? string.Empty;
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? string.Empty;

            bool enableSsl;
            if (!bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out enableSsl))
            {
                enableSsl = false;
            }
            _enableSsl = enableSsl;

            _fromEmail = ConfigurationManager.AppSettings["SmtpFromEmail"] ?? "noreply@aqoonhub.com";
            _fromDisplayName = ConfigurationManager.AppSettings["SmtpFromDisplayName"] ?? "AQOONHUB SMS";
        }

        /// <summary>
        /// Sends a plain text email to the specified recipient.
        /// </summary>
        /// <param name="to">The recipient email address.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="body">The plain text body of the email.</param>
        /// <returns>true if the email was sent successfully; otherwise, false.</returns>
        public bool SendEmail(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                return false;
            }

            try
            {
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_fromEmail, _fromDisplayName);
                    mailMessage.To.Add(to);
                    mailMessage.Subject = subject ?? string.Empty;
                    mailMessage.Body = body ?? string.Empty;
                    mailMessage.IsBodyHtml = false;

                    using (SmtpClient smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtpClient.EnableSsl = _enableSsl;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                        if (!string.IsNullOrEmpty(_smtpUsername))
                        {
                            smtpClient.Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword);
                        }

                        smtpClient.Send(mailMessage);
                    }
                }

                return true;
            }
            catch (SmtpException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends an HTML-formatted email to the specified recipient.
        /// </summary>
        /// <param name="to">The recipient email address.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="htmlBody">The HTML body of the email.</param>
        /// <returns>true if the email was sent successfully; otherwise, false.</returns>
        public bool SendHtmlEmail(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                return false;
            }

            try
            {
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_fromEmail, _fromDisplayName);
                    mailMessage.To.Add(to);
                    mailMessage.Subject = subject ?? string.Empty;
                    mailMessage.Body = htmlBody ?? string.Empty;
                    mailMessage.IsBodyHtml = true;

                    using (SmtpClient smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        smtpClient.EnableSsl = _enableSsl;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                        if (!string.IsNullOrEmpty(_smtpUsername))
                        {
                            smtpClient.Credentials = new System.Net.NetworkCredential(_smtpUsername, _smtpPassword);
                        }

                        smtpClient.Send(mailMessage);
                    }
                }

                return true;
            }
            catch (SmtpException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}