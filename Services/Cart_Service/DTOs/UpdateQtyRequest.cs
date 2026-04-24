namespace Cart_Service.DTOs;

public class UpdateQtyRequest
{
    public int Quantity { get; set; }
     public UpdateQtyRequest(int quantity)
    {
        Quantity = quantity;
    }
}