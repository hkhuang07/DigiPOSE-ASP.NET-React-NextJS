using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Order
    {
        [Key][Display(Name = "Order ID")] public int OrderId { get; set; }
        [Display(Name = "Branch")] public int BranchId { get; set; }
        [Display(Name = "Shift")] public int ShiftId { get; set; }
        [Display(Name = "Employee")] public int UserId { get; set; }
        [Display(Name = "Customer")] public int? CustomerId { get; set; }

        [StringLength(100)] public string? SnapshotCustomerName { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)] public string? SnapshotCustomerPhone { get; set; }
        [Display(Name = "Status")] public int StatusId { get; set; }
        [Display(Name = "Payment Method")] public int? PaymentMethodId { get; set; }

        [Column(TypeName = "decimal(18,4)")][Display(Name = "Gross Amount")] public decimal GrossAmount { get; set; } 
        [Column(TypeName = "decimal(18,4)")][Display(Name = "Discount")] public decimal DiscountAmount { get; set; } = 0;
        [Column(TypeName = "decimal(18,4)")][Display(Name = "Tax")] public decimal TaxAmount { get; set; } = 0;
        [Column(TypeName = "decimal(18,4)")][Display(Name = "Total Amount")] public decimal TotalAmount { get; set; }

        [StringLength(255)] public string? DiscountReason { get; set; }
        [Display(Name = "Created Date")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Shift? Shift { get; set; }
        public User? User { get; set; }
        public Customer? Customer { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }

        public Invoice? invoice { get; set; }
    }
}