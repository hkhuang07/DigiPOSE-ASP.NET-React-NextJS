using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class OrderStatus
    {
        [Key] public int StatusId { get; set; }
        [Required, StringLength(50)][Display(Name = "Order status name")] public string StatusName { get; set; } = null!;
        [StringLength(50)] public string? BadgeColor { get; set; }
        [StringLength(255)][Display(Name = "Description")] public string? Description { get; set; } = null!;
        public ICollection<Order>? Orders { get; set; }
    }
}
