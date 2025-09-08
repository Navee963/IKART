using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace IKart_ServerSide.Services
{
    public class EmailService
    {
        private readonly string smtpServer = "smtp.gmail.com";
        private readonly int smtpPort = 587;
        private readonly string senderEmail = "tk.navee.tk@gmail.com"; 
        private readonly string senderPassword = "ymxk oorl lvgy hwgq"; 

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false; // 👈 must disable default creds
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
