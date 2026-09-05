using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ClientApplication.Services
{
    public class SmsService
    {
        private const string AccountSid = "ACea986071895f77cde730ab3dc22c39f";
        private const string AuthToken = "7be60f5a66464ff92772f85fe66af912";
        private const string FromNumber = "+18142596913";

        public async Task SendSmsAsync(string to, string message)
        {
            TwilioClient.Init(AccountSid, AuthToken);

            await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(FromNumber),
                to: new PhoneNumber(to));
        }
    }
}