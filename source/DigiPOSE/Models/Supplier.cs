using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Supplier
    {
        [Key] public int SupplierId { get; set; }
        [Required, StringLength(150)][Display(Name = "Supplier Name")] public string SupplierName { get; set; } = null!;
        [StringLength(20)][Display(Name = "Phone")] public string? Phone { get; set; }
        [StringLength(100)][Display(Name = "Contact Person")] public string? ContactPerson { get; set; }
        [StringLength(100)][Display(Name = "Email")] public string? Email { get; set; }
        [StringLength(255)][Display(Name = "Address")] public string? Address { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Tax Code")] public string? TaxCode { get; set; }
        [StringLength(150)][Display(Name = "Bank Name")] public string? BankName { get; set; }
        [Column(TypeName = "varchar(50)")][StringLength(50)][Display(Name = "Bank Account")] public string? BankAccount { get; set; }
        [StringLength(150)][Display(Name = "Website")] public string? Website { get; set; }
        [StringLength(500)][Display(Name = "Description")] public string? Description { get; set; }
        [Column(TypeName = "decimal(18,4)")][Display(Name = "Debt Balance")] public decimal DebtBalance { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ICollection<StockVoucher>? StockVouchers { get; set; }
    }
}
