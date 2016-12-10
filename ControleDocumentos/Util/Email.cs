using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace ControleDocumentos.Util
{
    public class Email
    {
        public static void EnviarEmail(string from, string[] to, string subject, string emailHtmlBody)
        {
            var email = new MailMessage()
            {
                Body = emailHtmlBody,
                IsBodyHtml = true,
                Subject = subject,
                From = new MailAddress(from)
            };
            
            foreach (var item in to)
            {
                email.To.Add(new MailAddress(item));
            }

            var client = new SmtpClient(ConfigurationManager.AppSettings["SmtpServer"], 587);
            client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MailUserName"], ConfigurationManager.AppSettings["MailUserPassword"]);
            client.EnableSsl = true;
            client.Send(email);
        }
    }
}