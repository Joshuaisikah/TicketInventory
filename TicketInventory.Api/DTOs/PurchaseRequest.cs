using System.ComponentModel.DataAnnotations;

namespace TicketInventory.Api.DTOs;

public class PurchaseRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be atleast 1.")]
    public int Quantity { get; set; }
}
