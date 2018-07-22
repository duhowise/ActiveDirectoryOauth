using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Smsgh.Api.Sdk.Smsgh;

namespace ActiveDirectoryAuth.Services
{
    public class SmsService:IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            await SendSms(message.Destination, message.Body);
        }

        public static Task SendSms(string phoneNumber, string message)
        {
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            var host = new ApiHost(new BasicAuth(clientId, clientSecret));
            new MessagingApi(host).SendQuickMessage(ConfigurationManager.AppSettings["SenderName"], phoneNumber, message, true);
            return Task.FromResult(0);

        }

    }
}