using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class StockVoucher
    {
        [Key] public int VoucherId { get; set; }
        [Display(Name = "Branch")] public int BranchId { get; set; }
        [Display(Name = "Employee")] public int UserId { get; set; }
        [Display(Name = "Supplier")] public int? SupplierId { get; set; }
        [Required, StringLength(50)][Display(Name = "Voucher Type")] public string VoucherType { get; set; } = null!;
        [Column(TypeName = "decimal(18,4)")][Display(Name = "Total Value")] public decimal TotalValue { get; set; }
        [Display(Name = "Created At")] public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Branch? Branch { get; set; }
        public User? User { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<StockVoucherDetail>? StockVoucherDetails { get; set; }
    }
}
