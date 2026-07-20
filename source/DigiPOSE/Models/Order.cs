using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Order
    {
        [Key] public int OrderId { get; set; }
        [Display(Name = "Branch")] public int BranchId { get; set; }
        public int ShiftId { get; set; }
        public int UserId { get; set; }
        public int? CustomerId { get; set; }

        [StringLength(100)] public string? SnapshotCustomerName { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)] public string? SnapshotCustomerPhone { get; set; }
        public int StatusId { get; set; }
        public int? PaymentMethodId { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal GrossAmount { get; set; } 
        [Column(TypeName = "decimal(18,2)")] public decimal DiscountAmount { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")] public decimal TaxAmount { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }

        [StringLength(255)] public string? DiscountReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Shift? Shift { get; set; }
        public User? User { get; set; }
        public Customer? Customer { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }

        public Invoice? invoice { get; set; }
    }
}