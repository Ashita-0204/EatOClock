namespace Cart_Service.DTOs;

public class ApplyPromoRequest
{
    public string PromoCode { get; set; } = string.Empty;
public ApplyPromoRequest(string promoCode)
    {
        PromoCode = promoCode;
    }
}
