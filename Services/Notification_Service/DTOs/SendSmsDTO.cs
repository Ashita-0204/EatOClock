using Notification_Service.Enums;

namespace Notification_Service.DTOs;
public class SendSmsDTO
{
    public string ToPhone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
