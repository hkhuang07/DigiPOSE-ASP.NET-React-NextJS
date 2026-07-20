using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Customer
    {
        [Key] public int CustomerId { get; set; }
        [Display(Name = "Type")] public int CustomeTypeId { get; set; }
        [Required, StringLength(100)][Display(Name = "Full Name")] public string FullName { get; set; } = null!;
        [StringLength(200)][Display(Name = "Company Name")] public string? CompanyName { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Tax Code")] public string? TaxCode { get; set; }
        [Column(TypeName = "varchar(50)")][StringLength(50)][Display(Name = "Budget Code")] public string? BudgetCode { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Id No")] public string? IdNo { get; set; }
        [Column(TypeName = "varchar(20)")][StringLength(20)][Display(Name = "Phone Number")] public string? PhoneNumber { get; set; }
        [StringLength(100)][Display(Name = "Email")] public string? Email { get; set; }
        [StringLength(255)][Display(Name = "Address")] public string? Address { get; set; }
        [StringLength(150)][Display(Name = "Bank Name")] public string? BankName { get; set; }
        [Column(TypeName = "varchar(50)")][StringLength(50)][Display(Name = "Bank Account")] public string? BankAccount { get; set; }
        [Display(Name = "Reward Points")] public int RewardPoints { get; set; } = 0;
        [Column(TypeName = "decimal(18,4)")][Display(Name = "Debt Balance")] public decimal DebtBalance { get; set; } = 0;
        [StringLength(500)][Display(Name = "Notes")] public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        public CustomeType? CustomeType { get; set; }
    }
}
