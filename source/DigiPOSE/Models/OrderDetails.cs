using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class OrderDetail
    {
        [Key] public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int NatureId { get; set; }
        public int TaxTypeId { get; set; }

        public int Quantity { get; set; }

        [Required, StringLength(100)] public string ProductName { get; set; } = null!;
        [Required, StringLength(50)] public string UnitName { get; set; } = null!;

        [Column(TypeName = "decimal(18,4)")] public decimal UnitPrice { get; set; } 
        [Column(TypeName = "decimal(18,4)")] public decimal DiscountRate { get; set; } = 0;
        [Column(TypeName = "decimal(18,4)")] public decimal DiscountAmount { get; set; } = 0;
        [Column(TypeName = "decimal(18,4)")] public decimal TaxRate { get; set; } = 0;
        [Column(TypeName = "decimal(18,4)")] public decimal TaxAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; } = 0; // = (Qty*UnitPrice) - Discount + Tax

        [StringLength(500)] public string? Notes { get; set; }
        public bool IsFree { get; set; } = false; 

        public Order? Order { get; set; }
        public Product? Product { get; set; }
        public ItemNature? ItemNature { get; set; }
        public TaxType? TaxType { get; set; }
    }
}