using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace ActiveDirectoryAuth.Services
{
    public class EmailService:IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            var client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Email"],ConfigurationManager.AppSettings["Password"])
            };

            var mm =
                new MailMessage(ConfigurationManager.AppSettings["Email"], message.Destination, message.Subject, message.Body)
                {
                    BodyEncoding = Encoding.UTF8,IsBodyHtml = true
                };

            await client.SendMailAsync(mm);
        }
    }
}