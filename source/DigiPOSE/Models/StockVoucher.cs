using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class StockVoucher
    {
        [Key] public int VoucherId { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public int? SupplierId { get; set; }
        [Required, StringLength(50)] public string VoucherType { get; set; } = null!;
        [Column(TypeName = "decimal(18,2)")] public decimal TotalValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Branch? Branch { get; set; }
        public User? User { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<StockVoucherDetail>? StockVoucherDetails { get; set; }
    }
}
