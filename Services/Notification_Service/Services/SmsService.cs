using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Notification_Service.DTOs;
using Notification_Service.Interfaces;

namespace Notification_Service.Services;

/*
 * FREE SMS via Twilio Trial Account
 * -----------------------------------
 * 1. Sign up at https://www.twilio.com (no credit card for trial)
 * 2. Get a free Twilio phone number from the console.
 * 3. Trial limit: can only send to verified numbers (add yours in console).
 * 4. Free credit: ~$15 USD (~hundreds of SMS messages).
 * 5. Set in appsettings / docker env:
 *      Twilio__AccountSid  = ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
 *      Twilio__AuthToken   = your-auth-token
 *      Twilio__FromNumber  = +1415XXXXXXX   (your Twilio number)
 *
 * To go live: just upgrade account - same code, no changes needed.
 */
public class SmsService : ISmsService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IConfiguration config, ILogger<SmsService> logger)
    {
        _config = config;
        _logger = logger;

        var sid   = _config["Twilio:AccountSid"];
        var token = _config["Twilio:AuthToken"];

        if (!string.IsNullOrEmpty(sid) && !string.IsNullOrEmpty(token))
            TwilioClient.Init(sid, token);
    }

    public async Task SendAsync(SendSmsDTO dto)
    {
        var from = _config["Twilio:FromNumber"];

        if (string.IsNullOrEmpty(from))
        {
            _logger.LogWarning("Twilio not configured. Skipping SMS to {Phone}", dto.ToPhone);
            return;
        }

        try
        {
            var msg = await MessageResource.CreateAsync(
                body: dto.Message,
                from: new PhoneNumber(from),
                to:   new PhoneNumber(dto.ToPhone));

            _logger.LogInformation("SMS sent to {Phone}, SID={Sid}", dto.ToPhone, msg.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", dto.ToPhone);
        }
    }
}