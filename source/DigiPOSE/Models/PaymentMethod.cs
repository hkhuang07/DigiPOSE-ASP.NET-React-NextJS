using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DigiPOSE.Models
{
    public class PaymentMethod
    {
        [Key] public int PaymentMethodId { get; set; }
        [Required, StringLength(100)] public string MethodName { get; set; } = null!;
        [StringLength(255)] public string? Description { get; set; }

        public ICollection<Order>? Orders { get; set; }
    }
}
