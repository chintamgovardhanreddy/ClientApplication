using MailKit.Net.Smtp;
using MimeKit;
//using System.Net.Mail;

namespace ClientApplication.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                Console.WriteLine("Entered SendEmailAsync()");

                var email = new MimeMessage();

                email.From.Add(new MailboxAddress("Client Application", "govardhanreddychintam@gmail.com"));
                email.To.Add(MailboxAddress.Parse(toEmail));

                email.Subject = subject;

                email.Body = new TextPart("plain")
                {
                    Text = body
                };

                using var smtp = new SmtpClient();

                Console.WriteLine("Connecting SMTP...");

                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                Console.WriteLine("Authenticating...");

                // Use your Gmail App Password here
                await smtp.AuthenticateAsync("govardhanreddychintam@gmail.com", "iuovfzuydapcoubz");

                Console.WriteLine("Sending Email...");

                await smtp.SendAsync(email);

                Console.WriteLine("Email sent successfully.");

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Error : " + ex.Message);
                throw;
            }
        }
    }
}