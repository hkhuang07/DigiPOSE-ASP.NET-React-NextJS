using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Supplier
    {
        [Key] public int SupplierId { get; set; }
        [Required, StringLength(150)][Display(Name = "Supplier Name")] public string SupplierName { get; set; } = null!;
        [StringLength(20)][Display(Name = "Phone")] public string? Phone { get; set; }
        [Column(TypeName = "decimal(18,2)")][Display(Name = "Debt Balance")] public decimal DebtBalance { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ICollection<StockVoucher>? StockVouchers { get; set; }
    }
}
