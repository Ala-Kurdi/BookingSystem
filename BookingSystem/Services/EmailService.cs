using System;
using System.Net.Mail;

namespace BookingSystem.Services
{
    public class EmailService
    {
        public void SendNotification(string recipientEmail, string subject, string message)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("ZemheriSevdam@gmail.com", "dhvr utdr eytp aima"),
                    EnableSsl = true
                };

                mail.From = new MailAddress("ZemheriSevdam@gmail.com", "Booking System");
                mail.ReplyToList.Add(new MailAddress("noreply@booking.com"));
                mail.To.Add(recipientEmail);
                mail.Subject = subject;
                mail.Body = message;

                smtpClient.Send(mail);
                Console.WriteLine("Notifikation sendt succesfuldt!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fejl ved afsendelse af notifikation: {ex.Message}");
            }
        }
    }
}
